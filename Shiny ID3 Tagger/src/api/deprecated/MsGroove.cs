//-----------------------------------------------------------------------
// <copyright file="MsGroove.cs" company="Shiny ID3 Tagger">
// Copyright (c) Shiny ID3 Tagger. All rights reserved.
// </copyright>
// <author>ShinyId3Tagger Team</author>
//-----------------------------------------------------------------------

namespace GetTags
{
	using System;
	using System.Collections.Generic;
	using System.Diagnostics;
	using System.Net;
	using System.Net.Http;
	using System.Threading;
	using System.Threading.Tasks;
	using GlobalVariables;
	using Newtonsoft.Json.Linq;
	using Utils;

	/// <summary>
	/// Class for Microsoft Groove API
	/// </summary>
	[Obsolete("API shutdown end of 2017 (https://docs.microsoft.com/en-us/groove/api-overview)", true)]
	internal class MsGroove : IGetTagsService
	{
		/// <summary>
		/// Gets ID3 tags from Microsoft Groove API
		/// https://apps.dev.microsoft.com/#/appList
		/// https://developer.microsoft.com/de-de/dashboard/groove
		/// https://docs.microsoft.com/en-us/groove/groove-service-rest-reference/uri-search-content#examples
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
			Id3 o = new Id3 { Service = "Microsoft Groove" };

			Stopwatch sw = new Stopwatch();
			sw.Start();

			// ###########################################################################
			string searchTermEnc = WebUtility.UrlEncode(artist + " - " + title);

			if (ApiSessionData.MsAccessToken == null || ApiSessionData.MsAccessTokenExpireDate < DateTime.Now)
			{
				using (HttpRequestMessage loginRequest = new HttpRequestMessage())
				{
					loginRequest.Method = HttpMethod.Post;
					loginRequest.RequestUri = new Uri("https://login.live.com/accesstoken.srf");
					loginRequest.Content = new FormUrlEncodedContent(new[]
					{
						new KeyValuePair<string, string>("grant_type", "client_credentials"),
						new KeyValuePair<string, string>("client_id", (string)User.Accounts["MsGroove"]["ClientId"]),
						new KeyValuePair<string, string>("client_secret", (string)User.Accounts["MsGroove"]["ClientSecret"]),
						new KeyValuePair<string, string>("scope", "app.music.xboxlive.com"),
					});

					string loginContent = await Utils.GetHttpResponse(client, loginRequest, cancelToken);
					JObject loginData = Utils.DeserializeJson(loginContent);

					if (loginData?.SelectToken("access_token") != null)
					{
						ApiSessionData.MsAccessToken = (string)loginData.SelectToken("access_token");
						TimeSpan validDuration = TimeSpan.FromSeconds((int)loginData.SelectToken("expires_in"));
						ApiSessionData.MsAccessTokenExpireDate = DateTime.Now.Add(validDuration);
					}
				}
			}

			if (ApiSessionData.MsAccessToken != null)
			{
				string tokenEncoded = WebUtility.UrlEncode(ApiSessionData.MsAccessToken);

				using (HttpRequestMessage searchRequest = new HttpRequestMessage())
				{
					searchRequest.RequestUri = new Uri("https://music.xboxlive.com/1/content/music/search?q=" + searchTermEnc + "&maxItems=1&filters=tracks&contentType=JSON");
					searchRequest.Headers.Add("Authorization", "Bearer " + tokenEncoded);

					// ###########################################################################
					string searchContent = await Utils.GetHttpResponse(client, searchRequest, cancelToken);
					JObject searchData = Utils.DeserializeJson(searchContent);

					if (searchData?.SelectToken("Tracks.Items") != null)
					{
						o.Artist = (string)searchData.SelectToken("Tracks.Items[0].Artists[0].Artist.Name");
						o.Title = (string)searchData.SelectToken("Tracks.Items[0].Name");
						o.Album = (string)searchData.SelectToken("Tracks.Items[0].Album.Name");
						o.Date = (string)searchData.SelectToken("Tracks.Items[0].ReleaseDate");
						o.Genre = (string)searchData.SelectToken("Tracks.Items[0].Genres[0]");
						o.Cover = (string)searchData.SelectToken("Tracks.Items[0].Album.ImageUrl");
						o.DiscCount = null;
						o.DiscNumber = null;
						o.TrackNumber = (string)searchData.SelectToken("Tracks.Items[0].TrackNumber");

						// ###########################################################################
						using (HttpRequestMessage albumRequest = new HttpRequestMessage())
						{
							albumRequest.Headers.Add("Authorization", "Bearer " + tokenEncoded);
							albumRequest.RequestUri = new Uri("https://music.xboxlive.com/1/content/" + (string)searchData.SelectToken("Tracks.Items[0].Album.Id") + "/lookup?contentType=JSON");

							string albumContent = await Utils.GetHttpResponse(client, albumRequest, cancelToken, suppressedStatusCodes: new[] { 404 });
							JObject albumData = Utils.DeserializeJson(albumContent);

							if (albumData?.SelectToken("Albums.Items") != null)
							{
								o.TrackCount = (string)albumData.SelectToken("Albums.Items[0].TrackCount");
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
