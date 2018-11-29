//-----------------------------------------------------------------------
// <copyright file="DataGridView_SortCompare.cs" company="Shiny ID3 Tagger">
// Copyright (c) Shiny ID3 Tagger. All rights reserved.
// </copyright>
// <author>ShinyId3Tagger Team</author>
//-----------------------------------------------------------------------

namespace Shiny_ID3_Tagger
{
	using System;
	using System.Globalization;
	using System.Windows.Forms;
	using GlobalVariables;
	using Utils;

	/// <summary>
	/// Represents the MainForm class which contains all methods who interacts with the UI
	/// </summary>
	public partial class MainForm : Form
	{
		/// <summary>
		/// Custom sort rules when sorting datagridview columns. tries to convert string to dates or integers
		/// <seealso href="https://social.msdn.microsoft.com/Forums/windows/en-US/43a85553-2b94-4f4e-9db3-498311af4ecd/datagridview-sorting-with-null-values?forum=winforms"/>
		/// </summary>
		/// <param name="sender">The object which has raised the event</param>
		/// <param name="e">Contains additional information about the event</param>
		private void DataGridView_SortCompare(object sender, DataGridViewSortCompareEventArgs e)
		{
			// If both values are equal, or both values are null/DBNull. Compare case sensitive
			if (e.CellValue1 == e.CellValue2)
			{
				e.SortResult = 0;
				e.Handled = true;
				return;
			}

			// If first value is null/DBNull
			if (e.CellValue1 == null || e.CellValue1 == DBNull.Value)
			{
				e.SortResult = -1;
				e.Handled = true;
				return;
			}

			// If second value is null/DBNull
			if (e.CellValue2 == null || e.CellValue2 == DBNull.Value)
			{
				e.SortResult = 1;
				e.Handled = true;
				return;
			}

			// If both values can be converted to dateTime
			if (DateTime.TryParseExact(e.CellValue1.ToString(), Utils.DateTimeformats, GlobalVariables.CultEng, DateTimeStyles.None, out DateTime outDate1) &&
				DateTime.TryParseExact(e.CellValue2.ToString(), Utils.DateTimeformats, GlobalVariables.CultEng, DateTimeStyles.None, out DateTime outDate2))
			{
				e.SortResult = DateTime.Compare(outDate1, outDate2);
				e.Handled = true;
				return;
			}

			// If both values can be converted to decimal
			if (decimal.TryParse(e.CellValue1.ToString(), out decimal outDecimal1) &&
				decimal.TryParse(e.CellValue2.ToString(), out decimal outDecimal2))
			{
				e.SortResult = decimal.Compare(outDecimal1, outDecimal2);
				e.Handled = true;
				return;
			}

			// Default comparison, simple string comparison
			e.SortResult = ((IComparable)e.CellValue1.ToString()).CompareTo(e.CellValue2.ToString());
			e.Handled = true;
			return;
		}
	}
}
