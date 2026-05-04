using System.ComponentModel;
using System.Windows.Forms;

namespace NomadLogic.PrintStation.Ui.custom.Nestea
{
    partial class EmailDialog
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
            this.panel.Location = new System.Drawing.Point(1, 3);
            this.panel.Name = "panel";
            this.panel.Size = new System.Drawing.Size(777, 500);
            this.panel.TabIndex = 1;
            // 
            // kb_panel
            // 
            this.kb_panel.Location = new System.Drawing.Point(35, 348);
            this.kb_panel.Name = "kb_panel";
            this.kb_panel.Size = new System.Drawing.Size(790, 400);
            this.kb_panel.TabIndex = 2;
            // 
            // EmailDialog
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.BackColor = System.Drawing.Color.White;
            this.BackgroundImageLayout = System.Windows.Forms.ImageLayout.None;
            this.ClientSize = new System.Drawing.Size(800, 1280);
            this.ControlBox = false;
            this.Controls.Add(this.kb_panel);
            this.Controls.Add(this.panel);
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.None;
            this.Name = "EmailDialog";
            this.StartPosition = System.Windows.Forms.FormStartPosition.Manual;
            this.Text = "InputDialog";
            this.ResumeLayout(false);

        }

        #endregion

        private Panel panel;
        private Panel kb_panel;


    }
}