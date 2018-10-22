using System;
using System.IO;

namespace Shiny_ID3_Tagger.Setting
{
    using Newtonsoft.Json.Linq;

    public class JsonFileSetting : BaseSetting
    {
        private JObject currentSetting;

        public JsonFileSetting(JObject defaultSetting,
                               string filePath,
                               bool watchForChanges = true)
            : base(defaultSetting)
        {
            if (filePath == null || Utils.Utils.IsValidFilePath(filePath))
            {
                throw new ArgumentException(nameof(filePath));
            }

            // Create setting file if don't exits
            if (!File.Exists(filePath))
            {
                // TODO - CREATE FILE IF DON'T EXISTS AND WRITE THE DEFAULT SETTING
            }

            if (watchForChanges)
            {
                // TODO - ADD EVENTS FOR CHANGED DELETED AND MORE
                Utils.Utils.CreateFileWatcher(filePath, this.OnChanged, this.OnChanged, this.OnChanged, this.OnRenamed);
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
            throw new System.NotImplementedException();
        }

        // Define the event handlers.
        private void OnChanged(object source, FileSystemEventArgs e)
        {
            if (e.ChangeType.)
                // Specify what is done when a file is changed, created, or deleted.
                Console.WriteLine("File: " + e.FullPath + " " + e.ChangeType);
        }

        private void OnRenamed(object source, RenamedEventArgs e)
        {
            // Specify what is done when a file is renamed.
            Console.WriteLine("File: {0} renamed to {1}", e.OldFullPath, e.FullPath);
        }
    }
}