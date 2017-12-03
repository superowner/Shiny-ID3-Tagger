//-----------------------------------------------------------------------
// <copyright file="DataGridView_CellMouseEnter.cs" company="Shiny Id3 Tagger">
//	 Copyright (c) Shiny Id3 Tagger. All rights reserved.
// </copyright>
// <author>ShinyId3Tagger Team</author>
// <summary>Highlights currently selected row/rows</summary>
//-----------------------------------------------------------------------

namespace GlobalNamespace
{
	using System;
	using System.Drawing;
	using System.Linq;
	using System.Windows.Forms;

	public partial class Form1 : Form
	{
		private void DataGridView_CellMouseEnter(object sender, DataGridViewCellEventArgs e)
		{
			DataGridView dgv = (DataGridView)sender;
			if (e.RowIndex >= 0)
			{				
				dgv.Rows[e.RowIndex].DefaultCellStyle.BackColor = Color.FromArgb(229, 243, 255);
			}		
		}	
	}
}