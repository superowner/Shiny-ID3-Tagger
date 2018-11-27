//-----------------------------------------------------------------------
// <copyright file="RichTextBox_LinkClicked.cs" company="Shiny ID3 Tagger">
// Copyright (c) Shiny ID3 Tagger. All rights reserved.
// </copyright>
// <author>ShinyId3Tagger Team</author>
//-----------------------------------------------------------------------

namespace Shiny_ID3_Tagger
{
	using System.Diagnostics;
	using System.Windows.Forms;

	/// <summary>
	/// Represents the MainForm class which contains all methods who interacts with the UI
	/// </summary>
	public partial class MainForm : Form
	{
		/// <summary>
		/// Start default browser when clicking links in rich text boxes (search, write, error log)
		/// </summary>
		/// <param name="sender">The object which has raised the event</param>
		/// <param name="e">Contains additional information about the event</param>
		private void RichTextBox_LinkClicked(object sender, LinkClickedEventArgs e)
		{
			Process.Start(e.LinkText);
		}
	}
}
