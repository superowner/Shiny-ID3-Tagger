//-----------------------------------------------------------------------
// <copyright file="UpdateClient.cs" company="Shiny ID3 Tagger">
// Copyright (c) Shiny ID3 Tagger. All rights reserved.
// </copyright>
// <author>ShinyId3Tagger Team</author>
// <summary>Checks for updates and downloads newest application files from GitHub</summary>
//-----------------------------------------------------------------------

namespace Utils
{
	using System;
	using System.IO;
	using System.Net.Http;
	using System.Threading;
	using System.Threading.Tasks;
	using System.Windows.Forms;
	using GlobalVariables;
	using Newtonsoft.Json.Linq;
	using Shiny_ID3_Tagger;

	internal partial class Utils
	{
		internal static async Task<bool> UpdateClient()
		{
			DateTimeOffset? localCommitDate = null;
			DateTimeOffset? remoteCommitDate = null;
			string lastCommitSha = null;
			string remoteCommitSha = null;

			// ######################################################################################################################
			// Get the commit date when the local program files were created
			string lastCommitPath = AppDomain.CurrentDomain.BaseDirectory + @"lastCommit.json";
			string lastCommitSchemaPath = AppDomain.CurrentDomain.BaseDirectory + @"config\schemas\lastCommit.schema.json";

			try
			{
				// Read content from lastCommit.json
				// lastCommit file is automatically created after each built run
				// Done via Visual Studio project properties > post build command
				// 		cd $(SolutionDir)
				// 		git log -1 --pretty=format:"{commit: %%H, date: %%ad}" > "$(TargetDir)lastCommit.json"
				// Read command documentation: https://git-scm.com/docs/git-show
				string lastCommitJson = File.ReadAllText(lastCommitPath);

				// Validate lastCommit.json. If any validation errors occurred, ValidateConfig will throw an exception which is catched later
				ValidateSchema(lastCommitJson, lastCommitSchemaPath);

				// Save last commit to JObject for later access throughout the program
				JObject lastCommitData = JObject.Parse(lastCommitJson);

				if (lastCommitData != null)
				{
					lastCommitSha = (string)lastCommitData.SelectToken("commit");
					localCommitDate = DateTimeOffset.Parse((string)lastCommitData.SelectToken("date"));

					Form1.Instance.Text = Application.ProductName + "     Client date: " + localCommitDate.Value.ToString("yyyy-MM-dd HH:mm:ss");
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
				Form1.Instance.RichTextBox_PrintErrorMessage(errorMsg);
			}

			// ######################################################################################################################
			// Get the latest commit date from GitHub repository
			using (HttpClient client = InitiateHttpClient())
			{
				// Issue new cancellation token winch does not interfere with the global cancellation token
				GlobalVariables.TokenSource = new CancellationTokenSource();
				CancellationToken cancelToken = GlobalVariables.TokenSource.Token;

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

			// ######################################################################################################################
			// If local commit date is older than remote commit date, then there should be a newer GitHub release
				if (localCommitDate.HasValue && remoteCommitDate.HasValue && localCommitDate < remoteCommitDate)
				{
					// Ask user if he want's to update
					DialogResult dialogResult = MessageBox.Show("Download update now?", "Update available", MessageBoxButtons.YesNo);
					if (dialogResult == DialogResult.Yes)
					{
						// Get the URL for the latest release
						using (HttpRequestMessage latestReleaseRequest = new HttpRequestMessage())
						{
							latestReleaseRequest.Headers.Add("Authorization", "token " + (string)User.Accounts["GitHub"]["AccessToken"]);
							latestReleaseRequest.Headers.Add("User-Agent", (string)User.Settings["UserAgent"]);
							latestReleaseRequest.Headers.Accept.Add(new System.Net.Http.Headers.MediaTypeWithQualityHeaderValue("application/vnd.github.v3+json"));

							latestReleaseRequest.RequestUri = new Uri("https://api.github.com/repos/ShinyId3Tagger/Shiny-ID3-Tagger/releases/latest");

							string latestRelease = await GetResponse(client, latestReleaseRequest, cancelToken);
							JObject latestReleaseData = DeserializeJson(latestRelease);

							string latestReleaseDownloadUrl = (string)latestReleaseData.SelectToken("assets[0].browser_download_url");

							// Download the release file
							if (latestReleaseDownloadUrl != null && Utils.IsValidUrl(latestReleaseDownloadUrl))
							{
								using (HttpRequestMessage downloadRequest = new HttpRequestMessage())
								{
									downloadRequest.Headers.Add("Authorization", "token " + (string)User.Accounts["GitHub"]["AccessToken"]);
									downloadRequest.Headers.Add("User-Agent", (string)User.Settings["UserAgent"]);
									downloadRequest.Headers.Accept.Add(new System.Net.Http.Headers.MediaTypeWithQualityHeaderValue("application/vnd.github.v3+json"));

									latestReleaseRequest.RequestUri = new Uri(latestReleaseDownloadUrl);

									byte[] downloadData = await GetResponse(client, latestReleaseRequest, cancelToken, returnByteArray: true);

									if (downloadData != null)
									{
										File.WriteAllBytes("update.zip", downloadData);
									}
								}
							}
						}
					}
				}
			}

			return false;
		}
	}
}
