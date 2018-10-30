//-----------------------------------------------------------------------
// <copyright file="Gracenote.cs" company="Shiny ID3 Tagger">
// Copyright (c) Shiny ID3 Tagger. All rights reserved.
// </copyright>
// <author>ShinyId3Tagger Team</author>
//-----------------------------------------------------------------------

namespace GetTags
{
	using System;
	using System.Diagnostics;
	using System.Linq;
	using System.Net.Http;
	using System.Text.RegularExpressions;
	using System.Threading;
	using System.Threading.Tasks;
	using GlobalVariables;
	using Newtonsoft.Json.Linq;
	using Utils;

	/// <summary>
	/// Class for Gracenote API
	/// </summary>
	internal class Gracenote : IGetTagsService
	{
		/// <summary>
		/// Gets ID3 data from Gracenote API
		/// https://developer.gracenote.com/web-api
		/// https://developer.gracenote.com/sites/default/files/web/html/index.html#PDFs/Music-Web-API-Developers-Guide-o.pdf
		/// https://developer.gracenote.com/sites/default/files/web/webapi/index.html#music-web-api/Registering%20a%20Device.html#scroll-bookmark-25
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
			Id3 o = new Id3 { Service = "Gracenote (Sony)" };

			Stopwatch sw = new Stopwatch();
			sw.Start();

			// ###########################################################################
			string artistTemp = Regex.Replace(artist, "&(?!(amp|apos|quot|lt|gt);)", "&amp;");
			string titleTemp = Regex.Replace(title, "&(?!(amp|apos|quot|lt|gt);)", "&amp;");

			using (HttpRequestMessage searchRequest = new HttpRequestMessage())
			{
				searchRequest.Method = HttpMethod.Post;
				searchRequest.RequestUri = new Uri("https://c123.web.cddbp.net/webapi/xml/1.0/");
				searchRequest.Content = new StringContent(@"
					<QUERIES>
						<AUTH>
							<CLIENT>" + User.Accounts["Gracenote"]["ClientId"] + @"</CLIENT>
							<USER>" + User.Accounts["Gracenote"]["UserId"] + @"</USER>
						</AUTH>
						<LANG>eng</LANG>
						<QUERY CMD=""ALBUM_SEARCH"">
							<TEXT TYPE=""ARTIST"">" + artistTemp + @"</TEXT>
							<TEXT TYPE=""TRACK_TITLE"">" + titleTemp + @"</TEXT>
							<RANGE>
								<START>1</START>
								<END>2</END>
							</RANGE>
						</QUERY>
					</QUERIES>");

				string searchContent = await Utils.GetResponse(client, searchRequest, cancelToken);
				JObject searchData = Utils.DeserializeJson(Utils.ConvertXmlToJson(searchContent));

				if (searchData?.SelectToken("RESPONSES.RESPONSE.ALBUM") != null)
				{
					JToken album = searchData.SelectToken("RESPONSES.RESPONSE.ALBUM");

					// album can be an array of objects (=multiple albums returned) or just an object (=just one album returned)
					if (album is JArray)
					{
						// Sort all albums by date, take the oldest one
						JToken oldestAlbum = (from albumItem in album
												where albumItem.SelectToken("DATE") != null
												orderby Utils.ConvertStringToDate((string)albumItem["DATE"]) ascending
												select albumItem).FirstOrDefault();

						// If none of the albums have a "DATE" value, then oldestAlbum will be null. Take the first album then
						album = oldestAlbum ?? album.FirstOrDefault();
					}

					o.Artist = (string)album.SelectToken("ARTIST");
					o.Title = (string)album.SelectToken("TITLE");
					o.Album = (string)album.SelectToken("TITLE");
					o.Date = (string)album.SelectToken("DATE");
					o.Genre = (string)album.SelectToken("GENRE.#text");
					o.DiscCount = null;
					o.DiscNumber = null;
					o.TrackCount = (string)album.SelectToken("TRACK_COUNT");
					o.TrackNumber = (string)album.SelectToken("MATCHED_TRACK_NUM");

					// Additional query just for the cover (cover like you only get a cover when using SINGLE_BEST or SINGLE_BEST_COVER or ALBUM_FETCH)
					if (album.SelectToken("GN_ID") != null)
					{
						using (HttpRequestMessage albumRequest = new HttpRequestMessage())
						{
							albumRequest.Method = HttpMethod.Post;
							albumRequest.RequestUri = new Uri("https://c123.web.cddbp.net/webapi/xml/1.0/");
							albumRequest.Content = new StringContent(@"
								<QUERIES>
									<AUTH>
										<CLIENT>" + User.Accounts["Gracenote"]["ClientId"] + @"</CLIENT>
										<USER>" + User.Accounts["Gracenote"]["UserId"] + @"</USER>
									</AUTH>
									<LANG>eng</LANG>
									<QUERY CMD=""ALBUM_FETCH"">
										<GN_ID>" + album.SelectToken("GN_ID") + @"</GN_ID>
										<OPTION>
											<PARAMETER>SELECT_EXTENDED</PARAMETER>
											<VALUE>COVER</VALUE>
										</OPTION>
										<OPTION>
											<PARAMETER>COVER_SIZE</PARAMETER>
											<VALUE>XLARGE,LARGE,MEDIUM,SMALL</VALUE>
										</OPTION>
									</QUERY>
								</QUERIES>");

							string albumContent = await Utils.GetResponse(client, albumRequest, cancelToken);
							JObject albumData = Utils.DeserializeJson(Utils.ConvertXmlToJson(albumContent));

							if (albumData != null)
							{
								o.Cover = (string)albumData.SelectToken("RESPONSES.RESPONSE.ALBUM.URL.#text");
							}
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
