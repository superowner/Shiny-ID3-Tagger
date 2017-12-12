//-----------------------------------------------------------------------
// <copyright file="RichTexBox_LinkClicked.cs" company="Shiny ID3 Tagger">
// Copyright (c) Shiny ID3 Tagger. All rights reserved.
// </copyright>
// <author>ShinyId3Tagger Team</author>
// <summary>Start default browser when clicking links in rich text boxes (search, write, error log)</summary>
//-----------------------------------------------------------------------

namespace GlobalNamespace
{
	using System;
	using System.Diagnostics;
	using System.Windows.Forms;

	public partial class Form1 : Form
	{
		private void RichTexBox_LinkClicked(object sender, LinkClickedEventArgs e)
		{
			Process.Start(e.LinkText);
		}
	}
}