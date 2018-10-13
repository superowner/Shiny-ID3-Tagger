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

    internal static class GlobalVariables
	{
		internal static readonly CultureInfo CultEng = new CultureInfo("en-US");

		internal static readonly DateTime Epoch = new DateTime(1970, 1, 1, 0, 0, 0, DateTimeKind.Utc);

		internal static readonly Dictionary<string, int> AlbumHits = new Dictionary<string, int>(StringComparer.OrdinalIgnoreCase);

		internal static readonly Dictionary<string, decimal> TotalDuration = new Dictionary<string, decimal>(StringComparer.OrdinalIgnoreCase);

		internal static DataGridView ActiveDGV { get; set; }

		internal static string LastUsedFolder { get; set; }

		internal static CancellationTokenSource TokenSource { get; set; }
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
		internal static JObject Accounts { get; set; }

		internal static JObject Settings { get; set; }
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
            DataTable table = new DataTable{ Locale = new CultureInfo("en-US") };
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
		internal DataGridViewDoubleBuffered()
		{
			this.DoubleBuffered = true;
		}
	}
}
