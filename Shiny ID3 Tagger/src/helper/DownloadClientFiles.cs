//-----------------------------------------------------------------------
// <copyright file="DownloadClientFiles.cs" company="Shiny ID3 Tagger">
// Copyright (c) Shiny ID3 Tagger. All rights reserved.
// </copyright>
// <author>ShinyId3Tagger Team</author>
// <summary>Gets configurations from external config files</summary>
//-----------------------------------------------------------------------

// VS: Post build command: cd $(SolutionDir)
// VS: git log -1 --pretty=format:"{commit: %%H, date: %%ad}" > "$(TargetDir)lastCommit.json"
// https://git-scm.com/docs/git-show

// MAIN: Rest query Github => GET /repos/:owner/:repo/branches/:branch (use develop or master according to user settings)
// MAIN: Compare date from remote with date in local file "lastCommit.log"
// MAIN: If remote is newer, download all new files into new folder called "update"
// https://developer.github.com/v3/repos/contents/#get-contents
// fileRequest.RequestUri = new Uri("https://api.github.com/repos/ShinyId3Tagger/Shiny-ID3-Tagger/contents/Shiny%20ID3%20Tagger/config/accounts.json");
// MAIN: Check if there are any files in "update" folder (maybe from last program start or this one)
// MAIN: If yes, start updater.exe and close main program

// UPDATER: Check if main program is closed. wait 5s, close updater if thread is still alive
// UPDATER: Loop through all files in update folder and copy file one by one to main folder
// UPDATER: Delete update folder
// UPDATER: Start main program
// UPDATER: Close updater
namespace Utils
{
	using System;
	using System.IO;
	using System.Net.Http;
	using System.Threading;
	using System.Threading.Tasks;
	using System.Windows.Forms;
	using GlobalNamespace;
	using GlobalVariables;
	using Newtonsoft.Json.Linq;

	internal partial class Utils
	{
		internal static async Task<bool> DownloadClientFiles()
		{
			DateTime? lastCommitDate = null;
			DateTime? remoteCommitDate = null;
			string lastCommitSha = null;
			string remoteCommitSha = null;

			// ######################################################################################################################
			// Path for lastCommit file
			string lastCommitPath = AppDomain.CurrentDomain.BaseDirectory + @"lastCommit.json";

			try
			{
				// Read content from lastCommit.json
				string lastCommitJson = File.ReadAllText(lastCommitPath);

				// Validate lastCommit.json. If any validation errors occurred, ValidateConfig will throw an exception which is catched later
				ValidateSchema(lastCommitJson, lastCommitSchemaStr);

				// Save last commit to JObject for later access throughout the program
				JObject lastCommitData = JObject.Parse(lastCommitJson);

				if (lastCommitData != null)
				{
					lastCommitSha = (string)lastCommitData.SelectToken("commit");
					lastCommitDate = (DateTime)lastCommitData.SelectToken("date");

					Form1.Instance.Text = Application.ProductName + "     GitHub commit date: " + lastCommitDate.Value.ToString("yyyy-MM-dd HH:mm:ss");
				}
			}
			catch (Exception ex)
			{
				string[] errorMsg =
					{
					@"ERROR:    Failed to read lastCommit.json! Please close program and fix this first...",
					"Filepath: " + lastCommitPath,
					"Message:  " + ex.Message.TrimEnd('\r', '\n')
				};
				Form1.Instance.PrintErrorMessage(errorMsg);
			}

			// ######################################################################################################################
			// Issue new cancellation token
			GlobalVariables.TokenSource = new CancellationTokenSource();
			CancellationToken cancelToken = GlobalVariables.TokenSource.Token;

			using (HttpClient client = InitiateHttpClient())
			{
				// Check if user credentials and user settings are available
				if (User.Accounts != null && User.Settings != null)
				{
					using (HttpRequestMessage remoteCommitRequest = new HttpRequestMessage())
					{
						remoteCommitRequest.Headers.Add("Authorization", "token " + (string)User.Accounts["GitHub"]["AccessToken"]);
						remoteCommitRequest.Headers.Add("User-Agent", (string)User.Settings["UserAgent"]);
						remoteCommitRequest.Headers.Accept.Add(new System.Net.Http.Headers.MediaTypeWithQualityHeaderValue("application/vnd.github.v3+json"));

						remoteCommitRequest.RequestUri = new Uri("https://api.github.com/repos/ShinyId3Tagger/Shiny-ID3-Tagger/branches/" + (string)User.Settings["Branch"]);

						string remoteCommitContent = await GetResponse(client, remoteCommitRequest, cancelToken);
						JObject remoteCommitData = DeserializeJson(remoteCommitContent);

						if (remoteCommitData != null)
						{
							remoteCommitSha = (string)remoteCommitData.SelectToken("commit.sha");
							remoteCommitDate = (DateTime?)remoteCommitData.SelectToken("commit.commit.author.date");
						}
					}
				}

				// If local/last commit date is older (last < remote) than remote commit date, then there must be an update available on GitHub
				if (lastCommitDate.HasValue && remoteCommitDate.HasValue && lastCommitDate < remoteCommitDate)
				{
					DialogResult dialogResult = MessageBox.Show("Download update now?", "Update available", MessageBoxButtons.YesNo);
					if (dialogResult == DialogResult.Yes)
					{
						using (HttpRequestMessage remoteTreeRequest = new HttpRequestMessage())
						{
							remoteTreeRequest.Headers.Add("Authorization", "token " + (string)User.Accounts["GitHub"]["AccessToken"]);
							remoteTreeRequest.Headers.Add("User-Agent", (string)User.Settings["UserAgent"]);
							remoteTreeRequest.Headers.Accept.Add(new System.Net.Http.Headers.MediaTypeWithQualityHeaderValue("application/vnd.github.v3+json"));

							// https://developer.github.com/v3/git/trees/#get-a-tree-recursively
							// https://stackoverflow.com/questions/7106012/download-a-single-folder-or-directory-from-a-github-repo
							remoteTreeRequest.RequestUri = new Uri("https://api.github.com/repos/ShinyId3Tagger/Shiny-ID3-Tagger/git/trees/" + remoteCommitSha + "?recursive=1");

							string remoteTreeContent = await GetResponse(client, remoteTreeRequest, cancelToken);
							JObject remoteTreeData = DeserializeJson(remoteTreeContent);

							if (remoteTreeData != null)
							{
								// Use GET CONTENT to download a file
								// https://developer.github.com/v3/repos/contents/#get-contents
							}
						}
					}
				}
			}

			return false;
		}
	}
}
