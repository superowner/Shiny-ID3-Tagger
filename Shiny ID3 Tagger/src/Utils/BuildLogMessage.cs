﻿//-----------------------------------------------------------------------
// <copyright file="BuildLogMessage.cs" company="Shiny ID3 Tagger">
// Copyright (c) Shiny ID3 Tagger. All rights reserved.
// </copyright>
// <author>ShinyId3Tagger Team</author>
// <summary>Prepares the content which should be printed out from HttpRquests when debugging is enabled</summary>
//-----------------------------------------------------------------------

namespace Utils
{
	using System.Collections.Generic;
	using System.Net.Http;

	internal partial class Utils
	{
		internal static List<string> BuildLogMessage(HttpRequestMessage request, string requestContent, HttpResponseMessage response)
		{
			// Add parameters from request
			List<string> errorMsg = new List<string>
			{
				"Request:  " + request.Method + " " + request.RequestUri.OriginalString,
				"Status:   " + response.ReasonPhrase + ": " + (int)response.StatusCode
			};

			foreach (var element in request.Headers)
			{
				errorMsg.Add("Header:   " + element.Key + ": " + string.Join(" ", element.Value));
			}

			if (!string.IsNullOrEmpty(requestContent))
			{
				errorMsg.Add("Body:     " + requestContent);
				requestContent = string.Empty;
			}

			// Add response content
			if (response.Content != null)
			{
				string responseContent = response.Content.ReadAsStringAsync().Result;
				errorMsg.Add("Response: " + responseContent);
			}

			return errorMsg;
		}
	}
}
