//-----------------------------------------------------------------------
// <copyright file="MusixMatch.cs" company="Shiny ID3 Tagger">
// Copyright (c) Shiny ID3 Tagger. All rights reserved.
// </copyright>
// <author>ShinyId3Tagger Team</author>
//-----------------------------------------------------------------------
// TODO: Renew musixmatch licence

namespace GetTags
{
	using System;
	using System.Diagnostics;
	using System.Linq;
	using System.Net;
	using System.Net.Http;
	using System.Threading;
	using System.Threading.Tasks;
	using GlobalVariables;
	using Newtonsoft.Json.Linq;
	using Utils;

	/// <summary>
	/// Class for MusixMatch API
	/// </summary>
	internal class MusixMatch : IGetTagsService
	{
		/// <summary>
		/// Gets ID3 data from MusixMatch API
		/// <seealso href="https://developer.musixmatch.com/documentation/api-reference/matcher-track-get"/>
		/// <seealso href="https://developer.musixmatch.com/documentation/input-parameters"/>
		/// Only 1000 hits per day
		/// </summary>
		/// <param name="client">The HTTP client which is passed on to GetResponse method</param>
		/// <param name="artist">The input artist to search for</param>
		/// <param name="title">The input song title to search for</param>
		/// <param name="cancelToken">The cancelation token which is passed on to GetResponse method</param>
		/// <returns>
		/// The ID3 tag object with the results from this API for:
		/// 		Artist
		/// 		Title
		/// 		Album
		/// 		Date
		/// 		Genre
		/// 		DiscNumber
		/// 		DiscCount
		/// 		TrackNumber
		/// 		TrackCount
		/// 		Cover URL
		/// </returns>
		public async Task<Id3> GetTags(HttpMessageInvoker client, string artist, string title, CancellationToken cancelToken)
		{
			Id3 o = new Id3 { Service = "Musixmatch" };

			Stopwatch sw = new Stopwatch();
			sw.Start();

			// ###########################################################################
			foreach (var item in User.Accounts[o.Service])
			{
				if (item["lastUsed"] == null)
				{
					item["lastUsed"] = 0;
				}
			}

			JToken account = (from item in User.Accounts[o.Service]
						   orderby item["lastUsed"] ascending
						   select item).FirstOrDefault();
			account["lastUsed"] = DateTime.Now.Ticks;

			string artistEncoded = WebUtility.UrlEncode(artist);
			string titleEncoded = WebUtility.UrlEncode(title);

			using (HttpRequestMessage searchRequest = new HttpRequestMessage())
			{
				searchRequest.RequestUri = new Uri("http://api.musixmatch.com/ws/1.1/matcher.track.get?q_artist=" + artistEncoded + "&q_track=" + titleEncoded + "&apikey=" + (string)account["ApiKey"]);

				string searchContent = await Utils.GetHttpResponse(client, searchRequest, cancelToken);
				JObject searchData = Utils.DeserializeJson(searchContent);

				if (searchData?.SelectToken("message.body.track") != null)
				{
					o.Artist = (string)searchData.SelectToken("message.body.track.artist_name");
					o.Title = (string)searchData.SelectToken("message.body.track.track_name");
					o.Album = (string)searchData.SelectToken("message.body.track.album_name");
					o.Genre = (string)searchData.SelectToken("message.body.track.primary_genres.music_genre_list[0].music_genre.music_genre_name");

					string albumid = (string)searchData.SelectToken("message.body.track.album_id");

					// ###########################################################################
					using (HttpRequestMessage albumRequest = new HttpRequestMessage())
					{
						albumRequest.RequestUri = new Uri("http://api.musixmatch.com/ws/1.1/album.get?album_id=" + albumid + "&apikey=" + (string)account["ApiKey"]);

						string albumContent = await Utils.GetHttpResponse(client, albumRequest, cancelToken);
						JObject albumData = Utils.DeserializeJson(albumContent);

						if (albumData?.SelectToken("message.body.album") != null)
						{
							o.Date = (string)albumData.SelectToken("message.body.album.album_release_date");
							o.TrackCount = (string)albumData.SelectToken("message.body.album.album_track_count");
							o.DiscCount = null;
							o.DiscNumber = null;
							o.Cover = null;		// "album.get" method provides several fields for cover URLs. But they are never used/filled with data
						}
					}

					// ###########################################################################
					using (HttpRequestMessage albumtracksRequest = new HttpRequestMessage())
					{
						albumtracksRequest.RequestUri = new Uri("http://api.musixmatch.com/ws/1.1/album.tracks.get?album_id=" + albumid + "&page_size=100&apikey=" + (string)account["ApiKey"]);

						string albumtracksContent = await Utils.GetHttpResponse(client, albumtracksRequest, cancelToken);
						JObject albumtracksData = Utils.DeserializeJson(albumtracksContent);

						if (albumtracksData?.SelectToken("message.body.track_list") != null)
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
