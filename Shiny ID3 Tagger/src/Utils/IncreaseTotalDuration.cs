//-----------------------------------------------------------------------
// <copyright file="IncreaseTotalDuration.cs" company="Shiny ID3 Tagger">
// Copyright (c) Shiny ID3 Tagger. All rights reserved.
// </copyright>
// <author>ShinyId3Tagger Team</author>
//-----------------------------------------------------------------------
// Reviewed and checked if all possible exceptions are prevented or handled

namespace Utils
{
	using GlobalVariables;

	/// <summary>
	/// Represents the Utility class which holds various helper functions
	/// </summary>
	internal partial class Utils
	{
		/// <summary>
		/// Increases the total duration for a given API when the current search task finished
		/// </summary>
		/// <param name="service">name of current API for which the total duration should be increased</param>
		/// <param name="duration">duration to add</param>
		internal static void IncreaseTotalDuration(string service, string duration)
		{
			// Prevents exception "ArgumentNullException"
			if (service == null)
			{
				return;
			}

			// Add new entry to list (Initilize)
			if (GlobalVariables.TotalDuration.ContainsKey(service) == false)
			{
				GlobalVariables.TotalDuration.Add(service, 0);
			}

			// Prevents exception "ArgumentNullException"
			if (duration == null)
			{
				return;
			}

			if (decimal.TryParse(duration, out decimal durationAsDecimal))
			{
				GlobalVariables.TotalDuration[service] += durationAsDecimal;
			}
		}
	}
}
