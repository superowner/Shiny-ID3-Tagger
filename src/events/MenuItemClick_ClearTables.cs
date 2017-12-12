//-----------------------------------------------------------------------
// <copyright file="MenuItemClick_ClearTables.cs" company="Shiny ID3 Tagger">
// Copyright (c) Shiny ID3 Tagger. All rights reserved.
// </copyright>
// <author>ShinyId3Tagger Team</author>
// <summary>Clears log and results from all dataGridViews</summary>
//-----------------------------------------------------------------------

namespace GlobalNamespace
{
	using System;
	using System.Windows.Forms;

	public partial class Form1 : Form
	{
		private void MenuItemClick_ClearTables(object sender, EventArgs e)
		{
			TokenSource.Cancel();
			albumHits.Clear();
			ActiveDGV.Refresh();

			this.rtbSearchLog.Clear();
			this.rtbWriteLog.Clear();
			this.rtbErrorLog.Clear();

			this.btnCancel.Visible = false;
			this.dataGridView1.Rows.Clear();
			this.dataGridView2.Rows.Clear();
		}
	}
}
