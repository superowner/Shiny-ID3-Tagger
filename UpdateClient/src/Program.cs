//-----------------------------------------------------------------------
// <copyright file="Program.cs" company="Shiny ID3 Tagger">
// Copyright (c) Shiny ID3 Tagger. All rights reserved.
// </copyright>
// <author>ShinyId3Tagger Team</author>
//-----------------------------------------------------------------------
// TODO: Find a way to cancel while loop if main app does not shutdown in time
//       A break after 2 seconds in loop does not work for whatever reason

namespace UpdateClient
{
	using System;
	using System.Diagnostics;
	using System.IO;
	using System.IO.Pipes;
	using System.Linq;
	using System.Text.RegularExpressions;
	using System.Threading.Tasks;
	using Utils;

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
			string mainAppFullPath = null;
			string mainAppProcessId = null;

			try
			{
				// Redirect all console output to the file "update.log"
				FileStream updateLogFileStream = new FileStream(AppDomain.CurrentDomain.BaseDirectory + "update.log", FileMode.Append);
				StreamWriter updateLogStreamwriter = new StreamWriter(updateLogFileStream) { AutoFlush = true };
				Console.SetOut(updateLogStreamwriter);
				Console.SetError(updateLogStreamwriter);

				Utils.ConsoleWriteLine("UpdateClient started");

				// SEND MESSAGE TO MAIN APP
				Utils.ConsoleWriteLine("UpdateClient sets up the named pipe \"UpdateClientToMainApp\"");
				NamedPipeClientStream client = new NamedPipeClientStream("UpdateClientToMainApp");

				Utils.ConsoleWriteLine("UpdateClient connects to main app");
				client.Connect();

				StreamWriter writer = new StreamWriter(client);

				Utils.ConsoleWriteLine("UpdateClient send message to main app that UpdateClient is running");
				writer.WriteLine("UpdateClient is running");
				writer.Flush();

				// RECEIVE MESSAGE FROM MAIN APP
				Utils.ConsoleWriteLine("UpdateClient sets up the named pipe \"MainAppToUpdateClient\"");
				NamedPipeServerStream server = new NamedPipeServerStream("MainAppToUpdateClient");

				Utils.ConsoleWriteLine("UpdateClient waits for main app to connect");
				server.WaitForConnection();

				StreamReader reader = new StreamReader(server);

				while (true)
				{
					string message = reader.ReadLine();

					// Parse which message was send
					if (message != null)
					{
						Utils.ConsoleWriteLine("UpdateClient received the message: \"" + message + "\"");

						Match match = Regex.Match(message, "(?<key>.*?) = (?<value>.*)");
						if (match.Success)
						{
							switch (match.Groups["key"].Value)
							{
								case "path":
									mainAppFullPath = match.Groups["value"].Value;
									Utils.ConsoleWriteLine("Main app full path is: " + mainAppFullPath);
									break;
								case "id":
									mainAppProcessId = match.Groups["value"].Value;
									Utils.ConsoleWriteLine("Main app process ID is: " + mainAppProcessId);
									break;
							}
						}
					}

					// Check if main app is closed
					if (mainAppFullPath != null && mainAppProcessId != null)
					{
						int id = int.TryParse(mainAppProcessId, out int result) ? result : -1;

						Task.Delay(1000);

						Utils.ConsoleWriteLine("UpdateClient checks if main app process is closed");

						Process[] processlist = Process.GetProcesses();
						Process process = processlist.FirstOrDefault(pr => pr.Id == id);

						if (process == null)
						{
							break;
						}
					}
				}

				Utils.ConsoleWriteLine("UpdateClient starts to copy all files from update folder (source) to main app folder (target)");
				Utils.ConsoleWriteLine("Source folder: \"" + AppDomain.CurrentDomain.BaseDirectory + "\"");
				Utils.ConsoleWriteLine("Target folder: \"" + Path.GetDirectoryName(mainAppFullPath) + "\"");

				DirectoryInfo diSource = new DirectoryInfo(AppDomain.CurrentDomain.BaseDirectory);
				DirectoryInfo diTarget = new DirectoryInfo(Path.GetDirectoryName(mainAppFullPath));

				Utils.CopyDirectory(diSource, diTarget);
				Utils.ConsoleWriteLine("UpdateClient finished copying");

				Utils.ConsoleWriteLine("UpdateClient starts main app: " + mainAppFullPath);
				Process.Start(mainAppFullPath);

				Utils.ConsoleWriteLine("UpdateClient shuts down");
				Console.WriteLine("SUCCESS");
			}
			catch (Exception ex)
			{
				Utils.ConsoleWriteLine(ex.Message);
				Console.WriteLine("FAILED");
			}

			// Put no code here
		}
	}
}
