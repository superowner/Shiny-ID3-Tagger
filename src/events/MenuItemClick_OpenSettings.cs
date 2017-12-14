//-----------------------------------------------------------------------
// <copyright file="MenuItemClick_OpenSettings.cs" company="Shiny ID3 Tagger">
// Copyright (c) Shiny ID3 Tagger. All rights reserved.
// </copyright>
// <author>ShinyId3Tagger Team</author>
// <summary>Open settings.json file</summary>
//-----------------------------------------------------------------------

namespace GlobalNamespace
{
	using System;
	using System.Diagnostics;
	using System.Windows.Forms;

	public partial class Form1 : Form
	{
		private void MenuItemClick_OpenSettings(object sender, EventArgs e)
		{
			string file = AppDomain.CurrentDomain.BaseDirectory + @"\config\settings.json";
			Process.Start(file);
		}
	}
}
