//-----------------------------------------------------------------------
// <copyright file="Popup_RemoveClick.cs" company="Shiny ID3 Tagger">
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
	/// Represents the Form1 class which contains all methods who interacts with the UI
	/// </summary>
	public partial class Form1 : Form
	{
		/// <summary>
		/// Remove all selected rows from active datagridview when using the context menu "Remove lines"
		/// </summary>
		/// <param name="sender">The object which has raised the event</param>
		/// <param name="e">Contains additional information about the event</param>
		private void Popup_RemoveClick(object sender, EventArgs e)
		{
			foreach (DataGridViewRow row in GlobalVariables.ActiveDGV.SelectedRows)
			{
				GlobalVariables.ActiveDGV.Rows.Remove(row);
			}
		}
	}
}
