//-----------------------------------------------------------------------
// <copyright file="TabControl_SelectedIndexChanged.cs" company="Shiny ID3 Tagger">
// Copyright (c) Shiny ID3 Tagger. All rights reserved.
// </copyright>
// <author>ShinyId3Tagger Team</author>
//-----------------------------------------------------------------------

namespace Shiny_ID3_Tagger
{
	using System;
	using System.Windows.Forms;
	using GlobalVariables;

	/// <summary>
	/// Represents the MainForm class which contains all methods who interacts with the UI
	/// </summary>
	public partial class MainForm : Form
	{
		/// <summary>
		/// Set variable "ActiveDGV" accordingly to the active tab (datagridview)
		/// </summary>
		/// <param name="sender">The object which has raised the event</param>
		/// <param name="e">Contains additional information about the event</param>
		private void TabControl_SelectedIndexChanged(object sender, EventArgs e)
		{
			switch (this.tabControl1.SelectedIndex)
			{
				case 0:
					GlobalVariables.ActiveDGV = this.dataGridView1;
					break;
				case 1:
					GlobalVariables.ActiveDGV = this.dataGridView2;
					break;
			}

			GlobalVariables.ActiveDGV.Select();
		}
	}
}
