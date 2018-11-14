//-----------------------------------------------------------------------
// <copyright file="Program.cs" company="Shiny ID3 Tagger">
// Copyright (c) Shiny ID3 Tagger. All rights reserved.
// </copyright>
// <author>ShinyId3Tagger Team</author>
//-----------------------------------------------------------------------
// TODO: Find a way to cancel while loop if main app does not shutdown in time
//       A break after 2 seconds in loop does not work for whatever reason
// TODO: Implement a way to signal main app that copying failed

namespace UpdateClient
{
	using System;
	using System.Diagnostics;
	using System.IO;
	using System.IO.Pipes;
	using System.Linq;
	using System.Text.RegularExpressions;
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
			string regExPattern = "(?<key>.*?) = (?<value>.*)";
			string mainAppFullPath = null;
			string mainAppProcessId = null;

			// SEND
			// Set up a named pipe client and send that UpdateClient has started
			NamedPipeClientStream client = new NamedPipeClientStream("UpdateClientToMainApp");
			client.Connect();

			StreamWriter writer = new StreamWriter(client);

			// Send message to main app that we are running. Don't change this string
			writer.WriteLine("UpdateClient is running");
			writer.Flush();

			// RECEIVE
			// Set up a named pipe server and listen for infos from MainApp
			NamedPipeServerStream server = new NamedPipeServerStream("MainAppToUpdateClient");
			server.WaitForConnection();

			StreamReader reader = new StreamReader(server);

			while (true)
			{
				string message = reader.ReadLine();

				// Parse which message was send
				if (message != null)
				{
					Match match = Regex.Match(message, regExPattern);
					if (match.Success)
					{
						switch (match.Groups["key"].Value)
						{
							case "path":
								mainAppFullPath = match.Groups["value"].Value;
								break;
							case "id":
								mainAppProcessId = match.Groups["value"].Value;
								break;
						}
					}
				}

				// Check if main app is closed
				if (mainAppFullPath != null && mainAppProcessId != null)
				{
					int id = int.TryParse(mainAppProcessId, out int result) ? result : -1;

					Task.Delay(1000);

					Process[] processlist = Process.GetProcesses();
					Process process = processlist.FirstOrDefault(pr => pr.Id == id);

					if (process == null)
					{
						break;
					}
				}
			}

			try
			{
				// Updater copies all files from own folder (= temp) to main app folder
				DirectoryInfo diSource = new DirectoryInfo(AppDomain.CurrentDomain.BaseDirectory);
				DirectoryInfo diTarget = new DirectoryInfo(Path.GetDirectoryName(mainAppFullPath));
				CopyDirectory.CopyAll(diSource, diTarget);

				// TODO: Dont copy this. Pass a blacklist to CopyAll
				// 	- accounts.user.json
				// 	- settings.user.json
				Process.Start(mainAppFullPath);
				Environment.Exit(0);
			}
			catch (Exception ex)
			{
				Console.WriteLine(ex.Message);
				Console.ReadLine();
			}
		}
	}
}
