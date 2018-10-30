//-----------------------------------------------------------------------
// <copyright file="DataGridView_CellMouseDown.cs" company="Shiny ID3 Tagger">
// Copyright (c) Shiny ID3 Tagger. All rights reserved.
// </copyright>
// <author>ShinyId3Tagger Team</author>
//-----------------------------------------------------------------------

namespace Shiny_ID3_Tagger
{
	using System;
	using System.Windows.Forms;

	/// <summary>
	/// Represents the Form1 class which contains all methods who interacts with the UI
	/// </summary>
	public partial class Form1 : Form
	{
		/// <summary>
		/// >Selects a new row if right clicking on a row which is not already selected
		/// </summary>
		/// <param name="sender">The object which has raised the event</param>
		/// <param name="e">Contains additional information about the event</param>
		private void DataGridView_CellMouseDown(object sender, DataGridViewCellMouseEventArgs e)
		{
			if (e.RowIndex != -1 && e.Button == MouseButtons.Right)
			{
				DataGridViewCell cell = (sender as DataGridView)[e.ColumnIndex, e.RowIndex];
				if (!cell.Selected)
				{
					cell.DataGridView.ClearSelection();
					cell.DataGridView.CurrentCell = cell;
					cell.Selected = true;
				}
			}
		}
	}
}
