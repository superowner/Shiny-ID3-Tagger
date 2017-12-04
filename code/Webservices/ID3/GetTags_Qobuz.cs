//-----------------------------------------------------------------------
// <copyright file="GetTags_Qobuz.cs" company="Shiny Id3 Tagger">
//	 Copyright (c) Shiny Id3 Tagger. All rights reserved.
// </copyright>
// <author>ShinyId3Tagger Team</author>
// <summary>Gets ID3 data from Qobuz API for current track</summary>
// http://www.qobuz.com/account/profile
// https://github.com/Qobuz/api-documentation
// https://github.com/Qobuz/api-documentation/blob/master/endpoints/track/search.md
// No english genre names possible. API returns french genre names
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
		private async Task<Id3> GetTags_Qobuz(HttpMessageInvoker client, string artist, string title, CancellationToken cancelToken)
		{
			Id3 o = new Id3();
			o.Service = "Qobuz";

			Stopwatch sw = new Stopwatch();
			sw.Start();

			// ###########################################################################
			string searchTermEnc = WebUtility.UrlEncode(artist + " - " + title);

			HttpRequestMessage request = new HttpRequestMessage();
			request.RequestUri = new Uri("http://www.qobuz.com/api.json/0.2/track/search?limit=1&app_id=" + User.Accounts["QoAppId"] + "&query=" + searchTermEnc);

			string content = await this.GetResponse(client, request, cancelToken);
			JObject data = JsonConvert.DeserializeObject<JObject>(content, this.GetJsonSettings());

			if (data != null && data.SelectToken("tracks.items[0]") != null)
			{
				o.Artist = (string)data.SelectToken("tracks.items[0].album.artist.name");
				o.Title = (string)data.SelectToken("tracks.items[0].title");
				o.Album = (string)data.SelectToken("tracks.items[0].album.title");
				o.Genre = (string)data.SelectToken("tracks.items[0].album.genre.name");
				o.DiscCount = (string)data.SelectToken("tracks.items[0].album.media_count");
				o.DiscNumber = (string)data.SelectToken("tracks.items[0].media_number");
				o.TrackCount = (string)data.SelectToken("tracks.items[0].album.tracks_count");
				o.TrackNumber = (string)data.SelectToken("tracks.items[0].track_number");
				o.Cover = (string)data.SelectToken("tracks.items[0].album.image.large");

				long seconds;
				string strSeconds = (string)data.SelectToken("tracks.items[0].album.released_at");
				if (long.TryParse(strSeconds, out seconds))
				{
					o.Date = epoch.AddSeconds(seconds).ToString();
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