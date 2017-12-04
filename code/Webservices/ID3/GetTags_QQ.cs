//-----------------------------------------------------------------------
// <copyright file="GetTags_QQ.cs" company="Shiny Id3 Tagger">
//	 Copyright (c) Shiny Id3 Tagger. All rights reserved.
// </copyright>
// <author>ShinyId3Tagger Team</author>
// <summary>Gets ID3 data from qq.com API for current track</summary>
// https://github.com/LIU9293/musicAPI/blob/master/src/qq.js
// https://github.com/metowolf/TencentMusicApi/blob/master/TencentMusicAPI.php
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

			HttpRequestMessage request = new HttpRequestMessage();
			request.RequestUri = new Uri("http://c.y.qq.com/soso/fcgi-bin/search_cp?w=" + searchTermEnc + "&format=json&p=0&n=100&aggr=1&lossless=1&cr=1");

			string content1 = await this.GetResponse(client, request, cancelToken);
			JObject data1 = JsonConvert.DeserializeObject<JObject>(content1, this.GetJsonSettings());

			if (data1 != null && data1.SelectToken("data.song.list[0]") != null)
			{
				JToken firstSong = data1.SelectToken("data.song.list[0]");

				// only 24 album hits where gracenote has 73 in Pop folder (already tried it twice to improve it. No success)
				foreach (JToken grpAlbum in data1.SelectToken("data.song.list[0].grp"))
				{
					if ((long)grpAlbum["pubtime"] > 0 && (long)grpAlbum["pubtime"] <= (long)firstSong["pubtime"])
					{
						firstSong = grpAlbum;
					}
				}

				o.Title = (string)firstSong["songname"];

				// ###########################################################################
				request = new HttpRequestMessage();
				request.RequestUri = new Uri("http://c.y.qq.com/v8/fcg-bin/fcg_v8_album_info_cp.fcg?format=json&albumid=" + firstSong["albumid"]);

				string content2 = await this.GetResponse(client, request, cancelToken);
				JObject data2 = JsonConvert.DeserializeObject<JObject>(content2, this.GetJsonSettings());

				if (data2 != null && data2.SelectToken("data") != null)
				{
					o.Artist = (string)data2.SelectToken("data.singername");
					o.Album = (string)data2.SelectToken("data.name");
					o.Date = (string)data2.SelectToken("data.aDate");

					if (data2.SelectToken("data.genre") != null)
					{
						string genre = (string)data2.SelectToken("data.genre");
						o.Genre = Regex.Replace(genre, " [\u4e00-\u9fa5]+$", string.Empty, RegexOptions.IgnoreCase);
					}

					o.TrackCount = (string)data2.SelectToken("data.total");
					o.DiscCount = null;
					o.Cover = "http://y.gtimg.cn/music/photo_new/T002R500x500M000" + (string)data2.SelectToken("data.mid") + ".jpg";
				}

				// ###########################################################################
				request = new HttpRequestMessage();
				request.RequestUri = new Uri("http://c.y.qq.com/v8/fcg-bin/fcg_play_single_song.fcg?format=json&songid=" + firstSong["songid"]);

				string content3 = await this.GetResponse(client, request, cancelToken);
				JObject data3 = JsonConvert.DeserializeObject<JObject>(content3, this.GetJsonSettings());

				if (data3 != null && data3.SelectToken("data") != null)
				{
					o.TrackNumber = (string)data3.SelectToken("data[0].index_album");
					o.DiscNumber = null;	// Maybe "data[0].index_cd" is the correct property, but Im unsure
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