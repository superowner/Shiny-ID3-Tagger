//-----------------------------------------------------------------------
// <copyright file="IsValidUrl.cs" company="Shiny Id3 Tagger">
//	 Copyright (c) Shiny Id3 Tagger. All rights reserved.
// </copyright>
// <author>ShinyId3Tagger Team</author>
// <summary>Checks if a given string is a valid URL</summary>
// https://dotnetfiddle.net/XduN3A				this method still produces some false positives
// https://mathiasbynens.be/demo/url-regex		regex validator pattern comparison
//-----------------------------------------------------------------------

namespace GlobalNamespace
{
	using System;
	using System.Linq;

	public partial class Form1
	{
		private static bool IsValidUrl(string stringUri)
		{
			Uri uriResult;
			bool result = Uri.TryCreate(stringUri, UriKind.Absolute, out uriResult)
								&& (uriResult.Scheme == Uri.UriSchemeHttp
								|| uriResult.Scheme == Uri.UriSchemeHttps);
			return result;
		}
	}
}
