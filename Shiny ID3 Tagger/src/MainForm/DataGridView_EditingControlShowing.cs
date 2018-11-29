//-----------------------------------------------------------------------
// <copyright file="DataGridView_EditingControlShowing.cs" company="Shiny ID3 Tagger">
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
		/// Works in combination with DataGridView_CellValidating()
		/// <seealso href="https://www.c-sharpcorner.com/blogs/how-to-create-editable-combobox-cell-in-datagridview1"/>
		/// </summary>
		/// <param name="sender">The object which has raised the event</param>
		/// <param name="e">Contains additional information about the event</param>
		private void DataGridView_EditingControlShowing(object sender, DataGridViewEditingControlShowingEventArgs e)
		{
			if (e.Control is ComboBox cb)
			{
				cb.DropDownStyle = ComboBoxStyle.DropDown;
			}
		}
	}
}
