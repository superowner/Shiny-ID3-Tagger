//-----------------------------------------------------------------------
// <copyright file="IncreaseAlbumCounter.cs" company="Shiny ID3 Tagger">
// Copyright (c) Shiny ID3 Tagger. All rights reserved.
// </copyright>
// <author>ShinyId3Tagger Team</author>
// <summary>Checks if current WEB API has returned an album which is also the majority album from all other APIs</summary>
//-----------------------------------------------------------------------

namespace GlobalNamespace
{
	using System;

	public partial class Form1
	{
		private static string IncreaseAlbumCounter(string service, string apiAlbum, string majorityAlbum)
		{
			if (!albumHits.ContainsKey(service.ToString()))
			{
				albumHits.Add(service.ToString(), 0);
			}

			if (apiAlbum != null && majorityAlbum != null)
			{
				if (Strip(apiAlbum.ToString().ToLowerInvariant()) == Strip(majorityAlbum.ToLowerInvariant()))
				{
					albumHits[service.ToString()] += 1;
				}
			}

			string result = albumHits[service.ToString()].ToString().ToLowerInvariant();

			return result;
		}
	}
}