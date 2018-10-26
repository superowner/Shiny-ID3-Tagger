//-----------------------------------------------------------------------
// <copyright file="ParseInt.cs" company="Shiny ID3 Tagger">
// Copyright (c) Shiny ID3 Tagger. All rights reserved.
// </copyright>
// <author>ShinyId3Tagger Team</author>
//-----------------------------------------------------------------------

namespace Utils
{
	/// <summary>
	/// Represents the Utility class which holds various helper functions
	/// </summary>
	internal partial class Utils
	{
		/// <summary>
		/// Parses a given string to an integer.Returns 0 if a NULL value is passed
		/// </summary>
		/// <param name="str">String to convert</param>
		/// <param name="defaultValue">Default value </param>
		/// <returns>The converted int</returns>
		internal static int ParseInt(string str, int defaultValue = 0)
		{
			return int.TryParse(str, out int result) ? result : defaultValue;
		}
	}
}
