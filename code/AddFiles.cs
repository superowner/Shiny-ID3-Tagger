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

			Runtime.TokenSource = new CancellationTokenSource();
			CancellationToken cancelToken = Runtime.TokenSource.Token;
			
			if (files == null || !files.Any())
			{
				OpenFileDialog dialog = new OpenFileDialog();
				dialog.Filter = "MP3 Files|*.mp3";
				dialog.Multiselect = true;

				if (Runtime.LastUsedFolder == null)
				{
					Runtime.LastUsedFolder = Environment.GetFolderPath(Environment.SpecialFolder.MyMusic) + @"\";
				}
			
				dialog.InitialDirectory = Runtime.LastUsedFolder;
			
				if (dialog.ShowDialog() == DialogResult.OK)
				{
					Runtime.LastUsedFolder = Path.GetDirectoryName(dialog.FileNames[0]);
					files = dialog.FileNames;
				}
			}
			
			if (files != null && files.Any())
			{				
				if (Directory.Exists(files[0]))
				{
					files = Directory.GetFiles(files[0], "*.mp3", SearchOption.AllDirectories);	
				}
			}
	
			if (files != null)
			{
				this.progressBar1.Maximum = files.Length;
				this.progressBar1.Value = 0;
				this.progressBar1.Visible = true;
			
				foreach (string filepath in files)
				{
					if (cancelToken.IsCancellationRequested)
					{
						break;
					}
					
					await Task.Run(() =>
					{
						AddFileToTable(filepath);
					});					
				
					this.progressBar1.PerformStep();
				}
			}
			
			this.dataGridView1.ClearSelection();						
						
			this.progressBar1.Visible = false;				
			this.btnSearch.Enabled = true;
			this.btnWrite.Enabled = true;
			this.btnAddFiles.Enabled = true;
			
			if (User.Settings["AutoSearch"])
			{
				this.StartSearching();
			}
		}

		// ###########################################################################
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
				
				// Extract artist and title from ID3 tags
				string tagArtist = null;
				if (!string.IsNullOrWhiteSpace(tagFile.Tag.FirstPerformer))
				{
					tagArtist = tagFile.Tag.FirstPerformer;
				}
				
				string tagTitle = null;
				if (!string.IsNullOrWhiteSpace(tagFile.Tag.Title))
				{
					tagTitle = tagFile.Tag.Title;
				}
				
				// Case: artist and title from filename found, but no ID3 tags found
				if (fileArtist != null && tagArtist == null)
				{
					tagOld.Artist = fileArtist;
				}
				
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

				// Case: Both sources (filename and ID3 tags) have a value. Select source according to user setting "PreferTags"
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
				
				// Case: When both sources don't give a valid value, use the whole filename as fallback
				tagOld.Artist = tagOld.Artist ?? filename;
				tagOld.Title = tagOld.Title ?? filename;				
				
				// ReaderWriterLock in AllowDrop remaining Id3 GetTags_7digital from file
				// Replace possible null values with empty strings. This avoids many additional null checks later
				tagOld.Album = tagFile.Tag.Album ?? string.Empty;
				tagOld.Date = (tagFile.Tag.Year > 0) ? tagFile.Tag.Year.ToString(Runtime.CultEng) : string.Empty;
				tagOld.Genre = tagFile.Tag.FirstGenre ?? string.Empty;
				tagOld.DiscCount = (tagFile.Tag.DiscCount > 0) ? tagFile.Tag.DiscCount.ToString(Runtime.CultEng) : string.Empty;
				tagOld.DiscNumber = (tagFile.Tag.Disc > 0) ? tagFile.Tag.Disc.ToString(Runtime.CultEng) : string.Empty;
				tagOld.TrackCount = (tagFile.Tag.TrackCount > 0) ? tagFile.Tag.TrackCount.ToString(Runtime.CultEng) : string.Empty;
				tagOld.TrackNumber = (tagFile.Tag.Track > 0) ? tagFile.Tag.Track.ToString(Runtime.CultEng) : string.Empty;
				tagOld.Lyrics = tagFile.Tag.Lyrics ?? string.Empty;
				tagOld.Cover = tagFile.Tag.Pictures.Any() ? tagFile.Tag.Pictures[0].Description : string.Empty;								
				
				this.Invoke((MethodInvoker)delegate
				{
					this.dataGridView1.Rows.Add(
						mp3Icon,
						tagOld.Filepath,
						tagOld.Artist,
						tagOld.Title,
						tagOld.Album,
						tagOld.Date,
						tagOld.Genre,
						tagOld.DiscCount,
						tagOld.DiscNumber,
						tagOld.TrackCount,
						tagOld.TrackNumber,
						tagOld.Lyrics,
						tagOld.Cover);

					int row = this.dataGridView1.RowCount - 1;
					this.MarkChange(row, this.artist1.Index, tagArtist, tagOld.Artist, true);
					this.MarkChange(row, this.title1.Index, tagTitle, tagOld.Title, true);
				});
				
				tagFile.Dispose();					
			}				
		}
	}
}
