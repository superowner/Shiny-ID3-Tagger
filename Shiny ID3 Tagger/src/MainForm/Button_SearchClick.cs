//-----------------------------------------------------------------------
// <copyright file="Button_SearchClick.cs" company="Shiny ID3 Tagger">
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
	/// Represents the MainForm class which contains all methods who interacts with the UI
	/// </summary>
	public partial class MainForm : Form
	{
		/// <summary>
		/// Starts main routine "SearchTags" for querying all APIs
		/// </summary>
		/// <param name="sender">The object which has raised the event</param>
		/// <param name="e">Contains additional information about the event</param>
		private void Button_SearchClick(object sender, EventArgs e)
		{
			// Refresh cancel token which is used for all requests
			GlobalVariables.TokenSource = new CancellationTokenSource();
			CancellationToken cancelToken = GlobalVariables.TokenSource.Token;

			this.StartSearching(cancelToken);
		}
	}
}
