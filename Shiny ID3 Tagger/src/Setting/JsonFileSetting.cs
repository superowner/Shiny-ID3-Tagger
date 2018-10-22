using Newtonsoft.Json;

namespace Shiny_ID3_Tagger.Setting
{
    using System;
    using System.IO;
    using Newtonsoft.Json.Linq;
    using Newtonsoft.Json.Schema;

    public class JsonFileSetting : BaseSetting
    {
        public JObject CurrentSetting
        {
            get;
            private set { this.SettingChangedCallback?.Invoke(value); }
        }

        private string filePath;

        public Action<JObject> SettingChangedCallback;

        public JsonFileSetting(JSchema defaultSettingSchema,
                               string filePath,
                               bool watchForChanges = true,
                               Action<JObject> callback = null)
            : base(defaultSettingSchema)
        {
            this.SettingChangedCallback = callback;
            if (filePath == null)
            {
                filePath = AppDomain.CurrentDomain.BaseDirectory + @"config\settings.json";
            }

            if (Utils.Utils.IsValidFilePath(filePath))
            {
                throw new ArgumentException(nameof(filePath));
            }

            this.filePath = filePath;

            // Create setting file if don't exits
            if (!File.Exists(filePath))
            {
                // TODO - CREATE FILE IF DON'T EXISTS AND WRITE THE DEFAULT SETTING
            }

            if (watchForChanges)
            {
                // TODO - ADD EVENTS FOR RENAME FOR RETURNING THE NAME TO THE REGULAR NAME
                Utils.Utils.CreateFileWatcher(filePath, this.OnChanged, this.OnDeleted);
            }
        }

        public override dynamic GetValue(string key)
        {
            throw new System.NotImplementedException();
        }

        public override bool SetValue(string key, dynamic value)
        {
            throw new System.NotImplementedException();
        }

        public override bool RemoveKey(string key)
        {
            throw new System.NotImplementedException();
        }

        public override void ResetToDefault()
        {
            SetSettingFileContent(JObject.Parse(this.DefaultSettingSchemaSchema.ToString()));
        }

        /// <summary>
        /// On File Changed Handler
        /// </summary>
        /// <param name="source">Source of the event</param>
        /// <param name="e">Event</param>
        private void OnChanged(object source, FileSystemEventArgs e)
        {
            if ((e.ChangeType & WatcherChangeTypes.Changed) != 0)
            {
                // Read user settings from settings.json
                string settingsJson = File.ReadAllText(this.filePath);

                // Validate settings config. If any validation errors occurred, ValidateConfig will throw an exception which is catched later
                Utils.Utils.ValidateSchema(settingsJson, this.DefaultSettingSchemaSchema);

                // Save settings to JObject for later access throughout the program
                this.CurrentSetting = JObject.Parse(settingsJson);
            }
        }

        /// <summary>
        /// On File Deleted Handler
        /// </summary>
        /// <param name="source">Source of the event</param>
        /// <param name="e">Event</param>
        private void OnDeleted(object source, FileSystemEventArgs e)
        {
            if ((e.ChangeType & WatcherChangeTypes.Deleted) != 0)
            {
                SetSettingFileContent(SetSettingFileContent ?? this.DefaultSettingSchemaSchema);
            }
        }

        /// <summary>
        /// Set Setting File data
        /// </summary>
        /// <param name="objectToWrite">Object to write to file</param>
        private void SetSettingFileContent(JObject objectToWrite)
        {
            if (!File.Exists(this.filePath))
            {
                File.WriteAllText(this.filePath, objectToWrite.ToString());

                // Write JSON directly to a file
                using (StreamWriter file = File.CreateText(this.filePath))
                {
                    using (JsonTextWriter writer = new JsonTextWriter(file))
                    {
                        objectToWrite.WriteTo(writer);
                    }
                }
            }
        }
    }
}