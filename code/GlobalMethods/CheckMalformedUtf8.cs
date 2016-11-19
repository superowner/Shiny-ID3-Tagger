﻿//-----------------------------------------------------------------------
// <copyright file="CheckMalformedUtf8.cs" company="Shiny Id3 Tagger">
//	 Copyright (c) Shiny Id3 Tagger. All rights reserved.
// </copyright>
// <author>ShinyId3Tagger Team</author>
// <summary>Checks if a given string needs to be converted to UTF-8</summary>
//-----------------------------------------------------------------------

namespace GlobalNamespace
{
	using System;
	using System.Collections.Generic;
	using System.Linq;
	using System.Text;
	using System.Text.RegularExpressions;
	
	public partial class Form1
	{
		public static string CheckMalformedUtf8(string data)
		{
			Match match = CreateRegex().Match(data);
			if (match.Success)
			{
				return Encoding.UTF8.GetString(Encoding.GetEncoding("iso-8859-1").GetBytes(data));
			}
			else
			{
				return data;
			}
		}
		
		public static Regex CreateRegex()
		{
			const string Specials = "ÀÁÂÃÄÅÆÇÈÉÊËÌÍÎÏÐÑÒÓÔÕÖ×ØÙÚÛÜÝÞßàáâãäåæçèéêëìíîïðñòóôõö";

			List<string> flags = new List<string>();
			foreach (char c in Specials)
			{
				string interpretedAsLatin1 = Encoding.GetEncoding("iso-8859-1").GetString(Encoding.UTF8.GetBytes(c.ToString())).Trim();		
				if (interpretedAsLatin1.Length > 0)
				{
					flags.Add(interpretedAsLatin1);
				}
			}

			string regex = string.Empty;
			foreach (string s in flags)
			{
				if (regex.Length > 0)
				{
					regex += '|';
				}
				
				regex += s;
			}
			
			return new Regex("(" + regex + ")");
		}
	}
}
