using System.ComponentModel;
using System.Windows.Forms;

namespace NomadLogic.iBrandDesktop.Ui.ApplicationScreen
{
    partial class SmsConfirmDialog
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
            this.panel = new System.Windows.Forms.Panel();
            this.kb_panel = new System.Windows.Forms.Panel();
            this.SuspendLayout();
            // 
            // panel
            // 
            this.panel.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            this.panel.Location = new System.Drawing.Point(1, 4);
            this.panel.Name = "panel";
            this.panel.Size = new System.Drawing.Size(571, 322);
            this.panel.TabIndex = 0;
            // 
            // kb_panel
            // 
            this.kb_panel.BackColor = System.Drawing.Color.White;
            this.kb_panel.Location = new System.Drawing.Point(1, 329);
            this.kb_panel.Name = "kb_panel";
            this.kb_panel.Size = new System.Drawing.Size(571, 372);
            this.kb_panel.TabIndex = 1;
            // 
            // SmsConfirmDialog
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.BackColor = System.Drawing.Color.White;
            this.ClientSize = new System.Drawing.Size(584, 706);
            this.ControlBox = false;
            this.Controls.Add(this.kb_panel);
            this.Controls.Add(this.panel);
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.None;
            this.Name = "SmsConfirmDialog";
            this.StartPosition = System.Windows.Forms.FormStartPosition.Manual;
            this.Text = "SmsConfirmDialog";
            this.ResumeLayout(false);

        }

        #endregion

        private Panel panel;
        private Panel kb_panel;
    }
}