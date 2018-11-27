//-----------------------------------------------------------------------
// <copyright file="Form_EnableUI.cs" company="Shiny ID3 Tagger">
// Copyright (c) Shiny ID3 Tagger. All rights reserved.
// </copyright>
// <author>ShinyId3Tagger Team</author>
//-----------------------------------------------------------------------

namespace Shiny_ID3_Tagger
{
	/// <summary>
	/// Represents the MainForm class which contains all methods who interacts with the UI
	/// </summary>
	public partial class MainForm
	{
		/// <summary>
		/// Enables or disables UI elements which could potentially interrupt add/search/write methods
		/// </summary>
		/// <param name="enabled">Boolean to indicate if UI elements like menu bar or buttons should be active (clickable) or not</param>
		private void Form_EnableUI(bool enabled)
		{
			if (enabled)
			{
				this.btnAddFiles.Enabled = true;
				this.btnWrite.Enabled = true;
				this.btnSearch.Enabled = true;
				this.menuStrip1.Enabled = true;
			}
			else
			{
				this.btnAddFiles.Enabled = false;
				this.btnWrite.Enabled = false;
				this.btnSearch.Enabled = false;
				this.menuStrip1.Enabled = false;
			}
		}
	}
}
