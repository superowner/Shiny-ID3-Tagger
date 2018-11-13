//-----------------------------------------------------------------------
// <copyright file="MainForm.cs" company="Shiny ID3 Tagger">
// Copyright (c) Shiny ID3 Tagger. All rights reserved.
// </copyright>
// <author>ShinyId3Tagger Team</author>
//-----------------------------------------------------------------------

namespace Shiny_ID3_Tagger
{
	using System;
	using System.Linq;
	using System.Reflection;
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

			// Get user settings and user accounts
			User.Settings = Utils.ReadConfig(@"config\settings.json", @"config\schemas\settings.schema.json");
			User.Accounts = Utils.ReadConfig(@"config\accounts.json", @"config\schemas\accounts.schema.json");

			// If user settings or user accounts are not available: Don't continue, disable UI, let user read error message
			if (User.Accounts == null || User.Settings == null)
			{
				GlobalVariables.TokenSource.Cancel();
				this.Form_EnableUI(false);
			}
			else
			{
				// If AutoUpdate is enabled, update client files
				if ((bool)User.Settings["AutoUpdate"])
				{
					bool successDownload = await Utils.CheckAndDownloadUpdate();
					if (successDownload)
					{
						// Wait for UpdateClient.exe to say it's ready to deploy new program files
						Utils.StartUpdateClient();
					}
				}

				// If AutoCollectFiles is enabled
				if ((bool)User.Settings["AutoCollectFiles"])
				{
					// Add new files to dataGridView1
					bool hasNewFiles = await this.CollectFiles(args, cancelToken);

					// If new files were added and AutoSearch is enabled, start searching
					if ((bool)User.Settings["AutoSearch"] && hasNewFiles)
					{
						this.StartSearching(cancelToken);
					}
				}
			}
		}

		/// <summary>
		/// Method is executed only once regardless how many times the program is called
		/// </summary>
		/// <param name="e">Default parameter which holds event arguments</param>
		protected override void OnShown(EventArgs e)
		{
			base.OnShown(e);

			// Set icon programatically. (Had issues doing it in MainForm design view)
			this.Icon = Shiny_ID3_Tagger.properties.Resources.icon_main;

			// Initialize helper variable to track which dataGridView is currently shown
			GlobalVariables.ActiveDGV = this.dataGridView1;

			// Set dataGridView property "DoubleBuffered" to true
			Type dgvType = this.dataGridView1.GetType();
			PropertyInfo pi = dgvType.GetProperty("DoubleBuffered", BindingFlags.Instance | BindingFlags.NonPublic);
			pi.SetValue(this.dataGridView1, true, null);
			pi.SetValue(this.dataGridView2, true, null);

			// Read in command line arguments and pass them to main program
			// First argument is always the path to program executable itself, skip it)
			string[] args = Environment.GetCommandLineArgs().Skip(1).ToArray();

			this.Form1Shown(args);
		}
	}
}
