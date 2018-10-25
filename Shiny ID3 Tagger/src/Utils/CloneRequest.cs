//-----------------------------------------------------------------------
// <copyright file="CloneRequest.cs" company="Shiny ID3 Tagger">
// Copyright (c) Shiny ID3 Tagger. All rights reserved.
// </copyright>
// <author>ShinyId3Tagger Team</author>
//-----------------------------------------------------------------------

namespace Utils
{
	using System.Net.Http;

	/// <summary>
	/// Represents the Utility class which holds various helper functions
	/// </summary>
	internal partial class Utils
	{
		/// <summary>
		/// Backup a HTTP request message before the request is fired. Otherwise you can't do retries after an unsuccessful try
		/// </summary>
		/// <param name="original">Original request object which should be duplicated</param>
		/// <returns>The exact copy of the original request</returns>
		internal static HttpRequestMessage CloneRequest(HttpRequestMessage original)
		{
			HttpRequestMessage backup = new HttpRequestMessage(original.Method, original.RequestUri)
			{
				Content = original.Content,
				Version = original.Version
			};

			foreach (var property in original.Properties)
			{
				backup.Properties.Add(property);
			}

			foreach (var header in original.Headers)
			{
				backup.Headers.TryAddWithoutValidation(header.Key, header.Value);
			}

			return backup;
		}
	}
}
