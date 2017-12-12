//-----------------------------------------------------------------------
// <copyright file="WellFormedCsvValue.cs" company="Shiny Id3 Tagger">
// Copyright (c) Shiny Id3 Tagger. All rights reserved.
// </copyright>
// <author>ShinyId3Tagger Team</author>
// <summary>Handles null values when generating a CSV export. Encloses every value with double quotes</summary>
//-----------------------------------------------------------------------

namespace GlobalNamespace
{
	using System;

	public partial class Form1
	{
		private static string WellFormedCsvValue(object cell)
		{
			string value = cell != null ? cell.ToString() : string.Empty;
			value = value.Replace("\r\n", "\n");
			value = value.Replace("\"", "\"\"");
			value = "\"" + value + "\"";
			return value;
		}
	}
}
