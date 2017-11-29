//-----------------------------------------------------------------------
// <copyright file="GetLyrics_Chartlyrics.cs" company="Shiny Id3 Tagger">
//	 Copyright (c) Shiny Id3 Tagger. All rights reserved.
// </copyright>
// <author>ShinyId3Tagger Team</author>
// <summary>Retrieves track lyrics from chartlyrics.com</summary>
// http://www.chartlyrics.com/api.aspx
// https://msdn.microsoft.com/en-us/library/system.text.encodinginfo.getencoding(v=vs.110).aspx
//-----------------------------------------------------------------------

namespace GlobalNamespace
{
	using System;
	using System.Linq;
	using System.Net;
	using System.Net.Http;
	using System.Threading;
	using System.Threading.Tasks;
	using Newtonsoft.Json;
	using Newtonsoft.Json.Linq;

	public partial class Form1
	{
		private async Task<string> GetLyrics_Chartlyrics(HttpMessageInvoker client, Id3 tagNew, CancellationToken cancelToken)
		{
			if (tagNew.Artist != null && tagNew.Title != null)
			{
				string artistEnc = WebUtility.UrlEncode(tagNew.Artist);
				string titleEnc = WebUtility.UrlEncode(tagNew.Title);
				
				HttpRequestMessage request = new HttpRequestMessage();
				request.RequestUri = new Uri("http://api.chartlyrics.com/apiv1.asmx/SearchLyricDirect?artist=" + artistEnc + "&song=" + titleEnc);

				string content = await this.GetRequest(client, request, cancelToken);

				if (!content.StartsWith("SearchLyricDirect:", StringComparison.InvariantCultureIgnoreCase))
				{
					string xml = this.ConvertXmlToJson(content);
					JObject data = JsonConvert.DeserializeObject<JObject>(xml, this.GetJsonSettings());

					if (data != null && data.SelectToken("GetLyricResult.Lyric") != null)
					{
						string artistTemp = (string)data.SelectToken("GetLyricResult.LyricArtist");
						string titleTemp = (string)data.SelectToken("GetLyricResult.LyricSong");

						if (artistTemp == tagNew.Artist && titleTemp == tagNew.Title)
						{
							string response = (string)data.SelectToken("GetLyricResult.Lyric");

							if (!string.IsNullOrWhiteSpace(response))
							{
								response = CheckMalformedUtf8(response);
								
								tagNew.Lyrics = response.Trim('\r','n').Trim().Trim('\r','n');							// Remove leading or ending line breaks and white space
								this.PrintLogMessage("search", new[] { "  Lyrics taken from Chartlyrics" });
							}
						}
					}
				}

				request.Dispose();
			}

			return tagNew.Lyrics;
		}
	}
}

// System.IO.File.WriteAllText (@"D:\response.json", content);
