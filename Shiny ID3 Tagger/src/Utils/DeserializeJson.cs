//-----------------------------------------------------------------------
// <copyright file="DeserializeJson.cs" company="Shiny ID3 Tagger">
// Copyright (c) Shiny ID3 Tagger. All rights reserved.
// </copyright>
// <author>ShinyId3Tagger Team</author>
// <summary>
// Deserialize a JSON string into an object.
// Also uses some non-default settings for all JSON deserializations. This is helpful for debugging invalid JSON strings
// </summary>
//-----------------------------------------------------------------------

namespace Utils
{
	using GlobalNamespace;
	using GlobalVariables;
	using Newtonsoft.Json;
	using Newtonsoft.Json.Linq;

	internal partial class Utils
	{
		internal static JObject DeserializeJson(string content)
		{
			JObject data = null;

			if (content != null)
			{
				// Set custom settings for JSON parser
				JsonSerializerSettings jsonSettings = new JsonSerializerSettings { Formatting = Formatting.Indented };

				// Build a useful error message if JSON parsing fails
				if ((int)User.Settings["DebugLevel"] >= 2)
				{
					jsonSettings.Error += (obj, errorArgs) =>
					{
						string[] errorMsg =
						{
							"WARNING:  Could not convert response to JSON!",
							"Message:  " + errorArgs.ErrorContext.Error.Message.TrimEnd('\r', '\n')
						};
						Form1.Instance.RichTextBox_PrintErrorMessage(errorMsg);
					};
				}

				data = JsonConvert.DeserializeObject<JObject>(content, jsonSettings);
			}

			return data;
		}
	}
}
