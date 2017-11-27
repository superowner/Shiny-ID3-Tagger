//-----------------------------------------------------------------------
// <copyright file="GetTags_Spotify.cs" company="Shiny Id3 Tagger">
//	 Copyright (c) Shiny Id3 Tagger. All rights reserved.
// </copyright>
// <author>ShinyId3Tagger Team</author>
// <summary>Gets ID3 data from Spotify API for current track</summary>
// https://developer.spotify.com/web-api/search-item/
// https://developer.spotify.com/web-api/object-model/
//-----------------------------------------------------------------------

namespace GlobalNamespace
{
	using System;
	using System.Collections.Generic;
	using System.Diagnostics;	
	using System.Linq;
	using System.Net;
	using System.Net.Http;
	using System.Text;
	using System.Threading;
	using System.Threading.Tasks;
	using Newtonsoft.Json;
	using Newtonsoft.Json.Linq;

	public partial class Form1
	{
		private async Task<Id3> GetTags_Spotify(HttpMessageInvoker client, string artist, string title, CancellationToken cancelToken)
		{
			Id3 o = new Id3();
			o.Service = "Spotify";
			
			Stopwatch sw = new Stopwatch();
			sw.Start();
			
			// ###########################################################################
			string artistEnc = WebUtility.UrlEncode(artist);
			string titleEnc = WebUtility.UrlEncode(title);
			
			HttpRequestMessage request = new HttpRequestMessage();
			
			if (SessionData.SpAccessToken == null || SessionData.SpAccessTokenExpireDate < DateTime.Now)
			{			
				request = new HttpRequestMessage(HttpMethod.Post, "https://accounts.spotify.com/api/token");
				
				Dictionary<string, string> postBody = new Dictionary<string, string> { { "grant_type", "client_credentials" } };			
				FormUrlEncodedContent postBodyEnc = new FormUrlEncodedContent(postBody);
				request.Content = postBodyEnc;
		
				string credidsPlain = User.Accounts["SpClientId"] + ":" + User.Accounts["SpClientSecret"];
				byte[] credidsBytes = Encoding.UTF8.GetBytes(credidsPlain);
				string creditsBase64 = Convert.ToBase64String(credidsBytes);
				request.Headers.Add("Authorization", "Basic " + creditsBase64);
	
				string loginContent = await this.GetRequest(client, request, cancelToken);
				JObject loginData = JsonConvert.DeserializeObject<JObject>(loginContent, this.GetJsonSettings());

				if (loginData != null && loginData.SelectToken("access_token") != null)
				{
					SessionData.SpAccessToken = (string)loginData.SelectToken("access_token");
					TimeSpan validDuration = TimeSpan.FromSeconds((int)loginData.SelectToken("expires_in"));
					SessionData.SpAccessTokenExpireDate = DateTime.Now.Add(validDuration);					
				}				
			}
			
			if (SessionData.SpAccessToken != null)
			{
				request = new HttpRequestMessage();
				request.RequestUri = new Uri("https://api.spotify.com/v1/search?q=artist:\"" + artistEnc + "\" title:\"" + titleEnc + "\"&type=track&limit=1");
				request.Headers.Add("Authorization", "Bearer " + SessionData.SpAccessToken);
	
				string content1 = await this.GetRequest(client, request, cancelToken);
				JObject data1 = JsonConvert.DeserializeObject<JObject>(content1, this.GetJsonSettings());
	
				if (data1 != null && data1.SelectToken("tracks.items") != null && data1.SelectToken("tracks.items").Any())
				{
					o.Artist = (string)data1.SelectToken("tracks.items[0].artists[0].name");
					o.Title = (string)data1.SelectToken("tracks.items[0].name");
					o.Album = (string)data1.SelectToken("tracks.items[0].album.name");
					o.DiscNumber = (string)data1.SelectToken("tracks.items[0].disc_number");
					o.TrackNumber = (string)data1.SelectToken("tracks.items[0].track_number");
	
					// ###########################################################################
					request = new HttpRequestMessage();
					request.Headers.Add("Authorization", "Bearer " + SessionData.SpAccessToken);
					
					string url = (string)data1.SelectToken("tracks.items[0].album.href");
					if (IsValidUrl(url))
					{
						request.RequestUri = new Uri(url); 
					
						string content2 = await this.GetRequest(client, request, cancelToken);
						JObject data2 = JsonConvert.DeserializeObject<JObject>(content2, this.GetJsonSettings());
		
						if (data2 != null)
						{
							o.Date = (string)data2.SelectToken("release_date");
							o.DiscCount = (string)data2.SelectToken("tracks.items[-1:].disc_number");
							o.TrackCount = (string)data2.SelectToken("tracks.total");
							o.Genre = string.Join(", ", data2.SelectToken("genres"));		// genres is always empty even for full album lookups. Seems like a general spotify issue: https://github.com/spotify/web-api/issues/157				
							o.Cover = (string)data2.SelectToken("images").OrderBy(obj => obj["height"]).Last()["url"];
						}
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

// System.IO.File.WriteAllText (@"D:\response.json", content1);