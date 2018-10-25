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

	/// <summary>
	/// Represents the Utility class which holds various helper functions
	/// </summary>
	internal partial class Utils
	{
		internal static string IncreaseTotalDuration(string service, string duration)
		{
			decimal.TryParse(duration, out decimal durationAsDecimal);

			if (!GlobalVariables.TotalDuration.ContainsKey(service))
			{
				GlobalVariables.TotalDuration.Add(service.ToString(), 0);
			}

			GlobalVariables.TotalDuration[service] = GlobalVariables.TotalDuration[service] + durationAsDecimal;

			return GlobalVariables.TotalDuration[service].ToString(GlobalVariables.CultEng);
		}
	}
}
