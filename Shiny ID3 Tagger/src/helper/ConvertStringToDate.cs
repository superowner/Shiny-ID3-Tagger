//-----------------------------------------------------------------------
// <copyright file="ConvertStringToDate.cs" company="Shiny ID3 Tagger">
// Copyright (c) Shiny ID3 Tagger. All rights reserved.
// </copyright>
// <author>ShinyId3Tagger Team</author>
// <summary>Tries to convert a given string to a dateTime object</summary>
//-----------------------------------------------------------------------

namespace Utils
{
	using System;
	using System.Globalization;
	using System.Text.RegularExpressions;
	using GlobalNamespace;
	using GlobalVariables;

	internal partial class Utils
	{
        // Valid date formats for ID3 according to http://id3.org/id3v2.4.0-structure
        // MSDN about standard dateTime formats https://msdn.microsoft.com/en-us/library/az4se3k1(v=vs.85).aspx
        internal static readonly string[] DateTimeformats =
		{
			"yyyy",
			"yyyy-MM",
			"yyyy-MM-dd",
			"yyyy-MM-dd HH",
			"yyyy-MM-dd HH:mm",
			"yyyy-MM-dd HH:mm:ss",
			"MM/dd/yyyy",
			"MM/dd/yyyy HH",
			"MM/dd/yyyy HH:mm",
			"MM/dd/yyyy HH:mm:ss",
            "MM/dd/yyyy hh:mm:ss tt",
            "M/d/YYYY h:mm:ss tt",
			"M/dd/yyyy h:mm:ss tt",
            "dd/yyyy",
            "dd.MM.yyyy HH:mm:ss",
        };

		internal static DateTime ConvertStringToDate(string dateString)
		{
			if (!string.IsNullOrWhiteSpace(dateString) && dateString != "0")
			{
				if (DateTime.TryParseExact(dateString, DateTimeformats, GlobalVariables.CultEng, DateTimeStyles.None, out DateTime resultDate))
				{
					return resultDate;
				}

				// Handle edge cases where API database' date has default value like "0000" or "0000-00-00"
				// They cannot converted to a date but we don't wanna log an error since this is too common
				// RegEx tests if dateString contains anything which is not zero, dot or dash
				Regex regEx = new Regex(@"[^\.\-0]+");

				if (regEx.IsMatch(dateString))
				{
					if ((int)User.Settings["DebugLevel"] >= 2)
					{
						string[] errorMsg =	{ "WARNING:  Could not convert \"" + dateString + "\" to a date!" };
						Form1.Instance.PrintErrorMessage(errorMsg);
					}
				}
			}

			return default(DateTime);
		}
	}
}
