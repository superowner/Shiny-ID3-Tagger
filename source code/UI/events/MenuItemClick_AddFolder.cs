//-----------------------------------------------------------------------
// <copyright file="MenuItemClick_AddFolder.cs" company="Shiny Id3 Tagger">
//	 Copyright (c) Shiny Id3 Tagger. All rights reserved.
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
					bool newFiles = await this.AddFiles(folderpath, cancelToken);

					// If user setting allows it and new files were added (dialog not canceled or files were already added), continue straight with searching
					if (User.Settings["AutoSearch"] && newFiles && !cancelToken.IsCancellationRequested)
					{
						this.StartSearching(cancelToken);
					}
				}
			}
		}
	}
}