//-----------------------------------------------------------------------
// <copyright file="GetResponse.cs" company="Shiny ID3 Tagger">
// Copyright (c) Shiny ID3 Tagger. All rights reserved.
// </copyright>
// <author>ShinyId3Tagger Team</author>
// <summary></summary>
//-----------------------------------------------------------------------
// TODO: Remove all "catch (Exception ex)" statements

namespace Utils
{
	using System;
	using System.Collections.Generic;
	using System.Linq;
	using System.Net.Http;
	using System.Threading;
	using System.Threading.Tasks;
	using GlobalVariables;
	using Shiny_ID3_Tagger;

	/// <summary>
	/// Represents the Utility class which holds various helper functions
	/// </summary>
	internal partial class Utils
	{
		/// <summary>
		/// Sends out all API requests to the web. Has a built-in retry handler and error logger
		/// </summary>
		/// <param name="client">The HTTP client for sending and receiving</param>
		/// <param name="request">The requests which holds the URL, method type, post body and headers</param>
		/// <param name="cancelToken">The global token for canceling any ongoing request</param>
		/// <param name="returnByteArray">A bool to indicate if the response should be read as a byte array instead of a string</param>
		/// <param name="suppressedStatusCodes">A array which holds statuscodes of common errors which can be ignored (not logged as error)</param>
		/// <param name="customTimeout">A timeout in seconds after a request is automatically canceled. Useful if a certain server has no own timeout</param>
		/// <returns>A <see cref="Task"/> representing the asynchronous operation.</returns>
		internal static async Task<dynamic> GetResponse(
			HttpMessageInvoker client,
			HttpRequestMessage request,
			CancellationToken cancelToken,
			bool returnByteArray = false,
			int[] suppressedStatusCodes = null,
			int? customTimeout = null)
		{
			const int maxRetries = 3;
			const int retryDelay = 2;
			int timeout = customTimeout ?? 15;

			dynamic result = null;
			HttpResponseMessage response = new HttpResponseMessage();
			HttpRequestMessage requestBackup = CloneRequest(request);

			for (int i = maxRetries; i >= 1; i--)
			{
				if (cancelToken.IsCancellationRequested)
				{
					return null;
				}

				string requestContent = string.Empty;
				request = CloneRequest(requestBackup);

				CancellationTokenSource timeoutToken = CancellationTokenSource.CreateLinkedTokenSource(cancelToken);
				timeoutToken.CancelAfter(TimeSpan.FromSeconds(timeout));

				try
				{
					// Save request content for later reuse when an error occurs
					if (request.Content != null)
					{
						requestContent = await request.Content.ReadAsStringAsync();
					}

					// Print out all requests, not only failed once
					List<string> errorMsg = BuildLogMessage(request, requestContent, null);
					Form1.Instance.RichTextBox_LogMessage(errorMsg.ToArray(), 3);

					response = await client.SendAsync(request, timeoutToken.Token);

					// Check if the returned status code is within a call-specific array of codes to suppress
					// These are common errors i.e. when a lyric doesn't exist. Don't log these errors
					if (suppressedStatusCodes != null && suppressedStatusCodes.Contains((int)response.StatusCode))
					{
						break;
					}

					// Response was successful. Read content from response and return content
					if (response.IsSuccessStatusCode)
					{
						// In most cases we return a string. But the viewLyrics module needs a byte array
						if (returnByteArray)
						{
							result = await response.Content.ReadAsByteArrayAsync();
						}
						else
						{
							result = await response.Content.ReadAsStringAsync();
						}

						break;
					}
					else
					{
						// Response was not successful. But it was also not a common error
						// Check if user pressed cancel button. If no, print the error
						if (!cancelToken.IsCancellationRequested)
						{
							// Print out all request and response properties
							errorMsg = new List<string> { "WARNING:  Response was unsuccessful! " + i + " retries left. Retrying..." };
							errorMsg.AddRange(BuildLogMessage(request, requestContent, response));
							Form1.Instance.RichTextBox_LogMessage(errorMsg.ToArray(), 2);

							// Response was not successful. But it was also not a common error. And user did not press cancel
							// This must be an uncommon error. Continue with our retry logic
							// But wait a bit before trying it again to give server some time to eventually recover
							await Task.Delay(retryDelay * 1000);
						}
					}
				}
				catch (TaskCanceledException)
				{
					// Don't log failed requests when a custom timeout is set (usually very short and often occuring)
					if (customTimeout == null)
					{
						// Request timed out. Server took too long to respond. Cancel request immediately and don't try again
						// Print out the request
						if (!cancelToken.IsCancellationRequested)
						{
							List<string> errorMsg = new List<string> { "WARNING:  Server took longer than " + timeout + " seconds to respond! Abort..." };
							errorMsg.AddRange(BuildLogMessage(request, requestContent, response));

							Form1.Instance.RichTextBox_LogMessage(errorMsg.ToArray(), 2);
						}
					}

					break;
				}
				catch (Exception error)
				{
					// An unknown application error occurred. Cancel request immediately and don't try again
					// Print out all request properties
					if (!cancelToken.IsCancellationRequested)
					{
						Exception realError = error;
						while (realError.InnerException != null)
						{
							realError = realError.InnerException;
						}

						List<string> errorMsg = new List<string> { "ERROR:    An unknown application error occured! Abort..." };
						errorMsg.AddRange(BuildLogMessage(request, requestContent, response));
						errorMsg.Add("Message:  " + realError.Message.ToString());

						Form1.Instance.RichTextBox_LogMessage(errorMsg.ToArray(), 2);
					}

					break;
				}
			}

			return result;
		}
	}
}
