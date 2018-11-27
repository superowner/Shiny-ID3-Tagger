//-----------------------------------------------------------------------
// <copyright file="DataGridView_CellMouseEnter.cs" company="Shiny ID3 Tagger">
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
		/// Highlights rows after they are selected. Opposing method to "DataGridView_CellMouseEnter"
		/// </summary>
		/// <param name="sender">The object which has raised the event</param>
		/// <param name="e">Contains additional information about the event</param>
		private void DataGridView_CellMouseEnter(object sender, DataGridViewCellEventArgs e)
		{
			DataGridView dgv = (DataGridView)sender;
			if (e.RowIndex >= 0)
			{
				dgv.Rows[e.RowIndex].DefaultCellStyle.BackColor = Color.FromArgb(229, 243, 255);
			}
		}
	}
}
