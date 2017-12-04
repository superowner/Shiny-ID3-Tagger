//-----------------------------------------------------------------------
// <copyright file="MainForm.cs" company="Shiny Id3 Tagger">
//	 Copyright (c) Shiny Id3 Tagger. All rights reserved.
// </copyright>
// <author>ShinyId3Tagger Team</author>
// <summary>Executed when program starts. Shows main window</summary>
// https://social.msdn.microsoft.com/Forums/windows/en-US/43a85553-2b94-4f4e-9db3-498311af4ecd/datagridview-sorting-with-null-values?forum=winforms
//-----------------------------------------------------------------------

// TODO: Move column for album hits to the right/end of table
// TODO: Move column for API duration per track to the right/end of table
// TODO: Add a new column for accumulated API duration which sums up all API durations per track to one big number
// TODO: Add a new option to import CSV files with artist/title info to lookup. So a mp3 folder is not needed
// TODO: Small blue border on mouse hover around row if already selected
namespace GlobalNamespace
{
	using System;
	using System.Linq;
	using System.Threading;	
	using System.Windows.Forms;

	public partial class Form1 : Form
	{
		public Form1()
		{
			this.InitializeComponent();
		}
		
		// ###########################################################################
		internal async void Form1Shown(string[] args)
		{						
			// Refresh cancel token
			TokenSource = new CancellationTokenSource();
			CancellationToken cancelToken = TokenSource.Token;
			
			ActiveDGV = this.dataGridView1;
			
			Version version = new Version(Application.ProductVersion);
			this.Text = Application.ProductName + " " + version.Major + "." + version.Minor;
			
			bool successReadingVariables = this.ReadAccountCredentials();
			if (successReadingVariables)
			{
				// Add new files
				bool newFiles = await this.AddFiles(args, cancelToken);
				
				// If the setting allows it and new files were added (dialog not canceled or files were already added), continue straight with searching
				if (User.Settings["AutoSearch"] && newFiles && !cancelToken.IsCancellationRequested)
				{
					this.StartSearching(cancelToken);
				}				
			}
			else
			{
				MessageBox.Show(
					"Could not read all user settings. Program closing",
					"Could not read all user settings",
					MessageBoxButtons.OK,
					MessageBoxIcon.Question,
					MessageBoxDefaultButton.Button1,
					new MessageBoxOptions());
				
				Application.Exit();
			}
		}

		// ###########################################################################
		protected override void OnShown(EventArgs e)
		{
			base.OnShown(e);
			string[] args = Environment.GetCommandLineArgs().Skip(1).ToArray();
			this.Form1Shown(args);
		}
	}
}
