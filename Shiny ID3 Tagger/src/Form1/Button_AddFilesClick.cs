//-----------------------------------------------------------------------
// <copyright file="Button_AddFilesClick.cs" company="Shiny ID3 Tagger">
// Copyright (c) Shiny ID3 Tagger. All rights reserved.
// </copyright>
// <author>ShinyId3Tagger Team</author>
//-----------------------------------------------------------------------

namespace Shiny_ID3_Tagger
{
	using System;
	using System.Threading;
	using System.Windows.Forms;
	using GlobalVariables;

	/// <summary>
	/// Represents the Form1 class which contains all methods who interacts with the UI
	/// </summary>
	public partial class Form1 : Form
	{
		/// <summary>
		/// Opens a file selection window when pressing "Add files" button
		/// </summary>
		/// <param name="sender">The object which has raised the event</param>
		/// <param name="e">Contains additional information about the event</param>
		private async void Button_AddFilesClick(object sender, EventArgs e)
		{
			// Refresh cancel token
			GlobalVariables.TokenSource = new CancellationTokenSource();
			CancellationToken cancelToken = GlobalVariables.TokenSource.Token;

			// Add new files
			bool hasNewFiles = await this.CollectFiles(null, cancelToken);

			// Continue with searching if user setting allows it and if new files were added
			if ((bool)User.Settings["AutoSearch"] && hasNewFiles)
			{
				this.StartSearching(cancelToken);
			}
		}
	}
}
