//-----------------------------------------------------------------------
// <copyright file="Strip.cs" company="Shiny ID3 Tagger">
// Copyright (c) Shiny ID3 Tagger. All rights reserved.
// </copyright>
// <author>ShinyId3Tagger Team</author>
//-----------------------------------------------------------------------
// Reviewed and checked if all possible exceptions are prevented or handled

namespace Utils
{
	using System;
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
		/// <returns>The clensed string</returns>
		internal static string Strip(string str)
		{
			// Prevents exception "ArgumentNullException"
			if (str == null)
			{
				return str;
			}

			// Catches possible exceptions
			// - ArgumentException
			// - ArgumentNullException
			// - ArgumentOutOfRangeException
			// - RegexMatchTimeoutException
			// - OverflowException
			try
			{
				// Remove "version", "edition", "deluxe", "explicit", "disc"
				if ((bool)User.Settings["RemoveVersion"])
				{
					str = Regex.Replace(
						str,
						@"\s[\(\[].*?(version|edition|deluxe|explicit|disc).*?[\)\]]",
						string.Empty,
						RegexOptions.IgnoreCase,
						TimeSpan.FromMilliseconds(100));
				}

				// Remove "featuring, ""feat.", "ft."
				if ((bool)User.Settings["RemoveFeaturing"])
				{
					str = Regex.Replace(
						str,
						@"\s[\(\[].*?(feat(\.)? |ft(\.)? |featuring ).*?[\)\]]",
						string.Empty,
						RegexOptions.IgnoreCase,
						TimeSpan.FromMilliseconds(100));
				}

				// Replace uncommon spaces
				str = Regex.Replace(
					str,
					@"[\u202F\u00A0\u2005\u2009\u200B\u2060\u3000\uFEFF]",
					" ",
					RegexOptions.IgnoreCase,
					TimeSpan.FromMilliseconds(100));

				// Replace single sign ellipsis with 3 dots		https://en.wikipedia.org/wiki/Ellipsis
				str = Regex.Replace(
					str,
					@"…",
					"...",
					RegexOptions.IgnoreCase,
					TimeSpan.FromMilliseconds(100));

				// Replace uncommon apostrophe
				str = Regex.Replace(
					str,
					@"[‘’ʼ]",
					"'",
					RegexOptions.IgnoreCase,
					TimeSpan.FromMilliseconds(100));

				// Replace uncommon dashes		https://www.cs.tut.fi/~jkorpela/dashes.html
				str = Regex.Replace(
					str,
					@"[‐‑−－]",
					"-",
					RegexOptions.IgnoreCase,
					TimeSpan.FromMilliseconds(100));

				str = str.Trim();
			}
			catch (Exception)
			{
				// Do nothing
			}

			return str;
		}
	}
}
