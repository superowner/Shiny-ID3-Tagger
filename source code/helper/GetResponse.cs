//-----------------------------------------------------------------------
// <copyright file="GetResponse.cs" company="Shiny Id3 Tagger">
//	 Copyright (c) Shiny Id3 Tagger. All rights reserved.
// </copyright>
// <author>ShinyId3Tagger Team</author>
// <summary>Executes all API requests. Has a built-in retry handler and a logger</summary>
//-----------------------------------------------------------------------

namespace GlobalNamespace
{
	using System;
	using System.Collections.Generic;
	using System.Linq;
	using System.Net;
	using System.Net.Http;
	using System.Threading;
	using System.Threading.Tasks;

	public partial class Form1
	{
		private async Task<string> GetResponse(HttpMessageInvoker client, HttpRequestMessage request, CancellationToken cancelToken)
		{
			const int Timeout = 15;
			const int MaxRetries = 3;
			const int RetryDelay = 2;

			string responseString = string.Empty;
			HttpResponseMessage response = new HttpResponseMessage();

			try
			{
				using (HttpRequestMessage requestBackup = CloneRequest(request))
				{
					for (int i = MaxRetries; i >= 1; i--)
					{
						if (cancelToken.IsCancellationRequested)
						{
							return string.Empty;
						}

						string requestContent = string.Empty;
						request = CloneRequest(requestBackup);

						var timeoutToken = CancellationTokenSource.CreateLinkedTokenSource(cancelToken);
						timeoutToken.CancelAfter(TimeSpan.FromSeconds(Timeout));

						try
						{
							// Save the request content for later reuse when an error occurs or debugging enabled is
							if (request.Content != null)
							{
								requestContent = request.Content.ReadAsStringAsync().Result;
							}

							// If debugging level is 3 (DEBUG) or higher, print out all requests, not only failed once
							if (User.Settings["DebugLevel"] >= 3)
							{
								List<string> errorMsg = BuildLogMessage(request, requestContent, null);
								this.PrintLogMessage("error", errorMsg.ToArray());
							}

							response = await client.SendAsync(request, timeoutToken.Token);

							// These are common errors i.e. when a queried track does not exist.Suppress them and return with an empty string
							if ((request.RequestUri.Host == "api.musicgraph.com" && response.StatusCode == HttpStatusCode.NotFound)
								|| (request.RequestUri.Host == "music.xboxlive.com" && response.StatusCode == HttpStatusCode.NotFound)
								|| (request.RequestUri.Host == "api.lololyrics.com" && response.StatusCode == HttpStatusCode.NotFound)
								|| (request.RequestUri.Host == "api.chartlyrics.com" && response.StatusCode == HttpStatusCode.NotFound)
								|| (request.RequestUri.Host == "coverartarchive.org" && response.StatusCode == HttpStatusCode.NotFound)
								|| (request.RequestUri.Host == "api.chartlyrics.com" && response.StatusCode == HttpStatusCode.InternalServerError)
								|| (request.RequestUri.Host == "accounts.spotify.com" && response.StatusCode == HttpStatusCode.BadGateway))
							{
								break;
							}

							if (response.IsSuccessStatusCode)
							{
								// Response was successful. Read content from response and return content
								responseString = await response.Content.ReadAsStringAsync();
								break;
							}
							else
							{
								// Response was not successful. But it was also not a common error
								// Check if user pressed cancel button. If no, print the error
								if (!cancelToken.IsCancellationRequested)
								{
									// If debugging is enabled in settings, print out all request properties
									if (User.Settings["DebugLevel"] >= 2)
									{
										List<string> errorMsg = new List<string> { "WARNING:  Response was unsuccessful! Retrying..." };
										errorMsg.Add("Retry:    " + i + "/" + MaxRetries);
										errorMsg.AddRange(BuildLogMessage(request, requestContent, response));

										this.PrintLogMessage("error", errorMsg.ToArray());
									}

									// Response was not successful. But it was also not a common error. And the user did not press cancel
									// This must be an uncommon error. Continue with our retry logic
									// But wait some seconds before you try it again to give the server time to recover
									Task wait = Task.Delay(RetryDelay * 1000);
								}
							}
						}
						catch (TaskCanceledException)
						{
							// The request timed out. Server took too long to respond. Cancel request immediately and don't try again
							// If debugging is enabled in settings, print out all request properties
							if (!cancelToken.IsCancellationRequested && User.Settings["DebugLevel"] >= 2)
							{
								List<string> errorMsg = new List<string> { "WARNING:  Server took longer than " + Timeout + " seconds to respond! Abort..." };
								errorMsg.AddRange(BuildLogMessage(request, requestContent, null));

								this.PrintLogMessage("error", errorMsg.ToArray());
							}

							break;
						}
						catch (Exception error)
						{
							// An unknown application error occurred. Cancel request immediately and don't try again
							// If debugging is enabled in settings, print out all request properties
							if (!cancelToken.IsCancellationRequested && User.Settings["DebugLevel"] >= 1)
							{
								Exception realerror = error;
								while (realerror.InnerException != null)
								{
									realerror = realerror.InnerException;
								}

								List<string> errorMsg = new List<string> { "ERROR:    An unknown application error occured! Abort..." };
								errorMsg.AddRange(BuildLogMessage(request, requestContent, null));
								errorMsg.Add("Message:  " + realerror.ToString().TrimEnd('\r', '\n'));

								this.PrintLogMessage("error", errorMsg.ToArray());
							}

							break;
						}
					}
				}
			}
			finally
			{
				// Couldn't figure out how to use a using statement since a new value gets assigned inside the using block
				if (response != null)
				{
					response.Dispose();
				}
			}

			return responseString;
		}
	}
}
