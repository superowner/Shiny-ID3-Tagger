//-----------------------------------------------------------------------
// <copyright file="MainForm.cs" company="Shiny ID3 Tagger">
// Copyright (c) Shiny ID3 Tagger. All rights reserved.
// </copyright>
// <author>ShinyId3Tagger Team</author>
// <summary>Executed when program starts. Shows main window</summary>
//-----------------------------------------------------------------------

namespace Shiny_ID3_Tagger
{
	using System;
	using System.Linq;
	using System.Threading;
	using System.Windows.Forms;
	using GlobalVariables;
	using Utils;

	/// <summary>
	/// Represents the Form1 class which contains all methods who interacts with the UI
	/// </summary>
	public partial class Form1 : Form
	{
		/// <summary>
		/// Initializes a new instance of the <see cref="Form1"/> class.
		/// The main form which will be shown immediately after program start
		/// </summary>
		public Form1()
		{
			Form1.Instance = this;
			this.InitializeComponent();
		}

		/// <summary>
		/// Gets or sets the single instance of the the main form to get public access from other classes like Utils class
		/// </summary>
		public static Form1 Instance { get; set; }

		/// <summary>
		/// Method is executed every time the program is called
		/// </summary>
		/// <param name="args">Default parameter which holds event arguments</param>
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

		/// <summary>
		/// Method is executed only once regardless how many times the program is called
		/// </summary>
		/// <param name="e">Default parameter which holds event arguments</param>
		protected async override void OnShown(EventArgs e)
		{
			base.OnShown(e);

			// Get user settings and user accounts. Needs to be done before UpdateClient
			User.Settings = Utils.ReadConfig(@"config\settings.json", @"config\schemas\settings.schema.json");
			User.Accounts = Utils.ReadConfig(@"config\accounts.json", @"config\schemas\accounts.schema.json");

			// Update this program via Github
			await Utils.UpdateClient();

			// Initialize helper variable to track which dataGridView is currently shown
			GlobalVariables.ActiveDGV = this.dataGridView1;

			// Read in command line arguments and pass them to main program
			// First argument is always the path to program executable itself, skip it)
			string[] args = Environment.GetCommandLineArgs().Skip(1).ToArray();

			this.Form1Shown(args);
		}
	}
}
