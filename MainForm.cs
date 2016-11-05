//-----------------------------------------------------------------------
// <copyright file="MainForm.cs" company="Shiny Id3 Tagger">
//	 Copyright (c) Shiny Id3 Tagger. All rights reserved.
// </copyright>
// <author>ShinyId3Tagger Team</author>
// <summary>Code fired when program starts. Shows the main window</summary>
//-----------------------------------------------------------------------

namespace GlobalNamespace
{
	using System;
	using System.Collections.Generic;
	using System.Diagnostics;
	using System.Drawing;
	using System.Globalization;
	using System.IO;
	using System.Linq;
	using System.Text;
	using System.Windows.Forms;
	using Ookii.Dialogs;

	public partial class Form1 : Form
	{
		public Form1()
		{
			this.InitializeComponent();
		}
		
		// ###########################################################################
		internal void Form1Shown(string[] args)
		{						
			Version version = new Version(Application.ProductVersion);
			this.Text = Application.ProductName + " " + version.Major + "." + version.Minor;
			
			Runtime.CultEng = new CultureInfo("en-US");
			Runtime.AlbumHits = new Dictionary<string, int>();			

			bool successReadingVariables = this.ReadUserVariables();
			if (successReadingVariables)
			{
				this.AddFiles(args);
			}
			else
			{
				MessageBox.Show(
					"Could not read all user settings. Program closing",
					"Could not read all user settings",
					MessageBoxButtons.OK,
					MessageBoxIcon.Question,
					MessageBoxDefaultButton.Button1,
					new MessageBoxOptions());
				
				Application.Exit();
			}
		}

		// ###########################################################################
		protected override void OnShown(EventArgs e)
		{
			base.OnShown(e);
			string[] args = Environment.GetCommandLineArgs().Skip(1).ToArray();
			this.Form1Shown(args);
		}		
		
		// ###########################################################################
		private void BtnAddFilesClick(object sender, EventArgs e)
		{
			this.AddFiles(null);
		}

		// ###########################################################################
		private void BtnSearchClick(object sender, EventArgs e)
		{
			this.StartSearching(null);
		}

		// ###########################################################################
		private void BtnWriteClick(object sender, EventArgs e)
		{
			this.StartWriting();
		}

		// ###########################################################################
		private void RichTexBox_LinkClicked(object sender, LinkClickedEventArgs e)
		{
			Process.Start(e.LinkText);
		}
		
		// ###########################################################################
		private void BtnCancelClick(object sender, EventArgs e)
		{
			Runtime.TokenSource.Cancel();
			this.btnCancel.Visible = false;
		}

		// ###########################################################################
		private void ProgressBar1_VisibleChanged(object sender, EventArgs e)
		{
			if (this.progressBar1.Visible)
			{
				this.btnCancel.Visible = true;
			}
			else
			{
				this.btnCancel.Visible = false;
			}
		}
		
		// ###########################################################################
		private void DataGridView_CellMouseEnter(object sender, DataGridViewCellEventArgs e)
		{
			DataGridView dgv = (DataGridView)sender;
			if (e.RowIndex >= 0)
			{				
				dgv.Rows[e.RowIndex].DefaultCellStyle.BackColor = Color.FromArgb(229, 243, 255);
			}		
		}			

		// ###########################################################################
		private void DataGridView_CellMouseLeave(object sender, DataGridViewCellEventArgs e)
		{
			DataGridView dgv = (DataGridView)sender;
			if (e.RowIndex >= 0)
			{	
				// Ugly workaround until I found a better way to preserve yellow background color for result rows
				if (dgv.Name == "dataGridView2" && dgv.Rows[e.RowIndex].Cells[this.service2.Index].Value.ToString() == "RESULT")
				{
					dgv.Rows[e.RowIndex].DefaultCellStyle.BackColor = Color.Yellow;	
				}
				else
				{
					Color foreColor = dgv.Rows[e.RowIndex].DefaultCellStyle.ForeColor;
					dgv.Rows[e.RowIndex].DefaultCellStyle = null;	
					dgv.Rows[e.RowIndex].DefaultCellStyle.ForeColor = foreColor;
				}
			}		
		}
		
		// ###########################################################################
		private void DataGridView_CellContentClick(object sender, DataGridViewCellEventArgs e)
		{
			DataGridView dgv = (DataGridView)sender;
			if (dgv.Columns[e.ColumnIndex] is DataGridViewLinkColumn && e.RowIndex >= 0)
			{
				string url = (string)dgv.Rows[e.RowIndex].Cells[e.ColumnIndex].Value;
				if (IsValidUrl(url))
				{
					Process.Start(url);
				}
				else
				{
					string[] errorMsg =
					{
						"ERROR: Invalid URL: " + url
					};
					this.Log("error", errorMsg);
				}
			}
		}
		
		// ###########################################################################
		private void DataGridView_CellDoubleClick(object sender, DataGridViewCellEventArgs e)
		{
			DataGridView dgv = (DataGridView)sender;
			if (e.RowIndex >= 0)
			{				
				string filepath = dgv.Rows[e.RowIndex].Cells[this.filepath1.Index].Value.ToString();
				Process.Start(filepath);
			}
		}

		// ###########################################################################
		private void DataGridView_KeyPress(object sender, KeyPressEventArgs e)
		{			
			if (e.KeyChar == (char)Keys.Escape)
			{
				DataGridView dgv = (DataGridView)sender;
				dgv.ClearSelection();
			}
		}

		// ###########################################################################
		private void OnDataGridViewSortCompare(object sender, DataGridViewSortCompareEventArgs e)
		{
			string value1 = e.CellValue1 != null ? e.CellValue1.ToString() : string.Empty;
			string value2 = e.CellValue2 != null ? e.CellValue2.ToString() : string.Empty;

			switch (e.Column.HeaderText)
			{
				case "#":
				case "Disc Count":
				case "Disc Number":
				case "Track Count":
				case "Track Number":
					uint outValue1;
					uint outValue2;
					bool result1 = uint.TryParse(value1, out outValue1);
					bool result2 = uint.TryParse(value2, out outValue2);
					if (result1 && result2)
					{
						e.SortResult = decimal.Compare(outValue1, outValue2);
					}

					break;
				case "Date":
					DateTime outDate1 = !string.IsNullOrEmpty(value1) ? this.ConvertStringToDate(value1) : default(DateTime);
					DateTime outDate2 = !string.IsNullOrEmpty(value2) ? this.ConvertStringToDate(value2) : default(DateTime);
					e.SortResult = DateTime.Compare(outDate1, outDate2);
					break;
				default:
					e.SortResult = string.Compare(value1, value2, StringComparison.Ordinal);
					break;
			}

			e.Handled = true;
		}

		// ###########################################################################
		private void AddFiles_MenuItemClick(object sender, EventArgs e)
		{
			VistaFolderBrowserDialog fbd = new VistaFolderBrowserDialog();

			if (Runtime.LastUsedFolder == null)
			{
				Runtime.LastUsedFolder = Environment.GetFolderPath(Environment.SpecialFolder.MyMusic) + @"\";
			}

			fbd.SelectedPath = Runtime.LastUsedFolder;

			if (fbd.ShowDialog() == DialogResult.OK)
			{
				Runtime.LastUsedFolder = fbd.SelectedPath;
				string[] folderpath = { fbd.SelectedPath };
				this.AddFiles(folderpath);
			}
		}

		// ###########################################################################
		private void TabControl1_SelectedIndexChanged(object sender, EventArgs e)
		{
			switch (this.tabControl1.SelectedIndex)
			{
				case 0:
					Runtime.ActiveDGV = this.dataGridView1;
					break;
				case 1:
					Runtime.ActiveDGV = this.dataGridView2;
					break;
			}
	}

		// ###########################################################################
		private void ClearResults_MenuItemClick(object sender, EventArgs e)
		{
			this.dataGridView1.Rows.Clear();
			this.dataGridView2.Rows.Clear();
			Runtime.ActiveDGV.Refresh();
		}

		// ###########################################################################
		private void ExportCSV_MenuItemClick(object sender, EventArgs e)
		{
			StringBuilder csvContent = new StringBuilder();
			string seperator = CultureInfo.CurrentCulture.TextInfo.ListSeparator;

			IEnumerable<DataGridViewColumn> headers = Runtime.ActiveDGV.Columns.Cast<DataGridViewColumn>();
			csvContent.AppendLine(string.Join(seperator, headers.Select(column => WellFormedCsvValue(column.HeaderCell.Value)).ToArray()));

			foreach (DataGridViewRow row in Runtime.ActiveDGV.Rows)
			{
				IEnumerable<DataGridViewCell> cells = row.Cells.Cast<DataGridViewCell>();
				csvContent.AppendLine(string.Join(seperator, cells.Select(cell => WellFormedCsvValue(cell.Value)).ToArray()));
			}

			SaveFileDialog dialog = new SaveFileDialog();
			dialog.Filter = "CSV files (*.csv)|*.csv|All files (*.*)|*.*";
			dialog.FileName = "Shiny ID3 Tagger Export " + DateTime.Now.ToString("yy-MM-dd HH-mm-ss", Runtime.CultEng);
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
