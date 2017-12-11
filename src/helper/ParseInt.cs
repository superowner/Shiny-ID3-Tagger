//-----------------------------------------------------------------------
// <copyright file="IsValidInt.cs" company="Shiny Id3 Tagger">
//	 Copyright (c) Shiny Id3 Tagger. All rights reserved.
// </copyright>
// <author>ShinyId3Tagger Team</author>
// <summary>Checks and parses a given string to an integer. Can handle NULL values</summary>
//-----------------------------------------------------------------------

namespace GlobalNamespace
{
	using System;

	public partial class Form1
	{
		public static int ParseInt(string str, int defaultValue = 0)
		{
			return Int32.TryParse(str, out int result) ? result : defaultValue;
		}
	}
}
