//-----------------------------------------------------------------------
// <copyright file="MenuItemClick_AddFolder.cs" company="Shiny ID3 Tagger">
// Copyright (c) Shiny ID3 Tagger. All rights reserved.
// </copyright>
// <author>ShinyId3Tagger Team</author>
// <summary>Opens a Ookii folder selection window when using menu item "Add directory"</summary>
//-----------------------------------------------------------------------

namespace GlobalNamespace
{
	using System;
	using System.Threading;
	using System.Windows.Forms;
	using Ookii.Dialogs;

	public partial class Form1 : Form
	{
		private async void MenuItemClick_AddFolder(object sender, EventArgs e)
		{
			// Refresh cancel token
			TokenSource = new CancellationTokenSource();
			CancellationToken cancelToken = TokenSource.Token;

			if (LastUsedFolder == null)
			{
				LastUsedFolder = Environment.GetFolderPath(Environment.SpecialFolder.MyMusic) + @"\";
			}

			// Ookii dialog (3rd party library) looks more like a normal file selection dialog. (windows forms default folder dialog looks ugly)
			using (VistaFolderBrowserDialog dialog = new VistaFolderBrowserDialog()
			{
				SelectedPath = LastUsedFolder
			})
			{
				if (dialog.ShowDialog() == DialogResult.OK)
				{
					LastUsedFolder = dialog.SelectedPath;
					string[] folderpath = { dialog.SelectedPath };

					// Add new files
					bool hasNewFiles = await this.AddFiles(folderpath, cancelToken);

					// Continue with searching if user setting allows it and if new files were added (new row count != old row count)
					if (User.Settings["AutoSearch"] && hasNewFiles)
					{
						this.StartSearching(cancelToken);
					}
				}
			}
		}
	}
}
