//-----------------------------------------------------------------------
// <copyright file="IsValidRegex.cs" company="Shiny ID3 Tagger">
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
		/// Checks if a given string can be interpreted as a valid RegEx pattern.
		/// An empty string is not valid pattern
		/// </summary>
		/// <param name="pattern">Pattern string to check</param>
		/// <returns>True if pattern is a valid Regex pattern</returns>
		internal static bool IsValidRegex(string pattern)
		{
			if (string.IsNullOrWhiteSpace(pattern))
			{
				return false;
			}

			try
			{
				Regex.Match(string.Empty, pattern);
			}
			catch (ArgumentException)
			{
				return false;
			}

			return true;
		}
	}
}
