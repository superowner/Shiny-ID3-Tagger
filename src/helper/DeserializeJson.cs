//-----------------------------------------------------------------------
// <copyright file="DeserializeJson.cs" company="Shiny Id3 Tagger">
//	 Copyright (c) Shiny Id3 Tagger. All rights reserved.
// </copyright>
// <author>ShinyId3Tagger Team</author>
// <summary>Sets non-default settings for all JSON deserializations. This is helpful for debugging invalid JSON strings</summary>
//-----------------------------------------------------------------------

namespace GlobalNamespace
{
	using System;
	using Newtonsoft.Json;
	using Newtonsoft.Json.Linq;

	public partial class Form1
	{
		private JObject DeserializeJson(string content)
		{
			JObject data = null;

			if (content != null)
			{
				// Set custom settings for JSON parser
				JsonSerializerSettings jsonSettings = new JsonSerializerSettings { Formatting = Formatting.Indented };

				// Build a useful error message if JSON parsing fails
				if (User.Settings["DebugLevel"] >= 2)
				{
					jsonSettings.Error += (obj, errorArgs) =>
					{
						string[] errorMsg =
						{
							"WARNING:  Could not convert response to JSON!",
							"Message:  " + errorArgs.ErrorContext.Error.Message.TrimEnd('\r', '\n')
						};
						this.PrintLogMessage("error", errorMsg);
					};
				}

				data = JsonConvert.DeserializeObject<JObject>(content, jsonSettings);
			}

			return data;
		}
	}
}
