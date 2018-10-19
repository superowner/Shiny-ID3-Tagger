//-----------------------------------------------------------------------
// <copyright file="Button_AddFilesClick.cs" company="Shiny ID3 Tagger">
// Copyright (c) Shiny ID3 Tagger. All rights reserved.
// </copyright>
// <author>ShinyId3Tagger Team</author>
// <summary>Opens a file selection window when pressing "Add files" button</summary>
//-----------------------------------------------------------------------

namespace Shiny_ID3_Tagger
{
	using System;
	using System.Threading;
	using System.Windows.Forms;
	using GlobalVariables;

	public partial class Form1 : Form
	{
		private async void Button_AddFilesClick(object sender, EventArgs e)
		{
			// Refresh cancel token
			GlobalVariables.TokenSource = new CancellationTokenSource();
			CancellationToken cancelToken = GlobalVariables.TokenSource.Token;

			// Add new files
			bool hasNewFiles = await this.AddFiles(null, cancelToken);

			// Continue with searching if user setting allows it and if new files were added
			if ((bool)User.Settings["AutoSearch"] && hasNewFiles)
			{
				this.StartSearching(cancelToken);
			}
		}
	}
}
