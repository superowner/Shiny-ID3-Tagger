//-----------------------------------------------------------------------
// <copyright file="BtnAddFilesClick.cs" company="Shiny Id3 Tagger">
//	 Copyright (c) Shiny Id3 Tagger. All rights reserved.
// </copyright>
// <author>ShinyId3Tagger Team</author>
// <summary>Opens the file selection window when pressing "Add files" button</summary>
//-----------------------------------------------------------------------

namespace GlobalNamespace
{
	using System;
	using System.Linq;
	using System.Windows.Forms;

	public partial class Form1 : Form
	{
		private void BtnAddFilesClick(object sender, EventArgs e)
		{
			this.AddFiles(null);
		}
	}
}