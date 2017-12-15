//-----------------------------------------------------------------------
// <copyright file="ReadConfig.cs" company="Shiny ID3 Tagger">
// Copyright (c) Shiny ID3 Tagger. All rights reserved.
// </copyright>
// <author>ShinyId3Tagger Team</author>
// <summary>Gets configurations from external config files</summary>
//-----------------------------------------------------------------------

namespace GlobalNamespace
{
	using System;
	using System.Collections.Generic;
	using System.Data;
	using System.IO;
	using System.Security.Cryptography;
	using System.Text;
	using Newtonsoft.Json;

	public partial class Form1
	{
		private bool ReadConfig()
		{
			// Path for Config files
			string settingsConfigFilepath = AppDomain.CurrentDomain.BaseDirectory + @"config\settings.json";
			string accountsConfigFilepath = AppDomain.CurrentDomain.BaseDirectory + @"config\accounts.json";
			byte[] key = new byte[] { 90, 181, 178, 196, 110, 221, 12, 79, 98, 48, 40, 239, 237, 175, 23, 69, 42, 201, 36, 157, 170, 67, 161, 9, 69, 114, 232, 179, 195, 158, 151, 124 };
			byte[] iv = new byte[] { 221, 237, 248, 138, 53, 16, 87, 148, 28, 20, 30, 199, 195, 221, 209, 188 };

			try
			{
				// Read user accounts credentials from accounts.json
				string encryptedJsonStr = File.ReadAllText(accountsConfigFilepath);

				// Decrypt accounts.json into valid JSON syntax
				Aes decryptor = Aes.Create();
				ICryptoTransform decryptorTransformer = decryptor.CreateDecryptor(key, iv);
				byte[] encryptedBytes = Convert.FromBase64String(encryptedJsonStr);
				byte[] decryptedBytes = decryptorTransformer.TransformFinalBlock(encryptedBytes, 0, encryptedBytes.Length);
				string accountsJsonStr = Encoding.UTF8.GetString(decryptedBytes);

				// Save settings to dictionary for later access throughout the program
				User.Accounts = JsonConvert.DeserializeObject<Dictionary<string, string>>(accountsJsonStr);

				// Validate settings config, store errors
				IList<string> errorMessages = this.ValidateConfig(accountsJsonStr, this.accountsSchemaStr);

				// If any validation error occurred, throw exception
				if (errorMessages.Count > 0)
				{
					string errorMsg = string.Join("\n          ", (IEnumerable<string>)errorMessages);
					errorMsg += "\nFilepath: " + accountsConfigFilepath;

					throw new ArgumentException(errorMsg);
				}

				// TODO: Find a better way to store these little account lists in User.Accounts variable. Maybe <string, dynamic>?
				// Generic method to create tables, pass tuples as arguments for row initialization
				User.DbAccounts = new DataTable();
				User.DbAccounts.Locale = cultEng;
				User.DbAccounts.Columns.Add("lastUsed", typeof(double));
				User.DbAccounts.Columns.Add("id", typeof(string));
				User.DbAccounts.Columns.Add("key", typeof(string));
				User.DbAccounts.Rows.Add(1, User.Accounts["DbAppId1"], User.Accounts["DbAppKey1"]);
				User.DbAccounts.Rows.Add(2, User.Accounts["DbAppId2"], User.Accounts["DbAppKey2"]);
				User.DbAccounts.Rows.Add(3, User.Accounts["DbAppId3"], User.Accounts["DbAppKey3"]);
				User.DbAccounts.Rows.Add(4, User.Accounts["DbAppId4"], User.Accounts["DbAppKey4"]);
				User.DbAccounts.Rows.Add(5, User.Accounts["DbAppId5"], User.Accounts["DbAppKey5"]);

				User.MgAccounts = new DataTable();
				User.MgAccounts.Locale = cultEng;
				User.MgAccounts.Columns.Add("lastUsed", typeof(double));
				User.MgAccounts.Columns.Add("key", typeof(string));
				User.MgAccounts.Rows.Add(1, User.Accounts["MgAppKey1"]);
				User.MgAccounts.Rows.Add(2, User.Accounts["MgAppKey2"]);
				User.MgAccounts.Rows.Add(3, User.Accounts["MgAppKey3"]);

				User.MmAccounts = new DataTable();
				User.MmAccounts.Locale = cultEng;
				User.MmAccounts.Columns.Add("lastUsed", typeof(double));
				User.MmAccounts.Columns.Add("key", typeof(string));
				User.MmAccounts.Rows.Add(1, User.Accounts["MmApiKey1"]);
				User.MmAccounts.Rows.Add(2, User.Accounts["MmApiKey2"]);
				User.MmAccounts.Rows.Add(3, User.Accounts["MmApiKey3"]);

				// Read user configuration from settings.json
				string settingsJsonStr = File.ReadAllText(settingsConfigFilepath);

				// Save settings to dictionary for later access throughout the program
				User.Settings = JsonConvert.DeserializeObject<Dictionary<string, dynamic>>(settingsJsonStr);

				// Validate settings config, store errors
				errorMessages = this.ValidateConfig(settingsJsonStr, this.settingsSchemaStr);

				// If any validation error occurred, throw exception
				if (errorMessages.Count > 0)
				{
					string errorMsg = string.Join("\n          ", (IEnumerable<string>)errorMessages);
					errorMsg += "\nFilepath: " + settingsConfigFilepath;

					throw new ArgumentException(errorMsg);
				}

				return true;
			}
			catch (Exception ex)
			{
				string[] errorMsg =
					{
					@"ERROR:    Failed to read user configuration! Please close program and fix it first...",
					"Message:  " + ex.Message.TrimEnd('\r', '\n')
				};
				this.PrintLogMessage(this.rtbErrorLog, errorMsg);
			}

			return false;
		}
	}
}
