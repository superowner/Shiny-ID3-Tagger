//-----------------------------------------------------------------------
// <copyright file="ReadSettings.cs" company="Shiny ID3 Tagger">
// Copyright (c) Shiny ID3 Tagger. All rights reserved.
// </copyright>
// <author>ShinyId3Tagger Team</author>
// <summary>Gets custom user settings from external file</summary>
//-----------------------------------------------------------------------

namespace GlobalNamespace
{
	using System;
	using System.Collections.Generic;
	using System.IO;
	using Newtonsoft.Json.Linq;

	public partial class Form1
	{
		private void ReadSettings()
		{
			// Path for user settings file
			string settingsConfigPath = AppDomain.CurrentDomain.BaseDirectory + @"config\settings.json";

			try
			{
				// Read user settings from settings.json
				string settingsJson = File.ReadAllText(settingsConfigPath);

				// Validate settings config. If any validation errors occurred, ValidateConfig will throw an exception which is catched later
				this.ValidateSchema(settingsJson, this.settingsSchemaStr);

				// Save settings to JObject for later access throughout the program
				User.Settings = JObject.Parse(settingsJson);
			}
			catch (Exception ex)
			{
				string[] errorMsg =
					{
					@"ERROR:    Failed to read user settings! Please close program and fix this first...",
					"Filepath: " + settingsConfigPath,
					"Message:  " + ex.Message.TrimEnd('\r', '\n')
				};
				this.PrintLogMessage(this.rtbErrorLog, errorMsg);
			}
		}
	}
}
