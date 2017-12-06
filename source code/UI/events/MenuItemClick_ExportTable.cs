//-----------------------------------------------------------------------
// <copyright file="MenuItemClick_ExportTable.cs" company="Shiny Id3 Tagger">
//	 Copyright (c) Shiny Id3 Tagger. All rights reserved.
// </copyright>
// <author>ShinyId3Tagger Team</author>
// <summary>Exports a CSV file</summary>
//-----------------------------------------------------------------------

namespace GlobalNamespace
{
	using System;
	using System.Collections.Generic;
	using System.Globalization;
	using System.IO;
	using System.Linq;
	using System.Text;
	using System.Windows.Forms;

	public partial class Form1 : Form
	{
		private void MenuItemClick_ExportTable(object sender, EventArgs e)
		{
			StringBuilder csvContent = new StringBuilder();
			string seperator = CultureInfo.CurrentCulture.TextInfo.ListSeparator;

			IEnumerable<DataGridViewColumn> headers = ActiveDGV.Columns.Cast<DataGridViewColumn>();
			csvContent.AppendLine(string.Join(seperator, headers.Select(column => Helper.WellFormedCsvValue(column.HeaderCell.Value)).ToArray()));

			foreach (DataGridViewRow row in ActiveDGV.Rows)
			{
				IEnumerable<DataGridViewCell> cells = row.Cells.Cast<DataGridViewCell>();
				csvContent.AppendLine(string.Join(seperator, cells.Select(cell => Helper.WellFormedCsvValue(cell.Value)).ToArray()));
			}

			// Set dialog properties like filename, overwrite prompt and start folder
			using (SaveFileDialog dialog = new SaveFileDialog()
			{
				Filter = "CSV files (*.csv)|*.csv|All files (*.*)|*.*",
				FileName = DateTime.Now.ToString("yyMMdd", cultEng) + " - Shiny ID3 Tagger Export - " + tabControl1.SelectedTab.Text,
				InitialDirectory = Environment.GetFolderPath(Environment.SpecialFolder.Desktop),
				OverwritePrompt = true
			})
			{
				// Open Save as dialog and save a text file (only the extension is CSV) as UTF8 with BOM encoding
				if (dialog.ShowDialog() == DialogResult.OK)
				{
					string fullPath = dialog.FileName;
					Encoding utf8WithBom = new UTF8Encoding(true);
					File.WriteAllText(fullPath, csvContent.ToString(), utf8WithBom);
				}
			}
		}
	}
}
