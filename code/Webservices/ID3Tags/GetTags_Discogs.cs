//-----------------------------------------------------------------------
// <copyright file="GetTags_Discogs.cs" company="Shiny Id3 Tagger">
//	 Copyright (c) Shiny Id3 Tagger. All rights reserved.
// </copyright>
// <author>ShinyId3Tagger Team</author>
// <summary>Gets ID3 data from Discogs API for current track</summary>
// https://www.discogs.com/developers/#page:database
// http://www.onemusicapi.com/blog/2013/06/12/better-discogs-searching/
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
		private async Task<Id3> GetTags_Discogs(HttpMessageInvoker client, string artist, string title, CancellationToken cancelToken)
		{			
			Id3 o = new Id3();
			o.Service = "Discogs";
			
			Stopwatch sw = new Stopwatch();
			sw.Start();			

			// ###########################################################################
			string artistTemp = string.Join("* ", artist.Split(' ')) + "*";
			string titleTemp = string.Join("* ", title.Split(' ')) + "*";
			string searchTermEnc = WebUtility.UrlEncode(artistTemp + " " + titleTemp);
			
			HttpRequestMessage request = new HttpRequestMessage();
			request.Headers.Add("User-Agent", User.Settings["UserAgent"]);
			request.RequestUri = new Uri("https://api.discogs.com/database/search?q=" + searchTermEnc +
				"&format=album" +
				"&type=master" + 
				"&per_page=1" +
				"&key=" + User.Accounts["DcKey"] +
				"&secret=" + User.Accounts["DcSecret"]);
			
			string content1 = await this.GetRequest(client, request, cancelToken);
			JObject data1 = JsonConvert.DeserializeObject<JObject>(content1, this.GetJsonSettings());
			
			if (data1 != null && data1.SelectToken("results").Any())
			{
				request = new HttpRequestMessage();				
				request.Headers.Add("User-Agent", User.Settings["UserAgent"]);
			
				string url = (string)data1.SelectToken("results[0].resource_url") + "?key=" + User.Accounts["DcKey"] + "&secret=" + User.Accounts["DcSecret"];
				if (IsValidUrl(url))
				{
					request.RequestUri = new Uri(url);
				}

				string content2 = await this.GetRequest(client, request, cancelToken);
				JObject data2 = JsonConvert.DeserializeObject<JObject>(content2, this.GetJsonSettings());

				if (data2 != null)
				{
					o.Artist = (string)data2.SelectToken("artists[0].name");
					o.Title = null;
					o.Album = (string)data2.SelectToken("title");
					o.Date = (string)data2.SelectToken("year");
					o.Genre = (string)data2.SelectToken("genres[0]");
					o.DiscCount = null;
					o.DiscNumber = null;
					o.Cover = (string)data2.SelectToken("images[0].uri");
	
					o.TrackCount = data2.SelectTokens("tracklist[*]")
						.Where(t => t.SelectToken("type_").ToString() == "track")
						.ToArray().Length.ToString();
					
					// This API is strange. You can either search for a release and don't get the album. Or you search for the album but don't get the release			
					// You can do one search with "format=album" to maybe get the album and a second search without "format=album" to maybe get the single and therefore the track title
					// But no one will guarantee that the second search shows a title which is on your album from the first search
					// How can I get a response which holds the album and title at the same time?
					// Currently I'm just checking if the album track list contains a title which equals the initial title (from filename or ID3 tag)
					JToken[] tracklist = data2.SelectTokens("tracklist[*].title").ToArray();
					int temp = Array.FindIndex(tracklist, t => t.ToString().Equals(title, StringComparison.InvariantCultureIgnoreCase));
					if (temp != -1)
					{
						o.Title = title;
						o.TrackNumber = (temp + 1).ToString();
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