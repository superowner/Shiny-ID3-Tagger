//-----------------------------------------------------------------------
// <copyright file="Program.cs" company="Shiny ID3 Tagger">
// Copyright (c) Shiny ID3 Tagger. All rights reserved.
// </copyright>
// <author>ShinyId3Tagger Team</author>
// <summary>Code executed when program starts. Assures that only one instance is running</summary>
//-----------------------------------------------------------------------

namespace Shiny_ID3_Tagger
{
	using System;
	using System.Collections.Generic;
	using System.Linq;
	using System.Windows.Forms;
	using Microsoft.VisualBasic.ApplicationServices;

	internal static class Program
	{
		// Code is executed on all program calls (1,2,3,4....)
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
		// Code is executed on all program calls (1,2,3,4....)
		internal SingleInstanceController()
		{
			this.IsSingleInstance = true;
			this.StartupNextInstance += this.This_StartupNextInstance;
		}

		// Code is only executed on the first program call (1)
		protected override void OnCreateMainForm()
		{
			this.MainForm = new Form1();
		}

		// Code is executed on subsequent calls (2,3,4,...)
		private void This_StartupNextInstance(object sender, StartupNextInstanceEventArgs e)
		{
			Form1 form = this.MainForm as Form1;
			string[] args = new List<string>(e.CommandLine).Skip(1).ToArray();
			form.Form1Shown(args);
		}
	}
}
