//-----------------------------------------------------------------------
// <copyright file="GetTags_MusixMatch.cs" company="Shiny Id3 Tagger">
//	 Copyright (c) Shiny Id3 Tagger. All rights reserved.
// </copyright>
// <author>ShinyId3Tagger Team</author>
// <summary>Gets ID3 data from MusixMatch API for current track</summary>
// https://developer.musixmatch.com/documentation/api-reference/track-search
// https://developer.musixmatch.com/documentation/input-parameters
// Only 1000 hits per day. Since 3 calls per file are needed, you can only search for 333 files a day. That's not much
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
		private async Task<Id3> GetTags_MusixMatch(HttpMessageInvoker client, string artist, string title, CancellationToken cancelToken)
		{
			Id3 o = new Id3();
			o.Service = "Musixmatch";
			
			Stopwatch sw = new Stopwatch();
			sw.Start();
			
			// ###########################################################################
			DataRow[] account = User.MmAccounts.Select("lastused = MIN(lastused)");
			User.MmAccounts.Select("lastused = MIN(lastused)")[0]["lastused"] = DateTime.Now.Ticks;			
				
			string artistEncoded = WebUtility.UrlEncode(artist);
			string titleEncoded = WebUtility.UrlEncode(title);
			
			HttpRequestMessage request = new HttpRequestMessage();
			request.RequestUri = new Uri("http://api.musixmatch.com/ws/1.1/track.search?q_artist=" + artistEncoded + "&q_track=" + titleEncoded + "&page_size=1&apikey=" + (string)account[0]["key"]);
			
			string content1 = await this.GetResponse(client, request, cancelToken);
			JObject data1 = JsonConvert.DeserializeObject<JObject>(content1, this.GetJsonSettings());

			if (data1 != null && data1.SelectToken("message.body.track_list[0].track") != null)
			{
				o.Artist = (string)data1.SelectToken("message.body.track_list[0].track.artist_name");
				o.Title = (string)data1.SelectToken("message.body.track_list[0].track.track_name");
				o.Album = (string)data1.SelectToken("message.body.track_list[0].track.album_name");
				o.Genre = (string)data1.SelectToken("message.body.track_list[0].track.primary_genres.music_genre_list[0].music_genre.music_genre_name");
				
				string albumid = (string)data1.SelectToken("message.body.track_list[0].track.album_id");
				
				// ###########################################################################
				request = new HttpRequestMessage();
				request.RequestUri = new Uri("http://api.musixmatch.com/ws/1.1/album.get?album_id=" + albumid + "&apikey=" + (string)account[0]["key"]);

				string content2 = await this.GetResponse(client, request, cancelToken);
				JObject data2 = JsonConvert.DeserializeObject<JObject>(content2, this.GetJsonSettings());

				if (data2 != null && data2.SelectToken("message.body.album") != null)
				{
					o.Date = (string)data2.SelectToken("message.body.album.album_release_date");					
					o.TrackCount = (string)data2.SelectToken("message.body.album.album_track_count");
					o.DiscCount = null;
					o.DiscNumber = null;

					// Musixmatch (when using "album.get" instead of "album.tracks.get" method) provides several fields for cover URLs. But they are never used/filled with data
					o.Cover = null;
				}
				
				// ###########################################################################
				request = new HttpRequestMessage();
				request.RequestUri = new Uri("http://api.musixmatch.com/ws/1.1/album.tracks.get?album_id=" + albumid + "&page_size=100&apikey=" + (string)account[0]["key"]);

				string content3 = await this.GetResponse(client, request, cancelToken);
				JObject data3 = JsonConvert.DeserializeObject<JObject>(content3, this.GetJsonSettings());

				if (data3 != null && data3.SelectToken("message.body.track_list") != null)
				{
					JToken[] tracklist = data3.SelectTokens("message.body.track_list[*].track.track_name").ToArray();
					int temp = Array.FindIndex(tracklist, t => t.ToString().Equals(o.Title, StringComparison.OrdinalIgnoreCase));
					if (temp != -1)
					{
						o.TrackNumber = (temp + 1).ToString();
					}					
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