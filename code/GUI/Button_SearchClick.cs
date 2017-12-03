//-----------------------------------------------------------------------
// <copyright file="Button_SearchClick.cs" company="Shiny Id3 Tagger">
//	 Copyright (c) Shiny Id3 Tagger. All rights reserved.
// </copyright>
// <author>ShinyId3Tagger Team</author>
// <summary>Starts the main routine "SearchTags" for querying all APIs</summary>
//-----------------------------------------------------------------------

namespace GlobalNamespace
{
	using System;
	using System.Linq;
	using System.Windows.Forms;

	public partial class Form1 : Form
	{
		private void Button_SearchClick(object sender, EventArgs e)
		{
			this.StartSearching();
		}
	}
}