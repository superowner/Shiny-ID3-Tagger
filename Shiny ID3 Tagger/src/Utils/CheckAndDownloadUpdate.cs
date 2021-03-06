﻿//-----------------------------------------------------------------------
// <copyright file="CheckAndDownloadUpdate.cs" company="Shiny ID3 Tagger">
// Copyright (c) Shiny ID3 Tagger. All rights reserved.
// </copyright>
// <author>ShinyId3Tagger Team</author>
//-----------------------------------------------------------------------

namespace Utils
{
	using System;
	using System.Diagnostics;
	using System.IO;
	using System.IO.Compression;
	using System.Linq;
	using System.Net.Http;
	using System.Threading;
	using System.Threading.Tasks;
	using System.Windows.Forms;
	using GlobalVariables;
	using Newtonsoft.Json.Linq;
	using Shiny_ID3_Tagger;

	/// <summary>
	/// Represents the Utility class which holds various helper functions
	/// </summary>
	internal partial class Utils
	{
		/// <summary>
		/// Checks for updates and downloads newest program files from GitHub
		/// <seealso href="https://developer.github.com/v3/"/>
		/// <seealso href="https://git-scm.com/docs/git-show"/>
		/// </summary>
		/// <param name="showMessages">Sets if the update messages should be printed. Default is true</param>
		/// <returns>A <see cref="Task"/>Representing the asynchronous operation.</returns>
		internal static async Task<bool> CheckAndDownloadUpdate(bool showMessages = true)
		{
			DateTimeOffset? localCommitDate = null;
			DateTimeOffset? latestReleaseDate = null;
			JObject latestReleaseJson = null;

			string updateProcessName = "UpdateClient";
			string zipFullPath = Path.GetTempPath() + "shiny-id3-tagger-update.zip";
			string updateFolder = Path.GetTempPath() + @"shiny-id3-tagger-update\";
			string updateExeFullPath = updateFolder + "UpdateClient.exe";
			string updateLogFullPath = updateFolder + "update.log";

			// ######################################################################################################################
			// Get running updater process
			Process oldUpdateProcess = (from process in Process.GetProcessesByName(updateProcessName)
									 where process.MainModule.FileName == updateExeFullPath
									 select process).FirstOrDefault();

			// Check if a running updater process was found
			if (oldUpdateProcess != null)
			{
				string[] warningMsg =
				{
					"WARNING:  Could not update program!",
					"Message:  A previous update is still running",
					"Process:  " + oldUpdateProcess.ProcessName,
				};
				Form1.Instance.RichTextBox_LogMessage(warningMsg, 3);
				return false;
			}

			// Clean up old update folders and then check if it's not present anymore
			bool isDeleted = await DeleteFileOrFolder(updateFolder);
			if (isDeleted == false)
			{
				return false;
			}

			// Clean up old update file and then check if it's not present anymore
			isDeleted = await DeleteFileOrFolder(zipFullPath);
			if (isDeleted == false)
			{
				return false;
			}

			// ######################################################################################################################
			// Get the commit date of the local program files
			// lastCommit file is automatically created after each built run via Visual Studio project properties > post build command
			// 		cd $(SolutionDir)
			// 		git log -1 --pretty=format:"{commit: %%H, date: %%cI}" > "$(TargetDir)config\lastCommit.json"
			string configPath = AppDomain.CurrentDomain.BaseDirectory + @"config\lastCommit.json";
			string configSchemaPath = AppDomain.CurrentDomain.BaseDirectory + @"config\schemas\lastCommit.json";
			JObject lastCommit = Utils.ReadConfig(configPath, configSchemaPath);

			// Check if commit date could be read
			if (lastCommit == null)
			{
				return false;
			}

			// The schema already makes sure that a "date" node exists. No additional null check needed
			localCommitDate = DateTimeOffset.Parse((string)lastCommit.SelectToken("date"), GlobalVariables.CultEng.DateTimeFormat);

			// ######################################################################################################################
			using (HttpClient client = InitiateHttpClient())
			{
				client.DefaultRequestHeaders.Add("Authorization", "token " + (string)User.Accounts["GitHub"]["AccessToken"]);
				client.DefaultRequestHeaders.Add("User-Agent", (string)User.Settings["UserAgent"]);
				client.DefaultRequestHeaders.Accept.Add(new System.Net.Http.Headers.MediaTypeWithQualityHeaderValue("application/vnd.github.v3+json"));

				// Issue new seperate cancellation token winch does not interfere with the global cancellation token
				GlobalVariables.TokenSource = new CancellationTokenSource();
				CancellationToken cancelToken = GlobalVariables.TokenSource.Token;

				// ######################################################################################################################
				// Get the latest release info from GitHub repository
				using (HttpRequestMessage latestReleaseRequest = new HttpRequestMessage())
				{
					latestReleaseRequest.RequestUri = new Uri("https://api.github.com/repos/ShinyId3Tagger/Shiny-ID3-Tagger/releases/latest");

					string latestRelease = await GetHttpResponse(client, latestReleaseRequest, cancelToken);
					latestReleaseJson = DeserializeJson(latestRelease);

					// Check if latest release info could be downloaded
					if (latestReleaseJson == null)
					{
						string[] warningMsg =
						{
							"WARNING:  Could not update program!",
							"Message:  Failed to get info for latest release from GitHub",
						};
						Form1.Instance.RichTextBox_LogMessage(warningMsg, 3);
						return false;
					}

					// "created_at" in JSON refers to the corresponding commit date. GitHub responds with UTC time in ISO 8601 format "yyyy-MM-ddTHH:mm:ssZ"
					latestReleaseDate = Utils.ConvertStringToDate((string)latestReleaseJson.SelectToken("created_at"));

					// Check if latest release date could be found
					if (latestReleaseDate == default(DateTimeOffset))
					{
						string[] warningMsg =
						{
							"WARNING:  Could not update program!",
							"Message:  Failed to get date for latest release from GitHub",
						};
						Form1.Instance.RichTextBox_LogMessage(warningMsg, 3);
						return false;
					}
				}

				// ######################################################################################################################
				// If local file date is equal or newer then release date, than there is no update availale
				if (localCommitDate >= latestReleaseDate)
				{
					if (showMessages)
					{
						string[] generalMsg =
						{
				 			"No newer version found!",
				 			"Your version:\t" + localCommitDate.Value.ToLocalTime().ToString("yyyy-MM-dd HH:mm:ss"),
				 			"Newest version:\t" + latestReleaseDate.Value.ToLocalTime().ToString("yyyy-MM-dd HH:mm:ss"),
						};
						Form1.Instance.RichTextBox_LogMessage(generalMsg, 1, GlobalVariables.OutputLog.Search);
					}

					return false;
				}

				// Ask user if he want's to update the program
				DialogResult dialogResult = MessageBox.Show(
					"Your version:\t" + localCommitDate.Value.ToLocalTime().ToString("yyyy-MM-dd HH:mm:ss") + "\n" +
					"Newest version:\t" + latestReleaseDate.Value.ToLocalTime().ToString("yyyy-MM-dd HH:mm:ss") + "\n\n" +
					"Update now?",
					"Update available",
					MessageBoxButtons.OKCancel);

				// If user didn't press "OK", quit update method
				if (dialogResult != DialogResult.OK)
				{
					return false;
				}

				using (HttpRequestMessage downloadRequest = new HttpRequestMessage())
				{
					// URL for latest release file from GitHub
					string downloadUrl = (string)latestReleaseJson.SelectToken("assets[0].browser_download_url");

					// Check if URL for new update file is a valid URL
					if (Utils.IsValidUrl(downloadUrl) == false)
					{
						string[] warningMsg =
						{
							"WARNING:  Could not update program!",
							"Message:  Update URL is not a valid URL",
							"URL:      " + downloadUrl,
						};
						Form1.Instance.RichTextBox_LogMessage(warningMsg, 3);
						return false;
					}

					downloadRequest.RequestUri = new Uri(downloadUrl);

					// Download the latest release from GitHub
					byte[] downloadData = await GetHttpResponse(client, downloadRequest, cancelToken, returnByteArray: true);

					// Check if new update file could be downloaded
					if (downloadData == null)
					{
						string[] warningMsg =
						{
							"WARNING:  Could not update program!",
							"Message:  Failed to download update file",
							"URL:      " + downloadUrl,
						};
						Form1.Instance.RichTextBox_LogMessage(warningMsg, 3);
						return false;
					}

					// ######################################################################################################################
					// Write the release file to temp folder
					File.WriteAllBytes(zipFullPath, downloadData);

					// Get size of local zip file and expected download size from GitHub
					long localFileSize = new FileInfo(zipFullPath).Length;
					long downloadSize = Utils.ParseLong((string)latestReleaseJson.SelectToken("assets[0].size"));

					// Check if both sizes match
					if (downloadSize != localFileSize)
					{
						string[] warningMsg =
						{
							"WARNING:  Could not update program!",
							"Message:  Incorrect size of update file: " + downloadSize + " bytes (expected) !=" + localFileSize + " bytes (actual)",
							"Path:     " + zipFullPath,
						};
						Form1.Instance.RichTextBox_LogMessage(warningMsg, 3);
						return false;
					}

					// Try to extract update zip file
					try
					{
						ZipFile.ExtractToDirectory(zipFullPath, updateFolder);
					}
					catch (Exception ex)
					{
						string[] warningMsg =
						{
							"WARNING:  Could not update program!",
							"Message:  Extraction of update file failed",
							"          " + ex.Message,
							"Source:   " + zipFullPath,
							"Target:   " + updateFolder,
						};
						Form1.Instance.RichTextBox_LogMessage(warningMsg, 3);
						return false;
					}

					// Clean up new update file and then check if it's not present anymore
					isDeleted = await DeleteFileOrFolder(zipFullPath);
					if (isDeleted == false)
					{
						return false;
					}

					return true;
				}
			}
		}
	}
}
