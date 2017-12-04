//-----------------------------------------------------------------------
// <copyright file="GetTags_MsGroove.cs" company="Shiny Id3 Tagger">
//	 Copyright (c) Shiny Id3 Tagger. All rights reserved.
// </copyright>
// <author>ShinyId3Tagger Team</author>
// <summary>Gets ID3 data from Microsoft Groove API for current track</summary>
// https://apps.dev.microsoft.com/#/appList
// https://developer.microsoft.com/de-de/dashboard/groove
// https://docs.microsoft.com/en-us/groove/groove-service-rest-reference/uri-search-content#examples
//-----------------------------------------------------------------------

namespace GlobalNamespace
{
	using System;
	using System.Collections.Generic;
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
		private async Task<Id3> GetTags_MsGroove(HttpMessageInvoker client, string artist, string title, CancellationToken cancelToken)
		{
			Id3 o = new Id3();
			o.Service = "Microsoft Groove";
			
			Stopwatch sw = new Stopwatch();
			sw.Start();

			// ###########################################################################
			string searchTermEnc = WebUtility.UrlEncode(artist + " - " + title);

			HttpRequestMessage request = new HttpRequestMessage();
			
			if (ApiSessionData.MsAccessToken == null || ApiSessionData.MsAccessTokenExpireDate < DateTime.Now)
			{
				request = new HttpRequestMessage(HttpMethod.Post, "https://login.live.com/accesstoken.srf");
				request.Content = new FormUrlEncodedContent(new[]
				{
					new KeyValuePair<string, string>("grant_type", "client_credentials"),
					new KeyValuePair<string, string>("client_id", User.Accounts["MsClientId"]),
					new KeyValuePair<string, string>("client_secret", User.Accounts["MsClientSecret"]),
					new KeyValuePair<string, string>("scope", "app.music.xboxlive.com")
				});

				string loginContent = await this.GetResponse(client, request, cancelToken);
				JObject loginData = JsonConvert.DeserializeObject<JObject>(loginContent, this.GetJsonSettings());
				
				if (loginData != null && loginData.SelectToken("access_token") != null)
				{
					ApiSessionData.MsAccessToken = (string)loginData.SelectToken("access_token");
					TimeSpan validDuration = TimeSpan.FromSeconds((int)loginData.SelectToken("expires_in"));
					ApiSessionData.MsAccessTokenExpireDate = DateTime.Now.Add(validDuration);
				}
			}
				
			if (ApiSessionData.MsAccessToken != null)
			{
				string tokenEncoded = WebUtility.UrlEncode(ApiSessionData.MsAccessToken);
				
				request = new HttpRequestMessage(HttpMethod.Get, "https://music.xboxlive.com/1/content/music/search?q=" + searchTermEnc + "&maxItems=1&filters=tracks&contentType=JSON");
				request.Headers.Add("Authorization", "Bearer " + tokenEncoded);
				
				// ###########################################################################
				string content1 = await this.GetResponse(client, request, cancelToken);
				JObject data1 = JsonConvert.DeserializeObject<JObject>(content1, this.GetJsonSettings());

				if (data1 != null && data1.SelectToken("Tracks.Items") != null)
				{
					o.Artist = (string)data1.SelectToken("Tracks.Items[0].Artists[0].Artist.Name");
					o.Title = (string)data1.SelectToken("Tracks.Items[0].Name");
					o.Album = (string)data1.SelectToken("Tracks.Items[0].Album.Name");
					o.Date = (string)data1.SelectToken("Tracks.Items[0].ReleaseDate");
					o.Genre = (string)data1.SelectToken("Tracks.Items[0].Genres[0]");
					o.Cover = (string)data1.SelectToken("Tracks.Items[0].Album.ImageUrl");
					o.DiscCount = null;
					o.DiscNumber = null;
					o.TrackNumber = (string)data1.SelectToken("Tracks.Items[0].TrackNumber");
					
					// ###########################################################################
					request = new HttpRequestMessage();
					request.Headers.Add("Authorization", "Bearer " + tokenEncoded);
					request.RequestUri = new Uri("https://music.xboxlive.com/1/content/" + (string)data1.SelectToken("Tracks.Items[0].Album.Id") + "/lookup?contentType=JSON");
				
					string content2 = await this.GetResponse(client, request, cancelToken);
					JObject data2 = JsonConvert.DeserializeObject<JObject>(content2, this.GetJsonSettings());

					if (data2 != null && data2.SelectToken("Albums.Items") != null)
					{
						o.TrackCount = (string)data2.SelectToken("Albums.Items[0].TrackCount");
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