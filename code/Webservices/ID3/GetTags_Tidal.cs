//-----------------------------------------------------------------------
// <copyright file="GetTags_Tidal.cs" company="Shiny Id3 Tagger">
//	 Copyright (c) Shiny Id3 Tagger. All rights reserved.
// </copyright>
// <author>ShinyId3Tagger Team</author>
// <summary>Gets ID3 data from Tidal API for current track</summary>
// https://github.com/lucaslg26/TidalAPI/blob/master/lib/client.js
// https://pythonhosted.org/tidalapi/api.html#api
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
		private async Task<Id3> GetTags_Tidal(HttpMessageInvoker client, string artist, string title, CancellationToken cancelToken)
		{
			Id3 o = new Id3();
			o.Service = "Tidal";
			
			Stopwatch sw = new Stopwatch();
			sw.Start();

			// ###########################################################################
			string searchTermEnc = WebUtility.UrlEncode(artist + " - " + title);
			
			HttpRequestMessage request = new HttpRequestMessage();

			if (ApiSessionData.TiSessionID == null || ApiSessionData.TiSessionExpireDate < DateTime.Now)
			{				
				request = new HttpRequestMessage(HttpMethod.Post, "http://api.tidalhifi.com/v1/login/username");
				request.Headers.Add("X-Tidal-Token", User.Accounts["TiToken"]);
				request.Content = new FormUrlEncodedContent(new[]
					{
						new KeyValuePair<string, string>("username", User.Accounts["TiUsername"]),
						new KeyValuePair<string, string>("password", User.Accounts["TiPassword"])
					});			
					
				string loginContent = await this.GetResponse(client, request, cancelToken);
				JObject loginData = JsonConvert.DeserializeObject<JObject>(loginContent, this.GetJsonSettings());
				
				if (loginData != null && loginData.SelectToken("sessionId") != null)
				{
					ApiSessionData.TiSessionID = (string)loginData.SelectToken("sessionId");
					ApiSessionData.TiCountryCode = (string)loginData.SelectToken("countryCode");

					string userID = (string)loginData.SelectToken("userId");
					
					request = new HttpRequestMessage(HttpMethod.Get, "http://api.tidalhifi.com/v1/users/" + userID + "/subscription");
					request.Headers.Add("X-Tidal-SessionId", ApiSessionData.TiSessionID);
					
					string sessionContent = await this.GetResponse(client, request, cancelToken);
					JObject sessionData = JsonConvert.DeserializeObject<JObject>(sessionContent, this.GetJsonSettings());

					if (sessionData != null)
					{
						// 30mins is the "offlineGracePeriod" which I asume is the timespan a session is valid. I could be wrong since there is no documentation about this :(
						TimeSpan validDuration = TimeSpan.FromSeconds((int)sessionData.SelectToken("subscription.offlineGracePeriod") * 60); 
						ApiSessionData.TiSessionExpireDate = DateTime.Now.Add(validDuration);
					}
				}
			}
			
			if (ApiSessionData.TiSessionID != null)
			{
				request = new HttpRequestMessage();			
				request.Headers.Add("Origin", "http://listen.tidal.com");
				request.Headers.Add("X-Tidal-SessionId", ApiSessionData.TiSessionID);
				request.RequestUri = new Uri("http://api.tidalhifi.com/v1/search?types=TRACKS&countryCode=" + ApiSessionData.TiCountryCode + "&query=" + searchTermEnc);
				
				string content1 = await this.GetResponse(client, request, cancelToken);
				JObject data1 = JsonConvert.DeserializeObject<JObject>(content1, this.GetJsonSettings());
				
				if (data1 != null && data1.SelectToken("tracks.items[0]") != null)
				{
					o.Artist = (string)data1.SelectToken("tracks.items[0].artists[0].name");
					o.Title = (string)data1.SelectToken("tracks.items[0].title");
					o.Album = (string)data1.SelectToken("tracks.items[0].album.title");				
					o.TrackNumber = (string)data1.SelectToken("tracks.items[0].trackNumber");
					o.DiscNumber = (string)data1.SelectToken("tracks.items[0].volumeNumber");
				
					if (data1.SelectToken("tracks.items[0].album.id") != null)
					{
						request = new HttpRequestMessage();			
						request.Headers.Add("Origin", "http://listen.tidal.com");
						request.Headers.Add("X-Tidal-SessionId", ApiSessionData.TiSessionID);				
						request.RequestUri = new Uri("http://api.tidalhifi.com/v1/albums/" + (string)data1.SelectToken("tracks.items[0].album.id") + "?countryCode=" + ApiSessionData.TiCountryCode);
		
						string content2 = await this.GetResponse(client, request, cancelToken);
						JObject data2 = JsonConvert.DeserializeObject<JObject>(content2, this.GetJsonSettings());
						
						if (data2 != null)
						{
							o.Genre = null;													// tidal API doesn't provide genres for specific items, only a general list of genres (https://pythonhosted.org/tidalapi/api.html#api)
							o.Date = (string)data2.SelectToken("releaseDate");		
							o.TrackCount = (string)data2.SelectToken("numberOfTracks");
							o.DiscCount = (string)data2.SelectToken("numberOfVolumes");
						
							if (data2.SelectToken("cover") != null)
							{
								o.Cover = "http://resources.tidal.com/images/" + ((string)data2.SelectToken("cover")).Replace("-", "/") + "/1280x1280.jpg";
							}
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