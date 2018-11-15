//-----------------------------------------------------------------------
// <copyright file="StartUpdateClient.cs" company="Shiny ID3 Tagger">
// Copyright (c) Shiny ID3 Tagger. All rights reserved.
// </copyright>
// <author>ShinyId3Tagger Team</author>
//-----------------------------------------------------------------------

namespace Utils
{
	using System;
	using System.Diagnostics;
	using System.IO;
	using System.IO.Pipes;
	using System.Threading.Tasks;
	using System.Windows.Forms;
	using Shiny_ID3_Tagger;

	/// <summary>
	/// Represents the Utility class which holds various helper functions
	/// <seealso href="https://stackoverflow.com/a/13806752/935614"/>
	/// </summary>
	internal partial class Utils
	{
		/// <summary>
		/// Communicate with updater (UpdateClient.exe)
		/// </summary>
		internal static void StartUpdateClient()
		{
			string updateFolder = Path.GetTempPath() + @"shiny-id3-tagger-update\";
			string updateExeFullPath = updateFolder + "UpdateClient.exe";

			// Check if the UpdateClient.exe exists in the update folder
			if (File.Exists(updateExeFullPath) == false)
			{
				string[] warningMsg =
				{
							"WARNING:  Could not update program!",
							"Message:  Update program not found",
							"Path:     " + updateExeFullPath,
				};
				Form1.Instance.RichTextBox_LogMessage(warningMsg, 3);
				return;
			}

			// Start UpdateClient.exe in temp folder (= new version from GitHub)
			// Process.Start(updateExeFullPath);
			System.Diagnostics.Process updateClient = new System.Diagnostics.Process();
			updateClient.StartInfo.FileName = updateExeFullPath;
			updateClient.StartInfo.UseShellExecute = false;
			updateClient.StartInfo.CreateNoWindow = true;
			updateClient.Start();

			// RECEIVE
			// Set up a named pipe server and listen if UpdateClient has started
			NamedPipeServerStream server = new NamedPipeServerStream("UpdateClientToMainApp");
			server.WaitForConnection();

			StreamReader reader = new StreamReader(server);

			// Start stopwatch to throw an error if client does not respond in time
			Stopwatch sw = new Stopwatch();
			sw.Start();

			while (true)
			{
				string message = reader.ReadLine();

				string[] debugMsg =
				{
					"Debug:    Received message from UpdateClient",
					"Message:  " + message,
				};
				Form1.Instance.RichTextBox_LogMessage(debugMsg, 4);

				// If UpdateClient responds with the proper string, then exit this loop and continue
				if (message == "UpdateClient is running")
				{
					break;
				}
			}

			// SEND
			// Set up a named pipe client and send infos from MainApp
			NamedPipeClientStream client = new NamedPipeClientStream("MainAppToUpdateClient");
			client.Connect();

			StreamWriter writer = new StreamWriter(client);

			Process mainAppProcess = Process.GetCurrentProcess();

			// Reply the path of this exe and the own process ID
			writer.WriteLine("path = " + mainAppProcess.MainModule.FileName);
			writer.WriteLine("id = " + mainAppProcess.Id);
			writer.Flush();

			Application.Exit();
		}
	}
}
