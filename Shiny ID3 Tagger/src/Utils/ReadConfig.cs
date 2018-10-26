//-----------------------------------------------------------------------
// <copyright file="ReadConfig.cs" company="Shiny ID3 Tagger">
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
		/// Reads a config file from an external file and saves it
		/// </summary>
		/// <param name="configPath">Path to config file</param>
		/// <param name="schemaPath">Path to schema file for the config file</param>
		/// <param name="config">Variable where config should be saved in</param>
		internal static void ReadConfig(string configPath, string schemaPath, JObject config)
		{
			// Build path to config file and schema file
			string fullConfigPath = AppDomain.CurrentDomain.BaseDirectory + configPath;
			string fullSchemaPath = AppDomain.CurrentDomain.BaseDirectory + schemaPath;

			try
			{
				// Read config
				string fileContent = File.ReadAllText(fullConfigPath);

				// Validate config. If any validation errors occurred, ValidateConfig will return an error string
				string validationResult = ValidateSchema(fileContent, fullSchemaPath);

				// If the config could not be validated on first try, assume the config file is encrypted
				if (validationResult != string.Empty)
				{
					// Decrypt config
					Aes decryptor = Aes.Create();
					byte[] key = new byte[] { 90, 181, 178, 196, 110, 221, 12, 79, 98, 48, 40, 239, 237, 175, 23, 69, 42, 201, 36, 157, 170, 67, 161, 9, 69, 114, 232, 179, 195, 158, 151, 124 };
					byte[] iv = new byte[] { 221, 237, 248, 138, 53, 16, 87, 148, 28, 20, 30, 199, 195, 221, 209, 188 };
					ICryptoTransform decryptorTransformer = decryptor.CreateDecryptor(key, iv);
					byte[] encryptedBytes = Convert.FromBase64String(fileContent);
					byte[] decryptedBytes = decryptorTransformer.TransformFinalBlock(encryptedBytes, 0, encryptedBytes.Length);
					fileContent = Encoding.UTF8.GetString(decryptedBytes);

					// Validate config again
					validationResult = ValidateSchema(fileContent, fullSchemaPath);

					// If the config could not be validated, throw an exception
					if (validationResult != string.Empty)
					{
						throw new ArgumentException(validationResult);
					}
				}

				// Save config
				config = JObject.Parse(fileContent);
			}
			catch (ArgumentException ex)
			{
				// Validation failed. Malformed config file
				if ((int)User.Settings["DebugLevel"] >= 1)
				{
					string[] errorMsg =
					{
						@"ERROR:    Failed to parse user accounts!",
						"Filepath: " + fullConfigPath,
						"Message:  " + ex.Message.TrimEnd('\r', '\n')
					};
					Form1.Instance.RichTextBox_LogMessage(errorMsg);
				}
			}
			catch (FileNotFoundException)
			{
				// Config file is not found
				if ((int)User.Settings["DebugLevel"] >= 1)
				{
					string[] errorMsg =
					{
						"ERROR:    File not found!",
						"Filepath: " + fullConfigPath
					};
					Form1.Instance.RichTextBox_LogMessage(errorMsg);
				}
			}
			catch (IOException)
			{
				// Config file has a write lock (i.e. opened in another program)
				if ((int)User.Settings["DebugLevel"] >= 1)
				{
					string[] errorMsg =
					{
						"ERROR:    Cannot access file. Already in use!",
						"Filepath: " + fullConfigPath
					};
					Form1.Instance.RichTextBox_LogMessage(errorMsg);
				}
			}
		}
	}
}
