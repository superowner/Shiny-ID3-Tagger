//-----------------------------------------------------------------------
// <copyright file="IncreaseTotalDuration.cs" company="Shiny ID3 Tagger">
// Copyright (c) Shiny ID3 Tagger. All rights reserved.
// </copyright>
// <author>ShinyId3Tagger Team</author>
//-----------------------------------------------------------------------

namespace Utils
{
	using GlobalVariables;

	/// <summary>
	/// Represents the Utility class which holds various helper functions
	/// </summary>
	internal partial class Utils
	{
		/// <summary>
		/// Increases the total duration list after an API task finished its job
		/// </summary>
		/// <param name="service">name of current API for which the total duration should be increased</param>
		/// <param name="duration">duration to add</param>
		/// <returns>The updated value of the total duration for the specified API</returns>
		internal static string IncreaseTotalDuration(string service, string duration)
		{
			decimal.TryParse(duration, out decimal durationAsDecimal);

			if (!GlobalVariables.TotalDuration.ContainsKey(service))
			{
				GlobalVariables.TotalDuration.Add(service, 0);
			}

			GlobalVariables.TotalDuration[service] = GlobalVariables.TotalDuration[service] + durationAsDecimal;

			string result = GlobalVariables.TotalDuration[service].ToString();

			return result;
		}
	}
}
