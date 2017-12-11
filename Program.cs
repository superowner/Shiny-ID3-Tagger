//-----------------------------------------------------------------------
// <copyright file="Program.cs" company="Shiny Id3 Tagger">
//	 Copyright (c) Shiny Id3 Tagger. All rights reserved.
// </copyright>
// <author>ShinyId3Tagger Team</author>
// <summary>Code executed when program starts. Assures that only one instance is running</summary>
//-----------------------------------------------------------------------

// TODO: Move cover download to new function. Don't clear background color for cover cell when false is returned
// TODO: User option to choose ID3v2.3 (UTF16, Windows) or ID3v2.4 (UTF8, Mac)
// TODO: User option to add ID3v1 additionally to ID3v2
// TODO: User option to choose if unknown tags should be removed
// TODO: User option to add custom RegEx patterns to detect artist/title from filename i.e. "^(\d+\s?)(-\s+?)(?<artist>.*\w+)\s+-\s+(?<title>\w+.*)$"
// TODO: Small blue border on mouse hover around row if already selected

namespace GlobalNamespace
{
	using System;
	using System.Collections.Generic;
	using System.Linq;
	using System.Windows.Forms;
	using Microsoft.VisualBasic.ApplicationServices;

	internal static class Program
	{
		[STAThread]
		private static void Main()
		{
			Application.EnableVisualStyles();
			Application.SetCompatibleTextRenderingDefault(false);
			string[] args = Environment.GetCommandLineArgs();
			SingleInstanceController controller = new SingleInstanceController();
			controller.Run(args);
		}
	}

	internal class SingleInstanceController : WindowsFormsApplicationBase
	{
		public SingleInstanceController()
		{
			this.IsSingleInstance = true;
			this.StartupNextInstance += this.This_StartupNextInstance;
		}

		protected override void OnCreateMainForm()
		{
			this.MainForm = new Form1();
		}

		private void This_StartupNextInstance(object sender, StartupNextInstanceEventArgs e)
		{
			Form1 form = MainForm as Form1;
			string[] args = new List<string>(e.CommandLine).Skip(1).ToArray();
			form.Form1Shown(args);
		}
	}
}
