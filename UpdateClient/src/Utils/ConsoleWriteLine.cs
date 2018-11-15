//-----------------------------------------------------------------------
// <copyright file="ConsoleWriteLine.cs" company="Shiny ID3 Tagger">
// Copyright (c) Shiny ID3 Tagger. All rights reserved.
// </copyright>
// <author>ShinyId3Tagger Team</author>
//-----------------------------------------------------------------------

namespace Utils
{
	using System;
	using System.IO;
	using System.Linq;

	/// <summary>
	/// Represents the Utils class for helper methods
	/// </summary>
	internal partial class Utils
	{
		/// <summary>
		/// Outputs message to console and adds a timestamp in front of message
		/// </summary>
		/// <param name="message">Message to write</param>
		internal static void ConsoleWriteLine(string message)
		{
			Console.WriteLine(DateTime.Now.ToString("HH:mm:ss.fff") + "\t" + message);
		}
	}
}
