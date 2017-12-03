//-----------------------------------------------------------------------
// <copyright file="Program.cs" company="Shiny Id3 Tagger">
//	 Copyright (c) Shiny Id3 Tagger. All rights reserved.
// </copyright>
// <author>ShinyId3Tagger Team</author>
// <summary>Code fired when program starts. Assures that only one instance is running</summary>
//-----------------------------------------------------------------------

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
