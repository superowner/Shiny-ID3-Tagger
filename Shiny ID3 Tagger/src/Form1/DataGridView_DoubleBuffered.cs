//-----------------------------------------------------------------------
// <copyright file="DataGridView_DoubleBuffered.cs" company="Shiny ID3 Tagger">
// Copyright (c) Shiny ID3 Tagger. All rights reserved.
// </copyright>
// <author>ShinyId3Tagger Team</author>
// <summary>Creates a new class based on dataGridView and enables double buffering for a smoother UI experience</summary>
//-----------------------------------------------------------------------

namespace Shiny_ID3_Tagger
{
	using System;
	using System.Globalization;
	using System.Windows.Forms;
	using GlobalVariables;
	using Utils;

	public class DataGridViewDoubleBuffered : DataGridView
	{
		public DataGridViewDoubleBuffered()
		{
			this.DoubleBuffered = true;
		}
	}
}
