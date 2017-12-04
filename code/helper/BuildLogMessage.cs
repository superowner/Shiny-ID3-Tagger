//-----------------------------------------------------------------------
// <copyright file="BuildLogMessage.cs" company="Shiny Id3 Tagger">
//	 Copyright (c) Shiny Id3 Tagger. All rights reserved.
// </copyright>
// <author>ShinyId3Tagger Team</author>
// <summary>Prepares the content which should be printed out from HttpRquests when debugging is enabled</summary>
//-----------------------------------------------------------------------

namespace GlobalNamespace
{
	using System;
	using System.Collections.Generic;
	using System.Linq;
	using System.Net.Http;
	using System.Text.RegularExpressions;

	public partial class Form1
	{
		private static List<string> BuildLogMessage(HttpRequestMessage request, string requestContent, HttpResponseMessage response)
		{
			// Add parameters from request
			List<string> errorMsg = new List<string>();
			errorMsg.Add("Request:  " + request.Method + " " + request.RequestUri.OriginalString);

			foreach (var element in request.Headers)
			{
				errorMsg.Add("Header:   " + element.Key + ": " + string.Join(" ", element.Value));
			}

			if (!string.IsNullOrEmpty(requestContent))
			{
				errorMsg.Add("Body:     " + requestContent);
				requestContent = string.Empty;
			}

			// Add parameters from response
			if (response != null)
			{
				string responseContent = response.Content.ReadAsStringAsync().Result;

				// Try to extract the HTML body
				Match match = Regex.Match(responseContent, "(?<=<body>)(?<text>.*?)(?=</body>)", RegexOptions.Singleline);
				if (match.Success)
				{
					responseContent = match.Groups["text"].Value.Trim();
				}

				// Try to extract the XML error message (Amazon API specific)
				match = Regex.Match(responseContent, "(?<=<Message>)(?<text>.*?)(?=</Message>)", RegexOptions.Singleline);
				if (match.Success)
				{
					responseContent = match.Groups["text"].Value.Trim();
				}

				// Show the complete response including HTML tags OR the extracted body/message if extracting was successful
				errorMsg.Add("Code:     " + response.ReasonPhrase + ": " + (int)response.StatusCode);
				errorMsg.Add("Response: " + responseContent);
			}

			return errorMsg;
		}
	}
}
