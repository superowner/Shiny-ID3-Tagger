//-----------------------------------------------------------------------
// <copyright file="Itunes.cs" company="Shiny ID3 Tagger">
// Copyright (c) Shiny ID3 Tagger. All rights reserved.
// </copyright>
// <author>ShinyId3Tagger Team</author>
//-----------------------------------------------------------------------

namespace GetTags
{
	using System;
	using System.Diagnostics;
	using System.Net;
	using System.Net.Http;
	using System.Threading;
	using System.Threading.Tasks;
	using GlobalVariables;
	using Newtonsoft.Json.Linq;
	using Utils;

	/// <summary>
	/// Class for Itunes API
	/// </summary>
	internal class ITunes : IGetTagsService
	{
		/// <summary>
		/// Gets ID3 data from Itunes API
		/// https://www.apple.com/itunes/affiliates/resources/documentation/itunes-store-web-service-search-api.html
		/// </summary>
		/// <param name="client">The HTTP client which is passed on to GetResponse method</param>
		/// <param name="artist">The input artist to search for</param>
		/// <param name="title">The input song title to search for</param>
		/// <param name="cancelToken">The cancelation token which is passed on to GetResponse method</param>
		/// <returns>
		/// The ID3 tag object with the results from this API for:
		/// 		Artist
		/// 		Title
		/// 		Album
		/// 		Date
		/// 		Genre
		/// 		DiscNumber
		/// 		DiscCount
		/// 		TrackNumber
		/// 		TrackCount
		/// 		Cover URL
		/// </returns>
		public async Task<Id3> GetTags(HttpMessageInvoker client, string artist, string title, CancellationToken cancelToken)
		{
			Id3 o = new Id3 { Service = "iTunes" };

			Stopwatch sw = new Stopwatch();
			sw.Start();

			// ###########################################################################
			string searchTermEnc = WebUtility.UrlEncode(artist + " - " + title);

			using (HttpRequestMessage searchRequest = new HttpRequestMessage())
			{
				searchRequest.RequestUri = new Uri("http://itunes.apple.com/search?term=" + searchTermEnc + "&media=music&limit=1");

				string searchContent = await Utils.GetHttpResponse(client, searchRequest, cancelToken, suppressedStatusCodes: new[] { 404 });
				JObject searchData = Utils.DeserializeJson(searchContent);

				if (searchData?.SelectToken("results") != null)
				{
					o.Artist = (string)searchData.SelectToken("results[0].artistName");
					o.Title = (string)searchData.SelectToken("results[0].trackName");
					o.Album = (string)searchData.SelectToken("results[0].collectionName");
					o.Date = (string)searchData.SelectToken("results[0].releaseDate");
					o.Genre = (string)searchData.SelectToken("results[0].primaryGenreName");
					o.DiscCount = (string)searchData.SelectToken("results[0].discCount");
					o.DiscNumber = (string)searchData.SelectToken("results[0].discNumber");
					o.TrackCount = (string)searchData.SelectToken("results[0].trackCount");
					o.TrackNumber = (string)searchData.SelectToken("results[0].trackNumber");
					o.Cover = (string)searchData.SelectToken("results[0].artworkUrl100");
					if (o.Cover != null)
					{
						o.Cover = o.Cover.Replace("100x100", "600x600");
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
