//-----------------------------------------------------------------------
// <copyright file="IncreaseTotalDuration.cs" company="Shiny ID3 Tagger">
// Copyright (c) Shiny ID3 Tagger. All rights reserved.
// </copyright>
// <author>ShinyId3Tagger Team</author>
// <summary>Sums up duration per API/per file to total duration per API</summary>
//-----------------------------------------------------------------------

namespace Utils
{
	using GlobalVariables;

	public partial class Utils
	{
		public static string IncreaseTotalDuration(string service, string duration)
		{
			decimal.TryParse(duration, out decimal durationAsDecimal);

			if (!GlobalVariables.totalDuration.ContainsKey(service))
			{
				GlobalVariables.totalDuration.Add(service.ToString(), 0);
			}

			GlobalVariables.totalDuration[service] = GlobalVariables.totalDuration[service] + durationAsDecimal;

			return GlobalVariables.totalDuration[service].ToString(GlobalVariables.cultEng);
		}
	}
}
