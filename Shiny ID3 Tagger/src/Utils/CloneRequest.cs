//-----------------------------------------------------------------------
// <copyright file="CloneRequest.cs" company="Shiny ID3 Tagger">
// Copyright (c) Shiny ID3 Tagger. All rights reserved.
// </copyright>
// <author>ShinyId3Tagger Team</author>
// <summary>Backup a HTTP request message before request is fired. Otherwise you can't do retries after an unsuccessful try</summary>
//-----------------------------------------------------------------------

namespace Utils
{
	using System.Net.Http;

	internal partial class Utils
	{
		internal static HttpRequestMessage CloneRequest(HttpRequestMessage original)
		{
			HttpRequestMessage backup = new HttpRequestMessage(original.Method, original.RequestUri);

			backup.Content = original.Content;
			backup.Version = original.Version;

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
