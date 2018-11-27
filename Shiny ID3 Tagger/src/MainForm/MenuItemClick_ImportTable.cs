//-----------------------------------------------------------------------
// <copyright file="MenuItemClick_ImportTable.cs" company="Shiny ID3 Tagger">
// Copyright (c) Shiny ID3 Tagger. All rights reserved.
// </copyright>
// <author>ShinyId3Tagger Team</author>
//-----------------------------------------------------------------------

namespace Shiny_ID3_Tagger
{
	using System;
	using System.Globalization;
	using System.Linq;
	using System.Windows.Forms;
	using GlobalVariables;
	using Microsoft.VisualBasic.FileIO;

	/// <summary>
	/// Represents the MainForm class which contains all methods who interacts with the UI
	/// </summary>
	public partial class MainForm : Form
	{
		/// <summary>
		/// Imports a CSV file to dataGridView1
		/// <seealso href="https://stackoverflow.com/a/3508572/935614"/>
		/// </summary>
		/// <param name="sender">The object which has raised the event</param>
		/// <param name="e">Contains additional information about the event</param>
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

						string[] csvHeaderFields = null;
						string[] dgvHeaderFields = null;
						int i = 1;

						try
						{
							// Check if first line in CSV has same headers as dataGridView1 (column order is important)
							csvHeaderFields = parser.ReadFields();
							dgvHeaderFields = this.dataGridView1.Columns.Cast<DataGridViewColumn>()
									.Select(column => column.HeaderText).ToArray<string>();

							if (csvHeaderFields.SequenceEqual(dgvHeaderFields) == false)
							{
								throw new FormatException("Header row doesn't match the table headers");
							}

							// Read in CSV line by line until end of file (EOF) is reached, header row is already skipped
							while (!parser.EndOfData)
							{
								i++;

								// Convert current line as array
								string[] fields = parser.ReadFields().ToArray();

								// A valid line needs at least two values. fields[1] is the filename
								if (fields.Length < 2)
								{
									throw new MalformedLineException("No filename found in line " + i + " as second value: \"" + string.Join(seperator, fields) + "\"");
								}

								// Update "number" with new line number
								fields[0] = (this.dataGridView1.Rows.Count + 1).ToString(GlobalVariables.CultEng);

								// Check if file was already added
								bool rowAlreadyExists = (from row in this.dataGridView1.Rows.Cast<DataGridViewRow>()
															where row.Cells[this.filepath1.Index].Value.ToString()
																	.ToLowerInvariant() == fields[1].ToLowerInvariant()
															select row).Any();

								if (!rowAlreadyExists)
								{
									this.dataGridView1.Rows.Add(fields);
								}
							}
						}
						catch (MalformedLineException ex)
						{
							// Malformed CSV values somewhere which couldn't be parsed
							string[] errorMsg =
							{
								"ERROR:    Could not parse CSV file \"" + fullPath + "\"",
								"Message:  " + ex.Message.TrimEnd('\r', '\n'),
							};
							MainForm.Instance.RichTextBox_LogMessage(errorMsg, 2);
						}
						catch (FormatException ex)
						{
							// Malformed CSV values somewhere which couldn't be parsed
							string[] errorMsg =
							{
								"ERROR:    Could not parse CSV file \"" + fullPath + "\"",
								"Message:  " + ex.Message,
								"CSV:      " + string.Join(seperator, csvHeaderFields),
								"Required: " + string.Join(seperator, dgvHeaderFields),
							};
							MainForm.Instance.RichTextBox_LogMessage(errorMsg, 2);
						}
					}
				}
			}
		}
	}
}
