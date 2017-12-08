//-----------------------------------------------------------------------
// <copyright file="DataGridView_KeyPress.cs" company="Shiny Id3 Tagger">
//	 Copyright (c) Shiny Id3 Tagger. All rights reserved.
// </copyright>
// <author>ShinyId3Tagger Team</author>
// <summary>Clears a selected row/rows when pressing ESC key</summary>
//-----------------------------------------------------------------------

namespace GlobalNamespace
{
	using System;
	using System.Windows.Forms;

	public partial class Form1 : Form
	{
		private void DataGridView_KeyPress(object sender, KeyPressEventArgs e)
		{
			if (e.KeyChar == (char)Keys.Escape)
			{
				DataGridView dgv = (DataGridView)sender;
				dgv.ClearSelection();
			}
		}
	}
}