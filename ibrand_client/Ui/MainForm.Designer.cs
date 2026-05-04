using System.ComponentModel;
using System.Windows.Forms;

namespace NomadLogic.iBrandDesktop.Ui
{
    partial class MainForm
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
            this.screen_panel = new System.Windows.Forms.Panel();
            this.SuspendLayout();
            // 
            // screen_panel
            // 
            this.screen_panel.Location = new System.Drawing.Point(-1, 0);
            this.screen_panel.Name = "screen_panel";
            this.screen_panel.Size = new System.Drawing.Size(1920, 1080);
            this.screen_panel.TabIndex = 0;
            // 
            // MainForm
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.BackColor = System.Drawing.SystemColors.Desktop;
            this.ClientSize = new System.Drawing.Size(1914, 1045);
            this.Controls.Add(this.screen_panel);
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedSingle;
            this.MinimizeBox = false;
            this.Name = "MainForm";
            this.Text = "iBrand(tm)";
            this.ResumeLayout(false);

        }

        #endregion

        private Panel screen_panel;
    }
}