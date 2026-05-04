using System.ComponentModel;
using System.Windows.Forms;

namespace NomadLogic.iBrandDesktop.Ui.Choice
{
    partial class VideoGalleryTextBox
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
            this.textBox = new System.Windows.Forms.TextBox();
            this.chooseButton = new System.Windows.Forms.Button();
            this.previewButton = new System.Windows.Forms.Button();
            this.SuspendLayout();
            // 
            // textBox
            // 
            this.textBox.Location = new System.Drawing.Point(3, 6);
            this.textBox.Name = "textBox";
            this.textBox.Size = new System.Drawing.Size(203, 20);
            this.textBox.TabIndex = 0;
            // 
            // chooseButton
            // 
            this.chooseButton.Location = new System.Drawing.Point(212, 4);
            this.chooseButton.Name = "chooseButton";
            this.chooseButton.Size = new System.Drawing.Size(75, 23);
            this.chooseButton.TabIndex = 1;
            this.chooseButton.Text = "Choose";
            this.chooseButton.UseVisualStyleBackColor = true;
            // 
            // previewButton
            // 
            this.previewButton.Location = new System.Drawing.Point(290, 4);
            this.previewButton.Name = "previewButton";
            this.previewButton.Size = new System.Drawing.Size(75, 23);
            this.previewButton.TabIndex = 2;
            this.previewButton.Text = "Preview";
            this.previewButton.UseVisualStyleBackColor = true;
            // 
            // VideoGalleryTextBox
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.Controls.Add(this.previewButton);
            this.Controls.Add(this.chooseButton);
            this.Controls.Add(this.textBox);
            this.Name = "VideoGalleryTextBox";
            this.Size = new System.Drawing.Size(368, 29);
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private TextBox textBox;
        private Button chooseButton;
        private Button previewButton;
    }
}
