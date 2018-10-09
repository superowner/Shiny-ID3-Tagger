//-----------------------------------------------------------------------
// <copyright file="WellFormedCsvValue.cs" company="Shiny ID3 Tagger">
// Copyright (c) Shiny ID3 Tagger. All rights reserved.
// </copyright>
// <author>ShinyId3Tagger Team</author>
// <summary>Handles null values when generating a CSV export. Encloses every value with double quotes</summary>
//-----------------------------------------------------------------------

namespace Utils
{
	public partial class Utils
	{
		public static string WellFormedCsvValue(object cell)
		{
			string value = cell != null ? cell.ToString() : string.Empty;
			value = value.Replace("\r\n", "\n");
			value = value.Replace("\"", "\"\"");
			value = "\"" + value + "\"";
			return value;
		}
	}
}
