//-----------------------------------------------------------------------
// <copyright file="GetTags_7digital.cs" company="Shiny Id3 Tagger">
//	 Copyright (c) Shiny Id3 Tagger. All rights reserved.
// </copyright>
// <author>ShinyId3Tagger Team</author>
// <summary>Gets ID3 data from 7Digital API for current track</summary>
// http://docs.7digital.com/#_release_details_get
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
		private async Task<Id3> GetTags_7digital(HttpMessageInvoker client, string artist, string title, CancellationToken cancelToken)
		{
			Id3 o = new Id3();
			o.Service = "7digital";
			
			Stopwatch sw = new Stopwatch();
			sw.Start();
			
			// ###########################################################################
			string artistEnc = WebUtility.UrlEncode(artist);
			string titleEnc = WebUtility.UrlEncode(title);

			HttpRequestMessage request = new HttpRequestMessage();			
			request.RequestUri = new Uri("http://api.7digital.com/1.2/track/search?q=" + artistEnc + "+" + titleEnc + "&pagesize=10&imageSize=800&oauth_consumer_key=" + User.Accounts["7dKey"]);

			string content1 = await this.GetRequest(client, request, cancelToken);
			JObject data1 = JsonConvert.DeserializeObject<JObject>(this.ConvertXmlToJson(content1), this.GetJsonSettings());

			if (data1 != null && data1.SelectToken("response.searchResults.searchResult[0].track") != null)
			{
				JToken track = (from tracks in data1.SelectTokens("response.searchResults.searchResult[*].track")
									where (string)tracks["release"]["type"] == "Album"
									select tracks).FirstOrDefault();
				
				if (track != null)
				{
					o.Artist = (string)track.SelectToken("artist.name");
					o.Title = (string)track.SelectToken("title");
					o.Album = (string)track.SelectToken("release.title");
					o.Date = (string)track.SelectToken("release.releaseDate");
					o.DiscNumber = (string)track.SelectToken("discNumber");
					o.TrackNumber = (string)track.SelectToken("number");
					o.Genre = null;
					o.Cover = (string)track.SelectToken("release.image");
					
					// ###########################################################################
					request = new HttpRequestMessage();
					string releaseId = (string)track.SelectToken("release.@id");
					request.RequestUri = new Uri("http://api.7digital.com/1.2/release/details?releaseid=" + releaseId + "&usageTypes=download,subscriptionstreaming,adsupportedstreaming&oauth_consumer_key=" + User.Accounts["7dKey"]);
					string content2 = await this.GetRequest(client, request, cancelToken);
					JObject data2 = JsonConvert.DeserializeObject<JObject>(this.ConvertXmlToJson(content2), this.GetJsonSettings());
					
					if (data2 != null && data2.SelectToken("response.release") != null)
					{
						o.TrackCount = (string)data2.SelectToken("response.release.trackCount");
						
						string tempDate = (string)data2.SelectToken("response.release.year");
						if (this.ConvertStringToDate(tempDate).Date < this.ConvertStringToDate(o.Date).Date && tempDate != null)
						{
							o.Date = tempDate;
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

// System.IO.File.WriteAllText (@"D:\response.json", content1);