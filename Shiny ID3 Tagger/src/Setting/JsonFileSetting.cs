namespace Shiny_ID3_Tagger.Setting
{
    using System;
    using System.IO;
    using Newtonsoft.Json;
    using Newtonsoft.Json.Linq;
    using Newtonsoft.Json.Schema;

    public class JsonFileSetting : BaseSetting
    {
        private JObject currentSetting;

        private JObject CurrentSetting
        {
            get => currentSetting;
            set
            {
                currentSetting = value;
                this.SettingChangedCallback?.Invoke(value);
            }
        }

        private string filePath;

        public Action<JObject> SettingChangedCallback;

        public JsonFileSetting(JSchema defaultSettingSchema,
                               string filePath,
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
                this.SetSettingFileContent(JObject.Parse(defaultSettingSchema.ToString()));
            }
            else
            {
                string json = File.ReadAllText(this.filePath);
                this.CurrentSetting = JObject.Parse(json);
            }

            // TODO - ADD EVENTS FOR RENAME FOR RETURNING THE NAME TO THE REGULAR NAME
            Utils.Utils.CreateFileWatcher(filePath, this.OnChanged, this.OnDeleted);
        }

        public override dynamic GetValue(string key)
        {
            return CurrentSetting[key];
        }

        public override bool SetValue(string key, dynamic value)
        {
            CurrentSetting[key] = value;
            return this.SetSettingFileContent(CurrentSetting);
        }

        public override bool RemoveKey(string key)
        {
            CurrentSetting.Remove(key);
            return this.SetSettingFileContent(CurrentSetting);
        }

        public override void ResetToDefault()
        {
            this.SetSettingFileContent(JObject.Parse(this.DefaultSettingSchemaSchema.ToString()));
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
                SetSettingFileContent(CurrentSetting ?? this.DefaultSettingSchemaSchema);
            }
        }

        /// <summary>
        /// Set Setting File data
        /// </summary>
        /// <param name="objectToWrite">Object to write to file</param>
        /// <returns>Return the result of writing</returns>
        private bool SetSettingFileContent(JObject objectToWrite)
        {
            try
            {
                File.WriteAllText(this.filePath, JsonConvert.SerializeObject(objectToWrite, Formatting.Indented));
            }
            catch (Exception e)
            {
                // TODO - Print Error in custom console
                return false;
            }

            return true;
        }
    }
}