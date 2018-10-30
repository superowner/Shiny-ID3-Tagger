//-----------------------------------------------------------------------
// <copyright file="Id3.cs" company="Shiny ID3 Tagger">
// Copyright (c) Shiny ID3 Tagger. All rights reserved.
// </copyright>
// <author>ShinyId3Tagger Team</author>
//-----------------------------------------------------------------------

namespace GlobalVariables
{
	using System;
	using System.Data;

	/// <summary>
	/// Class representing a ID3 tag
	/// </summary>
	internal class Id3
	{
		/// <summary>
		/// Gets or sets the filepath for which new ID3 tags are searched
		/// </summary>
		internal string Filepath { get; set; }

		/// <summary>
		/// Gets or sets the API service name which is the current source for all the following info fields
		/// </summary>
		internal string Service { get; set; }

		/// <summary>
		/// Gets or sets the artist name
		/// </summary>
		internal string Artist { get; set; }

		/// <summary>
		/// Gets or sets the song title
		/// </summary>
		internal string Title { get; set; }

		/// <summary>
		/// Gets or sets the album name
		/// </summary>
		internal string Album { get; set; }

		/// <summary>
		/// Gets or sets the album publish date
		/// </summary>
		internal string Date { get; set; }

		/// <summary>
		/// Gets or sets the song or album genre
		/// </summary>
		internal string Genre { get; set; }

		/// <summary>
		/// Gets or sets the disc number on which a track is listed
		/// </summary>
		internal string DiscNumber { get; set; }

		/// <summary>
		/// Gets or sets the total disc count of the album
		/// </summary>
		internal string DiscCount { get; set; }

		/// <summary>
		/// Gets or sets the track number of a track on a disc
		/// </summary>
		internal string TrackNumber { get; set; }

		/// <summary>
		/// Gets or sets the total track count of the album
		/// </summary>
		internal string TrackCount { get; set; }

		/// <summary>
		/// Gets or sets the lyrics of a track
		/// </summary>
		internal string Lyrics { get; set; }

		/// <summary>
		/// Gets or sets the URL pointing at a album cover image
		/// </summary>
		internal string Cover { get; set; }

		/// <summary>
		/// Gets or sets the duration in seconds how long the method for the service took to complete
		/// </summary>
		internal string Duration { get; set; }

		/// <summary>
		/// Method to initialize an empty table with predefined columns. Used to fill the dataGridView with results
		/// </summary>
		/// <returns>A dataTable</returns>
		internal static DataTable CreateId3Table()
		{
			DataTable table = new DataTable { Locale = GlobalVariables.CultEng };
			table.Columns.Add("number", typeof(uint));
			table.Columns.Add("filepath", typeof(string));
			table.Columns.Add("service", typeof(string));
			table.Columns.Add("artist", typeof(string));
			table.Columns.Add("title", typeof(string));
			table.Columns.Add("album", typeof(string));
			table.Columns.Add("date", typeof(string));
			table.Columns.Add("genre", typeof(string));
			table.Columns.Add("disccount", typeof(string));
			table.Columns.Add("discnumber", typeof(string));
			table.Columns.Add("trackcount", typeof(string));
			table.Columns.Add("tracknumber", typeof(string));
			table.Columns.Add("lyrics", typeof(string));
			table.Columns.Add("cover", typeof(string));
			table.Columns.Add("duration", typeof(string));
			return table;
		}
	}
}
