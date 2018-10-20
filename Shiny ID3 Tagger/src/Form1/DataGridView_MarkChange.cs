//-----------------------------------------------------------------------
// <copyright file="DataGridView_MarkChange.cs" company="Shiny ID3 Tagger">
// Copyright (c) Shiny ID3 Tagger. All rights reserved.
// </copyright>
// <author>ShinyId3Tagger Team</author>
// <summary>Put green (new value), yellow (minor change)
// or red (big change) as cell background color in dataGridView1</summary>
//-----------------------------------------------------------------------

namespace Shiny_ID3_Tagger
{
	using System;
	using System.Drawing;
	using GlobalVariables;
	using Utils;

	public partial class Form1
	{
		internal void DataGridView_MarkChange(int row, int col, string oldValue, string newValue, bool signalBigChanges = false)
		{
			if (!string.IsNullOrWhiteSpace(newValue) && oldValue != newValue)
			{
				try
				{
					this.dataGridView1[col, row].Value = newValue;

					if (string.IsNullOrWhiteSpace(oldValue))
					{
						this.dataGridView1[col, row].Style.BackColor = Color.LightGreen;
						this.dataGridView1[this.hasNewValues.Index, row].Value = true;
					}
					else
					{
						this.dataGridView1[col, row].ToolTipText = oldValue;

						long allowedEdits = oldValue.Length * (int)User.Settings["ThresholdRedValue"] / 100;
						if (signalBigChanges && (Utils.LevenshteinDistance(oldValue, newValue) > allowedEdits))
						{
							this.dataGridView1[col, row].Style.BackColor = Color.Red;
							this.dataGridView1[this.hasNewValues.Index, row].Value = true;
						}
						else
						{
							this.dataGridView1[col, row].Style.BackColor = Color.Yellow;
							this.dataGridView1[this.hasNewValues.Index, row].Value = true;
						}
					}
				}
				catch (ArgumentOutOfRangeException)
				{
					// If user cleares all (or current) row while running this method an ArgumentOutOfRangeException would occur. Catch error silently
				}
			}
		}
	}
}
