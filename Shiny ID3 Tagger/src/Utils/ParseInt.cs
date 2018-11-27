//-----------------------------------------------------------------------
// <copyright file="ParseInt.cs" company="Shiny ID3 Tagger">
// Copyright (c) Shiny ID3 Tagger. All rights reserved.
// </copyright>
// <author>ShinyId3Tagger Team</author>
//-----------------------------------------------------------------------
// Reviewed and checked if all possible exceptions are prevented or handled

namespace Utils
{
	/// <summary>
	/// Represents the Utility class which holds various helper functions
	/// </summary>
	internal partial class Utils
	{
		/// <summary>
		/// Parses a given string to an integer.
		/// Returns 0 if a NULL value is passed
		/// </summary>
		/// <param name="str">String to convert</param>
		/// <returns>The converted int</returns>
		internal static long ParseLong(string str)
		{
			return long.TryParse(str, out long result) ? result : 0;
		}
	}
}
