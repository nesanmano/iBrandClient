using System.ComponentModel;
using System.Windows.Forms;

namespace NomadLogic.iBrandDesktop.Ui.Choice
{
    partial class CheckTextBox
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
            this.checkBox = new System.Windows.Forms.CheckBox();
            this.label = new System.Windows.Forms.Label();
            this.SuspendLayout();
            // 
            // checkBox
            // 
            this.checkBox.Location = new System.Drawing.Point(0, 1);
            this.checkBox.Name = "checkBox";
            this.checkBox.Size = new System.Drawing.Size(21, 24);
            this.checkBox.TabIndex = 0;
            // 
            // label
            // 
            this.label.Location = new System.Drawing.Point(15, 5);
            this.label.Name = "label";
            this.label.Size = new System.Drawing.Size(179, 16);
            this.label.TabIndex = 0;
            this.label.Text = "all";
            // 
            // CheckTextBox
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(96F, 96F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Dpi;
            this.Controls.Add(this.label);
            this.Controls.Add(this.checkBox);
            this.Name = "CheckTextBox";
            this.Size = new System.Drawing.Size(200, 24);
            this.ResumeLayout(false);

        }

        #endregion

        private CheckBox checkBox;
        private Label label;
    }
}
