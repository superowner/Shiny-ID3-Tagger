//-----------------------------------------------------------------------
// <copyright file="DeserializeJson.cs" company="Shiny ID3 Tagger">
// Copyright (c) Shiny ID3 Tagger. All rights reserved.
// </copyright>
// <author>ShinyId3Tagger Team</author>
//-----------------------------------------------------------------------
// REVIEW: Refactor whole method

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
		/// <param name="throwError">True or false (default) if parsing errors should throw an error</param>
		/// <returns>The new JObject holding all the values from content string</returns>
		internal static JObject DeserializeJson(string jsonStr, bool throwError = true)
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

				if (throwError)
				{
					// Build a more useful error message when JSON parsing fails later
					jsonSettings.Error += (obj, errorArgs) =>
					{
						string[] errorMsg =
						{
							"WARNING:  Could not convert string to JSON!",
							"Message:  " + errorArgs.ErrorContext.Error.Message.TrimEnd('\r', '\n')
						};
						Form1.Instance.RichTextBox_LogMessage(errorMsg, 2);
					};
				}
				else
				{
					// If throwError is set to false, then set error on handled=true. This method doesn't throw errors then
					jsonSettings.Error += (obj, errorArgs) =>
					{
						errorArgs.ErrorContext.Handled = true;
					};
				}

				// Parse JSON
				jsonObj = JsonConvert.DeserializeObject<JObject>(jsonStr, jsonSettings);
			}

			return jsonObj;
		}
	}
}
