//-----------------------------------------------------------------------
// <copyright file="Strip.cs" company="Shiny Id3 Tagger">
//	 Copyright (c) Shiny Id3 Tagger. All rights reserved.
// </copyright>
// <author>ShinyId3Tagger Team</author>
// <summary>Removes brackets and unwanted stuff like "feat.". Some weird chars are replaced through better alternatives</summary>
//-----------------------------------------------------------------------

namespace GlobalNamespace
{
	using System;
	using System.Diagnostics;
	using System.Linq;
	using System.Text.RegularExpressions;

	public partial class Form1
	{
		private static string Strip(string str)
		{
			if (str != null)
			{
				if (User.Settings["RemoveBrackets"])
				{
					const string RegExPattern = @"\s[\(\[].*?(version|edition|deluxe|explicit).*?[\)\]]";
					str = Regex.Replace(str, RegExPattern, string.Empty, RegexOptions.IgnoreCase);
				}

				if (User.Settings["RemoveFeaturing"])
				{
					const string RegExPattern = @"\s([\(\[])?\s(feat(\.)?|ft(\.)?|featuring)(.*?[\)\]]|.*)";
					str = Regex.Replace(str, RegExPattern, string.Empty, RegexOptions.IgnoreCase);
				}

				str = Regex.Replace(str, @"[\u202F\u00A0\u2005\u2009\u200B\u2060\u3000\uFEFF]", " ");  // Replace uncommon spaces
				str = Regex.Replace(str, @"…", "...");			// Replace single sign elipsis with 3 dots.	https://en.wikipedia.org/wiki/Ellipsis
				str = Regex.Replace(str, @"[‘’ʼ]", "'");		// Replace uncommon apostrophe
				str = Regex.Replace(str, @"[‐‑−－]", "-");		// Replace uncommon dashes					https://www.cs.tut.fi/~jkorpela/dashes.html
				str = str.Trim();
			}

			return str;
		}
	}
}
