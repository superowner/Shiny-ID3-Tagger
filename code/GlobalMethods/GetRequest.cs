//-----------------------------------------------------------------------
// <copyright file="GetRequest.cs" company="Shiny Id3 Tagger">
//	 Copyright (c) Shiny Id3 Tagger. All rights reserved.
// </copyright>
// <author>ShinyId3Tagger Team</author>
// <summary>Executes all API requests. Has a built-in retry handler</summary>
//-----------------------------------------------------------------------

namespace GlobalNamespace
{
	using System;
	using System.Linq;
	using System.Net;
	using System.Net.Http;
	using System.Text.RegularExpressions;
	using System.Threading;
	using System.Threading.Tasks;

	public partial class Form1
	{
		private async Task<string> GetRequest(HttpMessageInvoker client, HttpRequestMessage request, CancellationToken cancelToken)
		{
			const int Timeout = 10;
			const int MaxRetries = 3;

			HttpResponseMessage response = new HttpResponseMessage();
			HttpRequestMessage requestBackup = CloneRequest(request);

			string responseString = string.Empty;
			
			for (int i = MaxRetries; i >= 1; i--)
			{
				if (cancelToken.IsCancellationRequested)
				{
					return string.Empty;
				}

				request = CloneRequest(requestBackup);

				var timeoutToken = CancellationTokenSource.CreateLinkedTokenSource(cancelToken);
				timeoutToken.CancelAfter(TimeSpan.FromSeconds(Timeout));

				// TODO: skip a webservice for the rest of the run if it wasn't reachable three times before
				try
				{
					response = await client.SendAsync(request, timeoutToken.Token);

					// These are common errors. We suppress them and dont show an error message. Don't read body. Return with empty string
					if ((request.RequestUri.Host == "api.spotify.com" && response.StatusCode == HttpStatusCode.NotFound) ||
						(request.RequestUri.Host == "music.xboxlive.com" && response.StatusCode == HttpStatusCode.NotFound) ||
						(request.RequestUri.Host == "coverartarchive.org" && response.StatusCode == HttpStatusCode.NotFound) ||
						(request.RequestUri.Host == "api.musicgraph.com" && response.StatusCode == HttpStatusCode.NotFound) ||
						(request.RequestUri.Host == "api.lololyrics.com" && response.StatusCode == HttpStatusCode.NotFound) ||
						(request.RequestUri.Host == "api.chartlyrics.com" && response.StatusCode == HttpStatusCode.NotFound) ||
						(request.RequestUri.Host == "api.musicgraph.com" && response.StatusCode == HttpStatusCode.InternalServerError) ||
						(request.RequestUri.Host == "api.chartlyrics.com" && response.StatusCode == HttpStatusCode.InternalServerError) ||
						(request.RequestUri.Host == "data.quantonemusic.com" && response.StatusCode == HttpStatusCode.Forbidden))
					{
						break;
					}

					if (response.IsSuccessStatusCode)
					{
						// response was successful. Read body and return
						responseString = await response.Content.ReadAsStringAsync();
						break;
					}
					else
					{
						// response was not successful. But it was also not a common error
						string errorResponse = response.Content.ReadAsStringAsync().Result;

						// Try to extract the HTML body
						Match match = Regex.Match(errorResponse, "(?<=<body>)(?<text>.*?)(?=</body>)", RegexOptions.Singleline);
						if (match.Success)
						{
							errorResponse = match.Groups["text"].Value.Trim();
						}

						// Try to extract the XML error message (Amazon API specific)
						match = Regex.Match(errorResponse, "(?<=<Message>)(?<text>.*?)(?=</Message>)", RegexOptions.Singleline);
						if (match.Success)
						{
							errorResponse = match.Groups["text"].Value.Trim();
						}						

						// Show the complete response including HTML tags OR the extracted body/message if extracting was successful
						string[] errorMsg =
							{
								"ERROR: Server response was unsuccessful",
								"Response code: " + response.ReasonPhrase + ": " + (int)response.StatusCode,
								"Requst URL: " + request.RequestUri.OriginalString,
								"Response message: " + errorResponse,
								"Retries left: " + i + "/" + MaxRetries
							};
						this.Log("error", errorMsg);
					}
				}
				catch (TaskCanceledException)
				{
					// The request timed out
					if (!cancelToken.IsCancellationRequested)
					{
						string[] errorMsg =
						{
							"ERROR: Server response took longer than " + Timeout + " seconds",
							"Requst URL: " + request.RequestUri.OriginalString
						};
						this.Log("error", errorMsg);
					}
					
					// Search was canceled by user. Be quiet in this case
					break;
				}
				catch (Exception ex)
				{
					// An unknown application error occured
					string[] errorMsg =
					{
						"ERROR: Unknown error occured",
						"Requst URL: " + request.RequestUri.OriginalString,
						ex.ToString()
					};
					this.Log("error", errorMsg);

					break;
				}

				await Task.Delay(2000);
			}

			response.Dispose();
			requestBackup.Dispose();

			return responseString;
		}
	}
}
