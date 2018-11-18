//-----------------------------------------------------------------------
// <copyright file="GetAccounts.cs" company="Shiny ID3 Tagger">
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
		/// Reads accounts.default.json and accounts.user.json
		/// Sets the variable User.Accounts by merging both configs together
		/// User accounts take precedence over values in default accounts
		/// <seealso href="https://stackoverflow.com/a/27502360/935614"/>
		/// </summary>
		/// <returns>The User.Accounts object</returns>
		internal static JObject GetAccounts()
		{
			// Read in default accounts
			string accountsDefaultPath = GlobalVariables.AppDir + @"config\accounts.default.json";
			string accountsDefaultSchemaPath = GlobalVariables.AppDir + @"config\schemas\accounts.default.json";
			JObject accountsDefault = Utils.ReadConfig(accountsDefaultPath, accountsDefaultSchemaPath);

			// Read in user accounts
			string accountsUserPath = GlobalVariables.AppDir + @"config\accounts.user.json";
			string accountsUserSchemaPath = GlobalVariables.AppDir + @"config\schemas\accounts.user.json";
			JObject accountsUser = Utils.ReadConfig(accountsUserPath, accountsUserSchemaPath);

			if (accountsDefault != null && accountsUser != null)
			{
				// "MergeArrayHandling. = Replace" means that user accounts overwrite default accounts
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
