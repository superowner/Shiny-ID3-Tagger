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
			string updateFolder = Path.GetTempPath() + "shiny-id3-tagger-update";
			string fullZipPath = Path.GetTempPath() + "shiny-id3-tagger-update.zip";

			// ######################################################################################################################
			// Clean up old update file and update folder in %temp% folder
			// If old update file could not be deleted: Quit update method
			bool isDeleted = await DeleteFileOrFolder(fullZipPath);
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
				localCommitDate = DateTimeOffset.Parse((string)lastCommit.SelectToken("date"));

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
				// Get the latest release date from GitHub repository
				using (HttpRequestMessage latestReleaseRequest = new HttpRequestMessage())
				{
					latestReleaseRequest.RequestUri = new Uri("https://api.github.com/repos/ShinyId3Tagger/Shiny-ID3-Tagger/releases/latest");

					string latestRelease = await GetResponse(client, latestReleaseRequest, cancelToken);
					latestReleaseJson = DeserializeJson(latestRelease);

					// "created_at" in JSON is always the same as the corresponding commit date
					latestReleaseDate = DateTimeOffset.Parse((string)latestReleaseJson.SelectToken("created_at"));
				}

				// If local latest release date could not be found: Log error message and quit update method
				if (latestReleaseDate.HasValue == false)
				{
					string[] errorMsg = { "ERROR:    Could not get date of update file!" };
					Form1.Instance.RichTextBox_LogMessage(errorMsg, 2);
					return false;
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
					File.WriteAllBytes(fullZipPath, downloadData);

					// Compare size of local zip file and expected download size from GitHub
					long localFileSize = new FileInfo(fullZipPath).Length;
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
						ZipFile.ExtractToDirectory(fullZipPath, updateFolder);
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
					isDeleted = await DeleteFileOrFolder(fullZipPath);
					if (isDeleted == false)
					{
						return false;
					}

					// TODO: Continue here
					// https://andreasrohner.at/posts/Programming/C%23/A-platform-independent-way-for-a-C%23-program-to-update-itself/
					// Check if an old updater process is still running. Close it if yes and recheck
					// Call Updater.exe in temp folder (= new version) with this path as argument
					// Exit this process
					// Updater checks if Shiny Id3 Tagger.exe is not running. Retries 3 times with 1s delay
					// Updater checks if Shiny Id3 Tagger.exe is present in argument folder
					// Updater copies all files from temp to program folder. Including itself
					// Throws an error if file could not be copied
					// But makes exceptions according to a blacklist
					// 	- accounts.user.json
					// 	- settings.user.json
					// Verify all files size/date is same in temp folder and program folder
					// Updater starts Shiny Id3 Tagger.exe and closes itself immediatly
					return true;
				}
			}
		}
	}
}
