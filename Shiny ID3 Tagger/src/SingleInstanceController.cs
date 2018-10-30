//-----------------------------------------------------------------------
// <copyright file="SingleInstanceController.cs" company="Shiny ID3 Tagger">
// Copyright (c) Shiny ID3 Tagger. All rights reserved.
// </copyright>
// <author>ShinyId3Tagger Team</author>
//-----------------------------------------------------------------------

namespace Shiny_ID3_Tagger
{
	using System;
	using System.Collections.Generic;
	using System.Linq;
	using Microsoft.VisualBasic.ApplicationServices;

	/// <summary>
	/// Represents the SingleInstanceController class which is used to assure,
	/// that only one instance of the program is running at every time
	/// </summary>
	internal class SingleInstanceController : WindowsFormsApplicationBase
	{
		/// <summary>
		/// Initializes a new instance of the <see cref="SingleInstanceController"/> class.
		/// Method is executed on all program calls (1,2,3,4....)
		/// </summary>
		internal SingleInstanceController()
		{
			this.IsSingleInstance = true;
			this.StartupNextInstance += this.This_StartupNextInstance;
		}

		/// <summary>
		/// Method is only executed on the first program call (1)
		/// </summary>
		protected override void OnCreateMainForm()
		{
			this.MainForm = new Form1();
		}

		/// <summary>
		/// Method is executed on all subsequent calls (2,3,4,...)
		/// </summary>
		/// <param name="sender">default parameter, not used</param>
		/// <param name="e">default parameter which holds all command line arguments</param>
		private void This_StartupNextInstance(object sender, StartupNextInstanceEventArgs e)
		{
			Form1 form = this.MainForm as Form1;
			string[] args = new List<string>(e.CommandLine).Skip(1).ToArray();
			form.Form1Shown(args);
		}
	}
}
