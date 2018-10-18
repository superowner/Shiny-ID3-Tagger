namespace Shiny_ID3_Tagger.Setting
{
    partial class Setting
    {
        /// <summary>
        /// Required designer variable.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary>
        /// Clean up any resources being used.
        /// </summary>
        /// <param name="disposing">true if managed resources should be disposed; otherwise, false.</param>
        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        #region Windows Form Designer generated code

        /// <summary>
        /// Required method for Designer support - do not modify
        /// the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            this.SettingLabel = new System.Windows.Forms.Label();
            this.UISettingCheckBox = new System.Windows.Forms.CheckBox();
            this.SuspendLayout();
            // 
            // SettingLabel
            // 
            this.SettingLabel.Dock = System.Windows.Forms.DockStyle.Top;
            this.SettingLabel.Font = new System.Drawing.Font("Arial", 12F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(177)));
            this.SettingLabel.Location = new System.Drawing.Point(0, 0);
            this.SettingLabel.Name = "SettingLabel";
            this.SettingLabel.Padding = new System.Windows.Forms.Padding(0, 17, 0, 17);
            this.SettingLabel.Size = new System.Drawing.Size(800, 60);
            this.SettingLabel.TabIndex = 0;
            this.SettingLabel.Text = "Setting";
            this.SettingLabel.TextAlign = System.Drawing.ContentAlignment.TopCenter;
            // 
            // UISettingCheckBox
            // 
            this.UISettingCheckBox.AutoSize = true;
            this.UISettingCheckBox.Checked = true;
            this.UISettingCheckBox.CheckState = System.Windows.Forms.CheckState.Checked;
            this.UISettingCheckBox.Font = new System.Drawing.Font("Arial Narrow", 10.2F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.UISettingCheckBox.Location = new System.Drawing.Point(13, 64);
            this.UISettingCheckBox.Name = "UISettingCheckBox";
            this.UISettingCheckBox.Size = new System.Drawing.Size(91, 26);
            this.UISettingCheckBox.TabIndex = 1;
            this.UISettingCheckBox.Text = "UI Setting";
            this.UISettingCheckBox.UseVisualStyleBackColor = true;
            this.UISettingCheckBox.CheckedChanged += new System.EventHandler(this.UISettingCheckBox_CheckedChanged);
            // 
            // Setting
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(8F, 16F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(800, 450);
            this.Controls.Add(this.UISettingCheckBox);
            this.Controls.Add(this.SettingLabel);
            this.Name = "Setting";
            this.Text = "Setting";
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.Label SettingLabel;
        private System.Windows.Forms.CheckBox UISettingCheckBox;
    }
}