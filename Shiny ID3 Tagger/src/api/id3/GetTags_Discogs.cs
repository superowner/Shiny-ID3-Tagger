//-----------------------------------------------------------------------
// <copyright file="GetTags_Discogs.cs" company="Shiny ID3 Tagger">
// Copyright (c) Shiny ID3 Tagger. All rights reserved.
// </copyright>
// <author>ShinyId3Tagger Team</author>
// <summary>Gets ID3 data from Discogs API for current track</summary>
// https://www.discogs.com/developers/#page:database
// http://www.onemusicapi.com/blog/2013/06/12/better-discogs-searching/
//-----------------------------------------------------------------------

namespace GlobalNamespace
{
	using System;
	using System.Diagnostics;
	using System.Linq;
	using System.Net;
	using System.Net.Http;
	using System.Threading;
	using System.Threading.Tasks;
	using Newtonsoft.Json.Linq;

	public partial class Form1
	{
		private async Task<Id3> GetTags_Discogs(HttpMessageInvoker client, string artist, string title, CancellationToken cancelToken)
		{
			Id3 o = new Id3();
			o.Service = "Discogs";

			Stopwatch sw = new Stopwatch();
			sw.Start();

			// ###########################################################################
			string artistTemp = string.Join("* ", artist.Split(' ')) + "*";
			string titleTemp = string.Join("* ", title.Split(' ')) + "*";
			string searchTermEnc = WebUtility.UrlEncode(artistTemp + " " + titleTemp);

			using (HttpRequestMessage searchRequest = new HttpRequestMessage())
			{
				// Following syntax has worse results: "https://api.discogs.com/database/search?track=" + titleTemp + "&artist=" + artistTemp
				searchRequest.Headers.Add("User-Agent", (string)User.Settings["UserAgent"]);
				searchRequest.RequestUri = new Uri("https://api.discogs.com/database/search?q=" + searchTermEnc +
					"&format=album" +
					"&type=master" +
					"&per_page=1" +
					"&key=" + User.Accounts["Discogs"]["Key"] +
					"&secret=" + User.Accounts["Discogs"]["Secret"]);

				string searchContent = await this.GetResponse(client, searchRequest, cancelToken);
				JObject searchData = this.DeserializeJson(searchContent);

				if (searchData != null && searchData.SelectToken("results").Any())
				{
					string albumUrl = (string)searchData.SelectToken("results[0].resource_url");
					if (albumUrl != null && IsValidUrl(albumUrl))
					{
						using (HttpRequestMessage albumRequest = new HttpRequestMessage())
						{
							albumRequest.Headers.Add("User-Agent", (string)User.Settings["UserAgent"]);
							albumRequest.RequestUri = new Uri(albumUrl +
								"?key=" + User.Accounts["Discogs"]["Key"] +
								"&secret=" + User.Accounts["Discogs"]["Secret"]);

							string albumContent = await this.GetResponse(client, albumRequest, cancelToken);
							JObject albumData = this.DeserializeJson(albumContent);

							if (albumData != null)
							{
								o.Artist = (string)albumData.SelectToken("artists[0].name");
								o.Title = null;
								o.Album = (string)albumData.SelectToken("title");
								o.Date = (string)albumData.SelectToken("year");
								o.Genre = (string)albumData.SelectToken("genres[0]");
								o.DiscCount = null;
								o.DiscNumber = null;
								o.Cover = (string)albumData.SelectToken("images[0].uri");

								o.TrackCount = albumData.SelectTokens("tracklist[*]")
									.Where(t => t.SelectToken("type_").ToString().ToLowerInvariant() == "track")
									.ToArray().Length.ToString();

								// This API is strange. You can either search for a release and don't get the album. Or you search for albums but don't get releases
								// You can do one search with "format=album" to maybe get the album and a second search without "format=album" to maybe get tracks and therefore a track title
								// But no one will guarantee that the second search shows a title which is on your album from the first search
								// How can I get a response which holds album and title at the same time?
								// Currently it's just checking if album track list contains a title which equals initial search title which can be wrong since it's from filename or old ID3 tags
								JToken[] tracklist = albumData.SelectTokens("tracklist[*].title").ToArray();
								int temp = Array.FindIndex(tracklist, t => t.ToString().ToLowerInvariant() == title.ToLowerInvariant());
								if (temp != -1)
								{
									o.Title = title;
									o.TrackNumber = (temp + 1).ToString();
								}
							}
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
