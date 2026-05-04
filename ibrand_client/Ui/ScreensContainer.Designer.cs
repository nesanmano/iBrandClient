using System.ComponentModel;

namespace NomadLogic.iBrandDesktop.Ui
{
    partial class ScreensContainer
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

        #region Component Designer generated code

        /// <summary> 
        /// Required method for Designer support - do not modify 
        /// the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            this.ScreenPanel = new System.Windows.Forms.Panel();
            this.SuspendLayout();
            // 
            // screen_panel
            // 
            this.ScreenPanel.BackColor = System.Drawing.SystemColors.ControlLightLight;
            this.ScreenPanel.Location = new System.Drawing.Point(0, 0);
            this.ScreenPanel.Name = "ScreenPanel";
            this.ScreenPanel.Size = new System.Drawing.Size(140, 264);
            this.ScreenPanel.TabIndex = 0;
            // 
            // ScreensContainer
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.BackColor = System.Drawing.SystemColors.ControlLightLight;
            this.Controls.Add(this.ScreenPanel);
            this.Name = "ScreensContainer";
            this.Size = new System.Drawing.Size(226, 313);
            this.ResumeLayout(false);

        }

        #endregion
    }
}
