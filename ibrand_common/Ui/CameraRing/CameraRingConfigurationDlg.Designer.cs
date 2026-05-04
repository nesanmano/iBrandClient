using System.ComponentModel;
using System.Windows.Forms;

namespace NomadLogic.Common.Ui.CameraRing
{
    partial class CameraRingConfigurationDlg
    {
        /// <summary>
        /// Required designer variable.
        /// </summary>
        private IContainer components = null;

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
            var resources = new System.ComponentModel.ComponentResourceManager(typeof(CameraRingConfigurationDlg));
            this.pictures_panel = new System.Windows.Forms.Panel();
            this.SuspendLayout();
            // 
            // pictures_panel
            // 
            this.pictures_panel.Location = new System.Drawing.Point(0, 1);
            this.pictures_panel.Name = "pictures_panel";
            this.pictures_panel.Size = new System.Drawing.Size(644, 135);
            this.pictures_panel.TabIndex = 0;
            // 
            // CameraRingConfigurationDlg
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.AutoScroll = true;
            this.BackColor = System.Drawing.SystemColors.ControlLightLight;
            this.ClientSize = new System.Drawing.Size(644, 136);
            this.Controls.Add(this.pictures_panel);
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedDialog;
            this.Icon = ((System.Drawing.Icon)(resources.GetObject("$this.Icon")));
            this.MaximizeBox = false;
            this.MinimizeBox = false;
            this.Name = "CameraRingConfigurationDlg";
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterParent;
            this.Text = "iBrand(tm)";
            this.ResumeLayout(false);

        }

        #endregion

        private Panel pictures_panel;
    }
}