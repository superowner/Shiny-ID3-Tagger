//-----------------------------------------------------------------------
// <copyright file="DataGridView_CellContentClick.cs" company="Shiny ID3 Tagger">
// Copyright (c) Shiny ID3 Tagger. All rights reserved.
// </copyright>
// <author>ShinyId3Tagger Team</author>
//-----------------------------------------------------------------------

namespace Shiny_ID3_Tagger
{
	using System.Diagnostics;
	using System.Windows.Forms;
	using Utils;

	/// <summary>
	/// Represents the MainForm class which contains all methods who interacts with the UI
	/// </summary>
	public partial class MainForm : Form
	{
		/// <summary>
		/// Starts browser which is associated with URL protocol when clicked on a link in DataGridViewLinkColumn
		/// </summary>
		/// <param name="sender">The object which has raised the event</param>
		/// <param name="e">Contains additional information about the event</param>
		private void DataGridView_CellContentClick(object sender, DataGridViewCellEventArgs e)
		{
			DataGridView dgv = (DataGridView)sender;
			if (e.RowIndex >= 0 && dgv.Columns[e.ColumnIndex] is DataGridViewLinkColumn)
			{
				string url = (string)dgv.Rows[e.RowIndex].Cells[e.ColumnIndex].Value;

				if (string.IsNullOrWhiteSpace(url) == true)
				{
					return;
				}

				if (Utils.IsValidUrl(url) == false)
				{
					string[] errorMsg =
					{
						"ERROR:    Invalid URL",
						"URL:      " + url,
					};
					MainForm.Instance.RichTextBox_LogMessage(errorMsg, 2);
				}

				Process.Start(url);
			}
		}
	}
}
