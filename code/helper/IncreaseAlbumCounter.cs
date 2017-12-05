//-----------------------------------------------------------------------
// <copyright file="IncreaseAlbumCounter.cs" company="Shiny Id3 Tagger">
//	 Copyright (c) Shiny Id3 Tagger. All rights reserved.
// </copyright>
// <author>ShinyId3Tagger Team</author>
// <summary>Checks if the current WEB API has returned an album which is also the majority album from all other APIs</summary>
//-----------------------------------------------------------------------

namespace GlobalNamespace
{
	using System;
	using System.Linq;

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
				if (Strip(apiAlbum.ToString().ToLower(cultEng)) == Strip(majorityAlbum.ToLower(cultEng)))
				{
					albumHits[service.ToString()] += 1;
				}
			}

			string result = albumHits[service.ToString()].ToString(cultEng);

			return result;
		}
	}
}