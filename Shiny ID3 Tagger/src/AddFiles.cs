//-----------------------------------------------------------------------
// <copyright file="AddFiles.cs" company="Shiny ID3 Tagger">
// Copyright (c) Shiny ID3 Tagger. All rights reserved.
// </copyright>
// <author>ShinyId3Tagger Team</author>
//-----------------------------------------------------------------------

namespace Shiny_ID3_Tagger
{
	using System;
	using System.Collections.Generic;
	using System.IO;
	using System.Linq;
	using System.Text.RegularExpressions;
	using System.Threading;
	using System.Threading.Tasks;
	using System.Windows.Forms;
	using GlobalVariables;
	using Utils;

	/// <summary>
	/// Represents the MainForm class which contains all methods who interacts with the UI
	/// </summary>
	public partial class MainForm
	{
		/// <summary>
		/// Method collects and reads in existing ID3 tags
		/// Shows them in dataGridView1
		/// Runs when "Add files" button is clicked
		/// <seealso href="https://blog.stephencleary.com/2013/08/taskrun-vs-backgroundworker-round-3.html"/>
		/// </summary>
		/// <param name="newFiles">String list of files to add to dataGridView1</param>
		/// <param name="cancelToken">Global cancelation token</param>
		/// <returns>A <see cref="Task"/> representing the asynchronous operation.</returns>
		private async Task<bool> CollectFiles(string[] newFiles, CancellationToken cancelToken)
		{
			// ###########################################################################
			// Work starts, disable all buttons to prevent side effects when user clicks them despite an already running task
			this.Form_EnableUI(false);

			// If no files were passed through command line, open a new file dialog. files can be an empty array, so use any() to check this
			if (newFiles == null || !newFiles.Any())
			{
				using (OpenFileDialog dialog = new OpenFileDialog()
				{
					Filter = "MP3 Files|*.mp3",
					Multiselect = true,
				})
				{
					if (GlobalVariables.LastUsedFolder == null)
					{
						GlobalVariables.LastUsedFolder = Environment.GetFolderPath(Environment.SpecialFolder.MyMusic) + @"\";
					}

					dialog.InitialDirectory = GlobalVariables.LastUsedFolder;

					if (dialog.ShowDialog() == DialogResult.OK)
					{
						GlobalVariables.LastUsedFolder = Path.GetDirectoryName(dialog.FileNames[0]);
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
				HashSet<string> existingFiles = new HashSet<string>(from row in this.dataGridView1.Rows.Cast<DataGridViewRow>()
																		select row.Cells[this.filepath1.Index].Value.ToString());

				// Prepare handler which receives the progress and increases progressBar (update UI thread)
				Progress<int> progressHandler = new Progress<int>(value =>
				{
					this.slowProgressBar.Value = value;
				});

				// Prepare the variable used to send progress from worker thread to GUI thread
				IProgress<int> progress = progressHandler as IProgress<int>;

				// Using try/catch and cancellation token is the proper way to cancel a task
				try
				{
					// Start task to collect all file tags
					fileTable = await Task.Run(() =>
					{
						cancelToken.ThrowIfCancellationRequested();
						return this.AddFilesToTable(newFiles, existingFiles, progress, cancelToken);
					});

					// Add the list of dataGridViewRows to first dataGridView (update UI thread)
					this.dataGridView1.Rows.AddRange(fileTable.ToArray());
				}
				catch (TagLib.CorruptFileException error)
				{
					// Error handling when any error occurs during file reading
					string[] errorMsg =
					{
						"ERROR:    Could not read all file tags! File is corrupt",
						"Message:  " + error.ToString().TrimEnd('\r', '\n'),
					};
					MainForm.Instance.RichTextBox_LogMessage(errorMsg, 2);
				}
				catch (TagLib.UnsupportedFormatException error)
				{
					// Error handling when any error occurs during file reading
					string[] errorMsg =
					{
						"ERROR:    Could not read all file tags! File or tag format is not supported",
						"Message:  " + error.ToString().TrimEnd('\r', '\n'),
					};
					MainForm.Instance.RichTextBox_LogMessage(errorMsg, 2);
				}
				catch (OperationCanceledException)
				{
					// User pressed Cancel button. Nothing further to do
				}
			}

			// Work finished, re-enable all buttons and hide progress bar
			this.slowProgressBar.Visible = false;

			this.Form_EnableUI(true);

			// Report to parent method if any new files were added
			return fileTable.Any();
		}

		/// ###########################################################################
		/// <summary>
		/// Adds a single file with all currents ID3 tags to a dataTable
		/// </summary>
		/// <param name="newFiles">List of paths of all selected and new files to add</param>
		/// <param name="existingFiles">List of all already added file paths</param>
		/// <param name="progress">Variable is used to report the current progress back to parent thread</param>
		/// <param name="cancelToken">Global cancellation token</param>
		/// <returns>A list of dataGridView rows with unique files (old and new ones)</returns>
		private List<DataGridViewRow> AddFilesToTable(
			string[] newFiles,
			HashSet<string> existingFiles,
			IProgress<int> progress,
			CancellationToken cancelToken)
		{
			int rowIndex = 0;
			List<DataGridViewRow> rowList = new List<DataGridViewRow>();

			// Prepare an empty dataGridViewRow as template from first dataGridView
			DataGridViewRow rowTemplate = new DataGridViewRow();
			rowTemplate.CreateCells(MainForm.Instance.dataGridView1);

			// Loop through each file and add it to dataGridView1
			foreach (string filepath in newFiles)
			{
				// Check if user pressed cancel or ESC and if file wasn't already added earlier and if file is a valid mp3 file
				if (cancelToken.IsCancellationRequested == false &&
					existingFiles.Contains(filepath) == false &&
					Utils.IsValidMp3(filepath))
				{
					string filename = Path.GetFileNameWithoutExtension(filepath);
					string[] artistChoices = new[] { null, null, filename };
					string[] titleChoices = new[] { null, null, filename };

					foreach (string pattern in User.Settings["FilenamePatterns"])
					{
						if (Utils.IsValidRegex(pattern))
						{
							// Test each pattern against current filename
							Match match = Regex.Match(filename, pattern);

							// Stop at first successful match
							if (match.Success)
							{
								// Extract artist and title from filename and store them in array at index 0 or 1 according to setting "PreferTags"
								if ((bool)User.Settings["PreferTags"] == false)
								{
									artistChoices[0] = match.Groups["artist"].Value;
									titleChoices[0] = match.Groups["title"].Value;
								}
								else
								{
									artistChoices[1] = match.Groups["artist"].Value;
									titleChoices[1] = match.Groups["title"].Value;
								}

								break;
							}
						}
					}

					using (TagLib.File tagFile = TagLib.File.Create(filepath, "audio/mp3", TagLib.ReadStyle.None))
					{
						// Extract artist and title from ID3 tags and store them in array at index 0 or 1 according to setting "PreferTags"
						if ((bool)User.Settings["PreferTags"])
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
						rowIndex++;
						progress.Report(rowIndex);

						// Prepare a new row, has all columns from datagridview1
						DataGridViewRow newRow = (DataGridViewRow)rowTemplate.Clone();

						// Set filenumber
						newRow.Cells[MainForm.Instance.filenumber1.Index].Value = (existingFiles.Count + rowIndex).ToString();

						// Set filepath
						newRow.Cells[MainForm.Instance.filepath1.Index].Value = filepath ?? string.Empty;

						// Set artist
						DataGridViewComboBoxCell artistCell = newRow.Cells[MainForm.Instance.artist1.Index] as DataGridViewComboBoxCell;
						artistCell.Value = artistChoices.FirstOrDefault(s => !string.IsNullOrEmpty(s)) ?? string.Empty;
						artistCell.Items.Add(artistCell.Value);

						// Set title
						DataGridViewComboBoxCell titleCell = newRow.Cells[MainForm.Instance.title1.Index] as DataGridViewComboBoxCell;
						titleCell.Value = titleChoices.FirstOrDefault(s => !string.IsNullOrEmpty(s)) ?? string.Empty;
						titleCell.Items.Add(titleCell.Value);

						// Set album
						DataGridViewComboBoxCell albumCell = newRow.Cells[MainForm.Instance.album1.Index] as DataGridViewComboBoxCell;
						albumCell.Value = tagFile.Tag.Album ?? string.Empty;
						albumCell.Items.Add(albumCell.Value);

						// Set date
						DataGridViewComboBoxCell dateCell = newRow.Cells[MainForm.Instance.album1.Index] as DataGridViewComboBoxCell;
						dateCell.Value = (tagFile.Tag.Year > 0) ? tagFile.Tag.Year.ToString(GlobalVariables.CultEng) : string.Empty;
						dateCell.Items.Add(dateCell.Value);

						// Set genre
						DataGridViewComboBoxCell genreCell = newRow.Cells[MainForm.Instance.album1.Index] as DataGridViewComboBoxCell;
						genreCell.Value = tagFile.Tag.FirstGenre ?? string.Empty;
						genreCell.Items.Add(genreCell.Value);

						// Set disc count
						DataGridViewComboBoxCell disccountCell = newRow.Cells[MainForm.Instance.album1.Index] as DataGridViewComboBoxCell;
						disccountCell.Value = (tagFile.Tag.DiscCount > 0) ? tagFile.Tag.DiscCount.ToString(GlobalVariables.CultEng) : string.Empty;
						disccountCell.Items.Add(disccountCell.Value);

						// Set disc number
						DataGridViewComboBoxCell discnumberCell = newRow.Cells[MainForm.Instance.album1.Index] as DataGridViewComboBoxCell;
						discnumberCell.Value = (tagFile.Tag.Disc > 0) ? tagFile.Tag.Disc.ToString(GlobalVariables.CultEng) : string.Empty;
						discnumberCell.Items.Add(discnumberCell.Value);

						// Set track count
						DataGridViewComboBoxCell trackcountCell = newRow.Cells[MainForm.Instance.album1.Index] as DataGridViewComboBoxCell;
						trackcountCell.Value = (tagFile.Tag.TrackCount > 0) ? tagFile.Tag.TrackCount.ToString(GlobalVariables.CultEng) : string.Empty;
						trackcountCell.Items.Add(trackcountCell.Value);

						// Set track number
						DataGridViewComboBoxCell tracknumberCell = newRow.Cells[MainForm.Instance.album1.Index] as DataGridViewComboBoxCell;
						tracknumberCell.Value = (tagFile.Tag.Track > 0) ? tagFile.Tag.Track.ToString(GlobalVariables.CultEng) : string.Empty;
						tracknumberCell.Items.Add(tracknumberCell.Value);

						// Set lyrics
						newRow.Cells[MainForm.Instance.lyrics1.Index].Value = tagFile.Tag.Lyrics ?? string.Empty;

						// Set cover URL
						newRow.Cells[MainForm.Instance.cover1.Index].Value = tagFile.Tag.Pictures.Any() ? tagFile.Tag.Pictures[0].Description : string.Empty;

						// Set isVirtualFile
						newRow.Cells[MainForm.Instance.isVirtualFile.Index].Value = false;

						// Set hasNewValues ("Save" in column header)
						newRow.Cells[MainForm.Instance.hasNewValues.Index].Value = false;

						// Add new DataGridViewRow to a fileList which is returned later
						rowList.Add(newRow);
					}
				}
			}

			return rowList;
		}
	}
}
