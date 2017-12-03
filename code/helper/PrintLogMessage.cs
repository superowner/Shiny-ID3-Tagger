//-----------------------------------------------------------------------
// <copyright file="PrintLogMessage.cs" company="Shiny Id3 Tagger">
//	 Copyright (c) Shiny Id3 Tagger. All rights reserved.
// </copyright>
// <author>ShinyId3Tagger Team</author>
// <summary>Outputs a given log message to it's corresponding richtextbox</summary>
//-----------------------------------------------------------------------

namespace GlobalNamespace
{
	using System;
	using System.Linq;

	public partial class Form1
	{
		private void PrintLogMessage(string logtype, string[] values)
		{
			if (InvokeRequired)
			{
				Invoke(new Action<string, string[]>(this.PrintLogMessage), new object[] { logtype, values });
				return;
			}

			try
			{
				string message = DateTime.Now.ToString("HH:mm:ss.fff   ", cultEng) + string.Join(Environment.NewLine + "               ", values);

				switch (logtype)
				{
					case "search":
						this.rtbSearchLog.AppendText(message + Environment.NewLine);
						this.rtbSearchLog.ScrollToCaret();
						break;
					case "write":
						this.rtbWriteLog.AppendText(message + Environment.NewLine);
						this.rtbWriteLog.ScrollToCaret();
						this.tabControl2.SelectTab(1);
						break;
					case "error":
						this.rtbErrorLog.AppendText(message + Environment.NewLine);
						this.rtbErrorLog.ScrollToCaret();
						this.tabControl2.SelectTab(2);
						break;						
				}
			}
			catch (ObjectDisposedException)
			{
				// User closed window. Therefore richtextbox is alread disposed and not available for output. Nothing more to do here
			}
		}
	}
}
