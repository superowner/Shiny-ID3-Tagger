//-----------------------------------------------------------------------
// <copyright file="DeserializeJson.cs" company="Shiny ID3 Tagger">
// Copyright (c) Shiny ID3 Tagger. All rights reserved.
// </copyright>
// <author>ShinyId3Tagger Team</author>
//-----------------------------------------------------------------------
// Reviewed and checked if all possible exceptions are prevented or handled
// TODO: Examine why only the first JSON parsing error is printed out. It should print out all errors if there are more than 1
// TODO: Throws an error if JArray is read. It always expects JObject. Us try and catch JsonException

namespace Utils
{
	using System.Collections.Generic;
	using Newtonsoft.Json;
	using Newtonsoft.Json.Linq;
	using Shiny_ID3_Tagger;

	/// <summary>Represents the Utility class which holds various helper functions</summary>
	internal partial class Utils
	{
		/// <summary> Deserialize a JSON string to a JSON.NET JObject.
		/// Returns NULL if parsing failed
		/// <seealso href="https://www.newtonsoft.com/json/help/html/T_Newtonsoft_Json_Formatting.htm"/>
		/// <seealso href="https://www.newtonsoft.com/json/help/html/T_Newtonsoft_Json_DateParseHandling.htm"/>
		/// <seealso href="https://www.newtonsoft.com/json/help/html/P_Newtonsoft_Json_Serialization_ErrorContext_Handled.htm"/>
		/// <seealso href="https://stackoverflow.com/a/26264889/935614"/>
		/// </summary>
		/// <param name="jsonStr">The input string which should be deserialized</param>
		/// <returns>The new JObject holding all the values from content string</returns>
		internal static JObject DeserializeJson(string jsonStr)
		{
			// Prevents exception "ArgumentNullException"
			if (jsonStr == null)
			{
				return null;
			}

			List<string> errorMessages = new List<string>();

			// Set up custom deserialize settings
			// 1) Intend each child 4 spaces
			// 2) Don't convert a date string to date objects. There is a seperat method for that
			// 3) Set error on handled=true. DeserializeJson should not throw an exception
			JsonSerializerSettings jsonSettings = new JsonSerializerSettings
			{
				Formatting = Formatting.Indented,
				DateParseHandling = DateParseHandling.None,
				Error = (obj, errorArgs) =>
				{
					errorMessages.Add(errorArgs.ErrorContext.Error.Message);
					errorArgs.ErrorContext.Handled = true;
				},
			};

			// Deserialize JSON
			JObject json = JsonConvert.DeserializeObject<JObject>(jsonStr, jsonSettings);

			// Check if errors occured
			if (errorMessages.Count > 0)
			{
				// Build a more useful error message when JSON parsing fails
				string[] warningMsg =
				{
					"WARNING:  Could not parse JSON!",
					"Message:  " + string.Join("\n          ", errorMessages),
					"JSON:     " + jsonStr,
				};

				Form1.Instance.RichTextBox_LogMessage(warningMsg, 3);
			}

			return json;
		}
	}
}
