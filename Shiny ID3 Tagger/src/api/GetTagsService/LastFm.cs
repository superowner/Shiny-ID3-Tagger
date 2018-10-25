//-----------------------------------------------------------------------
// <copyright file="LastFm.cs" company="Shiny ID3 Tagger">
// Copyright (c) Shiny ID3 Tagger. All rights reserved.
// </copyright>
// <author>ShinyId3Tagger Team</author>
// <summary>Gets ID3 data from Last.fm API for current track</summary>
// http://www.last.fm/api/rest
// http://www.last.fm/api/show/track.getInfo
// limit=1 not available for track.getInfo or album.getInfo method
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

	internal class LastFm : IGetTagsService
	{
		public async Task<Id3> GetTags(HttpMessageInvoker client, string artist, string title, CancellationToken cancelToken)
		{
			Id3 o = new Id3 { Service = "Last.fm" };

			Stopwatch sw = new Stopwatch();
			sw.Start();

			// ###########################################################################
			string artistEncoded = WebUtility.UrlEncode(artist);
			string titleEncoded = WebUtility.UrlEncode(title);

			using (HttpRequestMessage searchRequest = new HttpRequestMessage())
			{
				searchRequest.Method = HttpMethod.Post;
				searchRequest.RequestUri = new Uri("http://ws.audioscrobbler.com/2.0/");
				searchRequest.Headers.ExpectContinue = false;
				searchRequest.Content = new StringContent("method=track.getInfo&artist=" + artistEncoded + "&track=" + titleEncoded +
					"&api_key=" + User.Accounts["Lastfm"]["ApiKey"] + "&format=json&autocorrect=1");

				string searchContent = await Utils.GetResponse(client, searchRequest, cancelToken);
				JObject searchData = Utils.DeserializeJson(searchContent);

				if (searchData?.SelectToken("track") != null)
				{
					o.Artist = (string)searchData.SelectToken("track.artist.name");
					o.Title = (string)searchData.SelectToken("track.name");
					o.Album = (string)searchData.SelectToken("track.album.title");
					o.Genre = (string)searchData.SelectToken("track.toptags.tag[0].name");
					o.TrackNumber = (string)searchData.SelectToken("track.album.@attr.position");

					// ###########################################################################
					string albumId = (string)searchData.SelectToken("track.album.mbid");

					using (HttpRequestMessage albumRequest = new HttpRequestMessage())
					{
						albumRequest.Method = HttpMethod.Post;
						albumRequest.RequestUri = new Uri("http://ws.audioscrobbler.com/2.0/");
						albumRequest.Headers.ExpectContinue = false;
						albumRequest.Content = new StringContent("method=album.getInfo&mbid=" + albumId + "&api_key=" + User.Accounts["Lastfm"]["ApiKey"] + "&format=json");

						string albumContent = await Utils.GetResponse(client, albumRequest, cancelToken);
						JObject albumData = Utils.DeserializeJson(albumContent);

						if (albumData?.SelectToken("album") != null)
						{
							o.Date = null;  // "releasedate" property is broken on Lastfm's site. They eventually fix this in 2018 with a new API (https://getsatisfaction.com/lastfm/topics/album-getinfo-is-missing-releasedate)
							o.DiscCount = null;
							o.DiscNumber = null;
							o.TrackCount = (string)albumData.SelectToken("album.tracks.track[-1:].@attr.rank");
							o.Cover = (string)albumData.SelectToken("album.image[-1:].#text");

							if (o.Cover != null)
							{
								o.Cover = o.Cover.Replace("/i/u/300x300/", "/i/u/600x600/");		// Largest version on lastfm image servers seems to be 600x600 pixel
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
