//-----------------------------------------------------------------------
// <copyright file="GlobalVariables.cs" company="Shiny ID3 Tagger">
// Copyright (c) Shiny ID3 Tagger. All rights reserved.
// </copyright>
// <author>ShinyId3Tagger Team</author>
// <summary>Static or global variables</summary>
//-----------------------------------------------------------------------

namespace GlobalNamespace
{
	using System;
	using System.Collections.Generic;
	using System.Data;
	using System.Globalization;
	using System.Threading;
	using System.Windows.Forms;

	public partial class Form1
	{
		private static CultureInfo cultEng = new CultureInfo("en-US");

		private static DateTime epoch = new DateTime(1970, 1, 1, 0, 0, 0, DateTimeKind.Utc);

		private static Dictionary<string, int> albumHits = new Dictionary<string, int>(StringComparer.OrdinalIgnoreCase);

		private static Dictionary<string, decimal> totalDuration = new Dictionary<string, decimal>(StringComparer.OrdinalIgnoreCase);

		private static DataGridView ActiveDGV { get; set; }

		private static string LastUsedFolder { get; set; }

		private static CancellationTokenSource TokenSource { get; set; }
	}

	internal static class ApiSessionData
	{
		internal static string MsAccessToken { get; set; }

		internal static DateTime MsAccessTokenExpireDate { get; set; }

		internal static string SpAccessToken { get; set; }

		internal static DateTime SpAccessTokenExpireDate { get; set; }

		internal static string TiSessionID { get; set; }

		internal static string TiCountryCode { get; set; }

		internal static DateTime TiSessionExpireDate { get; set; }
	}

	internal static class User
	{
		internal static Dictionary<string, dynamic> Settings { get; set; }

		internal static Dictionary<string, string> Accounts { get; set; }

		internal static DataTable DbAccounts { get; set; }

		internal static DataTable MgAccounts { get; set; }

		internal static DataTable MmAccounts { get; set; }
	}

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
			DataTable table = new DataTable();
			table.Locale = new CultureInfo("en-US");
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

	internal class DataGridViewDoubleBuffered : DataGridView
	{
		public DataGridViewDoubleBuffered()
		{
			this.DoubleBuffered = true;
		}
	}
}
