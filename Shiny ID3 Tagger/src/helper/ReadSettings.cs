//-----------------------------------------------------------------------
// <copyright file="ReadSettings.cs" company="Shiny ID3 Tagger">
// Copyright (c) Shiny ID3 Tagger. All rights reserved.
// </copyright>
// <author>ShinyId3Tagger Team</author>
// <summary>Gets custom user settings from external file</summary>
//-----------------------------------------------------------------------

namespace Utils
{
    using System;
    using System.IO;
    using GlobalNamespace;
    using GlobalVariables;
    using Newtonsoft.Json.Linq;

    internal partial class Utils
    {
        internal static void ReadSettings()
        {
            // Path for user settings file
            string settingsConfigPath = AppDomain.CurrentDomain.BaseDirectory + @"config\settings.json";

            try
            {
                // Read user settings from settings.json
                string settingsJson = File.ReadAllText(settingsConfigPath);

                // Validate settings config. If any validation errors occurred, ValidateConfig will throw an exception which is catched later
                ValidateSchema(settingsJson, settingsSchemaStr);

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
                Form1.Instance.PrintErrorMessage(errorMsg);
            }
        }
    }
}
