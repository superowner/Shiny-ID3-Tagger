//-----------------------------------------------------------------------
// <copyright file="DataGridView_CellMouseLeave.cs" company="Shiny Id3 Tagger">
//	 Copyright (c) Shiny Id3 Tagger. All rights reserved.
// </copyright>
// <author>ShinyId3Tagger Team</author>
// <summary>De-highlights rows after they aren't selected anymore. Opposing method to "DataGridView_CellMouseEnter"</summary>
//-----------------------------------------------------------------------

namespace GlobalNamespace
{
	using System;
	using System.Drawing;
	using System.Windows.Forms;

	public partial class Form1 : Form
	{
		private void DataGridView_CellMouseLeave(object sender, DataGridViewCellEventArgs e)
		{
			DataGridView dgv = (DataGridView)sender;
			if (e.RowIndex >= 0)
			{
				// Ugly workaround until I found a better way to preserve yellow background color for result rows
				if (dgv.Name.ToLowerInvariant() == "dataGridView2".ToLowerInvariant() &&
					dgv.Rows[e.RowIndex].Cells[this.service2.Index].Value.ToString().ToUpperInvariant() == "RESULT")
				{
					dgv.Rows[e.RowIndex].DefaultCellStyle.BackColor = Color.Yellow;
				}
				else
				{
					Color foreColor = dgv.Rows[e.RowIndex].DefaultCellStyle.ForeColor;
					dgv.Rows[e.RowIndex].DefaultCellStyle = null;
					dgv.Rows[e.RowIndex].DefaultCellStyle.ForeColor = foreColor;
				}
			}
		}
	}
}