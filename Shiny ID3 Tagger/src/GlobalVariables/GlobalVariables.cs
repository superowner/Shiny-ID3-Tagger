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
	using System.Globalization;
	using System.Threading;
	using System.Windows.Forms;

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
}
