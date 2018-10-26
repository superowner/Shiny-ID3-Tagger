//-----------------------------------------------------------------------
// <copyright file="IncreaseAlbumCounter.cs" company="Shiny ID3 Tagger">
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
		/// Checks if current API has returned an album which is also the majority album from all other APIs
		/// If yes, then increase the corresponding entry in GlobalVariables.AlbumHits list
		/// </summary>
		/// <param name="service">name of current API</param>
		/// <param name="apiAlbum">name of album to check</param>
		/// <param name="majorityAlbum">name of most often returned album name by all other APIs</param>
		/// <returns>The updated value of albumHits for the specified API</returns>
		internal static string IncreaseAlbumCounter(string service, string apiAlbum, string majorityAlbum)
		{
			if (!GlobalVariables.AlbumHits.ContainsKey(service))
			{
				GlobalVariables.AlbumHits.Add(service, 0);
			}

			if (apiAlbum != null && majorityAlbum != null)
			{
				if (Strip(apiAlbum.ToLowerInvariant()) == Strip(majorityAlbum.ToLowerInvariant()))
				{
					GlobalVariables.AlbumHits[service] += 1;
				}
			}

			string result = GlobalVariables.AlbumHits[service].ToString();

			return result;
		}
	}
}
