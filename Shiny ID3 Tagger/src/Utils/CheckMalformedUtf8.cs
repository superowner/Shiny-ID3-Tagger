//-----------------------------------------------------------------------
// <copyright file="CheckMalformedUtf8.cs" company="Shiny ID3 Tagger">
// Copyright (c) Shiny ID3 Tagger. All rights reserved.
// </copyright>
// <author>ShinyId3Tagger Team</author>
//-----------------------------------------------------------------------

namespace Utils
{
	using System.Collections.Generic;
	using System.Text;
	using System.Text.RegularExpressions;

	/// <summary>
	/// Represents the Utility class which holds various helper functions
	/// </summary>
	internal partial class Utils
	{
		/// <summary>
		/// Checks if a given string needs to be converted to UTF-8. Most occuring case is when an API saved a UTF-8 string as latin-1
		/// </summary>
		/// <param name="str">Input string which should be checked for certain characters which indicate a malformed UTF-8 string</param>
		/// <returns>The corrected string as UTF-8 string</returns>
		internal static string CheckMalformedUtf8(string str)
		{
			Regex regex = CreateRegex();
			Match match = regex.Match(str);

			if (match.Success)
			{
				return Encoding.UTF8.GetString(Encoding.GetEncoding("ISO-8859-1").GetBytes(str));
			}
			else
			{
				return str;
			}
		}

		/// <summary>
		/// <seealso href="https://stackoverflow.com/q/10484833/935614"/>
		/// </summary>
		/// <returns>A regex with the specified pattern</returns>
		private static Regex CreateRegex()
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

			string regex = string.Empty;
			foreach (string s in flags)
			{
				if (regex.Length > 0)
				{
					regex += '|';
				}

				regex += s;
			}

			return new Regex("(" + regex + ")");
		}
	}
}
