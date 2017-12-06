//-----------------------------------------------------------------------
// <copyright file="GetLyrics_Lololyrics.cs" company="Shiny Id3 Tagger">
//	 Copyright (c) Shiny Id3 Tagger. All rights reserved.
// </copyright>
// <author>ShinyId3Tagger Team</author>
// <summary>Retrieves track lyrics from lololyrics.com</summary>
// http://api.lololyrics.com/
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
		private async Task<Id3> GetLyrics_Lololyrics(HttpMessageInvoker client, Id3 tagNew, CancellationToken cancelToken)
		{
			Id3 o = new Id3();
			o.Service = "Lololyrics";

			Stopwatch sw = new Stopwatch();
			sw.Start();

			// ###########################################################################
			if (tagNew.Artist != null && tagNew.Title != null)
			{
				string artistEncoded = WebUtility.UrlEncode(tagNew.Artist);
				string titleEncoded = WebUtility.UrlEncode(tagNew.Title);

				using (HttpRequestMessage searchRequest = new HttpRequestMessage())
				{
					searchRequest.RequestUri = new Uri("http://api.lololyrics.com/0.5/getLyric?artist=" + artistEncoded + "&track=" + titleEncoded + "&rawutf8=1");

					string searchContent = await this.GetResponse(client, searchRequest, cancelToken);
					JObject searchData = JsonConvert.DeserializeObject<JObject>(this.ConvertXmlToJson(searchContent), this.GetJsonSettings());

					if (searchData != null && searchData.SelectToken("result.response") != null)
					{
						string rawLyrics = (string)searchData.SelectToken("result.response");

						// Sanitize lyrics
						rawLyrics = WebUtility.HtmlDecode(rawLyrics);                                               // URL decode lyrics
						rawLyrics = string.Join("\n", rawLyrics.Split('\n').Select(s => s.Trim()));                 // Remove leading or ending white space per line
						rawLyrics = rawLyrics.Trim();                                                               // Remove leading or ending line breaks and white space

						if (rawLyrics.Length > 1)
						{
							o.Lyrics = rawLyrics;
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

// System.IO.File.WriteAllText (@"D:\response.json", content);