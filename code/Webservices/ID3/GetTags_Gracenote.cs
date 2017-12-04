//-----------------------------------------------------------------------
// <copyright file="GetTags_Gracenote.cs" company="Shiny Id3 Tagger">
//	 Copyright (c) Shiny Id3 Tagger. All rights reserved.
// </copyright>
// <author>ShinyId3Tagger Team</author>
// <summary>Gets ID3 data from Gracenote API for current track</summary>
// https://developer.gracenote.com/web-api
// https://developer.gracenote.com/sites/default/files/web/webapi/index.html#music-web-api/Registering%20a%20Device.html#scroll-bookmark-25

//-----------------------------------------------------------------------

namespace GlobalNamespace
{
	using System;
	using System.Diagnostics;
	using System.Linq;
	using System.Net.Http;
	using System.Text.RegularExpressions;
	using System.Threading;
	using System.Threading.Tasks;
	using Newtonsoft.Json;
	using Newtonsoft.Json.Linq;

	public partial class Form1
	{
		private async Task<Id3> GetTags_Gracenote(HttpMessageInvoker client, string artist, string title, CancellationToken cancelToken)
		{
			Id3 o = new Id3();
			o.Service = "Gracenote (Sony)";

			Stopwatch sw = new Stopwatch();
			sw.Start();

			// ###########################################################################
			string artistTemp = Regex.Replace(artist, "&(?!(amp|apos|quot|lt|gt);)", "&amp;");
			string titleTemp = Regex.Replace(title, "&(?!(amp|apos|quot|lt|gt);)", "&amp;");

			HttpRequestMessage request = new HttpRequestMessage(HttpMethod.Post, "https://c123.web.cddbp.net/webapi/xml/1.0/");
			request.Content = new StringContent(@"
					<QUERIES>
						<AUTH>
							<CLIENT>" + User.Accounts["GnClientId"] + @"</CLIENT>
							<USER>" + User.Accounts["GnUserId"] + @"</USER>
						</AUTH>
						<LANG>eng</LANG>
						<QUERY CMD=""ALBUM_SEARCH"">
							<MODE>SINGLE_BEST_COVER</MODE>
							<TEXT TYPE=""ARTIST"">" + artistTemp + @"</TEXT>
							<TEXT TYPE=""TRACK_TITLE"">" + titleTemp + @"</TEXT>
							<OPTION>
								<PARAMETER>COVER_SIZE</PARAMETER>
								<VALUE>LARGE,XLARGE,MEDIUM,SMALL,THUMBNAIL</VALUE>
							</OPTION>
						</QUERY>
					</QUERIES>");

			string content = await this.GetResponse(client, request, cancelToken);
			JObject data = JsonConvert.DeserializeObject<JObject>(this.ConvertXmlToJson(content), this.GetJsonSettings());

			if (data != null && data.SelectToken("RESPONSES.RESPONSE") != null)
			{
				if (data != null && data.SelectToken("RESPONSES.RESPONSE") != null)
				{
					o.Artist = (string)data.SelectToken("RESPONSES.RESPONSE.ALBUM.ARTIST");
					o.Title = (string)data.SelectToken("RESPONSES.RESPONSE.ALBUM.TRACK.TITLE");
					o.Album = (string)data.SelectToken("RESPONSES.RESPONSE.ALBUM.TITLE");
					o.Date = (string)data.SelectToken("RESPONSES.RESPONSE.ALBUM.DATE");
					o.Genre = (string)data.SelectToken("RESPONSES.RESPONSE.ALBUM.GENRE.#text");
					o.DiscCount = null;
					o.DiscNumber = null;
					o.TrackCount = (string)data.SelectToken("RESPONSES.RESPONSE.ALBUM.TRACK_COUNT");
					o.TrackNumber = (string)data.SelectToken("RESPONSES.RESPONSE.ALBUM.MATCHED_TRACK_NUM");
					o.Cover = (string)data.SelectToken("RESPONSES.RESPONSE.ALBUM.URL.#text");
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