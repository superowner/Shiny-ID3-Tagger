//-----------------------------------------------------------------------
// <copyright file="MainForm.cs" company="Shiny ID3 Tagger">
// Copyright (c) Shiny ID3 Tagger. All rights reserved.
// </copyright>
// <author>ShinyId3Tagger Team</author>
// <summary>Executed when program starts. Shows main window</summary>
// https://social.msdn.microsoft.com/Forums/windows/en-US/43a85553-2b94-4f4e-9db3-498311af4ecd/datagridview-sorting-with-null-values?forum=winforms
//-----------------------------------------------------------------------

namespace GlobalNamespace
{
	using System;
	using System.Linq;
	using System.Threading;
	using System.Windows.Forms;

	public partial class Form1 : Form
	{
		/// <summary>
		/// Initializes a new instance of the <see cref="Form1"/> class.
		/// The main form which will be shown immediately after program start
		/// </summary>
		public Form1()
		{
			this.InitializeComponent();
		}

		// Code is executed on all program calls (1,2,3,4....)
		internal async void Form1Shown(string[] args)
		{
			// Refresh cancellation token
			TokenSource = new CancellationTokenSource();
			CancellationToken cancelToken = TokenSource.Token;

			// Continue only if user credentials and user settings are present
			if (User.Accounts != null && User.Settings != null)
			{
				// Add new files to dataGridView1
				bool hasNewFiles = await this.AddFiles(args, cancelToken);

				// Continue only if user setting allows it and if new files were added
				if ((bool)User.Settings["AutoSearch"] && hasNewFiles)
				{
					this.StartSearching(cancelToken);
				}
			}
			else
			{
				TokenSource.Cancel();
				this.EnableUI(false);
			}
		}

		// Code is only executed on the first program call (1)
		protected async override void OnShown(EventArgs e)
		{
			base.OnShown(e);

			// Get user settings
			this.ReadSettings();

			// Get user credentials
			this.ReadCredentials();

			// MAIN: Check if there are any files in "update" folder
			// MAIN: If yes, start updater.exe and close main program
			// MAIN: If no, continue with github check

			// UPDATER: Check if main program is closed. wait 5s, close updater if thread is still alive
			// UPDATER: Loop through all files in update folder and copy file one by one to main folder
			// UPDATER: Delete update folder
			// UPDATER: Start main program (this will not start another updater since no update folder is present)
			// UPDATER: Close updater

			// MAIN: Check if there are any new files on Github (use develop or master channel according to user settings)
			// MAIN: Download all new files into new folder called "update"

			bool result = await this.DownloadClientFiles();

			// Initialize helper variable to track which dataGridView is currently shown
			ActiveDGV = this.dataGridView1;

			// Change form name to include version
			Version version = new Version(Application.ProductVersion);
			this.Text = Application.ProductName + " v" + version.Major + "." + version.Minor;

			// Read in command line arguments and pass them to main program
			string[] args = Environment.GetCommandLineArgs().Skip(1).ToArray();
			this.Form1Shown(args);
		}
	}
}
