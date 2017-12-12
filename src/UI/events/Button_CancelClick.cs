//-----------------------------------------------------------------------
// <copyright file="Button_CancelClick.cs" company="Shiny Id3 Tagger">
// Copyright (c) Shiny Id3 Tagger. All rights reserved.
// </copyright>
// <author>ShinyId3Tagger Team</author>
// <summary>Uses the main token source to signalize all running tasks to cancel their work and return to main method</summary>
//-----------------------------------------------------------------------

namespace GlobalNamespace
{
	using System;
	using System.Windows.Forms;

	public partial class Form1 : Form
	{
		private void Button_CancelClick(object sender, EventArgs e)
		{
			TokenSource.Cancel();
			this.btnCancel.Visible = false;
		}
	}
}