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
	using Newtonsoft.Json;
	using Newtonsoft.Json.Linq;
	using Newtonsoft.Json.Schema;
	using Shiny_ID3_Tagger;

	/// <summary>
	/// Represents the Utility class which holds various helper functions
	/// </summary>
	internal partial class Utils
	{
		/// <summary>
		/// Algorythm key and Initialization vector to decrypt
		/// </summary>
		private static readonly byte[] AlgorythmKey = new byte[] { 90, 181, 178, 196, 110, 221, 12, 79, 98, 48, 40, 239, 237, 175, 23, 69, 42, 201, 36, 157, 170, 67, 161, 9, 69, 114, 232, 179, 195, 158, 151, 124 };
		private static readonly byte[] InitializationVector = new byte[] { 221, 237, 248, 138, 53, 16, 87, 148, 28, 20, 30, 199, 195, 221, 209, 188 };

		/// <summary>
		/// Reads a config file from an external file and saves it
		/// </summary>
		/// <param name="configPath">Relative path to config file</param>
		/// <param name="schemaPath">Relative path to schema file for the config file</param>
		/// <returns>The parsed and validated config object</returns>
		internal static JObject ReadConfig(string configPath, string schemaPath)
		{
			string fileContent = null;

			try
			{
				// Read config
				fileContent = File.ReadAllText(configPath);
			}
			catch (Exception ex)
			{
				// Config file has a write lock (i.e. opened in another program)
				string[] errorMsg =
				{
					"ERROR:    Cannot open or access file!",
					"Config:   " + configPath,
					"Schema:   " + schemaPath,
					"Message:  " + ex.Message.TrimEnd('\r', '\n'),
				};
				Form1.Instance.RichTextBox_LogMessage(errorMsg, 2);

				return null;
			}

			// FileContent cannot be null from here on
			try
			{
				// If it's a valid base64 string, then it's probably an encrypted config
				if (IsValidBase64(fileContent))
				{
					// Decrypt config before trying to parse JSON
					Aes decryptor = Aes.Create();
					ICryptoTransform decryptorTransformer = decryptor.CreateDecryptor(AlgorythmKey, InitializationVector);

					// Converts base64 string to normal string (can throw FormatException)
					byte[] encryptedBytes = Convert.FromBase64String(fileContent);
					byte[] decryptedBytes = decryptorTransformer.TransformFinalBlock(encryptedBytes, 0, encryptedBytes.Length);
					fileContent = Encoding.UTF8.GetString(decryptedBytes);
				}

				// Parse JSON (throws JsonException)
				JObject json = Utils.DeserializeJson(fileContent);

				// Validate config (throws JSchemaValidationException)
				ValidateConfig(json, schemaPath);

				// Return config
				return json;
			}
			catch (FormatException ex)
			{
				// Config file is not a valid decrypted AES256 file
				string[] errorMsg =
				{
					"ERROR:    Failed to deserialize or decrypt a config file!",
					"Config:   " + configPath,
					"Schema:   " + schemaPath,
					"Message:  " + ex.Message.TrimEnd('\r', '\n'),
				};
				Form1.Instance.RichTextBox_LogMessage(errorMsg, 2);

				return null;
			}
			catch (JsonException ex)
			{
				// Parsing failed. File content is not a valid JSON
				string[] errorMsg =
				{
					"ERROR:    Failed to parse a config file!",
					"Config:   " + configPath,
					"Schema:   " + schemaPath,
					"Message:  " + ex.Message.TrimEnd('\r', '\n'),
				};
				Form1.Instance.RichTextBox_LogMessage(errorMsg, 2);

				return null;
			}
			catch (JSchemaValidationException ex)
			{
				// Validation failed. JSON could not be validated against its schema
				string[] errorMsg =
				{
					"ERROR:    Failed to validate a config file!",
					"Config:   " + configPath,
					"Schema:   " + schemaPath,
					"Message:  " + ex.Message.TrimEnd('\r', '\n'),
				};
				Form1.Instance.RichTextBox_LogMessage(errorMsg, 2);

				return null;
			}
		}
	}
}
