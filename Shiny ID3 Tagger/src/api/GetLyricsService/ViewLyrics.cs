﻿//-----------------------------------------------------------------------
// <copyright file="ViewLyrics.cs" company="Shiny ID3 Tagger">
// Copyright (c) Shiny ID3 Tagger. All rights reserved.
// </copyright>
// <author>ShinyId3Tagger Team</author>
// <summary>Retrieves track lyrics from viewlyrics.com</summary>
// https://github.com/rikels/LyricsSearch/blob/master/lyrics.py#L88
// https://github.com/PedroHLC/ViewLyricsOpenSearcher
// https://github.com/PedroHLC/ViewLyricsOpenSearcher/blob/master/viewlyricsopensearcher.php#L60
// https://github.com/osdlyrics/osdlyrics/blob/master/lyricsources/viewlyrics/viewlyrics.py
// LRC format explained: https://wiki.mobileread.com/wiki/LRC
//-----------------------------------------------------------------------

namespace GetLyrics
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
	using GlobalVariables;
	using Newtonsoft.Json.Linq;
	using Utils;

	internal class ViewLyrics : IGetLyricsService
	{
		public async Task<Id3> GetLyrics(HttpMessageInvoker client, Id3 tagNew, CancellationToken cancelToken)
		{
			Id3 o = new Id3 {Service = "Viewlyrics" };

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
				byte[] queryEnc = this.EncodeQuery(query);
				searchRequest.Content = new ByteArrayContent(queryEnc);
				searchRequest.Content.Headers.ContentType = new MediaTypeHeaderValue("application/x-www-form-urlencoded");

				// Retrieve response from server, 4th argument tells GetResponse() to return a byte array rather than a string
				byte[] response = await Utils.GetResponse(client, searchRequest, cancelToken, true);
				string searchContent = this.DecodeResponse(response);
				JObject searchData = Utils.DeserializeJson(Utils.ConvertXmlToJson(searchContent));

				if (searchData?.SelectToken("return.fileinfo") != null && searchData.SelectToken("return.fileinfo").Any())
				{
					IJEnumerable<JToken> linkList = searchData.SelectToken("return.fileinfo");

					string lyricsLink = (from item in linkList
									orderby Utils.ParseInt((string)item.SelectToken("@downloads")) descending
									select (string)item.SelectToken("@link")).FirstOrDefault();

					if (lyricsLink != null)
					{
						using (HttpRequestMessage lyricsRequest = new HttpRequestMessage())
						{
							lyricsRequest.RequestUri = new Uri("http://www.viewlyrics.com/" + lyricsLink);

							string lyricsContent = await Utils.GetResponse(client, lyricsRequest, cancelToken);
							string rawLyrics = lyricsContent;

							if (!string.IsNullOrWhiteSpace(rawLyrics))
							{
								// Sanitize
								rawLyrics = Regex.Replace(rawLyrics, @"\[\d{2}:\d{2}(\.\d{2})?\]([\r\n])?", string.Empty);	// Remove timestamps like [01:01:123] or [01:01]
								rawLyrics = Regex.Replace(rawLyrics, @"\[.*?\]", string.Empty);								// Remove square brackets [by: XYZ] credits
								rawLyrics = string.Join("\n", rawLyrics.Split('\n').Select(s => s.Trim()));					// Remove leading or ending white space per line
								rawLyrics = rawLyrics.Trim();																// Remove leading or ending line breaks

								if (rawLyrics.Length > 1)
								{
									o.Lyrics = rawLyrics;
								}
							}
						}
					}
				}
			}

			// ###########################################################################
			sw.Stop();
			o.Duration = $"{sw.Elapsed:s\\,f}";

			return o;
		}

		private string DecodeResponse(byte[] data)
		{
			string result = null;

			if (data == null)
			{
				return null;
			}

			// Get magic key to decode response
			byte magicKey = data[1];

			// Remove first 22 bytes. XML string starts at byte 23
			data = data.Skip(22).ToArray();

			// Loop through encoded data and decode it with magic key
			int queryLen = data.Length;
			byte[] dataDecBytes = new byte[queryLen];
			for (int i = 0; i < queryLen; i++)
			{
				int decByte = data[i] ^ magicKey;
				dataDecBytes[i] = (byte)decByte;
			}

			// Covert byte array to UTF8 string
			result = Encoding.UTF8.GetString(dataDecBytes);

			return result;
		}

		private byte[] EncodeQuery(string query)
		{
			// Get query length. Don't use "query.Length" since it produces wrong results for strings containing non-ASCII characters
			int queryLen = Encoding.UTF8.GetByteCount(query);

			// Generate MD5 hash from query and salt
			string md5Salt = "Mlv1clt4.0";
			byte[] saltedQueryBytes = Encoding.UTF8.GetBytes(query + md5Salt);
			byte[] md5HashBytes = MD5.Create().ComputeHash(saltedQueryBytes);

			// Calculate magic key
			decimal j = 0;
			for (int i = 0; i < queryLen; i++)
			{
				byte queryByte = Encoding.UTF8.GetBytes(query)[i];
				j += (int)queryByte;
			}

			char magicKey = (char)Math.Round(j / queryLen);

			// Encode query bytes with magic key to get the encoded query
			byte[] queryBytes = Encoding.UTF8.GetBytes(query);
			byte[] queryEncBytes = new byte[queryLen];
			for (int i = 0; i < queryLen; i++)
			{
				int encByte = queryBytes[i] ^ magicKey;
				queryEncBytes[i] = (byte)encByte;
			}

			// Combine all byte arrays to one
			byte[] result = Encoding.UTF8.GetBytes("\x02" + magicKey + "\x04\x00\x00\x00");
			result = result.Concat(md5HashBytes).Concat(queryEncBytes).ToArray();

			return result;
		}
	}
}
