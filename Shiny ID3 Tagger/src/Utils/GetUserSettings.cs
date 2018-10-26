//-----------------------------------------------------------------------
// <copyright file="GetUserSettings.cs" company="Shiny ID3 Tagger">
// Copyright (c) Shiny ID3 Tagger. All rights reserved.
// </copyright>
// <author>ShinyId3Tagger Team</author>
// <summary>Gets custom user settings from external file</summary>
//-----------------------------------------------------------------------

namespace Utils
{
	using System;
	using System.IO;
	using GlobalVariables;
	using Newtonsoft.Json.Linq;
	using Shiny_ID3_Tagger;

	/// <summary>
	/// Represents the Utility class which holds various helper functions
	/// </summary>
	internal partial class Utils
	{
		/// <summary>
		/// Gets program settings from an external file which are allowed to be changed by users
		/// </summary>
		internal static void GetUserSettings()
		{
			// Path to user settings file
			string settingsConfigPath = AppDomain.CurrentDomain.BaseDirectory + @"config\settings.json";
			string settingsSchemaPath = AppDomain.CurrentDomain.BaseDirectory + @"config\schemas\settings.schema.json";

			try
			{
				// Read user settings from settings.json
				string settingsJson = File.ReadAllText(settingsConfigPath);

				// Validate settings config. If any validation errors occurred, ValidateConfig will throw an ArgumentException
				ValidateSchema(settingsJson, settingsSchemaPath);

				// Save settings to JObject for later access throughout the program
				User.Settings = JObject.Parse(settingsJson);
			}
			catch (ArgumentException ex)
			{
				// Validation failed
				if ((int)User.Settings["DebugLevel"] >= 1)
				{
					string[] errorMsg =
					{
						@"ERROR:    Failed to parse user settings!",
						"Filepath: " + settingsConfigPath,
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
						"Filepath: " + settingsConfigPath
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
						"Filepath: " + settingsConfigPath
					};
					Form1.Instance.RichTextBox_PrintErrorMessage(errorMsg);
				}
			}
		}
	}
}
