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
	using System.Collections.Generic;
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

			string artistEnc = WebUtility.UrlEncode(artist);
			string titleEnc = WebUtility.UrlEncode(title);
			
			HttpRequestMessage request = new HttpRequestMessage();
			request.RequestUri = new Uri("http://api.musicgraph.com/api/v2/track/search?api_key=" + (string)account[0]["key"] + "&artist_name=" + artistEnc + "&title=" + titleEnc + "&limit=5");

			string content1 = await this.GetRequest(client, request, cancelToken);
			JObject data1 = JsonConvert.DeserializeObject<JObject>(content1, this.GetJsonSettings());

			if (data1 != null && data1.SelectToken("data") != null && data1.SelectToken("data").Any())
			{
				IEnumerable<JToken> tracksSorted =
					from t in data1.SelectToken("data")
					orderby t["release_year"], t["popularity"] descending
					select t;

				JToken track = tracksSorted.ToArray()[0];

				o.Artist = (string)track.SelectToken("artist_name");
				o.Title = (string)track.SelectToken("title");
				o.Album = (string)track.SelectToken("album_title");
				o.Date = (string)track.SelectToken("original_release_year");
				o.Genre = (string)track.SelectToken("main_genre");
				o.TrackNumber = (string)track.SelectToken("track_index");

				// ###########################################################################
				// musicgraph has a database flaw where many album IDs in track responses point to non-existing albums in their database resulting in 404 "Not found" errors
				string albumid = (string)track.SelectToken("track_album_id");

				request = new HttpRequestMessage();
				request.RequestUri = new Uri("http://api.musicgraph.com/api/v2/album/" + albumid + "?api_key=" + (string)account[0]["key"]);

				string content2 = await this.GetRequest(client, request, cancelToken);
				JObject data2 = JsonConvert.DeserializeObject<JObject>(content2, this.GetJsonSettings());

				if (data2 != null && data2.SelectToken("data") != null)
				{
					o.TrackCount = (string)data2.SelectToken("data.number_of_tracks");
					o.DiscCount = null;
					o.DiscNumber = null;
					o.Cover = null;
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