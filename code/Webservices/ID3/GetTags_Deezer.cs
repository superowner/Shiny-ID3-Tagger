//-----------------------------------------------------------------------
// <copyright file="GetTags_Deezer.cs" company="Shiny Id3 Tagger">
//	 Copyright (c) Shiny Id3 Tagger. All rights reserved.
// </copyright>
// <author>ShinyId3Tagger Team</author>
// <summary>Gets ID3 data from Deezer API for current track</summary>
// http://developers.deezer.com/api/search
// http://developers.deezer.com/api/explorer
// http://developers.deezer.com/api/search/autocomplete
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
		private async Task<Id3> GetTags_Deezer(HttpMessageInvoker client, string artist, string title, CancellationToken cancelToken)
		{
			Id3 o = new Id3();
			o.Service = "Deezer";
			
			Stopwatch sw = new Stopwatch();
			sw.Start();
			
			// ###########################################################################
			string artistEncoded = WebUtility.UrlEncode(artist);
			string titleEncoded = WebUtility.UrlEncode(title);

			HttpRequestMessage request = new HttpRequestMessage();			
			request.RequestUri = new Uri("http://api.deezer.com/search?q=artist:\"" + artistEncoded + "\"+track:\"" + titleEncoded + "\"&limit=1&order=RANKING");

			string content1 = await this.GetResponse(client, request, cancelToken);
			JObject data1 = JsonConvert.DeserializeObject<JObject>(content1, this.GetJsonSettings());

			if (data1 != null && data1.SelectToken("data") != null)
			{
				o.Artist = (string)data1.SelectToken("data[0].artist.name");
				o.Title = (string)data1.SelectToken("data[0].title");

				// ###########################################################################
				request = new HttpRequestMessage();
				request.RequestUri = new Uri("http://api.deezer.com/album/" + data1.SelectToken("data[0].album.id"));

				string content2 = await this.GetResponse(client, request, cancelToken);
				JObject data2 = JsonConvert.DeserializeObject<JObject>(content2, this.GetJsonSettings());

				if (data2 != null)
				{
					o.Album = (string)data2.SelectToken("title");
					o.Date = (string)data2.SelectToken("release_date");
					o.DiscCount = null;
					o.DiscNumber = null;
					o.TrackCount = (string)data2.SelectToken("nb_tracks");
					o.Cover = (string)data2.SelectToken("cover_big");
					o.Genre = (string)data2.SelectToken("genres.data[0].name");

					JToken[] tracklist = data2.SelectTokens("tracks.data[*].title").ToArray();
					int temp = Array.FindIndex(tracklist, t => t.ToString().Equals(o.Title, StringComparison.OrdinalIgnoreCase));
					if (temp != -1)
					{
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

// System.IO.File.WriteAllText (@"D:\response.json", content1);