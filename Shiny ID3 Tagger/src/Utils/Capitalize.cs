//-----------------------------------------------------------------------
// <copyright file="Capitalize.cs" company="Shiny ID3 Tagger">
// Copyright (c) Shiny ID3 Tagger. All rights reserved.
// </copyright>
// <author>ShinyId3Tagger Team</author>
//-----------------------------------------------------------------------*/
// Reviewed and checked if all possible exceptions are prevented or handled

namespace Utils
{
	using System.Linq;
	using System.Text.RegularExpressions;
	using GlobalVariables;

	/// <summary>
	/// Represents the Utility class which holds various helper functions
	/// </summary>
	internal partial class Utils
	{
		private static string[] lowercaseWords =
		{
			"a", "an", "the", "and", "but", "or", "nor", "at", "by", "for", "from", "in",
			"into", "of", "off", "on", "onto", "out", "over", "to", "up", "with", "as",
		};

		/// <summary>
		/// Capitalize a given string according to these rules. Only rule #3 is not implemented
		/// <seealso href="http://aitech.ac.jp/~ckelly/midi/help/caps.html"/>
		/// #1	The first and last words are always capitalized, and all except the words listed below are capitalized.
		/// #2	These are lower-case, unless they are the first word or last word.
		/// 	- articles: a, an, the
		/// 	- conjunctions: and, but, or, nor
		///  	- prepositions that are less than five letters long: at, by, for, from, in, into, of, off, on, onto, out, over, to, up, with
		///  	- as (only if it is followed by a noun)
		///  #4	These short words are capitalized.
		///  	- also, be, if, than, that, thus, when
		///  	- as (if it is followed by a verb)
		/// </summary>
		/// <param name="originalStr">The string which should be capitalized</param>
		/// <returns>The capitalized string</returns>
		internal static string Capitalize(string originalStr)
		{
			string outputStr = originalStr;

			// If string is not null/empty and user setting "AutoCapitalize" is set to true
			if ((bool)User.Settings["AutoCapitalize"] && string.IsNullOrWhiteSpace(originalStr) == false)
			{
				// Use RegEx to extract all words from str. Use custom word separators
				// Edge case "P!nk" is not split into two words since the exclamation mark is not included in separator list
				// Edge case "Mind the G.A.T.T" is not split into several words since the point is not included in separator list
				const string Sep = @" ,&/\(\)\{\}\[\]\><";
				MatchCollection words = Regex.Matches(originalStr, "(?<=^|([" + Sep + "])+)[^" + Sep + "]+?(?=$|([" + Sep + "])+)");

				if (words?.Count > 0)
				{
					int firstIndex = words[0].Index;
					int lastIndex = words[words.Count - 1].Index;

					foreach (Match w in words)
					{
						string word = w.Value;

						// Rule #2
						// Search special words and lowercase them
						// But ignore first/last word. They must always be capitalized
						// Example: "and" => stays "and"			"And" => goes "and"		"AND" => stays "AND"
						if (lowercaseWords.Contains(word.ToLowerInvariant()) && w.Index != firstIndex && w.Index != lastIndex)
						{
							outputStr = originalStr.Remove(w.Index, w.Length).Insert(w.Index, word.ToLowerInvariant());
							continue;
						}

						// Rule #1 and #4
						// All remaining lowercase words are capitalized
						// But if the whole string contains any uppercase character you can assume the API is case sensitive. Do nothing then
						if (word == word.ToLowerInvariant() && originalStr.Any(char.IsUpper) == false)
						{
							string newWord = word.First().ToString().ToUpperInvariant() + word.Substring(1);
							outputStr = originalStr.Remove(w.Index, w.Length).Insert(w.Index, newWord);
						}
					}
				}
			}

			return outputStr;
		}
	}
}
