using System.ComponentModel;
using System.Windows.Forms;

namespace NomadLogic.iBrandDesktop.Ui.ApplicationScreen
{
    partial class ErrorMessageDialog
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
            this.ok_button = new System.Windows.Forms.Button();
            this.error_picture_box = new System.Windows.Forms.PictureBox();
            this.message = new System.Windows.Forms.Label();
            ((System.ComponentModel.ISupportInitialize)(this.error_picture_box)).BeginInit();
            this.SuspendLayout();
            // 
            // ok_button
            // 
            this.ok_button.Location = new System.Drawing.Point(104, 79);
            this.ok_button.Name = "ok_button";
            this.ok_button.Size = new System.Drawing.Size(75, 37);
            this.ok_button.TabIndex = 0;
            this.ok_button.Text = "OK";
            this.ok_button.UseVisualStyleBackColor = true;
            // 
            // error_picture_box
            // 
            this.error_picture_box.Location = new System.Drawing.Point(23, 20);
            this.error_picture_box.Name = "error_picture_box";
            this.error_picture_box.Size = new System.Drawing.Size(51, 48);
            this.error_picture_box.TabIndex = 1;
            this.error_picture_box.TabStop = false;
            // 
            // message
            // 
            this.message.AutoSize = true;
            this.message.Font = new System.Drawing.Font("Microsoft Sans Serif", 9.75F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.message.Location = new System.Drawing.Point(90, 30);
            this.message.Name = "message";
            this.message.Size = new System.Drawing.Size(42, 16);
            this.message.TabIndex = 2;
            this.message.Text = "Label";
            // 
            // ErrorMessageDialog
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(284, 123);
            this.ControlBox = false;
            this.Controls.Add(this.message);
            this.Controls.Add(this.error_picture_box);
            this.Controls.Add(this.ok_button);
            this.Name = "ErrorMessageDialog";
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterScreen;
            this.Text = "iBrand";
            ((System.ComponentModel.ISupportInitialize)(this.error_picture_box)).EndInit();
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private Button ok_button;
        private PictureBox error_picture_box;
        private Label message;
    }
}