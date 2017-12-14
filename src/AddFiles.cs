//-----------------------------------------------------------------------
// <copyright file="AddFiles.cs" company="Shiny ID3 Tagger">
// Copyright (c) Shiny ID3 Tagger. All rights reserved.
// </copyright>
// <author>ShinyId3Tagger Team</author>
// <summary>Method to run when "Add files" button is clicked</summary>
// Tips on how to speed up dataGridView rendering: https://10tec.com/articles/why-datagridview-slow.aspx
// HowTo use Task.Run: https://blog.stephencleary.com/2013/08/taskrun-vs-backgroundworker-round-3.html
//-----------------------------------------------------------------------

namespace GlobalNamespace
{
	using System;
	using System.Collections.Generic;
	using System.Data;
	using System.Diagnostics;
	using System.IO;
	using System.Linq;
	using System.Text.RegularExpressions;
	using System.Threading;
	using System.Threading.Tasks;
	using System.Windows.Forms;

	/// <summary>
	/// Method selects and reads in existing tags and shows them in a dataGridView
	/// </summary>
	public partial class Form1
	{
		// ###########################################################################
		private async Task<bool> AddFiles(string[] newFiles, CancellationToken cancelToken)
		{
			// Work starts, disable all buttons to prevent side effects when user clicks them despite an already running task
			this.btnSearch.Enabled = false;
			this.btnWrite.Enabled = false;
			this.btnAddFiles.Enabled = false;

			// If no files were passed through command line, open a new file dialog. files can be an empty array, so use any() to check this
			if (newFiles == null || !newFiles.Any())
			{
				using (OpenFileDialog dialog = new OpenFileDialog()
				{
					Filter = "MP3 Files|*.mp3",
					Multiselect = true
				})
				{
					if (LastUsedFolder == null)
					{
						LastUsedFolder = Environment.GetFolderPath(Environment.SpecialFolder.MyMusic) + @"\";
					}

					dialog.InitialDirectory = LastUsedFolder;

					if (dialog.ShowDialog() == DialogResult.OK)
					{
						LastUsedFolder = Path.GetDirectoryName(dialog.FileNames[0]);
						newFiles = dialog.FileNames;
					}
				}
			}

			// If anything was selected in previous dialog, check if it was a folder and get it's mp3 files
			if (newFiles != null && newFiles.Any())
			{
				if (Directory.Exists(newFiles[0]))
				{
					newFiles = Directory.GetFiles(newFiles[0], "*.mp3", SearchOption.AllDirectories);
				}
			}

			// Prepare an empty result table
			List<DataGridViewRow> fileTable = new List<DataGridViewRow>();

			// If any files were retrieved previously (either selected in file dialog, from a folder dialog or via command line)
			if (newFiles != null)
			{
				// Prepare progress bar
				this.slowProgressBar.Visible = true;
				this.slowProgressBar.Maximum = newFiles.Length;
				this.slowProgressBar.Value = 0;

				// Gather all existing file paths from first dataGridView for later check if new filePath was already added. (this avoids access to UI in worker)
				HashSet<string> existingFilePaths = new HashSet<string>(from row in this.dataGridView1.Rows.Cast<DataGridViewRow>()
																		select row.Cells[this.filepath1.Index].Value.ToString());

				// Prepare an empty dataGridViewRow as template from first dataGridView. (this avoids access to UI thread in worker thread)
				DataGridViewRow emptyRow = new DataGridViewRow();
				emptyRow.CreateCells(this.dataGridView1);

				// Prepare handler which receives the progress and increases progressBar (update UI thread)
				Progress<int> progressHandler = new Progress<int>(value =>
				{
					this.slowProgressBar.Value = value;
				});

				// Prepare the variable used to send progress from worker thread to GUI thread
				IProgress<int> progress = progressHandler as IProgress<int>;

				// using try/catch and cancellation token is the proper way to cancel a task
				try
				{
					// Start task to collect all file tags
					fileTable = await Task.Run(() =>
					{
						cancelToken.ThrowIfCancellationRequested();
						return this.AddFilesToTable(newFiles, existingFilePaths, emptyRow, progress, cancelToken);
					});

					// Add the list of dataGridViewRows to first dataGridView (update UI thread)
					this.dataGridView1.Rows.AddRange(fileTable.ToArray());
				}
				catch (OperationCanceledException)
				{
					// User pressed Cancel button. Nothing further to do
				}
				catch (Exception error)
				{
					// Error handling when any error occurs during file reading
					if (User.Settings["DebugLevel"] >= 2)
					{
						string[] errorMsg =
						{
							"ERROR:    Could not read all file tags!",
							"Message:  " + error.ToString().TrimEnd('\r', '\n')
						};
						this.PrintLogMessage(this.rtbErrorLog, errorMsg);
					}
				}
			}

			// Work finished, re-enable all buttons and hide progress bar
			this.slowProgressBar.Visible = false;

			this.btnSearch.Enabled = true;
			this.btnWrite.Enabled = true;
			this.btnAddFiles.Enabled = true;

			// Report to parent method if any new files were added
			return fileTable.Any();
		}

		// ###########################################################################
		// Adds a single file to dataTable with all its present ID3 tags
		private List<DataGridViewRow> AddFilesToTable(string[] newFiles, HashSet<string> existingFilePaths, DataGridViewRow emptyRow, IProgress<int> progress, CancellationToken cancelToken)
		{
			int counter = 0;
			List<DataGridViewRow> fileList = new List<DataGridViewRow>();

			// Loop through each file and add it to first dataGridView
			foreach (string filepath in newFiles)
			{
				//// Check if user pressed cancel or ESC
				if (!cancelToken.IsCancellationRequested)
				{
					// Check if file wasn't already added earlier
					if (!existingFilePaths.Contains(filepath))
					{
						// Check if file is a valid mp3 file
						if (this.IsValidMp3(filepath))
						{
							using (TagLib.File tagFile = TagLib.File.Create(filepath, "audio/mp3", TagLib.ReadStyle.None))
							{
								string filename = Path.GetFileNameWithoutExtension(filepath);
								Match match = Regex.Match(filename, @"^(\d+\s)?(-\s+)?(?<artist>.*\w+)\s+-\s+(?<title>\w+.*)$");
								string[] artistChoices = new string[] { null, null, filename };
								string[] titleChoices = new string[] { null, null, filename };

								// Extract artist and title from filename and store them in array at index 0 or 1 according to setting "PreferTags"
								if (match.Success)
								{
									if (User.Settings["PreferTags"])
									{
										artistChoices[1] = match.Groups["artist"].Value;
										titleChoices[1] = match.Groups["title"].Value;
									}
									else
									{
										artistChoices[0] = match.Groups["artist"].Value;
										titleChoices[0] = match.Groups["title"].Value;
									}
								}

								// Extract artist and title from ID3 tags and store them in array at index 0 or 1 according to setting "PreferTags"
								if (User.Settings["PreferTags"])
								{
									artistChoices[0] = tagFile.Tag.FirstPerformer;
									titleChoices[0] = tagFile.Tag.Title;
								}
								else
								{
									artistChoices[1] = tagFile.Tag.FirstPerformer;
									titleChoices[1] = tagFile.Tag.Title;
								}

								// used to report progress and to calculate current row index
								counter++;
								progress.Report(counter);

								// Add values to a new (cloned) DataGridViewRow
								// Use empty string values as fall back when NULL is encountered
								DataGridViewRow row = (DataGridViewRow)emptyRow.Clone();
								row.SetValues(
									(existingFilePaths.Count + counter).ToString(),
									filepath ?? string.Empty,
									artistChoices.FirstOrDefault(s => !string.IsNullOrEmpty(s)) ?? string.Empty,            // Select the first non-null artist choice, value order in array is important therefore
									titleChoices.FirstOrDefault(s => !string.IsNullOrEmpty(s)) ?? string.Empty,             // Select the first non-null title choice, value order in array is important therefore
									tagFile.Tag.Album ?? string.Empty,
									(tagFile.Tag.Year > 0) ? tagFile.Tag.Year.ToString(cultEng) : string.Empty,
									tagFile.Tag.FirstGenre ?? string.Empty,
									(tagFile.Tag.DiscCount > 0) ? tagFile.Tag.DiscCount.ToString(cultEng) : string.Empty,
									(tagFile.Tag.Disc > 0) ? tagFile.Tag.Disc.ToString(cultEng) : string.Empty,
									(tagFile.Tag.TrackCount > 0) ? tagFile.Tag.TrackCount.ToString(cultEng) : string.Empty,
									(tagFile.Tag.Track > 0) ? tagFile.Tag.Track.ToString(cultEng) : string.Empty,
									tagFile.Tag.Lyrics ?? string.Empty,
									tagFile.Tag.Pictures.Any() ? tagFile.Tag.Pictures[0].Description : string.Empty,
									false);

								// Add new DataGridViewRow to a fileList which is returned later
								fileList.Add(row);
							}
						}
					}
				}
			}

			return fileList;
		}
	}
}
