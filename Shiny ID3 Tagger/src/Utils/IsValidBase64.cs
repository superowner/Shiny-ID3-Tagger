//-----------------------------------------------------------------------
// <copyright file="IsValidBase64.cs" company="Shiny ID3 Tagger">
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
		/// Checks if a given string can be interpreted as a base64 string
		/// </summary>
		/// <param name="str">String to check</param>
		/// <returns>True if string is a valid base64 string</returns>
		internal static bool IsValidBase64(string str)
		{
			// Prevents exception "ArgumentNullException"
			if (str == null)
			{
				return false;
			}

			str = str.Trim();

			// The length of a base64 string is always a multiple of 4
			if (str.Length % 4 != 0)
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
				return Regex.IsMatch(str, @"^[a-zA-Z0-9\+/]*={0,3}$", RegexOptions.None, TimeSpan.FromMilliseconds(100));
			}
			catch (Exception)
			{
				return false;
			}
		}
	}
}
