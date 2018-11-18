//-----------------------------------------------------------------------
// <copyright file="ChartLyrics.cs" company="Shiny ID3 Tagger">
// Copyright (c) Shiny ID3 Tagger. All rights reserved.
// </copyright>
// <author>ShinyId3Tagger Team</author>
//-----------------------------------------------------------------------

namespace GetLyrics
{
	using System;
	using System.Diagnostics;
	using System.Linq;
	using System.Net;
	using System.Net.Http;
	using System.Threading;
	using System.Threading.Tasks;
	using GlobalVariables;
	using Newtonsoft.Json.Linq;
	using Utils;

	/// <summary>
	/// Class for chartlyrics API
	/// </summary>
	internal class ChartLyrics : IGetLyricsService
	{
		/// <summary>
		/// Gets lyrics from chartlyrics API
		/// <seealso href="http://www.chartlyrics.com/api.aspx"/>
		/// </summary>
		/// <param name="client">The HTTP client which is passed on to GetResponse method</param>
		/// <param name="tagNew">The input artist and song title to search for</param>
		/// <param name="cancelToken">The cancelation token which is passed on to GetResponse method</param>
		/// <returns>The ID3 tag object with the results from this API for lyrics</returns>
		public async Task<Id3> GetLyrics(HttpMessageInvoker client, Id3 tagNew, CancellationToken cancelToken)
		{
			Id3 o = new Id3 { Service = "Chartlyrics" };

			Stopwatch sw = new Stopwatch();
			sw.Start();

			// ###########################################################################
			string artistEncoded = WebUtility.UrlEncode(tagNew.Artist);
			string titleEncoded = WebUtility.UrlEncode(tagNew.Title);

			using (HttpRequestMessage searchRequest = new HttpRequestMessage())
			{
				searchRequest.RequestUri = new Uri("http://api.chartlyrics.com/apiv1.asmx/SearchLyricDirect?artist=" +
												   artistEncoded + "&song=" + titleEncoded);

				string searchContent = await Utils.GetHttpResponse(client, searchRequest, cancelToken, suppressedStatusCodes: new[] { 404, 500 });
				JObject searchData = Utils.DeserializeJson(Utils.ConvertXmlToJson(searchContent));

				if (searchData?.SelectToken("GetLyricResult.Lyric") != null)
				{
					string artistLrc = (string)searchData.SelectToken("GetLyricResult.LyricArtist");
					string titleLrc = (string)searchData.SelectToken("GetLyricResult.LyricSong");

					if (string.Equals(artistLrc, tagNew.Artist, StringComparison.InvariantCultureIgnoreCase) &&
						string.Equals(titleLrc, tagNew.Title, StringComparison.InvariantCultureIgnoreCase) &&
						(string)searchData.SelectToken("GetLyricResult.Lyric") != null)
					{
						string rawLyrics = (string)searchData.SelectToken("GetLyricResult.Lyric");

						// Sanitize lyrics
						rawLyrics = Utils.ConvertMalformedUtf8(rawLyrics); // Checks and converts a string to UTF-8 if needed/possible
						rawLyrics = string.Join(
							Environment.NewLine,
							rawLyrics.Split('\n')
									 .Select(s => s.Trim())); // Remove leading or ending white space per line
						rawLyrics = rawLyrics.Trim(); // Remove leading or ending line breaks and white space

						if (rawLyrics.Length > 1)
						{
							o.Lyrics = rawLyrics;
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
