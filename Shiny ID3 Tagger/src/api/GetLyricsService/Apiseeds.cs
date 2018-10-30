//-----------------------------------------------------------------------
// <copyright file="Apiseeds.cs" company="Shiny ID3 Tagger">
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
	/// Class for Apiseeds API
	/// </summary>
	internal class Apiseeds : IGetLyricsService
	{
		/// <summary>
		/// Gets lyrics from Apiseeds API
		/// https://apiseeds.com/account/dashboard
		/// https://apiseeds.com/documentation/lyrics
		/// Warning: Can only do 20.000 calls per month
		/// </summary>
		/// <param name="client">The HTTP client which is passed on to GetResponse method</param>
		/// <param name="tagNew">The input artist and song title to search for</param>
		/// <param name="cancelToken">The cancelation token which is passed on to GetResponse method</param>
		/// <returns>The ID3 tag object with the results from this API for lyrics</returns>
		public async Task<Id3> GetLyrics(HttpMessageInvoker client, Id3 tagNew, CancellationToken cancelToken)
		{
			Id3 o = new Id3 { Service = "Apiseeds" };

			Stopwatch sw = new Stopwatch();
			sw.Start();

			// ###########################################################################
			string artistEncoded = WebUtility.UrlEncode(tagNew.Artist);
			string titleEncoded = WebUtility.UrlEncode(tagNew.Title);

			using (HttpRequestMessage searchRequest = new HttpRequestMessage())
			{
				searchRequest.RequestUri = new Uri("https://orion.apiseeds.com/api/music/lyric/" +
					artistEncoded + "/" + titleEncoded + "?apikey=" + (string)User.Accounts["Apiseeds"]["ApiKey"]);

				string searchContent = await Utils.GetResponse(client, searchRequest, cancelToken, suppressedStatusCodes: new[] { 404 }, customTimeout: 2);
				JObject searchData = Utils.DeserializeJson(searchContent);

				if (searchData?.SelectToken("result.track.text") != null)
				{
					string rawLyrics = (string)searchData.SelectToken("result.track.text");

					// Sanitize lyrics
					rawLyrics = string.Join("\n", rawLyrics.Split('\n').Select(s => s.Trim()));					// Remove leading or ending white space per line
					rawLyrics = rawLyrics.Trim();																// Remove leading or ending line breaks and white space

					if (rawLyrics.Length > 1)
					{
						o.Lyrics = rawLyrics;
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
