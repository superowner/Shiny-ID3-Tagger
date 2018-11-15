//-----------------------------------------------------------------------
// <copyright file="GetSettingsAccounts.cs" company="Shiny ID3 Tagger">
// Copyright (c) Shiny ID3 Tagger. All rights reserved.
// </copyright>
// <author>ShinyId3Tagger Team</author>
//-----------------------------------------------------------------------

namespace Utils
{
	using System;
	using System.Text;
	using GlobalVariables;
	using Newtonsoft.Json;
	using Newtonsoft.Json.Linq;
	using Shiny_ID3_Tagger;

	/// <summary>
	/// Represents the Utility class which holds various helper functions
	/// </summary>
	internal partial class Utils
	{
		/// <summary>
		/// Sets the variable User.settings and User.accounts
		/// Uses default settings/accounts first and then applies user settings/accounts onto them
		/// </summary>
		/// <returns>A bool</returns>
		internal static bool GetSettingsAccounts()
		{
			// Read in default settings
			string settingsDefaultPath = AppDomain.CurrentDomain.BaseDirectory + @"config\settings.default.json";
			string settingsDefaultSchemaPath = AppDomain.CurrentDomain.BaseDirectory + @"config\schemas\settings.default.json";
			User.Settings = Utils.ReadConfig(settingsDefaultPath, settingsDefaultSchemaPath);

			// Read in user settings
			string settingsUserPath = AppDomain.CurrentDomain.BaseDirectory + @"config\settings.user.json";
			string settingsUserSchemaPath = AppDomain.CurrentDomain.BaseDirectory + @"config\schemas\settings.user.json";
			JObject settingsUser = Utils.ReadConfig(settingsUserPath, settingsUserSchemaPath);

			if (settingsUser != null)
			{
				User.Settings = MergeConfig(User.Settings, settingsUser);
			}

			// Read in default accounts
			string accountsDefaulPath = AppDomain.CurrentDomain.BaseDirectory + @"config\accounts.default.json";
			string accountsDefaultSchemaPath = AppDomain.CurrentDomain.BaseDirectory + @"config\schemas\accounts.default.json";
			User.Accounts = Utils.ReadConfig(accountsDefaulPath, accountsDefaultSchemaPath);

			// Read in user accounts
			string accountsUserPath = AppDomain.CurrentDomain.BaseDirectory + @"config\accounts.user.json";
			string accountsUserSchemaPath = AppDomain.CurrentDomain.BaseDirectory + @"config\schemas\accounts.user.json";
			JObject accountsUser = Utils.ReadConfig(accountsUserPath, accountsUserSchemaPath);

			if (accountsUser != null)
			{
				User.Accounts = MergeConfig(User.Accounts, accountsUser);
			}

			return User.Settings != null && User.Accounts != null;
		}

		/// <summary>
		/// Merges a user config on top of a default config, user values take priority over default values
		/// </summary>
		/// <param name="defaultConfig">The default or base config file</param>
		/// <param name="userConfig">The config file which has to merged on top of base file. But skip empty entries</param>
		/// <returns>The merged config object</returns>
		private static JObject MergeConfig(JObject defaultConfig, JObject userConfig)
		{
			return defaultConfig;
		}
	}
}
