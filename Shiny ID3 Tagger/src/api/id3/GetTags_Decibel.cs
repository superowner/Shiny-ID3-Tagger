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

namespace GlobalNamespace
{
	using System;
	using System.Data;
	using System.Diagnostics;
	using System.Linq;
	using System.Net;
	using System.Net.Http;
	using System.Threading;
	using System.Threading.Tasks;
	using Newtonsoft.Json.Linq;

	public partial class Form1
	{
		private async Task<Id3> GetTags_Decibel(HttpMessageInvoker client, string artist, string title, CancellationToken cancelToken)
		{
			Id3 o = new Id3();
			o.Service = "Decibel (Quantone Music)";

			Stopwatch sw = new Stopwatch();
			sw.Start();

			// ###########################################################################
			var account = (from item in User.Accounts["Decibel"]
						   orderby item["lastused"] ascending
						   select item).FirstOrDefault();
			account["lastUsed"] = DateTime.Now.Ticks;

			string artistEncoded = WebUtility.UrlEncode(artist);
			string titleEncoded = WebUtility.UrlEncode(title);

			using (HttpRequestMessage searchRequest = new HttpRequestMessage())
			{
				searchRequest.RequestUri = new Uri("https://data.quantonemusic.com/v3/Recordings?artists=" + artistEncoded + "&title=" + titleEncoded + "&depth=genres&PageSize=1&PageNumber=1");
				searchRequest.Headers.Add("DecibelAppID", (string)account["AppId"]);
				searchRequest.Headers.Add("DecibelAppKey", (string)account["AppKey"]);

				string searchContent = await this.GetResponse(client, searchRequest, cancelToken);
				JObject searchData = this.DeserializeJson(searchContent);

				if (searchData != null && searchData.SelectToken("Results") != null && searchData.SelectToken("Results").ToString() != "[]")
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
						albumRequest.RequestUri = new Uri("https://data.quantonemusic.com/v3/Albums?artists=" + artistEncoded + "&recordings=" + titleEncoded + "&depth=Genres,Recordings&PageSize=1&PageNumber=1");
						albumRequest.Headers.Add("DecibelAppID", (string)account["AppId"]);
						albumRequest.Headers.Add("DecibelAppKey", (string)account["AppKey"]);

						string albumContent = await this.GetResponse(client, albumRequest, cancelToken);
						JObject albumData = this.DeserializeJson(albumContent);

						if (albumData != null && albumData.SelectToken("Results") != null && albumData.SelectToken("Results").ToString() != "[]")
						{
							o.Genre = (string)albumData.SelectToken("Results[0].Genres[0].Name");
							o.TrackCount = (string)albumData.SelectToken("Results[0].Recordings[-1:].AlbumSequence");
							o.Cover = "https://data.quantonemusic.com/v3/Images/" + (string)albumData.SelectToken("Results[0].ImageId"); // don't include in settings.json CoverOrder

							foreach (JToken recording in albumData.SelectTokens("Results[0].Recordings[*]"))
							{
								if (((string)recording.SelectToken("Title")).ToLowerInvariant() == o.Title.ToLowerInvariant())
								{
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
			o.Duration = string.Format("{0:s\\,f}", sw.Elapsed);

			return o;
		}
	}
}
