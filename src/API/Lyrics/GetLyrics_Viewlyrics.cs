//-----------------------------------------------------------------------
// <copyright file="GetLyrics_Viewlyrics.cs" company="Shiny Id3 Tagger">
//	 Copyright (c) Shiny Id3 Tagger. All rights reserved.
// </copyright>
// <author>ShinyId3Tagger Team</author>
// <summary>Retrieves track lyrics from viewlyrics.com</summary>
// https://github.com/rikels/LyricsSearch/blob/master/lyrics.py#L88
// https://github.com/PedroHLC/ViewLyricsOpenSearcher
// https://github.com/PedroHLC/ViewLyricsOpenSearcher/blob/master/viewlyricsopensearcher.php#L60
//-----------------------------------------------------------------------

namespace GlobalNamespace
{
	using System;
	using System.Diagnostics;
	using System.Linq;
	using System.Net.Http;
	using System.Net.Http.Headers;
	using System.Security.Cryptography;
	using System.Text;
	using System.Text.RegularExpressions;
	using System.Threading;
	using System.Threading.Tasks;
	using Newtonsoft.Json;
	using Newtonsoft.Json.Linq;

	public partial class Form1
	{
		private async Task<Id3> GetLyrics_Viewlyrics(HttpMessageInvoker client, Id3 tagNew, CancellationToken cancelToken)
		{
			Id3 o = new Id3();
			o.Service = "Viewlyrics";

			Stopwatch sw = new Stopwatch();
			sw.Start();

			// ###########################################################################
			string query = "<?xml version='1.0' encoding='utf-8' standalone='yes' ?><searchV1 client=\"ViewLyricsOpenSearcher\" artist=\"" + tagNew.Artist + "\" title=\"" + tagNew.Title + "\" OnlyMatched=\"1\" />";

			using (HttpRequestMessage searchRequest = new HttpRequestMessage())
			{
				searchRequest.Headers.Add("User-Agent", "MiniLyrics");
				searchRequest.Headers.Accept.Add(new MediaTypeWithQualityHeaderValue("application/xml"));
				searchRequest.Headers.ConnectionClose = false;
				searchRequest.Headers.ExpectContinue = true;
				searchRequest.Method = HttpMethod.Post;
				searchRequest.RequestUri = new Uri("http://search.crintsoft.com/searchlyrics.htm");

				byte[] queryEnc = EncodeQuery(query);
				searchRequest.Content = new ByteArrayContent(queryEnc);
				searchRequest.Content.Headers.ContentType = new MediaTypeHeaderValue("application/x-www-form-urlencoded");

				string searchContent = await this.GetResponse(client, searchRequest, cancelToken);

				if (searchContent != null)
				{
					string searchXml = DecodeResponse(searchContent);
					JObject searchData = JsonConvert.DeserializeObject<JObject>(this.ConvertXmlToJson(searchXml), this.GetJsonSettings());

					if (searchData != null && searchData.SelectToken("return.fileinfo") != null && searchData.SelectToken("return.fileinfo").Any())
					{
						IJEnumerable<JToken> linkList = searchData.SelectToken("return.fileinfo");

						string lyricsLink = (from item in linkList
										orderby ParseInt((string)item.SelectToken("@downloads")) descending
										select (string)item.SelectToken("@link")).FirstOrDefault();

						if (lyricsLink != null) {

							using (HttpRequestMessage lyricsRequest = new HttpRequestMessage())
							{
								lyricsRequest.RequestUri = new Uri("http://www.viewlyrics.com/" + lyricsLink);

								string lyricsContent = await this.GetResponse(client, lyricsRequest, cancelToken);
								string rawLyrics = lyricsContent;

								if (!string.IsNullOrWhiteSpace(rawLyrics))
								{
									// Sanitize
									rawLyrics = Regex.Replace(rawLyrics, @"\[\d{2}:\d{2}(\.\d{2})?\]([\r\n])?", string.Empty);  // Remove timestamps like [01:01:123] or [01:01]
									rawLyrics = Regex.Replace(rawLyrics, @"\[.*?\]", string.Empty);                             // Remove square brackets [by: XYZ] credits
									rawLyrics = string.Join("\n", rawLyrics.Split('\n').Select(s => s.Trim()));                 // Remove leading or ending white space per line
									rawLyrics = rawLyrics.Trim();                                                               // Remove leading or ending line breaks

									if (rawLyrics.Length > 1)
									{
										o.Lyrics = rawLyrics;
									}
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

		private static string DecodeResponse(string data)
		{
			string result = string.Empty;

			if (!string.IsNullOrWhiteSpace(data))
			{
				char magicKey = data[1];

				for (int i = 0; i < data.Length; i++)
				{
					result += (char)(data[i] ^ magicKey);
				}

				// HACK: Usually you just cut off the first 22 chars (bytes?) to remove control chars and get a valid XML string. Done via "int i = 22"
				// But viewlyrics.com returns a UTF8 response, and C# treats strings as UTF16 when reading in a response
				// I assume that I should not use "ReadAsStringAsync" in GetResponse. I should maybe work with a byte array to avoid encoding mistakes?
				result = Regex.Replace(result, @"^.*?\<\?xml ", "<?xml ", RegexOptions.Singleline | RegexOptions.IgnoreCase);
			}

			return result;
		}

		private static string HexToStr(string hex)
		{
			string str = string.Empty;
			for (int i = 0; i < hex.Length - 1; i += 2)
			{
				str = str + (char)Convert.ToInt32(string.Concat(hex[i], hex[i + 1]), 16);
			}
			return str;
		}

		private static byte[] EncodeQuery(string query)
		{
			// HACK: Remove all non-ASCII characters. Otherwise we would get a "502 bad gateway" error
			query = Regex.Replace(query, @"[^\u0000-\u007F]+", string.Empty);

			var dataLen = query.Length;
			string md5salt = "Mlv1clt4.0";

			byte[] asciiBytes = Encoding.ASCII.GetBytes(query + md5salt);
			byte[] hashBytes = MD5CryptoServiceProvider.Create().ComputeHash(asciiBytes);
			string hashHex = BitConverter.ToString(hashBytes).Replace("-", "").ToLower();
			string hashStr = HexToStr(hashHex);

			decimal j = 0;
			for (int i = 0; i < dataLen; i++)
			{
				j += (int)query[i];
			}

			char magicKey = (char)(Math.Round(j / dataLen));

			StringBuilder queryEnc = new StringBuilder(query);
			for (int i = 0; i < dataLen; i++)
			{
				queryEnc[i] = (char)(query[i] ^ magicKey);
			}

			byte[] utf16byteArray = Encoding.Unicode.GetBytes("\x02" + magicKey + "\x04\x00\x00\x00" + hashStr + queryEnc);

			// HACK: char stores values as UTF-16 (2 bytes). That's where the leading zeros come from. Find a better way to stay in UTF8 encoding
			// https://stackoverflow.com/questions/10708548/encoding-used-in-cast-from-char-to-byte#10708629
			byte[] pseudoUtf8 = utf16byteArray.Where((x, i) => i % 2 == 0).ToArray();

			return pseudoUtf8;
		}
	}
}
