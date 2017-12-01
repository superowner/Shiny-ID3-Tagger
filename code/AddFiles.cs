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
	using System.Drawing;
	using System.IO;
	using System.Linq;
	using System.Text.RegularExpressions;
	using System.Threading;
	using System.Threading.Tasks;
	using System.Windows.Forms;
	
	public partial class Form1
	{
		// ###########################################################################
		private async void AddFiles(string[] files)
		{
			this.btnSearch.Enabled = false;
			this.btnWrite.Enabled = false;
			this.btnAddFiles.Enabled = false;			

			// Issue a new token each time we add files so we can cancel if when we ie read in hundreds of files and ID3 tags
			TokenSource = new CancellationTokenSource();
			CancellationToken cancelToken = TokenSource.Token;
			
			// If no files were passed through command line, open a new file dialog. files can be an empty array, so we use any() to check this
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
				this.progressBar1.Maximum = files.Length;
				this.progressBar1.Value = 0;
				this.progressBar1.Visible = true;
			
				// Loop through each file and add it to the first datagridview
				foreach (string filepath in files)
				{
					if (cancelToken.IsCancellationRequested)
					{
						break;
					}
					
					// TODO Remember the reason why I used Task.Run()
					await Task.Run(() =>
					{
						AddFileToTable(filepath);
					});					
				
					this.progressBar1.PerformStep();
				}
			}
			
			// Work finished, reenable all buttons and hide progress bar
			this.dataGridView1.ClearSelection();						
						
			this.progressBar1.Visible = false;				
			this.btnSearch.Enabled = true;
			this.btnWrite.Enabled = true;
			this.btnAddFiles.Enabled = true;
			
			// If the setting allows it, continue straight with searching for tags without the need to press the search button
			if (User.Settings["AutoSearch"])
			{
				this.StartSearching();
			}
		}

		// ###########################################################################
		// Adds a single file to first datagridview with all its present ID3 tags
		private void AddFileToTable(string filepath)
		{
			bool rowAlreadyExists = (from row in this.dataGridView1.Rows.Cast<DataGridViewRow>()
									where row.Cells[this.filepath1.Index].Value.ToString() == filepath
									select row).Any();
			
			if (this.IsValidMp3(filepath) && rowAlreadyExists == false)
			{
				Id3 tagOld = new Id3();
				tagOld.Filepath = filepath;
				
				Bitmap mp3Icon = new Bitmap(Icon.ExtractAssociatedIcon(filepath).ToBitmap(), 16, 16);
				
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
				
				// We replace possible null values with empty strings. This avoids many additional null checks later in the code. Hacky, I know
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
						mp3Icon,
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
						tagOld.Cover ?? string.Empty);

					int lastRow = this.dataGridView1.RowCount - 1;
					this.MarkChange(lastRow, this.artist1.Index, tagArtist, tagOld.Artist, true);
					this.MarkChange(lastRow, this.title1.Index, tagTitle, tagOld.Title, true);
				});
				
				// Remove any locks on files
				tagFile.Dispose();					
			}				
		}
	}
}
