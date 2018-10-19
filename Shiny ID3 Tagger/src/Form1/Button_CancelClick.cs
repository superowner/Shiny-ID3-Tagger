//-----------------------------------------------------------------------
// <copyright file="Button_CancelClick.cs" company="Shiny ID3 Tagger">
// Copyright (c) Shiny ID3 Tagger. All rights reserved.
// </copyright>
// <author>ShinyId3Tagger Team</author>
// <summary>Uses the main token source to signalize all running tasks to cancel their work and return to main method</summary>
//-----------------------------------------------------------------------

namespace Shiny_ID3_Tagger
{
	using System;
	using System.Windows.Forms;
	using GlobalVariables;

	public partial class Form1 : Form
	{
		private void Button_CancelClick(object sender, EventArgs e)
		{
			GlobalVariables.TokenSource.Cancel();
			this.btnCancel.Visible = false;
		}
	}
}
