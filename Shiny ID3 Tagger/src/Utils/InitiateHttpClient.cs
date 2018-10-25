//-----------------------------------------------------------------------
// <copyright file="InitiateHttpClient.cs" company="Shiny ID3 Tagger">
// Copyright (c) Shiny ID3 Tagger. All rights reserved.
// </copyright>
// <author>ShinyId3Tagger Team</author>
//-----------------------------------------------------------------------

namespace Utils
{
	using System;
	using System.Net;
	using System.Net.Http;

	/// <summary>
	/// Represents the Utility class which holds various helper functions
	/// </summary>
	internal partial class Utils
	{
		/// <summary>
		/// Creates a HTTP client which is used throughout the whole program for all API requests
		/// https://contrivedexample.com/2017/07/01/using-httpclient-as-it-was-intended-because-youre-not/
		/// </summary>
		/// <returns>The HTTP client</returns>
		internal static HttpClient InitiateHttpClient()
		{
			// Defaul settings for all requests
			// Compression is enabled by default (depends on if server supports it)
			// "UseCookies = false" is needed for Netease
			HttpClientHandler handler = new HttpClientHandler
			{
				UseCookies = false,
				AutomaticDecompression = DecompressionMethods.GZip
				| DecompressionMethods.Deflate
			};

			HttpClient client = new HttpClient(handler);
			client.DefaultRequestHeaders.Clear();
			client.DefaultRequestHeaders.ConnectionClose = false;			// Will attempt to keep connections open which makes more efficient use of the client.
			client.DefaultRequestHeaders.Connection.Add("Keep-Alive");		// Will attempt to keep connections open which makes more efficient use of the client.
			client.Timeout = TimeSpan.FromSeconds(15);						// Musicbrainz has 15s timeout in response header. Don't know if this setting is needed
			client.MaxResponseContentBufferSize = 256000000;
			ServicePointManager.DefaultConnectionLimit = 24;				// Not sure if it's needed since this limit applies to connections per remote host (per API), not in total per client

			return client;
		}
	}
}
