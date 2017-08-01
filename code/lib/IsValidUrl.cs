//-----------------------------------------------------------------------
// <copyright file="IsValidUrl.cs" company="Shiny Id3 Tagger">
//	 Copyright (c) Shiny Id3 Tagger. All rights reserved.
// </copyright>
// <author>ShinyId3Tagger Team</author>
// <summary>Checks if a given string is a valid URL</summary>
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
