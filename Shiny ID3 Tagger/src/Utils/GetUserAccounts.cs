//-----------------------------------------------------------------------
// <copyright file="GetUserAccounts.cs" company="Shiny ID3 Tagger">
// Copyright (c) Shiny ID3 Tagger. All rights reserved.
// </copyright>
// <author>ShinyId3Tagger Team</author>
//-----------------------------------------------------------------------

namespace Utils
{
	using System;
	using System.IO;
	using System.Security.Cryptography;
	using System.Text;
	using GlobalVariables;
	using Newtonsoft.Json.Linq;
	using Shiny_ID3_Tagger;

	/// <summary>
	/// Represents the Utility class which holds various helper functions
	/// </summary>
	internal partial class Utils
	{
		/// <summary>
		/// Gets user credentials for all used APIs from an external file
		/// </summary>
		// TODO: Use accounts.json in plain text. If reading this file fails, try decrypting first. If this also fails, throw an error
		// TODO: Merge GetUserAccounts and GetUserSettings into one method
		internal static void GetUserAccounts()
		{
			// Path to user accounts file
			string accountsConfigPath = AppDomain.CurrentDomain.BaseDirectory + @"config\accounts.json";
			string accountsSchemaPath = AppDomain.CurrentDomain.BaseDirectory + @"config\schemas\accounts.schema.json";

			try
			{
				// Read encrypted credentials from accounts.json
				string encryptedJson = File.ReadAllText(accountsConfigPath);

				// Decrypt credentials into valid JSON syntax
				Aes decryptor = Aes.Create();
				byte[] key = new byte[] { 90, 181, 178, 196, 110, 221, 12, 79, 98, 48, 40, 239, 237, 175, 23, 69, 42, 201, 36, 157, 170, 67, 161, 9, 69, 114, 232, 179, 195, 158, 151, 124 };
				byte[] iv = new byte[] { 221, 237, 248, 138, 53, 16, 87, 148, 28, 20, 30, 199, 195, 221, 209, 188 };
				ICryptoTransform decryptorTransformer = decryptor.CreateDecryptor(key, iv);
				byte[] encryptedBytes = Convert.FromBase64String(encryptedJson);
				byte[] decryptedBytes = decryptorTransformer.TransformFinalBlock(encryptedBytes, 0, encryptedBytes.Length);
				string accountsJson = Encoding.UTF8.GetString(decryptedBytes);

				// Validate credentials. If any validation errors occurred, ValidateConfig will throw an exception which is catched later
				ValidateSchema(accountsJson, accountsSchemaPath);

				// Save credentials as JObject for later access throughout the program
				User.Accounts = JObject.Parse(accountsJson);

				// Insert "lastUsed" variables to track which API key was used last if multiple keys for one API are provided
				foreach (string api in new[] { "decibel", "musicgraph", "musixmatch" })
				{
					var account = User.Accounts.GetValue(api, StringComparison.OrdinalIgnoreCase);
					foreach (var item in account)
					{
						item["lastUsed"] = 0;
					}
				}
			}
			catch (ArgumentException ex)
			{
				// Validation failed
				if ((int)User.Settings["DebugLevel"] >= 1)
				{
					string[] errorMsg =
					{
						@"ERROR:    Failed to parse user accounts!",
						"Filepath: " + accountsConfigPath,
						"Message:  " + ex.Message.TrimEnd('\r', '\n')
					};
					Form1.Instance.RichTextBox_PrintErrorMessage(errorMsg);
				}
			}
			catch (FileNotFoundException)
			{
				// File is not found
				if ((int)User.Settings["DebugLevel"] >= 1)
				{
					string[] errorMsg =
					{
						"ERROR:    File not found!",
						"Filepath: " + accountsConfigPath
					};
					Form1.Instance.RichTextBox_PrintErrorMessage(errorMsg);
				}
			}
			catch (IOException)
			{
				// File has a write lock (i.e. opened in another program)
				if ((int)User.Settings["DebugLevel"] >= 1)
				{
					string[] errorMsg =
					{
						"ERROR:    Cannot access file. Already in use!",
						"Filepath: " + accountsConfigPath
					};
					Form1.Instance.RichTextBox_PrintErrorMessage(errorMsg);
				}
			}
		}
	}
}
