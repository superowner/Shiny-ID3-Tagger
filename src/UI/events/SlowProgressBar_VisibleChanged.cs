//-----------------------------------------------------------------------
// <copyright file="SlowProgressBar_VisibleChanged.cs" company="Shiny Id3 Tagger">
//	 Copyright (c) Shiny Id3 Tagger. All rights reserved.
// </copyright>
// <author>ShinyId3Tagger Team</author>
// <summary>Show cancel button as long as slow progress bar is visible. Hide cancel button if not</summary>
//-----------------------------------------------------------------------

namespace GlobalNamespace
{
	using System;
	using System.Windows.Forms;

	public partial class Form1 : Form
	{
		private void slowProgressBar_VisibleChanged(object sender, EventArgs e)
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