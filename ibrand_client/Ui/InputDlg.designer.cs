using System.ComponentModel;
using System.Windows.Forms;

namespace NomadLogic.iBrandDesktop.Ui
{
    partial class InputDlg
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
            this.label = new System.Windows.Forms.Label();
            this.ok_button = new System.Windows.Forms.Button();
            this.cancel_button = new System.Windows.Forms.Button();
            this.inputTextBox = new System.Windows.Forms.TextBox();
            this.SuspendLayout();
            // 
            // label
            // 
            this.label.Location = new System.Drawing.Point(5, 5);
            this.label.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
            this.label.Name = "label";
            this.label.Size = new System.Drawing.Size(191, 25);
            this.label.TabIndex = 4;
            this.label.Text = "Password :";
            // 
            // ok_button
            // 
            this.ok_button.Location = new System.Drawing.Point(9, 64);
            this.ok_button.Margin = new System.Windows.Forms.Padding(4, 4, 4, 4);
            this.ok_button.Name = "ok_button";
            this.ok_button.Size = new System.Drawing.Size(92, 25);
            this.ok_button.TabIndex = 2;
            this.ok_button.Text = "Ok";
            // 
            // cancel_button
            // 
            this.cancel_button.Location = new System.Drawing.Point(109, 64);
            this.cancel_button.Margin = new System.Windows.Forms.Padding(4, 4, 4, 4);
            this.cancel_button.Name = "cancel_button";
            this.cancel_button.Size = new System.Drawing.Size(92, 25);
            this.cancel_button.TabIndex = 3;
            this.cancel_button.Text = "Cancel";
            // 
            // inputTextBox
            // 
            this.inputTextBox.Location = new System.Drawing.Point(5, 26);
            this.inputTextBox.Margin = new System.Windows.Forms.Padding(4, 4, 4, 4);
            this.inputTextBox.Name = "inputTextBox";
            this.inputTextBox.Size = new System.Drawing.Size(195, 22);
            this.inputTextBox.TabIndex = 1;
            // 
            // InputDlg
            // 
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.None;
            this.AutoScroll = true;
            this.ClientSize = new System.Drawing.Size(209, 96);
            this.ControlBox = false;
            this.Controls.Add(this.inputTextBox);
            this.Controls.Add(this.cancel_button);
            this.Controls.Add(this.ok_button);
            this.Controls.Add(this.label);
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedDialog;
            this.Margin = new System.Windows.Forms.Padding(4, 4, 4, 4);
            this.MaximizeBox = false;
            this.MinimizeBox = false;
            this.Name = "InputDlg";
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterParent;
            this.Text = "iBrand(tm)";
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private Label label;
        private Button ok_button;
        private Button cancel_button;
        private TextBox inputTextBox;
        //private Microsoft.WindowsCE.Forms.InputPanel inputPanel;
    }
}