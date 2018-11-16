//-----------------------------------------------------------------------
// <copyright file="IsValidBase64.cs" company="Shiny ID3 Tagger">
// Copyright (c) Shiny ID3 Tagger. All rights reserved.
// </copyright>
// <author>ShinyId3Tagger Team</author>
//-----------------------------------------------------------------------

namespace Utils
{
	using System;
	using System.Text.RegularExpressions;

	/// <summary>
	/// Represents the Utility class which holds various helper functions
	/// </summary>
	internal partial class Utils
	{
		/// <summary>
		/// Checks if a given string can be interpreted as a base64 string
		/// An empty string is not valid pattern
		/// </summary>
		/// <param name="str">String to check</param>
		/// <returns>True if string is a valid base64 string</returns>
		internal static bool IsValidBase64(string str)
		{
			str = str.Trim();
			return (str.Length % 4 == 0) && Regex.IsMatch(str, @"^[a-zA-Z0-9\+/]*={0,3}$", RegexOptions.None);
		}
	}
}
