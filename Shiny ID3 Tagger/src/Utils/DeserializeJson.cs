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
		/// <param name="jsonStr">The input string which should be deserialized. If jsonStr = null, then null will be returned</param>
		/// <param name="throwException">If true, then throw  an exception. Otherwise just print a warning (default)</param>
		/// <returns>The new JObject holding all the values from content string</returns>
		/// <exception cref="JsonException">If input string is not a valid JSON</exception>
		internal static JObject DeserializeJson(string jsonStr, bool throwException = false)
		{
			// string.Empty is used to prevent a NullReference exception
			return JsonConvert.DeserializeObject<JObject>(jsonStr ?? string.Empty, GetJsonDeserializerSettings(jsonStr, throwException));
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
		/// <exception cref="JsonException">If input string is not a valid JSON</exception>
		private static JsonSerializerSettings GetJsonDeserializerSettings(string jsonStr, bool throwException)
		{
			JsonSerializerSettings jsonSettings = new JsonSerializerSettings
			{
				Formatting = Formatting.Indented,
				DateParseHandling = DateParseHandling.None,
			};

			jsonSettings.Error += (obj, errorArgs) =>
			{
				if (throwException == false)
				{
					// Set error on handled=true. DeserializeJson shall not throw an error
					// Other methods should expect a null value if parsing failed
					errorArgs.ErrorContext.Handled = true;

					// Build a more useful error message when JSON parsing fails
					string[] warningMsg =
					{
						"WARNING:  Could not parse JSON!",
						"Message:  " + errorArgs.ErrorContext.Error.Message.TrimEnd('\r', '\n'),
						"JSON:     " + jsonStr,
					};

					Form1.Instance.RichTextBox_LogMessage(warningMsg, 3);
				}
			};

			return jsonSettings;
		}
	}
}
