//-----------------------------------------------------------------------
// <copyright file="StartUpdateClient.cs" company="Shiny ID3 Tagger">
// Copyright (c) Shiny ID3 Tagger. All rights reserved.
// </copyright>
// <author>ShinyId3Tagger Team</author>
//-----------------------------------------------------------------------
// Reviewed and checked if all possible exceptions are prevented or handled

namespace Utils
{
	using System;
	using System.Diagnostics;
	using System.IO;
	using System.IO.Pipes;
	using System.Threading.Tasks;
	using System.Windows.Forms;
	using Exceptions;
	using GlobalVariables;
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
			string updateFolder = GlobalVariables.TempDir + @"shiny-id3-tagger-update\";
			string updateExeFullPath = updateFolder + "UpdateClient.exe";

			try
			{
				// Check if the UpdateClient.exe exists in the update folder
				if (File.Exists(updateExeFullPath) == false)
				{
					throw new UpdateClientException("Update program not found");
				}

				// Start UpdateClient.exe in temp folder (= new version from GitHub)
				System.Diagnostics.Process updateClient = new Process();
				updateClient.StartInfo.FileName = updateExeFullPath;
				updateClient.StartInfo.UseShellExecute = false;
				updateClient.StartInfo.CreateNoWindow = true;
				updateClient.Start();

				// RECEIVE
				// Set up a named pipe server and listen if UpdateClient has started
				NamedPipeServerStream server = new NamedPipeServerStream("UpdateClientToMainApp");
				server.WaitForConnection();

				StreamReader reader = new StreamReader(server);

				while (true)
				{
					// If UpdateClient responds with the proper string, then exit this loop and continue
					if (reader.ReadLine() == "UpdateClient is running")
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

				// Reply to updater the path of this exe and the own process ID
				writer.WriteLine("path = " + mainAppProcess.MainModule.FileName);
				writer.WriteLine("id = " + mainAppProcess.Id);
				writer.Flush();

				Application.Exit();
			}
			catch (Exception ex)
			{
				string[] warningMsg =
				{
							"WARNING:  Could not update program!",
							"Message:  " + ex.Message,
				};
				Form1.Instance.RichTextBox_LogMessage(warningMsg, 3);
				return;
			}
		}
	}
}
