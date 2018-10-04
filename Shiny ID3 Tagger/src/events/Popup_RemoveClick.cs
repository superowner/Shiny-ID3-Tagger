//-----------------------------------------------------------------------
// <copyright file="Popup_RemoveClick.cs" company="Shiny ID3 Tagger">
// Copyright (c) Shiny ID3 Tagger. All rights reserved.
// </copyright>
// <author>ShinyId3Tagger Team</author>
// <summary>Remove all selected rows from active datagridview when using the context menu "Remove lines"</summary>
//-----------------------------------------------------------------------

namespace GlobalNamespace
{
	using System;
	using System.Windows.Forms;
	using GlobalVariables;

	public partial class Form1 : Form
	{
		private void Popup_RemoveClick(object sender, EventArgs e)
		{
			foreach (DataGridViewRow row in GlobalVariables.ActiveDGV.SelectedRows)
			{
				GlobalVariables.ActiveDGV.Rows.Remove(row);
			}
		}
	}
}
