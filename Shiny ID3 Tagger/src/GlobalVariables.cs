//-----------------------------------------------------------------------
// <copyright file="GlobalVariables.cs" company="Shiny ID3 Tagger">
// Copyright (c) Shiny ID3 Tagger. All rights reserved.
// </copyright>
// <author>ShinyId3Tagger Team</author>
// <summary>Static or global variables</summary>
//-----------------------------------------------------------------------

namespace GlobalVariables
{
	using System;
	using System.Collections.Generic;
	using System.Data;
	using System.Globalization;
	using System.Threading;
	using System.Windows.Forms;
	using Newtonsoft.Json.Linq;

	public class GlobalVariables
	{
		public static CultureInfo cultEng = new CultureInfo("en-US");

		public static DateTime epoch = new DateTime(1970, 1, 1, 0, 0, 0, DateTimeKind.Utc);

		public static Dictionary<string, int> albumHits = new Dictionary<string, int>(StringComparer.OrdinalIgnoreCase);

		public static Dictionary<string, decimal> totalDuration = new Dictionary<string, decimal>(StringComparer.OrdinalIgnoreCase);

		public static DataGridView ActiveDGV { get; set; }

		public static string LastUsedFolder { get; set; }

		public static CancellationTokenSource TokenSource { get; set; }
	}

	public static class ApiSessionData
	{
		public static string MsAccessToken { get; set; }

		public static DateTime MsAccessTokenExpireDate { get; set; }

		public static string SpAccessToken { get; set; }

		public static DateTime SpAccessTokenExpireDate { get; set; }

		public static string TiSessionID { get; set; }

		public static string TiCountryCode { get; set; }

		public static DateTime TiSessionExpireDate { get; set; }
	}

	public static class User
	{
		public static JObject Accounts { get; set; }

		public static JObject Settings { get; set; }
	}

	public class Id3
	{
		public string Filepath { get; set; }

		public string Service { get; set; }

		public string Artist { get; set; }

		public string Title { get; set; }

		public string Album { get; set; }

		public string Date { get; set; }

		public string Genre { get; set; }

		public string DiscNumber { get; set; }

		public string DiscCount { get; set; }

		public string TrackNumber { get; set; }

		public string TrackCount { get; set; }

		public string Lyrics { get; set; }

		public string Cover { get; set; }

		public string Duration { get; set; }

		public static DataTable CreateId3Table()
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

	public class DataGridViewDoubleBuffered : DataGridView
	{
		public DataGridViewDoubleBuffered()
		{
			this.DoubleBuffered = true;
		}
	}
}
