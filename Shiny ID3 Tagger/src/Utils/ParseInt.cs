//-----------------------------------------------------------------------
// <copyright file="ParseInt.cs" company="Shiny ID3 Tagger">
// Copyright (c) Shiny ID3 Tagger. All rights reserved.
// </copyright>
// <author>ShinyId3Tagger Team</author>
// <summary>Checks and parses a given string to an integer. Can handle NULL values</summary>
//-----------------------------------------------------------------------

namespace Utils
{
	internal partial class Utils
	{
		internal static int ParseInt(string str, int defaultValue = 0)
		{
			return int.TryParse(str, out int result) ? result : defaultValue;
		}
	}
}
