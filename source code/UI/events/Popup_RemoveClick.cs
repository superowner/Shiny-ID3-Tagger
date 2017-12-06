//-----------------------------------------------------------------------
// <copyright file="Popup_RemoveClick.cs" company="Shiny Id3 Tagger">
//	 Copyright (c) Shiny Id3 Tagger. All rights reserved.
// </copyright>
// <author>ShinyId3Tagger Team</author>
// <summary>Removes all selected rows from the active datagridview when using the context menu "Remove lines"</summary>
//-----------------------------------------------------------------------

namespace GlobalNamespace
{
	using System;
	using System.Linq;
	using System.Windows.Forms;

	public partial class Form1 : Form
	{
		private void Popup_RemoveClick(object sender, EventArgs e)
		{
			foreach (DataGridViewRow row in ActiveDGV.SelectedRows)
			{
				ActiveDGV.Rows.Remove(row);
			}
		}
	}
}
