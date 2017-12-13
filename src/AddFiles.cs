//-----------------------------------------------------------------------
// <copyright file="AddFiles.cs" company="Shiny ID3 Tagger">
// Copyright (c) Shiny ID3 Tagger. All rights reserved.
// </copyright>
// <author>ShinyId3Tagger Team</author>
// <summary>Method to run when "Add files" button is clicked</summary>
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

	/// <summary>
	/// Method selects and reads in existing tags and shows them in a dataGridView
	/// </summary>
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
						files = dialog.FileNames;
					}
				}
			}

			// If anything was selected in previous dialog, check if it was a folder and get it's mp3 files
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

				// Loop through each file and add it to first dataGridView
				foreach (string filepath in files)
				{
					if (cancelToken.IsCancellationRequested)
					{
						break;
					}

					// Start this in separate thread to decrease UI sluggishness
					await Task.Run(() =>
					{
						// Check if file was already added
						bool rowAlreadyExists = (from row in this.dataGridView1.Rows.Cast<DataGridViewRow>()
							where row.Cells[this.filepath1.Index].Value.ToString().ToLowerInvariant() == filepath.ToLowerInvariant()
							select row).Any();

						// Check if file is a valid mp3 file
						if (this.IsValidMp3(filepath) && !rowAlreadyExists)
						{
							this.AddFileToTable(filepath);
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
		// Adds a single file to datagridview1 with all its present ID3 tags
		private void AddFileToTable(string filepath)
		{
			using (TagLib.File tagFile = TagLib.File.Create(filepath, "audio/mpeg", TagLib.ReadStyle.Average))
			{
				string filename = Path.GetFileNameWithoutExtension(filepath);
				string[] artistChoices = new string[] { null, null, filename };
				string[] titleChoices = new string[] { null, null, filename };

				// Extract artist and title from filename and store them in array at index 0 or 1 according to setting "PreferTags"
				Match match = Regex.Match(filename, @"^(\d+\s)?(-\s+)?(?<artist>.*\w+)\s+-\s+(?<title>\w+.*)$");
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

				// Create ID3 instance and fill values with present tags
				Id3 tagOld = new Id3
				{
					Filepath = filepath,
					Artist = artistChoices.FirstOrDefault(s => !string.IsNullOrEmpty(s)),       // Select the first non-null artist choice, value order in array is important therefore
					Title = titleChoices.FirstOrDefault(s => !string.IsNullOrEmpty(s)),         // Select the first non-null title choice, value order in array is important therefore
					Album = tagFile.Tag.Album,
					Date = (tagFile.Tag.Year > 0) ? tagFile.Tag.Year.ToString(cultEng) : null,
					Genre = tagFile.Tag.FirstGenre,
					DiscCount = (tagFile.Tag.DiscCount > 0) ? tagFile.Tag.DiscCount.ToString(cultEng) : null,
					DiscNumber = (tagFile.Tag.Disc > 0) ? tagFile.Tag.Disc.ToString(cultEng) : null,
					TrackCount = (tagFile.Tag.TrackCount > 0) ? tagFile.Tag.TrackCount.ToString(cultEng) : null,
					TrackNumber = (tagFile.Tag.Track > 0) ? tagFile.Tag.Track.ToString(cultEng) : null,
					Lyrics = tagFile.Tag.Lyrics,
					Cover = tagFile.Tag.Pictures.Any() ? tagFile.Tag.Pictures[0].Description : null
				};

				// Show present tags in dataGridView1
				this.Invoke(new Action(
					() =>
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
					}));
			}
		}
	}
}
