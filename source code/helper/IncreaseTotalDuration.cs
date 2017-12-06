//-----------------------------------------------------------------------
// <copyright file="IncreaseTotalDuration.cs" company="Shiny Id3 Tagger">
//	 Copyright (c) Shiny Id3 Tagger. All rights reserved.
// </copyright>
// <author>ShinyId3Tagger Team</author>
// <summary>Sums up the duration per API/per file to a total duration per API</summary>
//-----------------------------------------------------------------------

namespace GlobalNamespace
{
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