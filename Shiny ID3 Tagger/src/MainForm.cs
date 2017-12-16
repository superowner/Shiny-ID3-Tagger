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

		// ###########################################################################
		internal async void Form1Shown(string[] args)
		{
			// Refresh cancel token
			TokenSource = new CancellationTokenSource();
			CancellationToken cancelToken = TokenSource.Token;

			ActiveDGV = this.dataGridView1;

			Version version = new Version(Application.ProductVersion);
			this.Text = Application.ProductName + " v" + version.Major + "." + version.Minor;

			bool isSuccess = this.ReadConfig();
			if (isSuccess)
			{
				// Add new files
				bool hasNewFiles = await this.AddFiles(args, cancelToken);

				// Continue with searching if user setting allows it and if new files were added
				if ((bool)User.Settings["AutoSearch"] && hasNewFiles)
				{
					this.StartSearching(cancelToken);
				}
			}
			else
			{
				TokenSource.Cancel();
				this.btnAddFiles.Enabled = false;
				this.btnSearch.Enabled = false;
				this.btnWrite.Enabled = false;
				this.menuStrip1.Enabled = false;
			}
		}

		// ###########################################################################

		/// <inheritdoc/>
		protected override void OnShown(EventArgs e)
		{
			base.OnShown(e);
			string[] args = Environment.GetCommandLineArgs().Skip(1).ToArray();
			this.Form1Shown(args);
		}
	}
}
