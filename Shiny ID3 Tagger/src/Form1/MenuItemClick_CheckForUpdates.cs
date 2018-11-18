//-----------------------------------------------------------------------
// <copyright file="MenuItemClick_CheckForUpdates.cs" company="Shiny ID3 Tagger">
// Copyright (c) Shiny ID3 Tagger. All rights reserved.
// </copyright>
// <author>ShinyId3Tagger Team</author>
//-----------------------------------------------------------------------

namespace Shiny_ID3_Tagger
{
	using System;
	using System.Windows.Forms;
	using Utils;

	/// <summary>
	/// Represents the Form1 class which contains all methods who interacts with the UI
	/// </summary>
	public partial class Form1 : Form
	{
		/// <summary>
		/// Start method to update program
		/// </summary>
		/// <param name="sender">The object which has raised the event</param>
		/// <param name="e">Contains additional information about the event</param>
		private async void MenuItemClick_CheckForUpdates(object sender, EventArgs e)
		{
			bool successDownload = await CheckAndDownloadUpdate();
			if (successDownload)
			{
				// Wait for UpdateClient.exe to say it's ready to deploy new program files
				Utils.StartUpdateClient();
			}
		}
	}
}
