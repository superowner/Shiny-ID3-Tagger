//-----------------------------------------------------------------------
// <copyright file="ReadConfig.cs" company="Shiny ID3 Tagger">
// Copyright (c) Shiny ID3 Tagger. All rights reserved.
// </copyright>
// <author>ShinyId3Tagger Team</author>
//-----------------------------------------------------------------------
// Reviewed and checked if all possible exceptions are prevented or handled

namespace Utils
{
	using System;
	using System.Collections.Generic;
	using System.IO;
	using System.Security.Cryptography;
	using System.Text;
	using Exceptions;
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
				// Catches possible exceptions
				// - ArgumentException
				// - ArgumentNullException
				// - PathTooLongException
				// - DirectoryNotFoundException
				// - IOException
				// - UnauthorizedAccessException
				// - FileNotFoundException
				// - NotSupportedException
				// - SystemException
				// - System.Security.SecurityException
				try
				{
					// Try to read config file content
					fileContent = File.ReadAllText(configPath);
				}
				catch (Exception ex)
				{
					throw new ReadConfigException("File not found or inaccessible!", ex);
				}

				try
				{
					// If it's a valid base64 string, then it's probably an encrypted config
					// FileContent cannot be null since ReadAllText() was successful
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

					// Parse JSON
					JObject json = Utils.DeserializeJson(fileContent);

					// If file content was not a valid JSON
					if (json == null)
					{
						throw new ReadConfigException("Failed to parse a config file!");
					}

					// Validate config (returns a list of errors
					List<string> errorMessages = ValidateConfig(json, schemaPath);

					// If the config could not be validated, throw an exception
					if (errorMessages.Count > 0)
					{
						string innerExceptionMessage = string.Join("\n          ", errorMessages);

						throw new ReadConfigException("Failed to validate a config file against its schema!", new ArgumentException(innerExceptionMessage));
					}

					// Return config
					return json;
				}
				catch (FormatException ex)
				{
					throw new ReadConfigException("Failed to decrypt a config file. File content is not a valid base64 string!", ex);
				}
				catch (OverflowException ex)
				{
					throw new ReadConfigException("Failed to decrypt a config file. File too big!", ex);
				}
			}
			catch (ReadConfigException ex)
			{
				List<string> errorMsg = new List<string>
				{
					"ERROR:    " + ex.Message,
					"Config:   " + configPath,
					"Schema:   " + schemaPath,
				};

				if (ex.InnerException != null)
				{
					errorMsg.AddRange(new[] { "Message:  " + ex.InnerException.Message });
				}

				MainForm.Instance.RichTextBox_LogMessage(errorMsg.ToArray(), 2);

				return null;
			}
		}
	}
}
