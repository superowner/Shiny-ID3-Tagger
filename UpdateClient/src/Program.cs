//-----------------------------------------------------------------------
// <copyright file="Program.cs" company="Shiny ID3 Tagger">
// Copyright (c) Shiny ID3 Tagger. All rights reserved.
// </copyright>
// <author>ShinyId3Tagger Team</author>
//-----------------------------------------------------------------------

namespace UpdateClient
{
	using System;
	using System.Collections.Generic;
	using System.IO;
	using System.IO.Pipes;
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
			var client = new NamedPipeClientStream("Shiny_Id3_Tagger_UpdateClient");

			client.Connect();

			StreamReader reader = new StreamReader(client);
			StreamWriter writer = new StreamWriter(client);

			// Start to exchange infos betwen main app and updater
			writer.WriteLine("UpdateClient is running");
			writer.Flush();

			Console.WriteLine(args[0]);
			//Console.WriteLine("Shiny ID3 Tagger process ID: " + args[1]);

			while (!reader.EndOfStream)
			{
				var message = reader.ReadLine();
				Console.WriteLine("Received message: " + message);

				if (message == "Main app is shutting down")
				{
					// Updater checks if Shiny Id3 Tagger.exe is present in argument folder
					Console.WriteLine("Updater checks if Shiny Id3 Tagger.exe is present in argument folder");

					// Wait until process ID is not found anymore
					Console.WriteLine("Wait until process ID is not found anymore");

					// Updater copies all files from temp to program folder. Including itself
					Console.WriteLine("Updater copies all files from temp to program folder. Including itself");

					// Throws an error if file could not be copied
					// But makes exceptions according to a blacklist
					// 	- accounts.user.json
					// 	- settings.user.json
					// Verify all files size/date is same in temp folder and program folder

					// Updater starts Shiny Id3 Tagger.exe and closes itself immediatly
					Console.WriteLine("Updater starts Shiny Id3 Tagger.exe and closes itself immediatly");
				}
			}

			Console.ReadLine();
		}
	}
}
