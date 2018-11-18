//-----------------------------------------------------------------------
// <copyright file="CheckAndDownloadUpdate.cs" company="Shiny ID3 Tagger">
// Copyright (c) Shiny ID3 Tagger. All rights reserved.
// </copyright>
// <author>ShinyId3Tagger Team</author>
//-----------------------------------------------------------------------

namespace Shiny_ID3_Tagger
{
	using System;
	using System.Collections.Generic;
	using System.Diagnostics;
	using System.Globalization;
	using System.IO;
	using System.IO.Compression;
	using System.Linq;
	using System.Net.Http;
	using System.Threading;
	using System.Threading.Tasks;
	using System.Windows.Forms;
	using Exceptions;
	using GlobalVariables;
	using Newtonsoft.Json.Linq;
	using Utils;

	/// <summary>
	/// Represents the Utility class which holds various helper functions
	/// </summary>
	public partial class Form1
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
			List<string> additionalErrorInfo = new List<string>() { };

			try
			{
				DateTimeOffset? localCommitDate = null;
				DateTimeOffset? latestReleaseDate = null;
				JObject latestReleaseJson = null;
				Process oldUpdateProcess = null;

				string updateProcessName = "UpdateClient";
				string zipFullPath = Path.GetTempPath() + "shiny-id3-tagger-update.zip";
				string updateFolder = Path.GetTempPath() + @"shiny-id3-tagger-update\";
				string updateExeFullPath = updateFolder + "UpdateClient.exe";
				string updateLogFullPath = updateFolder + "update.log";

				// ######################################################################################################################
				// Try to get running updater process. Can throw multiple exceptions
				oldUpdateProcess = (from process in Process.GetProcessesByName(updateProcessName)
									where process.MainModule.FileName == updateExeFullPath
									select process).FirstOrDefault();

				// Check if a running updater process was found
				if (oldUpdateProcess?.ProcessName != null)
				{
					throw new UpdateClientException("A previous update is still running");
				}

				// Clean up old update folders and then check if it's not present anymore
				bool isDeleted = await Utils.DeleteFileOrFolder(updateFolder);
				if (isDeleted == false)
				{
					throw new UpdateClientException("Could not delete a previous update folder");
				}

				// Clean up old update file and then check if it's not present anymore
				isDeleted = await Utils.DeleteFileOrFolder(zipFullPath);
				if (isDeleted == false)
				{
					throw new UpdateClientException("Could not delete a previous update zip file");
				}

				// ######################################################################################################################
				// Get the commit date of the local program files
				// lastCommit file is automatically created after each built run via Visual Studio project properties > post build command
				// 		cd $(SolutionDir)
				// 		git log -1 --pretty=format:"{commit: %%H, date: %%cI}" > "$(TargetDir)config\lastCommit.json"
				string configPath = GlobalVariables.AppDir + @"config\lastCommit.json";
				string configSchemaPath = GlobalVariables.AppDir + @"config\schemas\lastCommit.json";
				JObject lastCommit = Utils.ReadConfig(configPath, configSchemaPath);

				// Check if commit date exists
				if (lastCommit?.SelectToken("date") == null)
				{
					throw new UpdateClientException("Could not find date of the local program files");
				}

				// Try to parse local date (ISO 8601 format: yyyy-MM-ddTHH:mm:ss.FFFK)
				if (DateTimeOffset.TryParseExact(
					(string)lastCommit.SelectToken("date"),
					"yyyy-MM-ddTHH:mm:ss.FFFK",
					GlobalVariables.CultEng,
					DateTimeStyles.None,
					out DateTimeOffset result) == false)
				{
					throw new UpdateClientException("Could not parse the date of the local program files");
				}

				localCommitDate = result;

				// ######################################################################################################################
				using (HttpClient client = Utils.InitiateHttpClient())
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

						string latestRelease = await Utils.GetHttpResponse(client, latestReleaseRequest, cancelToken);
						latestReleaseJson = Utils.DeserializeJson(latestRelease);

						// Check if latest release info could be downloaded
						if (latestReleaseJson == null)
						{
							throw new UpdateClientException("Failed to get info for latest release from GitHub");
						}

						// "created_at" in JSON refers to the corresponding commit date. GitHub responds with UTC time in ISO 8601 format "yyyy-MM-ddTHH:mm:ssZ"
						latestReleaseDate = Utils.ConvertStringToDate((string)latestReleaseJson.SelectToken("created_at"));

						// Check if latest release date could be found
						if (latestReleaseDate == default(DateTimeOffset))
						{
							throw new UpdateClientException("Failed to get date for latest release from GitHub");
						}
					}

					// ######################################################################################################################
					// If local file date is equal or newer then release date, than there is no update availale
					if (localCommitDate >= latestReleaseDate)
					{
						// App menu > Search Update > Show message
						// AutoUpdate=true > app start > Show no message
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
							throw new UpdateClientException("Update URL is not a valid URL");
						}

						downloadRequest.RequestUri = new Uri(downloadUrl);

						// Download the latest release from GitHub
						byte[] downloadData = await Utils.GetHttpResponse(client, downloadRequest, cancelToken, returnByteArray: true);

						// Check if new update file could be downloaded
						if (downloadData == null)
						{
							throw new UpdateClientException("Failed to download update file");
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
							throw new UpdateClientException("Incorrect size of update file: " + downloadSize + " bytes (expected) !=" + localFileSize + " bytes (actual)");
						}

						// Try to extract update zip file
						try
						{
							ZipFile.ExtractToDirectory(zipFullPath, updateFolder);
						}
						catch (Exception ex)
						{
							throw new UpdateClientException("Extraction of update file failed: " + ex.Message);
						}

						// Clean up new update file and then check if it's not present anymore
						isDeleted = await Utils.DeleteFileOrFolder(zipFullPath);
						if (isDeleted == false)
						{
							throw new UpdateClientException("Could not delete the update zip file during cleanup");
						}

						return true;
					}
				}
			}
			catch (UpdateClientException ex)
			{
				string[] warningMsg =
				{
					"WARNING:  Could not update program!",
					"Message:  " + ex.Message,
				};
				Form1.Instance.RichTextBox_LogMessage(warningMsg, 3);

				return false;
			}
		}
	}
}
