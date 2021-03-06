﻿//-----------------------------------------------------------------------
// <copyright file="MenuItemClick_OpenSettings.cs" company="Shiny ID3 Tagger">
// Copyright (c) Shiny ID3 Tagger. All rights reserved.
// </copyright>
// <author>ShinyId3Tagger Team</author>
//-----------------------------------------------------------------------

namespace Shiny_ID3_Tagger
{
	using System;
	using System.Diagnostics;
	using System.Windows.Forms;

	/// <summary>
	/// Represents the Form1 class which contains all methods who interacts with the UI
	/// </summary>
	public partial class Form1 : Form
	{
		/// <summary>
		/// Open settings.json file
		/// </summary>
		/// <param name="sender">The object which has raised the event</param>
		/// <param name="e">Contains additional information about the event</param>
		private void MenuItemClick_OpenSettings(object sender, EventArgs e)
		{
			string file = AppDomain.CurrentDomain.BaseDirectory + @"\config\settings.user.json";
			Process.Start(file);
		}
	}
}
