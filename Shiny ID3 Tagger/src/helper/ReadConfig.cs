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
	using System.IO;
	using System.Security.Cryptography;
	using System.Text;
	using Newtonsoft.Json.Linq;

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

				// Save accounts to JObject for later access throughout the program
				User.Accounts = JObject.Parse(accountsJsonStr);

				// Validate settings config, store errors
				IList<string> errorMessages = this.ValidateConfig(accountsJsonStr, this.accountsSchemaStr);

				// If any validation error occurred, throw exception
				if (errorMessages.Count > 0)
				{
					string errorMsg = string.Join("\n          ", (IEnumerable<string>)errorMessages);
					errorMsg += "\nFilepath: " + accountsConfigFilepath;

					throw new ArgumentException(errorMsg);
				}

				// Insert "lastUsed" variables to track which API key usage
				foreach (string api in new string[] { "decibel", "musicgraph", "musixmatch" })
				{
					var account = User.Accounts.GetValue(api, StringComparison.OrdinalIgnoreCase);
					foreach (var item in account)
					{
						item["lastUsed"] = 0;
					}
				}

				// Read user configuration from settings.json
				string settingsJsonStr = File.ReadAllText(settingsConfigFilepath);

				// Save settings to JObject for later access throughout the program
				User.Settings = JObject.Parse(settingsJsonStr);

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
