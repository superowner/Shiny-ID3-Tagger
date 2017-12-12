//-----------------------------------------------------------------------
// <copyright file="DataGridView_CellDoubleClick.cs" company="Shiny Id3 Tagger">
// Copyright (c) Shiny Id3 Tagger. All rights reserved.
// </copyright>
// <author>ShinyId3Tagger Team</author>
// <summary>Starts the external program associated with mp3 files</summary>
//-----------------------------------------------------------------------

namespace GlobalNamespace
{
	using System;
	using System.Diagnostics;
	using System.Windows.Forms;

	public partial class Form1 : Form
	{
		private void DataGridView_CellDoubleClick(object sender, DataGridViewCellEventArgs e)
		{
			DataGridView dgv = (DataGridView)sender;
			if (e.RowIndex >= 0)
			{
				string filepath = dgv.Rows[e.RowIndex].Cells[this.filepath1.Index].Value.ToString();
				Process.Start(filepath);
			}
		}
	}
}