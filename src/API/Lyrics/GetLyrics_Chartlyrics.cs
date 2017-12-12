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
			string artistEncoded = WebUtility.UrlEncode(tagNew.Artist);
			string titleEncoded = WebUtility.UrlEncode(tagNew.Title);

			using (HttpRequestMessage searchRequest = new HttpRequestMessage())
			{
				searchRequest.RequestUri = new Uri("http://api.chartlyrics.com/apiv1.asmx/SearchLyricDirect?artist=" + artistEncoded + "&song=" + titleEncoded);

				string searchContent = await this.GetResponse(client, searchRequest, cancelToken);
				JObject searchData = this.DeserializeJson(this.ConvertXmlToJson(searchContent));

				if (searchData != null && searchData.SelectToken("GetLyricResult.Lyric") != null)
				{
					string artistLrc = (string)searchData.SelectToken("GetLyricResult.LyricArtist");
					string titleLrc = (string)searchData.SelectToken("GetLyricResult.LyricSong");

					if (artistLrc.ToLowerInvariant() == tagNew.Artist.ToLowerInvariant() &&
						titleLrc.ToLowerInvariant() == tagNew.Title.ToLowerInvariant() &&
						(string)searchData.SelectToken("GetLyricResult.Lyric") != null)
					{
						string rawLyrics = (string)searchData.SelectToken("GetLyricResult.Lyric");

						// Sanitize lyrics
						rawLyrics = CheckMalformedUtf8(rawLyrics);                                                  // Checks and converts a string to UTF-8 if needed/possible
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
