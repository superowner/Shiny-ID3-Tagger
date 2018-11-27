//-----------------------------------------------------------------------
// <copyright file="DataGridView_MarkChange.cs" company="Shiny ID3 Tagger">
// Copyright (c) Shiny ID3 Tagger. All rights reserved.
// </copyright>
// <author>ShinyId3Tagger Team</author>
//-----------------------------------------------------------------------

namespace Shiny_ID3_Tagger
{
	using System;
	using System.Drawing;
	using GlobalVariables;
	using Utils;

	/// <summary>
	/// Represents the Form1 class which contains all methods who interacts with the UI
	/// </summary>
	public partial class Form1
	{
		/// <summary>
		/// Compares two values and signals changes through background colors in dataGridView1
		/// If a change is detected, the "Save" checkbox is activated (true)
		/// Green = new value, Yellow = minor change, Red = big change
		/// </summary>
		/// <param name="row">Row to alter</param>
		/// <param name="col">Column to alter</param>
		/// <param name="oldValue">Old value</param>
		/// <param name="newValue">New value</param>
		/// <param name="signalBigChanges">True or false if major changes should be signalized with red</param>
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
						if (signalBigChanges && (Utils.CalcStringSimilarity(oldValue, newValue) > allowedEdits))
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
