using System.ComponentModel;
using System.Windows.Forms;

namespace NomadLogic.Common.Ui.CameraRing
{
    partial class CameraView
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
            this.camera_name_label = new System.Windows.Forms.Label();
            this.picture_box = new System.Windows.Forms.PictureBox();
            ((System.ComponentModel.ISupportInitialize)(this.picture_box)).BeginInit();
            this.SuspendLayout();
            // 
            // camera_name_label
            // 
            this.camera_name_label.AutoSize = true;
            this.camera_name_label.Font = new System.Drawing.Font("Microsoft Sans Serif", 12F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.camera_name_label.Location = new System.Drawing.Point(52, 131);
            this.camera_name_label.Name = "camera_name_label";
            this.camera_name_label.Size = new System.Drawing.Size(77, 20);
            this.camera_name_label.TabIndex = 0;
            this.camera_name_label.Text = "Camera : ";
            // 
            // picture_box
            // 
            this.picture_box.BackColor = System.Drawing.SystemColors.ActiveCaptionText;
            this.picture_box.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            this.picture_box.Location = new System.Drawing.Point(3, 3);
            this.picture_box.Name = "picture_box";
            this.picture_box.Size = new System.Drawing.Size(191, 127);
            this.picture_box.TabIndex = 1;
            this.picture_box.TabStop = false;
            this.picture_box.Click += new System.EventHandler(this.picture_box_Click);
            // 
            // CameraView
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.BackColor = System.Drawing.SystemColors.ControlLightLight;
            this.Controls.Add(this.picture_box);
            this.Controls.Add(this.camera_name_label);
            this.Name = "CameraView";
            this.Size = new System.Drawing.Size(197, 156);
            ((System.ComponentModel.ISupportInitialize)(this.picture_box)).EndInit();
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private Label camera_name_label;
        private PictureBox picture_box;
    }
}
