//-----------------------------------------------------------------------
// <copyright file="CalcStringSimilarity.cs" company="Shiny ID3 Tagger">
// Copyright (c) Shiny ID3 Tagger. All rights reserved.
// </copyright>
// <author>ShinyId3Tagger Team</author>
//-----------------------------------------------------------------------
// Reviewed and checked if all possible exceptions are prevented or handled

namespace Utils
{
	using System.Linq;

	/// <summary>
	/// Represents the Utility class which holds various helper functions
	/// </summary>
	internal partial class Utils
	{
		/// <summary>
		/// Calculates how similar two strings are using the so called Levenshtein method
		/// Outputs how many edits are needed to get from one string to another
		/// For performance reasons you cannot compare strings longer than 32767 characters
		/// </summary>
		/// <param name="s">First string to compare</param>
		/// <param name="t">Second string to compare</param>
		/// <returns>Number of edits as a measurement for similarity</returns>
		internal static int CalcStringSimilarity(string s, string t)
		{
			if (s == t)
			{
				return 0;
			}

			if (s.Length > short.MaxValue || t.Length > short.MaxValue)
			{
				return 0;
			}

			if (s.Length == 0)
			{
				return t.Length;
			}

			if (t.Length == 0)
			{
				return s.Length;
			}

			int[] v0 = new int[t.Length + 1];
			int[] v1 = new int[t.Length + 1];

			for (int i = 0; i < v0.Length; i++)
			{
				v0[i] = i;
			}

			for (int i = 0; i < s.Length; i++)
			{
				v1[0] = i + 1;

				for (int j = 0; j < t.Length; j++)
				{
					int cost = (s[i].ToString().ToUpperInvariant() == t[j].ToString().ToUpperInvariant()) ? 0 : 1;
					v1[j + 1] = new[] { v1[j] + 1, v0[j + 1] + 1, v0[j] + cost }.Min();
				}

				for (int j = 0; j < v0.Length; j++)
				{
					v0[j] = v1[j];
				}
			}

			return v1[t.Length];
		}
	}
}
