//-----------------------------------------------------------------------
// <copyright file="Program.cs" company="Shiny ID3 Tagger">
// Copyright (c) Shiny ID3 Tagger. All rights reserved.
// </copyright>
// <author>ShinyId3Tagger Team</author>
// <summary>Code executed when program starts. Assures that only one instance is running</summary>
//-----------------------------------------------------------------------

// TODO: Automatic GitHub releases https://marketplace.visualstudio.com/items?itemName=jakobehn.jakobehn-vsts-github-tasks
// TODO: Move cover download to new function. Don't clear background color for cover cell when false is returned (download failed)
// TODO: taglib has a downgradeFrames method. What does it do? http://taglib.org/api/classTagLib_1_1ID3v2_1_1Tag.html#a8d2a42a20fade71c9acb68229445dee8
// TODO: Implement JSON schema to check if settings.json and accounts.json are still valid JSON (user can edit them)
// TODO: User option to en/disable single APIs (sometimes they are down)
// TODO: User option to choose ID3v2.3 (UTF16, Windows) or ID3v2.4 (UTF8, Mac)
// TODO: User option to write ID3v1 tags additionally to ID3v2
// TODO: User option to choose if unknown tags should be removed
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
		internal SingleInstanceController()
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
			Form1 form = this.MainForm as Form1;
			string[] args = new List<string>(e.CommandLine).Skip(1).ToArray();
			form.Form1Shown(args);
		}
	}
}
