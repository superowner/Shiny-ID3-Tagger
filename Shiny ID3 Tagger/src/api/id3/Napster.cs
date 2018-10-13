//-----------------------------------------------------------------------
// <copyright file="Napster.cs" company="Shiny ID3 Tagger">
// Copyright (c) Shiny ID3 Tagger. All rights reserved.
// </copyright>
// <author>ShinyId3Tagger Team</author>
// <summary>Gets ID3 data from Napster API for current track</summary>
// https://developer.rhapsody.com/api#search
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

	internal class Napster : IGetTagsService
	{
		public async Task<Id3> GetTags(HttpMessageInvoker client, string artist, string title, CancellationToken cancelToken)
		{
			Id3 o = new Id3 {Service = "Napster (Rhapsody)" };

			Stopwatch sw = new Stopwatch();
			sw.Start();

			// ###########################################################################
			string searchTermEnc = WebUtility.UrlEncode(artist + " - " + title);

			using (HttpRequestMessage searchRequest = new HttpRequestMessage())
			{
				searchRequest.RequestUri = new Uri("http://api.rhapsody.com/v2/search?q=" + searchTermEnc + "&include=genres&type=track&limit=1&apikey=" + User.Accounts["Napster"]["ApiKey"]);

				string searchContent = await Utils.GetResponse(client, searchRequest, cancelToken);
				JObject searchData = Utils.DeserializeJson(searchContent);

				if (searchData != null)
				{
					o.Artist = (string)searchData.SelectToken("data[0].artistName");
					o.Title = (string)searchData.SelectToken("data[0].name");
					o.Album = (string)searchData.SelectToken("data[0].albumName");
					o.TrackNumber = (string)searchData.SelectToken("data[0].index");
					o.DiscNumber = (string)searchData.SelectToken("data[0].disc");
					o.Genre = (string)searchData.SelectToken("data[0].linked.genres[0].name");

					string albumId = (string)searchData.SelectToken("data[0].albumId");

					if (albumId != null)
					{
						o.Cover = "http://direct.rhapsody.com/imageserver/v2/albums/" + albumId + "/images/500x500.jpg";

						using (HttpRequestMessage albumRequest = new HttpRequestMessage())
						{
							albumRequest.RequestUri = new Uri("http://api.rhapsody.com/v2/albums/" + albumId + "?apikey=" + User.Accounts["Napster"]["ApiKey"]);

							string albumContent = await Utils.GetResponse(client, albumRequest, cancelToken);
							JObject albumData = Utils.DeserializeJson(albumContent);

							if (albumData != null)
							{
								o.Date = (string)albumData.SelectToken("albums[0].released");
								o.DiscCount = (string)albumData.SelectToken("albums[0].discCount");
								o.TrackCount = albumData.SelectToken("albums[0].trackCount").ToString();
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
