using System.ComponentModel;
using System.Windows.Forms;

namespace NomadLogic.iBrandDesktop.Ui.Synchro
{
    partial class GetProjectDlg
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
            this.user_name_label = new System.Windows.Forms.Label();
            this.userNameTextBox = new System.Windows.Forms.TextBox();
            this.pw_label = new System.Windows.Forms.Label();
            this.pwTextBox = new System.Windows.Forms.TextBox();
            this.messageTextBox = new System.Windows.Forms.TextBox();
            this.progressBar = new System.Windows.Forms.ProgressBar();
            this.SuspendLayout();
            // 
            // user_name_label
            // 
            this.user_name_label.Location = new System.Drawing.Point(7, 6);
            this.user_name_label.Name = "user_name_label";
            this.user_name_label.Size = new System.Drawing.Size(84, 20);
            this.user_name_label.TabIndex = 6;
            this.user_name_label.Text = "User name :";
            // 
            // userNameTextBox
            // 
            this.userNameTextBox.Location = new System.Drawing.Point(101, 6);
            this.userNameTextBox.Name = "userNameTextBox";
            this.userNameTextBox.Size = new System.Drawing.Size(132, 20);
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
            this.pwTextBox.Size = new System.Drawing.Size(132, 20);
            this.pwTextBox.TabIndex = 2;
            // 
            // messageTextBox
            // 
            this.messageTextBox.Location = new System.Drawing.Point(7, 57);
            this.messageTextBox.Multiline = true;
            this.messageTextBox.Name = "messageTextBox";
            this.messageTextBox.ReadOnly = true;
            this.messageTextBox.ScrollBars = System.Windows.Forms.ScrollBars.Vertical;
            this.messageTextBox.Size = new System.Drawing.Size(226, 143);
            this.messageTextBox.TabIndex = 4;
            // 
            // progressBar
            // 
            this.progressBar.Location = new System.Drawing.Point(7, 206);
            this.progressBar.Name = "progressBar";
            this.progressBar.Size = new System.Drawing.Size(226, 20);
            this.progressBar.TabIndex = 0;
            // 
            // GetProjectDlg
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(96F, 96F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Dpi;
            this.ClientSize = new System.Drawing.Size(240, 261);
            this.Controls.Add(this.progressBar);
            this.Controls.Add(this.messageTextBox);
            this.Controls.Add(this.pwTextBox);
            this.Controls.Add(this.pw_label);
            this.Controls.Add(this.userNameTextBox);
            this.Controls.Add(this.user_name_label);
            this.Name = "GetProjectDlg";
            this.Controls.SetChildIndex(this.user_name_label, 0);
            this.Controls.SetChildIndex(this.userNameTextBox, 0);
            this.Controls.SetChildIndex(this.pw_label, 0);
            this.Controls.SetChildIndex(this.pwTextBox, 0);
            this.Controls.SetChildIndex(this.messageTextBox, 0);
            this.Controls.SetChildIndex(this.progressBar, 0);
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


    }
}