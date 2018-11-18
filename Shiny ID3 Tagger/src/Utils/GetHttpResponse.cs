//-----------------------------------------------------------------------
// <copyright file="GetHttpResponse.cs" company="Shiny ID3 Tagger">
// Copyright (c) Shiny ID3 Tagger. All rights reserved.
// </copyright>
// <author>ShinyId3Tagger Team</author>
//-----------------------------------------------------------------------
// Reviewed and checked if all possible exceptions are prevented or handled

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
		/// <param name="customTimeoutMs">A timeout in seconds after a request is automatically canceled. Useful if a certain server has no own timeout</param>
		/// <returns>A <see cref="Task"/> representing the asynchronous operation.</returns>
		internal static async Task<dynamic> GetHttpResponse(
			HttpMessageInvoker client,
			HttpRequestMessage request,
			CancellationToken cancelToken,
			bool returnByteArray = false,
			int[] suppressedStatusCodes = null,
			int? customTimeoutMs = null)
		{
			const int maxRetries = 3;
			const int retryDelayMs = 2000;
			dynamic responseContent = null;
			string requestContent = null;
			int defaultTimeoutMs = 15000;
			HttpResponseMessage response = new HttpResponseMessage();
			CancellationTokenSource timeoutToken = null;

			// Prevent ArgumentNullException
			if (client == null || request == null || cancelToken == null)
			{
				return null;
			}

			// Catches possible exceptions
			// - ArgumentException
			// - ArgumentNullException
			// - ArgumentOutOfRangeException
			// - ObjectDisposedException
			try
			{
				// Create a timeout token which is linked to global cancelToken
				timeoutToken = CancellationTokenSource.CreateLinkedTokenSource(cancelToken);
				timeoutToken.CancelAfter(customTimeoutMs ?? defaultTimeoutMs);
			}
			catch (Exception)
			{
				return null;
			}

			// Take a backup copy of original request. Otherwise you can't send a request multiple times
			HttpRequestMessage requestBackup = CopyRequest(request);

			// If a post request has a body, save it for later (BuildLogMessage needs it)
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

				// Catches possible exceptions
				// - TaskCanceledException
				// - HttpRequestException
				// - ObjectDisposedException
				try
				{
					// Restore original request before each loop otherwise the request cannot be executed more than one time
					request = CopyRequest(requestBackup);

					// Send request and save response
					response = await client.SendAsync(request, timeoutToken.Token);

					// Read response as string. So far only the viewLyrics module needs a byte array
					if (returnByteArray)
					{
						responseContent = await response.Content.ReadAsByteArrayAsync();
					}
					else
					{
						responseContent = await response.Content.ReadAsStringAsync();
					}

					// Print request parameters and headers (after request and potential exception)
					List<string> debugMsg = new List<string> { "DEBUG:    API request was send" };
					debugMsg.AddRange(BuildLogMessage(request, requestContent, response, responseContent));
					Form1.Instance.RichTextBox_LogMessage(debugMsg.ToArray(), 4);

					// Cancel request and don't try again if the status code should be suppressed
					// These statuscodes are very common i.e. when a network resource doesn't exist
					// Return null because very often you get a HTTP body instead of valid JSON which later shows a JSON parsing warning
					if (suppressedStatusCodes != null && suppressedStatusCodes.Contains((int)response.StatusCode))
					{
						return null;
					}

					// A result was found. Cancel request and don't try again if response was successful
					if (response.IsSuccessStatusCode)
					{
						return responseContent;
					}

					// If we reach this point, that means the request was unsuccessful. Retry the request
					// Print out all request and response properties as a warning
					List<string> warningMsg = new List<string> { "WARNING:  Request was unsuccessful! " + i + " retries left. Retrying..." };
					warningMsg.AddRange(BuildLogMessage(request, requestContent, response, responseContent));
					Form1.Instance.RichTextBox_LogMessage(warningMsg.ToArray(), 3);

					// Wait some time before retrying to give server time to recover
					await Task.Delay(retryDelayMs);
				}
				catch (TaskCanceledException ex)
				{
					// Request timed out. Server took too long to respond. Cancel request and don't try again

					// User pressing ESC causes a TaskCanceledException too. Don't log then
					if (cancelToken.IsCancellationRequested == false)
					{
						// Also don't log failed requests when a custom timeout shorter than default timeout is set
						if (customTimeoutMs == null ||
							(customTimeoutMs != null && customTimeoutMs > defaultTimeoutMs))
						{
							List<string> warningMsg = new List<string> { "WARNING:  Server took longer than " + customTimeoutMs ?? defaultTimeoutMs + " milliseconds to respond!" };
							warningMsg.AddRange(BuildLogMessage(request, requestContent, response, responseContent));
							warningMsg.AddRange(new[] { "Message:  " + ex.Message });
							Form1.Instance.RichTextBox_LogMessage(warningMsg.ToArray(), 3);
						}
					}

					break;
				}
				catch (HttpRequestException ex)
				{
					// Request failed. A common error is when network connection is down. Cancel request immediately and don't try again
					List<string> warningMsg = new List<string> { "WARNING:  Server could not be reached!" };
					warningMsg.AddRange(BuildLogMessage(request, requestContent, response, responseContent));
					warningMsg.AddRange(new[] { "Message:  " + ex.Message });
					Form1.Instance.RichTextBox_LogMessage(warningMsg.ToArray(), 3);

					break;
				}
				catch (ObjectDisposedException)
				{
					// Usually this exception occurs when trying to access timeoutToken.Token while the source was already disposed by pressing eSC
					break;
				}
			}

			return responseContent;
		}
	}
}
