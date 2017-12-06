//-----------------------------------------------------------------------
// <copyright file="InitiateHttpClient.cs" company="Shiny Id3 Tagger">
//	 Copyright (c) Shiny Id3 Tagger. All rights reserved.
// </copyright>
// <author>ShinyId3Tagger Team</author>
// <summary>Create a single HTTP client which is used later for all Web API requests</summary>
// https://contrivedexample.com/2017/07/01/using-httpclient-as-it-was-intended-because-youre-not/
//-----------------------------------------------------------------------

namespace GlobalNamespace
{
	using System;
	using System.Net;
	using System.Net.Http;

	internal partial class Helper
	{
		internal static HttpClient InitiateHttpClient()
		{
			// Default settings for all connections
			HttpClientHandler handler = new HttpClientHandler();
			handler.UseCookies = false;										// this setting is needed for netease
			handler.AutomaticDecompression = DecompressionMethods.GZip
				| DecompressionMethods.Deflate;								// enable compression (depends on if server supports it)

			HttpClient client = new HttpClient(handler);
			client.DefaultRequestHeaders.Clear();
			client.DefaultRequestHeaders.ConnectionClose = false;			// Will attempt to keep the connection open which makes more efficient use of the client.
			client.DefaultRequestHeaders.Connection.Add("Keep-Alive");		// Will attempt to keep the connection open which makes more efficient use of the client.
			client.Timeout = TimeSpan.FromSeconds(15);						// Musicbrainz has 15s timeout in response header. Don't know if this setting is needed
			client.MaxResponseContentBufferSize = 256000000;
			ServicePointManager.DefaultConnectionLimit = 24;				// Not sure if it's needed since this limit applies to connections per remote host (per API), not in total per client

			return client;
		}
	}
}
