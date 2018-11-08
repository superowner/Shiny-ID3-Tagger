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
				Form1.Instance.RichTextBox_LogMessage(new[] { "Couldn't get date of program files" }, 2);
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
					Form1.Instance.RichTextBox_LogMessage(new[] { "Could not get date of update file" }, 2);
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
						Form1.Instance.RichTextBox_LogMessage(new[] { "Update URL is not a valid URL" }, 2);
						return false;
					}

					downloadRequest.RequestUri = new Uri(downloadUrl);

					// Download the release from GitHub
					byte[] downloadData = await GetResponse(client, downloadRequest, cancelToken, returnByteArray: true);

					// If new update file coulnd't be downloaded: Log error message and quit update method
					if (downloadData == null)
					{
						Form1.Instance.RichTextBox_LogMessage(new[] { "Couldn't download the update file" }, 2);
						return false;
					}

					// ######################################################################################################################
					// Build path to zip file
					string fullZipPath = AppDomain.CurrentDomain.BaseDirectory + "update.zip";

					// TODO: Continue here
					// Try to delete old files three times (with a small delay of 50ms between)
					bool deleted = await SaveDeleteFileOrFolder(fullZipPath);

					// If old update file coulnd't be deleted: Log error message and quit update method
					if (deleted == false)
					{
						Form1.Instance.RichTextBox_LogMessage(new[] { "Couldn't delete the old update file" }, 2);
						return false;
					}

					// Write the file as "update.zip" to program folder
					File.WriteAllBytes(fullZipPath, downloadData);

					// Compare size if file and downloadSize
					long downloadSize = Utils.ParseLong((string)latestReleaseJson.SelectToken("assets[0].size"));
					long localFileSize = new FileInfo(fullZipPath).Length;

					// If size doesn't match: Log error message and quit update method
					if (downloadSize != localFileSize)
					{
						Form1.Instance.RichTextBox_LogMessage(new[] { "Incorrect size of update file" }, 2);
						return false;
					}

					string extractPath = AppDomain.CurrentDomain.BaseDirectory + "update";

					// TODO: Continue here. Save delete update folder with SaveDeleteFileOrFolder()
					try
					{
						ZipFile.ExtractToDirectory(fullZipPath, extractPath);
					}
					catch (Exception ex)
					{
						Form1.Instance.RichTextBox_LogMessage(new[] { ex.Message }, 2);
						return false;
					}

					//Process.Start("Updater.exe", "Hello you too");

					return true;
				}
			}
		}

		private static async Task<bool> SaveDeleteFileOrFolder(string fullZipPath)
		{
			const int WriteDelay = 50;
			const int MaxRetries = 3;
			string errorMessage = null;

			for (int retry = 0; retry < MaxRetries; retry++)
			{
				if (File.Exists(fullZipPath))
				{
					try
					{
						File.Delete(fullZipPath);
					}
					catch (Exception ex)
					{
						errorMessage = ex.Message;
						await Task.Delay(WriteDelay);
					}
				}
				else
				{
					errorMessage = null;
					break;
				}
			}

			if (errorMessage == null)
			{
				// No errorMessage => Deletion successfull
				return true;
			}
			else
			{
				// Has ErrorMessage => Deletion not successfull
				return false;
			}
		}
	}
}
