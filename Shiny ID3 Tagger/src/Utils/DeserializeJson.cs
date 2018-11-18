//-----------------------------------------------------------------------
// <copyright file="DeserializeJson.cs" company="Shiny ID3 Tagger">
// Copyright (c) Shiny ID3 Tagger. All rights reserved.
// </copyright>
// <author>ShinyId3Tagger Team</author>
//-----------------------------------------------------------------------
// Reviewed and checked if all possible exceptions are prevented or handled

namespace Utils
{
	using Newtonsoft.Json;
	using Newtonsoft.Json.Linq;
	using Shiny_ID3_Tagger;

	/// <summary>Represents the Utility class which holds various helper functions</summary>
	internal partial class Utils
	{
		/// <summary> Deserialize a JSON string to a JSON.NET JObject.
		/// Returns NULL is parsing failed
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

			return JsonConvert.DeserializeObject<JObject>(jsonStr, GetJsonDeserializerSettings(jsonStr));
		}

		/// <summary>
		/// Set custom settings for JSON deserialization
		/// 1) Intend each child 4 spaces
		/// 2) Don't convert a date string to date objects. There is a seperat method for that
		/// <seealso href="https://www.newtonsoft.com/json/help/html/T_Newtonsoft_Json_Formatting.htm"/>
		/// <seealso href="https://www.newtonsoft.com/json/help/html/T_Newtonsoft_Json_DateParseHandling.htm"/>
		/// <seealso href="https://www.newtonsoft.com/json/help/html/P_Newtonsoft_Json_Serialization_ErrorContext_Handled.htm"/>
		/// </summary>
		/// <returns>A JsonSerializerSettings object holding all settings</returns>
		private static JsonSerializerSettings GetJsonDeserializerSettings(string jsonStr)
		{
			JsonSerializerSettings jsonSettings = new JsonSerializerSettings
			{
				Formatting = Formatting.Indented,
				DateParseHandling = DateParseHandling.None,
			};

			jsonSettings.Error += (obj, errorArgs) =>
			{
				// Set error on handled=true. DeserializeJson should never throw an error
				// Caller should expect null value if parsing fails
				errorArgs.ErrorContext.Handled = true;

				// Build a more useful error message when JSON parsing fails
				string[] warningMsg =
				{
					"WARNING:  Could not parse JSON!",
					"Message:  " + errorArgs.ErrorContext.Error.Message.TrimEnd('\r', '\n'),
					"JSON:     " + jsonStr,
				};

				Form1.Instance.RichTextBox_LogMessage(warningMsg, 3);
			};

			return jsonSettings;
		}
	}
}
