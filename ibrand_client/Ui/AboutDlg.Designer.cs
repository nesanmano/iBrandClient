using System.ComponentModel;
using System.Windows.Forms;

namespace NomadLogic.iBrandDesktop.Ui
{
    partial class AboutDlg
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
            this.version_label = new System.Windows.Forms.Label();
            this.company_label = new System.Windows.Forms.Label();
            this.copyright_label = new System.Windows.Forms.Label();
            this.SuspendLayout();
            // 
            // version_label
            // 
            this.version_label.Location = new System.Drawing.Point(2, 31);
            this.version_label.Name = "version_label";
            this.version_label.Size = new System.Drawing.Size(175, 20);
            this.version_label.TabIndex = 2;
            this.version_label.Text = "Version : ";
            this.version_label.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
            // 
            // company_label
            // 
            this.company_label.Location = new System.Drawing.Point(3, 10);
            this.company_label.Name = "company_label";
            this.company_label.Size = new System.Drawing.Size(174, 20);
            this.company_label.TabIndex = 1;
            this.company_label.Text = "Company :";
            this.company_label.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
            // 
            // copyright_label
            // 
            this.copyright_label.Location = new System.Drawing.Point(3, 51);
            this.copyright_label.Name = "copyright_label";
            this.copyright_label.Size = new System.Drawing.Size(174, 39);
            this.copyright_label.TabIndex = 0;
            this.copyright_label.Text = "Copyright";
            this.copyright_label.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
            // 
            // AboutDlg
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(96F, 96F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Dpi;
            this.AutoScroll = true;
            this.ClientSize = new System.Drawing.Size(180, 100);
            this.Controls.Add(this.copyright_label);
            this.Controls.Add(this.company_label);
            this.Controls.Add(this.version_label);
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedDialog;
            this.MaximizeBox = false;
            this.MinimizeBox = false;
            this.Name = "AboutDlg";
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterParent;
            this.Text = "iBrand (tm)";
            this.ResumeLayout(false);

        }

        #endregion

        private Label version_label;
        private Label company_label;
        private Label copyright_label;
    }
}