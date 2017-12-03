//-----------------------------------------------------------------------
// <copyright file="GetTags_Decibel.cs" company="Shiny Id3 Tagger">
//	 Copyright (c) Shiny Id3 Tagger. All rights reserved.
// </copyright>
// <author>ShinyId3Tagger Team</author>
// <summary>Gets ID3 data from Decibel API for current track</summary>
// https://developer.decibel.net/authentication-v3
// https://developer.decibel.net/rest-api-v3#classQueryAlbums
// https://developer.decibel.net/object-documentation
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
	using Newtonsoft.Json;
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
			DataRow[] account = User.DbAccounts.Select("lastused = MIN(lastused)");
			User.DbAccounts.Select("lastused = MIN(lastused)")[0]["lastused"] = DateTime.Now.Ticks;

			string artistEncoded = WebUtility.UrlEncode(artist);
			string titleEncoded = WebUtility.UrlEncode(title);

			HttpRequestMessage request = new HttpRequestMessage();
			request.RequestUri = new Uri("https://data.quantonemusic.com/v3/Recordings?artists=" + artistEncoded + "&title=" + titleEncoded + "&depth=genres&PageSize=1&PageNumber=1");
			request.Headers.Add("DecibelAppID", (string)account[0]["id"]);
			request.Headers.Add("DecibelAppKey", (string)account[0]["key"]);

			string content1 = await this.GetResponse(client, request, cancelToken);
			JObject data1 = JsonConvert.DeserializeObject<JObject>(content1, this.GetJsonSettings());

			if (data1 != null && data1.SelectToken("Results") != null && data1.SelectToken("Results").ToString() != "[]")
			{
				o.Artist = (string)data1.SelectToken("Results[0].MainArtistsLiteral");
				o.Title = (string)data1.SelectToken("Results[0].Title");
				o.Album = (string)data1.SelectToken("Results[0].OriginalAlbumTitle");
				o.Date = (string)data1.SelectToken("Results[0].OriginalReleaseDate");
				o.DiscCount = null;
				o.DiscNumber = null;

				// ###########################################################################
				request = new HttpRequestMessage();
				request.RequestUri = new Uri("https://data.quantonemusic.com/v3/Albums?artists=" + artistEncoded + "&recordings=" + titleEncoded + "&depth=Genres,Recordings&PageSize=1&PageNumber=1");
				request.Headers.Add("DecibelAppID", (string)account[0]["id"]);
				request.Headers.Add("DecibelAppKey", (string)account[0]["key"]);

				string content2 = await this.GetResponse(client, request, cancelToken);
				JObject data2 = JsonConvert.DeserializeObject<JObject>(content2, this.GetJsonSettings());

				if (data2 != null && data2.SelectToken("Results") != null && data2.SelectToken("Results").ToString() != "[]")
				{
					o.Genre = (string)data2.SelectToken("Results[0].Genres[0].Name");
					o.TrackCount = (string)data2.SelectToken("Results[0].Recordings[-1:].AlbumSequence");
					o.Cover = "https://data.quantonemusic.com/v3/Images/" + (string)data2.SelectToken("Results[0].ImageId"); // don't include in settings.json CoverOrder

					foreach (JToken recording in data2.SelectTokens("Results[0].Recordings[*]"))
					{
						if (((string)recording.SelectToken("Title")).ToLower() == o.Title.ToLower())
						{
							o.TrackNumber = (string)recording["AlbumSequence"];
							break;
						}
					}
				}
			}

			// ###########################################################################
			sw.Stop();
			o.Duration = string.Format("{0:s\\,f}", sw.Elapsed);

			request.Dispose();
			return o;
		}
	}
}

// System.IO.File.WriteAllText (@"D:\response.json", content2);