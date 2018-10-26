//-----------------------------------------------------------------------
// <copyright file="SanitizeForCsv.cs" company="Shiny ID3 Tagger">
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
		/// Converts a cell value to a string when generating a CSV export
		/// Treats null values as empty string
		/// Encloses every value with double quotes
		/// </summary>
		/// <param name="cell">dataGridView cell which has to converted to string</param>
		/// <returns>The resulting string</returns>
		internal static string SanitizeForCsv(object cell)
		{
			string value = cell != null ? cell.ToString() : string.Empty;
			value = value.Replace("\r\n", "\n");
			value = value.Replace("\"", "\"\"");
			value = "\"" + value + "\"";
			return value;
		}
	}
}
