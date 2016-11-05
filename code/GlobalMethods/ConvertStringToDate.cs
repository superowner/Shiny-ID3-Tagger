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
		private DateTime ConvertStringToDate(string dateString)
		{
			if (!string.IsNullOrWhiteSpace(dateString) && dateString != "0")
			{
				// Valid date formats according to http://id3.org/id3v2.4.0-structure
				string[] formats =
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
					"MM.dd.yyyy HH:mm:ss"
				};
	
				DateTime resultDate = new DateTime();
				if (DateTime.TryParseExact(dateString, formats, Runtime.CultEng, DateTimeStyles.None, out resultDate))
				{
					return resultDate;
				}
				else
				{
					string message = string.Format(Runtime.CultEng, "ERROR: Could not convert \"" + dateString + "\" to a date");
					this.Log("error", new[] { message });					
				}
			}
			
			return default(DateTime);
		}
	}
}
