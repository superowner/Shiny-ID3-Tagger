//-----------------------------------------------------------------------
// <copyright file="Capitalize.cs" company="Shiny Id3 Tagger">
//	 Copyright (c) Shiny Id3 Tagger. All rights reserved.
// </copyright>
// <author>ShinyId3Tagger Team</author>
// <summary>Capitalize a given string according to the rules from the link below</summary>
// http://aitech.ac.jp/~ckelly/midi/help/caps.html
// #1	The first and last words are always capitalized, and all except the words listed below are capitalized.
// #2	These are lower-case, unless they are the first word or last word.
//		- articles: a, an, the
//		- conjunctions: and, but, or, nor
//		- prepositions that are less than five letters long: at, by, for, from, in, into, of, off, on, onto, out, over, to, up, with
//		- as (only if it is followed by a noun)
// #3	Prepositions are sometimes capitalized.
//		- Prepositions are capitalized when they are the first or last word.
//		- Prepositions that are part of two-word "phrasal verbs" (Come On, Hold On, etc....) are capitalized.
//		- Prepositions that are over four letters long. (across, after, among, beyond, ...)
// #4	These short words are capitalized.
//		- also, be, if, than, that, thus, when
//		- as (if it is followed by a verb)
//-----------------------------------------------------------------------

namespace GlobalNamespace
{
	using System.Linq;
	using System.Text.RegularExpressions;

	internal partial class Helper
	{
		private static string[] lowercase =
		{
			"a", "an", "the", "and", "but", "or", "nor", "at", "by", "for", "from", "in",
			"into", "of", "off", "on", "onto", "out", "over", "to", "up", "with", "as"
		};

		internal static string Capitalize(string str)
		{
			// If string is not empty and user setting "AutoCapitalize" is set to true
			if (User.Settings["AutoCapitalize"] && !string.IsNullOrWhiteSpace(str))
			{
				// Use RegEx to extract all words from str. Use custom word separators
				// Edge case "P!nk" is not split into two words since the exclamation mark is not included in separator list
				// Edge case "Mind the G.A.T.T" is not split into several words since the point is not included in separator list
				const string Sep = @" ,&/\(\)\{\}\[\]\><";
				MatchCollection words = Regex.Matches(str, "(?<=^|([" + Sep + "])+)[^" + Sep + "]+?(?=$|([" + Sep + "])+)");

				int firstIndex = words[0].Index;
				int lastIndex = words[words.Count - 1].Index;

				foreach (Match w in words)
				{
					string word = w.Value;

					// Search special words and lowercase them
					// "and" => stays "and"			"And" => goes "and"		"AND" => stays "AND"
					// But ignore the first/last word. They must always be capitalized
					if (lowercase.Contains(word.ToLowerInvariant()) && w.Index != firstIndex && w.Index != lastIndex)
					{
						string newWord = word.ToLowerInvariant();
						str = str.Remove(w.Index, w.Length).Insert(w.Index, newWord);
						continue;
					}

					// All remaining lowercase words are capitalized
					// But if the whole string contains any uppercase character you can assume the API is case sensitive. Do nothing then
					if (word == word.ToLowerInvariant() && !str.Any(char.IsUpper))
					{
						string newWord = word.First().ToString().ToUpperInvariant() + word.Substring(1);
						str = str.Remove(w.Index, w.Length).Insert(w.Index, newWord);
					}
				}
			}

			return str;
		}
	}
}
