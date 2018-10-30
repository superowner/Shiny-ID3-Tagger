//-----------------------------------------------------------------------
// <copyright file="Genius.cs" company="Shiny ID3 Tagger">
// Copyright (c) Shiny ID3 Tagger. All rights reserved.
// </copyright>
// <author>ShinyId3Tagger Team</author>
//-----------------------------------------------------------------------

namespace GetTags
{
	using System;
	using System.Diagnostics;
	using System.Net;
	using System.Net.Http;
	using System.Threading;
	using System.Threading.Tasks;
	using GlobalVariables;
	using Newtonsoft.Json.Linq;
	using Utils;

	/// <summary>
	/// Class for Genius API
	/// </summary>
	internal class Genius : IGetTagsService
	{
		/// <summary>
		/// Gets ID3 data from Genius API
		/// https://genius.com/api-clients
		/// https://docs.genius.com/#search-h2
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
			Id3 o = new Id3 { Service = "Genius" };

			Stopwatch sw = new Stopwatch();
			sw.Start();

			// ###########################################################################
			string searchTermEnc = WebUtility.UrlEncode(artist + " - " + title);

			// Does this access_token expire ever? Docs don't mention a expire time when requesting an access_token
			using (HttpRequestMessage searchRequest = new HttpRequestMessage())
			{
				searchRequest.RequestUri = new Uri("https://api.genius.com/search?q=" +
					searchTermEnc +
					"&access_token=" + User.Accounts["Genius"]["AccessToken"] +
					"&text_format=plain");

				string searchContent = await Utils.GetResponse(client, searchRequest, cancelToken);
				JObject searchData = Utils.DeserializeJson(searchContent);

				if (searchData?.SelectToken("response.hits[0].result.api_path") != null)
				{
					using (HttpRequestMessage trackRequest = new HttpRequestMessage())
					{
						trackRequest.RequestUri = new Uri("https://api.genius.com" +
							searchData.SelectToken("response.hits[0].result.api_path") +
							"?access_token=" + User.Accounts["Genius"]["AccessToken"] +
							"&text_format=plain");

						string trackContent = await Utils.GetResponse(client, trackRequest, cancelToken);
						JObject trackData = Utils.DeserializeJson(trackContent);

						if (trackData != null)
						{
							o.Artist = (string)trackData.SelectToken("response.song.primary_artist.name");
							o.Title = (string)trackData.SelectToken("response.song.title");
							o.Album = (string)trackData.SelectToken("response.song.album.name");
							o.Genre = null;				// They don't offer this tag via API (https://genius.com/discussions/279491-Are-genius-song-tags-rap-rock-etc-exposed-through-the-songs-api)
							o.DiscNumber = null;		// Not provided in API response
							o.TrackNumber = null;		// Not provided in API response
							o.Cover = (string)trackData.SelectToken("response.song.album.cover_art_url");

							string albumPath = (string)trackData.SelectToken("response.song.album.api_path");
							if (albumPath != null)
							{
								using (HttpRequestMessage albumRequest = new HttpRequestMessage())
								{
									albumRequest.RequestUri = new Uri("https://api.genius.com" + albumPath + "?access_token=" + User.Accounts["Genius"]["AccessToken"] + "&text_format=plain");

									string albumContent = await Utils.GetResponse(client, albumRequest, cancelToken);
									JObject albumData = Utils.DeserializeJson(albumContent);

									if (albumData != null)
									{
										o.DiscCount = null;			// Not provided in API response
										o.TrackCount = null;		// Not provided in API response
										o.Date = (string)albumData.SelectToken("response.album.release_date");
									}
								}
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
