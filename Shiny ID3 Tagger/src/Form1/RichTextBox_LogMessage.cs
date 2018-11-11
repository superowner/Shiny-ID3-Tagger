//-----------------------------------------------------------------------
// <copyright file="RichTextBox_LogMessage.cs" company="Shiny ID3 Tagger">
// Copyright (c) Shiny ID3 Tagger. All rights reserved.
// </copyright>
// <author>ShinyId3Tagger Team</author>
//-----------------------------------------------------------------------

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
		/// <param name="messageLevel">Debug level or severity of the message
		/// 1: General (searching, writing, spelling mistake and so on)
		/// 2: General + Error (API timeout, Config not readable, JSON malformed and so on)
		/// 3: General + Error + Warning (all outgoing API requests, all incoming API responses)
		/// 4: General + Error + Warning + Debug (all outgoing API requests, all incoming API responses)
		/// </param>
		/// <param name="messageType">richTextBox to use. Possible values are "Search", "Write" or "Error" (default)</param>
		internal void RichTextBox_LogMessage(string[] values, int messageLevel, string messageType = default)
		{
			// When called from a different thread then Form1, switch back to thread which owns Form1
			if (this.InvokeRequired)
			{
				this.Invoke(new Action<string[], int, string>(Form1.Instance.RichTextBox_LogMessage), new object[] { values, messageLevel, messageType });
				return;
			}

			// If user settings LogLevel is not high enough for the current message, then do nothing and return
			if (User.Settings != null && (int)User.Settings["LogLevel"] < messageLevel)
			{
				return;
			}

			// Set correct richTextBox to use. Use rtbErrorLog as default fallback
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
					richTextBox = this.rtbErrorLog;
					break;
				default:
					richTextBox = this.rtbErrorLog;
					break;
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
