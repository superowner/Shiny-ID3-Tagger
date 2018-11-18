//-----------------------------------------------------------------------
// <copyright file="GetSettings.cs" company="Shiny ID3 Tagger">
// Copyright (c) Shiny ID3 Tagger. All rights reserved.
// </copyright>
// <author>ShinyId3Tagger Team</author>
//-----------------------------------------------------------------------
// Reviewed and checked if all possible exceptions are prevented or handled

namespace Utils
{
	using System;
	using GlobalVariables;
	using Newtonsoft.Json.Linq;

	/// <summary>
	/// Represents the Utility class which holds various helper functions
	/// </summary>
	internal partial class Utils
	{
		/// <summary>
		/// Reads settings.default.json and settings.user.json
		/// Sets the variable User.Settings by merging both configs together
		/// User settings take precedence over values in default settings
		/// <seealso href="https://stackoverflow.com/a/27502360/935614"/>
		/// </summary>
		/// <returns>The User.Settings object</returns>
		internal static JObject GetSettings()
		{
			// Read in default settings
			string settingsDefaultPath = GlobalVariables.AppDir + @"config\settings.default.json";
			string settingsDefaultSchemaPath = GlobalVariables.AppDir + @"config\schemas\settings.default.json";
			JObject settingsDefault = Utils.ReadConfig(settingsDefaultPath, settingsDefaultSchemaPath);

			// Read in user settings
			string settingsUserPath = GlobalVariables.AppDir + @"config\settings.user.json";
			string settingsUserSchemaPath = GlobalVariables.AppDir + @"config\schemas\settings.user.json";
			JObject settingsUser = Utils.ReadConfig(settingsUserPath, settingsUserSchemaPath);

			if (settingsDefault != null && settingsUser != null)
			{
				// MergeArrayHandling.Replace is important. Otherwise default settings won't get overwritten
				settingsDefault.Merge(settingsUser, new JsonMergeSettings
				{
					MergeArrayHandling = MergeArrayHandling.Replace,
					MergeNullValueHandling = MergeNullValueHandling.Ignore,
				});
			}

			return settingsDefault;
		}
	}
}
