﻿//-----------------------------------------------------------------------
// <copyright file="Strip.cs" company="Shiny ID3 Tagger">
// Copyright (c) Shiny ID3 Tagger. All rights reserved.
// </copyright>
// <author>ShinyId3Tagger Team</author>
//-----------------------------------------------------------------------

namespace Utils
{
	using System.Text.RegularExpressions;
	using GlobalVariables;

	/// <summary>
	/// Represents the Utility class which holds various helper functions
	/// </summary>
	internal partial class Utils
	{
		/// <summary>
		/// Removes brackets and unwanted stuff like "feat."
		/// Some unusual chars are replaced through better alternatives
		/// </summary>
		/// <param name="str">string to clean/sanitize</param>
		/// <returns>The cleaned string</returns>
		internal static string Strip(string str)
		{
			if (str != null)
			{
				if ((bool)User.Settings["RemoveVersion"])
				{
					const string RegExPattern = @"\s[\(\[].*?(version|edition|deluxe|explicit|disc).*?[\)\]]";
					str = Regex.Replace(str, RegExPattern, string.Empty, RegexOptions.IgnoreCase);
				}

				if ((bool)User.Settings["RemoveFeaturing"])
				{
					const string RegExPattern = @"\s[\(\[].*?(feat(\.)? |ft(\.)? |featuring ).*?[\)\]]";
					str = Regex.Replace(str, RegExPattern, string.Empty, RegexOptions.IgnoreCase);
				}

				str = Regex.Replace(str, @"[\u202F\u00A0\u2005\u2009\u200B\u2060\u3000\uFEFF]", " ");  // Replace uncommon spaces
				str = Regex.Replace(str, @"…", "...");			// Replace single sign ellipsis with 3 dots.	https://en.wikipedia.org/wiki/Ellipsis
				str = Regex.Replace(str, @"[‘’ʼ]", "'");		// Replace uncommon apostrophe
				str = Regex.Replace(str, @"[‐‑−－]", "-");		// Replace uncommon dashes					https://www.cs.tut.fi/~jkorpela/dashes.html
				str = str.Trim();
			}

			return str;
		}
	}
}
