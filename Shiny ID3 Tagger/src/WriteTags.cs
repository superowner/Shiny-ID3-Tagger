//-----------------------------------------------------------------------
// <copyright file="WriteTags.cs" company="Shiny ID3 Tagger">
// Copyright (c) Shiny ID3 Tagger. All rights reserved.
// </copyright>
// <author>ShinyId3Tagger Team</author>
// <summary>Main method for ID3 tag saving. Loops through all result rows and writes their values as tags it's corresponding file</summary>
// https://github.com/mono/taglib-sharp/blob/master/src/TagLib/Id3v2/Frame.cs
//-----------------------------------------------------------------------

namespace Shiny_ID3_Tagger
{
	using System;
	using System.Collections.Generic;
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
	using GlobalVariables;
	using TagLib;
	using TagLib.Id3v2;
	using Utils;

	/// <summary>
	/// Represents the Form1 class which contains all methods who interacts with the UI
	/// </summary>
	public partial class Form1
	{
		// ###########################################################################
		private async void StartWriting()
		{
			this.Form_EnableUI(false);

			this.slowProgressBar.Maximum = this.dataGridView1.Rows.Count;
			this.slowProgressBar.Value = 0;
			this.slowProgressBar.Visible = true;

			using (HttpClient client = Utils.InitiateHttpClient())
			{
				// Begin looping through all files in first dataGridView
				foreach (DataGridViewRow row in this.dataGridView1.Rows)
				{
					this.slowProgressBar.PerformStep();
					string filepath = (string)row.Cells[this.filepath1.Index].Value;

					// Cancel remaining work for this file and continue with next file, if
					// - file is a virtual file from CSV Import
					// - file has no new changes (this is to prevent empty ID3 tags)
					if ((bool)row.Cells[this.isVirtualFile.Index].Value || (bool)row.Cells[this.hasNewValues.Index].EditedFormattedValue == false)
					{
						continue;
					}

					string tagType = "ID3v2.3";

					// Log message to signal begin of writing
					string message = string.Format(GlobalVariables.CultEng, "{0,-100}{1}", $"Begin writing of {tagType} tags", "filepath: \"" + filepath + "\"");
					this.PrintLogMessage(this.rtbWriteLog, new[] { message });

					// Get all existing frames from current file
					using (TagLib.File tagFile = TagLib.File.Create(filepath, "audio/mpeg", ReadStyle.Average))
					{
						// Remove ID3v1 tags. ID3v1 use rarely used nowadays and obsolete
						tagFile.RemoveTags(TagTypes.Id3v1);

						// Store all existing frames in a container which can be altered freely without actually touching the file
						TagLib.Id3v2.Tag tagContainer = (TagLib.Id3v2.Tag)tagFile.GetTag(TagTypes.Id3v2, true);

						// Set ID3 version to ID3v2.3 which means we have to use UTF16 for all strings (a "4" would mean ID3v2.4 where UTF8 must be used)
						tagContainer.Version = 3;

						// List of all taglib-sharp "frame classes" which have a "TextEncoding" property. Not all frame classes support it
						// https://github.com/mono/taglib-sharp/tree/master/src/TagLib/Id3v2/Frames
						HashSet<string> useEncodingClasses = new HashSet<string>()
						{
							"TextInformationFrame",
							"AttachedPictureFrame",
							"CommentsFrame",
							"UnsynchronisedLyricsFrame",
							"SynchronisedLyricsFrame",
							"GeneralEncapsulatedObjectFrame",
							"TermsOfUseFrame",
							"UrlLinkFrame"
						};

						// Change encoding for all frames/tags to UTF16
						IEnumerable<Frame> frameList = tagContainer.GetFrames<Frame>();
						foreach (dynamic frame in frameList)
						{
							string typeName = frame.GetType().Name;
							if (useEncodingClasses.Contains(typeName))
							{
								frame.TextEncoding = StringType.UTF16;
							}

							frame.Render(3);
						}

						// Update tag container (frames and header) to reflect encoding changes
						tagContainer.Render();

						// Add (or replace) text values from dataGridView1 to tag container
						tagContainer = this.AddResultsToTagContainer(tagFile, row, tagContainer);

						// Download and add (or replace) cover image from dataGridView1 to tag container
						tagContainer = await this.AddCoverToTagContainer(tagFile, row, tagContainer, client);

						// Save tag container to file
						bool successWrite = await this.SaveFile(tagFile);

						// Clear background color of current row if all tags could be saved to file successfully
						if (successWrite)
						{
							// Log message to signal end of writing
							this.PrintLogMessage(this.rtbWriteLog, new[] { "DONE!" });

							foreach (DataGridViewCell cell in row.Cells)
							{
								cell.Style.BackColor = Color.Empty;
							}
						}
						else
						{
							// Log message to signal end of writing
							this.PrintLogMessage(this.rtbWriteLog, new[] { "FAILED!" });
						}
					}
				}
			}

			// Work finished, re-enable all buttons and hide progress bar
			this.slowProgressBar.Visible = false;

			this.Form_EnableUI(true);
		}

		// ###########################################################################
		// Overwrite tag values with results from API search
		private TagLib.Id3v2.Tag AddResultsToTagContainer(
			TagLib.File tagFile,
			DataGridViewRow row,
			TagLib.Id3v2.Tag tagContainer)
		{
			// Artist
			string oldArtist = tagFile.Tag.FirstPerformer;
			string newArtist = (string)row.Cells[this.artist1.Index].Value;
			if (oldArtist != newArtist && !string.IsNullOrWhiteSpace(newArtist))
			{
				tagContainer.RemoveFrames("TPE1");
				tagContainer.SetTextFrame("TPE1", newArtist);

				this.PrintLogMessage(this.rtbWriteLog, new[] { "Artist:   " + newArtist });
			}

			// Title
			string oldTitle = tagFile.Tag.Title;
			string newTitle = (string)row.Cells[this.title1.Index].Value;
			if (oldTitle != newTitle && !string.IsNullOrWhiteSpace(newTitle))
			{
				tagContainer.RemoveFrames("TIT2");
				tagContainer.SetTextFrame("TIT2", newTitle);

				this.PrintLogMessage(this.rtbWriteLog, new[] { "Title:    " + newTitle });
			}

			// Album
			string oldAlbum = tagFile.Tag.Album;
			string newAlbum = (string)row.Cells[this.album1.Index].Value;
			if (oldAlbum != newAlbum && !string.IsNullOrWhiteSpace(newAlbum))
			{
				tagContainer.RemoveFrames("TALB");
				tagContainer.SetTextFrame("TALB", newAlbum);

				this.PrintLogMessage(this.rtbWriteLog, new[] { "Album:    " + newAlbum });
			}

			// Genre
			string oldGenre = tagFile.Tag.FirstGenre;
			string newGenre = (string)row.Cells[this.genre1.Index].Value;
			if (oldGenre != newGenre && !string.IsNullOrWhiteSpace(newGenre))
			{
				tagContainer.RemoveFrames("TCON");
				tagContainer.SetTextFrame("TCON", newGenre);

				this.PrintLogMessage(this.rtbWriteLog, new[] { "Genre:    " + newGenre });
			}

			// Disc number + disc count
			string oldDiscnumber = tagFile.Tag.Disc.ToString(GlobalVariables.CultEng);
			string oldDisccount = tagFile.Tag.DiscCount.ToString(GlobalVariables.CultEng);
			string newDiscnumber = (string)row.Cells[this.discnumber1.Index].Value;
			string newDisccount = (string)row.Cells[this.disccount1.Index].Value;
			if ((oldDiscnumber != newDiscnumber && !string.IsNullOrWhiteSpace(newDiscnumber)) ||
				(oldDisccount != newDisccount && !string.IsNullOrWhiteSpace(newDisccount)))
			{
				newDiscnumber = string.IsNullOrWhiteSpace(newDiscnumber) ? oldDiscnumber : newDiscnumber;
				newDisccount = string.IsNullOrWhiteSpace(newDisccount) ? oldDisccount : newDisccount;
				tagContainer.RemoveFrames("TPOS");
				tagContainer.SetTextFrame("TPOS", newDiscnumber + "/" + newDisccount);

				this.PrintLogMessage(this.rtbWriteLog, new[] { "Disc:     " + newDiscnumber + "/" + newDisccount });
			}

			// Track number + track count
			string oldTrackNumber = tagFile.Tag.Track.ToString(GlobalVariables.CultEng);
			string oldTrackCount = tagFile.Tag.TrackCount.ToString(GlobalVariables.CultEng);
			string newTrackNumber = (string)row.Cells[this.tracknumber1.Index].Value;
			string newTrackCount = (string)row.Cells[this.trackcount1.Index].Value;
			if ((oldTrackNumber != newTrackNumber && !string.IsNullOrWhiteSpace(newTrackNumber)) ||
				(oldTrackCount != newTrackCount && !string.IsNullOrWhiteSpace(newTrackCount)))
			{
				newTrackNumber = string.IsNullOrWhiteSpace(newTrackNumber) ? oldTrackNumber : newTrackNumber;
				newTrackCount = string.IsNullOrWhiteSpace(newTrackCount) ? oldTrackCount : newTrackCount;
				tagContainer.RemoveFrames("TRCK");
				tagContainer.SetTextFrame("TRCK", newTrackNumber + "/" + newTrackCount);

				this.PrintLogMessage(this.rtbWriteLog, new[] { "Track:    " + newTrackNumber + "/" + newTrackCount });
			}

			// Date
			// Value is stored in TYER frame which represents only the year
			// TDRC (date of recording) stores full date+time info and consolidates TYER (YYYY), TDAT (DDMM) and TIME (HHMM)
			// But TDRC is only available in ID3v2.4 - and this program uses ID3v2.3 for Windows XP/Vista/7 Explorer compatibility. Windows 10 (Creators Update) finally supports ID3v2.4
			// Surprisingly you have to remove TDRC to also remove TYER frames. Probably because taglib operates with 2.4 frame names internally
			string oldDate = tagFile.Tag.Year.ToString(GlobalVariables.CultEng);
			string newDate = (string)row.Cells[this.date1.Index].Value;
			if (oldDate != newDate && !string.IsNullOrWhiteSpace(newDate))
			{
				tagContainer.RemoveFrames("TDRC");
				tagContainer.RemoveFrames("TYER");
				tagContainer.RemoveFrames("TDAT");
				tagContainer.RemoveFrames("TIME");
				tagContainer.SetNumberFrame("TYER", (uint)Utils.ConvertStringToDate(newDate).Year, 0);

				this.PrintLogMessage(this.rtbWriteLog, new[] { "Date:     " + newDate });
			}

			// Lyrics
			// To set "eng" as language you have to remove and add whole frame back again
			// Otherwise taglib sets system default language e.g "deu" or "esp" as lyrics language if USLT tag already existed
			string oldLyrics = tagFile.Tag.Lyrics;
			string newLyrics = (string)row.Cells[this.lyrics1.Index].Value;
			if (oldLyrics != newLyrics && !string.IsNullOrWhiteSpace(newLyrics))
			{
				UnsynchronisedLyricsFrame frmUSLT = new UnsynchronisedLyricsFrame(string.Empty, "eng", StringType.UTF16)
				{
					Text = newLyrics
				};

				tagContainer.RemoveFrames("USLT");
				tagContainer.AddFrame(frmUSLT);

				string lyricsSnippet = string.Join(string.Empty, newLyrics.Take(80));
				lyricsSnippet = Regex.Replace(lyricsSnippet, @"\r\n?|\n", " ");
				this.PrintLogMessage(this.rtbWriteLog, new[] { "Lyrics:   " + lyricsSnippet + "..." });
			}

			return tagContainer;
		}

		// ###########################################################################
		// Download and add cover image
		private async Task<TagLib.Id3v2.Tag> AddCoverToTagContainer(TagLib.File tagFile, DataGridViewRow row, TagLib.Id3v2.Tag tagContainer, HttpClient client)
		{
			string[] errorMsg = null;
			string url = (string)row.Cells[this.cover1.Index].Value;

			// Check if any URL was found from search results
			if (string.IsNullOrEmpty(url))
			{
				return tagContainer;
			}

			// Check if any valid cover already exists
			foreach (IPicture picture in tagFile.Tag.Pictures)
			{
				if (picture.Data.Count > 0 && (bool)User.Settings["OverwriteImage"] == false)
				{
					// A valid cover already exists AND user don't want to overwrite it => return unmodified tag container
					return tagContainer;
				}

				// Check if picture in file was previously downloaded from same URL as new cover URL which we are about to download
				if (picture.Description == url)
				{
					return tagContainer;
				}
			}

			HttpResponseMessage response = new HttpResponseMessage();
			HttpRequestMessage request = new HttpRequestMessage();
			request.Headers.Add("User-Agent", (string)User.Settings["UserAgent"]); // Mandatory for downloads from Discogs

			// Check if cover URL from search results is a valid URL. If yes, download cover
			if (Utils.IsValidUrl(url))
			{
				request.RequestUri = new Uri(url);
				response = await client.SendAsync(request);

				// Check if any content was returned
				if (response.IsSuccessStatusCode && response.Content != null)
				{
					// Read cover as stream (avoids saving a new file to disk)
					using (MemoryStream streamOrg = (MemoryStream)await response.Content.ReadAsStreamAsync())
					using (MemoryStream streamResized = new MemoryStream())
					{
						// Check if downloaded stream is an image. Some servers use a wrong content type for HTTP response. Therefore you need to check byte markers
						if (Utils.IsValidImage(streamOrg))
						{
							using (Image image = Image.FromStream(streamOrg))
							{
								// Resize image according to user setting "MaxImageSize". Always resize and re-encode
								int longSide = new List<int> { image.Width, image.Height }.Max();
								float resizeFactor = new List<float>
								{
									(float)User.Settings["MaxImageSize"] / (float)image.Width,
									(float)User.Settings["MaxImageSize"] / (float)image.Height
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
									ImageCodecInfo imageEncoder = ImageCodecInfo
																 .GetImageEncoders()
																 .First(
																	  codec => codec.FormatID == ImageFormat.Jpeg.Guid);
									encoderParams.Param[0] = new EncoderParameter(Encoder.Quality, 100L);

									// Save and encode bitmap as new stream which now holds a resized JPEG
									bitmap.Save(streamResized, imageEncoder, encoderParams);
									streamResized.Position = 0;
								}

								// Create ID3 cover tag, use "FrontCover" as type
								AttachedPictureFrame taglibpicture = new AttachedPictureFrame()
								{
									Data = ByteVector.FromStream(streamResized),
									MimeType = MediaTypeNames.Image.Jpeg,
									Type = PictureType.FrontCover,
									Description = url,
									TextEncoding = StringType.Latin1 // Strangely, Unicode is not supported for this field
								};

								// Add cover tag to tag container, this deletes all other existing covers like "BackCover" or "BandLogo"
								tagContainer.Pictures = new IPicture[] { taglibpicture };
							}
						}
						else
						{
							errorMsg = new[]
							{
								"ERROR:    Cover URL does not point to a valid image. File signature did not match any image format!",
								"URL:      " + url,
								"type:     " + response.Content.Headers.ContentType.ToString()
							};
						}
					}
				}
				else
				{
					errorMsg = new[]
					{
						"ERROR:    Cover URL is not reachable!",
						"URL:      " + url,
						"Status:   " + response.ReasonPhrase + ": " + (int)response.StatusCode
					};
				}
			}
			else
			{
				errorMsg = new[]
				{
					"ERROR:    Cover URL is not valid URL format!",
					"URL:      " + url
				};
			}

			if (errorMsg == null)
			{
				string message = string.Format("Picture:  " + request.RequestUri);
				this.PrintLogMessage(this.rtbWriteLog, new[] { message });
			}
			else
			{
				if ((int)User.Settings["DebugLevel"] >= 1)
				{
					this.PrintLogMessage(this.rtbErrorLog, errorMsg);
				}
			}

			return tagContainer;
		}

		// ###########################################################################
		private async Task<bool> SaveFile(TagLib.File tagFile)
		{
			const int WriteDelay = 50;
			const int MaxRetries = 3;
			DateTime lastWriteTime = default(DateTime);
			string exMessage = null;

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
				catch (Exception ex)
				{
					exMessage = ex.Message;
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
					catch (Exception ex)
					{
						exMessage = ex.Message;
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
					catch (Exception ex)
					{
						exMessage = ex.Message;
						successWrite = false;
					}

					await Task.Delay(WriteDelay);
				}
			}

			if (!successWrite)
			{
				string[] errorMsg =
				{
					@"ERROR:    Could not write ID3 tags to file!",
					"File:     " + tagFile.Name,
					"Message:  " + exMessage
				};
				this.PrintLogMessage(this.rtbErrorLog, errorMsg);
			}

			return successWrite;
		}
	}
}
