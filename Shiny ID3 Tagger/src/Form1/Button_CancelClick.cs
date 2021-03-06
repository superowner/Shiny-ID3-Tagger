﻿//-----------------------------------------------------------------------
// <copyright file="Button_CancelClick.cs" company="Shiny ID3 Tagger">
// Copyright (c) Shiny ID3 Tagger. All rights reserved.
// </copyright>
// <author>ShinyId3Tagger Team</author>
//-----------------------------------------------------------------------

namespace Shiny_ID3_Tagger
{
	using System;
	using System.Windows.Forms;
	using GlobalVariables;

	/// <summary>
	/// Represents the Form1 class which contains all methods who interacts with the UI
	/// </summary>
	public partial class Form1 : Form
	{
		/// <summary>
		/// Uses the main token source to signalize all running tasks to cancel their work and return to main method
		/// </summary>
		/// <param name="sender">The object which has raised the event</param>
		/// <param name="e">Contains additional information about the event</param>
		private void Button_CancelClick(object sender, EventArgs e)
		{
			GlobalVariables.TokenSource.Cancel();
			this.btnCancel.Visible = false;
		}
	}
}
