﻿//-----------------------------------------------------------------------
// <copyright file="SlowProgressBar_VisibleChanged.cs" company="Shiny ID3 Tagger">
// Copyright (c) Shiny ID3 Tagger. All rights reserved.
// </copyright>
// <author>ShinyId3Tagger Team</author>
// <summary>Show cancel button as long as slow progress bar is visible. Hide cancel button if not</summary>
//-----------------------------------------------------------------------

namespace Shiny_ID3_Tagger
{
	using System;
	using System.Windows.Forms;

	/// <summary>
	/// Represents the Form1 class which contains all methods who interacts with the UI
	/// </summary>
	public partial class Form1 : Form
	{
		private void SlowProgressBar_VisibleChanged(object sender, EventArgs e)
		{
			if (this.slowProgressBar.Visible)
			{
				this.btnCancel.Visible = true;
			}
			else
			{
				this.btnCancel.Visible = false;
			}
		}
	}
}
