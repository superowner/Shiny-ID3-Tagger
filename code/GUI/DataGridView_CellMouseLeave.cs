//-----------------------------------------------------------------------
// <copyright file="DataGridView_CellMouseLeave.cs" company="Shiny Id3 Tagger">
//	 Copyright (c) Shiny Id3 Tagger. All rights reserved.
// </copyright>
// <author>ShinyId3Tagger Team</author>
// <summary>Unhighlights rows after they aren't selected anymore. Opposing method to "DataGridView_CellMouseEnter"</summary>
//-----------------------------------------------------------------------

namespace GlobalNamespace
{
	using System;
	using System.Drawing;
	using System.Linq;
	using System.Windows.Forms;

	public partial class Form1 : Form
	{
		private void DataGridView_CellMouseLeave(object sender, DataGridViewCellEventArgs e)
		{
			DataGridView dgv = (DataGridView)sender;
			if (e.RowIndex >= 0)
			{
				// Ugly workaround until I found a better way to preserve yellow background color for result rows
				if (dgv.Name == "dataGridView2" && dgv.Rows[e.RowIndex].Cells[this.service2.Index].Value.ToString() == "RESULT")
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