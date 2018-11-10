//-----------------------------------------------------------------------
// <copyright file="UpdateClient.cs" company="Shiny ID3 Tagger">
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
		/// Checks for updates and downloads newest application files from GitHub
		/// https://developer.github.com/v3/
		/// lastCommit file is automatically created after each built run
		/// Done via Visual Studio project properties > post build command
		/// 		cd $(SolutionDir)
		/// 		git log -1 --pretty=format:"{commit: %%H, date: %%ad}" > "$(TargetDir)config\lastCommit.json"
		/// Read more about GIT commands: https://git-scm.com/docs/git-show
		/// </summary>
		/// <returns>A <see cref="Task"/> representing the asynchronous operation.</returns>
		internal static async Task<bool> UpdateClient()
		{
			DateTimeOffset? localCommitDate = null;
			DateTimeOffset? latestReleaseDate = null;
			JObject latestReleaseJson = null;

			string zipFullPath = Path.GetTempPath() + "shiny-id3-tagger-update.zip";
			string updateFolder = Path.GetTempPath() + @"shiny-id3-tagger-update\";
			string updateProcessName = "Shiny ID3 Tagger Updater";
			string updateExeFullPath = updateFolder + "Shiny ID3 Tagger Updater.exe";

			// ######################################################################################################################
			// Clean up old update file and update folder in %temp% folder
			// Check if an old updater process is still running
			Process updateProcess = (from process in Process.GetProcessesByName(updateProcessName)
									 where process.MainModule.FileName == updateExeFullPath
									 select process).FirstOrDefault();

			// If an old updater is still running: Log error message and quit update method
			if (updateProcess != null)
			{
				string[] errorMsg = { "ERROR:    A previous update is still running!" };
				Form1.Instance.RichTextBox_LogMessage(errorMsg, 2);
				return false;
			}

			// If old update file could not be deleted: Quit update method
			bool isDeleted = await DeleteFileOrFolder(zipFullPath);
			if (isDeleted == false)
			{
				return false;
			}

			// If old update file could not be deleted: Quit update method
			isDeleted = await DeleteFileOrFolder(updateFolder);
			if (isDeleted == false)
			{
				return false;
			}

			// ######################################################################################################################
			// Get the commit date of the local program files
			JObject lastCommit = Utils.ReadConfig(@"config\lastCommit.json", @"config\schemas\lastCommit.schema.json");

			if (lastCommit != null)
			{
				localCommitDate = DateTimeOffset.Parse((string)lastCommit.SelectToken("date"), GlobalVariables.CultEng.DateTimeFormat);

				Form1.Instance.Text = Application.ProductName + "     Version: " + localCommitDate.Value.ToString("yyyy-MM-dd HH:mm:ss");
			}

			// If local program files commit date could not be found: Log error message and quit update method
			if (localCommitDate.HasValue == false)
			{
				string[] errorMsg = { "ERROR:    Could not get date of program files!" };
				Form1.Instance.RichTextBox_LogMessage(errorMsg, 2);
				return false;
			}

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

					string latestRelease = await GetResponse(client, latestReleaseRequest, cancelToken);
					latestReleaseJson = DeserializeJson(latestRelease);

					// If latest release info could not be downloaded: Log error message and quit update method
					if (latestReleaseJson == null)
					{
						string[] errorMsg = { "ERROR:    Could not get latest update info!" };
						Form1.Instance.RichTextBox_LogMessage(errorMsg, 2);
						return false;
					}

					// "created_at" in JSON is always the same as the corresponding commit date
					latestReleaseDate = DateTimeOffset.Parse((string)latestReleaseJson.SelectToken("created_at"), GlobalVariables.CultEng.DateTimeFormat);

					// If local latest release date could not be found: Log error message and quit update method
					if (latestReleaseDate.HasValue == false)
					{
						string[] errorMsg = { "ERROR:    Could not get date of update file!" };
						Form1.Instance.RichTextBox_LogMessage(errorMsg, 2);
						return false;
					}
				}

				// ######################################################################################################################
				// If local file date is equal or newer then release date, then there is now update availale. Quit update method
				// TODO: Switch back to: localCommitDate >= latestReleaseDate
				if (localCommitDate < latestReleaseDate)
				{
					return false;
				}

				// Ask user if he want's to update the program: If user didn't press "OK", quit update method
				DialogResult dialogResult = MessageBox.Show(
					"Local date: " + localCommitDate.Value.LocalDateTime.ToString("yyyy-MM-dd HH:mm:ss") + "\n" +
					"Update date: " + latestReleaseDate.Value.LocalDateTime.ToString("yyyy-MM-dd HH:mm:ss") + "\n\n" +
					"Update now?",
					"Update available",
					MessageBoxButtons.OKCancel);

				if (dialogResult != DialogResult.OK)
				{
					return false;
				}

				using (HttpRequestMessage downloadRequest = new HttpRequestMessage())
				{
					// URL for latest release file from GitHub
					string downloadUrl = (string)latestReleaseJson.SelectToken("assets[0].browser_download_url");

					// If URL for new update file is not a valid URL (or just empty): Log error message and quit update method
					if (Utils.IsValidUrl(downloadUrl) == false)
					{
						string[] errorMsg = { "ERROR:    Update URL is not a valid URL!" };
						Form1.Instance.RichTextBox_LogMessage(errorMsg, 2);
						return false;
					}

					downloadRequest.RequestUri = new Uri(downloadUrl);

					// Download the release from GitHub
					byte[] downloadData = await GetResponse(client, downloadRequest, cancelToken, returnByteArray: true);

					// If new update file could not be downloaded: Log error message and quit update method
					if (downloadData == null)
					{
						string[] errorMsg = { "ERROR:    Could not download the update file!" };
						Form1.Instance.RichTextBox_LogMessage(errorMsg, 2);
						return false;
					}

					// ######################################################################################################################
					// Write the release file to temp folder
					File.WriteAllBytes(zipFullPath, downloadData);

					// Compare size of local zip file and expected download size from GitHub
					long localFileSize = new FileInfo(zipFullPath).Length;
					long downloadSize = Utils.ParseLong((string)latestReleaseJson.SelectToken("assets[0].size"));

					// If size doesn't match: Log error message and quit update method
					if (downloadSize != localFileSize)
					{
						string[] errorMsg = { "ERROR:    Incorrect size of update file!" };
						Form1.Instance.RichTextBox_LogMessage(errorMsg, 2);
						return false;
					}

					// Extract update zip file
					try
					{
						ZipFile.ExtractToDirectory(zipFullPath, updateFolder);
					}
					catch (Exception ex)
					{
						string[] errorMsg =
						{
							"ERROR:    Could not extract update file!",
							"Message:  " + ex.Message,
						};
						Form1.Instance.RichTextBox_LogMessage(errorMsg, 2);
						return false;
					}

					// If new update file could not be deleted: Quit update method
					isDeleted = await DeleteFileOrFolder(zipFullPath);
					if (isDeleted == false)
					{
						return false;
					}

					// Call Updater.exe in temp folder (= new updater version) with this path as argument
					Process.Start(updateExeFullPath, AppDomain.CurrentDomain.BaseDirectory);

					// Exit this process
					Environment.Exit(0);

					return true;
				}
			}
		}
	}
}
