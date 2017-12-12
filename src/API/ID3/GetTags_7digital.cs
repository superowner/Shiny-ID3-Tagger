//-----------------------------------------------------------------------
// <copyright file="GetTags_7digital.cs" company="Shiny Id3 Tagger">
// Copyright (c) Shiny Id3 Tagger. All rights reserved.
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
			string artistEncoded = WebUtility.UrlEncode(artist);
			string titleEncoded = WebUtility.UrlEncode(title);

			// you can switch from XML to JSON by setting "application/json" as accept header.
			// But strangely 7digital results are then different and worse than the XML ones
			using (HttpRequestMessage searchRequest = new HttpRequestMessage())
			{
				searchRequest.RequestUri = new Uri("http://api.7digital.com/1.2/track/search?q=" + artistEncoded + "+" + titleEncoded + "&pagesize=10&imageSize=800&usageTypes=download&oauth_consumer_key=" + User.Accounts["7dKey"]);

				string searchContent = await this.GetResponse(client, searchRequest, cancelToken);
				JObject searchData = this.DeserializeJson(this.ConvertXmlToJson(searchContent));

				if (searchData != null && searchData.SelectToken("response.searchResults.searchResult[0].track") != null)
				{
					JToken track = (from tracks in searchData.SelectTokens("response.searchResults.searchResult[*].track")
									where tracks["release"]["type"].ToString().ToLowerInvariant() == "album"
									select tracks).FirstOrDefault();

					if (track != null)
					{
						o.Artist = (string)track.SelectToken("artist.name");
						o.Title = (string)track.SelectToken("title");
						o.Album = (string)track.SelectToken("release.title");
						o.Date = (string)track.SelectToken("download.releaseDate");
						o.DiscNumber = (string)track.SelectToken("discNumber");
						o.TrackNumber = (string)track.SelectToken("number");
						o.Cover = (string)track.SelectToken("release.image");

						string releaseId = (string)track.SelectToken("release.@id");

						// ###########################################################################
						using (HttpRequestMessage releaseRequest = new HttpRequestMessage())
						{
							releaseRequest.RequestUri = new Uri("http://api.7digital.com/1.2/release/details?releaseid=" + releaseId + "&usageTypes=download&oauth_consumer_key=" + User.Accounts["7dKey"]);

							string releaseContent = await this.GetResponse(client, releaseRequest, cancelToken);
							JObject releaseData = this.DeserializeJson(this.ConvertXmlToJson(releaseContent));

							if (releaseData != null && releaseData.SelectToken("response.release") != null)
							{
								o.TrackCount = (string)releaseData.SelectToken("response.release.trackCount");
							}
						}

						// ###########################################################################
						using (HttpRequestMessage tagsRequest = new HttpRequestMessage())
						{
							tagsRequest.RequestUri = new Uri("http://api.7digital.com/1.2/release/tags?releaseid=" + releaseId + "&country=US&oauth_consumer_key=" + User.Accounts["7dKey"]);

							string tagsContent = await this.GetResponse(client, tagsRequest, cancelToken);
							JObject tagsData = this.DeserializeJson(this.ConvertXmlToJson(tagsContent));

							if (tagsData != null && tagsData.SelectToken("response.tags.tag") != null)
							{
								if (tagsData.SelectToken("response.tags.tag").Type == JTokenType.Array)
								{
									o.Genre = (string)tagsData.SelectToken("response.tags.tag[0].text");
								}
								else
								{
									o.Genre = (string)tagsData.SelectToken("response.tags.tag.text");
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
