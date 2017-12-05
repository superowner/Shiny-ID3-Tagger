//-----------------------------------------------------------------------
// <copyright file="AddFiles.cs" company="Shiny Id3 Tagger">
//	 Copyright (c) Shiny Id3 Tagger. All rights reserved.
// </copyright>
// <author>ShinyId3Tagger Team</author>
// <summary>Code fired when "Add files" button is clicked</summary>
//-----------------------------------------------------------------------

namespace GlobalNamespace
{
	using System;
	using System.IO;
	using System.Linq;
	using System.Text.RegularExpressions;
	using System.Threading;
	using System.Threading.Tasks;
	using System.Windows.Forms;

	public partial class Form1
	{
		// ###########################################################################
		private async Task<bool> AddFiles(string[] files, CancellationToken cancelToken)
		{
			// Work starts, disable all buttons to prevent side effects when user clicks them despite an already running task
			this.btnSearch.Enabled = false;
			this.btnWrite.Enabled = false;
			this.btnAddFiles.Enabled = false;

			// Return value which indicates if any new files were successfully added to datagridview1
			bool newFiles = false;

			// If no files were passed through command line, open a new file dialog. files can be an empty array, so use any() to check this
			if (files == null || !files.Any())
			{
				OpenFileDialog dialog = new OpenFileDialog();
				dialog.Filter = "MP3 Files|*.mp3";
				dialog.Multiselect = true;

				if (LastUsedFolder == null)
				{
					LastUsedFolder = Environment.GetFolderPath(Environment.SpecialFolder.MyMusic) + @"\";
				}

				dialog.InitialDirectory = LastUsedFolder;

				if (dialog.ShowDialog() == DialogResult.OK)
				{
					LastUsedFolder = Path.GetDirectoryName(dialog.FileNames[0]);
					files = dialog.FileNames;
				}
			}

			// If anything was selected in the previous dialog, check if it was a folder and get it's mp3 files
			if (files != null && files.Any())
			{
				if (Directory.Exists(files[0]))
				{
					files = Directory.GetFiles(files[0], "*.mp3", SearchOption.AllDirectories);
				}
			}

			// If any files were retrieved previously (either directly selected, via command line or from a folder)
			if (files != null)
			{
				this.slowProgressBar.Maximum = files.Length;
				this.slowProgressBar.Value = 0;
				this.slowProgressBar.Visible = true;

				// Loop through each file and add it to the first datagridview
				foreach (string filepath in files)
				{
					if (cancelToken.IsCancellationRequested)
					{
						break;
					}

					// Start this in separate thread to decrease UI sluggishness
					await Task.Run(() =>
					{
						// Check if the file was already added
						bool rowAlreadyExists = (from row in this.dataGridView1.Rows.Cast<DataGridViewRow>()
							where row.Cells[this.filepath1.Index].Value.ToString() == filepath
							select row).Any();

						// Check if the file is a valid mp3 file
						if (this.IsValidMp3(filepath) && !rowAlreadyExists)
						{
							AddFileToTable(filepath);
							newFiles = true;
						}
					});

					this.slowProgressBar.PerformStep();
				}
			}

			// Work finished, re-enable all buttons and hide progress bar
			this.slowProgressBar.Visible = false;
			this.btnSearch.Enabled = true;
			this.btnWrite.Enabled = true;
			this.btnAddFiles.Enabled = true;

			return newFiles;
		}

		// ###########################################################################
		// Adds a single file to first datagridview with all its present ID3 tags
		private void AddFileToTable(string filepath)
		{
			Id3 tagOld = new Id3();
			tagOld.Filepath = filepath;

			TagLib.File tagFile = TagLib.File.Create(filepath, "audio/mpeg", TagLib.ReadStyle.Average);
			string filename = Path.GetFileNameWithoutExtension(filepath);

			// Extract artist and title from filename
			string fileArtist = null;
			string fileTitle = null;
			Match match = Regex.Match(filename, @"^(?<artist>.*\w+) - (?<title>\w+.*)$");
			if (match.Success)
			{
				fileArtist = match.Groups["artist"].Value;
				fileTitle = match.Groups["title"].Value;
			}

			// Extract and set artist from ID3 tags
			string tagArtist = null;
			if (!string.IsNullOrWhiteSpace(tagFile.Tag.FirstPerformer))
			{
				tagArtist = tagFile.Tag.FirstPerformer;
			}

			// Extract and set title from ID3 tags
			string tagTitle = null;
			if (!string.IsNullOrWhiteSpace(tagFile.Tag.Title))
			{
				tagTitle = tagFile.Tag.Title;
			}

			// Case: artist and title from filename found, but no ID3 tags found
			// Set artist from filename
			if (fileArtist != null && tagArtist == null)
			{
				tagOld.Artist = fileArtist;
			}

			// Set title from filename
			if (fileTitle != null && tagTitle == null)
			{
				tagOld.Title = fileTitle;
			}

			// Case: ID3 tags found, but no artist and title from filename (pattern didn't match)
			if (fileArtist == null && tagArtist != null)
			{
				tagOld.Artist = tagArtist;
			}

			if (fileTitle == null && tagTitle != null)
			{
				tagOld.Title = tagTitle;
			}

			// Case: Both sources (filename and ID3 tags) have a value
			// Select artist according to user setting "PreferTags"
			if (fileArtist != null && tagArtist != null)
			{
				if (User.Settings["PreferTags"])
				{
					tagOld.Artist = tagArtist;
				}
				else
				{
					tagOld.Artist = fileArtist;
				}
			}

			// Select title according to user setting "PreferTags"
			if (fileTitle != null && tagTitle != null)
			{
				if (User.Settings["PreferTags"])
				{
					tagOld.Title = tagTitle;
				}
				else
				{
					tagOld.Title = fileTitle;
				}
			}

			// Case: When both sources don't have a valid value, use the whole filename as fallback
			tagOld.Artist = tagOld.Artist ?? filename;
			tagOld.Title = tagOld.Title ?? filename;

			// Read in existing ID3 tags from file
			tagOld.Album = tagFile.Tag.Album;
			tagOld.Date = (tagFile.Tag.Year > 0) ? tagFile.Tag.Year.ToString(cultEng) : null;
			tagOld.Genre = tagFile.Tag.FirstGenre;
			tagOld.DiscCount = (tagFile.Tag.DiscCount > 0) ? tagFile.Tag.DiscCount.ToString(cultEng) : null;
			tagOld.DiscNumber = (tagFile.Tag.Disc > 0) ? tagFile.Tag.Disc.ToString(cultEng) : null;
			tagOld.TrackCount = (tagFile.Tag.TrackCount > 0) ? tagFile.Tag.TrackCount.ToString(cultEng) : null;
			tagOld.TrackNumber = (tagFile.Tag.Track > 0) ? tagFile.Tag.Track.ToString(cultEng) : null;
			tagOld.Lyrics = tagFile.Tag.Lyrics;
			tagOld.Cover = tagFile.Tag.Pictures.Any() ? tagFile.Tag.Pictures[0].Description : null;

			// Show old tags in gridview panel
			this.Invoke((MethodInvoker)delegate
			{
				this.dataGridView1.Rows.Add(
					(this.dataGridView1.Rows.Count + 1).ToString(cultEng),
					tagOld.Filepath ?? string.Empty,
					tagOld.Artist ?? string.Empty,
					tagOld.Title ?? string.Empty,
					tagOld.Album ?? string.Empty,
					tagOld.Date ?? string.Empty,
					tagOld.Genre ?? string.Empty,
					tagOld.DiscCount ?? string.Empty,
					tagOld.DiscNumber ?? string.Empty,
					tagOld.TrackCount ?? string.Empty,
					tagOld.TrackNumber ?? string.Empty,
					tagOld.Lyrics ?? string.Empty,
					tagOld.Cover ?? string.Empty,
					false);

				int lastRow = this.dataGridView1.RowCount - 1;
				this.MarkChange(lastRow, this.artist1.Index, tagArtist, tagOld.Artist, true);
				this.MarkChange(lastRow, this.title1.Index, tagTitle, tagOld.Title, true);
			});

			// Remove any locks on files
			tagFile.Dispose();
		}
	}
}
