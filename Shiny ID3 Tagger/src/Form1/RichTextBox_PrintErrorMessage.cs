//-----------------------------------------------------------------------
// <copyright file="RichTextBox_PrintErrorMessage.cs" company="Shiny ID3 Tagger">
// Copyright (c) Shiny ID3 Tagger. All rights reserved.
// </copyright>
// <author>ShinyId3Tagger Team</author>
// <summary>Outputs a given log message to it's corresponding richTextBox</summary>
//-----------------------------------------------------------------------

namespace GlobalNamespace
{
	using System;
	using System.Drawing;
	using System.Linq;
	using System.Windows.Forms;
	using GlobalVariables;

	public partial class Form1
	{
		internal void RichTextBox_PrintErrorMessage(string[] values)
		{
			this.PrintLogMessage(this.rtbErrorLog, values);
		}

		internal void PrintLogMessage(RichTextBox richTextBox, string[] values)
		{
			// When called from a different thread then Form1, switch back to thread which owns Form1
			if (this.InvokeRequired)
			{
				this.Invoke(new Action<RichTextBox, string[]>(this.PrintLogMessage), new object[] { richTextBox, values });
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
				if (richTextBox.Name == "rtbErrorLog")
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
