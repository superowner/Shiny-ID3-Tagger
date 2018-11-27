//-----------------------------------------------------------------------
// <copyright file="IncreaseAlbumCounter.cs" company="Shiny ID3 Tagger">
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
		/// Increase the albumCounter for a given service,
		/// if current API request has returned an album which is also the majority album from all other APIs
		/// </summary>
		/// <param name="service">name of current API service</param>
		/// <param name="apiAlbum">name of album of current API service which will be checked</param>
		/// <param name="majorityAlbum">name of most often named album name from all other APIs</param>
		internal static void IncreaseAlbumCounter(string service, string apiAlbum, string majorityAlbum)
		{
			// Prevents exception "ArgumentNullException"
			if (service == null)
			{
				return;
			}

			// Add new entry to list (Initilize)
			if (GlobalVariables.AlbumCounter.ContainsKey(service) == false)
			{
				GlobalVariables.AlbumCounter.Add(service, 0);
			}

			// Prevents exception "ArgumentNullException"
			if (apiAlbum == null || majorityAlbum == null)
			{
				return;
			}

			// Increase counter by 1
			if (Strip(apiAlbum.ToLowerInvariant()) == Strip(majorityAlbum.ToLowerInvariant()))
			{
				GlobalVariables.AlbumCounter[service] += 1;
			}
		}
	}
}
