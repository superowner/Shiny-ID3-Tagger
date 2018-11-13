//-----------------------------------------------------------------------
// <copyright file="GlobalVariables.cs" company="Shiny ID3 Tagger">
// Copyright (c) Shiny ID3 Tagger. All rights reserved.
// </copyright>
// <author>ShinyId3Tagger Team</author>
//-----------------------------------------------------------------------

namespace GlobalVariables
{
	using System;
	using System.Collections.Generic;
	using System.Globalization;
	using System.Threading;
	using System.Windows.Forms;

	/// <summary>
	/// Represents a class for global program variables used throughout the whole program and are not API specific
	/// </summary>
	internal static class GlobalVariables
	{
		/// <summary>
		/// Gets or sets the language culture to english. Important for string to date conversions or string comparisons
		/// </summary>
		internal static readonly CultureInfo CultEng = new CultureInfo("en-US");

		/// <summary>
		/// Gets or sets a ceertain date. Used for unix timespans which start at this date
		/// </summary>
		internal static readonly DateTime Epoch = new DateTime(1970, 1, 1, 0, 0, 0, DateTimeKind.Utc);

		/// <summary>
		/// Gets or sets a list of counters (one entry per service) to keep track how many correct albums were found
		/// </summary>
		internal static readonly Dictionary<string, int> AlbumHits = new Dictionary<string, int>(StringComparer.OrdinalIgnoreCase);

		/// <summary>
		/// Gets or sets a list of seconds (one entry per service) to measure how long a service took to respond to API requests
		/// </summary>
		internal static readonly Dictionary<string, decimal> TotalDuration = new Dictionary<string, decimal>(StringComparer.OrdinalIgnoreCase);

		/// <summary>
		/// Enum to decide which richTextBox to use
		/// </summary>
		internal enum MessageType
		{
			/// <summary>
			/// Use tab "Search Log" for output
			/// </summary>
			Search,

			/// <summary>
			/// Use tab "Write Log" for output
			/// </summary>
			Write,

			/// <summary>
			/// Use tab "Error Log" for output
			/// </summary>
			Error,
		}

		/// <summary>
		/// Gets or sets the currently visible and active dataGridView (is changed via tabs, dataGridView1 is set as initial value in MainForm.cs OnShown() method)
		/// </summary>
		internal static DataGridView ActiveDGV { get; set; }

		/// <summary>
		/// Gets or sets the last used Windows Explorer folder
		/// </summary>
		internal static string LastUsedFolder { get; set; }

		/// <summary>
		/// Gets or sets the global source for any kind of cancelation
		/// </summary>
		internal static CancellationTokenSource TokenSource { get; set; }
	}
}
