//-----------------------------------------------------------------------
// <copyright file="MusicGraph.cs" company="Shiny ID3 Tagger">
// Copyright (c) Shiny ID3 Tagger. All rights reserved.
// </copyright>
// <author>ShinyId3Tagger Team</author>
//-----------------------------------------------------------------------

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
	/// Class for MusicGraph API
	/// </summary>
	[Obsolete("Website and API service is closed", true)]
	internal class MusicGraph : IGetTagsService
	{
		/// <summary>
		/// Gets ID3 tags from MusicGraph API
		/// https://developer.musicgraph.com/api-docs/v2/tracks
		/// "limit=1" should not be used, filtering on client side is a better
		/// </summary>
		/// <param name="client">The HTTP client which is passed on to GetResponse method</param>
		/// <param name="artist">The input artist to search for</param>
		/// <param name="title">TThe input song title to search for</param>
		/// <param name="cancelToken">The cancelation token which is passed on to GetResponse method</param>
		/// <returns>A <see cref="Task"/> representing the asynchronous operation.</returns>
		public async Task<Id3> GetTags(HttpMessageInvoker client, string artist, string title, CancellationToken cancelToken)
		{
			Id3 o = new Id3 { Service = "Musicgraph" };

			Stopwatch sw = new Stopwatch();
			sw.Start();

			// ###########################################################################
			// TODO: Undone
			var accounts = User.Accounts.GetValue(o.Service, StringComparison.OrdinalIgnoreCase);
			foreach (var acc in accounts)
			{
				acc["lastUsed"] = 0;
			}

			var account = (from item in User.Accounts[o.Service]
						   orderby item["lastused"] ascending
						   select item).FirstOrDefault();
			account["lastUsed"] = DateTime.Now.Ticks;

			string artistEncoded = WebUtility.UrlEncode(artist);
			string titleEncoded = WebUtility.UrlEncode(title);

			using (HttpRequestMessage searchRequest = new HttpRequestMessage())
			{
				searchRequest.RequestUri = new Uri("http://api.musicgraph.com/api/v2/track/search?api_key=" + (string)account["AppKey"] + "&artist_name=" + artistEncoded + "&title=" + titleEncoded + "&limit=5");

				string searchContent = await Utils.GetResponse(client, searchRequest, cancelToken);
				JObject searchData = Utils.DeserializeJson(searchContent);

				if (searchData?.SelectToken("data") != null && searchData.SelectToken("data").Any())
				{
					JToken track = (from item in searchData.SelectToken("data")
									orderby Utils.ParseInt((string)item["release_year"]), Utils.ParseInt((string)item["popularity"]) descending
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
						albumRequest.RequestUri = new Uri("http://api.musicgraph.com/api/v2/album/" + (string)track.SelectToken("track_album_id") + "?api_key=" + (string)account["AppKey"]);

						string albumContent = await Utils.GetResponse(client, albumRequest, cancelToken, suppressedStatusCodes: new[] { 404 });
						JObject albumData = Utils.DeserializeJson(albumContent);

						if (albumData?.SelectToken("data") != null)
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
			o.Duration = $"{sw.Elapsed:s\\,f}";

			return o;
		}
	}
}
