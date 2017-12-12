//-----------------------------------------------------------------------
// <copyright file="GetTags_Netease.cs" company="Shiny Id3 Tagger">
// Copyright (c) Shiny Id3 Tagger. All rights reserved.
// </copyright>
// <author>ShinyId3Tagger Team</author>
// <summary>Gets ID3 data from Netease API for current track</summary>
// https://github.com/JounQin/netease-muisc-api/tree/master/api
// https://github.com/yanunon/NeteaseCloudMusic/wiki/NetEase-cloud-music-analysis-API-%5BEN%5D
// parameter "type=10" instead of "type=1" filters for albums only. But produces less results. Stick to using "专辑" as post filter
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

			using (HttpRequestMessage searchRequest = new HttpRequestMessage())
			{
				searchRequest.Method = HttpMethod.Post;
				searchRequest.RequestUri = new Uri("http://music.163.com/api/search/pc/");
				searchRequest.Headers.Add("Referer", "http://music.163.com");
				searchRequest.Content = new FormUrlEncodedContent(new[]
					{
						new KeyValuePair<string, string>("s", searchTermEnc),
						new KeyValuePair<string, string>("type", "1")
					});

				string searchContent = await this.GetResponse(client, searchRequest, cancelToken);
				JObject searchData = this.DeserializeJson(searchContent);

				if (searchData != null && searchData.SelectToken("result.songs") != null)
				{
					// Chinese translation is "album", excludes EPs and compilations and stuff like that
					List<JToken> albums = (from songs in searchData.SelectToken("result.songs")
										   where (string)songs["album"]["type"] == "专辑"
										   select songs).ToList();

					if (albums.Count > 0)
					{
						o.Artist = (string)albums[0].SelectToken("artists[0].name");
						o.Title = (string)albums[0].SelectToken("name");
						o.Album = (string)albums[0].SelectToken("album.name");
						o.Genre = null;					// Netease provides a detailed album query with a property called "tags". But value seems always empty
						o.DiscCount = null;
						o.DiscNumber = (string)albums[0].SelectToken("disc");
						o.TrackCount = (string)albums[0].SelectToken("album.size");
						o.TrackNumber = (string)albums[0].SelectToken("position");
						o.Cover = (string)albums[0].SelectToken("album.picUrl");

						string strMilliseconds = (string)albums[0].SelectToken("album.publishTime");
						if (long.TryParse(strMilliseconds, out long milliseconds))
						{
							o.Date = epoch.AddMilliseconds(milliseconds).ToString();
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
