//-----------------------------------------------------------------------
// <copyright file="OnDataGridViewSortCompare.cs" company="Shiny Id3 Tagger">
//	 Copyright (c) Shiny Id3 Tagger. All rights reserved.
// </copyright>
// <author>ShinyId3Tagger Team</author>
// <summary>Custom sort rules when sorting datagridview columns. tries to convert string to dates or integers</summary>
//-----------------------------------------------------------------------

namespace GlobalNamespace
{
	using System;
	using System.Linq;
	using System.Windows.Forms;

	public partial class Form1 : Form
	{
		private void OnDataGridViewSortCompare(object sender, DataGridViewSortCompareEventArgs e)
		{
			
			// If both values are the same, or both values are null/DBNull
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
			
			// If both values can be converted to decimal
			decimal outValue1;
			decimal outValue2;
			if (decimal.TryParse((string)e.CellValue1, out outValue1) && decimal.TryParse((string)e.CellValue2, out outValue2))
			{
				e.SortResult = decimal.Compare(outValue1, outValue2);
				e.Handled = true;
				return;
			}
			
			e.SortResult = ((IComparable)e.CellValue1).CompareTo(e.CellValue2);
			e.Handled = true;			
			return;
		}		
		
		
		
//		private void OnDataGridViewSortCompare(object sender, DataGridViewSortCompareEventArgs e)
//		{
//			string value1 = e.CellValue1 != null ? e.CellValue1.ToString() : string.Empty;
//			string value2 = e.CellValue2 != null ? e.CellValue2.ToString() : string.Empty;
//
//			switch (e.Column.HeaderText)
//			{
//				case "#":
//				case "Album Hits":
//				case "Disc Count":
//				case "Disc Number":
//				case "Track Count":
//				case "Track Number":
//					uint outValue1;
//					uint outValue2;
//					bool result1 = uint.TryParse(value1, out outValue1);
//					bool result2 = uint.TryParse(value2, out outValue2);
//					if (result1 && result2)
//					{
//						e.SortResult = decimal.Compare(outValue1, outValue2);
//						e.SortResult = value1.CompareTo(value2, cultEng);
//					}
//
//					break;
//				case "Date":
//					DateTime outDate1 = !string.IsNullOrEmpty(value1) ? this.ConvertStringToDate(value1) : default(DateTime);
//					DateTime outDate2 = !string.IsNullOrEmpty(value2) ? this.ConvertStringToDate(value2) : default(DateTime);
//					e.SortResult = DateTime.Compare(outDate1, outDate2);
//					break;
//				default:
//					e.SortResult = string.Compare(value1, value2, StringComparison.Ordinal);
//					break;
//			}
//
//			e.Handled = true;
//		}
	}
}