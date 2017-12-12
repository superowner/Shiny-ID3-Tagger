//-----------------------------------------------------------------------
// <copyright file="DataGridView_SortCompare.cs" company="Shiny ID3 Tagger">
// Copyright (c) Shiny ID3 Tagger. All rights reserved.
// </copyright>
// <author>ShinyId3Tagger Team</author>
// <summary>Custom sort rules when sorting datagridview columns. tries to convert string to dates or integers</summary>
//-----------------------------------------------------------------------

namespace GlobalNamespace
{
	using System;
	using System.Globalization;
	using System.Windows.Forms;

	public partial class Form1 : Form
	{
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
			if (DateTime.TryParseExact((string)e.CellValue1, dateTimeformats, cultEng, DateTimeStyles.None, out DateTime outDate1) &&
			    DateTime.TryParseExact((string)e.CellValue2, dateTimeformats, cultEng, DateTimeStyles.None, out DateTime outDate2))
			{
				e.SortResult = DateTime.Compare(outDate1, outDate2);
				e.Handled = true;
				return;
			}

			// If both values can be converted to decimal
			if (decimal.TryParse((string)e.CellValue1, out decimal outDecimal1) &&
				decimal.TryParse((string)e.CellValue2, out decimal outDecimal2))
			{
				e.SortResult = decimal.Compare(outDecimal1, outDecimal2);
				e.Handled = true;
				return;
			}

			// Default comparison, probably only string comparisons
			e.SortResult = ((IComparable)e.CellValue1).CompareTo(e.CellValue2);
			e.Handled = true;
			return;
		}
	}
}
