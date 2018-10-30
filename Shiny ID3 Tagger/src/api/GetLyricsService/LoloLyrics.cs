﻿//-----------------------------------------------------------------------
// <copyright file="LoloLyrics.cs" company="Shiny ID3 Tagger">
// Copyright (c) Shiny ID3 Tagger. All rights reserved.
// </copyright>
// <author>ShinyId3Tagger Team</author>
//-----------------------------------------------------------------------

namespace GetLyrics
{
	using System;
	using System.Collections.Generic;
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
	/// Class for LoloLyrics API
	/// </summary>
	internal class LoloLyrics : IGetLyricsService
	{
		/// <summary>
		/// Gets lyrics from LoloLyrics API
		/// http://api.lololyrics.com/
		/// </summary>
		/// <param name="client">The HTTP client which is passed on to GetResponse method</param>
		/// <param name="tagNew">The input artist and song title to search for</param>
		/// <param name="cancelToken">The cancelation token which is passed on to GetResponse method</param>
		/// <returns>The ID3 tag object with the results from this API for lyrics</returns>
		public async Task<Id3> GetLyrics(HttpMessageInvoker client, Id3 tagNew, CancellationToken cancelToken)
		{
			Id3 o = new Id3 { Service = "Lololyrics" };

			Stopwatch sw = new Stopwatch();
			sw.Start();

			// ###########################################################################
			string artistEncoded = WebUtility.UrlEncode(tagNew.Artist);
			string titleEncoded = WebUtility.UrlEncode(tagNew.Title);

			using (HttpRequestMessage searchRequest = new HttpRequestMessage())
			{
				searchRequest.RequestUri = new Uri("http://api.lololyrics.com/0.5/getLyric?artist=" + artistEncoded + "&track=" + titleEncoded + "&rawutf8=1");

				string searchContent = await Utils.GetResponse(client, searchRequest, cancelToken, suppressedStatusCodes: new[] { 404 });
				JObject searchData = Utils.DeserializeJson(Utils.ConvertXmlToJson(searchContent));

				if (searchData?.SelectToken("result.response") != null)
				{
					string rawLyrics = (string)searchData.SelectToken("result.response");

					// Sanitize lyrics
					rawLyrics = WebUtility.HtmlDecode(rawLyrics);												// URL decode lyrics
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
