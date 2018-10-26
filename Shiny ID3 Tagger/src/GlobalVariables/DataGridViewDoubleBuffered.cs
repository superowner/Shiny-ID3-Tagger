//-----------------------------------------------------------------------
// <copyright file="DataGridViewDoubleBuffered.cs" company="Shiny ID3 Tagger">
// Copyright (c) Shiny ID3 Tagger. All rights reserved.
// </copyright>
// <author>ShinyId3Tagger Team</author>
//-----------------------------------------------------------------------

namespace Shiny_ID3_Tagger
{
	using System.Windows.Forms;

	/// <summary>Represents a new class based on System.Windows.Forms.DataGridView</summary>
	public class DataGridViewDoubleBuffered : DataGridView
	{
		/// <summary>
		/// Initializes a new instance of the <see cref="DataGridViewDoubleBuffered"/> class
		/// which has double buffering enabled for a smoother UI experience
		/// How to speed up dataGridView rendering: https://10tec.com/articles/why-datagridview-slow.aspx
		/// </summary>
		public DataGridViewDoubleBuffered()
		{
			this.DoubleBuffered = true;
		}
	}
}
