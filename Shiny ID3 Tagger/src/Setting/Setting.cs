using Newtonsoft.Json.Schema;

namespace Shiny_ID3_Tagger.Setting
{
    using System;
    using System.Windows.Forms;

    public partial class Setting : Form
    {
        JsonFileSetting setting = new JsonFileSetting(null, JSchema.Parse(Utils.Utils.settingsSchemaStr), null, null);

        public Setting()
        {
            InitializeComponent();
        }

        private void UISettingCheckBox_CheckedChanged(object sender, EventArgs e)
        {
            this.setting.SetValue("ShowUISetting", !this.setting.GetValue("ShowUISetting"));
        }
    }
}
