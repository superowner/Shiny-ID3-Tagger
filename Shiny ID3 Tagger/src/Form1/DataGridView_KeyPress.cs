//-----------------------------------------------------------------------
// <copyright file="DataGridView_KeyPress.cs" company="Shiny ID3 Tagger">
// Copyright (c) Shiny ID3 Tagger. All rights reserved.
// </copyright>
// <author>ShinyId3Tagger Team</author>
//-----------------------------------------------------------------------

namespace Shiny_ID3_Tagger
{
	using System;
	using System.Windows.Forms;
	using GlobalVariables;

	/// <summary>
	/// Represents the Form1 class which contains all methods who interacts with the UI
	/// </summary>
	public partial class Form1 : Form
	{
		/// <summary>
		/// Unselect all selected rows within a dataGridView
		/// </summary>
		/// <param name="sender">The object which has raised the event</param>
		/// <param name="e">Contains additional information about the event</param>
		private void DataGridView_KeyPress(object sender, KeyPressEventArgs e)
		{
			// Unselect all selected rows within a dataGridView when ESC is pressed
			if ((Keys)e.KeyChar == Keys.Escape)
			{
				DataGridView dgv = (DataGridView)sender;
				dgv.ClearSelection();
			}

			// Toggles the checkboxes for column "Save" when space is pressed AND datagridview1 is active
			if ((Keys)e.KeyChar == Keys.Space && GlobalVariables.ActiveDGV == this.dataGridView1)
			{
				DataGridView dgv = (DataGridView)sender;
				bool checkBoxChanged = false;

				// By default all checkboxes are set to unchecked
				checkBoxChanged = this.CheckBox_Toggle(dgv, this.hasNewValues.Index, false);

				// But if not a single checkbox within the selected rows were toggled from checked to unchecked,
				// then all checkboxes were already unchecked. Set all to checked then
				if (checkBoxChanged == false)
				{
					this.CheckBox_Toggle(dgv, this.hasNewValues.Index, true);
				}
			}
		}

		// Toggle checkboxes to a desired value
		private bool CheckBox_Toggle(DataGridView dgv, int columnIndex, bool desiredValue)
		{
			bool checkBoxChanged = false;

			foreach (DataGridViewRow curRow in dgv.SelectedRows)
			{
				DataGridViewCheckBoxCell checkBoxCell = (DataGridViewCheckBoxCell)curRow.Cells[columnIndex];

				if ((bool)checkBoxCell.EditedFormattedValue == !desiredValue)
				{
					checkBoxChanged = true;
					checkBoxCell.Value = desiredValue;
				}
			}

			return checkBoxChanged;
		}
	}
}
