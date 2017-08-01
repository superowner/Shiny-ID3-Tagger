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
					const string RegExPattern = @"[\(\[\{].*?[\)\]\}]";
					str = Regex.Replace(str, RegExPattern, string.Empty, RegexOptions.IgnoreCase);
				}

				if (User.Settings["RemoveFeaturing"])
				{
					const string RegExPattern = @" ([\(\[\{])?(feat(\.)?|ft(\.)?|featuring)(.*?[\)\]\}]|.*)";
					str = Regex.Replace(str, RegExPattern, string.Empty, RegexOptions.IgnoreCase);
				}

				str = Regex.Replace(str, @"[\u202F\u00A0\u2005\u2009\u200B\u2060\u3000\uFEFF]", " ");
				str = Regex.Replace(str, @"…", "...");
				str = Regex.Replace(str, @"[‘’ʼ]", "'");
				str = Regex.Replace(str, @"[‐‑−－]", "-");		// https://www.cs.tut.fi/~jkorpela/dashes.html
				str = str.Trim();
			}

			return str;
		}
	}
}
