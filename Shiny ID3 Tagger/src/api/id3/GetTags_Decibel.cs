//-----------------------------------------------------------------------
// <copyright file="GetTags_Decibel.cs" company="Shiny ID3 Tagger">
// Copyright (c) Shiny ID3 Tagger. All rights reserved.
// </copyright>
// <author>ShinyId3Tagger Team</author>
// <summary>Gets ID3 data from Decibel API for current track</summary>
// https://developer.quantonemusic.com/authentication-v3
// https://developer.quantonemusic.com/rest-api-v3#classQueryAlbums
// https://developer.quantonemusic.com/object-documentation
// titleSearchType=PartialName has poorer results than without
//-----------------------------------------------------------------------

namespace GetTags
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

	public class Decibel : IGetTagsService
	{
		public const string ServiceName = "Decibel (Quantone Music)";

		public async Task<Id3> GetTags(HttpMessageInvoker client, string artist, string title, CancellationToken cancelToken)
		{
			Id3 o = new Id3 {Service = ServiceName};

			Stopwatch sw = new Stopwatch();
			sw.Start();

			// ###########################################################################
			var account = (from item in User.Accounts["Decibel"]
						   orderby item["lastused"] ascending
						   select item).FirstOrDefault();
			if (account != null)
			{
				account["lastUsed"] = DateTime.Now.Ticks;

				string artistEncoded = WebUtility.UrlEncode(artist);
				string titleEncoded = WebUtility.UrlEncode(title);

				using (HttpRequestMessage searchRequest = new HttpRequestMessage())
				{
					searchRequest.RequestUri = new Uri("https://data.quantonemusic.com/v3/Recordings?artists=" +
					                                   artistEncoded + "&title=" + titleEncoded +
					                                   "&depth=genres&PageSize=1&PageNumber=1");
					searchRequest.Headers.Add("DecibelAppID", (string)account["AppId"]);
					searchRequest.Headers.Add("DecibelAppKey", (string)account["AppKey"]);

					string searchContent = await Utils.GetResponse(client, searchRequest, cancelToken);
					JObject searchData = Utils.DeserializeJson(searchContent);

					if (searchData?.SelectToken("Results") != null && searchData.SelectToken("Results").ToString() != "[]")
					{
						o.Artist = (string)searchData.SelectToken("Results[0].MainArtistsLiteral");
						o.Title = (string)searchData.SelectToken("Results[0].Title");
						o.Album = (string)searchData.SelectToken("Results[0].OriginalAlbumTitle");
						o.Date = (string)searchData.SelectToken("Results[0].OriginalReleaseDate");
						o.DiscCount = null;
						o.DiscNumber = null;

						// ###########################################################################
						using (HttpRequestMessage albumRequest = new HttpRequestMessage())
						{
							albumRequest.RequestUri =
								new Uri("https://data.quantonemusic.com/v3/Albums?artists=" + artistEncoded +
								        "&recordings=" + titleEncoded +
								        "&depth=Genres,Recordings&PageSize=1&PageNumber=1");
							albumRequest.Headers.Add("DecibelAppID", (string) account["AppId"]);
							albumRequest.Headers.Add("DecibelAppKey", (string) account["AppKey"]);

							string albumContent = await Utils.GetResponse(client, albumRequest, cancelToken);
							JObject albumData = Utils.DeserializeJson(albumContent);

							if (albumData?.SelectToken("Results") != null && albumData.SelectToken("Results").ToString() != "[]")
							{
								o.Genre = (string)albumData.SelectToken("Results[0].Genres[0].Name");
								o.TrackCount =
									(string)albumData.SelectToken("Results[0].Recordings[-1:].AlbumSequence");
								o.Cover = "https://data.quantonemusic.com/v3/Images/" +
								          (string)albumData.SelectToken(
									          "Results[0].ImageId"); // don't include in settings.json CoverOrder

								foreach (JToken recording in albumData.SelectTokens("Results[0].Recordings[*]"))
								{
									if (!string.Equals((string)recording.SelectToken("Title"), o.Title, StringComparison.InvariantCultureIgnoreCase))
									{
										continue;
									}
									o.TrackNumber = (string)recording["AlbumSequence"];
									break;
								}
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
