using System.ComponentModel;
using System.Windows.Forms;

namespace NomadLogic.iBrandDesktop.Ui.Choice
{
    partial class CaptureTextBoxCanon
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
            this.captureButton = new System.Windows.Forms.Button();
            this.previewButton = new System.Windows.Forms.Button();
            this.printButton = new System.Windows.Forms.Button();
            this.livePreviewButton = new System.Windows.Forms.Button();
            this.status_text_box = new System.Windows.Forms.TextBox();
            this.clearButton = new System.Windows.Forms.Button();
            this.SuspendLayout();
            // 
            // textBox
            // 
            this.textBox.Location = new System.Drawing.Point(3, 6);
            this.textBox.Name = "textBox";
            this.textBox.Size = new System.Drawing.Size(75, 20);
            this.textBox.TabIndex = 0;
            // 
            // captureButton
            // 
            this.captureButton.Location = new System.Drawing.Point(197, 4);
            this.captureButton.Name = "captureButton";
            this.captureButton.Size = new System.Drawing.Size(65, 23);
            this.captureButton.TabIndex = 1;
            this.captureButton.Text = "Capture";
            this.captureButton.UseVisualStyleBackColor = true;
            // 
            // previewButton
            // 
            this.previewButton.Location = new System.Drawing.Point(264, 4);
            this.previewButton.Name = "previewButton";
            this.previewButton.Size = new System.Drawing.Size(60, 23);
            this.previewButton.TabIndex = 2;
            this.previewButton.Text = "Preview";
            this.previewButton.UseVisualStyleBackColor = true;
            // 
            // printButton
            // 
            this.printButton.Location = new System.Drawing.Point(374, 4);
            this.printButton.Name = "printButton";
            this.printButton.Size = new System.Drawing.Size(42, 23);
            this.printButton.TabIndex = 3;
            this.printButton.Text = "Print";
            this.printButton.UseVisualStyleBackColor = true;
            // 
            // livePreviewButton
            // 
            this.livePreviewButton.Location = new System.Drawing.Point(418, 4);
            this.livePreviewButton.Name = "livePreviewButton";
            this.livePreviewButton.Size = new System.Drawing.Size(38, 23);
            this.livePreviewButton.TabIndex = 4;
            this.livePreviewButton.Text = "Live";
            this.livePreviewButton.UseVisualStyleBackColor = true;
            // 
            // status_text_box
            // 
            this.status_text_box.Enabled = false;
            this.status_text_box.Location = new System.Drawing.Point(82, 6);
            this.status_text_box.Name = "status_text_box";
            this.status_text_box.Size = new System.Drawing.Size(113, 20);
            this.status_text_box.TabIndex = 5;
            // 
            // clearButton
            // 
            this.clearButton.Location = new System.Drawing.Point(327, 4);
            this.clearButton.Name = "clearButton";
            this.clearButton.Size = new System.Drawing.Size(42, 23);
            this.clearButton.TabIndex = 6;
            this.clearButton.Text = "Clear";
            this.clearButton.UseVisualStyleBackColor = true;
            // 
            // CaptureTextBoxCanon
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.Controls.Add(this.clearButton);
            this.Controls.Add(this.status_text_box);
            this.Controls.Add(this.livePreviewButton);
            this.Controls.Add(this.printButton);
            this.Controls.Add(this.previewButton);
            this.Controls.Add(this.captureButton);
            this.Controls.Add(this.textBox);
            this.Name = "CaptureTextBoxCanon";
            this.Size = new System.Drawing.Size(464, 29);
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        protected TextBox textBox;
        protected Button captureButton;
        protected Button previewButton;
        private Button printButton;
        private Button livePreviewButton;
        private TextBox status_text_box;
        private Button clearButton;
    }
}
