//-----------------------------------------------------------------------
// <copyright file="MainForm.cs" company="Shiny ID3 Tagger">
// Copyright (c) Shiny ID3 Tagger. All rights reserved.
// </copyright>
// <author>ShinyId3Tagger Team</author>
// <summary>Executed when program starts. Shows main window</summary>
// https://social.msdn.microsoft.com/Forums/windows/en-US/43a85553-2b94-4f4e-9db3-498311af4ecd/datagridview-sorting-with-null-values?forum=winforms
//-----------------------------------------------------------------------

namespace Shiny_ID3_Tagger
{
	using System;
	using System.Linq;
	using System.Threading;
	using System.Windows.Forms;
	using GlobalVariables;
	using Utils;

	public partial class Form1 : Form
	{
		public static Form1 Instance;

		/// <summary>
		/// Initializes a new instance of the <see cref="Form1"/> class.
		/// The main form which will be shown immediately after program start
		/// </summary>
		public Form1()
		{
			Form1.Instance = this;
			this.InitializeComponent();
		}

		// Code is executed on all program calls (1,2,3,4....)
		internal async void Form1Shown(string[] args)
		{
			// Refresh cancellation token
			GlobalVariables.TokenSource = new CancellationTokenSource();
			CancellationToken cancelToken = GlobalVariables.TokenSource.Token;

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
				GlobalVariables.TokenSource.Cancel();
				this.Form_EnableUI(false);
			}
		}

		// Code is only executed on the first program call (1)
		protected async override void OnShown(EventArgs e)
		{
			base.OnShown(e);

			// Get user settings and credentials
			Utils.ReadSettings();
			Utils.ReadCredentials();

			// Update this program via Github
			bool result = await Utils.DownloadClientFiles();

			// Initialize helper variable to track which dataGridView is currently shown
			GlobalVariables.ActiveDGV = this.dataGridView1;

			// Read in command line arguments and pass them to main program
			string[] args = Environment.GetCommandLineArgs().Skip(1).ToArray();
			this.Form1Shown(args);
		}
	}
}
