//-----------------------------------------------------------------------
// <copyright file="DownloadClientFiles.cs" company="Shiny ID3 Tagger">
// Copyright (c) Shiny ID3 Tagger. All rights reserved.
// </copyright>
// <author>ShinyId3Tagger Team</author>
// <summary>Gets configurations from external config files</summary>
//-----------------------------------------------------------------------

namespace GlobalNamespace
{
	using System;
	using System.Net.Http;
	using System.Threading;
	using System.Threading.Tasks;
	using Newtonsoft.Json.Linq;

	public partial class Form1
	{
		private async Task<bool> DownloadClientFiles(CancellationToken cancelToken)
		{
			// Path for user credentials file
			string accountsConfigFilepath = AppDomain.CurrentDomain.BaseDirectory + @"config\accounts.json";

			using (HttpClient client = InitiateHttpClient())
			{
				using (HttpRequestMessage fileRequest = new HttpRequestMessage())
				{
					fileRequest.Headers.Accept.Add(new System.Net.Http.Headers.MediaTypeWithQualityHeaderValue("application/vnd.github.v3+json"));
					fileRequest.Headers.Add("User-Agent", (string)User.Settings["UserAgent"]);
					fileRequest.RequestUri = new Uri("https://api.github.com/repos/ShinyId3Tagger/Shiny-ID3-Tagger/contents/Shiny%20ID3%20Tagger/config/accounts.json");

					string fileContent = await this.GetResponse(client, fileRequest, cancelToken);
					JObject fileData = this.DeserializeJson(fileContent);

					string fileSha1 = (string)fileData.SelectToken("sha");
				}
			}

			return false;
		}
	}
}
