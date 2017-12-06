﻿//-----------------------------------------------------------------------
// <copyright file="IsValidUrl.cs" company="Shiny Id3 Tagger">
//	 Copyright (c) Shiny Id3 Tagger. All rights reserved.
// </copyright>
// <author>ShinyId3Tagger Team</author>
// <summary>Checks if a given string is a valid URL</summary>
// https://dotnetfiddle.net/XduN3A				this method still produces some false positives
// https://mathiasbynens.be/demo/url-regex		RegEx validator pattern comparison
//-----------------------------------------------------------------------

namespace GlobalNamespace
{
	using System;

	internal partial class Helper
	{
		internal static bool IsValidUrl(string stringUri)
		{
			bool result = Uri.TryCreate(stringUri, UriKind.Absolute, out Uri uriResult)
								&& (uriResult.Scheme == Uri.UriSchemeHttp
								|| uriResult.Scheme == Uri.UriSchemeHttps);
			return result;
		}
	}
}