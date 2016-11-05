//-----------------------------------------------------------------------
// <copyright file="GetCover_Bing.cs" company="Shiny Id3 Tagger">
//	 Copyright (c) Shiny Id3 Tagger. All rights reserved.
// </copyright>
// <author>ShinyId3Tagger Team</author>
// <summary>Retrieves an image via normal Bing Web Search</summary>
// https://datamarket.azure.com/dataset/explore/bing/search
//-----------------------------------------------------------------------

namespace GlobalNamespace
{
	using System;
	using System.Linq;
	using System.Net;
	using System.Net.Http;
	using System.Net.Http.Headers;
	using System.Text;
	using System.Threading;
	using System.Threading.Tasks;
	using Newtonsoft.Json;
	using Newtonsoft.Json.Linq;

	public partial class Form1
	{
		private async Task<string> GetCoverBing(HttpMessageInvoker client, Id3 tagOld, Id3 tagNew, CancellationToken cancelToken)
		{
			string artist = tagNew.Artist ?? tagOld.Artist;
			string title = tagNew.Title ?? tagOld.Title;
			string album = tagNew.Album ?? tagOld.Album;
			string searchTerm = artist + " " + title + " " + album;
			string searchTermEnc = WebUtility.UrlEncode("'" + searchTerm + "'");

			HttpRequestMessage request = new HttpRequestMessage();
			request.RequestUri = new Uri("https://api.datamarket.azure.com/Bing/Search/v1/Image?Query=" + searchTermEnc + "&$top=1&$format=JSON");
			
			string credentials = Convert.ToBase64String(Encoding.ASCII.GetBytes("MyUsername:" + User.Accounts["BiAccountKey"]));
			request.Headers.Authorization = new AuthenticationHeaderValue("Basic", credentials);

			string content1 = await this.GetRequest(client, request, cancelToken);
			JObject data = JsonConvert.DeserializeObject<JObject>(content1, this.GetJsonSettings());

			string coverUrl = null;
			if (data != null)
			{
				coverUrl = (string)data.SelectToken("d.results[0].MediaUrl");
			}
			
			request.Dispose();
			return coverUrl;
		}
	}
}

// System.IO.File.WriteAllText (@"D:\response.json", content2);