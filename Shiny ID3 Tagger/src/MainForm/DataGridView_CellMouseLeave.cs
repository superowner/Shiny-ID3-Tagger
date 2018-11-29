//-----------------------------------------------------------------------
// <copyright file="DataGridView_CellMouseLeave.cs" company="Shiny ID3 Tagger">
// Copyright (c) Shiny ID3 Tagger. All rights reserved.
// </copyright>
// <author>ShinyId3Tagger Team</author>
//-----------------------------------------------------------------------

namespace Shiny_ID3_Tagger
{
	using System;
	using System.Drawing;
	using System.Windows.Forms;

	/// <summary>
	/// Represents the MainForm class which contains all methods who interacts with the UI
	/// </summary>
	public partial class MainForm : Form
	{
		/// <summary>
		/// Un-highlights rows after they aren't selected anymore. Opposing method to "DataGridView_CellMouseEnter"
		/// </summary>
		/// <param name="sender">The object which has raised the event</param>
		/// <param name="e">Contains additional information about the event</param>
		private void DataGridView_CellMouseLeave(object sender, DataGridViewCellEventArgs e)
		{
			DataGridView dgv = (DataGridView)sender;
			if (e.RowIndex >= 0)
			{
				if (dgv == this.dataGridView2 &&
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
