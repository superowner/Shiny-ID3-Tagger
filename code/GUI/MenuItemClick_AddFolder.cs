//-----------------------------------------------------------------------
// <copyright file="MenuItemClick_AddFolder.cs" company="Shiny Id3 Tagger">
//	 Copyright (c) Shiny Id3 Tagger. All rights reserved.
// </copyright>
// <author>ShinyId3Tagger Team</author>
// <summary>Opens the Ookii folder selection window when using the menu item "Add directory"</summary>
//-----------------------------------------------------------------------

namespace GlobalNamespace
{
	using System;
	using System.Linq;
	using System.Windows.Forms;
	using Ookii.Dialogs;

	public partial class Form1 : Form
	{
		private async void MenuItemClick_AddFolder(object sender, EventArgs e)
		{
			// Ookii dialog (3rd party library) looks more like the normal files selection dialog. (default dialog looks ugly)
			VistaFolderBrowserDialog fbd = new VistaFolderBrowserDialog();

			if (LastUsedFolder == null)
			{
				LastUsedFolder = Environment.GetFolderPath(Environment.SpecialFolder.MyMusic) + @"\";
			}

			fbd.SelectedPath = LastUsedFolder;

			if (fbd.ShowDialog() == DialogResult.OK)
			{
				LastUsedFolder = fbd.SelectedPath;
				string[] folderpath = { fbd.SelectedPath };
				
				// Add new files
				bool newFiles = await this.AddFiles(folderpath);
				
				// If the setting allows it and new files were added (dialog not canceled or files were already added), continue straight with searching
				if (User.Settings["AutoSearch"] && newFiles)
				{
					this.StartSearching();
				}
			}
		}
	}
}