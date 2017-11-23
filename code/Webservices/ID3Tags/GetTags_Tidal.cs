//-----------------------------------------------------------------------
// <copyright file="GetTags_Tidal.cs" company="Shiny Id3 Tagger">
//	 Copyright (c) Shiny Id3 Tagger. All rights reserved.
// </copyright>
// <author>ShinyId3Tagger Team</author>
// <summary>Gets ID3 data from Tidal API for current track</summary>
// https://github.com/lucaslg26/TidalAPI/blob/master/lib/client.js
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
			
			HttpRequestMessage request = new HttpRequestMessage(HttpMethod.Post, "https://api.tidalhifi.com/v1/login/username");
			request.Headers.Add("X-Tidal-Token", User.Accounts["TiToken"]);
			request.Content = new FormUrlEncodedContent(new[]
				{
					new KeyValuePair<string, string>("username", User.Accounts["TiUsername"]),
					new KeyValuePair<string, string>("password", User.Accounts["TiPassword"])
				});

			string content1 = await this.GetRequest(client, request, cancelToken);
			JObject data1 = JsonConvert.DeserializeObject<JObject>(content1, this.GetJsonSettings());

			if (data1 != null && data1.SelectToken("sessionId") != null)
			{
				request = new HttpRequestMessage();			
				request.Headers.Add("Origin", "http://listen.tidal.com");
				request.Headers.Add("X-Tidal-SessionId", (string)data1.SelectToken("sessionId"));							
				request.RequestUri = new Uri("https://api.tidalhifi.com/v1/search?types=TRACKS&countryCode=" + (string)data1.SelectToken("countryCode") + "&query=" + searchTermEnc);
				
				string content2 = await this.GetRequest(client, request, cancelToken);
				JObject data2 = JsonConvert.DeserializeObject<JObject>(content2, this.GetJsonSettings());
				
				o.Artist = (string)data2.SelectToken("tracks.items[0].artists[0].name");
				o.Title = (string)data2.SelectToken("tracks.items[0].title");
				o.Album = (string)data2.SelectToken("tracks.items[0].album.title");				
				o.TrackNumber = (string)data2.SelectToken("tracks.items[0].trackNumber");
				o.DiscNumber = (string)data2.SelectToken("tracks.items[0].volumeNumber");
				
				if (data2 != null && data2.SelectToken("tracks.items[0].album.id") != null)
				{
					request = new HttpRequestMessage();			
					request.Headers.Add("Origin", "http://listen.tidal.com");
					request.Headers.Add("X-Tidal-SessionId", (string)data1.SelectToken("sessionId"));				
					request.RequestUri = new Uri("https://api.tidalhifi.com/v1/albums/" + (string)data2.SelectToken("tracks.items[0].album.id") + "?countryCode=" + (string)data1.SelectToken("countryCode"));
	
					string content3 = await this.GetRequest(client, request, cancelToken);
					JObject data3 = JsonConvert.DeserializeObject<JObject>(content3, this.GetJsonSettings());
					
					o.Date = (string)data3.SelectToken("releaseDate");		
					o.TrackCount = (string)data3.SelectToken("numberOfTracks");
					o.DiscCount = (string)data3.SelectToken("numberOfVolumes");
					
					if (data3 != null && data3.SelectToken("cover") != null)
					{
						o.Cover = "https://resources.tidal.com/images/" + ((string)data3.SelectToken("cover")).Replace("-", "/") + "/1280x1280.jpg";
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