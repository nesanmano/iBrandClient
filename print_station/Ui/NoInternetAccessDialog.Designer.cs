using System.ComponentModel;
using System.Windows.Forms;

namespace NomadLogic.PrintStation.Ui
{
    partial class NoInternetAccessDialog
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
            this.dont_show_again_checkbox = new System.Windows.Forms.CheckBox();
            this.label1 = new System.Windows.Forms.Label();
            this.ok_button = new System.Windows.Forms.Button();
            this.error_picture_box = new System.Windows.Forms.PictureBox();
            ((System.ComponentModel.ISupportInitialize)(this.error_picture_box)).BeginInit();
            this.SuspendLayout();
            // 
            // dont_show_again_checkbox
            // 
            this.dont_show_again_checkbox.AutoSize = true;
            this.dont_show_again_checkbox.Font = new System.Drawing.Font("Microsoft Sans Serif", 11.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.dont_show_again_checkbox.Location = new System.Drawing.Point(13, 118);
            this.dont_show_again_checkbox.Name = "dont_show_again_checkbox";
            this.dont_show_again_checkbox.Size = new System.Drawing.Size(237, 22);
            this.dont_show_again_checkbox.TabIndex = 0;
            this.dont_show_again_checkbox.Text = "Don\'t show this message again.";
            this.dont_show_again_checkbox.UseVisualStyleBackColor = true;
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Font = new System.Drawing.Font("Microsoft Sans Serif", 12F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.label1.Location = new System.Drawing.Point(80, 35);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(147, 20);
            this.label1.TabIndex = 1;
            this.label1.Text = "No Internet access.";
            // 
            // ok_button
            // 
            this.ok_button.Location = new System.Drawing.Point(100, 76);
            this.ok_button.Name = "ok_button";
            this.ok_button.Size = new System.Drawing.Size(75, 23);
            this.ok_button.TabIndex = 2;
            this.ok_button.Text = "OK";
            this.ok_button.UseVisualStyleBackColor = true;
            // 
            // error_picture_box
            // 
            this.error_picture_box.Location = new System.Drawing.Point(23, 30);
            this.error_picture_box.Name = "error_picture_box";
            this.error_picture_box.Size = new System.Drawing.Size(48, 50);
            this.error_picture_box.TabIndex = 3;
            this.error_picture_box.TabStop = false;
            // 
            // NoInternetAccessDialog
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(278, 147);
            this.ControlBox = false;
            this.Controls.Add(this.error_picture_box);
            this.Controls.Add(this.ok_button);
            this.Controls.Add(this.label1);
            this.Controls.Add(this.dont_show_again_checkbox);
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedSingle;
            this.Name = "NoInternetAccessDialog";
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterParent;
            this.Text = "LeNomad";
            ((System.ComponentModel.ISupportInitialize)(this.error_picture_box)).EndInit();
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private CheckBox dont_show_again_checkbox;
        private Label label1;
        private Button ok_button;
        private PictureBox error_picture_box;
    }
}