//-----------------------------------------------------------------------
// <copyright file="MenuItemClick_ExportTable.cs" company="Shiny ID3 Tagger">
// Copyright (c) Shiny ID3 Tagger. All rights reserved.
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
	using GlobalVariables;
	using Utils;

	public partial class Form1 : Form
	{
		private void MenuItemClick_ExportTable(object sender, EventArgs e)
		{
			// Use current system separator (comma or semicolon)
			string seperator = CultureInfo.CurrentCulture.TextInfo.ListSeparator;
			StringBuilder csvContent = new StringBuilder();

			IEnumerable<DataGridViewColumn> headers = GlobalVariables.ActiveDGV.Columns.Cast<DataGridViewColumn>();
			csvContent.AppendLine(string.Join(seperator, headers.Select(column => Utils.WellFormedCsvValue(column.HeaderCell.Value)).ToArray()));

			foreach (DataGridViewRow row in GlobalVariables.ActiveDGV.Rows)
			{
				IEnumerable<DataGridViewCell> cells = row.Cells.Cast<DataGridViewCell>();
				csvContent.AppendLine(string.Join(seperator, cells.Select(cell => Utils.WellFormedCsvValue(cell.Value)).ToArray()));
			}

			// Set dialog properties like filename, overwrite prompt and start folder
			using (SaveFileDialog dialog = new SaveFileDialog()
			{
				Filter = "CSV files (*.csv)|*.csv|All files (*.*)|*.*",
				FileName = DateTime.Now.ToString("yyMMdd", GlobalVariables.CultEng) + " - Shiny ID3 Tagger Export - " + this.tabControl1.SelectedTab.Text,
				InitialDirectory = Environment.GetFolderPath(Environment.SpecialFolder.Desktop),
				OverwritePrompt = true
			})
			{
				// Open "Save As" dialog and save a text file (extension is CSV) as UTF8 with BOM encoding
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
