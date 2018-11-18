//-----------------------------------------------------------------------
// <copyright file="IsValidRegex.cs" company="Shiny ID3 Tagger">
// Copyright (c) Shiny ID3 Tagger. All rights reserved.
// </copyright>
// <author>ShinyId3Tagger Team</author>
//-----------------------------------------------------------------------
// Reviewed and checked if all possible exceptions are prevented or handled

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
		/// User settings allow to define custom regex pattern which must be checked first
		/// </summary>
		/// <param name="pattern">Pattern string to check</param>
		/// <returns>True if pattern is a valid Regex pattern</returns>
		internal static bool IsValidRegex(string pattern)
		{
			if (string.IsNullOrWhiteSpace(pattern))
			{
				return false;
			}

			// Catches possible exceptions
			// - ArgumentException
			// - ArgumentNullException
			// - ArgumentOutOfRangeException
			// - RegexMatchTimeoutException
			// - OverflowException
			try
			{
				return Regex.IsMatch(string.Empty, pattern, RegexOptions.None, TimeSpan.FromMilliseconds(100));
			}
			catch (Exception)
			{
				return false;
			}
		}
	}
}
