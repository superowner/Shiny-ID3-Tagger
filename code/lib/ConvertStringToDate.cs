//-----------------------------------------------------------------------
// <copyright file="ConvertStringToDate.cs" company="Shiny Id3 Tagger">
//	 Copyright (c) Shiny Id3 Tagger. All rights reserved.
// </copyright>
// <author>ShinyId3Tagger Team</author>
// <summary>Tries to convert a given string to a datetime object</summary>
//-----------------------------------------------------------------------

namespace GlobalNamespace
{
	using System;
	using System.Globalization;
	using System.Linq;

	public partial class Form1
	{
		// Valid date formats for ID3 according to http://id3.org/id3v2.4.0-structure
		// MSDN about standard datetime formats https://msdn.microsoft.com/en-us/library/az4se3k1(v=vs.85).aspx
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
				DateTime resultDate = new DateTime();
				if (DateTime.TryParseExact(dateString, dateTimeformats, cultEng, DateTimeStyles.None, out resultDate))
				{
					return resultDate;
				}
				else
				{
					if (User.Settings["DebugLevel"] >= 2)
					{
						string[] errorMsg =	{ "WARNING:  Could not convert \"" + dateString + "\" to a date!" };
						this.PrintLogMessage("error", errorMsg);
					}
				}
			}
			
			return default(DateTime);
		}
	}
}
