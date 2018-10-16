﻿//-----------------------------------------------------------------------
// <copyright file="DataGridView_MarkChange.cs" company="Shiny ID3 Tagger">
// Copyright (c) Shiny ID3 Tagger. All rights reserved.
// </copyright>
// <author>ShinyId3Tagger Team</author>
// <summary>Inserts green, yellow or red as cell background color if a cell was changed</summary>
//-----------------------------------------------------------------------

namespace GlobalNamespace
{
	using System;
	using System.Drawing;
	using GlobalVariables;
	using Utils;

	public partial class Form1
	{
		internal void DataGridView_MarkChange(int row, int col, string oldValue, string newValue, bool signalBigChanges)
		{
			if (!string.IsNullOrWhiteSpace(newValue) && oldValue != newValue)
			{
				try
				{
					this.dataGridView1[col, row].Value = newValue;

					if (string.IsNullOrWhiteSpace(oldValue))
					{
						this.dataGridView1[col, row].Style.BackColor = Color.LightGreen;
					}
					else
					{
						this.dataGridView1[col, row].ToolTipText = oldValue;

						long allowedEdits = oldValue.Length * (int)User.Settings["ThresholdRedValue"] / 100;
						if (signalBigChanges && (Utils.LevenshteinDistance(oldValue, newValue) > allowedEdits))
						{
							this.dataGridView1[col, row].Style.BackColor = Color.Red;
						}
						else
						{
							this.dataGridView1[col, row].Style.BackColor = Color.Yellow;
						}
					}
				}
				catch (ArgumentOutOfRangeException)
				{
					// If user cleared all or current row an ArgumentOutOfRangeException would occur. Catch error and continue
				}
			}
		}
	}
}
