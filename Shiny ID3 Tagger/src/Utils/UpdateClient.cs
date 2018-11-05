//-----------------------------------------------------------------------
// <copyright file="UpdateClient.cs" company="Shiny ID3 Tagger">
// Copyright (c) Shiny ID3 Tagger. All rights reserved.
// </copyright>
// <author>ShinyId3Tagger Team</author>
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
		internal static async Task UpdateClient()
		{
			DateTimeOffset? localCommitDate = null;
			DateTimeOffset? latestReleaseDate = null;
			string downloadUrl = null;
			int? downloadSize = null;

			// ######################################################################################################################
			// Get the commit date when the local program files were created
			JObject lastCommit = Utils.ReadConfig(@"config\lastCommit.json", @"config\schemas\lastCommit.schema.json");

			if (lastCommit != null)
			{
				localCommitDate = DateTimeOffset.Parse((string)lastCommit.SelectToken("date"));

				Form1.Instance.Text = Application.ProductName + "     Version: " + localCommitDate.Value.ToString("yyyy-MM-dd HH:mm:ss");
			}

			// ######################################################################################################################
			// Compare local file date with the latest release date on GitHub
			using (HttpClient client = InitiateHttpClient())
			{
				client.DefaultRequestHeaders.Add("Authorization", "token " + (string)User.Accounts["GitHub"]["AccessToken"]);
				client.DefaultRequestHeaders.Add("User-Agent", (string)User.Settings["UserAgent"]);
				client.DefaultRequestHeaders.Accept.Add(new System.Net.Http.Headers.MediaTypeWithQualityHeaderValue("application/vnd.github.v3+json"));

				// Issue new cancellation token winch does not interfere with the global cancellation token
				GlobalVariables.TokenSource = new CancellationTokenSource();
				CancellationToken cancelToken = GlobalVariables.TokenSource.Token;

				// Get the latest release date from GitHub repository
				using (HttpRequestMessage latestReleaseRequest = new HttpRequestMessage())
				{
					latestReleaseRequest.RequestUri = new Uri("https://api.github.com/repos/ShinyId3Tagger/Shiny-ID3-Tagger/releases/latest");

					string latestRelease = await GetResponse(client, latestReleaseRequest, cancelToken);
					JObject latestReleaseData = DeserializeJson(latestRelease);

					latestReleaseDate = DateTimeOffset.Parse((string)latestReleaseData.SelectToken("created_at"));

					// URL for latest release file from GitHub
					downloadUrl = (string)latestReleaseData.SelectToken("assets[0].browser_download_url");
					downloadSize = (int)latestReleaseData.SelectToken("assets[0].size");
				}

				// ######################################################################################################################
				// If local file date is older than latest release date, then GitHub has a newer release
				// TODO: Switch back to: if (localCommitDate.HasValue && latestReleaseDate.HasValue && localCommitDate < latestReleaseDate)
				if (downloadUrl != null &&
					Utils.IsValidUrl(downloadUrl) &&
					localCommitDate.HasValue &&
					latestReleaseDate.HasValue &&
					localCommitDate > latestReleaseDate)
				{
					// Ask user if he want's to update the program
					DialogResult dialogResult = MessageBox.Show(
						"Local date: " + localCommitDate.Value.LocalDateTime.ToString("yyyy-MM-dd HH:mm:ss") + "\n" +
						"Update date: " + latestReleaseDate.Value.LocalDateTime.ToString("yyyy-MM-dd HH:mm:ss") + "\n\n" +
						"Update now?",
						"Update available",
						MessageBoxButtons.YesNo);
					if (dialogResult == DialogResult.Yes)
					{
						using (HttpRequestMessage downloadRequest = new HttpRequestMessage())
						{
							downloadRequest.RequestUri = new Uri(downloadUrl);

							// Download the release from GitHub
							byte[] downloadData = await GetResponse(client, downloadRequest, cancelToken, returnByteArray: true);

							if (downloadData != null)
							{
								// ######################################################################################################################
								// TODO: Check if an old update.zip file is present. If yes, delete it

								// Build path to Zip file
								string fullZipPath = AppDomain.CurrentDomain.BaseDirectory + "update.zip";

								// Write the file as "update.zip" to program folder
								File.WriteAllBytes(fullZipPath, downloadData);

								// TODO: Compare size if file and downloadSize
							}
						}
					}
				}
			}
		}
	}
}
