//-----------------------------------------------------------------------
// <copyright file="Popup_ShowExplorerClick.cs" company="Shiny ID3 Tagger">
// Copyright (c) Shiny ID3 Tagger. All rights reserved.
// </copyright>
// <author>ShinyId3Tagger Team</author>
//-----------------------------------------------------------------------

namespace Shiny_ID3_Tagger
{
	using System;
	using System.Diagnostics;
	using System.IO;
	using System.Windows.Forms;
	using GlobalVariables;

	/// <summary>
	/// Represents the Form1 class which contains all methods who interacts with the UI
	/// </summary>
	public partial class Form1 : Form
	{
		/// <summary>
		/// Open Windows Explorer and navigates to file from active row (Despite multiple rows can be selected, only one row can will be active)
		/// <seealso href="https://stackoverflow.com/q/334630/935614"/>
		/// </summary>
		/// <param name="sender">The object which has raised the event</param>
		/// <param name="e">Contains additional information about the event</param>
		private void Popup_ShowExplorerClick(object sender, EventArgs e)
		{
			if (GlobalVariables.ActiveDGV.CurrentRow != null)
			{
				var cell = GlobalVariables.ActiveDGV.CurrentRow.Cells[this.filepath1.Index];
				string filePath = cell.Value.ToString();

				if (File.Exists(filePath))
				{
					string argument = "/select, \"" + filePath + "\"";
					Process.Start("explorer.exe", argument);
				}
			}
		}
	}
}
