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

			using (HttpRequestMessage searchRequest = new HttpRequestMessage())
			{
				searchRequest.RequestUri = new Uri("http://api.deezer.com/search?q=artist:\"" + artistEncoded + "\"+track:\"" + titleEncoded + "\"&limit=1&order=RANKING");

				string searchContent = await this.GetResponse(client, searchRequest, cancelToken);
				JObject searchData = JsonConvert.DeserializeObject<JObject>(searchContent, this.GetJsonSettings());

				if (searchData != null && searchData.SelectToken("data") != null)
				{
					o.Artist = (string)searchData.SelectToken("data[0].artist.name");
					o.Title = (string)searchData.SelectToken("data[0].title");

					// ###########################################################################
					using (HttpRequestMessage albumRequest = new HttpRequestMessage())
					{
						albumRequest.RequestUri = new Uri("http://api.deezer.com/album/" + searchData.SelectToken("data[0].album.id"));

						string albumContent = await this.GetResponse(client, albumRequest, cancelToken);
						JObject albumData = JsonConvert.DeserializeObject<JObject>(albumContent, this.GetJsonSettings());

						if (albumData != null)
						{
							o.Album = (string)albumData.SelectToken("title");
							o.Date = (string)albumData.SelectToken("release_date");
							o.DiscCount = null;
							o.DiscNumber = null;
							o.TrackCount = (string)albumData.SelectToken("nb_tracks");
							o.Cover = (string)albumData.SelectToken("cover_big");
							o.Genre = (string)albumData.SelectToken("genres.data[0].name");

							JToken[] tracklist = albumData.SelectTokens("tracks.data[*].title").ToArray();
							int temp = Array.FindIndex(tracklist, t => t.ToString().Equals(o.Title, StringComparison.OrdinalIgnoreCase));
							if (temp != -1)
							{
								o.TrackNumber = (temp + 1).ToString();
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

// System.IO.File.WriteAllText (@"D:\response.json", content1);