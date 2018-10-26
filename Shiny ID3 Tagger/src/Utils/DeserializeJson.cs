//-----------------------------------------------------------------------
// <copyright file="DeserializeJson.cs" company="Shiny ID3 Tagger">
// Copyright (c) Shiny ID3 Tagger. All rights reserved.
// </copyright>
// <author>ShinyId3Tagger Team</author>
//-----------------------------------------------------------------------

namespace Utils
{
	using GlobalVariables;
	using Newtonsoft.Json;
	using Newtonsoft.Json.Linq;
	using Shiny_ID3_Tagger;

	/// <summary>Represents the Utility class which holds various helper functions</summary>
	internal partial class Utils
	{
		/// <summary> Deserialize a JSON string to a JSON.NET JObject </summary>
		/// <param name="jsonStr">The input string which should be deserialized</param>
		/// <returns>The new JObject holding all the values from content string</returns>
		internal static JObject DeserializeJson(string jsonStr)
		{
			JObject jsonObj = null;

			int debugLevel = 0;
			if (User.Settings != null)
			{
				debugLevel = (int)User.Settings["DebugLevel"];
			}

			if (jsonStr != null)
			{
				// Set custom settings for JSON parser
				JsonSerializerSettings jsonSettings = new JsonSerializerSettings { Formatting = Formatting.Indented };

				// Build a useful error message if JSON parsing fails
				if ((int)debugLevel >= 2)
				{
					jsonSettings.Error += (obj, errorArgs) =>
					{
						string[] errorMsg =
						{
							"WARNING:  Could not convert string to JSON!",
							"Message:  " + errorArgs.ErrorContext.Error.Message.TrimEnd('\r', '\n')
						};
						Form1.Instance.RichTextBox_LogMessage(errorMsg);
					};
				}

				jsonObj = JsonConvert.DeserializeObject<JObject>(jsonStr, jsonSettings);
			}

			return jsonObj;
		}
	}
}
