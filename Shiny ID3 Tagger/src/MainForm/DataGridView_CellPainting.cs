//-----------------------------------------------------------------------
// <copyright file="DataGridView_CellPainting.cs" company="Shiny ID3 Tagger">
// Copyright (c) Shiny ID3 Tagger. All rights reserved.
// </copyright>
// <author>ShinyId3Tagger Team</author>
//-----------------------------------------------------------------------

namespace Shiny_ID3_Tagger
{
	using System.Drawing;
	using System.Windows.Forms;
	using GlobalVariables;

	/// <summary>
	/// Represents the MainForm class which contains all methods who interacts with the UI
	/// </summary>
	public partial class MainForm : Form
	{
		/// <summary>
		/// Draw light blue borders around active row
		/// <seealso href="https://stackoverflow.com/a/32170212/935614"/>
		/// </summary>
		/// <param name="sender">The object which has raised the event</param>
		/// <param name="e">Contains additional information about the event</param>
		private void DataGridView_CellPainting(object sender, DataGridViewCellPaintingEventArgs e)
		{
			// Skip column headers
			if (e.RowIndex >= 0)
			{
				// Pen with color for selected row borders
				using (Pen selectedPen = new Pen(Color.FromArgb(255, 153, 209, 255), 1))

					// Pen with grid line colors
				using (Pen gridlinePen = new Pen(Color.FromArgb(255, 227, 227, 227), 1))

					// Pen with background color
				using (Pen backGroundPen = new Pen(Color.FromArgb(255, 205, 232, 255), 1))
				{
					// Cell coordinates
					Point topLeftPoint = new Point(e.CellBounds.Left, e.CellBounds.Top);
					Point topRightPoint = new Point(e.CellBounds.Right - 1, e.CellBounds.Top);
					Point bottomRightPoint = new Point(e.CellBounds.Right - 1, e.CellBounds.Bottom - 1);
					Point bottomleftPoint = new Point(e.CellBounds.Left, e.CellBounds.Bottom - 1);

					// Draw active row
					if (GlobalVariables.ActiveDGV.CurrentRow != null &&
						e.RowIndex == GlobalVariables.ActiveDGV.CurrentRow.Index)
					{
						// Paint all parts except borders.
						e.Paint(e.ClipBounds, DataGridViewPaintParts.All & ~DataGridViewPaintParts.Border);

						// Hide left border through using background color
						e.Graphics.DrawLine(backGroundPen, topLeftPoint, bottomleftPoint);

						// Paint right border with grid color
						e.Graphics.DrawLine(gridlinePen, topRightPoint, bottomRightPoint);

						// Paint top border slightly darker as background color
						e.Graphics.DrawLine(selectedPen, topLeftPoint, topRightPoint);

						// Paint bottom border slightly darker as background color
						e.Graphics.DrawLine(selectedPen, bottomleftPoint, bottomRightPoint);

						// Handled painting for this cell, Stop default rendering
						e.Handled = true;
					}
				}
			}
		}
	}
}
