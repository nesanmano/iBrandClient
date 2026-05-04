using System.ComponentModel;
using System.Windows.Forms;

namespace NomadLogic.PrintStation.Ui
{
    partial class LoginControl
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
            this.center_panel = new System.Windows.Forms.Panel();
            this.cant_log_label = new System.Windows.Forms.Label();
            this.login_button = new System.Windows.Forms.Button();
            this.pw_text_box = new System.Windows.Forms.TextBox();
            this.label2 = new System.Windows.Forms.Label();
            this.label1 = new System.Windows.Forms.Label();
            this.username_text_box = new System.Windows.Forms.TextBox();
            this.center_panel.SuspendLayout();
            this.SuspendLayout();
            // 
            // center_panel
            // 
            this.center_panel.Controls.Add(this.cant_log_label);
            this.center_panel.Controls.Add(this.login_button);
            this.center_panel.Controls.Add(this.pw_text_box);
            this.center_panel.Controls.Add(this.label2);
            this.center_panel.Controls.Add(this.label1);
            this.center_panel.Controls.Add(this.username_text_box);
            this.center_panel.Location = new System.Drawing.Point(311, 99);
            this.center_panel.Name = "center_panel";
            this.center_panel.Size = new System.Drawing.Size(514, 250);
            this.center_panel.TabIndex = 6;
            // 
            // cant_log_label
            // 
            this.cant_log_label.AutoSize = true;
            this.cant_log_label.Font = new System.Drawing.Font("Microsoft Sans Serif", 12F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.cant_log_label.ForeColor = System.Drawing.Color.Red;
            this.cant_log_label.Location = new System.Drawing.Point(146, 36);
            this.cant_log_label.Name = "cant_log_label";
            this.cant_log_label.Size = new System.Drawing.Size(211, 20);
            this.cant_log_label.TabIndex = 11;
            this.cant_log_label.Text = "Wrong username / password";
            this.cant_log_label.Visible = false;
            // 
            // login_button
            // 
            this.login_button.Font = new System.Drawing.Font("Arial", 9.75F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.login_button.Location = new System.Drawing.Point(214, 180);
            this.login_button.Name = "login_button";
            this.login_button.Size = new System.Drawing.Size(75, 30);
            this.login_button.TabIndex = 10;
            this.login_button.Text = "Login";
            this.login_button.UseVisualStyleBackColor = true;
            // 
            // pw_text_box
            // 
            this.pw_text_box.Location = new System.Drawing.Point(237, 131);
            this.pw_text_box.Name = "pw_text_box";
            this.pw_text_box.Size = new System.Drawing.Size(120, 20);
            this.pw_text_box.TabIndex = 9;
            // 
            // label2
            // 
            this.label2.AutoSize = true;
            this.label2.Font = new System.Drawing.Font("Microsoft Sans Serif", 9.75F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.label2.Location = new System.Drawing.Point(147, 132);
            this.label2.Name = "label2";
            this.label2.Size = new System.Drawing.Size(74, 16);
            this.label2.TabIndex = 8;
            this.label2.Text = "Password :";
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Font = new System.Drawing.Font("Microsoft Sans Serif", 9.75F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.label1.Location = new System.Drawing.Point(147, 89);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(83, 16);
            this.label1.TabIndex = 7;
            this.label1.Text = "User Name :";
            // 
            // username_text_box
            // 
            this.username_text_box.Location = new System.Drawing.Point(237, 88);
            this.username_text_box.Name = "username_text_box";
            this.username_text_box.Size = new System.Drawing.Size(120, 20);
            this.username_text_box.TabIndex = 6;
            // 
            // LoginControl
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.BackColor = System.Drawing.Color.White;
            this.Controls.Add(this.center_panel);
            this.Name = "LoginControl";
            this.Size = new System.Drawing.Size(1111, 718);
            this.center_panel.ResumeLayout(false);
            this.center_panel.PerformLayout();
            this.ResumeLayout(false);

        }

        #endregion

        private Panel center_panel;
        private Label cant_log_label;
        private Button login_button;
        private TextBox pw_text_box;
        private Label label2;
        private Label label1;
        private TextBox username_text_box;

    }
}
