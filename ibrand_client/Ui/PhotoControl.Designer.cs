using System.ComponentModel;
using System.Windows.Forms;

namespace NomadLogic.iBrandDesktop.Ui
{
    partial class PhotoControl
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
            this.ImageName = new System.Windows.Forms.Label();
            this.ImageBox = new System.Windows.Forms.PictureBox();
            ((System.ComponentModel.ISupportInitialize)(this.ImageBox)).BeginInit();
            this.SuspendLayout();
            // 
            // ImageName
            // 
            this.ImageName.Font = new System.Drawing.Font("Arial", 12F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.ImageName.ForeColor = System.Drawing.Color.White;
            this.ImageName.Location = new System.Drawing.Point(3, 171);
            this.ImageName.Name = "ImageName";
            this.ImageName.Size = new System.Drawing.Size(292, 30);
            this.ImageName.TabIndex = 0;
            this.ImageName.Text = "Image Name";
            this.ImageName.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
            // 
            // ImageBox
            // 
            this.ImageBox.Location = new System.Drawing.Point(4, 4);
            this.ImageBox.Name = "ImageBox";
            this.ImageBox.Size = new System.Drawing.Size(292, 164);
            this.ImageBox.SizeMode = System.Windows.Forms.PictureBoxSizeMode.Zoom;
            this.ImageBox.TabIndex = 1;
            this.ImageBox.TabStop = false;
            // 
            // PhotoControl
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.BackColor = System.Drawing.Color.Transparent;
            this.Controls.Add(this.ImageBox);
            this.Controls.Add(this.ImageName);
            this.Name = "PhotoControl";
            this.Size = new System.Drawing.Size(300, 210);
            ((System.ComponentModel.ISupportInitialize)(this.ImageBox)).EndInit();
            this.ResumeLayout(false);

        }

        #endregion

        private Label ImageName;
        private PictureBox ImageBox;
    }
}
