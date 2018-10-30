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
	using Newtonsoft.Json;
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
		/// <returns>The parsed and validated config object</returns>
		internal static JObject ReadConfig(string configPath, string schemaPath)
		{
			// Build path to config file and schema file
			string fullConfigPath = AppDomain.CurrentDomain.BaseDirectory + configPath;
			string fullSchemaPath = AppDomain.CurrentDomain.BaseDirectory + schemaPath;

			try
			{
				// Read config
				string fileContent = File.ReadAllText(fullConfigPath);

				// Parse JSON, don't throw an error so we can try to decrypt it later
				JObject json = Utils.DeserializeJson(fileContent, false);

				// If first parsing failed, json object stays null. Then we assume the config file is encrypted
				if (json == null)
				{
					// Decrypt config
					Aes decryptor = Aes.Create();
					byte[] key = new byte[] { 90, 181, 178, 196, 110, 221, 12, 79, 98, 48, 40, 239, 237, 175, 23, 69, 42, 201, 36, 157, 170, 67, 161, 9, 69, 114, 232, 179, 195, 158, 151, 124 };
					byte[] iv = new byte[] { 221, 237, 248, 138, 53, 16, 87, 148, 28, 20, 30, 199, 195, 221, 209, 188 };
					ICryptoTransform decryptorTransformer = decryptor.CreateDecryptor(key, iv);
					byte[] encryptedBytes = Convert.FromBase64String(fileContent);
					byte[] decryptedBytes = decryptorTransformer.TransformFinalBlock(encryptedBytes, 0, encryptedBytes.Length);
					string decryptedFileContent = Encoding.UTF8.GetString(decryptedBytes);

					// Parse JSON again (can throw an ??? Exception)
					json = Utils.DeserializeJson(decryptedFileContent);
				}

				// Validate config (can throw an ArgumentException)
				ValidateSchema(json, fullSchemaPath);

				// Return config
				return json;
			}
			catch (JsonException ex)
			{
				// Parsing failed. File content is not a valid JSON
				string[] errorMsg =
				{
					@"ERROR:    Failed to parse a config file!",
					"Filepath: " + fullConfigPath,
					"Message:  " + ex.Message.TrimEnd('\r', '\n')
				};
				Form1.Instance.RichTextBox_LogMessage(errorMsg, 2);

				return null;
			}
			catch (ArgumentException ex)
			{
				// Validation failed. JSON could not be validated against its schema
				string[] errorMsg =
				{
					@"ERROR:    Failed to validate a config file!",
					"Filepath: " + fullConfigPath,
					"Message:  " + ex.Message.TrimEnd('\r', '\n')
				};
				Form1.Instance.RichTextBox_LogMessage(errorMsg, 2);

				return null;
			}
			catch (FileNotFoundException)
			{
				// Config file is not found
				string[] errorMsg =
				{
					"ERROR:    File not found!",
					"Filepath: " + fullConfigPath
				};
				Form1.Instance.RichTextBox_LogMessage(errorMsg, 2);

				return null;
			}
			catch (IOException)
			{
				// Config file has a write lock (i.e. opened in another program)
				string[] errorMsg =
				{
					"ERROR:    Cannot access file. Already in use!",
					"Filepath: " + fullConfigPath
				};
				Form1.Instance.RichTextBox_LogMessage(errorMsg, 2);

				return null;
			}
		}
	}
}
