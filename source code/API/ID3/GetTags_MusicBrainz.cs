//-----------------------------------------------------------------------
// <copyright file="GetTags_MusicBrainz.cs" company="Shiny Id3 Tagger">
//	 Copyright (c) Shiny Id3 Tagger. All rights reserved.
// </copyright>
// <author>ShinyId3Tagger Team</author>
// <summary>Gets ID3 data from Musicbrainz API for current track</summary>
// https://wiki.musicbrainz.org/Development/XML_Web_Service/Version_2
// https://musicbrainz.org/doc/Development/XML_Web_Service/Version_2/Search
// https://musicbrainz.org/doc/MusicBrainz_Database
// limit=1 cannot be used, client side filter is used to sort by release date
// List of mirror servers: http://www.tranquilbase.org/category/musicbrainz/
// 1) https://musicbrainz.org		2) http://musicbrainz-mirror.eu:5000	3) http://musicbrainz.fin-alice.de:5000
//-----------------------------------------------------------------------

namespace GlobalNamespace
{
	using System;
	using System.Collections.Generic;
	using System.Diagnostics;
	using System.Linq;
	using System.Net.Http;
	using System.Net.Http.Headers;
	using System.Text.RegularExpressions;
	using System.Threading;
	using System.Threading.Tasks;
	using Newtonsoft.Json;
	using Newtonsoft.Json.Linq;

	public partial class Form1
	{
		private async Task<Id3> GetTags_MusicBrainz(HttpMessageInvoker client, string artist, string title, CancellationToken cancelToken)
		{
			Id3 o = new Id3();
			o.Service = "Musicbrainz";

			Stopwatch sw = new Stopwatch();
			sw.Start();

			// ###########################################################################

			const string InvalidChars = @"[#!(){}/;:\[\]\^\\\\&""]";

			string artistClean = Regex.Replace(artist, InvalidChars, string.Empty);
			string titleClean = Regex.Replace(title, InvalidChars, string.Empty);

			using (HttpRequestMessage searchRequest = new HttpRequestMessage())
			{
				searchRequest.Headers.Add("User-Agent", User.Settings["UserAgent"]);
				searchRequest.RequestUri = new Uri("http://beta.musicbrainz.org/ws/2/recording?" + Uri.EscapeUriString("query=artist:(" + artistClean + ") AND recording:(" + titleClean + ") AND status:official AND type:album&limit=10&fmt=json"));

				string searchContent = await this.GetResponse(client, searchRequest, cancelToken);
				JObject searchData = JsonConvert.DeserializeObject<JObject>(searchContent, this.GetJsonSettings());

				if (searchData != null && (string)searchData.SelectToken("count") != "0")
				{
					// Currently the next lines are only for finding the best album release out of a long list
					//  1) Take only those recordings which have a rating above 90
					//  2) Aggregate all releases for those recordings (a single recording has multiple releases i.e. one for each country)
					//  3) Loop through these releases and sort out compilations, take only official albums
					//  4) Loop again and take oldest release. This is presumably the original album for the searched track
					List<JToken> rec90 = (from recording in searchData.SelectToken("recordings")
										  where (int)recording["score"] >= 90
										  select recording).ToList();

					List<JToken> allReleases = new List<JToken>();
					foreach (JToken recording in rec90)
					{
						List<JToken> curReleases = recording.SelectTokens("releases[*]").ToList();
						allReleases.AddRange(curReleases);
					}

					List<JToken> onlyAlbumReleases = new List<JToken>();
					foreach (JToken curRelease in allReleases)
					{
						string curStatus = (string)curRelease.SelectToken("status");
						string curPrimType = (string)curRelease.SelectToken("release-group.primary-type");

						if (curStatus != null && curPrimType != null &&
							curStatus.ToLowerInvariant() == "official" &&
							curPrimType.ToLowerInvariant() == "album")
						{
							if (curRelease.SelectToken("release-group.secondary-types") != null)
							{
								if (curRelease.SelectToken("release-group.secondary-types[0]").ToString().ToLowerInvariant() == "compilation")
								{
									continue;
								}
							}

							onlyAlbumReleases.Add(curRelease);
						}
					}

					JToken bestRelease = onlyAlbumReleases.Any() ? onlyAlbumReleases[0] : allReleases[0];
					foreach (JToken curRelease in onlyAlbumReleases)
					{
						string curDate = (string)curRelease["date"];
						string firstDate = (string)bestRelease["date"];
						if (curDate != null && firstDate != null && this.ConvertStringToDate(curDate).Ticks < this.ConvertStringToDate(firstDate).Ticks)
						{
							bestRelease = curRelease;
						}
					}

					o.Title = (string)bestRelease["media"][0]["track"][0]["title"];
					o.Album = (string)bestRelease["title"];
					o.DiscCount = null;
					o.DiscNumber = null;
					o.TrackCount = (string)bestRelease["media"][0]["track-count"];
					o.TrackNumber = (string)bestRelease["media"][0]["track"][0]["number"];

					// ###########################################################################
					string releasegroupid = (string)bestRelease["release-group"]["id"];

					using (HttpRequestMessage releasegroupRequest = new HttpRequestMessage())
					{
						releasegroupRequest.Headers.Add("User-Agent", User.Settings["UserAgent"]);
						releasegroupRequest.RequestUri = new Uri("http://beta.musicbrainz.org/ws/2/release-group/" + releasegroupid + "?inc=tags+ratings+artists&fmt=json");

						string releasegroupContent = await this.GetResponse(client, releasegroupRequest, cancelToken);
						JObject releasegroupData = JsonConvert.DeserializeObject<JObject>(releasegroupContent, this.GetJsonSettings());

						if (releasegroupData != null)
						{
							o.Artist = (string)releasegroupData.SelectToken("artist-credit[0].name");
							o.Date = (string)releasegroupData.SelectToken("first-release-date");

							if (releasegroupData.SelectToken("tags").Any())
							{
								JToken objGenre = (from tag in releasegroupData.SelectToken("tags")
													orderby (int)tag["count"] descending
													select tag
												).ToArray()[0];

								o.Genre = (string)objGenre["name"];             // Not many results for genres because musicbrainz does not support them (https://wiki.musicbrainz.org/Genre)
							}
						}
					}

					// ###########################################################################
					using (HttpRequestMessage coverRequest = new HttpRequestMessage())
					{
						coverRequest.Headers.Add("User-Agent", User.Settings["UserAgent"]);
						coverRequest.Headers.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
						coverRequest.RequestUri = new Uri("http://coverartarchive.org/release-group/" + releasegroupid);

						string coverContent = await this.GetResponse(client, coverRequest, cancelToken);
						JObject coverData = JsonConvert.DeserializeObject<JObject>(coverContent, this.GetJsonSettings());

						if (coverData != null)
						{
							o.Cover = (string)coverData.SelectToken("images[0].image");
						}
					}
				}
			}
			// ###########################################################################
			sw.Stop();
			o.Duration = string.Format("{0:s\\,f}", sw.Elapsed);

			return o;
		}
	}
}
