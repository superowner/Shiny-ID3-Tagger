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

			using (HttpRequestMessage searchRequest = new HttpRequestMessage())
			{
				// Set headers and method
				searchRequest.Headers.Add("User-Agent", "MiniLyrics");
				searchRequest.Headers.ExpectContinue = true;
				searchRequest.Method = HttpMethod.Post;
				searchRequest.RequestUri = new Uri("http://search.crintsoft.com/searchlyrics.htm");

				// Encode query and set POST body
				string query = "<?xml version='1.0' encoding='utf-8' standalone='yes' ?><searchV1 client=\"ViewLyricsOpenSearcher\" artist=\"" + tagNew.Artist + "\" title=\"" + tagNew.Title + "\" OnlyMatched=\"1\" />";
				byte[] queryEnc = EncodeQuery(query);
				searchRequest.Content = new ByteArrayContent(queryEnc);
				searchRequest.Content.Headers.ContentType = new MediaTypeHeaderValue("application/x-www-form-urlencoded");

				// Retrieve response from server
				HttpResponseMessage response = await client.SendAsync(searchRequest, cancelToken);
				byte[] searchContent = await response.Content.ReadAsByteArrayAsync();

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

		private static string DecodeResponse(byte[] data)
		{
			// Important: You cannot use a UTF8 string as input. Thus you cannot use the default method GetResponse
			// The position where XML string starts would vary and would not always be at byte position 23
			string result = string.Empty;

			if (data != null)
			{
				// Get magic key to decode response
				byte magicKey = data[1];

				// Remove first 22 bytes. XML string starts at byte 23
				data = data.Skip(22).ToArray();

				// Loop through encoded data and decode it with magic key
				int queryLen = data.Length;
				byte[] dataDecBytes = new Byte[queryLen];
				for (int i = 0; i < queryLen; i++)
				{
					int decByte = data[i] ^ magicKey;
					dataDecBytes[i] = (byte)decByte;
				}

				// Covert byte array to UTF8 string
				result = Encoding.UTF8.GetString(dataDecBytes);
			}

			return result;
		}

		private static byte[] EncodeQuery(string query)
		{
			// Get query length. Don't use "query.Length" since it produces wrong results for strings containing non-ASCII characters
			var queryLen = Encoding.UTF8.GetByteCount(query);

			// Generate MD5 hash from query and salt
			string md5Salt = "Mlv1clt4.0";
			byte[] saltedQueryBytes = Encoding.UTF8.GetBytes(query + md5Salt);
			byte[] md5HashBytes = MD5CryptoServiceProvider.Create().ComputeHash(saltedQueryBytes);

			// Calculate magic key
			decimal j = 0;
			for (int i = 0; i < queryLen; i++)
			{
				byte queryByte = Encoding.UTF8.GetBytes(query)[i];
				j += (int)queryByte;
			}
			char magicKey = (char)(Math.Round(j / queryLen));

			// Encode query bytes with magic key to get the encoded query
			byte[] queryBytes = Encoding.UTF8.GetBytes(query);
			byte[] queryEncBytes = new Byte[queryLen];
			for (int i = 0; i < queryLen; i++)
			{
				int encByte = queryBytes[i] ^ magicKey;
				queryEncBytes[i] = (byte)encByte;
			}

			// Combine all byte arrays to one
			byte[] finalbyteArray = Encoding.UTF8.GetBytes("\x02" + magicKey + "\x04\x00\x00\x00");
			finalbyteArray = finalbyteArray.Concat(md5HashBytes).Concat(queryEncBytes).ToArray();

			return finalbyteArray;
		}
	}
}
