//-----------------------------------------------------------------------
// <copyright file="GetLyrics_Netease.cs" company="Shiny ID3 Tagger">
// Copyright (c) Shiny ID3 Tagger. All rights reserved.
// </copyright>
// <author>ShinyId3Tagger Team</author>
// <summary>Retrieves track lyrics from Netease</summary>
// https://github.com/JounQin/netease-muisc-api/blob/master/api/lyric.js
//-----------------------------------------------------------------------

namespace GetLyrics
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
    using GlobalVariables;
    using Newtonsoft.Json.Linq;
    using Utils;

	public class Netease : IGetLyricsService
	{
		private const string ServiceName = "Netease";

		public async Task<Id3> GetLyrics(HttpMessageInvoker client, Id3 tagNew, CancellationToken cancelToken)
		{
			Id3 o = new Id3 {Service = ServiceName};

			Stopwatch sw = new Stopwatch();
			sw.Start();

			// ###########################################################################
			using (HttpRequestMessage searchRequest = new HttpRequestMessage())
			{
				searchRequest.Method = HttpMethod.Post;
				searchRequest.Headers.Add("referer", "http://music.163.com");
				searchRequest.RequestUri = new Uri("http://music.163.com/api/search/get/");
				searchRequest.Content = new FormUrlEncodedContent(new[]
					{
						new KeyValuePair<string, string>("s", WebUtility.UrlEncode(tagNew.Artist + " - " + tagNew.Title)),
						new KeyValuePair<string, string>("type", "1")
					});

				string searchContent = await Utils.GetResponse(client, searchRequest, cancelToken);
				JObject searchData = Utils.DeserializeJson(searchContent);

				if (searchData != null)
				{
					// Check if any returned song artist and title match search parameters
					JToken song = (from track in searchData.SelectTokens("result.songs[*]")
									where track.SelectToken("artists[0].name").ToString().ToLowerInvariant() == tagNew.Artist.ToLowerInvariant()
									where track.SelectToken("name").ToString().ToLowerInvariant() == tagNew.Title.ToLowerInvariant()
									select track).FirstOrDefault();

					if (song?.SelectToken("id") != null)
					{
						string songId = (string)song.SelectToken("id");

						using (HttpRequestMessage lyricsRequest = new HttpRequestMessage())
						{
							lyricsRequest.Headers.Add("referer", "http://music.163.com");
							lyricsRequest.RequestUri = new Uri("http://music.163.com/api/song/lyric/?id=" + songId + "&lv=-1&kv=-1&tv=-1");

							string lyricsContent = await Utils.GetResponse(client, lyricsRequest, cancelToken);
							JObject lyricsData = Utils.DeserializeJson(lyricsContent);

							if (lyricsData?.SelectToken("lrc.lyric") != null)
							{
								string rawLyrics = (string)lyricsData.SelectToken("lrc.lyric");

								// Sanitize lyrics
								rawLyrics = Regex.Replace(rawLyrics, @"[\r\n]\[x-trans\].*", string.Empty);                 // Remove [x-trans] lines (Chinese translation)
								rawLyrics = Regex.Replace(rawLyrics, @"\[\d{2}:\d{2}(\.\d{2})?\]([\r\n])?", string.Empty);  // Remove timestamps like [01:01:123] or [01:01]
								rawLyrics = Regex.Replace(rawLyrics, @".*?[\u4E00-\u9FFF]+.*?[\r\n]", string.Empty);        // Remove lines where Chinese characters are. Most of them are credits like [by: XYZ]
								rawLyrics = Regex.Replace(rawLyrics, @"\[.*?\]", string.Empty);                             // Remove square brackets [by: XYZ] credits
								rawLyrics = Regex.Replace(rawLyrics, @"<\d+>", string.Empty);                               // Remove angle brackets <123>. No idea for what they are. Example track is "ABBA - Gimme Gimme Gimme"
								rawLyrics = string.Join("\n", rawLyrics.Split('\n').Select(s => s.Trim()));                 // Remove leading or ending white space per line
								rawLyrics = rawLyrics.Trim();                                                               // Remove leading or ending line breaks and white space

								if (rawLyrics.Length > 1)
								{
									o.Lyrics = rawLyrics;
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
