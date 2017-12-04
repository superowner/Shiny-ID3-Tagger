//-----------------------------------------------------------------------
// <copyright file="GetTags_Genius.cs" company="Shiny Id3 Tagger">
//	 Copyright (c) Shiny Id3 Tagger. All rights reserved.
// </copyright>
// <author>ShinyId3Tagger Team</author>
// <summary>Gets ID3 data from Genius API for current track</summary>
// https://genius.com/api-clients
// https://docs.genius.com/#search-h2
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
		private async Task<Id3> GetTags_Genius(HttpMessageInvoker client, string artist, string title, CancellationToken cancelToken)
		{
			Id3 o = new Id3();
			o.Service = "Genius";
			
			Stopwatch sw = new Stopwatch();
			sw.Start();			
			
			// ###########################################################################
			string searchTermEnc = WebUtility.UrlEncode(artist + " - " + title);
			
			// Does this access_token expire ever? Docs don't mention a expire time when requesting an access_token
			HttpRequestMessage request = new HttpRequestMessage();
			request.RequestUri = new Uri("https://api.genius.com/search?q=" + searchTermEnc + "&access_token=" + User.Accounts["GeAccessToken"]);

			string content1 = await this.GetResponse(client, request, cancelToken);
			JObject data1 = JsonConvert.DeserializeObject<JObject>(content1, this.GetJsonSettings());
			
			if (data1 != null && data1.SelectToken("response.hits[0].result.api_path") != null)
			{
				request = new HttpRequestMessage();
				request.RequestUri = new Uri("https://api.genius.com" + data1.SelectToken("response.hits[0].result.api_path") + "?access_token=" + User.Accounts["GeAccessToken"]);
	
				string content2 = await this.GetResponse(client, request, cancelToken);
				JObject data2 = JsonConvert.DeserializeObject<JObject>(content2, this.GetJsonSettings());	
				
				if (data2 != null)
				{
					o.Artist = (string)data2.SelectToken("response.song.primary_artist.name");
					o.Title = (string)data2.SelectToken("response.song.title");	
					o.Album = (string)data2.SelectToken("response.song.album.name");					
					o.Genre = null;			// They don't offer this tag via API (https://genius.com/discussions/279491-Are-genius-song-tags-rap-rock-etc-exposed-through-the-songs-api)
	
					o.Cover = (string)data2.SelectToken("response.song.album.cover_art_url");

					var albumPath = data2.SelectToken("response.song.album.api_path");
					if (albumPath != null)
					{					
						request = new HttpRequestMessage();
						request.RequestUri = new Uri("https://api.genius.com" + albumPath + "?access_token=" + User.Accounts["GeAccessToken"]);
					
						string content3 = await this.GetResponse(client, request, cancelToken);
						JObject data3 = JsonConvert.DeserializeObject<JObject>(content3, this.GetJsonSettings());

						if (data3 != null)
						{
							o.DiscCount = null;			// Not provided in API response
							o.DiscNumber = null;		// Not provided in API response
							o.TrackCount = null;		// Not provided in API response
							o.TrackNumber = null;		// Not provided in API response
							o.Date = (string)data3.SelectToken("response.album.release_date");
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

// System.IO.File.WriteAllText (@"D:\response.json", content2);