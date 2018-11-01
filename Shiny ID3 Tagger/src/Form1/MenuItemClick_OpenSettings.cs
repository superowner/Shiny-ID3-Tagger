//-----------------------------------------------------------------------
// <copyright file="MenuItemClick_OpenSettings.cs" company="Shiny ID3 Tagger">
// Copyright (c) Shiny ID3 Tagger. All rights reserved.
// </copyright>
// <author>ShinyId3Tagger Team</author>
// <summary>Open settings.json file</summary>
//-----------------------------------------------------------------------

using Shiny_ID3_Tagger.Setting;

namespace GlobalNamespace
{
	using System;
	using System.Diagnostics;
	using System.Windows.Forms;

	public partial class Form1 : Form
	{
		private void MenuItemClick_OpenSettings(object sender, EventArgs e)
		{
            if (this.setting.GetValue("ShowUISetting") is bool && (bool) this.setting.GetValue("ShowUISetting"))
		    {
                Setting setting = new Setting();
                setting.Show();
		    }
		    else
		    {
		        string file = AppDomain.CurrentDomain.BaseDirectory + @"\config\settings.json";
		        Process.Start(file);
            }
		}
	}
}
