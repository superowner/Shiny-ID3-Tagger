//-----------------------------------------------------------------------
// <copyright file="Button_WriteClick.cs" company="Shiny Id3 Tagger">
//	 Copyright (c) Shiny Id3 Tagger. All rights reserved.
// </copyright>
// <author>ShinyId3Tagger Team</author>
// <summary>Starts main routine "WriteTags" for writing all results to files</summary>
//-----------------------------------------------------------------------

namespace GlobalNamespace
{
	using System;
	using System.Windows.Forms;

	public partial class Form1 : Form
	{
		private void Button_WriteClick(object sender, EventArgs e)
		{
			this.StartWriting();
		}
	}
}