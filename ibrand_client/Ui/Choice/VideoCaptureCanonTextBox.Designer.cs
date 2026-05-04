using System.ComponentModel;
using System.Windows.Forms;

namespace NomadLogic.iBrandDesktop.Ui.Choice
{
    partial class VideoCaptureCanonTextBox
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
            this.previewButton = new System.Windows.Forms.Button();
            this.startStopButton = new System.Windows.Forms.Button();
            this.SuspendLayout();
            // 
            // textBox
            // 
            this.textBox.Location = new System.Drawing.Point(3, 4);
            this.textBox.Name = "textBox";
            this.textBox.Size = new System.Drawing.Size(143, 20);
            this.textBox.TabIndex = 0;
            // 
            // previewButton
            // 
            this.previewButton.Location = new System.Drawing.Point(177, 4);
            this.previewButton.Margin = new System.Windows.Forms.Padding(2, 2, 2, 2);
            this.previewButton.Name = "previewButton";
            this.previewButton.Size = new System.Drawing.Size(58, 23);
            this.previewButton.TabIndex = 1;
            this.previewButton.Text = "Preview";
            this.previewButton.UseVisualStyleBackColor = true;
            // 
            // startStopButton
            // 
            this.startStopButton.Location = new System.Drawing.Point(249, 4);
            this.startStopButton.Name = "startStopButton";
            this.startStopButton.Size = new System.Drawing.Size(62, 22);
            this.startStopButton.TabIndex = 2;
            this.startStopButton.Text = "Start";
            this.startStopButton.UseVisualStyleBackColor = true;
            // 
            // VideoCaptureCanonTextBox
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.Controls.Add(this.startStopButton);
            this.Controls.Add(this.previewButton);
            this.Controls.Add(this.textBox);
            this.Name = "VideoCaptureCanonTextBox";
            this.Size = new System.Drawing.Size(333, 29);
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private TextBox textBox;
        private Button previewButton;
        private Button startStopButton;
    }
}
