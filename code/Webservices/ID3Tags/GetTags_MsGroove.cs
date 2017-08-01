//-----------------------------------------------------------------------
// <copyright file="GetTags_MsGroove.cs" company="Shiny Id3 Tagger">
//	 Copyright (c) Shiny Id3 Tagger. All rights reserved.
// </copyright>
// <author>ShinyId3Tagger Team</author>
// <summary>Gets ID3 data from Microsoft Groove API for current track</summary>
// https://apps.dev.microsoft.com/#/appList
// https://developer.microsoft.com/de-de/dashboard/groove
// https://msdn.microsoft.com/en-us/library/ff512387.aspx
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

			HttpRequestMessage request = new HttpRequestMessage(HttpMethod.Post, "https://login.live.com/accesstoken.srf");
			request.Content = new FormUrlEncodedContent(new[]
				{
					new KeyValuePair<string, string>("grant_type", "client_credentials"),
					new KeyValuePair<string, string>("client_id", User.Accounts["MsClientId"]),
					new KeyValuePair<string, string>("client_secret", User.Accounts["MsClientSecret"]),
					new KeyValuePair<string, string>("scope", "app.music.xboxlive.com")
				});

			string content1 = await this.GetRequest(client, request, cancelToken);
			JObject data1 = JsonConvert.DeserializeObject<JObject>(content1, this.GetJsonSettings());

			if (data1 != null && data1.SelectToken("access_token") != null)
			{
				string token = WebUtility.UrlEncode((string)data1.SelectToken("access_token"));
				token = "Bearer" + " " + token;

				request = new HttpRequestMessage();
				request.Headers.Add("Authorization", token);
				request.RequestUri = new Uri("https://music.xboxlive.com/1/content/music/search?q=" + searchTermEnc +
					"&maxItems=1&filters=tracks&contentType=JSON&language=US&country=US");
				
				// ###########################################################################
				string content2 = await this.GetRequest(client, request, cancelToken);
				JObject data2 = JsonConvert.DeserializeObject<JObject>(content2, this.GetJsonSettings());

				if (data2 != null && data2.SelectToken("Tracks.Items") != null)
				{
					o.Artist = (string)data2.SelectToken("Tracks.Items[0].Artists[0].Artist.Name");
					o.Title = (string)data2.SelectToken("Tracks.Items[0].Name");
					o.Album = (string)data2.SelectToken("Tracks.Items[0].Album.Name");
					o.Date = (string)data2.SelectToken("Tracks.Items[0].ReleaseDate");
					o.Genre = (string)data2.SelectToken("Tracks.Items[0].Genres[0]");
					o.Cover = (string)data2.SelectToken("Tracks.Items[0].Album.ImageUrl");
					o.DiscCount = null;
					o.DiscNumber = null;
					o.TrackCount = null;
					o.TrackNumber = (string)data2.SelectToken("Tracks.Items[0].TrackNumber");
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