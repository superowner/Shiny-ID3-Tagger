//-----------------------------------------------------------------------
// <copyright file="GetTags_Netease.cs" company="Shiny Id3 Tagger">
//	 Copyright (c) Shiny Id3 Tagger. All rights reserved.
// </copyright>
// <author>ShinyId3Tagger Team</author>
// <summary>Gets ID3 data from Netease API for current track</summary>
// https://github.com/JounQin/netease-muisc-api/tree/master/api
// https://github.com/yanunon/NeteaseCloudMusic/wiki/NetEase-cloud-music-analysis-API-%5BEN%5D
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
		private async Task<Id3> GetTags_Netease(HttpMessageInvoker client, string artist, string title, CancellationToken cancelToken)
		{
			Id3 o = new Id3();
			o.Service = "Netease";
			
			Stopwatch sw = new Stopwatch();
			sw.Start();
			
			// ###########################################################################
			string searchTermEnc = WebUtility.UrlEncode(artist + " - " + title);
			
			HttpRequestMessage request = new HttpRequestMessage(HttpMethod.Post, "http://music.163.com/api/search/pc/");			
			request.Headers.Add("Referer", "http://music.163.com");
			request.Content = new FormUrlEncodedContent(new[]
				{
					new KeyValuePair<string, string>("s", searchTermEnc),
					new KeyValuePair<string, string>("type", "1")
				});
			
			string content = await this.GetResponse(client, request, cancelToken);
			JObject data = JsonConvert.DeserializeObject<JObject>(content, this.GetJsonSettings());			

			if (data != null && data.SelectToken("result.songs") != null)
			{
				List<JToken> albums = (from songs in data.SelectToken("result.songs")
					where (string)songs["album"]["type"] == "专辑"  // Chinese transation is "album", excludes EPs and compilations and stuff like that
					select songs).ToList();
				
				if (albums.Count > 0)
				{
					o.Artist = (string)albums[0].SelectToken("artists[0].name");
					o.Title = (string)albums[0].SelectToken("name");
					o.Album = (string)albums[0].SelectToken("album.name");
					o.Genre = null;			// Netease provide a detailed album query with a property called "tags". But the value seems always empty
					o.DiscCount = null;
					o.DiscNumber = (string)albums[0].SelectToken("disc");
					o.TrackCount = (string)albums[0].SelectToken("album.size");
					o.TrackNumber = (string)albums[0].SelectToken("position");
					o.Cover = (string)albums[0].SelectToken("album.picUrl");

					long milliseconds;
					string strMilliseconds = (string)albums[0].SelectToken("album.publishTime");
					if (long.TryParse(strMilliseconds, out milliseconds))
					{
						o.Date = epoch.AddMilliseconds(milliseconds).ToString();
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