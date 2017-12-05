//-----------------------------------------------------------------------
// <copyright file="GetTags_LastFm.cs" company="Shiny Id3 Tagger">
//	 Copyright (c) Shiny Id3 Tagger. All rights reserved.
// </copyright>
// <author>ShinyId3Tagger Team</author>
// <summary>Gets ID3 data from Last.fm API for current track</summary>
// http://www.last.fm/api/rest
// http://www.last.fm/api/show/track.getInfo
// limit=1 not available for track.getInfo or album.getInfo method
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
		private async Task<Id3> GetTags_LastFm(HttpMessageInvoker client, string artist, string title, CancellationToken cancelToken)
		{
			Id3 o = new Id3();
			o.Service = "Last.fm";

			Stopwatch sw = new Stopwatch();
			sw.Start();

			// ###########################################################################
			string artistEncoded = WebUtility.UrlEncode(artist);
			string titleEncoded = WebUtility.UrlEncode(title);

			HttpRequestMessage request = new HttpRequestMessage(HttpMethod.Post, "http://ws.audioscrobbler.com/2.0/");
			request.Headers.ExpectContinue = false;
			request.Content = new StringContent("api_key=" + User.Accounts["LaApiKey"] +
				"&format=json&method=track.getInfo&artist=" + artistEncoded + "&track=" + titleEncoded + "&autocorrect=1");

			string content1 = await this.GetResponse(client, request, cancelToken);
			JObject data1 = JsonConvert.DeserializeObject<JObject>(content1, this.GetJsonSettings());

			if (data1 != null && data1.SelectToken("track") != null)
			{
				o.Artist = (string)data1.SelectToken("track.artist.name");
				o.Title = (string)data1.SelectToken("track.name");
				o.Album = (string)data1.SelectToken("track.album.title");
				o.Genre = (string)data1.SelectToken("track.toptags.tag[0].name");
				o.TrackNumber = (string)data1.SelectToken("track.album.@attr.position");

				// ###########################################################################
				string albumid = (string)data1.SelectToken("track.album.mbid");

				request = new HttpRequestMessage(HttpMethod.Post, "http://ws.audioscrobbler.com/2.0/");
				request.Headers.ExpectContinue = false;
				request.Content = new StringContent("api_key=" + User.Accounts["LaApiKey"] +
					"&format=json&method=album.getInfo&mbid=" + albumid);

				string content2 = await this.GetResponse(client, request, cancelToken);
				JObject data2 = JsonConvert.DeserializeObject<JObject>(content2, this.GetJsonSettings());

				if (data2 != null && data2.SelectToken("album") != null)
				{
					o.Date = null;	// "releasedate" property is broken on lastfm's site. They said they eventually fix this in 2018 with a new API (https://getsatisfaction.com/lastfm/topics/album-getinfo-is-missing-releasedate)
					o.DiscCount = null;
					o.DiscNumber = null;
					o.TrackCount = (string)data2.SelectToken("album.tracks.track[-1:].@attr.rank");
					o.Cover = (string)data2.SelectToken("album.image[-1:].#text");

					if (o.Cover != null)
					{
						o.Cover = o.Cover.Replace("/i/u/300x300/", "/i/u/600x600/");		// Largest version on lastfm image servers seems to be 600x600 px
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