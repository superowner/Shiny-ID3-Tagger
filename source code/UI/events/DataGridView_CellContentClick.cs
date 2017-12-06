//-----------------------------------------------------------------------
// <copyright file="DataGridView_CellContentClick.cs" company="Shiny Id3 Tagger">
//	 Copyright (c) Shiny Id3 Tagger. All rights reserved.
// </copyright>
// <author>ShinyId3Tagger Team</author>
// <summary>Starts the browser associated with URLs when clicking on them</summary>
//-----------------------------------------------------------------------

namespace GlobalNamespace
{
	using System;
	using System.Diagnostics;
	using System.Linq;
	using System.Windows.Forms;

	public partial class Form1 : Form
	{
		private void DataGridView_CellContentClick(object sender, DataGridViewCellEventArgs e)
		{
			DataGridView dgv = (DataGridView)sender;
			if (dgv.Columns[e.ColumnIndex] is DataGridViewLinkColumn && e.RowIndex >= 0)
			{
				string url = (string)dgv.Rows[e.RowIndex].Cells[e.ColumnIndex].Value;
				if (Helper.IsValidUrl(url))
				{
					Process.Start(url);
				}
				else
				{
					string[] errorMsg =
					{
						"ERROR:    Invalid URL found: " + url
					};
					this.PrintLogMessage("error", errorMsg);
				}
			}
		}
	}
}