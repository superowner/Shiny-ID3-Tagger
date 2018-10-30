//-----------------------------------------------------------------------
// <copyright file="ConvertStringToDate.cs" company="Shiny ID3 Tagger">
// Copyright (c) Shiny ID3 Tagger. All rights reserved.
// </copyright>
// <author>ShinyId3Tagger Team</author>
//-----------------------------------------------------------------------

namespace Utils
{
	using System;
	using System.Globalization;
	using System.Text.RegularExpressions;
	using GlobalVariables;
	using Shiny_ID3_Tagger;

	/// <summary>
	/// Represents the Utility class which holds various helper functions
	/// </summary>
	internal partial class Utils
	{
		/// <summary>
		/// Valid date formats for ID3 according to http://id3.org/id3v2.4.0-structure
		/// More info on MSDN about standard dateTime formats https://msdn.microsoft.com/en-us/library/az4se3k1(v=vs.85).aspx
		/// </summary>
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

		/// <summary>Tries to convert a given string to a dateTime object</summary>
		/// <param name="dateString">Input string which represents a date. Must match one of the valid formats</param>
		/// <returns>The DateTime object representing the converted string value</returns>
		internal static DateTime ConvertStringToDate(string dateString)
		{
			if (!string.IsNullOrWhiteSpace(dateString) && dateString != "0")
			{
				if (DateTime.TryParseExact(dateString, DateTimeformats, GlobalVariables.CultEng, DateTimeStyles.None, out DateTime resultDate))
				{
					return resultDate;
				}

				// Handle edge cases where API database' date fields are filled with default values like "0000" or "0000-00-00"
				// They can not be converted to a date but we don't wanna log an error since this is too common
				// RegEx tests if dateString contains anything which is not zero, dot or dash
				Regex regEx = new Regex(@"[^\.\-0]+");

				if (regEx.IsMatch(dateString))
				{
					string[] errorMsg =	{ "WARNING:  Could not convert \"" + dateString + "\" to a date!" };
					Form1.Instance.RichTextBox_LogMessage(errorMsg, 2);
				}
			}

			return default;
		}
	}
}
