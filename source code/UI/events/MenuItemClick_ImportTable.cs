//-----------------------------------------------------------------------
// <copyright file="MenuItemClick_ImportTable.cs" company="Shiny Id3 Tagger">
//	 Copyright (c) Shiny Id3 Tagger. All rights reserved.
// </copyright>
// <author>ShinyId3Tagger Team</author>
// <summary>Imports a CSV file</summary>
// https://stackoverflow.com/a/3508572/935614
//-----------------------------------------------------------------------

namespace GlobalNamespace
{
	using Microsoft.VisualBasic.FileIO;
	using System;
	using System.Windows.Forms;

	public partial class Form1 : Form
	{
		private void MenuItemClick_ImportTable(object sender, EventArgs e)
		{

			// How to check encoding?
			//using (TextFieldParser parser = new TextFieldParser(@"c:\temp\test.csv"))
			//{
			//	parser.TextFieldType = FieldType.Delimited;
			//	parser.SetDelimiters(",");
			//	while (!parser.EndOfData)
			//	{
			//		//Processing row
			//		string[] fields = parser.ReadFields();
			//		foreach (string field in fields)
			//		{
			//			//TODO: Process field
			//		}
			//	}
			//}
		}
	}
}
