//-----------------------------------------------------------------------
// <copyright file="GetSettings.cs" company="Shiny ID3 Tagger">
// Copyright (c) Shiny ID3 Tagger. All rights reserved.
// </copyright>
// <author>ShinyId3Tagger Team</author>
//-----------------------------------------------------------------------

namespace Utils
{
	using System;
	using Newtonsoft.Json.Linq;

	/// <summary>
	/// Represents the Utility class which holds various helper functions
	/// </summary>
	internal partial class Utils
	{
		/// <summary>
		/// Sets the variable User.settings
		/// Uses default settings first and then applies user settings onto them
		/// <seealso href="https://stackoverflow.com/a/27502360/935614"/>
		/// </summary>
		/// <returns>The User.Settings object</returns>
		internal static JObject GetSettings()
		{
			// Read in default settings
			string settingsDefaultPath = AppDomain.CurrentDomain.BaseDirectory + @"config\settings.default.json";
			string settingsDefaultSchemaPath = AppDomain.CurrentDomain.BaseDirectory + @"config\schemas\settings.default.json";
			JObject settingsDefault = Utils.ReadConfig(settingsDefaultPath, settingsDefaultSchemaPath);

			// Read in user settings
			string settingsUserPath = AppDomain.CurrentDomain.BaseDirectory + @"config\settings.user.json";
			string settingsUserSchemaPath = AppDomain.CurrentDomain.BaseDirectory + @"config\schemas\settings.user.json";
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
