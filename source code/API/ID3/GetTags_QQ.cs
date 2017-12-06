//-----------------------------------------------------------------------
// <copyright file="GetTags_QQ.cs" company="Shiny Id3 Tagger">
//	 Copyright (c) Shiny Id3 Tagger. All rights reserved.
// </copyright>
// <author>ShinyId3Tagger Team</author>
// <summary>Gets ID3 data from qq.com API for current track</summary>
// https://github.com/LIU9293/musicAPI/blob/master/src/qq.js
// https://github.com/metowolf/TencentMusicApi/blob/master/TencentMusicAPI.php
// Very bad search results. Already tried it twice to improve it but without any success
//-----------------------------------------------------------------------

namespace GlobalNamespace
{
	using System;
	using System.Diagnostics;
	using System.Linq;
	using System.Net;
	using System.Net.Http;
	using System.Text.RegularExpressions;
	using System.Threading;
	using System.Threading.Tasks;
	using Newtonsoft.Json;
	using Newtonsoft.Json.Linq;

	public partial class Form1
	{
		private async Task<Id3> GetTags_QQ(HttpMessageInvoker client, string artist, string title, CancellationToken cancelToken)
		{
			Id3 o = new Id3();
			o.Service = "QQ (Tencent)";

			Stopwatch sw = new Stopwatch();
			sw.Start();

			// ###########################################################################
			string searchTermEnc = WebUtility.UrlEncode(artist + " - " + title);

			using (HttpRequestMessage searchRequest = new HttpRequestMessage())
			{
				searchRequest.RequestUri = new Uri("http://c.y.qq.com/soso/fcgi-bin/search_cp?w=" + searchTermEnc + "&format=json&p=0&n=100&aggr=1&lossless=1&cr=1");

				string searchContent = await this.GetResponse(client, searchRequest, cancelToken);
				JObject searchData = JsonConvert.DeserializeObject<JObject>(searchContent, this.GetJsonSettings());

				if (searchData != null && searchData.SelectToken("data.song.list[0]") != null)
				{
					JToken firstSong = searchData.SelectToken("data.song.list[0]");

					foreach (JToken grpAlbum in searchData.SelectToken("data.song.list[0].grp"))
					{
						if ((long)grpAlbum["pubtime"] > 0 && (long)grpAlbum["pubtime"] <= (long)firstSong["pubtime"])
						{
							firstSong = grpAlbum;
						}
					}

					o.Title = (string)firstSong["songname"];

					// ###########################################################################
					using (HttpRequestMessage albumRequest = new HttpRequestMessage())
					{
						albumRequest.RequestUri = new Uri("http://c.y.qq.com/v8/fcg-bin/fcg_v8_album_info_cp.fcg?format=json&albumid=" + firstSong["albumid"]);

						string albumContent = await this.GetResponse(client, albumRequest, cancelToken);
						JObject albumData = JsonConvert.DeserializeObject<JObject>(albumContent, this.GetJsonSettings());

						if (albumData != null && albumData.SelectToken("data") != null)
						{
							o.Artist = (string)albumData.SelectToken("data.singername");
							o.Album = (string)albumData.SelectToken("data.name");
							o.Date = (string)albumData.SelectToken("data.aDate");

							if (albumData.SelectToken("data.genre") != null)
							{
								string genre = (string)albumData.SelectToken("data.genre");
								o.Genre = Regex.Replace(genre, " [\u4e00-\u9fa5]+$", string.Empty, RegexOptions.IgnoreCase);
							}

							o.TrackCount = (string)albumData.SelectToken("data.total");
							o.DiscCount = null;
							o.Cover = "http://y.gtimg.cn/music/photo_new/T002R500x500M000" + (string)albumData.SelectToken("data.mid") + ".jpg";
						}
					}

					// ###########################################################################
					using (HttpRequestMessage trackRequest = new HttpRequestMessage())
					{
						trackRequest.RequestUri = new Uri("http://c.y.qq.com/v8/fcg-bin/fcg_play_single_song.fcg?format=json&songid=" + firstSong["songid"]);

						string trackContent = await this.GetResponse(client, trackRequest, cancelToken);
						JObject trackData = JsonConvert.DeserializeObject<JObject>(trackContent, this.GetJsonSettings());

						if (trackData != null && trackData.SelectToken("data") != null)
						{
							o.TrackNumber = (string)trackData.SelectToken("data[0].index_album");
							o.DiscNumber = null;    // Maybe "data[0].index_cd" is the correct property, but Im unsure
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

// System.IO.File.WriteAllText (@"D:\response.json", content2);