//-----------------------------------------------------------------------
// <copyright file="IncreaseTotalDuration.cs" company="Shiny ID3 Tagger">
// Copyright (c) Shiny ID3 Tagger. All rights reserved.
// </copyright>
// <author>ShinyId3Tagger Team</author>
// <summary>Sums up duration per API/per file to total duration per API</summary>
//-----------------------------------------------------------------------

namespace GlobalNamespace
{
	using System;

	public partial class Form1
	{
		private static string IncreaseTotalDuration(string service, string duration)
		{
			decimal.TryParse(duration, out decimal durationAsDecimal);

			if (!totalDuration.ContainsKey(service))
			{
				totalDuration.Add(service.ToString(), 0);
			}

			totalDuration[service] = totalDuration[service] + durationAsDecimal;

			return totalDuration[service].ToString(cultEng);
		}
	}
}