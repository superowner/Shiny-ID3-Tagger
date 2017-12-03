//-----------------------------------------------------------------------
// <copyright file="SearchTags.cs" company="Shiny Id3 Tagger">
//	 Copyright (c) Shiny Id3 Tagger. All rights reserved.
// </copyright>
// <author>ShinyId3Tagger Team</author>
// <summary>Code fired when "Search tags" button is clicked. Calls all webservice APIs</summary>
//-----------------------------------------------------------------------

namespace GlobalNamespace
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

	public partial class Form1
	{
		// ###########################################################################
		private async void StartSearching(CancellationToken cancelToken)		
		{
			// Prepare visual stuff like disable buttons during work, show two progress bars
			this.btnAddFiles.Enabled = false;
			this.btnWrite.Enabled = false;
			this.btnSearch.Enabled = false;

			this.progressBar2.Maximum = this.dataGridView1.Rows.Count;
			this.progressBar2.Value = 0;
			this.progressBar2.Visible = true;
			this.progressBar1.Visible = true;

			HttpClient client = InitiateHttpClient();
			Stopwatch sw = new Stopwatch();

			foreach (DataGridViewRow row in this.dataGridView1.Rows)
			{
				sw.Restart();

				Id3 tagOld = new Id3();
				tagOld.Filepath = row.Cells[this.filepath1.Index].Value.ToString();
				tagOld.Artist = row.Cells[this.artist1.Index].Value.ToString();
				tagOld.Title = row.Cells[this.title1.Index].Value.ToString();
				tagOld.Album = row.Cells[this.album1.Index].Value.ToString();
				tagOld.Date = row.Cells[this.date1.Index].Value.ToString();
				tagOld.Genre = row.Cells[this.genre1.Index].Value.ToString();
				tagOld.DiscCount = row.Cells[this.disccount1.Index].Value.ToString();
				tagOld.DiscNumber = row.Cells[this.discnumber1.Index].Value.ToString();
				tagOld.TrackCount = row.Cells[this.trackcount1.Index].Value.ToString();
				tagOld.TrackNumber = row.Cells[this.tracknumber1.Index].Value.ToString();
				tagOld.Lyrics = row.Cells[this.lyrics1.Index].Value.ToString();
				tagOld.Cover = row.Cells[this.cover1.Index].Value.ToString();

				bool rowAlreadyExists = (from r in this.dataGridView2.Rows.Cast<DataGridViewRow>()
									where r.Cells[this.filepath2.Index].Value.ToString() == tagOld.Filepath
									select r).Any();

				if (rowAlreadyExists == false)
				{
					Id3 tagNew = new Id3();
					tagNew.Service = "RESULT";
					tagNew.Filepath = tagOld.Filepath;
					
					Task<DataTable> webservicesTask = this.StartId3Search(client, tagOld, cancelToken);
					Task<KeyValuePair<string, string>> lyricSearchTask = this.StartLyricsSearch(client, tagOld, cancelToken);
					
					await Task.WhenAll(webservicesTask, lyricSearchTask);
					
					DataTable webserviceResults = webservicesTask.Result;
					KeyValuePair<string, string> lyricsNew = lyricSearchTask.Result;
					
					string artistNew = (from row1 in webserviceResults.AsEnumerable()
								where !string.IsNullOrWhiteSpace(row1.Field<string>("artist"))
								group row1 by Capitalize(Strip(row1.Field<string>("artist"))) into grp
								where grp.Count() >= 3
								orderby grp.Count() descending
								select grp.Key).FirstOrDefault();
	
					string titleNew = (from row1 in webserviceResults.AsEnumerable()
								where !string.IsNullOrWhiteSpace(row1.Field<string>("title"))
								group row1 by Capitalize(Strip(row1.Field<string>("title"))) into grp
								where grp.Count() >= 3
								orderby grp.Count() descending
								select grp.Key).FirstOrDefault();
	
					// If new artist or title are different from old ones, repeat all searches until new and old ones match.
					// This happens when spelling mistakes were corrected by many APIs
					if (artistNew != null && titleNew != null &&
					    (artistNew.ToLower() != tagOld.Artist.ToLower() ||
					      titleNew.ToLower() != tagOld.Title.ToLower()))
					{
						this.PrintLogMessage("search", new[] { "  Spelling mistake detected. New search for: \"" + artistNew + " - " + titleNew + "\"" });

						sw.Restart();
						
						lyricsNew = new KeyValuePair<string, string>();
						tagNew.Artist = artistNew;
						tagNew.Title = titleNew;
						
						webservicesTask = this.StartId3Search(client, tagNew, cancelToken);
						lyricSearchTask = this.StartLyricsSearch(client, tagNew, cancelToken);
						
						await Task.WhenAll(webservicesTask, lyricSearchTask);
						
						webserviceResults = webservicesTask.Result;
						lyricsNew = lyricSearchTask.Result;
					}
					
					// Aggregate all API results and select the most frequent values					
					tagNew = this.CalculateResults(webserviceResults, tagNew);

					if (tagNew.Album != null && lyricsNew.Value != null)
					{
						tagNew.Lyrics = lyricsNew.Value;
						this.PrintLogMessage("search", new[] { "  Lyrics taken from " + lyricsNew.Key });
					}
					
					if (cancelToken.IsCancellationRequested)
					{
						break;
					}
					
					foreach (DataRow r in webserviceResults.Rows)
					{
						string albumhit = IncreaseAlbumCounter(r["service"], r["album"], tagNew.Album);

						this.dataGridView2.Rows.Add(
							(this.dataGridView2.Rows.Count + 1).ToString(),
							tagNew.Filepath ?? string.Empty,
							r["service"] ?? string.Empty,
							r["duration"] ?? string.Empty,
							albumhit ?? string.Empty,
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
							r["cover"] ?? string.Empty);

						// Set row background color to grey if current row album doesnt match the most frequent album
						if (tagNew.Album == null ||
							(tagNew.Album != null && r["album"] != null &&
							Strip(tagNew.Album).ToLower() != Strip(r["album"].ToString().ToLower())))
						{
							this.dataGridView2.Rows[this.dataGridView2.RowCount - 1].DefaultCellStyle.ForeColor = Color.Gray;
						}
					}

					// Color cells green, yellow or red according to Levenshtein and allowedEditPercent setting
					this.MarkChange(row.Index, this.artist1.Index, tagOld.Artist, tagNew.Artist, true);
					this.MarkChange(row.Index, this.title1.Index, tagOld.Title, tagNew.Title, true);
					this.MarkChange(row.Index, this.album1.Index, tagOld.Album, tagNew.Album, true);
					this.MarkChange(row.Index, this.date1.Index, tagOld.Date, tagNew.Date, false);
					this.MarkChange(row.Index, this.genre1.Index, tagOld.Genre, tagNew.Genre, true);
					this.MarkChange(row.Index, this.disccount1.Index, tagOld.DiscCount, tagNew.DiscCount, false);
					this.MarkChange(row.Index, this.discnumber1.Index, tagOld.DiscNumber, tagNew.DiscNumber, false);
					this.MarkChange(row.Index, this.trackcount1.Index, tagOld.TrackCount, tagNew.TrackCount, false);
					this.MarkChange(row.Index, this.tracknumber1.Index, tagOld.TrackNumber, tagNew.TrackNumber, false);
					this.MarkChange(row.Index, this.lyrics1.Index, tagOld.Lyrics, tagNew.Lyrics, false);
					this.MarkChange(row.Index, this.cover1.Index, tagOld.Cover, tagNew.Cover, false);

					sw.Stop();
					tagNew.Duration = string.Format("{0:s\\,f}", sw.Elapsed);

					this.dataGridView2.Rows.Add(
						(this.dataGridView2.Rows.Count + 1).ToString(),
						tagNew.Filepath ?? string.Empty,
						tagNew.Service ?? string.Empty,
						tagNew.Duration ?? string.Empty,
						string.Empty,
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
						tagNew.Cover ?? string.Empty);

					this.dataGridView2.Rows[this.dataGridView2.RowCount - 1].Cells[this.lyrics2.Index].ToolTipText = tagNew.Lyrics;
					this.dataGridView2.Rows[this.dataGridView2.RowCount - 1].DefaultCellStyle.BackColor = Color.Yellow;
					this.dataGridView2.FirstDisplayedScrollingRowIndex = this.dataGridView2.RowCount - 1;
					this.dataGridView2.ClearSelection();
					webserviceResults.Dispose();
				}

				this.progressBar2.PerformStep();
			}

			client.Dispose();

			this.progressBar1.Visible = false;
			this.progressBar2.Visible = false;
			this.btnAddFiles.Enabled = true;
			this.btnWrite.Enabled = true;
			this.btnSearch.Enabled = true;
		}

		// ###########################################################################
		private async Task<KeyValuePair<string, string>> StartLyricsSearch(HttpMessageInvoker client, Id3 tagNew, CancellationToken cancelToken)
		{
			var lyricResults = new Dictionary<string, string>();
			
			List<Task<Id3>> taskList = new List<Task<Id3>>();
			taskList.Add(this.GetLyrics_Netease(client, tagNew, cancelToken));
			taskList.Add(this.GetLyrics_Chartlyrics(client, tagNew, cancelToken));
			taskList.Add(this.GetLyrics_Lololyrics(client, tagNew, cancelToken));
			taskList.Add(this.GetLyrics_Xiami(client, tagNew, cancelToken));

			while (taskList.Count > 0)
			{
				Tuple<List<Task<Id3>>, Id3> tpl = await this.CollectTaskResults(taskList);
				taskList = tpl.Item1;
				Id3 r = tpl.Item2;
				
				lyricResults.Add(r.Service, r.Lyrics);
			}
			
			// Netease and Xiami often have poorer lyrics in comparison to Lololyrics and Chartlyrics
			// The lyricsPriority setting in settings.json decides what lyrics should be taken if there are multiple sources available
			KeyValuePair<string, string> lyrics = new KeyValuePair<string, string>();
			foreach (string service in User.Settings["LyricsPriority"].Split(','))
			{
				lyrics = (from kvp in lyricResults
							where !string.IsNullOrWhiteSpace(kvp.Value)
							where kvp.Key == service.Trim()
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
			DataTable webserviceResults = Id3.CreateId3Table();

			string artistToSearch = Strip(tagOld.Artist);
			string titleToSearch = Strip(tagOld.Title);

			string message = string.Format(
								"{0,-100}{1}",
								"Search for: \""  + artistToSearch + " - " + titleToSearch + "\"",
								"file: \"" + tagOld.Filepath + "\"");
			this.PrintLogMessage("search", new[] { message });

			List<Task<Id3>> taskList = new List<Task<Id3>>();
			taskList.Add(this.GetTags_7digital(client, artistToSearch, titleToSearch, cancelToken));
			taskList.Add(this.GetTags_Amazon(client, artistToSearch, titleToSearch, cancelToken));
			taskList.Add(this.GetTags_Decibel(client, artistToSearch, titleToSearch, cancelToken));
			taskList.Add(this.GetTags_Deezer(client, artistToSearch, titleToSearch, cancelToken));
			taskList.Add(this.GetTags_Discogs(client, artistToSearch, titleToSearch, cancelToken));
			taskList.Add(this.GetTags_Genius(client, artistToSearch, titleToSearch, cancelToken));
			taskList.Add(this.GetTags_Gracenote(client, artistToSearch, titleToSearch, cancelToken));
			taskList.Add(this.GetTags_Itunes(client, artistToSearch, titleToSearch, cancelToken));
			taskList.Add(this.GetTags_LastFm(client, artistToSearch, titleToSearch, cancelToken));
			taskList.Add(this.GetTags_MsGroove(client, artistToSearch, titleToSearch, cancelToken));
			taskList.Add(this.GetTags_MusicBrainz(client, artistToSearch, titleToSearch, cancelToken));
			taskList.Add(this.GetTags_MusicGraph(client, artistToSearch, titleToSearch, cancelToken));
			taskList.Add(this.GetTags_MusixMatch(client, artistToSearch, titleToSearch, cancelToken));
			taskList.Add(this.GetTags_Napster(client, artistToSearch, titleToSearch, cancelToken));
			taskList.Add(this.GetTags_Netease(client, artistToSearch, titleToSearch, cancelToken));
			taskList.Add(this.GetTags_Qobuz(client, artistToSearch, titleToSearch, cancelToken));
			taskList.Add(this.GetTags_QQ(client, artistToSearch, titleToSearch, cancelToken));
			taskList.Add(this.GetTags_Spotify(client, artistToSearch, titleToSearch, cancelToken));
			taskList.Add(this.GetTags_Tidal(client, artistToSearch, titleToSearch, cancelToken));

			this.progressBar1.Maximum = taskList.Count;
			this.progressBar1.Value = 0;
			
			while (taskList.Count > 0)
			{
				Tuple<List<Task<Id3>>, Id3> tpl = await this.CollectTaskResults(taskList);
				taskList = tpl.Item1;
				Id3 r = tpl.Item2;
				
				webserviceResults.Rows.Add(
					webserviceResults.Rows.Count + 1,
					tagOld.Filepath,
					r.Service,
					r.Duration,
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
					r.Cover);

				this.progressBar1.PerformStep();
			}

			return webserviceResults;
		}

		private async Task<Tuple<List<Task<Id3>>, Id3>> CollectTaskResults(List<Task<Id3>> taskList)
		{
				// Use configureAwait(false) to remove sluggishness in GUI
				// https://www.thomaslevesque.com/2015/11/11/explicitly-switch-to-the-ui-thread-in-an-async-method/
				Task<Id3> finishedTask = await Task.WhenAny(taskList).ConfigureAwait(false);
				Id3 r = await finishedTask;
				
				taskList.Remove(finishedTask);
				finishedTask.Dispose();
				
				// We return 2 values with a tuple. First the new taskList which is one item smaller than before, because one task was finished
				// Second value is the actual result from that finished task. We hand this value back to a thread which updates the GUI
				return new Tuple<List<Task<Id3>>, Id3>(taskList, r);
		}
		
		// ###########################################################################
		private Id3 CalculateResults(DataTable webserviceResults, Id3 tagNew)
		{
			var majorityAlbumRows = (from row in webserviceResults.AsEnumerable()
							where !string.IsNullOrWhiteSpace(row.Field<string>("album"))
							orderby this.ConvertStringToDate(row.Field<string>("date")).ToString("yyyyMMddHHmmss", cultEng)
							group row by Strip(row.Field<string>("album").ToUpperInvariant()) into grp
							where grp.Count() >= 3
							orderby grp.Count() descending
							select grp).FirstOrDefault();

			if (majorityAlbumRows != null)
			{
				tagNew.Album = (from row in majorityAlbumRows
								group row by Capitalize(Strip(row.Field<string>("album"))) into grp
								orderby grp.Count() descending
								select grp.Key).FirstOrDefault();

				tagNew.Artist = (from row in majorityAlbumRows
								where !string.IsNullOrWhiteSpace(row.Field<string>("artist"))
								group row by Capitalize(Strip(row.Field<string>("artist"))) into grp
								orderby grp.Count() descending
								select grp.Key).FirstOrDefault();

				tagNew.Title = (from row in majorityAlbumRows
								where !string.IsNullOrWhiteSpace(row.Field<string>("title"))
								group row by Capitalize(Strip(row.Field<string>("title"))) into grp
								orderby grp.Count() descending
								select grp.Key).FirstOrDefault();

				tagNew.Date = (from row in majorityAlbumRows
								where !string.IsNullOrWhiteSpace(row.Field<string>("date"))
								group row by this.ConvertStringToDate(row.Field<string>("date")).Year.ToString(cultEng) into grp
								orderby grp.Count() descending, grp.Key ascending
								select grp.Key).FirstOrDefault();

				tagNew.Genre = (from row in majorityAlbumRows
								where !string.IsNullOrWhiteSpace(row.Field<string>("genre"))
								group row by Capitalize(Strip(row.Field<string>("genre"))) into grp
								orderby grp.Count() descending
								select grp.Key).FirstOrDefault();
				
				tagNew.DiscCount = (from row in majorityAlbumRows
								where !string.IsNullOrWhiteSpace(row.Field<string>("disccount"))
								group row by row.Field<string>("disccount") into grp
								orderby grp.Count() descending
								select grp.Key).FirstOrDefault();

				tagNew.DiscNumber = (from row in majorityAlbumRows
								where !string.IsNullOrWhiteSpace(row.Field<string>("discnumber"))
								group row by row.Field<string>("discnumber") into grp
								orderby grp.Count() descending
								select grp.Key).FirstOrDefault();

				tagNew.TrackCount = (from row in majorityAlbumRows
								where !string.IsNullOrWhiteSpace(row.Field<string>("trackcount"))
								group row by row.Field<string>("trackcount") into grp
								orderby grp.Count() descending
								select grp.Key).FirstOrDefault();

				tagNew.TrackNumber = (from row in majorityAlbumRows
								where !string.IsNullOrWhiteSpace(row.Field<string>("tracknumber"))
								group row by row.Field<string>("tracknumber") into grp
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
				 * Microsoft Groove)	no CDN, persistent cover URLs, API shut down soon				different sizes, 1200 to 3000 px, can be non-squared				
				 * Decibel				needs authentication
				 * Musixmatch			no covers
				 * Musicgraph			no covers				
				 *
				 * Musixmatch and Musicgraph do not provide any cover URL via API
				 * Despite Decibel provides a cover URL, the URL is not so easy to download because authorization via API key in a header is required
				 * Therefore these services must be skipped as cover source. This is done by removing them from "CoverPriority" variable in settings.json
				 */
				foreach (string service in User.Settings["CoverPriority"].Split(','))
				{
					tagNew.Cover = (from row in majorityAlbumRows
								where !string.IsNullOrWhiteSpace(row.Field<string>("cover"))
								where row.Field<string>("service") == service.Trim()
								select row.Field<string>("cover")).FirstOrDefault();

					if (tagNew.Cover != null)
					{
						this.PrintLogMessage("search", new[] { "  Cover taken from " + service });
						break;
					}
				}
			}

			return tagNew;
		}		
	}
}
