//-----------------------------------------------------------------------
// <copyright file="ConvertMalformedUtf8.cs" company="Shiny ID3 Tagger">
// Copyright (c) Shiny ID3 Tagger. All rights reserved.
// </copyright>
// <author>ShinyId3Tagger Team</author>
//-----------------------------------------------------------------------
// Reviewed and checked if all possible exceptions are prevented or handled

namespace Utils
{
	using System;
	using System.Collections.Generic;
	using System.Text;
	using System.Text.RegularExpressions;
	using GlobalVariables;

	/// <summary>
	/// Represents the Utility class which holds various helper functions
	/// </summary>
	internal partial class Utils
	{
		/// <summary>
		/// Checks and converts a string to UTF-8. Most occuring case is when an API saved a UTF-8 string as latin-1 in their database
		/// This method is only used by chartlyrics API. And malformed UTF-8 strings are very rare
		/// </summary>
		/// <param name="str">Input string</param>
		/// <returns>The corrected string in UTF-8 encoding</returns>
		internal static string ConvertMalformedUtf8(string str)
		{
			// Prevents exception "ArgumentNullException"
			if (str == null || GlobalVariables.MalformedUtf8Pattern == null)
			{
				return str;
			}

			// Catches possible exceptions
			// - ArgumentException
			// - ArgumentNullException
			// - ArgumentOutOfRangeException
			// - RegexMatchTimeoutException
			// - OverflowException
			// - EncoderFallbackException
			try
			{
				Regex regExObj = new Regex(GlobalVariables.MalformedUtf8Pattern, RegexOptions.None, TimeSpan.FromMilliseconds(100D));
				Match matchObj = regExObj.Match(str);

				if (matchObj.Success)
				{
					return Encoding.UTF8.GetString(Encoding.GetEncoding("ISO-8859-1").GetBytes(str));
				}
				else
				{
					return str;
				}
			}
			catch (Exception)
			{
				return str;
			}
		}

		/// <summary>
		/// Creates a regex pattern which detects if utf-8 bytes were saved into an iso-8859-1 database
		/// This leads to malformed strings like "CÃ©line" instead of "Céline"
		/// <seealso href="https://stackoverflow.com/q/10484833/935614"/>
		/// </summary>
		/// <returns>A string which represents a regex pattern</returns>
		internal static string CreateRegexToCheckMalformedUtf8()
		{
			const string Specials = "ÀÁÂÃÄÅÆÇÈÉÊËÌÍÎÏÐÑÒÓÔÕÖ×ØÙÚÛÜÝÞßàáâãäåæçèéêëìíîïðñòóôõö";

			List<string> flags = new List<string>();
			foreach (char c in Specials)
			{
				string interpretedAsLatin1 = Encoding.GetEncoding("ISO-8859-1").GetString(Encoding.UTF8.GetBytes(c.ToString())).Trim();
				if (interpretedAsLatin1.Length > 0)
				{
					flags.Add(interpretedAsLatin1);
				}
			}

			string pattern = string.Empty;
			foreach (string s in flags)
			{
				if (pattern.Length > 0)
				{
					pattern += '|';
				}

				pattern += s;
			}

			return "(" + pattern + ")";
		}
	}
}
