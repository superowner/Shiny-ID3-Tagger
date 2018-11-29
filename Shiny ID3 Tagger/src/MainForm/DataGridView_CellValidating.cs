//-----------------------------------------------------------------------
// <copyright file="DataGridView_CellValidating.cs" company="Shiny ID3 Tagger">
// Copyright (c) Shiny ID3 Tagger. All rights reserved.
// </copyright>
// <author>ShinyId3Tagger Team</author>
//-----------------------------------------------------------------------

namespace Shiny_ID3_Tagger
{
	using System.Windows.Forms;

	/// <summary>
	/// Represents the MainForm class which contains all methods who interacts with the UI
	/// </summary>
	public partial class MainForm : Form
	{
		/// <summary>
		/// Enables custom user input into datagridview combo boxes
		/// Works in combination with DataGridView_EditingControlShowing()
		/// <seealso href="https://www.c-sharpcorner.com/blogs/how-to-create-editable-combobox-cell-in-datagridview1"/>
		/// </summary>
		/// <param name="sender">The object which has raised the event</param>
		/// <param name="e">Contains additional information about the event</param>
		private void DataGridView_CellValidating(object sender, DataGridViewCellValidatingEventArgs e)
		{
			try
			{
				DataGridView dgv = (DataGridView)sender;
				if (dgv.CurrentCell is DataGridViewComboBoxCell cell && cell.Items.Contains(e.FormattedValue) == false)
				{
					cell.Items.Insert(0, e.FormattedValue);
					if (dgv.IsCurrentCellDirty)
					{
						dgv.CommitEdit(DataGridViewDataErrorContexts.Commit);
					}

					cell.Value = cell.Items[0];
				}
			}
			catch
			{
			}
		}
	}
}
