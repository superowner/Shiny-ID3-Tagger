//-----------------------------------------------------------------------
// <copyright file="SearchTags.cs" company="Shiny ID3 Tagger">
// Copyright (c) Shiny ID3 Tagger. All rights reserved.
// </copyright>
// <author>ShinyId3Tagger Team</author>
// <summary>Code fired when "Search tags" button is clicked. Calls all APIs and presents their results</summary>
//-----------------------------------------------------------------------

namespace Shiny_ID3_Tagger
{
	using System;
	using System.Collections.Generic;
	using System.Data;
	using System.Diagnostics;
	using System.Drawing;
	using System.Linq;
	using System.Net.Http;
	using System.Threading;
	using System.Threading.Tasks;
	using System.Windows.Forms;
	using GetLyrics;
	using GetTags;
	using GlobalVariables;
	using Utils;

	public partial class Form1
	{
		private List<IGetLyricsService> lyricsServices = new List<IGetLyricsService>
		{
			new GetLyrics.Apiseeds(),
			new GetLyrics.ChartLyrics(),
			new GetLyrics.LoloLyrics(),
			new GetLyrics.Netease(),
			new GetLyrics.ViewLyrics(),
			new GetLyrics.Xiami()
		};

		private List<IGetTagsService> tagsServices = new List<IGetTagsService>
		{
			new GetTags.Amazon(),
			new GetTags.Decibel(),
			new GetTags.Deezer(),
			new GetTags.Discogs(),
			new GetTags.Genius(),
			new GetTags.Gracenote(),
			new GetTags.ITunes(),
			new GetTags.LastFm(),
			new GetTags.MusicBrainz(),
			new GetTags.MusixMatch(),
			new GetTags.Napster(),
			new GetTags.Netease(),
			new GetTags.Qobuz(),
			new GetTags.QQ(),
			new GetTags.SevenDigital(),
			new GetTags.Spotify(),
			new GetTags.Tidal()
		};

		// ###########################################################################
		private async void StartSearching(CancellationToken cancelToken)
		{
			// Prepare visual stuff like disable buttons during work, show two progress bars
			this.Form_EnableUI(false);

			this.slowProgressBar.Maximum = this.dataGridView1.Rows.Count;
			this.slowProgressBar.Value = 0;
			this.slowProgressBar.Visible = true;
			this.fastProgressBar.Visible = true;

			Stopwatch sw = new Stopwatch();

			try
			{
				using (HttpClient client = Utils.InitiateHttpClient())
				{
					foreach (DataGridViewRow row in this.dataGridView1.Rows)
					{
						cancelToken.ThrowIfCancellationRequested();

						Id3 tagOld = new Id3
						{
							Filepath = row.Cells[this.filepath1.Index].Value.ToString(),
							Artist = row.Cells[this.artist1.Index].Value.ToString(),
							Title = row.Cells[this.title1.Index].Value.ToString(),
							Album = row.Cells[this.album1.Index].Value.ToString(),
							Date = row.Cells[this.date1.Index].Value.ToString(),
							Genre = row.Cells[this.genre1.Index].Value.ToString(),
							DiscCount = row.Cells[this.disccount1.Index].Value.ToString(),
							DiscNumber = row.Cells[this.discnumber1.Index].Value.ToString(),
							TrackCount = row.Cells[this.trackcount1.Index].Value.ToString(),
							TrackNumber = row.Cells[this.tracknumber1.Index].Value.ToString(),
							Lyrics = row.Cells[this.lyrics1.Index].Value.ToString(),
							Cover = row.Cells[this.cover1.Index].Value.ToString()
						};

						bool rowAlreadyExists = (from r in this.dataGridView2.Rows.Cast<DataGridViewRow>()
												 where r.Cells[this.filepath2.Index].Value.ToString() == tagOld.Filepath
												 select r).Any();

						// Check if row doesn't exist already in datagridview and if user didn't press cancel
						if (!rowAlreadyExists)
						{
							sw.Restart();

							Id3 tagNew = new Id3()
							{
								Service = "RESULT",
								Filepath = tagOld.Filepath
							};

							Task<DataTable> apiTask = this.StartId3Search(client, tagOld, cancelToken);
							Task<KeyValuePair<string, string>> lyricSearchTask =
								this.StartLyricsSearch(client, tagOld, cancelToken);

							await Task.WhenAll(apiTask, lyricSearchTask);

							DataTable apiResults = apiTask.Result;
							KeyValuePair<string, string> lyricsNew = lyricSearchTask.Result;

							string artistNew = (from row1 in apiResults.AsEnumerable()
												where !string.IsNullOrWhiteSpace(row1.Field<string>("artist"))
												group row1 by Utils.Capitalize(
													Utils.Strip(row1.Field<string>("artist")))
												into grp
												where grp.Count() >= 3
												orderby grp.Count() descending
												select grp.Key).FirstOrDefault();

							string titleNew = (from row1 in apiResults.AsEnumerable()
											   where !string.IsNullOrWhiteSpace(row1.Field<string>("title"))
											   group row1 by Utils.Capitalize(Utils.Strip(row1.Field<string>("title")))
											   into grp
											   where grp.Count() >= 3
											   orderby grp.Count() descending
											   select grp.Key).FirstOrDefault();

							// If new artist or title are different from old ones, repeat all searches until new and old ones match.
							// This happens when spelling mistakes were corrected by many APIs
							if (artistNew != null && titleNew != null &&
								(artistNew.ToLowerInvariant() != tagOld.Artist.ToLowerInvariant() ||
								 titleNew.ToLowerInvariant() != tagOld.Title.ToLowerInvariant()))
							{
								this.PrintLogMessage(this.rtbSearchLog, new[] { "  Spelling mistake detected. New search for: \"" + artistNew + " - " + titleNew + "\"" });

								sw.Restart();

								lyricsNew = default(KeyValuePair<string, string>);
								tagNew.Artist = artistNew;
								tagNew.Title = titleNew;

								apiTask = this.StartId3Search(client, tagNew, cancelToken);
								lyricSearchTask = this.StartLyricsSearch(client, tagNew, cancelToken);

								await Task.WhenAll(apiTask, lyricSearchTask);

								apiResults = apiTask.Result;
								lyricsNew = lyricSearchTask.Result;
							}

							// Aggregate all API results and select most frequent values
							tagNew = this.CalculateResults(apiResults, tagNew);

							if (tagNew.Album != null && lyricsNew.Value != null)
							{
								tagNew.Lyrics = lyricsNew.Value;
								this.PrintLogMessage(this.rtbSearchLog, new[] { "  Lyrics taken from " + lyricsNew.Key });
							}

							foreach (DataRow r in apiResults.Rows)
							{
								string albumhit = Utils.IncreaseAlbumCounter(r["service"].ToString(), r["album"].ToString(), tagNew.Album);
								string durationTotal = Utils.IncreaseTotalDuration(r["service"].ToString(), r["duration"].ToString());

								this.dataGridView2.Rows.Add(
									(this.dataGridView2.Rows.Count + 1).ToString(),
									tagNew.Filepath ?? string.Empty,
									r["service"] ?? string.Empty,
									r["artist"] ?? string.Empty,
									r["title"] ?? string.Empty,
									r["album"] ?? string.Empty,
									r["date"] ?? string.Empty,
									r["genre"] ?? string.Empty,
									r["disccount"] ?? string.Empty,
									r["discnumber"] ?? string.Empty,
									r["trackcount"] ?? string.Empty,
									r["tracknumber"] ?? string.Empty,
									r["lyrics"] ?? string.Empty,
									r["cover"] ?? string.Empty,
									r["duration"] ?? string.Empty,
									durationTotal ?? string.Empty,
									albumhit ?? string.Empty);

								// Set row foreground color to gray if current row album doesn't match most frequent album
								if (tagNew.Album == null ||
									(tagNew.Album != null && r["album"] != null &&
									 Utils.Strip(tagNew.Album).ToLowerInvariant() !=
									 Utils.Strip(r["album"].ToString().ToLowerInvariant())))
								{
									this.dataGridView2.Rows[this.dataGridView2.RowCount - 1].DefaultCellStyle
										.ForeColor = Color.Gray;
									DataGridViewLinkCell c =
										this.dataGridView2.Rows[this.dataGridView2.RowCount - 1]
											.Cells[this.cover2.Index] as DataGridViewLinkCell;
									c.LinkColor = Color.Gray;
								}
							}

							// Mark changes in datagridView1: green = new value, yellow = minor change, red = big change
							// Red is according to Levenshtein and allowedEditPercent setting
							this.DataGridView_MarkChange(row.Index, this.artist1.Index, tagOld.Artist, tagNew.Artist, true);
							this.DataGridView_MarkChange(row.Index, this.title1.Index, tagOld.Title, tagNew.Title, true);
							this.DataGridView_MarkChange(row.Index, this.album1.Index, tagOld.Album, tagNew.Album, true);
							this.DataGridView_MarkChange(row.Index, this.date1.Index, tagOld.Date, tagNew.Date);
							this.DataGridView_MarkChange(row.Index, this.genre1.Index, tagOld.Genre, tagNew.Genre, true);
							this.DataGridView_MarkChange(row.Index, this.disccount1.Index, tagOld.DiscCount, tagNew.DiscCount);
							this.DataGridView_MarkChange(row.Index, this.discnumber1.Index, tagOld.DiscNumber, tagNew.DiscNumber);
							this.DataGridView_MarkChange(row.Index, this.trackcount1.Index, tagOld.TrackCount, tagNew.TrackCount);
							this.DataGridView_MarkChange(row.Index, this.tracknumber1.Index, tagOld.TrackNumber, tagNew.TrackNumber);
							this.DataGridView_MarkChange(row.Index, this.lyrics1.Index, tagOld.Lyrics, tagNew.Lyrics);
							this.DataGridView_MarkChange(row.Index, this.cover1.Index, tagOld.Cover, tagNew.Cover);

							sw.Stop();
							tagNew.Duration = string.Format("{0:s\\,f}", sw.Elapsed);
							string allApiDurationTotal = Utils.IncreaseTotalDuration(tagNew.Service, tagNew.Duration);

							this.dataGridView2.Rows.Add(
								(this.dataGridView2.Rows.Count + 1).ToString(),
								tagNew.Filepath ?? string.Empty,
								tagNew.Service ?? string.Empty,
								tagNew.Artist ?? string.Empty,
								tagNew.Title ?? string.Empty,
								tagNew.Album ?? string.Empty,
								tagNew.Date ?? string.Empty,
								tagNew.Genre ?? string.Empty,
								tagNew.DiscCount ?? string.Empty,
								tagNew.DiscNumber ?? string.Empty,
								tagNew.TrackCount ?? string.Empty,
								tagNew.TrackNumber ?? string.Empty,
								tagNew.Lyrics ?? string.Empty,
								tagNew.Cover ?? string.Empty,
								tagNew.Duration ?? string.Empty,
								allApiDurationTotal ?? string.Empty,
								string.Empty);

							this.dataGridView2.Rows[this.dataGridView2.RowCount - 1].Cells[this.lyrics2.Index].ToolTipText = tagNew.Lyrics;
							this.dataGridView2.Rows[this.dataGridView2.RowCount - 1].DefaultCellStyle.BackColor = Color.Yellow;
							this.dataGridView2.FirstDisplayedScrollingRowIndex = this.dataGridView2.RowCount - 1;
							this.dataGridView2.ClearSelection();
						}

						this.slowProgressBar.PerformStep();
					}
				}
			}
			catch (OperationCanceledException)
			{
				// User pressed Cancel button. Nothing further to do
			}

			// Work finished, re-enable all buttons and hide progress bars
			this.slowProgressBar.Visible = false;
			this.fastProgressBar.Visible = false;

			// Set focus to the active dataGridView. Needed for keyPress events being fired like space or ESC key
			GlobalVariables.ActiveDGV.Select();

			// Enable all buttons and menus again
			this.Form_EnableUI(true);
		}

		// ###########################################################################
		private async Task<KeyValuePair<string, string>> StartLyricsSearch(HttpMessageInvoker client, Id3 tagNew, CancellationToken cancelToken)
		{
			KeyValuePair<string, string> lyrics = default(KeyValuePair<string, string>);
			Dictionary<string, string> lyricResults = new Dictionary<string, string>();

			if (tagNew.Artist == null && tagNew.Title == null)
			{
				return lyrics;
			}

			List<Task<Id3>> taskList = this.lyricsServices.Select(service => service.GetLyrics(
																  client, tagNew, cancelToken)).ToList();

			try
			{
				while (taskList.Count > 0)
				{
					cancelToken.ThrowIfCancellationRequested();

					Tuple<List<Task<Id3>>, Id3> tpl = await this.CollectTaskResults(taskList);
					taskList = tpl.Item1;
					Id3 r = tpl.Item2;

					lyricResults.Add(r.Service, r.Lyrics);
				}
			}
			catch (OperationCanceledException)
			{
				// User pressed Cancel button. Nothing further to do
			}

			// Netease and Xiami often have poorer lyrics in comparison to Lololyrics and Chartlyrics
			// "lyricsPriority" setting in settings.json decides what lyrics should be taken if there are multiple sources available
			foreach (string api in User.Settings["LyricsPriority"])
			{
				lyrics = (from kvp in lyricResults
						  where !string.IsNullOrWhiteSpace(kvp.Value)
						  where kvp.Key.ToLowerInvariant() == api.ToLowerInvariant()
						  select kvp).FirstOrDefault();

				if (lyrics.Value != null)
				{
					break;
				}
			}

			return lyrics;
		}

		// ###########################################################################
		private async Task<DataTable> StartId3Search(HttpMessageInvoker client, Id3 tagOld, CancellationToken cancelToken)
		{
			DataTable apiResults = Id3.CreateId3Table();

			string artistToSearch = Utils.Strip(tagOld.Artist);
			string titleToSearch = Utils.Strip(tagOld.Title);

			string message = $"{"Search for: \"" + artistToSearch + " - " + titleToSearch + "\"", -100}{"file: \"" + tagOld.Filepath + "\""}";
			this.PrintLogMessage(this.rtbSearchLog, new[] { message });

			List<Task<Id3>> taskList = this.tagsServices.Select(service =>
																	 service.GetTags(
																		 client,
																		 artistToSearch,
																		 titleToSearch,
																		 cancelToken))
										   .ToList();

			this.fastProgressBar.Maximum = taskList.Count;
			this.fastProgressBar.Value = 0;

			try
			{
				while (taskList.Count > 0)
				{
					cancelToken.ThrowIfCancellationRequested();

					Tuple<List<Task<Id3>>, Id3> tpl = await this.CollectTaskResults(taskList);
					taskList = tpl.Item1;
					Id3 r = tpl.Item2;

					apiResults.Rows.Add(
						apiResults.Rows.Count + 1,
						tagOld.Filepath,
						r.Service,
						r.Artist,
						r.Title,
						r.Album,
						r.Date,
						r.Genre,
						r.DiscCount,
						r.DiscNumber,
						r.TrackCount,
						r.TrackNumber,
						string.Empty,
						r.Cover,
						r.Duration);

					this.fastProgressBar.PerformStep();
				}
			}
			catch (OperationCanceledException)
			{
				// User pressed Cancel button. Nothing further to do
			}

			return apiResults;
		}

		private async Task<Tuple<List<Task<Id3>>, Id3>> CollectTaskResults(List<Task<Id3>> taskList)
		{
			// ConfigureAwait(false) is used to remove sluggishness in GUI	/ https://www.thomaslevesque.com/2015/11/11/explicitly-switch-to-the-ui-thread-in-an-async-method/
			Task<Id3> finishedTask = await Task.WhenAny(taskList).ConfigureAwait(false);
			Id3 r = await finishedTask;

			taskList.Remove(finishedTask);

			// Return 2 values with a tuple. First, the new taskList which is one item smaller than before, because one task was finished
			// Second value is the actual result from that finished task. Hand this value back to the parent thread which updates the GUI
			return new Tuple<List<Task<Id3>>, Id3>(taskList, r);

			// No need to dispose tasks https://blogs.msdn.microsoft.com/pfxteam/2012/03/25/do-i-need-to-dispose-of-tasks/
		}

		// ###########################################################################
		private Id3 CalculateResults(DataTable apiResults, Id3 tagNew)
		{
			var majorityAlbumRows = (from row in apiResults.AsEnumerable()
									 where !string.IsNullOrWhiteSpace(row.Field<string>("album"))
									 orderby Utils.ConvertStringToDate(row.Field<string>("date"))
												  .ToString("yyyyMMddHHmmss", GlobalVariables.CultEng)
									 group row by Utils.Strip(row.Field<string>("album").ToUpperInvariant())
									 into grp
									 where grp.Count() >= 3
									 orderby grp.Count() descending
									 select grp).FirstOrDefault();

			var test = from row in apiResults.AsEnumerable()
					   where !string.IsNullOrWhiteSpace(row.Field<string>("album"))
					   orderby Utils.ConvertStringToDate(row.Field<string>("date"))
									.ToString("yyyyMMddHHmmss", GlobalVariables.CultEng)
					   group row by Utils.Strip(row.Field<string>("album").ToUpperInvariant())
					   into grp
					   where grp.Count() >= 3
					   orderby grp.Count() descending
					   select grp;

			if (majorityAlbumRows != null)
			{
				tagNew.Album = (from row in majorityAlbumRows
								group row by Utils.Capitalize(Utils.Strip(row.Field<string>("album")))
								into grp
								orderby grp.Count() descending
								select grp.Key).FirstOrDefault();

				tagNew.Artist = (from row in majorityAlbumRows
								 where !string.IsNullOrWhiteSpace(row.Field<string>("artist"))
								 group row by Utils.Capitalize(Utils.Strip(row.Field<string>("artist")))
								 into grp
								 orderby grp.Count() descending
								 select grp.Key).FirstOrDefault();

				tagNew.Title = (from row in majorityAlbumRows
								where !string.IsNullOrWhiteSpace(row.Field<string>("title"))
								group row by Utils.Capitalize(Utils.Strip(row.Field<string>("title")))
								into grp
								orderby grp.Count() descending
								select grp.Key).FirstOrDefault();

				tagNew.Date = (from row in majorityAlbumRows
							   where !string.IsNullOrWhiteSpace(row.Field<string>("date"))
							   group row by Utils.ConvertStringToDate(row.Field<string>("date")).Year
												 .ToString(GlobalVariables.CultEng)
							   into grp
							   orderby grp.Count() descending, grp.Key ascending
							   select grp.Key).FirstOrDefault();

				tagNew.Genre = (from row in majorityAlbumRows
								where !string.IsNullOrWhiteSpace(row.Field<string>("genre"))
								group row by Utils.Capitalize(Utils.Strip(row.Field<string>("genre")))
								into grp
								orderby grp.Count() descending
								select grp.Key).FirstOrDefault();

				tagNew.DiscCount = (from row in majorityAlbumRows
									where !string.IsNullOrWhiteSpace(row.Field<string>("disccount"))
									group row by row.Field<string>("disccount")
									into grp
									orderby grp.Count() descending
									select grp.Key).FirstOrDefault();

				tagNew.DiscNumber = (from row in majorityAlbumRows
									 where !string.IsNullOrWhiteSpace(row.Field<string>("discnumber"))
									 group row by row.Field<string>("discnumber")
									 into grp
									 orderby grp.Count() descending
									 select grp.Key).FirstOrDefault();

				tagNew.TrackCount = (from row in majorityAlbumRows
									 where !string.IsNullOrWhiteSpace(row.Field<string>("trackcount"))
									 group row by row.Field<string>("trackcount")
									 into grp
									 orderby grp.Count() descending
									 select grp.Key).FirstOrDefault();

				tagNew.TrackNumber = (from row in majorityAlbumRows
									  where !string.IsNullOrWhiteSpace(row.Field<string>("tracknumber"))
									  group row by row.Field<string>("tracknumber")
									  into grp
									  orderby grp.Count() descending
									  select grp.Key).FirstOrDefault();

				/* COVER PRIORITY in settings.json
				 * Napster (Rhapsody)	no CDN, persistent cover URLs									500 px, always squared
				 * Discogs				no CDN, persistent cover URLs									different sizes, 500 to 600 px, can be non-squared
				 * Qobuz				no CDN, persistent cover URLs									600 px, always squared
				 * Tidal				no CDN, persistent cover URLs									1200 px, always squared
				 * Genius				no CDN, persistent cover URLs									different sizes, 600 to 1500 px, can be non-squared
				 * 7digital				uses a CDN and therefore periodically changes cover URLs		800 px, always squared
				 * Deezer				uses a CDN and therefore periodically changes cover URLs		500 px, always squared
				 * iTunes				uses a CDN and therefore periodically changes cover URLs		600 px, can be non-squared
				 * Last.fm				uses a CDN and therefore periodically changes cover URLs		600 px, always squared
				 * Spotify				uses a CDN and therefore periodically changes cover URLs		640 px, can be non-squared
				 * Gracenote (Sony)		uses a CDN and therefore periodically changes cover URLs		720 px, can be non-squared
				 * Amazon				uses a CDN and therefore periodically changes cover URLs		different sizes, 1200 to 1500 px, can be non-squared
				 * Musicbrainz			no CDN, persistent cover URLs, redirect and slow server			different sizes, 900 to 1800 px, always squared
				 * Netease				no CDN, persistent cover URLs, slow server						different sizes, 600 to 3000 px, can be non-squared
				 * QQ (Tencent)			no CDN, persistent cover URLs, slow server						500 px, always squared
				 * Decibel				needs authentication
				 * Musixmatch			no covers provided
				 *
				 * Musixmatch does not provide any cover URL via API
				 * Despite Decibel provides a cover URL, the resource is not easy to download since authorization via API key in a header is required
				 * Therefore these services must be skipped as cover source. This is done by removing them from "CoverPriority" variable in settings.json
				 */
				foreach (string api in User.Settings["CoverPriority"])
				{
					tagNew.Cover = (from row in majorityAlbumRows
									where !string.IsNullOrWhiteSpace(row.Field<string>("cover"))
									where row.Field<string>("service").ToLowerInvariant() == api.ToLowerInvariant()
									select row.Field<string>("cover")).FirstOrDefault();

					if (tagNew.Cover != null)
					{
						this.PrintLogMessage(this.rtbSearchLog, new[] { "  Cover taken from " + api });
						break;
					}
				}
			}

			return tagNew;
		}
	}
}
