namespace Shiny_ID3_Tagger.Setting
{
    using System;

    public class CSharpSettingFile : ISetting
    {
        private global::System.Configuration.ApplicationSettingsBase _setting;
        private dynamic _defaultSettings;

        public CSharpSettingFile(global::System.Configuration.ApplicationSettingsBase setting, dynamic defaultSetting)
        {
            this._setting = setting ?? throw new ArgumentException(@"Setting Can't be null", nameof(setting));
            this._defaultSettings = defaultSetting;
        }

        public dynamic GetValue(string key)
        {
            return this._setting.Context[key];
        }

        public bool SetValue(string key, dynamic value)
        {
            this._setting.Context[key] = value;
            return true;
        }

        public bool RemoveKey(string key)
        {
            this._setting.Context.Remove(key);
            return true;
        }

        public void ResetToDefault()
        {
            throw new System.NotImplementedException();
        }
    }
}