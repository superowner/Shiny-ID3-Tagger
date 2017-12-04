//-----------------------------------------------------------------------
// <copyright file="GetLyrics_Chartlyrics.cs" company="Shiny Id3 Tagger">
//	 Copyright (c) Shiny Id3 Tagger. All rights reserved.
// </copyright>
// <author>ShinyId3Tagger Team</author>
// <summary>Retrieves track lyrics from chartlyrics.com</summary>
// http://www.chartlyrics.com/api.aspx
//-----------------------------------------------------------------------

namespace GlobalNamespace
{
	using System;
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
		private async Task<Id3> GetLyrics_Chartlyrics(HttpMessageInvoker client, Id3 tagNew, CancellationToken cancelToken)
		{
			Id3 o = new Id3();
			o.Service = "Chartlyrics";

			Stopwatch sw = new Stopwatch();
			sw.Start();
			
			// ###########################################################################				
			if (tagNew.Artist != null && tagNew.Title != null)
			{
				string artistEncoded = WebUtility.UrlEncode(tagNew.Artist);
				string titleEncoded = WebUtility.UrlEncode(tagNew.Title);
				
				HttpRequestMessage request = new HttpRequestMessage();
				request.RequestUri = new Uri("http://api.chartlyrics.com/apiv1.asmx/SearchLyricDirect?artist=" + artistEncoded + "&song=" + titleEncoded);

				string content = await this.GetResponse(client, request, cancelToken);
				JObject data = JsonConvert.DeserializeObject<JObject>(this.ConvertXmlToJson(content), this.GetJsonSettings());

				if (data != null && data.SelectToken("GetLyricResult.Lyric") != null)
				{
					string artistTemp = (string)data.SelectToken("GetLyricResult.LyricArtist");
					string titleTemp = (string)data.SelectToken("GetLyricResult.LyricSong");

					if (artistTemp == tagNew.Artist && titleTemp == tagNew.Title && (string)data.SelectToken("GetLyricResult.Lyric") != null)
					{
						string rawLyrics = (string)data.SelectToken("GetLyricResult.Lyric");

						// Sanitize lyrics
						rawLyrics = CheckMalformedUtf8(rawLyrics);													// Checks and converts a string to UTF-8 if needed/possible
						rawLyrics = string.Join("\n", rawLyrics.Split('\n').Select(s => s.Trim()));					// Remove leading or ending white space per line
						rawLyrics = rawLyrics.Trim();																// Remove leading or ending line breaks and white space

						if (rawLyrics.Length > 1)
						{
							o.Lyrics = rawLyrics;
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