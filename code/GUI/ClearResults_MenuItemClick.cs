//-----------------------------------------------------------------------
// <copyright file="ClearResults_MenuItemClick.cs" company="Shiny Id3 Tagger">
//	 Copyright (c) Shiny Id3 Tagger. All rights reserved.
// </copyright>
// <author>ShinyId3Tagger Team</author>
// <summary>Clears log and results from all datagridviews</summary>
//-----------------------------------------------------------------------

namespace GlobalNamespace
{
	using System;
	using System.Linq;
	using System.Windows.Forms;

	public partial class Form1 : Form
	{
		private void ClearResults_MenuItemClick(object sender, EventArgs e)
		{
			TokenSource.Cancel();
			albumHits.Clear();
			ActiveDGV.Refresh();
			
			this.btnCancel.Visible = false;
			this.dataGridView1.Rows.Clear();
			this.dataGridView2.Rows.Clear();
		}
	}
}