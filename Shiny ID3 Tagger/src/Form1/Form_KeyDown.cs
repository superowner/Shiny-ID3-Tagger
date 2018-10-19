//-----------------------------------------------------------------------
// <copyright file="Form_KeyDown.cs" company="Shiny ID3 Tagger">
// Copyright (c) Shiny ID3 Tagger. All rights reserved.
// </copyright>
// <author>ShinyId3Tagger Team</author>
// <summary>Cancels all running tasks when pressing ESC key</summary>
//-----------------------------------------------------------------------

namespace Shiny_ID3_Tagger
{
	using System.Windows.Forms;
	using GlobalVariables;

	public partial class Form1 : Form
	{
		private void Form_KeyDown(object sender, KeyEventArgs e)
		{
			if (e.KeyCode == Keys.Escape)
			{
				GlobalVariables.TokenSource.Cancel();
			}
		}
	}
}
