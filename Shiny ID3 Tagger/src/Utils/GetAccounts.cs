//-----------------------------------------------------------------------
// <copyright file="GetAccounts.cs" company="Shiny ID3 Tagger">
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
		/// Sets the variable User.Accounts
		/// Uses default accounts first and then applies user accounts onto them
		/// <seealso href="https://stackoverflow.com/a/27502360/935614"/>
		/// </summary>
		/// <returns>The User.Accounts object</returns>
		internal static JObject GetAccounts()
		{
			// Read in default accounts
			string accountsDefaultPath = AppDomain.CurrentDomain.BaseDirectory + @"config\accounts.default.json";
			string accountsDefaultSchemaPath = AppDomain.CurrentDomain.BaseDirectory + @"config\schemas\accounts.default.json";
			JObject accountsDefault = Utils.ReadConfig(accountsDefaultPath, accountsDefaultSchemaPath);

			// Read in user accounts
			string accountsUserPath = AppDomain.CurrentDomain.BaseDirectory + @"config\accounts.user.json";
			string accountsUserSchemaPath = AppDomain.CurrentDomain.BaseDirectory + @"config\schemas\accounts.user.json";
			JObject accountsUser = Utils.ReadConfig(accountsUserPath, accountsUserSchemaPath);

			if (accountsDefault != null && accountsUser != null)
			{
				// MergeArrayHandling.Replace is important. Otherwise default accounts won't get overwritten
				accountsDefault.Merge(accountsUser, new JsonMergeSettings
				{
					MergeArrayHandling = MergeArrayHandling.Replace,
					MergeNullValueHandling = MergeNullValueHandling.Ignore,
				});
			}

			return accountsDefault;
		}
	}
}
