using System.Drawing;
using System.Windows.Forms;

namespace NomadLogic.iBrandDesktop.Ui.Synchro.New_UI
{
    partial class GetProjectTerminal
    {
        /// <summary>
        /// Required designer variable.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

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
            this.startButton = new System.Windows.Forms.Button();
            this.cancelButton = new System.Windows.Forms.Button();
            this.user_name_label = new System.Windows.Forms.Label();
            this.userNameTextBox = new System.Windows.Forms.TextBox();
            this.pw_label = new System.Windows.Forms.Label();
            this.pwTextBox = new System.Windows.Forms.TextBox();
            this.messageTextBox = new System.Windows.Forms.TextBox();
            this.progressBar = new System.Windows.Forms.ProgressBar();
            this.progressBar2 = new System.Windows.Forms.ProgressBar();
            this.SuspendLayout();
            // 
            // startButton
            // 
            this.startButton.Location = new System.Drawing.Point(10, 454);
            this.startButton.Name = "startButton";
            this.startButton.Size = new System.Drawing.Size(160, 23);
            this.startButton.TabIndex = 3;
            this.startButton.Text = "Start";
            this.startButton.UseVisualStyleBackColor = true;
            // 
            // cancelButton
            // 
            this.cancelButton.Location = new System.Drawing.Point(226, 453);
            this.cancelButton.Name = "cancelButton";
            this.cancelButton.Size = new System.Drawing.Size(157, 23);
            this.cancelButton.TabIndex = 4;
            this.cancelButton.Text = "Cancel";
            this.cancelButton.UseVisualStyleBackColor = true;
            // 
            // user_name_label
            // 
            this.user_name_label.Location = new System.Drawing.Point(7, 6);
            this.user_name_label.Name = "user_name_label";
            this.user_name_label.Size = new System.Drawing.Size(84, 20);
            this.user_name_label.TabIndex = 6;
            this.user_name_label.Text = "Username :";
            // 
            // userNameTextBox
            // 
            this.userNameTextBox.Location = new System.Drawing.Point(101, 6);
            this.userNameTextBox.Name = "userNameTextBox";
            this.userNameTextBox.Size = new System.Drawing.Size(282, 20);
            this.userNameTextBox.TabIndex = 0;
            // 
            // pw_label
            // 
            this.pw_label.Location = new System.Drawing.Point(7, 31);
            this.pw_label.Name = "pw_label";
            this.pw_label.Size = new System.Drawing.Size(84, 20);
            this.pw_label.TabIndex = 5;
            this.pw_label.Text = "Password :";
            // 
            // pwTextBox
            // 
            this.pwTextBox.Location = new System.Drawing.Point(101, 30);
            this.pwTextBox.Name = "pwTextBox";
            this.pwTextBox.Size = new System.Drawing.Size(282, 20);
            this.pwTextBox.TabIndex = 2;
            // 
            // messageTextBox
            // 
            this.messageTextBox.BackColor = System.Drawing.Color.White;
            this.messageTextBox.Font = new System.Drawing.Font("Liberation Mono", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.messageTextBox.Location = new System.Drawing.Point(12, 97);
            this.messageTextBox.Multiline = true;
            this.messageTextBox.Name = "messageTextBox";
            this.messageTextBox.ReadOnly = true;
            this.messageTextBox.ScrollBars = System.Windows.Forms.ScrollBars.Vertical;
            this.messageTextBox.Size = new System.Drawing.Size(371, 256);
            this.messageTextBox.TabIndex = 4;
            // 
            // progressBar
            // 
            this.progressBar.Location = new System.Drawing.Point(12, 373);
            this.progressBar.Name = "progressBar";
            this.progressBar.Size = new System.Drawing.Size(371, 20);
            this.progressBar.Style = System.Windows.Forms.ProgressBarStyle.Continuous;
            this.progressBar.TabIndex = 0;
            // 
            // progressBar2
            // 
            this.progressBar2.ForeColor = System.Drawing.Color.DarkGreen;
            this.progressBar2.Location = new System.Drawing.Point(12, 410);
            this.progressBar2.Name = "progressBar2";
            this.progressBar2.Size = new System.Drawing.Size(371, 20);
            this.progressBar2.Style = System.Windows.Forms.ProgressBarStyle.Continuous;
            this.progressBar2.TabIndex = 0;
            // 
            // GetProject2
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(395, 488);
            this.Controls.Add(this.progressBar);
            this.Controls.Add(this.progressBar2);
            this.Controls.Add(this.messageTextBox);
            this.Controls.Add(this.pwTextBox);
            this.Controls.Add(this.pw_label);
            this.Controls.Add(this.userNameTextBox);
            this.Controls.Add(this.user_name_label);
            this.Controls.Add(this.cancelButton);
            this.Controls.Add(this.startButton);
            this.Name = "GetProjectTerminal";
            this.Text = "GetProject2";
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion


        private Label user_name_label;
        private TextBox userNameTextBox;
        private Label pw_label;
        private TextBox pwTextBox;
        //private OpenNETCF.Windows.Forms.TextBox2 messageTextBox;
        private TextBox messageTextBox;
        private ProgressBar progressBar;
        private ProgressBar progressBar2;



        private Button startButton;
        private Button cancelButton;
    }
}