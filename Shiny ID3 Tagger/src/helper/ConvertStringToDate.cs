//-----------------------------------------------------------------------
// <copyright file="ConvertStringToDate.cs" company="Shiny ID3 Tagger">
// Copyright (c) Shiny ID3 Tagger. All rights reserved.
// </copyright>
// <author>ShinyId3Tagger Team</author>
// <summary>Tries to convert a given string to a dateTime object</summary>
//-----------------------------------------------------------------------

namespace GlobalNamespace
{
	using System;
	using System.Globalization;
	using System.Text.RegularExpressions;

	public partial class Form1
	{
		// Valid date formats for ID3 according to http://id3.org/id3v2.4.0-structure
		// MSDN about standard dateTime formats https://msdn.microsoft.com/en-us/library/az4se3k1(v=vs.85).aspx
		private static string[] dateTimeformats =
				{
					"yyyy",
					"yyyy-MM",
					"yyyy-MM-dd",
					"yyyy-MM-dd HH",
					"yyyy-MM-dd HH:mm",
					"yyyy-MM-dd HH:mm:ss",
					"dd/yyyy",
					"MM/dd/yyyy",
					"MM/dd/yyyy HH",
					"MM/dd/yyyy HH:mm",
					"MM/dd/yyyy HH:mm:ss",
					"dd.MM.yyyy HH:mm:ss"
				};

		private DateTime ConvertStringToDate(string dateString)
		{
			if (!string.IsNullOrWhiteSpace(dateString) && dateString != "0")
			{
				if (DateTime.TryParseExact(dateString, dateTimeformats, cultEng, DateTimeStyles.None, out DateTime resultDate))
				{
					return resultDate;
				}
				else
				{
					// Handle edge cases where API database' date has default value like "0000" or "0000-00-00"
					// They cannot converted to a date but we don't wanna log an error since this is too common
					// RegEx tests if dateString contains anything which is not zero, dot or dash
					Regex regEx = new Regex(@"[^\.\-0]+");

					if (regEx.IsMatch(dateString))
					{
						if ((int)User.Settings["DebugLevel"] >= 2)
						{
							string[] errorMsg =	{ "WARNING:  Could not convert \"" + dateString + "\" to a date!" };
							this.PrintLogMessage(this.rtbErrorLog, errorMsg);
						}
					}
				}
			}

			return default(DateTime);
		}
	}
}
