using System.Linq;
using System.Windows.Forms;

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
        private string defaultFilePath;

        public Action<JObject> SettingChangedCallback;

        public JsonFileSetting(JObject defaultSetting,
                               JSchema schema,
                               string filePath,
                               string defaultFilePath,
                               Action<JObject> callback = null)
            : base(defaultSetting, schema)
        {
            this.SettingChangedCallback = callback;
            if (filePath == null)
            {
                filePath = AppDomain.CurrentDomain.BaseDirectory + @"config\settings.json";
            }

            if (defaultFilePath == null)
            {
                defaultFilePath = AppDomain.CurrentDomain.BaseDirectory + @"config\settings-default.json";
            }

            if (!Utils.Utils.IsValidFilePath(filePath))
            {
                throw new ArgumentException(nameof(filePath));
            }

            if (!Utils.Utils.IsValidFilePath(defaultFilePath))
            {
                throw new ArgumentException(nameof(defaultFilePath));
            }

            this.filePath = filePath;
            this.defaultFilePath = filePath;

            if (this.DefaultSetting == null)
            {
                string json = File.ReadAllText(this.defaultFilePath);
                this.DefaultSetting = JObject.Parse(json);
            }

            // Create setting file if don't exits
            if (!File.Exists(filePath))
            {
                this.SetSettingFileContent(DefaultSetting);
            }
            else
            {
                string json = File.ReadAllText(this.filePath);
                this.currentSetting = JObject.Parse(json);
            }

            // TODO - ADD EVENTS FOR RENAME FOR RETURNING THE NAME TO THE REGULAR NAME
//            Utils.Utils.CreateFileWatcher(this.filePath, new FileSystemEventHandler(this.OnChanged), new FileSystemEventHandler(this.OnDeleted));
        }

        public override dynamic GetValue(string key)
        {
            if (this.CurrentSetting == null)
            {
                return null;
            }

            if (this.CurrentSetting.TryGetValue(key, out JToken value))
            {
                // TODO - CONVERT TO THE VALUE
                return value;
            }

            return null;
        }

        public override bool SetValue(string key, dynamic value)
        {
            if (this.CurrentSetting == null)
            {
                return false;
            }

            this.CurrentSetting[key] = value;
            bool res = this.SetSettingFileContent(this.CurrentSetting);

            if (res)
            {
                this.SettingChangedCallback(this.CurrentSetting);
            }

            return res;
        }

        public override bool RemoveKey(string key)
        {
            this.CurrentSetting.Remove(key);
            bool res = this.SetSettingFileContent(this.CurrentSetting);

            if (res)
            {
                this.SettingChangedCallback(this.CurrentSetting);
            }

            return res;
        }

        public override void ResetToDefault()
        {
            bool res = this.SetSettingFileContent(this.DefaultSetting);

            if (res)
            {
                this.SettingChangedCallback(this.CurrentSetting);
            }
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

                try
                {
                    if (this.SettingSchema != null)
                    {
                        // Validate settings config. If any validation errors occurred, ValidateConfig will throw an exception which is catched later
                        Utils.Utils.ValidateSchema(settingsJson, this.SettingSchema);
                    }
                }
                catch (Exception ex)
                {
                    MessageBox.Show(ex.ToString());
                    Console.WriteLine(ex);
                }

                // Save settings to JObject for later access throughout the program
                this.CurrentSetting = JObject.Parse(settingsJson);
            }
            else
            {
                Console.WriteLine("Hello", e.ChangeType);

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
                this.SetSettingFileContent(this.CurrentSetting ?? this.DefaultSetting);
            }
        }

        /// <summary>
        /// Set Setting File data
        /// </summary>
        /// <param name="objectToWrite">Object to write to file</param>
        /// <returns>Return the result of writing</returns>
        private bool SetSettingFileContent(JObject objectToWrite)
        {
            if (!File.Exists(this.filePath))
            {
                FileStream str = File.Create(filePath);
                str.Close();
            }

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