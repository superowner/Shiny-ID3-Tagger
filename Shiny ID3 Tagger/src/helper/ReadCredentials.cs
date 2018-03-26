//-----------------------------------------------------------------------
// <copyright file="ReadCredentials.cs" company="Shiny ID3 Tagger">
// Copyright (c) Shiny ID3 Tagger. All rights reserved.
// </copyright>
// <author>ShinyId3Tagger Team</author>
// <summary>Gets user credentials for all used APIs from external file</summary>
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
		private void ReadCredentials()
		{
			// Path for user credentials file
			string accountsConfigFilepath = AppDomain.CurrentDomain.BaseDirectory + @"config\accounts.json";

			try
			{
				// Read encrypted credentials from accounts.json
				string encryptedJsonStr = File.ReadAllText(accountsConfigFilepath);

				// Decrypt credentials into valid JSON syntax
				Aes decryptor = Aes.Create();
				byte[] key = new byte[] { 90, 181, 178, 196, 110, 221, 12, 79, 98, 48, 40, 239, 237, 175, 23, 69, 42, 201, 36, 157, 170, 67, 161, 9, 69, 114, 232, 179, 195, 158, 151, 124 };
				byte[] iv = new byte[] { 221, 237, 248, 138, 53, 16, 87, 148, 28, 20, 30, 199, 195, 221, 209, 188 };
				ICryptoTransform decryptorTransformer = decryptor.CreateDecryptor(key, iv);
				byte[] encryptedBytes = Convert.FromBase64String(encryptedJsonStr);
				byte[] decryptedBytes = decryptorTransformer.TransformFinalBlock(encryptedBytes, 0, encryptedBytes.Length);
				string accountsJsonStr = Encoding.UTF8.GetString(decryptedBytes);

				// Validate credentials, store errors
				// If any validation error occurred, throw exception to go into catch clause
				IList<string> validationErrors = this.ValidateConfig(accountsJsonStr, this.accountsSchemaStr);

				if (validationErrors.Count > 0)
				{
					string allValidationErrors = string.Join("\n          ", (IEnumerable<string>)validationErrors);

					throw new ArgumentException(allValidationErrors);
				}

				// Save credentials as JObject for later access throughout the program
				User.Accounts = JObject.Parse(accountsJsonStr);

				// Insert "lastUsed" variables to track which API key was used last if multiple keys for one API are provided
				foreach (string api in new string[] { "decibel", "musicgraph", "musixmatch" })
				{
					var account = User.Accounts.GetValue(api, StringComparison.OrdinalIgnoreCase);
					foreach (var item in account)
					{
						item["lastUsed"] = 0;
					}
				}
			}
			catch (Exception ex)
			{
				string[] errorMsg =
					{
					@"ERROR:    Failed to read user credentials! Please close program and fix this first...",
					"Filepath: " + accountsConfigFilepath,
					"Message:  " + ex.Message.TrimEnd('\r', '\n')
				};
				this.PrintLogMessage(this.rtbErrorLog, errorMsg);
			}
		}
	}
}
