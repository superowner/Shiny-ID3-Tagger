//-----------------------------------------------------------------------
// <copyright file="TabControl_SelectedIndexChanged.cs" company="Shiny ID3 Tagger">
// Copyright (c) Shiny ID3 Tagger. All rights reserved.
// </copyright>
// <author>ShinyId3Tagger Team</author>
// <summary>Helper to set variable "ActiveDGV" accordingly to the active tab (datagridview)</summary>
//-----------------------------------------------------------------------

namespace GlobalNamespace
{
	using System;
	using System.Windows.Forms;
	using GlobalVariables;

	public partial class Form1 : Form
	{
		private void TabControl_SelectedIndexChanged(object sender, EventArgs e)
		{
			switch (this.tabControl1.SelectedIndex)
			{
				case 0:
					GlobalVariables.ActiveDGV = this.dataGridView1;
					break;
				case 1:
					GlobalVariables.ActiveDGV = this.dataGridView2;
					break;
			}
		}
	}
}
