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

			using (HttpRequestMessage searchRequest = new HttpRequestMessage())
			{
				searchRequest.RequestUri = new Uri("http://api.musixmatch.com/ws/1.1/track.search?q_artist=" + artistEncoded + "&q_track=" + titleEncoded + "&page_size=1&apikey=" + (string)account[0]["key"]);

				string searchContent = await this.GetResponse(client, searchRequest, cancelToken);
				JObject searchData = JsonConvert.DeserializeObject<JObject>(searchContent, this.GetJsonSettings());

				if (searchData != null && searchData.SelectToken("message.body.track_list[0].track") != null)
				{
					o.Artist = (string)searchData.SelectToken("message.body.track_list[0].track.artist_name");
					o.Title = (string)searchData.SelectToken("message.body.track_list[0].track.track_name");
					o.Album = (string)searchData.SelectToken("message.body.track_list[0].track.album_name");
					o.Genre = (string)searchData.SelectToken("message.body.track_list[0].track.primary_genres.music_genre_list[0].music_genre.music_genre_name");

					string albumid = (string)searchData.SelectToken("message.body.track_list[0].track.album_id");

					// ###########################################################################
					using (HttpRequestMessage albumRequest = new HttpRequestMessage())
					{
						albumRequest.RequestUri = new Uri("http://api.musixmatch.com/ws/1.1/album.get?album_id=" + albumid + "&apikey=" + (string)account[0]["key"]);

						string albumContent = await this.GetResponse(client, albumRequest, cancelToken);
						JObject albumData = JsonConvert.DeserializeObject<JObject>(albumContent, this.GetJsonSettings());

						if (albumData != null && albumData.SelectToken("message.body.album") != null)
						{
							o.Date = (string)albumData.SelectToken("message.body.album.album_release_date");
							o.TrackCount = (string)albumData.SelectToken("message.body.album.album_track_count");
							o.DiscCount = null;
							o.DiscNumber = null;
							o.Cover = null;     // "album.get" method provides several fields for cover URLs. But they are never used/filled with data
						}
					}

					// ###########################################################################
					using (HttpRequestMessage albumtracksRequest = new HttpRequestMessage())
					{
						albumtracksRequest.RequestUri = new Uri("http://api.musixmatch.com/ws/1.1/album.tracks.get?album_id=" + albumid + "&page_size=100&apikey=" + (string)account[0]["key"]);

						string albumtracksContent = await this.GetResponse(client, albumtracksRequest, cancelToken);
						JObject albumtracksData = JsonConvert.DeserializeObject<JObject>(albumtracksContent, this.GetJsonSettings());

						if (albumtracksData != null && albumtracksData.SelectToken("message.body.track_list") != null)
						{
							JToken[] tracklist = albumtracksData.SelectTokens("message.body.track_list[*].track.track_name").ToArray();
							int temp = Array.FindIndex(tracklist, t => t.ToString().ToLowerInvariant() == o.Title.ToLowerInvariant());
							if (temp != -1)
							{
								o.TrackNumber = (temp + 1).ToString();
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
