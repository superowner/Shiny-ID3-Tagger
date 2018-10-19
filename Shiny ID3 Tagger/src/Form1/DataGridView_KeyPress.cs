//-----------------------------------------------------------------------
// <copyright file="DataGridView_KeyPress.cs" company="Shiny ID3 Tagger">
// Copyright (c) Shiny ID3 Tagger. All rights reserved.
// </copyright>
// <author>ShinyId3Tagger Team</author>
// <summary>Clears a selected row/rows when pressing ESC key</summary>
//-----------------------------------------------------------------------

namespace Shiny_ID3_Tagger
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
