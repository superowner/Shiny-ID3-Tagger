//-----------------------------------------------------------------------
// <copyright file="IsValidUrl.cs" company="Shiny ID3 Tagger">
// Copyright (c) Shiny ID3 Tagger. All rights reserved.
// </copyright>
// <author>ShinyId3Tagger Team</author>
//-----------------------------------------------------------------------

namespace Utils
{
	using System;

	/// <summary>
	/// Represents the Utility class which holds various helper functions
	/// </summary>
	internal partial class Utils
	{
		/// <summary>
		/// Checks if a given string can be interpreted as a valid HTTP or HTTPS URL
		/// this method still produces some false positives <seealso href="https://dotnetfiddle.net/XduN3A"/>
		/// RegEx validator pattern comparison <seealso href="https://mathiasbynens.be/demo/url-regex"/>
		/// </summary>
		/// <param name="str">string to check</param>
		/// <returns>True or false according to previous check</returns>
		internal static bool IsValidUrl(string str)
		{
			bool result = Uri.TryCreate(str, UriKind.Absolute, out Uri uriResult)
								&& (uriResult.Scheme == Uri.UriSchemeHttp
								|| uriResult.Scheme == Uri.UriSchemeHttps);
			return result;
		}
	}
}
