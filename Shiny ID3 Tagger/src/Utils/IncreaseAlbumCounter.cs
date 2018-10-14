﻿//-----------------------------------------------------------------------
// <copyright file="IncreaseAlbumCounter.cs" company="Shiny ID3 Tagger">
// Copyright (c) Shiny ID3 Tagger. All rights reserved.
// </copyright>
// <author>ShinyId3Tagger Team</author>
// <summary>Checks if current WEB API has returned an album which is also the majority album from all other APIs</summary>
//-----------------------------------------------------------------------

namespace Utils
{
	using GlobalVariables;

	internal partial class Utils
	{
		internal static string IncreaseAlbumCounter(string service, string apiAlbum, string majorityAlbum)
		{
			if (!GlobalVariables.AlbumHits.ContainsKey(service.ToString()))
			{
				GlobalVariables.AlbumHits.Add(service.ToString(), 0);
			}

			if (apiAlbum != null && majorityAlbum != null)
			{
				if (Strip(apiAlbum.ToString().ToLowerInvariant()) == Strip(majorityAlbum.ToLowerInvariant()))
				{
					GlobalVariables.AlbumHits[service.ToString()] += 1;
				}
			}

			string result = GlobalVariables.AlbumHits[service.ToString()].ToString().ToLowerInvariant();

			return result;
		}
	}
}
