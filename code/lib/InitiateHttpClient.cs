//-----------------------------------------------------------------------
// <copyright file="InitiateHttpClient.cs" company="Shiny Id3 Tagger">
//	 Copyright (c) Shiny Id3 Tagger. All rights reserved.
// </copyright>
// <author>ShinyId3Tagger Team</author>
// <summary>Create a single HTTP client which is used later for all Web API requests</summary>
//-----------------------------------------------------------------------

namespace GlobalNamespace
{
	using System;
	using System.Linq;
	using System.Net;
	using System.Net.Http;
	using System.Net.NetworkInformation;

	public partial class Form1
	{
		private static HttpClient InitiateHttpClient()
		{
			HttpClientHandler handler = new HttpClientHandler();

			Ping ping = new Ping();

			try
			{
				PingReply reply = ping.Send(User.Settings["Proxy"].Split(':')[0], 100);

				if (reply.Status == IPStatus.Success)
				{
					handler.Proxy = new WebProxy(User.Settings["Proxy"], false);
					handler.UseProxy = true;
				}
			}
			catch (ArgumentException)
			{
				// user entered an invalid "proxy:port" string in settings.json e.g. "0.0.0.0:0000"
			}
			catch (NullReferenceException)
			{
				// User closed the window while ping was still running
			}

			HttpClient client = new HttpClient(handler);
			client.MaxResponseContentBufferSize = 256000000;
			client.DefaultRequestHeaders.TryAddWithoutValidation("Content-Type", "application/json; charset=utf-8");
			client.DefaultRequestHeaders.Add("User-Agent", User.Settings["UserAgent"]);

			ping.Dispose();
			return client;
		}
	}
}
