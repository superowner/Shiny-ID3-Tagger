//-----------------------------------------------------------------------
// <copyright file="GlobalVariables.cs" company="Shiny Id3 Tagger">
//	 Copyright (c) Shiny Id3 Tagger. All rights reserved.
// </copyright>
// <author>ShinyId3Tagger Team</author>
// <summary>Static or global variables</summary>
//-----------------------------------------------------------------------

namespace GlobalNamespace
{
	using System.Collections.Generic;
	using System.Data;
	using System.Globalization;
	using System.Threading;	
	using System.Windows.Forms;
	
	internal static class User
	{
		internal static Dictionary<string, dynamic> Settings { get; set; }
		
		internal static Dictionary<string, string> Accounts { get; set; }

		internal static DataTable DbAccounts { get; set; }
		
		internal static DataTable MgAccounts { get; set; }
		
		internal static DataTable MmAccounts { get; set; }
	}
	
	internal static class Runtime
	{
		internal static DataGridView ActiveDGV { get; set; }
		
		internal static Dictionary<string, int> AlbumHits { get; set; }
		
		internal static CultureInfo CultEng { get; set; }
		
		internal static string LastUsedFolder { get; set; }
		
		internal static CancellationTokenSource TokenSource { get; set; }
	}
	
	internal class Id3
	{
		internal string Filepath { get; set; }

		internal string Service { get; set; }

		internal string Duration { get; set; }
		
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

		internal static DataTable CreateTable()
		{
			DataTable table = new DataTable();
			table.Locale = Runtime.CultEng;
			table.Columns.Add("number", typeof(uint));
			table.Columns.Add("filepath", typeof(string));
			table.Columns.Add("service", typeof(string));
			table.Columns.Add("duration", typeof(string));
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
			return table;
		}
	}

	internal class DataGridViewDoubleBuffered : DataGridView
	{
		public DataGridViewDoubleBuffered()
		{
			this.DoubleBuffered = true;
		}
	}	
}
