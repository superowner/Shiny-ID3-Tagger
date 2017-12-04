//-----------------------------------------------------------------------
// <copyright file="GetLyrics_Netease.cs" company="Shiny Id3 Tagger">
//	 Copyright (c) Shiny Id3 Tagger. All rights reserved.
// </copyright>
// <author>ShinyId3Tagger Team</author>
// <summary>Retrieves track lyrics from Netease</summary>
// https://github.com/JounQin/netease-muisc-api/blob/master/api/lyric.js
//-----------------------------------------------------------------------

namespace GlobalNamespace
{
	using System;
	using System.Collections.Generic;
	using System.Diagnostics;
	using System.Linq;
	using System.Net;	
	using System.Net.Http;
	using System.Text.RegularExpressions;
	using System.Threading;
	using System.Threading.Tasks;
	using Newtonsoft.Json;
	using Newtonsoft.Json.Linq;

	public partial class Form1
	{
		private async Task<Id3> GetLyrics_Netease(HttpMessageInvoker client, Id3 tagNew, CancellationToken cancelToken)
		{
			Id3 o = new Id3();
			o.Service = "Netease";

			Stopwatch sw = new Stopwatch();
			sw.Start();
			
			// ###########################################################################				
			if (tagNew.Artist != null && tagNew.Title != null)
			{
				HttpRequestMessage request = new HttpRequestMessage(HttpMethod.Post, "http://music.163.com/api/search/get/");			
				request.Headers.Add("referer", "http://music.163.com");
				request.Headers.Add("Cookie", "appver=2.0.2");
				request.Content = new FormUrlEncodedContent(new[]
					{
						new KeyValuePair<string, string>("s", WebUtility.UrlEncode(tagNew.Artist + " - " + tagNew.Title)),
						new KeyValuePair<string, string>("type", "1")
					});
				
				string content1 = await this.GetResponse(client, request, cancelToken);
				JObject data1 = JsonConvert.DeserializeObject<JObject>(content1, this.GetJsonSettings());	

				if (data1 != null && data1.SelectToken("result.songs") != null)
				{
					if ((string)data1.SelectToken("result.songs[0].artists[0].name") == tagNew.Artist &&
						(string)data1.SelectToken("result.songs[0].name") == tagNew.Title && 
						(string)data1.SelectToken("result.songs[0].id") != null)
					{
						string songid = (string)data1.SelectToken("result.songs[0].id");
						
						request = new HttpRequestMessage();
						request.Headers.Add("referer", "http://music.163.com");
						request.Headers.Add("Cookie", "appver=2.0.2");
						request.RequestUri = new Uri("http://music.163.com/api/song/lyric/?id=" + songid + "&lv=-1&kv=-1&tv=-1");
						
						string content2 = await this.GetResponse(client, request, cancelToken);
						JObject data2 = JsonConvert.DeserializeObject<JObject>(content2, this.GetJsonSettings());
						
						if (data2 != null && data2.SelectToken("lrc.lyric") != null)
						{
							string rawLyrics = (string)data2.SelectToken("lrc.lyric");
							
							// Sanitize lyrics
							rawLyrics = Regex.Replace(rawLyrics, @"[\r\n]\[x-trans\].*", string.Empty);					// Remove [x-trans] lines (chinese translation)
							rawLyrics = Regex.Replace(rawLyrics, @"\[\d{2}:\d{2}(\.\d{2})?\]([\r\n])?", string.Empty);	// Remove timestamps like [01:01:123] or [01:01]
							rawLyrics = Regex.Replace(rawLyrics, @".*?[\u4E00-\u9FFF]+.*?[\r\n]", string.Empty);		// Remove lines where chinese characters are. Most of time they are credits like [by: xyz]
							rawLyrics = Regex.Replace(rawLyrics, @"\[.*?\]", string.Empty);								// Remove square brackets [by: XYZ] credits
							rawLyrics = Regex.Replace(rawLyrics, @"<\d+>", string.Empty);								// Remove angle brackets <123>. No idea for what they are. Example track is "ABBA - Gimme Gimme Gimme"
							rawLyrics = string.Join("\n", rawLyrics.Split('\n').Select(s => s.Trim()));					// Remove leading or ending white space per line
							rawLyrics = rawLyrics.Trim();																// Remove leading or ending line breaks and white space
							
							if (rawLyrics.Length > 1)
							{
								o.Lyrics = rawLyrics;
							}
						}
					}
				}
				
				request.Dispose();
			}
			
			// ###########################################################################
			sw.Stop();
			o.Duration = string.Format("{0:s\\,f}", sw.Elapsed);

			return o;
		}
	}
}

// System.IO.File.WriteAllText (@"D:\response.json", content);