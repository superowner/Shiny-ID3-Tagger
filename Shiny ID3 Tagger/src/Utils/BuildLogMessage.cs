//-----------------------------------------------------------------------
// <copyright file="BuildLogMessage.cs" company="Shiny ID3 Tagger">
// Copyright (c) Shiny ID3 Tagger. All rights reserved.
// </copyright>
// <author>ShinyId3Tagger Team</author>
//-----------------------------------------------------------------------

namespace Utils
{
	using System.Collections.Generic;
	using System.Net.Http;

	/// <summary>
	/// Represents the Utility class which holds various helper functions
	/// </summary>
	internal partial class Utils
	{
		/// <summary>
		/// Prepares the message which should be printed out when a HttpRquest failed or debugging is enabled
		/// </summary>
		/// <param name="request">request object to get method and status code which may hold information about why a request failed</param>
		/// <param name="requestContent">request content which may hold an error message about why a request failed</param>
		/// <param name="response">The returned response</param>
		/// <param name="result">The result body of the response</param>
		/// <returns>The log message as list of string. Each list element represents a new line</returns>
		internal static List<string> BuildLogMessage(HttpRequestMessage request, string requestContent, HttpResponseMessage response, dynamic result)
		{
			List<string> errorMsg = new List<string>();

			// If request exists, add all parameters from request
			if (request != null && request.RequestUri != null && request.RequestUri.OriginalString != null)
			{
				errorMsg.Add("Request:  " + request.Method + " " + request.RequestUri.OriginalString);

				// Add headers
				foreach (var element in request.Headers)
				{
					errorMsg.Add("Header:   " + element.Key + ": " + string.Join(" ", element.Value));
				}
			}

			// If request body exists, add it
			if (requestContent != null)
			{
				errorMsg.Add("Body:     " + requestContent);
			}

			// If response exists, add it
			if (response != null)
			{
				errorMsg.Add("Status:   " + response.ReasonPhrase + ": " + (int)response.StatusCode);
			}

			// If response had a string result, add it
			if (result?.GetType() == typeof(string))
			{
				errorMsg.Add("Response: " + result);
			}

			return errorMsg;
		}
	}
}
