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
		/// <param name="requestContent">request content which may hold a error message about why a request failed</param>
		/// <param name="response">The returned response will be added to output as once</param>
		/// <returns>The log message as list of string. Each list element represents a new line</returns>
		internal static List<string> BuildLogMessage(HttpRequestMessage request, string requestContent, HttpResponseMessage response)
		{
			List<string> errorMsg = new List<string>();

			// If request exists, add all parameters from request
			if (request != null)
			{
				errorMsg.Add("Request:  " + request.Method + " " + request.RequestUri.OriginalString);

				// Add headers
				foreach (var element in request.Headers)
				{
					errorMsg.Add("Header:   " + element.Key + ": " + string.Join(" ", element.Value));
				}
			}

			// If post body exists, add it
			if (!string.IsNullOrEmpty(requestContent))
			{
				errorMsg.Add("Body:     " + requestContent);
				requestContent = string.Empty;
			}

			// If response exists, add it
			if (response != null)
			{
				string responseContent = response.Content.ReadAsStringAsync().Result;
				errorMsg.Add("Status:   " + response.ReasonPhrase + ": " + (int)response.StatusCode);
				errorMsg.Add("Response: " + responseContent);
			}

			return errorMsg;
		}
	}
}
