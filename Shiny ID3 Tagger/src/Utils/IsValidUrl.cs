//-----------------------------------------------------------------------
// <copyright file="IsValidUrl.cs" company="Shiny ID3 Tagger">
// Copyright (c) Shiny ID3 Tagger. All rights reserved.
// </copyright>
// <author>ShinyId3Tagger Team</author>
//-----------------------------------------------------------------------
// Reviewed and checked if all possible exceptions are prevented or handled

namespace Utils
{
	using System;

	/// <summary>
	/// Represents the Utility class which holds various helper functions
	/// </summary>
	internal partial class Utils
	{
		/// <summary>
		/// Checks if a given string can be interpreted as a valid HTTP or HTTPS url
		/// This method still produces some false positives
		/// <seealso href="https://dotnetfiddle.net/XduN3A"/>
		/// <seealso href="https://mathiasbynens.be/demo/url-regex"/>
		/// </summary>
		/// <param name="str">string to check</param>
		/// <returns>True or false according to previous check</returns>
		internal static bool IsValidUrl(string str)
		{
			bool isUri = Uri.TryCreate(str, UriKind.Absolute, out Uri uriResult)
								&& (uriResult.Scheme == Uri.UriSchemeHttp
								|| uriResult.Scheme == Uri.UriSchemeHttps);
			return isUri;
		}
	}
}
