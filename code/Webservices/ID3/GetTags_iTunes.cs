//-----------------------------------------------------------------------
// <copyright file="GetTags_Itunes.cs" company="Shiny Id3 Tagger">
//	 Copyright (c) Shiny Id3 Tagger. All rights reserved.
// </copyright>
// <author>ShinyId3Tagger Team</author>
// <summary>Gets ID3 data from Itunes API for current track</summary>
// https://www.apple.com/itunes/affiliates/resources/documentation/itunes-store-web-service-search-api.html
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
		private async Task<Id3> GetTags_Itunes(HttpMessageInvoker client, string artist, string title, CancellationToken cancelToken)
		{
			Id3 o = new Id3();
			o.Service = "iTunes";
			
			Stopwatch sw = new Stopwatch();
			sw.Start();
			
			// ###########################################################################
			string searchTermEnc = WebUtility.UrlEncode(artist + " - " + title);
			
			HttpRequestMessage request = new HttpRequestMessage();
			request.RequestUri = new Uri("http://itunes.apple.com/search?term=" + searchTermEnc + "&media=music&limit=1");

			string content = await this.GetResponse(client, request, cancelToken);
			JObject data = JsonConvert.DeserializeObject<JObject>(content, this.GetJsonSettings());

			if (data != null && data.SelectToken("results") != null)
			{
				o.Artist = (string)data.SelectToken("results[0].artistName");
				o.Title = (string)data.SelectToken("results[0].trackName");
				o.Album = (string)data.SelectToken("results[0].collectionName");
				o.Date = (string)data.SelectToken("results[0].releaseDate");
				o.Genre = (string)data.SelectToken("results[0].primaryGenreName");
				o.DiscCount = (string)data.SelectToken("results[0].discCount");
				o.DiscNumber = (string)data.SelectToken("results[0].discNumber");
				o.TrackCount = (string)data.SelectToken("results[0].trackCount");
				o.TrackNumber = (string)data.SelectToken("results[0].trackNumber");
				o.Cover = (string)data.SelectToken("results[0].artworkUrl100");
				if (o.Cover != null)
				{
					o.Cover = o.Cover.Replace("100x100", "600x600");
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