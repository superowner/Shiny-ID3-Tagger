//-----------------------------------------------------------------------
// <copyright file="RichTextBox_LogMessage.cs" company="Shiny ID3 Tagger">
// Copyright (c) Shiny ID3 Tagger. All rights reserved.
// </copyright>
// <author>ShinyId3Tagger Team</author>
//-----------------------------------------------------------------------
// TODO: Move debugLevel query inside this method

namespace Shiny_ID3_Tagger
{
	using System;
	using System.Drawing;
	using System.Linq;
	using System.Windows.Forms;
	using GlobalVariables;

	/// <summary>
	/// Represents the Form1 class which contains all methods who interacts with the UI
	/// </summary>
	public partial class Form1
	{
		/// <summary>
		/// Writes a given list of messages to the desired richTextBox
		/// </summary>
		/// <param name="values">List of messages to log</param>
		/// <param name="messageType">richTextBox to use. Possible values are "Search", "Write" or "Error" (default)</param>
		internal void RichTextBox_LogMessage(string[] values, string messageType = "Error")
		{
			// Set correct richTextBox to use
			RichTextBox richTextBox = null;
			switch (messageType)
			{
				case "Search":
					richTextBox = this.rtbSearchLog;
					break;
				case "Write":
					richTextBox = this.rtbWriteLog;
					break;
				case "Error":
				default:
					richTextBox = this.rtbErrorLog;
					break;
			}

			// When called from a different thread then Form1, switch back to thread which owns Form1
			if (this.InvokeRequired)
			{
				this.Invoke(new Action<string[], string>(this.RichTextBox_LogMessage), new object[] { richTextBox, values });
				return;
			}

			// If one value is a multi line string (i.e. HTML body), then split and rejoin each line with enough whitespace to align them
			values = values.Select(x => string.Join(Environment.NewLine + "               ", x.Split('\n'))).ToArray();

			// Join all lines into one string, add newline and whitespace to align them
			string message = string.Join(Environment.NewLine + "               ", values);

			try
			{
				richTextBox.SelectionColor = Color.Gray;
				richTextBox.AppendText(DateTime.Now.ToString("HH:mm:ss.fff   ", GlobalVariables.CultEng));

				richTextBox.SelectionColor = Color.Black;
				richTextBox.AppendText(message + Environment.NewLine);

				// Switch to error tab if it's an error message
				if (richTextBox == this.rtbErrorLog)
				{
					this.tabControl2.SelectedIndex = 2;
				}

				richTextBox.ScrollToCaret();
			}
				catch (ObjectDisposedException)
			{
				// User closed window. Therefore richTextBox is already disposed and not available for output. Nothing more to do here
			}
		}
	}
}
