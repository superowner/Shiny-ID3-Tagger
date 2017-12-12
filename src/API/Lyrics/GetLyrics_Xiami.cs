//-----------------------------------------------------------------------
// <copyright file="GetLyrics_Xiami.cs" company="Shiny Id3 Tagger">
//	 Copyright (c) Shiny Id3 Tagger. All rights reserved.
// </copyright>
// <author>ShinyId3Tagger Team</author>
// <summary>Retrieves track lyrics from xiami.com</summary>
// #https://github.com/LIU9293/musicAPI/tree/master/src
//-----------------------------------------------------------------------

namespace GlobalNamespace
{
	using System;
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
		private async Task<Id3> GetLyrics_Xiami(HttpMessageInvoker client, Id3 tagNew, CancellationToken cancelToken)
		{
			Id3 o = new Id3();
			o.Service = "Xiami";

			Stopwatch sw = new Stopwatch();
			sw.Start();

			// ###########################################################################
			string searchTermEnc = WebUtility.UrlEncode(tagNew.Artist + " - " + tagNew.Title);

			using (HttpRequestMessage searchRequest = new HttpRequestMessage())
			{
				searchRequest.RequestUri = new Uri("http://api.xiami.com/web?v=2.0&limit=30&r=search/songs&app_key=1&key=" + searchTermEnc);
				searchRequest.Headers.Add("referer", "http://h.xiami.com/");

				string searchContent = await this.GetResponse(client, searchRequest, cancelToken);
				JObject searchData = this.DeserializeJson(searchContent);

				if (searchData != null)
				{
					// Check if any returned song artist and title match search parameters
					JToken song = (from track in searchData.SelectTokens("data.songs[*]")
									where track.SelectToken("artist_name").ToString().ToLowerInvariant() == tagNew.Artist.ToLowerInvariant()
									where track.SelectToken("song_name").ToString().ToLowerInvariant() == tagNew.Title.ToLowerInvariant()
									select track.SelectToken("artist_name")).FirstOrDefault();

					if (song != null && IsValidUrl((string)song.SelectToken("lyric")))
					{
						HttpRequestMessage lyricRequest = new HttpRequestMessage();
						lyricRequest.RequestUri = new Uri((string)song.SelectToken("lyric"));

						string lyricsContent = await this.GetResponse(client, lyricRequest, cancelToken);
						string rawLyrics = lyricsContent;

						if (!string.IsNullOrWhiteSpace(rawLyrics))
						{
							// Sanitize
							rawLyrics = Regex.Replace(rawLyrics, @"[\r\n]\[x-trans\].*", string.Empty);                 // Remove [x-trans] lines (Chinese translation)
							rawLyrics = Regex.Replace(rawLyrics, @"\[\d{2}:\d{2}(\.\d{2})?\]([\r\n])?", string.Empty);  // Remove timestamps like [01:01:123] or [01:01]
							rawLyrics = Regex.Replace(rawLyrics, @".*?[\u4E00-\u9FFF]+.*?[\r\n]", string.Empty);        // Remove lines where Chinese characters are. Most of them are credits like [by: XYZ]
							rawLyrics = Regex.Replace(rawLyrics, @"\[.*?\]", string.Empty);                             // Remove square brackets [by: XYZ] credits
							rawLyrics = Regex.Replace(rawLyrics, @"<\d+>", string.Empty);                               // Remove angle brackets <123>. No idea for what they are. Example track is "ABBA - Gimme Gimme Gimme"
							rawLyrics = string.Join("\n", rawLyrics.Split('\n').Select(s => s.Trim()));                 // Remove leading or ending white space per line
							rawLyrics = rawLyrics.Trim();                                                               // Remove leading or ending line breaks

							if (rawLyrics.Length > 1)
							{
								o.Lyrics = rawLyrics;
							}
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
