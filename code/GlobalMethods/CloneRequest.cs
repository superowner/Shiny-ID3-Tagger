//-----------------------------------------------------------------------
// <copyright file="CloneRequest.cs" company="Shiny Id3 Tagger">
//	 Copyright (c) Shiny Id3 Tagger. All rights reserved.
// </copyright>
// <author>ShinyId3Tagger Team</author>
// <summary>Backups a HTTP request message before the reuqest is fired. Otherwise you couldn't do retries after an unsuccessful try</summary>
//-----------------------------------------------------------------------

namespace GlobalNamespace
{
	using System;
	using System.Collections.Generic;
	using System.Linq;
	using System.Net.Http;

	public partial class Form1
	{
		private static HttpRequestMessage CloneRequest(HttpRequestMessage original)
		{
			HttpRequestMessage backup = new HttpRequestMessage(original.Method, original.RequestUri);

			backup.Content = original.Content;
			backup.Version = original.Version;

			foreach (KeyValuePair<string, object> property in original.Properties)
			{
				backup.Properties.Add(property);
			}

			foreach (KeyValuePair<string, IEnumerable<string>> header in original.Headers)
			{
				backup.Headers.TryAddWithoutValidation(header.Key, header.Value);
			}

			return backup;
		}
	}
}
