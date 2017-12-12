//-----------------------------------------------------------------------
// <copyright file="LevenshteinDistance.cs" company="Shiny ID3 Tagger">
// Copyright (c) Shiny ID3 Tagger. All rights reserved.
// </copyright>
// <author>ShinyId3Tagger Team</author>
// <summary>Calculates how similar two strings are and outputs how many edits are needed to get from one string to another</summary>
//-----------------------------------------------------------------------

namespace GlobalNamespace
{
	using System;
	using System.Linq;

	public partial class Form1
	{
		private static int LevenshteinDistance(string s, string t)
		{
			if (s == t)
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
					var cost = (s[i].ToString().ToUpperInvariant() == t[j].ToString().ToUpperInvariant()) ? 0 : 1;
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
