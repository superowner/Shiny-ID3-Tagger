//-----------------------------------------------------------------------
// <copyright file="Button_AddFilesClick.cs" company="Shiny Id3 Tagger">
//	 Copyright (c) Shiny Id3 Tagger. All rights reserved.
// </copyright>
// <author>ShinyId3Tagger Team</author>
// <summary>Opens a file selection window when pressing "Add files" button</summary>
//-----------------------------------------------------------------------

namespace GlobalNamespace
{
	using System;
	using System.Threading;
	using System.Windows.Forms;

	public partial class Form1 : Form
	{
		private async void Button_AddFilesClick(object sender, EventArgs e)
		{
			// Refresh cancel token
			TokenSource = new CancellationTokenSource();
			CancellationToken cancelToken = TokenSource.Token;

			// Add new files
			bool newFiles = await this.AddFiles(null, cancelToken);

			// If user setting allows it and new files were added (dialog not canceled or files were already added), continue straight with searching
			if (User.Settings["AutoSearch"] && newFiles && !cancelToken.IsCancellationRequested)
			{
				this.StartSearching(cancelToken);
			}
		}
	}
}