//-----------------------------------------------------------------------
// <copyright file="Button_SearchClick.cs" company="Shiny ID3 Tagger">
// Copyright (c) Shiny ID3 Tagger. All rights reserved.
// </copyright>
// <author>ShinyId3Tagger Team</author>
// <summary>Starts main routine "SearchTags" for querying all APIs</summary>
//-----------------------------------------------------------------------

namespace GlobalNamespace
{
	using System;
	using System.Threading;
	using System.Windows.Forms;
	using GlobalVariables;

	public partial class Form1 : Form
	{
		private void Button_SearchClick(object sender, EventArgs e)
		{
			// Refresh cancel token which is used for all requests
			GlobalVariables.TokenSource = new CancellationTokenSource();
			CancellationToken cancelToken = GlobalVariables.TokenSource.Token;

			this.StartSearching(cancelToken);
		}
	}
}
