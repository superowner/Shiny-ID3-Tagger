//-----------------------------------------------------------------------
// <copyright file="GetHttpResponse.cs" company="Shiny ID3 Tagger">
// Copyright (c) Shiny ID3 Tagger. All rights reserved.
// </copyright>
// <author>ShinyId3Tagger Team</author>
//-----------------------------------------------------------------------

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
		internal static async Task<dynamic> GetHttpResponse(
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

			// Take a backup copy of original request. Otherwise you can't send a request multiple times
			HttpRequestMessage requestBackup = CopyRequest(request);

			// Create a timeout token which is linked to global cancelToken
			CancellationTokenSource timeoutToken = CancellationTokenSource.CreateLinkedTokenSource(cancelToken);
			timeoutToken.CancelAfter(TimeSpan.FromSeconds(timeout));

			// Save request content for later (BuildLogMessage needs it)
			string requestContent = null;
			if (request.Content != null)
			{
				requestContent = await request.Content.ReadAsStringAsync();
			}

			for (int i = maxRetries; i >= 1; i--)
			{
				// Cancel request and don't try again if user pressed ESC
				if (cancelToken.IsCancellationRequested)
				{
					break;
				}

				try
				{
					// Restore original request before each loop otherwise the request cannot be executed more than one time
					request = CopyRequest(requestBackup);

					// Send request and save response
					response = await client.SendAsync(request, timeoutToken.Token);

					// Read response as string. So far only the viewLyrics module needs a byte array
					if (returnByteArray)
					{
						result = await response.Content.ReadAsByteArrayAsync();
					}
					else
					{
						result = await response.Content.ReadAsStringAsync();
					}

					// Print request parameters and headers (after request and potential exception)
					List<string> debugMsg = new List<string> { "DEBUG:    API request was send" };
					debugMsg.AddRange(BuildLogMessage(request, requestContent, response, result));
					Form1.Instance.RichTextBox_LogMessage(debugMsg.ToArray(), 4);

					// Cancel request and don't try again if the status code should be suppressed
					// These statuscodes are very common i.e. when a network resource doesn't exist
					if (suppressedStatusCodes != null && suppressedStatusCodes.Contains((int)response.StatusCode))
					{
						break;
					}

					// Cancel request and don't try again if response was successful
					if (response.IsSuccessStatusCode)
					{
						break;
					}

					// If we reach this, that means the request was unsuccessful. Retry the request
					// Print out all request and response properties
					debugMsg = new List<string> { "WARNING:  Request was unsuccessful! " + i + " retries left. Retrying..." };
					debugMsg.AddRange(BuildLogMessage(request, requestContent, response, result));
					Form1.Instance.RichTextBox_LogMessage(debugMsg.ToArray(), 3);

					// Wait some time before retrying to give server time to recover
					await Task.Delay(retryDelay * 1000);
				}
				catch (TaskCanceledException)
				{
					// Request timed out. Server took too long to respond. Cancel request and don't try again

					// Don't log failed requests when a custom timeout is set (usually very short and often occuring)
					// User pressing ESC causes a TaskCanceledException too. Don't log then
					if (customTimeout == null && !cancelToken.IsCancellationRequested)
					{
						List<string> warningMsg = new List<string> { "WARNING:  Server took longer than " + timeout + " seconds to respond!" };
						warningMsg.AddRange(BuildLogMessage(request, requestContent, response, result));
						Form1.Instance.RichTextBox_LogMessage(warningMsg.ToArray(), 3);
					}

					break;
				}
				catch (HttpRequestException ex)
				{
					// Request failed. A common error is when network connection is down. Cancel request immediately and don't try again
					List<string> warningMsg = new List<string> { "WARNING:  Server could not be reached!" };
					warningMsg.AddRange(BuildLogMessage(request, requestContent, response, result));
					warningMsg.AddRange(new[] { ex.Message });
					Form1.Instance.RichTextBox_LogMessage(warningMsg.ToArray(), 3);

					break;
				}
			}

			return result;
		}
	}
}
