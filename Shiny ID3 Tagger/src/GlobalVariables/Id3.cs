//-----------------------------------------------------------------------
// <copyright file="Id3.cs" company="Shiny ID3 Tagger">
// Copyright (c) Shiny ID3 Tagger. All rights reserved.
// </copyright>
// <author>ShinyId3Tagger Team</author>
// <summary>Static or global variables</summary>
//-----------------------------------------------------------------------

namespace GlobalVariables
{
	using System;
	using System.Data;
	using System.Globalization;

	internal class Id3
	{
		internal string Filepath { get; set; }

		internal string Service { get; set; }

		internal string Artist { get; set; }

		internal string Title { get; set; }

		internal string Album { get; set; }

		internal string Date { get; set; }

		internal string Genre { get; set; }

		internal string DiscNumber { get; set; }

		internal string DiscCount { get; set; }

		internal string TrackNumber { get; set; }

		internal string TrackCount { get; set; }

		internal string Lyrics { get; set; }

		internal string Cover { get; set; }

		internal string Duration { get; set; }

		internal static DataTable CreateId3Table()
		{
			DataTable table = new DataTable{ Locale = GlobalVariables.CultEng };
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
