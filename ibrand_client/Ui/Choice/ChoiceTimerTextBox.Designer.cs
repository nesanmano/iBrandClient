using System.ComponentModel;
using System.Windows.Forms;

namespace NomadLogic.iBrandDesktop.Ui.Choice
{
    partial class ChoiceTimerTextBox
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
            this.text_box = new System.Windows.Forms.TextBox();
            this.start_button = new System.Windows.Forms.Button();
            this.reset_button = new System.Windows.Forms.Button();
            this.stop_button = new System.Windows.Forms.Button();
            this.SuspendLayout();
            // 
            // text_box
            // 
            this.text_box.Enabled = false;
            this.text_box.Location = new System.Drawing.Point(0, 6);
            this.text_box.Name = "text_box";
            this.text_box.Size = new System.Drawing.Size(160, 20);
            this.text_box.TabIndex = 0;
            // 
            // start_button
            // 
            this.start_button.Location = new System.Drawing.Point(166, 4);
            this.start_button.Name = "start_button";
            this.start_button.Size = new System.Drawing.Size(75, 23);
            this.start_button.TabIndex = 1;
            this.start_button.Text = "Start";
            this.start_button.UseVisualStyleBackColor = true;
            // 
            // reset_button
            // 
            this.reset_button.Location = new System.Drawing.Point(328, 4);
            this.reset_button.Name = "reset_button";
            this.reset_button.Size = new System.Drawing.Size(75, 23);
            this.reset_button.TabIndex = 2;
            this.reset_button.Text = "Reset";
            this.reset_button.UseVisualStyleBackColor = true;
            // 
            // stop_button
            // 
            this.stop_button.Location = new System.Drawing.Point(247, 4);
            this.stop_button.Name = "stop_button";
            this.stop_button.Size = new System.Drawing.Size(75, 23);
            this.stop_button.TabIndex = 3;
            this.stop_button.Text = "Stop";
            this.stop_button.UseVisualStyleBackColor = true;
            // 
            // ChoiceTimerTextBox
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.Controls.Add(this.stop_button);
            this.Controls.Add(this.reset_button);
            this.Controls.Add(this.start_button);
            this.Controls.Add(this.text_box);
            this.Name = "ChoiceTimerTextBox";
            this.Size = new System.Drawing.Size(413, 32);
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private TextBox text_box;
        private Button start_button;
        private Button reset_button;
        private Button stop_button;
    }
}
