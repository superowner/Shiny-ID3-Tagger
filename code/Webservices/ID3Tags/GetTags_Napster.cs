//-----------------------------------------------------------------------
// <copyright file="GetTags_Napster.cs" company="Shiny Id3 Tagger">
//	 Copyright (c) Shiny Id3 Tagger. All rights reserved.
// </copyright>
// <author>ShinyId3Tagger Team</author>
// <summary>Gets ID3 data from Napster API for current track</summary>
// https://developer.rhapsody.com/api#search
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
		private async Task<Id3> GetTags_Napster(HttpMessageInvoker client, string artist, string title, CancellationToken cancelToken)
		{
			Id3 o = new Id3();
			o.Service = "Napster (Rhapsody)";
			
			Stopwatch sw = new Stopwatch();
			sw.Start();
			
			// ###########################################################################
			string searchTermEnc = WebUtility.UrlEncode(artist + " - " + title);
			
			HttpRequestMessage request = new HttpRequestMessage();
			request = new HttpRequestMessage();
			request.RequestUri = new Uri("https://api.rhapsody.com/v2/search?q=" + searchTermEnc + "&include=genres&type=track&pretty=true&limit=1&apikey=" + User.Accounts["NaApiKey"]);

			string content1 = await this.GetRequest(client, request, cancelToken);
			JObject data1 = JsonConvert.DeserializeObject<JObject>(content1, this.GetJsonSettings());			

			if (data1 != null)
			{
				o.Artist = (string)data1.SelectToken("data[0].artistName");
				o.Title = (string)data1.SelectToken("data[0].name");
				o.Album = (string)data1.SelectToken("data[0].albumName");
				o.TrackNumber = (string)data1.SelectToken("data[0].index");
				o.DiscNumber = (string)data1.SelectToken("data[0].disc");
				o.Genre = (string)data1.SelectToken("data[0].linked.genres.ids[0]");

				if (data1.SelectToken("data[0].albumId") != null)
				{
					request = new HttpRequestMessage();
					request.RequestUri = new Uri("https://api.rhapsody.com/v2/albums/" + (string)data1.SelectToken("data[0].albumId") + "?pretty=true&apikey=" + User.Accounts["NaApiKey"]);
	
					string content2 = await this.GetRequest(client, request, cancelToken);
					JObject data2 = JsonConvert.DeserializeObject<JObject>(content2, this.GetJsonSettings());
	
					if (data2 != null)
					{
						o.Date = (string)data2.SelectToken("albums[0].released");
						o.DiscCount = (string)data2.SelectToken("albums[0].discCount");
						o.TrackCount = data2.SelectToken("albums[0].trackCount").ToString();
						o.Cover = "http://direct.rhapsody.com/imageserver/v2/albums/" + (string)data1.SelectToken("data[0].albumId") + "/images/500x500.jpg";		
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