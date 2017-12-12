//-----------------------------------------------------------------------
// <copyright file="WriteTags.cs" company="Shiny ID3 Tagger">
// Copyright (c) Shiny ID3 Tagger. All rights reserved.
// </copyright>
// <author>ShinyId3Tagger Team</author>
// <summary>Main method for ID3 tag saving. Loops through all result rows and writes their values as tags it's corresponding file</summary>
//-----------------------------------------------------------------------

namespace GlobalNamespace
{
	using System;
	using System.Collections.Generic;
	using System.Diagnostics;
	using System.Drawing;
	using System.Drawing.Drawing2D;
	using System.Drawing.Imaging;
	using System.IO;
	using System.Linq;
	using System.Net.Http;
	using System.Net.Mime;
	using System.Text.RegularExpressions;
	using System.Threading.Tasks;
	using System.Windows.Forms;
	using TagLib;
	using TagLib.Id3v2;

	public partial class Form1
	{
		// ###########################################################################
		private async void StartWriting()
		{
			this.btnAddFiles.Enabled = false;
			this.btnSearch.Enabled = false;
			this.btnWrite.Enabled = false;

			this.slowProgressBar.Maximum = this.dataGridView1.Rows.Count;
			this.slowProgressBar.Value = 0;
			this.slowProgressBar.Visible = true;

			// Begin looping through all files in first dataGridView
			foreach (DataGridViewRow row in this.dataGridView1.Rows)
			{
				// If file is a virtual file (CSV Import), cancel remaining work for this file and continue with next file
				if ((bool)row.Cells[this.isVirtualFile.Index].Value)
				{
					this.slowProgressBar.PerformStep();
					continue;
				}

				bool successWrite;
				string filepath = (string)row.Cells[this.filepath1.Index].Value;

				// ###########################################################################
				// Taglib does not convert ID3v2.4 (UTF-8, iTunes compatible) encoded frames to ID3v2.3 encoded frames (UTF-16, Windows Explorer compatible)
				// Even with "ForceDefaultEncoding = true". The workaround is to clear and rebuild all tags from scratch
				TagLib.Id3v2.Tag id3v2backup = null;
				using (TagLib.File tagFile = TagLib.File.Create(filepath, "audio/mpeg", ReadStyle.Average))
				{
					id3v2backup = (TagLib.Id3v2.Tag)tagFile.GetTag(TagTypes.Id3v2, true);

					// Clear all tags and save mp3 file without any tags. Save is necessary to wipe all tags
					tagFile.RemoveTags(TagTypes.AllTags);

					successWrite = await this.SaveFile(tagFile);
				}

				// If save failed, cancel remaining work for this file and continue with next file
				if (!successWrite)
				{
					this.slowProgressBar.PerformStep();
					continue;
				}

				// Write all old frames from backup variable back as ID3v2.3 frames
				using (TagLib.File tagFile = TagLib.File.Create(filepath, "audio/mpeg", ReadStyle.Average))
				{
					TagLib.Id3v2.Tag id3v2 = (TagLib.Id3v2.Tag)tagFile.GetTag(TagTypes.Id3v2, true);

					// Remove ID3v1 tags, use ID3v2.3 for new tags (not ID3v2.4)
					tagFile.RemoveTags(TagTypes.Id3v1);
					id3v2.Version = 3;

					// Read in user settings which tags should be preserved (all other unknown tags will be removed)
					string[] defaultFrames = User.Settings["DefaultFrames"].ToObject<string[]>();
					string[] userFrames = User.Settings["UserFrames"].ToObject<string[]>();

					// Loop through backup tags, add only frames which are in defaultFrames or userFrames array as new tag
					IEnumerable<Frame> backupFrameList = id3v2backup.GetFrames<Frame>();
					foreach (Frame frm in backupFrameList)
					{
						string frameId = frm.FrameId.ToString();
						if (defaultFrames.Contains(frameId, StringComparer.OrdinalIgnoreCase))
						{
							if (frameId.ToUpperInvariant() == "TXXX")
							{
								string frameDesc = ((UserTextInformationFrame)frm).Description;
								if (userFrames.Contains(frameDesc, StringComparer.OrdinalIgnoreCase))
								{
									id3v2.AddFrame(frm);
								}
								else
								{
									string message = string.Format("{0,-100}{1}", "Tag removed: " + frameDesc, "file: \"" + filepath + "\"");
									this.PrintLogMessage("write", new[] { message });
								}
							}
							else
							{
								id3v2.AddFrame(frm);
							}
						}
						else
						{
							string message = string.Format("{0,-100}{1}", "Tag removed: " + frameId, "file: \"" + filepath + "\"");
							this.PrintLogMessage("write", new[] { message });
						}
					}

					// ###########################################################################
					// Add search result tags (artist, title, album, genre, date, disc, track, lyrics) to new ID3 tag object
					id3v2 = this.WriteTags(tagFile, row, id3v2);

					// ###########################################################################
					// Download and add cover image
					if (User.Settings["OverwriteImage"] || !tagFile.Tag.Pictures.Any() || tagFile.Tag.Pictures[0].Data.Count == 0)
					{
						using (HttpClient client = InitiateHttpClient())
						using (HttpRequestMessage request = new HttpRequestMessage())
						{
							HttpResponseMessage response = new HttpResponseMessage();

							try
							{
								string url = (string)row.Cells[this.cover1.Index].Value;
								if (IsValidUrl(url))
								{
									request.RequestUri = new Uri(url);
									response = await client.SendAsync(request);
								}

								// Check if response is not null
								if (response != null && response.Content != null)
								{
									string message = string.Format("{0,-100}{1}", "Picture source: " + request.RequestUri.Authority, "file: \"" + filepath + "\"");
									this.PrintLogMessage("write", new[] { message });

									// Download cover as stream to avoid saving to disk
									using (MemoryStream streamOrg = (MemoryStream)await response.Content.ReadAsStreamAsync())
									using (MemoryStream streamResized = new MemoryStream())
									{
										// Check if downloaded stream is an image. Some servers use a wrong content type for HTTP response. Therefore you need to check stream bytes for defined byte markers
										if (this.GetImageType(streamOrg.ToArray()))
										{
											using (Image image = Image.FromStream(streamOrg))
											{
												// Resize image according to user setting "MaxImageSize". Always resize and re-encode
												int longSide = new List<int> { image.Width, image.Height }.Max();
												float resizeFactor = new List<float>
												{
													User.Settings["MaxImageSize"] / (float)image.Width,
													User.Settings["MaxImageSize"] / (float)image.Height
												}.Min();

												Size newSize = new Size((int)Math.Round(image.Width * resizeFactor), (int)Math.Round(image.Height * resizeFactor));

												using (Bitmap bitmap = new Bitmap(newSize.Width, newSize.Height))
												using (Graphics graph = Graphics.FromImage(bitmap))
												using (EncoderParameters encoderParams = new EncoderParameters(1))
												{
													// Resize bitmap with best quality
													graph.InterpolationMode = InterpolationMode.HighQualityBicubic;
													graph.DrawImage(image, 0, 0, newSize.Width, newSize.Height);

													// Prepare encoder object for JPEG format
													ImageCodecInfo imageEncoder = ImageCodecInfo.GetImageEncoders().First(codec => codec.FormatID == ImageFormat.Jpeg.Guid);
													encoderParams.Param[0] = new EncoderParameter(Encoder.Quality, 100L);

													// Save and encode bitmap as new stream which now holds a resized JPEG
													bitmap.Save(streamResized, imageEncoder, encoderParams);
													streamResized.Position = 0;
												}

												// Create ID3 cover tag, use "front cover" as type
												AttachedPictureFrame taglibpicture = new AttachedPictureFrame()
												{
													Data = ByteVector.FromStream(streamResized),
													MimeType = MediaTypeNames.Image.Jpeg,
													Type = PictureType.FrontCover,
													Description = url,
													TextEncoding = StringType.Latin1
												};

												// Add cover tag to ID3 tag object, delete all other image types like back cover
												tagFile.Tag.Pictures = new IPicture[] { taglibpicture };
											}
										}
										else
										{
											if (User.Settings["DebugLevel"] >= 1)
											{
												string[] errorMsg =
												{
													"ERROR:    Cover URL does not contain a valid image. File signature did not match any image format!",
													"URL:      " + url,
													"type:     " + response.Content.Headers.ContentType.ToString()
												};
												this.PrintLogMessage("error", errorMsg);
											}
										}
									}
								}
								else
								{
									if (User.Settings["DebugLevel"] >= 1)
									{
										string[] errorMsg =
										{
											"ERROR:    Cover URL is not reachable!",
											"URL:      " + url
										};
										this.PrintLogMessage("error", errorMsg);
									}
								}
							}
							finally
							{
								// Can't figure out how to use a using statement since a new value gets assigned inside the using block
								if (response != null)
								{
									response.Dispose();
								}
							}
						}
					}

					// ###########################################################################
					// Finally save all new tags to file
					successWrite = await this.SaveFile(tagFile);

					// Clear background color if file was processed successfully
					if (successWrite)
					{
						foreach (DataGridViewCell cell in row.Cells)
						{
							cell.Style.BackColor = Color.Empty;
						}
					}

					this.slowProgressBar.PerformStep();
				}
			}

			this.slowProgressBar.Visible = false;

			this.btnAddFiles.Enabled = true;
			this.btnSearch.Enabled = true;
			this.btnWrite.Enabled = true;
		}

		// ###########################################################################
		// Overwrite tag values with results from APIs
		private TagLib.Id3v2.Tag WriteTags(TagLib.File tagFile, DataGridViewRow row, TagLib.Id3v2.Tag id3v2)
		{
			// Artist
			string oldArtist = tagFile.Tag.FirstPerformer;
			string newArtist = (string)row.Cells[this.artist1.Index].Value;
			if (oldArtist != newArtist && !string.IsNullOrWhiteSpace(newArtist))
			{
				id3v2.RemoveFrames("TPE1");
				id3v2.SetTextFrame("TPE1", newArtist);

				string message = string.Format(cultEng, "{0,-100}{1}", "Artist: " + newArtist, "file: \"" + tagFile.Name + "\"");
				this.PrintLogMessage("write", new[] { message });
			}

			// Title
			string oldTitle = tagFile.Tag.Title;
			string newTitle = (string)row.Cells[this.title1.Index].Value;
			if (oldTitle != newTitle && !string.IsNullOrWhiteSpace(newTitle))
			{
				id3v2.RemoveFrames("TIT2");
				id3v2.SetTextFrame("TIT2", newTitle);

				string message = string.Format(cultEng, "{0,-100}{1}", "Title: " + newTitle, "file: \"" + tagFile.Name + "\"");
				this.PrintLogMessage("write", new[] { message });
			}

			// Album
			string oldAlbum = tagFile.Tag.Album;
			string newAlbum = (string)row.Cells[this.album1.Index].Value;
			if (oldAlbum != newAlbum && !string.IsNullOrWhiteSpace(newAlbum))
			{
				id3v2.RemoveFrames("TALB");
				id3v2.SetTextFrame("TALB", newAlbum);

				string message = string.Format(cultEng, "{0,-100}{1}", "Album: " + newAlbum, "file: \"" + tagFile.Name + "\"");
				this.PrintLogMessage("write", new[] { message });
			}

			// Genre
			string oldGenre = tagFile.Tag.FirstGenre;
			string newGenre = (string)row.Cells[this.genre1.Index].Value;
			if (oldGenre != newGenre && !string.IsNullOrWhiteSpace(newGenre))
			{
				id3v2.RemoveFrames("TCON");
				id3v2.SetTextFrame("TCON", newGenre);

				string message = string.Format(cultEng, "{0,-100}{1}", "Genre: " + newGenre, "file: \"" + tagFile.Name + "\"");
				this.PrintLogMessage("write", new[] { message });
			}

			// Disc number + disc count
			string oldDiscnumber = tagFile.Tag.Disc.ToString(cultEng);
			string oldDisccount = tagFile.Tag.DiscCount.ToString(cultEng);
			string newDiscnumber = (string)row.Cells[this.discnumber1.Index].Value;
			string newDisccount = (string)row.Cells[this.disccount1.Index].Value;
			if ((oldDiscnumber != newDiscnumber && !string.IsNullOrWhiteSpace(newDiscnumber)) ||
				(oldDisccount != newDisccount && !string.IsNullOrWhiteSpace(newDisccount)))
			{
				newDiscnumber = string.IsNullOrWhiteSpace(newDiscnumber) ? oldDiscnumber : newDiscnumber;
				newDisccount = string.IsNullOrWhiteSpace(newDisccount) ? oldDisccount : newDisccount;
				id3v2.RemoveFrames("TPOS");
				id3v2.SetTextFrame("TPOS", newDiscnumber + "/" + newDisccount);

				string message = string.Format(cultEng, "{0,-100}{1}", "Disc: " + newDiscnumber + "/" + newDisccount, "file: \"" + tagFile.Name + "\"");
				this.PrintLogMessage("write", new[] { message });
			}

			// Track number + track count
			string oldTracknumber = tagFile.Tag.Track.ToString(cultEng);
			string oldTrackcount = tagFile.Tag.TrackCount.ToString(cultEng);
			string newTracknumber = (string)row.Cells[this.tracknumber1.Index].Value;
			string newTrackcount = (string)row.Cells[this.trackcount1.Index].Value;
			if ((oldTracknumber != newTracknumber && !string.IsNullOrWhiteSpace(newTracknumber)) ||
				(oldTrackcount != newTrackcount && !string.IsNullOrWhiteSpace(newTrackcount)))
			{
				newTracknumber = string.IsNullOrWhiteSpace(newTracknumber) ? oldTracknumber : newTracknumber;
				newTrackcount = string.IsNullOrWhiteSpace(newTrackcount) ? oldTrackcount : newTrackcount;
				id3v2.RemoveFrames("TRCK");
				id3v2.SetTextFrame("TRCK", newTracknumber + "/" + newTrackcount);

				string message = string.Format(cultEng, "{0,-100}{1}", "Track: " + newTracknumber + "/" + newTrackcount, "file: \"" + tagFile.Name + "\"");
				this.PrintLogMessage("write", new[] { message });
			}

			// Date
			// Value is stored in TYER frame which represents only the year
			// TDRC (date of recording) stores full date+time info and consolidates TYER (YYYY), TDAT (DDMM) and TIME (HHMM)
			// But TDRC is only available in ID3v2.4 - and this program uses ID3v2.3 for Windows Explorer compatibility
			// Surprisingly you have to remove TDRC to also remove TYER frames. Probably because taglib operates with 2.4 frame names internally
			string oldDate = tagFile.Tag.Year.ToString(cultEng);
			string newDate = (string)row.Cells[this.date1.Index].Value;
			if (oldDate != newDate && !string.IsNullOrWhiteSpace(newDate))
			{
				id3v2.RemoveFrames("TDRC");
				id3v2.RemoveFrames("TYER");
				id3v2.RemoveFrames("TDAT");
				id3v2.RemoveFrames("TIME");
				id3v2.SetNumberFrame("TYER", (uint)this.ConvertStringToDate(newDate).Year, 0);

				string message = string.Format(cultEng, "{0,-100}{1}", "Date: " + newDate, "file: \"" + tagFile.Name + "\"");
				this.PrintLogMessage("write", new[] { message });
			}

			// Lyrics
			// To set "eng" as language you have to remove and add whole frame back again
			// Otherwise taglib sets system default language e.g "deu" or "esp" as lyrics language if USLT tag already existed
			string oldLyrics = tagFile.Tag.Lyrics;
			string newLyrics = (string)row.Cells[this.lyrics1.Index].Value;
			if (oldLyrics != newLyrics && !string.IsNullOrWhiteSpace(newLyrics))
			{
				string lyricPreview = string.Join(string.Empty, newLyrics.Take(50));
				string cleanPreview = Regex.Replace(lyricPreview, @"\r\n?|\n", string.Empty);
				UnsynchronisedLyricsFrame frmUSLT = new UnsynchronisedLyricsFrame(string.Empty, "eng", StringType.UTF16);
				frmUSLT.Text = newLyrics;
				id3v2.RemoveFrames("USLT");
				id3v2.AddFrame(frmUSLT);

				string message = string.Format(cultEng, "{0,-100}{1}", "Lyrics: " + cleanPreview, "file: \"" + tagFile.Name + "\"");
				this.PrintLogMessage("write", new[] { message });
			}

			return id3v2;
		}

		// ###########################################################################
		private async Task<bool> SaveFile(TagLib.File tagFile)
		{
			const int WriteDelay = 50;
			const int MaxRetries = 3;
			DateTime lastWriteTime = default(DateTime);

			// Read and backup LastWriteTime
			bool successWrite = false;
			for (int retry = 0; retry < MaxRetries; retry++)
			{
				try
				{
					lastWriteTime = System.IO.File.GetLastWriteTime(tagFile.Name);
					successWrite = true;
					break;
				}
				catch (IOException)
				{
					// Do nothing. Error is handled at end of SaveFile method
					successWrite = false;
				}

				await Task.Delay(WriteDelay);
			}

			// Save all mp3 tags (LastWriteTime will be modified here)
			if (successWrite)
			{
				successWrite = false;
				for (int retry = 0; retry < MaxRetries; retry++)
				{
					try
					{
						tagFile.Save();
						successWrite = true;
						break;
					}
					catch (IOException)
					{
						// Do nothing. Error is handled at end of SaveFile method
						successWrite = false;
					}

					await Task.Delay(WriteDelay);
				}
			}

			// Change LastWriteTime back to original
			if (successWrite)
			{
				successWrite = false;
				for (int retry = 0; retry < MaxRetries; retry++)
				{
					try
					{
						System.IO.File.SetLastWriteTime(tagFile.Name, lastWriteTime);
						successWrite = true;
						break;
					}
					catch (IOException)
					{
						// Do nothing. Error is handled at end of SaveFile method
						successWrite = false;
					}

					await Task.Delay(WriteDelay);
				}
			}

			if (!successWrite)
			{
				string[] errorMsg =
					{
						@"ERROR:    Could not read/write file!",
						"File:     " + tagFile.Name
					};
				this.PrintLogMessage("error", errorMsg);
			}

			return successWrite;
		}
	}
}