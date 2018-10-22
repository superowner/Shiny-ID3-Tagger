using Newtonsoft.Json.Schema;

namespace Shiny_ID3_Tagger.Setting
{
    using System;
    using Newtonsoft.Json.Linq;

    public class CSharpSettingFile : BaseSetting
    {
        private System.Configuration.ApplicationSettingsBase setting;

        public CSharpSettingFile(JSchema defaultSettingSchema, System.Configuration.ApplicationSettingsBase setting)
            : base(defaultSettingSchema)
        {
            this.setting = setting ?? throw new ArgumentException(@"Setting Can't be null", nameof(setting));
        }

        public override dynamic GetValue(string key)
        {
            return this.setting.Context[key];
        }

        public override bool SetValue(string key, dynamic value)
        {
            this.setting.Context[key] = value;
            this.setting.Save();
            return true;
        }

        public override bool RemoveKey(string key)
        {
            this.setting.Context.Remove(key);
            return true;
        }

        public override void ResetToDefault()
        {
            throw new System.NotImplementedException();
        }
    }
}