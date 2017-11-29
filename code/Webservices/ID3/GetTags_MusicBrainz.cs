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
			// const string Server = "https://musicbrainz.org";
			// const string Server = "http://musicbrainz-mirror.eu:5000";
			// const string Server = "http://musicbrainz.fin-alice.de:5000";
			const string Server = "http://beta.musicbrainz.org/";							// info: http://www.tranquilbase.org/category/musicbrainz/
			
			const string InvalidChars = @"[#!(){}/;:\[\]\^\\\\&""]";
			
			string artistTemp = Regex.Replace(artist, InvalidChars, string.Empty);
			string titleTemp = Regex.Replace(title, InvalidChars, string.Empty);
			
			HttpRequestMessage request = new HttpRequestMessage();
			request.Headers.Add("User-Agent", User.Settings["UserAgent"]);
			request.RequestUri = new Uri(Server + "/ws/2/recording?" + Uri.EscapeUriString("query=artist:(" + artistTemp + ") AND recording:(" + titleTemp  + ") AND status:official AND type:album&limit=10&fmt=json"));

			string content1 = await this.GetRequest(client, request, cancelToken);
			JObject data1 = JsonConvert.DeserializeObject<JObject>(content1, this.GetJsonSettings());

			if (data1 != null && (string)data1.SelectToken("count") != "0")
			{
				// Currently line 51 till 94 is only for finding the best album release
				// Take only those recording which have a rating above 90
				// Aggregate all releases for those recordings (a single recording has multiple releases i.e. one for each country)
				// Loop through these releases and sort out compilations, take only official albums
				// Loop again and take the first (=oldest) release. This is hopefully the the original album for our current track
				List<JToken> rec90 = (from recording in data1.SelectToken("recordings")
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
						curStatus == "Official" &&
						curPrimType == "Album")
					{
						if (curRelease.SelectToken("release-group.secondary-types") != null)
						{
							if ((string)curRelease.SelectToken("release-group.secondary-types[0]") == "Compilation")
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
					if (curDate != null && firstDate != null &&	this.ConvertStringToDate(curDate).Ticks < this.ConvertStringToDate(firstDate).Ticks)
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

				request = new HttpRequestMessage();	
				request.Headers.Add("User-Agent", User.Settings["UserAgent"]);
				request.RequestUri = new Uri(Server + "/ws/2/release-group/" + releasegroupid + "?inc=tags+ratings+artists&fmt=json");

				string content2 = await this.GetRequest(client, request, cancelToken);
				JObject data2 = JsonConvert.DeserializeObject<JObject>(content2, this.GetJsonSettings());

				if (data2 != null)
				{
					o.Artist = (string)data2.SelectToken("artist-credit[0].name");
					o.Date = (string)data2.SelectToken("first-release-date");

					if (data2.SelectToken("tags").Any())
					{
						IEnumerable<JToken> objGenres =
							from tag in data2.SelectToken("tags")
							orderby (int)tag["count"] descending
							select tag;

						JToken objGenre = objGenres.ToArray()[0];
						o.Genre = (string)objGenre["name"];				// Not many results for genres because musicbrainz does not support them (https://wiki.musicbrainz.org/Genre)
					}
				}

				// ###########################################################################
				
				// TODO coverartarchive.org is really slow. It takes between 2-3 seconds for a response
				request = new HttpRequestMessage();
				request.Headers.Add("User-Agent", User.Settings["UserAgent"]);
				request.Headers.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
				request.RequestUri = new Uri("http://coverartarchive.org/release-group/" + releasegroupid);
				
				string content3 = await this.GetRequest(client, request, cancelToken);
				JObject data3 = JsonConvert.DeserializeObject<JObject>(content3, this.GetJsonSettings());
				
				if (data3 != null)
				{
					o.Cover = (string)data3.SelectToken("images[0].image");
				}
			}

			// ###########################################################################
			sw.Stop();
			o.Duration = string.Format("{0:s\\,f}", sw.Elapsed);

			request.Dispose();
			return o;
		}
	}
}

// System.IO.File.WriteAllText (@"D:\response.json", content2);