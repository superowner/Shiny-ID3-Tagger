//-----------------------------------------------------------------------
// <copyright file="Capitalize.cs" company="Shiny Id3 Tagger">
//	 Copyright (c) Shiny Id3 Tagger. All rights reserved.
// </copyright>
// <author>ShinyId3Tagger Team</author>
// <summary>Capitalize a given string according to the rules #1, #2 and #4 taken from http://aitech.ac.jp/~ckelly/midi/help/caps.html</summary>
//-----------------------------------------------------------------------

namespace GlobalNamespace
{
	using System;
	using System.Linq;
	using System.Text.RegularExpressions;

	public partial class Form1
	{
		private static string Capitalize(string str)
		{
			if (!string.IsNullOrWhiteSpace(str))
			{
				string[] lowercase =
				{
					"a", "an", "the", "and", "but", "or", "nor", "at", "by", "for", "from", "in",
					"into", "of", "off", "on", "onto", "out", "over", "to", "up", "with", "as"
				};

				// Use regex to extract all words from str. Use custom word seperators
				// Edge case "P!nk" is not split into two words since the exclamation mark is not included in seperator list
				// Edge case "Mind the G.A.T.T" is not split into several words since the point is not included in seperator list
				const string Sep = @" ,&/\(\)\{\}\[\]\><";
				MatchCollection words = Regex.Matches(str, "(?<=^|([" + Sep + "])+)[^" + Sep + "]+?(?=$|([" + Sep + "])+)");

				int firstIndex = words[0].Index;
				int lastIndex = words[words.Count - 1].Index;

				foreach (Match w in words)
				{
					// ALL CAPS words should stay in uppercase. Except the "A"
					string word = w.Value;
					if (word == word.ToUpperInvariant() && word != "A")
					{
						continue;
					}

					// Search special words and lowercase them. But ignore the first/last word. They must be capitalized
					// "and" => stays "and"			"And" => goes "and"		"AND" => stays "AND"
					if (lowercase.Contains(word.ToLower(Runtime.CultEng)) && w.Index != firstIndex && w.Index != lastIndex)
					{
						string newWord = word.ToLower(Runtime.CultEng);
						str = str.Remove(w.Index, w.Length).Insert(w.Index, newWord);
						continue;
					}

					// All remaining lowercase only words are capitalized
					// edge cases like "iTunes" or "CHURCHES" stay as they are since they are not "lowercase only"
					if (word == word.ToLower(Runtime.CultEng))
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
