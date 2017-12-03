//-----------------------------------------------------------------------
// <copyright file="MenuItemClick_ExportCSV.cs" company="Shiny Id3 Tagger">
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
		private void MenuItemClick_ExportCSV(object sender, EventArgs e)
		{
			StringBuilder csvContent = new StringBuilder();
			string seperator = CultureInfo.CurrentCulture.TextInfo.ListSeparator;

			IEnumerable<DataGridViewColumn> headers = ActiveDGV.Columns.Cast<DataGridViewColumn>();
			csvContent.AppendLine(string.Join(seperator, headers.Select(column => WellFormedCsvValue(column.HeaderCell.Value)).ToArray()));

			foreach (DataGridViewRow row in ActiveDGV.Rows)
			{
				IEnumerable<DataGridViewCell> cells = row.Cells.Cast<DataGridViewCell>();
				csvContent.AppendLine(string.Join(seperator, cells.Select(cell => WellFormedCsvValue(cell.Value)).ToArray()));
			}

			SaveFileDialog dialog = new SaveFileDialog();
			dialog.Filter = "CSV files (*.csv)|*.csv|All files (*.*)|*.*";
			dialog.FileName = "Shiny ID3 Tagger Export " + DateTime.Now.ToString("yy-MM-dd HH-mm-ss", cultEng);
			dialog.InitialDirectory = Environment.GetFolderPath(Environment.SpecialFolder.Desktop);

			if (dialog.ShowDialog() == DialogResult.OK)
			{
				string filePath = dialog.FileName;
				Encoding utf8WithBom = new UTF8Encoding(true);
				File.WriteAllText(filePath, csvContent.ToString(), utf8WithBom);
			}
		}
	}
}
