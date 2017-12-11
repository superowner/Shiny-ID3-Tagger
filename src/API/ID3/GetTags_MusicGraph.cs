//-----------------------------------------------------------------------
// <copyright file="GetTags_MusicGraph.cs" company="Shiny Id3 Tagger">
//	 Copyright (c) Shiny Id3 Tagger. All rights reserved.
// </copyright>
// <author>ShinyId3Tagger Team</author>
// <summary>Gets ID3 data from MusicGraph API for current track</summary>
// https://developer.musicgraph.com/api-docs/v2/tracks
// "&limit=1" should not be used, filtering on client side is better
//-----------------------------------------------------------------------

namespace GlobalNamespace
{
	using System;
	using System.Data;
	using System.Diagnostics;
	using System.Linq;
	using System.Net;
	using System.Net.Http;
	using System.Threading;
	using System.Threading.Tasks;
	using Newtonsoft.Json;
	using Newtonsoft.Json.Linq;

	public partial class Form1
	{
		private async Task<Id3> GetTags_MusicGraph(HttpMessageInvoker client, string artist, string title, CancellationToken cancelToken)
		{
			Id3 o = new Id3();
			o.Service = "Musicgraph";

			Stopwatch sw = new Stopwatch();
			sw.Start();

			// ###########################################################################
			DataRow[] account = User.MgAccounts.Select("lastused = MIN(lastused)");
			User.MgAccounts.Select("lastused = MIN(lastused)")[0]["lastused"] = DateTime.Now.Ticks;

			string artistEncoded = WebUtility.UrlEncode(artist);
			string titleEncoded = WebUtility.UrlEncode(title);

			using (HttpRequestMessage searchRequest = new HttpRequestMessage())
			{
				searchRequest.RequestUri = new Uri("http://api.musicgraph.com/api/v2/track/search?api_key=" + (string)account[0]["key"] + "&artist_name=" + artistEncoded + "&title=" + titleEncoded + "&limit=5");

				string searchContent = await this.GetResponse(client, searchRequest, cancelToken);
				JObject searchData = JsonConvert.DeserializeObject<JObject>(searchContent, this.GetJsonSettings());

				if (searchData != null && searchData.SelectToken("data") != null && searchData.SelectToken("data").Any())
				{
					JToken track = (from item in searchData.SelectToken("data")
									orderby ParseInt((string)item["release_year"]), ParseInt((string)item["popularity"]) descending
									select item)
								.ToArray()[0];

					o.Artist = (string)track.SelectToken("artist_name");
					o.Title = (string)track.SelectToken("title");
					o.Album = (string)track.SelectToken("album_title");
					o.Date = (string)track.SelectToken("original_release_year");
					o.Genre = (string)track.SelectToken("main_genre");
					o.DiscNumber = null;
					o.TrackNumber = (string)track.SelectToken("track_index");

					// ###########################################################################
					using (HttpRequestMessage albumRequest = new HttpRequestMessage())
					{
						// Musicgraph has a database flaw where many album IDs in track responses point to non-existing album URLs resulting in a 404 "Not found" HTTP error
						albumRequest.RequestUri = new Uri("http://api.musicgraph.com/api/v2/album/" + (string)track.SelectToken("track_album_id") + "?api_key=" + (string)account[0]["key"]);

						string albumContent = await this.GetResponse(client, albumRequest, cancelToken);
						JObject albumData = JsonConvert.DeserializeObject<JObject>(albumContent, this.GetJsonSettings());

						if (albumData != null && albumData.SelectToken("data") != null)
						{
							o.TrackCount = (string)albumData.SelectToken("data.number_of_tracks");
							o.DiscCount = null;
							o.Cover = null;

							string albumGenre = (string)albumData.SelectToken("data.main_genre");
							if (string.IsNullOrWhiteSpace(o.Genre) && albumGenre != null)
							{
								o.Genre = albumGenre;
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
