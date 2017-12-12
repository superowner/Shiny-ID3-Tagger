//-----------------------------------------------------------------------
// <copyright file="Popup_ShowExplorerClick.cs" company="Shiny ID3 Tagger">
// Copyright (c) Shiny ID3 Tagger. All rights reserved.
// </copyright>
// <author>ShinyId3Tagger Team</author>
// <summary>Open Windows Explorer and navigates to file from active row (Despite multiple rows can be selected, only one row can will be active)</summary>
// https://stackoverflow.com/questions/334630/opening-a-folder-in-explorer-and-selecting-a-file
//-----------------------------------------------------------------------

namespace GlobalNamespace
{
	using System;
	using System.Diagnostics;
	using System.IO;
	using System.Windows.Forms;

	public partial class Form1 : Form
	{
		private void Popup_ShowExplorerClick(object sender, EventArgs e)
		{
			string filePath = ActiveDGV.CurrentRow.Cells[this.filepath1.Index].Value.ToString();

			if (File.Exists(filePath))
			{
				string argument = "/select, \"" + filePath + "\"";
				Process.Start("explorer.exe", argument);
			}
		}
	}
}
