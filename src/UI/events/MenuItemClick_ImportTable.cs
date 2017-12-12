//-----------------------------------------------------------------------
// <copyright file="MenuItemClick_ImportTable.cs" company="Shiny ID3 Tagger">
// Copyright (c) Shiny ID3 Tagger. All rights reserved.
// </copyright>
// <author>ShinyId3Tagger Team</author>
// <summary>Imports a CSV file</summary>
// https://stackoverflow.com/a/3508572/935614
//-----------------------------------------------------------------------

namespace GlobalNamespace
{
	using System;
	using System.Data;
	using System.Globalization;
	using System.Linq;
	using System.Windows.Forms;
	using Microsoft.VisualBasic.FileIO;

	public partial class Form1 : Form
	{
		private void MenuItemClick_ImportTable(object sender, EventArgs e)
		{
			// Get current system separator (comma or semicolon) and UTF8 encoder
			string seperator = CultureInfo.CurrentCulture.TextInfo.ListSeparator;

			// Set dialog properties like start folder and extension filter
			using (OpenFileDialog dialog = new OpenFileDialog()
			{
				Filter = "CSV files (*.csv)|*.csv|All files (*.*)|*.*",
				InitialDirectory = Environment.GetFolderPath(Environment.SpecialFolder.Desktop),
			})
			{
				// Open dialog and save selected path
				if (dialog.ShowDialog() == DialogResult.OK)
				{
					string fullPath = dialog.FileName;

					// Per default CSV parser detects encoding and byte order marker (BOM). If encoding can't be detected it uses UTF8
					using (TextFieldParser parser = new TextFieldParser(fullPath))
					{
						parser.TextFieldType = FieldType.Delimited;
						parser.HasFieldsEnclosedInQuotes = true;
						parser.SetDelimiters(seperator);

						try
						{
							// Check if first line in CSV has same headers as dataGridView1 (column order is important)
							string[] csvHeaderFields = parser.ReadFields();
							string[] dgvHeaderFields = this.dataGridView1.Columns.Cast<DataGridViewColumn>().Select(column => column.HeaderText).ToArray<string>();

							if (csvHeaderFields.SequenceEqual(dgvHeaderFields))
							{
								// Read in CSV line by line until end of file (EOF) is reached, header row is already skipped
								while (!parser.EndOfData)
								{
									// Convert current line as array
									var fields = parser.ReadFields().ToArray();

									// Update "number" with new line number
									fields[0] = (this.dataGridView1.Rows.Count + 1).ToString(cultEng);

									// Check if file was already added
									bool rowAlreadyExists = (from row in this.dataGridView1.Rows.Cast<DataGridViewRow>()
															 where row.Cells[this.filepath1.Index].Value.ToString().ToLowerInvariant() == fields[1].ToLowerInvariant()
															 select row).Any();

									// Check if file is a valid mp3 file
									if (!rowAlreadyExists)
									{
										this.dataGridView1.Rows.Add(fields);
									}
								}
							}
							else
							{
								// CSV header row doesn't match datagridView1 headers
								if (User.Settings["DebugLevel"] >= 2)
								{
									string[] errorMsg =
									{
										"ERROR:    Could not parse CSV file \"" + fullPath + "\"",
										"Message:  Header row doesn't match required headers. Look for differences and change them in your CSV file",
										"CSV:      " + string.Join(seperator, csvHeaderFields),
										"Required: " + string.Join(seperator, dgvHeaderFields)
									};
									this.PrintLogMessage("error", errorMsg);
								}
							}
						}
						catch (MalformedLineException ex)
						{
							// Malformed CSV values somewhere which couldn't be parsed
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
