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
// MAIN: Check if there are any files in "update" folder (maybe from last program start or this one)
// MAIN: If yes, start updater.exe and close main program

// UPDATER: Check if main program is closed. wait 5s, close updater if thread is still alive
// UPDATER: Loop through all files in update folder and copy file one by one to main folder
// UPDATER: Delete update folder
// UPDATER: Start main program
// UPDATER: Close updater


namespace GlobalNamespace
{
	using System;
	using System.Net.Http;
	using System.Threading;
	using System.Threading.Tasks;
	using System.Windows.Forms;
	using Newtonsoft.Json.Linq;

	public partial class Form1
	{
		private async Task<bool> DownloadClientFiles()
		{
			// TODO: Read in local lastCommit date and sha
			// TODO: Add the last commit date to form name
			// Change form name to include github commit date
			Version version = new Version(Application.ProductVersion);
			this.Text = Application.ProductName + " v" + version.Major + "." + version.Minor;

			// Issue new cancellation token
			TokenSource = new CancellationTokenSource();
			CancellationToken cancelToken = TokenSource.Token;

			// Continue only if user credentials and user settings are present
			if (User.Accounts != null && User.Settings != null)
			{
				string branch = (string)User.Settings["Branch"];

				using (HttpClient client = InitiateHttpClient())
				{
					using (HttpRequestMessage remoteCommitRequest = new HttpRequestMessage())
					{
						remoteCommitRequest.Headers.Accept.Add(new System.Net.Http.Headers.MediaTypeWithQualityHeaderValue("application/vnd.github.v3+json"));
						remoteCommitRequest.Headers.Add("User-Agent", (string)User.Settings["UserAgent"]);

						// https://developer.github.com/v3/repos/branches/#get-branch
						remoteCommitRequest.RequestUri = new Uri("https://api.github.com/repos/ShinyId3Tagger/Shiny-ID3-Tagger/branches/" + branch);

						string remoteCommitContent = await this.GetResponse(client, remoteCommitRequest, cancelToken);
						JObject remoteCommitData = this.DeserializeJson(remoteCommitContent);

						string remoteCommitSha = (string)remoteCommitData.SelectToken("commit.sha");
						DateTime remoteCommitDate = (DateTime)remoteCommitData.SelectToken("commit.commit.author.date");

						// https://developer.github.com/v3/repos/contents/#get-contents
						//fileRequest.RequestUri = new Uri("https://api.github.com/repos/ShinyId3Tagger/Shiny-ID3-Tagger/contents/Shiny%20ID3%20Tagger/config/accounts.json");
					}
				}
			}





			return false;
		}
	}
}
