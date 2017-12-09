//-----------------------------------------------------------------------
// <copyright file="GetTags_Spotify.cs" company="Shiny Id3 Tagger">
//	 Copyright (c) Shiny Id3 Tagger. All rights reserved.
// </copyright>
// <author>ShinyId3Tagger Team</author>
// <summary>Gets ID3 data from Spotify API for current track</summary>
// https://developer.spotify.com/web-api/search-item/
// https://developer.spotify.com/web-api/object-model/
// https://github.com/spotify/web-api/issues/140	=> overall results are worse when removing the chars ",:
// https://github.com/spotify/web-api/issues/409
// fuzzy search with an appended asterisk (*) is useless since it only applies to a single word. And you can only use a maximum of 2 asterisk per query
// A search for the following album returns nothing, but returns something as soon as ":, are removed: From "The Hunger Games: Mockingjay, Part 2" Soundtrack
//-----------------------------------------------------------------------

namespace GlobalNamespace
{
	using System;
	using System.Collections.Generic;
	using System.Diagnostics;
	using System.Linq;
	using System.Net;
	using System.Net.Http;
	using System.Text;
	using System.Threading;
	using System.Threading.Tasks;
	using Newtonsoft.Json;
	using Newtonsoft.Json.Linq;

	public partial class Form1
	{
		private async Task<Id3> GetTags_Spotify(HttpMessageInvoker client, string artist, string title, CancellationToken cancelToken)
		{
			Id3 o = new Id3();
			o.Service = "Spotify";

			Stopwatch sw = new Stopwatch();
			sw.Start();

			// ###########################################################################
			string artistEncoded = WebUtility.UrlEncode(artist);
			string titleEncoded = WebUtility.UrlEncode(title);

			if (ApiSessionData.SpAccessToken == null || ApiSessionData.SpAccessTokenExpireDate < DateTime.Now)
			{
				using (HttpRequestMessage loginRequest = new HttpRequestMessage())
				{
					loginRequest.Method = HttpMethod.Post;
					loginRequest.RequestUri = new Uri("https://accounts.spotify.com/api/token");
					loginRequest.Content = new FormUrlEncodedContent(
						new Dictionary<string, string> { { "grant_type", "client_credentials" } });

					string credidsPlain = User.Accounts["SpClientId"] + ":" + User.Accounts["SpClientSecret"];
					byte[] credidsBytes = Encoding.UTF8.GetBytes(credidsPlain);
					string creditsBase64 = Convert.ToBase64String(credidsBytes);
					loginRequest.Headers.Add("Authorization", "Basic " + creditsBase64);

					string loginContent = await this.GetResponse(client, loginRequest, cancelToken);
					JObject loginData = JsonConvert.DeserializeObject<JObject>(loginContent, this.GetJsonSettings());

					if (loginData != null && loginData.SelectToken("access_token") != null)
					{
						ApiSessionData.SpAccessToken = (string)loginData.SelectToken("access_token");
						TimeSpan validDuration = TimeSpan.FromSeconds((int)loginData.SelectToken("expires_in"));
						ApiSessionData.SpAccessTokenExpireDate = DateTime.Now.Add(validDuration);
					}
				}
			}

			// ###########################################################################
			if (ApiSessionData.SpAccessToken != null)
			{
				using (HttpRequestMessage searchRequest = new HttpRequestMessage())
				{
					searchRequest.RequestUri = new Uri("https://api.spotify.com/v1/search?q=artist:\"" + artistEncoded + "\"+title:\"" + titleEncoded + "\"&type=track&limit=1");
					searchRequest.Headers.Add("Authorization", "Bearer " + ApiSessionData.SpAccessToken);

					string searchContent = await this.GetResponse(client, searchRequest, cancelToken);
					JObject searchData = JsonConvert.DeserializeObject<JObject>(searchContent, this.GetJsonSettings());

					if (searchData != null && searchData.SelectToken("tracks.items") != null && searchData.SelectToken("tracks.items").Any())
					{
						o.Artist = (string)searchData.SelectToken("tracks.items[0].artists[0].name");
						o.Title = (string)searchData.SelectToken("tracks.items[0].name");
						o.Album = (string)searchData.SelectToken("tracks.items[0].album.name");
						o.DiscNumber = (string)searchData.SelectToken("tracks.items[0].disc_number");
						o.TrackNumber = (string)searchData.SelectToken("tracks.items[0].track_number");

						// ###########################################################################
						string albumUrl = (string)searchData.SelectToken("tracks.items[0].album.href");

						if (IsValidUrl(albumUrl))
						{
							using (HttpRequestMessage albumRequest = new HttpRequestMessage())
							{
								albumRequest.Headers.Add("Authorization", "Bearer " + ApiSessionData.SpAccessToken);
								albumRequest.RequestUri = new Uri(albumUrl);

								string albumContent = await this.GetResponse(client, albumRequest, cancelToken);
								JObject albumData = JsonConvert.DeserializeObject<JObject>(albumContent, this.GetJsonSettings());

								if (albumData != null)
								{
									o.Date = (string)albumData.SelectToken("release_date");
									o.DiscCount = (string)albumData.SelectToken("tracks.items[-1:].disc_number");
									o.TrackCount = (string)albumData.SelectToken("tracks.total");
									o.Genre = string.Join(", ", albumData.SelectToken("genres"));
									o.Cover = (string)albumData.SelectToken("images").OrderBy(obj => obj["height"]).Last()["url"];
								}
							}
						}

						// ###########################################################################
						// "genres" is always empty for track and album lookups. Seems like a general Spotify issue: https://github.com/spotify/web-api/issues/157
						// Only artist lookups provide sometimes a genre. But they aren't sorted or weighted. Therefore artist genres produce bad results most of the time
						string artistUrl = (string)searchData.SelectToken("tracks.items[0].artists[0].href");

						if (IsValidUrl(artistUrl))
						{
							using (HttpRequestMessage artistRequest = new HttpRequestMessage())
							{
								artistRequest.Headers.Add("Authorization", "Bearer " + ApiSessionData.SpAccessToken);
								artistRequest.RequestUri = new Uri(artistUrl);

								string artistContent = await this.GetResponse(client, artistRequest, cancelToken);
								JObject artistData = JsonConvert.DeserializeObject<JObject>(artistContent, this.GetJsonSettings());

								if (artistData != null && artistData.SelectToken("genres") != null && artistData.SelectToken("genres").Any())
								{
									o.Genre = (string)artistData.SelectToken("genres[0]");
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
