//-----------------------------------------------------------------------
// <copyright file="MainForm.cs" company="Shiny Id3 Tagger">
//	 Copyright (c) Shiny Id3 Tagger. All rights reserved.
// </copyright>
// <author>ShinyId3Tagger Team</author>
// <summary>Executed when program starts. Shows main window</summary>
// https://social.msdn.microsoft.com/Forums/windows/en-US/43a85553-2b94-4f4e-9db3-498311af4ecd/datagridview-sorting-with-null-values?forum=winforms
//-----------------------------------------------------------------------

namespace GlobalNamespace
{
	using System;
	using System.Linq;
	using System.Windows.Forms;

	public partial class Form1 : Form
	{
		public Form1()
		{
			this.InitializeComponent();
		}
		
		// ###########################################################################
		internal void Form1Shown(string[] args)
		{						
			Version version = new Version(Application.ProductVersion);
			this.Text = Application.ProductName + " " + version.Major + "." + version.Minor;
			
			bool successReadingVariables = this.ReadAccountCredentials();
			if (successReadingVariables)
			{
				this.AddFiles(args);
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
