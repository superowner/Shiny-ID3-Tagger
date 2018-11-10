//-----------------------------------------------------------------------
// <copyright file="Program.cs" company="Shiny ID3 Tagger">
// Copyright (c) Shiny ID3 Tagger. All rights reserved.
// </copyright>
// <author>ShinyId3Tagger Team</author>
//-----------------------------------------------------------------------

namespace Updater
{
	using System;
	using System.Collections.Generic;
	using System.Linq;
	using System.Text;
	using System.Threading.Tasks;

	/// <summary>
	/// The Program class is the default class to start a C# program
	/// </summary>
	public class Program
	{
		/// <summary>
		/// Main program which is called first when started
		/// </summary>
		/// <param name="args">Passed commandline arguments</param>
		private static void Main(string[] args)
		{
			Console.WriteLine("Hello world");
			Console.ReadKey();

			// Updater checks if Shiny Id3 Tagger.exe is not running. Retries 3 times with 1s delay
			// Updater checks if Shiny Id3 Tagger.exe is present in argument folder
			// Updater copies all files from temp to program folder. Including itself
			// Throws an error if file could not be copied
			// But makes exceptions according to a blacklist
			// 	- accounts.user.json
			// 	- settings.user.json
			// Verify all files size/date is same in temp folder and program folder
			// Updater starts Shiny Id3 Tagger.exe and closes itself immediatly
		}
	}
}
