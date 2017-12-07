//-----------------------------------------------------------------------
// <copyright file="MenuItemClick_ImportTable.cs" company="Shiny Id3 Tagger">
//	 Copyright (c) Shiny Id3 Tagger. All rights reserved.
// </copyright>
// <author>ShinyId3Tagger Team</author>
// <summary>Imports a CSV file</summary>
// https://stackoverflow.com/a/3508572/935614
//-----------------------------------------------------------------------

namespace GlobalNamespace
{
	using Microsoft.VisualBasic.FileIO;
	using System;
	using System.Data;
	using System.Diagnostics;
	using System.Globalization;
	using System.Text;
	using System.Windows.Forms;

	public partial class Form1 : Form
	{
		private void MenuItemClick_ImportTable(object sender, EventArgs e)
		{
			// Use current system separator (comma or semicolon)
			string seperator = CultureInfo.CurrentCulture.TextInfo.ListSeparator;
			DataTable importedCSV = Id3.CreateId3Table();

			// Set dialog properties like start folder and extension filter
			using (OpenFileDialog dialog = new OpenFileDialog()
			{
				Filter = "CSV files (*.csv)|*.csv|All files (*.*)|*.*",
				InitialDirectory = Environment.GetFolderPath(Environment.SpecialFolder.Desktop),
			})
			{
				// Open dialog and save the selected path
				if (dialog.ShowDialog() == DialogResult.OK)
				{
					string fullPath = dialog.FileName;
					Encoding utf8WithBom = new UTF8Encoding(true);

					using (TextFieldParser parser = new TextFieldParser(fullPath, utf8WithBom, true))
					{
						// Try to read in CSV line by line until EOF is reached
						parser.SetDelimiters(seperator);
						try
						{
							while (!parser.EndOfData)
							{
								// Split line by delimiter
								string[] fields = parser.ReadFields();
								foreach (string field in fields)
								{
									Debug.WriteLine(field);
								}
							}
						}
						catch (MalformedLineException ex)
						{
							if (User.Settings["DebugLevel"] >= 2)
							{
								string[] errorMsg =
								{
									"ERROR:    Could not parse CSV \"" + fullPath + "\"",
									"Message:  " + ex.Message.TrimEnd('\r', '\n')
								};
							this.PrintLogMessage("error", errorMsg);
							}
						}
					}
				}
			}
		}
	}
}
