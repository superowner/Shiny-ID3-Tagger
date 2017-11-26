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
	using System.Linq;
	using System.Net.Http;
	using System.Text.RegularExpressions;
	using System.Threading;
	using System.Threading.Tasks;
	using Newtonsoft.Json;
	using Newtonsoft.Json.Linq;

	public partial class Form1
	{
		private async Task<string> GetLyrics_Xiami(HttpMessageInvoker client, Id3 tagNew, CancellationToken cancelToken)
		{
			if (tagNew.Artist != null && tagNew.Title != null)
			{
				HttpRequestMessage request = new HttpRequestMessage();
				request.RequestUri = new Uri("http://api.xiami.com/web?v=2.0&limit=30&r=search/songs&app_key=1&key=" + tagNew.Artist + " - " + tagNew.Title);
				request.Headers.Add("referer", "http://h.xiami.com/");
				
				string content = await this.GetRequest(client, request, cancelToken);								
				JObject data = JsonConvert.DeserializeObject<JObject>(content, this.GetJsonSettings());

				if (data != null)
				{
					request = new HttpRequestMessage();
					
					string url = (string)data.SelectToken("data.songs[0].lyric");
					if (!string.IsNullOrWhiteSpace(url))
					{
						request.RequestUri = new Uri(url);
					
						string rawLyrics = await this.GetRequest(client, request, cancelToken);								
						
						MatchCollection matches = Regex.Matches(rawLyrics, @"(?<=^\[\d{2}:\d{2}\.\d{1,3}])[^\n\r]+", RegexOptions.Multiline);
						tagNew.Lyrics = string.Join(System.Environment.NewLine, matches.Cast<Match>().Select(
							m => Regex.Replace(m.Value, @"<\d+>", string.Empty))
						);
						
						// FIXME Wrong indention when chinese signs are visible
						this.Log("search", new[] { "  Lyrics taken from Xiami" });
					}
				}
				
				request.Dispose();
			}
			
			return tagNew.Lyrics;
		}
	}
}

// System.IO.File.WriteAllText (@"D:\response.json", content);