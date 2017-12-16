//-----------------------------------------------------------------------
// <copyright file="GetTags_Qobuz.cs" company="Shiny ID3 Tagger">
// Copyright (c) Shiny ID3 Tagger. All rights reserved.
// </copyright>
// <author>ShinyId3Tagger Team</author>
// <summary>Gets ID3 data from Qobuz API for current track</summary>
// http://www.qobuz.com/account/profile
// https://github.com/Qobuz/api-documentation
// https://github.com/Qobuz/api-documentation/blob/master/endpoints/track/search.md
// No English genre names possible. API returns French genre names
//-----------------------------------------------------------------------

namespace GlobalNamespace
{
	using System;
	using System.Diagnostics;
	using System.Net;
	using System.Net.Http;
	using System.Threading;
	using System.Threading.Tasks;
	using Newtonsoft.Json.Linq;

	public partial class Form1
	{
		private async Task<Id3> GetTags_Qobuz(HttpMessageInvoker client, string artist, string title, CancellationToken cancelToken)
		{
			Id3 o = new Id3();
			o.Service = "Qobuz";

			Stopwatch sw = new Stopwatch();
			sw.Start();

			// ###########################################################################
			string searchTermEnc = WebUtility.UrlEncode(artist + " - " + title);

			using (HttpRequestMessage searchRequest = new HttpRequestMessage())
			{
				searchRequest.RequestUri = new Uri("http://www.qobuz.com/api.json/0.2/track/search?limit=1&app_id=" + User.Accounts["Qobuz"]["AppId"] + "&query=" + searchTermEnc);

				string searchContent = await this.GetResponse(client, searchRequest, cancelToken);
				JObject searchData = this.DeserializeJson(searchContent);

				if (searchData != null && searchData.SelectToken("tracks.items[0]") != null)
				{
					o.Artist = (string)searchData.SelectToken("tracks.items[0].album.artist.name");
					o.Title = (string)searchData.SelectToken("tracks.items[0].title");
					o.Album = (string)searchData.SelectToken("tracks.items[0].album.title");
					o.Genre = (string)searchData.SelectToken("tracks.items[0].album.genre.name");
					o.DiscCount = (string)searchData.SelectToken("tracks.items[0].album.media_count");
					o.DiscNumber = (string)searchData.SelectToken("tracks.items[0].media_number");
					o.TrackCount = (string)searchData.SelectToken("tracks.items[0].album.tracks_count");
					o.TrackNumber = (string)searchData.SelectToken("tracks.items[0].track_number");
					o.Cover = (string)searchData.SelectToken("tracks.items[0].album.image.large");

					string strSeconds = (string)searchData.SelectToken("tracks.items[0].album.released_at");
					if (long.TryParse(strSeconds, out long seconds))
					{
						o.Date = epoch.AddSeconds(seconds).ToString();
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
