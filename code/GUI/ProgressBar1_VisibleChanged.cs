//-----------------------------------------------------------------------
// <copyright file="ProgressBar1_VisibleChanged.cs" company="Shiny Id3 Tagger">
//	 Copyright (c) Shiny Id3 Tagger. All rights reserved.
// </copyright>
// <author>ShinyId3Tagger Team</author>
// <summary>Shows the cancel button as long as a progress bar is also visible. Auto hide the cancel button afterwards</summary>
//-----------------------------------------------------------------------

namespace GlobalNamespace
{
	using System;
	using System.Linq;
	using System.Windows.Forms;

	public partial class Form1 : Form
	{
		private void ProgressBar1_VisibleChanged(object sender, EventArgs e)
		{
			if (this.fastProgressBar.Visible)
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