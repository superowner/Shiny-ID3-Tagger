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
		private async void StartSearching(string[] files)
		{
			this.btnAddFiles.Enabled = false;
			this.btnWrite.Enabled = false;
			this.btnSearch.Enabled = false;

			this.progressBar2.Maximum = this.dataGridView1.Rows.Count;
			this.progressBar2.Value = 0;
			
			this.progressBar1.Visible = true;
			this.progressBar2.Visible = true;
			
			Runtime.TokenSource = new CancellationTokenSource();
			CancellationToken cancelToken = Runtime.TokenSource.Token;
			
			HttpClient client = InitiateHttpClient();
			Stopwatch sw = new Stopwatch();

			if (files == null || !files.Any())
			{
				files = this.dataGridView1.Rows
					.Cast<DataGridViewRow>()
					.Select(row => row.Cells[this.filepath1.Index].Value.ToString())
					.ToArray();
			}

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
					DataTable webserviceResults = await this.StartAsyncTasks(client, tagOld, cancelToken);
					Id3 tagNew = await this.AnalyzeResults(client, webserviceResults, tagOld, cancelToken);

					if (cancelToken.IsCancellationRequested) 
					{ 
						break; 
					}
					
					foreach (DataRow r in webserviceResults.Rows)
					{
						string albumhit = IncreaseAlbumCounter(r["service"], r["album"], tagNew.Album);

						this.dataGridView2.Rows.Add(
							this.dataGridView2.Rows.Count + 1,
							tagNew.Filepath,
							r["service"],
							r["duration"],
							albumhit,
							r["artist"],
							r["title"],
							r["album"],
							r["date"],
							r["genre"],
							r["disccount"],
							r["discnumber"],
							r["trackcount"],
							r["tracknumber"],
							r["lyrics"],
							r["cover"]);

						if (tagNew.Album != null && r["album"] != null && Strip(tagNew.Album).ToLower() != Strip(r["album"].ToString().ToLower()))
						{
							this.dataGridView2.Rows[this.dataGridView2.RowCount - 1].DefaultCellStyle.ForeColor = Color.Gray;
						}
					}

					this.MarkChange(row.Index, this.artist1.Index, tagOld.Artist, tagNew.Artist, true);
					this.MarkChange(row.Index, this.title1.Index, tagOld.Title, tagNew.Title, true);
					this.MarkChange(row.Index, this.album1.Index, tagOld.Album, tagNew.Album, true);
					this.MarkChange(row.Index, this.date1.Index, tagOld.Date, tagNew.Date, true);
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
						this.dataGridView2.Rows.Count + 1,
						tagNew.Filepath,
						tagNew.Service,
						tagNew.Duration,
						null,
						tagNew.Artist,
						tagNew.Title,
						tagNew.Album,
						tagNew.Date,
						tagNew.Genre,
						tagNew.DiscCount,
						tagNew.DiscNumber,
						tagNew.TrackCount,
						tagNew.TrackNumber,
						tagNew.Lyrics,
						tagNew.Cover);										
					
					this.dataGridView2.Rows[this.dataGridView2.RowCount - 1].Cells[this.lyrics2.Index].ToolTipText = tagNew.Lyrics;
					this.dataGridView2.Rows[this.dataGridView2.RowCount - 1].DefaultCellStyle.BackColor = Color.Yellow;
					this.dataGridView2.FirstDisplayedScrollingRowIndex = this.dataGridView2.RowCount - 1;
					this.dataGridView2.ClearSelection();
					webserviceResults.Dispose();
				}	

				this.progressBar2.PerformStep();
			}

			Runtime.TokenSource.Dispose();
			client.Dispose();
					
			this.progressBar1.Visible = false;
			this.progressBar2.Visible = false;
			this.btnAddFiles.Enabled = true;
			this.btnWrite.Enabled = true;
			this.btnSearch.Enabled = true;
		}

		// ###########################################################################
		private async Task<Id3> AnalyzeResults(HttpMessageInvoker client, DataTable webserviceResults, Id3 tagOld, CancellationToken cancelToken)
		{
			Id3 tagNew = new Id3();

			tagNew.Service = "RESULT";
			tagNew.Filepath = tagOld.Filepath;
			
			var majorityAlbumRows = (from row in webserviceResults.AsEnumerable()
							where !string.IsNullOrWhiteSpace(row.Field<string>("album"))
							orderby this.ConvertStringToDate(row.Field<string>("date")).ToString("yyyyMMddHHmmss")
							group row by Capitalize(Strip(row.Field<string>("album"))) into grp
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
								group row by this.ConvertStringToDate(row.Field<string>("date")).Year.ToString(Runtime.CultEng) into grp
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

				// tagNew.Lyrics = await this.GetLyrics_Chartlyrics(client, tagNew, cancelToken);
				tagNew.Lyrics = await this.GetLyrics_Lololyrics(client, tagNew, cancelToken);

				// While Decibel and Discogs provide a cover URL, their URL is not easily downloadable via code because authorization via header or Oauth is required
				// Musicgraph does not provide any cover URLs via API at all
				// Therefore these 3 services cannot be used as cover source. This is done by removing them from "CoverOrder" variable in file settings.json
				foreach (string service in User.Settings["CoverOrder"].Split(','))
				{
					tagNew.Cover = (from row in majorityAlbumRows
								where !string.IsNullOrWhiteSpace(row.Field<string>("cover"))
								where row.Field<string>("service") == service
								select row.Field<string>("cover")).FirstOrDefault();

					if (tagNew.Cover != null)
					{
						break;
					}
				}
			}
			
			if (User.Settings["UseBingFallback"] && tagNew.Cover == null)
			{
				tagNew.Cover = await this.GetCoverBing(client, tagOld, tagNew, cancelToken);
			}
			
			return tagNew;
		}

		// ###########################################################################
		private async Task<DataTable> StartAsyncTasks(HttpMessageInvoker client, Id3 tagOld, CancellationToken cancelToken)
		{			
			DataTable webserviceResults = Id3.CreateTable();
		
			string artist = Strip(tagOld.Artist);
			string title = Strip(tagOld.Title);

			string message = string.Format(
								"{0,-100}{1}",
								"Search for: \""  + artist + " - " + title + "\"",
								"file: \"" + tagOld.Filepath + "\"");
			this.Log("search", new[] { message });
					
			for (int i = 1; i <= 2; i++)
			{
				List<Task<Id3>> taskList = new List<Task<Id3>>();
				taskList.Add(this.GetTags_Deezer(client, artist, title, cancelToken));
				taskList.Add(this.GetTags_Itunes(client, artist, title, cancelToken));
				taskList.Add(this.GetTags_Spotify(client, artist, title, cancelToken));
				taskList.Add(this.GetTags_Decibel(client, artist, title, cancelToken));
				taskList.Add(this.GetTags_Amazon(client, artist, title, cancelToken));
				taskList.Add(this.GetTags_Discogs(client, artist, title, cancelToken));
				taskList.Add(this.GetTags_Gracenote(client, artist, title, cancelToken));
				taskList.Add(this.GetTags_LastFm(client, artist, title, cancelToken));
				taskList.Add(this.GetTags_MsGroove(client, artist, title, cancelToken));
				taskList.Add(this.GetTags_MusixMatch(client, artist, title, cancelToken));
				taskList.Add(this.GetTags_Napster(client, artist, title, cancelToken));
				taskList.Add(this.GetTags_MusicGraph(client, artist, title, cancelToken));
				taskList.Add(this.GetTags_Qobuz(client, artist, title, cancelToken));
				taskList.Add(this.GetTags_Genius(client, artist, title, cancelToken));
				taskList.Add(this.GetTags_7digital(client, artist, title, cancelToken));
				taskList.Add(this.GetTags_MusicBrainz(client, artist, title, cancelToken));

				this.progressBar1.Maximum = taskList.Count;
				this.progressBar1.Value = 0;

				while (taskList.Count > 0)
				{
					Task<Id3> finishedTask = await Task.WhenAny(taskList);
					Id3 r = await finishedTask;

					taskList.Remove(finishedTask);
					finishedTask.Dispose();

					webserviceResults.Rows.Add(
						(uint)this.dataGridView2.Rows.Count + 1,
						r.Filepath,
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
						null,
						r.Cover);

					this.progressBar1.PerformStep();					
				}

				string artistTemp = (from row1 in webserviceResults.AsEnumerable()
							 where !string.IsNullOrWhiteSpace(row1.Field<string>("artist"))
							 group row1 by Capitalize(Strip(row1.Field<string>("artist"))) into grp
							 where grp.Count() >= 3
							 orderby grp.Count() descending
							 select grp.Key).FirstOrDefault();

				string titleTemp = (from row1 in webserviceResults.AsEnumerable()
							where !string.IsNullOrWhiteSpace(row1.Field<string>("title"))
							group row1 by Capitalize(Strip(row1.Field<string>("title"))) into grp
							where grp.Count() >= 3
							orderby grp.Count() descending
							select grp.Key).FirstOrDefault();

				if (artistTemp == null || titleTemp == null || i == 2 ||
						(artistTemp.ToLower() == artist.ToLower() &&
							titleTemp.ToLower() == title.ToLower()))
				{
					break;
				}
				else
				{
					artist = artistTemp;
					title = titleTemp;
					this.Log("search", new[] { "Spelling mistake detected. New search for: \"" + artist + " - " + title + "\"" });

					webserviceResults.Clear();
				}
			}

			return webserviceResults;
		}
	}
}
