﻿//-----------------------------------------------------------------------
// <copyright file="WriteTags.cs" company="Shiny Id3 Tagger">
//	 Copyright (c) Shiny Id3 Tagger. All rights reserved.
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

			HttpClient client = InitiateHttpClient();

			foreach (DataGridViewRow row in this.dataGridView1.Rows)
			{
				// Check if file is a real file and no virtual file (imported via CSV file)
				if ((bool)row.Cells[this.isVirtualFile.Index].Value)
				{
					// Cancel remaining work for this file and continue with next file
					this.slowProgressBar.PerformStep();
					continue;
				}

				bool successWrite;
				string filepath = (string)row.Cells[this.filepath1.Index].Value;

				// Taglib does not convert ID3v2.4 (UTF-8, iTunes compatible) encoded frames to ID3v2.3 encodede frames (UTF-16, Windows Explorer compatible)
				// Even with "ForceDefaultEncoding = true". The workaround is to clear and rebuild all tags from scratch
				TagLib.File tagFile = TagLib.File.Create(filepath, "audio/mpeg", ReadStyle.Average);
				TagLib.Id3v2.Tag oldId3v2 = (TagLib.Id3v2.Tag)tagFile.GetTag(TagTypes.Id3v2, true);

				// ###########################################################################
				// Clear all tags and save mp3 file without any tags. Save is neccessary to wipe all tags
				tagFile.RemoveTags(TagTypes.AllTags);

				successWrite = await this.SaveFile(tagFile);
				tagFile.Dispose();

				if (!successWrite)
				{
					// Cancel remaining work for this file and continue with next file if Saving wasn't possible
					this.slowProgressBar.PerformStep();
					continue;
				}

				// Write all old frames from backup variable back as ID3v2.3 frames
				tagFile = TagLib.File.Create(filepath, "audio/mpeg", ReadStyle.Average);
				TagLib.Id3v2.Tag id3v2 = (TagLib.Id3v2.Tag)tagFile.GetTag(TagTypes.Id3v2, true);

				tagFile.RemoveTags(TagTypes.Id3v1);
				id3v2.Version = 3;

				string[] defaultFrames = User.Settings["DefaultFrames"].Split(',');
				string[] userFrames = User.Settings["UserFrames"].Split(',');

				IEnumerable<Frame> oldFrameList = oldId3v2.GetFrames<Frame>();
				foreach (Frame frm in oldFrameList)
				{
					string frameId = frm.FrameId.ToString();
					if (defaultFrames.Contains(frameId))
					{
						if (frameId == "TXXX")
						{
							string frameDesc = ((UserTextInformationFrame)frm).Description;
							if (userFrames.Contains(frameDesc))
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
				// Write tags (artist, title, album, genre, date, disc, track, lyrics) to file
				id3v2 = this.WriteTags(tagFile, row, id3v2);

				// ###########################################################################
				if (User.Settings["OverwriteImage"] || !tagFile.Tag.Pictures.Any() || tagFile.Tag.Pictures[0].Data.Count == 0)
				{
					HttpRequestMessage request = new HttpRequestMessage();
					HttpResponseMessage response = new HttpResponseMessage();

					string url = (string)row.Cells[this.cover1.Index].Value;
					if (IsValidUrl(url))
					{
						request.RequestUri = new Uri(url);
						response = await client.SendAsync(request);
					}

					if (response != null && response.Content != null)
					{
						if (response.Content.Headers.ContentType.ToString().StartsWith("image/", StringComparison.OrdinalIgnoreCase))
						{
							string message = string.Format("{0,-100}{1}", "Picture source: " + request.RequestUri.Authority, "file: \"" + filepath + "\"");
							this.PrintLogMessage("write", new[] { message });

							MemoryStream stream = (MemoryStream)await response.Content.ReadAsStreamAsync();
							Image image = Image.FromStream(stream);
							stream.Position = 0;

							int longSide = (new List<int> { image.Width, image.Height }).Max();
							if (longSide != User.Settings["MaxImageSize"])
							{
								float resizeFactor = (new List<float>
								{
									User.Settings["MaxImageSize"] / (float)image.Width,
									User.Settings["MaxImageSize"] / (float)image.Height
								}).Min();
								Size newSize = new Size((int)Math.Round(image.Width * resizeFactor), (int)Math.Round(image.Height * resizeFactor));

								Bitmap bitmap = new Bitmap(newSize.Width, newSize.Height);
								Graphics graph = Graphics.FromImage(bitmap);
								graph.InterpolationMode = InterpolationMode.HighQualityBicubic;
								graph.DrawImage(image, 0, 0, newSize.Width, newSize.Height);

								ImageCodecInfo imageEncoder = ImageCodecInfo.GetImageEncoders().First(codec => codec.FormatID == ImageFormat.Jpeg.Guid);

								EncoderParameters encoderParams = new EncoderParameters(1);
								encoderParams.Param[0] = new EncoderParameter(Encoder.Quality, 100L);

								stream = new MemoryStream();
								bitmap.Save(stream, imageEncoder, encoderParams);
								stream.Position = 0;

								bitmap.Dispose();
								graph.Dispose();
								encoderParams.Dispose();
							}

							AttachedPictureFrame taglibpicture = new AttachedPictureFrame();
							taglibpicture.Data = ByteVector.FromStream(stream);
							taglibpicture.MimeType = MediaTypeNames.Image.Jpeg;
							taglibpicture.Type = PictureType.FrontCover;
							taglibpicture.Description = url;
							taglibpicture.TextEncoding = StringType.Latin1;

							tagFile.Tag.Pictures = new IPicture[] { taglibpicture };

							image.Dispose();
							stream.Dispose();
						}
						else
						{
							if (User.Settings["DebugLevel"] >= 1)
							{
								string[] errorMsg =
								{
									"ERROR:    Downloaded cover from server is not a valid image format!",
									"Format:   " + response.Content.Headers.ContentType
								};
								this.PrintLogMessage("error", errorMsg);
							}
						}
					}

					request.Dispose();
					response.Dispose();
				}

				// ###########################################################################
				Task.WaitAll();

				successWrite = await this.SaveFile(tagFile);
				tagFile.Dispose();

				if (!successWrite)
				{
					// Cancel remaining work for this file and continue with next file if Saving wasn't possible
					this.slowProgressBar.PerformStep();
					continue;
				}

				foreach (DataGridViewCell cell in row.Cells)
				{
					cell.Style.BackColor = Color.Empty;

				}

				this.slowProgressBar.PerformStep();
			}

			client.Dispose();

			this.slowProgressBar.Visible = false;
			this.btnAddFiles.Enabled = true;
			this.btnSearch.Enabled = true;
			this.btnWrite.Enabled = true;
		}

		// ###########################################################################
		private TagLib.Id3v2.Tag WriteTags(TagLib.File tagFile, DataGridViewRow row, TagLib.Id3v2.Tag id3v2)
		{
			// Overwrite tag values with results from webservice API
			string oldArtist = tagFile.Tag.FirstPerformer;
			string newArtist = (string)row.Cells[this.artist1.Index].Value;
			if (oldArtist != newArtist && !string.IsNullOrWhiteSpace(newArtist))
			{
				string message = string.Format(cultEng, "{0,-100}{1}", "Artist: " + newArtist, "file: \"" + tagFile.Name + "\"");
				this.PrintLogMessage("write", new[] { message });
				id3v2.RemoveFrames("TPE1");
				id3v2.SetTextFrame("TPE1", newArtist);
			}

			string oldTitle = tagFile.Tag.Title;
			string newTitle = (string)row.Cells[this.title1.Index].Value;
			if (oldTitle != newTitle && !string.IsNullOrWhiteSpace(newTitle))
			{
				string message = string.Format(cultEng, "{0,-100}{1}", "Title: " + newTitle, "file: \"" + tagFile.Name + "\"");
				this.PrintLogMessage("write", new[] { message });
				id3v2.RemoveFrames("TIT2");
				id3v2.SetTextFrame("TIT2", newTitle);
			}

			string oldAlbum = tagFile.Tag.Album;
			string newAlbum = (string)row.Cells[this.album1.Index].Value;
			if (oldAlbum != newAlbum && !string.IsNullOrWhiteSpace(newAlbum))
			{
				string message = string.Format(cultEng, "{0,-100}{1}", "Album: " + newAlbum, "file: \"" + tagFile.Name + "\"");
				this.PrintLogMessage("write", new[] { message });
				id3v2.RemoveFrames("TALB");
				id3v2.SetTextFrame("TALB", newAlbum);
			}

			string oldGenre = tagFile.Tag.FirstGenre;
			string newGenre = (string)row.Cells[this.genre1.Index].Value;
			if (oldGenre != newGenre && !string.IsNullOrWhiteSpace(newGenre))
			{
				string message = string.Format(cultEng, "{0,-100}{1}", "Genre: " + newGenre, "file: \"" + tagFile.Name + "\"");
				this.PrintLogMessage("write", new[] { message });
				id3v2.RemoveFrames("TCON");
				id3v2.SetTextFrame("TCON", newGenre);
			}

			string oldDiscnumber = tagFile.Tag.Disc.ToString(cultEng);
			string oldDisccount = tagFile.Tag.DiscCount.ToString(cultEng);
			string newDiscnumber = (string)row.Cells[this.discnumber1.Index].Value;
			string newDisccount = (string)row.Cells[this.disccount1.Index].Value;
			if ((oldDiscnumber != newDiscnumber && !string.IsNullOrWhiteSpace(newDiscnumber)) ||
				(oldDisccount  != newDisccount  && !string.IsNullOrWhiteSpace(newDisccount)))
			{
				newDiscnumber = string.IsNullOrWhiteSpace(newDiscnumber) ? oldDiscnumber : newDiscnumber;
				newDisccount = string.IsNullOrWhiteSpace(newDisccount) ? oldDisccount : newDisccount;
				string message = string.Format(cultEng, "{0,-100}{1}", "Disc: " + newDiscnumber + "/" + newDisccount, "file: \"" + tagFile.Name + "\"");
				this.PrintLogMessage("write", new[] { message });
				id3v2.RemoveFrames("TPOS");
				id3v2.SetTextFrame("TPOS", newDiscnumber + "/" + newDisccount);
			}

			string oldTracknumber = tagFile.Tag.Track.ToString(cultEng);
			string oldTrackcount = tagFile.Tag.TrackCount.ToString(cultEng);
			string newTracknumber = (string)row.Cells[this.tracknumber1.Index].Value;
			string newTrackcount = (string)row.Cells[this.trackcount1.Index].Value;
			if ((oldTracknumber != newTracknumber && !string.IsNullOrWhiteSpace(newTracknumber)) ||
				(oldTrackcount  != newTrackcount  && !string.IsNullOrWhiteSpace(newTrackcount)))
			{
				newTracknumber = string.IsNullOrWhiteSpace(newTracknumber) ? oldTracknumber : newTracknumber;
				newTrackcount = string.IsNullOrWhiteSpace(newTrackcount) ? oldTrackcount : newTrackcount;
				string message = string.Format(cultEng, "{0,-100}{1}", "Track: " + newTracknumber + "/" + newTrackcount, "file: \"" + tagFile.Name + "\"");
				this.PrintLogMessage("write", new[] { message });
				id3v2.RemoveFrames("TRCK");
				id3v2.SetTextFrame("TRCK", newTracknumber + "/" + newTrackcount);
			}

			// TDRC (date of recording) stores full date+time info and consolidates TYER (YYYY), TDAT (DDMM) and TIME (HHMM)
			// But TDRC is only available in ID3 v2.4 - and this program uses ID3 v2.3 for Windows Explorer compatibility
			// Strangely you have to use TDRC to remove TYER frames. Maybe because taglib operates with 2.4 frame names internally
			string oldDate = tagFile.Tag.Year.ToString(cultEng);
			string newDate = (string)row.Cells[this.date1.Index].Value;
			if (oldDate != newDate && !string.IsNullOrWhiteSpace(newDate))
			{
				string message = string.Format(cultEng, "{0,-100}{1}", "Date: " + newDate, "file: \"" + tagFile.Name + "\"");
				this.PrintLogMessage("write", new[] { message });
				id3v2.RemoveFrames("TDRC");
				id3v2.RemoveFrames("TYER");
				id3v2.RemoveFrames("TDAT");
				id3v2.RemoveFrames("TIME");
				id3v2.SetNumberFrame("TYER", (uint)this.ConvertStringToDate(newDate).Year, 0);
			}

			// To set "eng" as language you have to remove and add the whole frame back again
			// Otherwise taglib sets system default language e.g "deu" or "esp" as lyrics language if USLT tag already existed
			string oldLyrics = tagFile.Tag.Lyrics;
			string newLyrics = (string)row.Cells[this.lyrics1.Index].Value;
			if (oldLyrics != newLyrics && !string.IsNullOrWhiteSpace(newLyrics))
			{
				string lyricPreview = string.Join(string.Empty, newLyrics.Take(50));
				string cleanPreview = Regex.Replace(lyricPreview, @"\r\n?|\n", string.Empty);
				string message = string.Format(cultEng, "{0,-100}{1}", "Lyrics: " + cleanPreview, "file: \"" + tagFile.Name + "\"");
				this.PrintLogMessage("write", new[] { message });
				UnsynchronisedLyricsFrame frmUSLT = new UnsynchronisedLyricsFrame(string.Empty, "eng", StringType.UTF16);
				frmUSLT.Text = newLyrics;
				id3v2.RemoveFrames("USLT");
				id3v2.AddFrame(frmUSLT);
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
					successWrite = false;
					// Do nothing. Error is handled at the end of SaveFile method
				}

				await Task.Delay(WriteDelay);
			}

			// Save all mp3 tags (lastwritetime will be modified here)
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
						successWrite = false;
						// Do nothing. Error is handled at the end of SaveFile method
					}

					await Task.Delay(WriteDelay);
				}
			}

			// Change lastwritetime back to original
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
						successWrite = false;
						// Do nothing. Error is handled at the end of SaveFile method
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