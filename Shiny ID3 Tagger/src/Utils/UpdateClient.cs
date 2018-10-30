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
			DateTime? latestReleaseDate = null;
			string downloadUrl = null;

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

					latestReleaseDate = Utils.ConvertStringToDate((string)latestReleaseData.SelectToken("created_at"));

					// URL for latest release file from GitHub
					downloadUrl = (string)latestReleaseData.SelectToken("assets[0].browser_download_url");
				}

				// ######################################################################################################################
				// If local file date is older than latest release date, then GitHub has a newer release
				if (localCommitDate.HasValue && latestReleaseDate.HasValue && localCommitDate < latestReleaseDate)
				{
					// Ask user if he want's to update the program
					DialogResult dialogResult = MessageBox.Show("Download update now?", "Update available", MessageBoxButtons.YesNo);
					if (dialogResult == DialogResult.Yes)
					{
						if (downloadUrl != null && Utils.IsValidUrl(downloadUrl))
						{
							using (HttpRequestMessage downloadRequest = new HttpRequestMessage())
							{
								downloadRequest.RequestUri = new Uri(downloadUrl);

								byte[] downloadData = await GetResponse(client, downloadRequest, cancelToken, returnByteArray: true);

								// Write the file as "update.zip" to program folder
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
	}
}
