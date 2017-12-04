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
			if (tagNew.Artist != null && tagNew.Title != null)
			{
				string searchTermEnc = WebUtility.UrlEncode(tagNew.Artist + " - " + tagNew.Title);

				HttpRequestMessage request = new HttpRequestMessage();
				request.RequestUri = new Uri("http://api.xiami.com/web?v=2.0&limit=30&r=search/songs&app_key=1&key=" + searchTermEnc);
				request.Headers.Add("referer", "http://h.xiami.com/");

				string content1 = await this.GetResponse(client, request, cancelToken);
				JObject data1 = JsonConvert.DeserializeObject<JObject>(content1, this.GetJsonSettings());

				if (data1 != null)
				{
					// Check if any returned song artist and title match search parameters
					JToken song = (from track in data1.SelectTokens("data.songs[*]")
									where string.Equals((string)track.SelectToken("artist_name"), tagNew.Artist, StringComparison.OrdinalIgnoreCase)
									where string.Equals((string)track.SelectToken("song_name"), tagNew.Title, StringComparison.OrdinalIgnoreCase)
									select track.SelectToken("artist_name")).FirstOrDefault();

					if (song != null && IsValidUrl((string)song.SelectToken("lyric")))
					{
						request = new HttpRequestMessage();
						request.RequestUri = new Uri((string)song.SelectToken("lyric"));

						string content2 = await this.GetResponse(client, request, cancelToken);

						if (!string.IsNullOrWhiteSpace(content2))
						{
							string rawLyrics = content2;

							// Sanitize
							rawLyrics = Regex.Replace(rawLyrics, @"[\r\n]\[x-trans\].*", string.Empty);					// Remove [x-trans] lines (chinese translation)
							rawLyrics = Regex.Replace(rawLyrics, @"\[\d{2}:\d{2}(\.\d{2})?\]([\r\n])?", string.Empty);	// Remove timestamps like [01:01:123] or [01:01]
							rawLyrics = Regex.Replace(rawLyrics, @".*?[\u4E00-\u9FFF]+.*?[\r\n]", string.Empty);		// Remove lines where chinese characters are. Most of time they are credits like [by: xyz]
							rawLyrics = Regex.Replace(rawLyrics, @"\[.*?\]", string.Empty);								// Remove square brackets [by: XYZ] credits
							rawLyrics = Regex.Replace(rawLyrics, @"<\d+>", string.Empty);								// Remove angle brackets <123>. No idea for what they are. Example track is "ABBA - Gimme Gimme Gimme"
							rawLyrics = string.Join("\n", rawLyrics.Split('\n').Select(s => s.Trim()));					// Remove leading or ending white space per line
							rawLyrics = rawLyrics.Trim();																// Remove leading or ending line breaks

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