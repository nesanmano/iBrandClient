using System.ComponentModel;
using System.Windows.Forms;
using NomadLogic.Common.Util;

namespace NomadLogic.iBrandDesktop.Ui.Choice
{
    partial class CameraRingCanonTextBox
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

            _serverListeners?.StopAllService();
            FileUtil.DeleteFolderContent(_tmpDirName?.FullName);

            base.Dispose(disposing);
        }

        #region Component Designer generated code

        /// <summary> 
        /// Required method for Designer support - do not modify 
        /// the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            this.lblPictures = new System.Windows.Forms.Label();
            this.preview_button = new System.Windows.Forms.Button();
            this.nbr_picture_label = new System.Windows.Forms.Label();
            this.clear_button = new System.Windows.Forms.Button();
            this.picStatus = new System.Windows.Forms.PictureBox();
            this.calibrateButton = new System.Windows.Forms.Button();
            ((System.ComponentModel.ISupportInitialize)(this.picStatus)).BeginInit();
            this.SuspendLayout();
            // 
            // lblPictures
            // 
            this.lblPictures.AutoSize = true;
            this.lblPictures.Location = new System.Drawing.Point(3, 13);
            this.lblPictures.Name = "lblPictures";
            this.lblPictures.Size = new System.Drawing.Size(57, 13);
            this.lblPictures.TabIndex = 0;
            this.lblPictures.Text = "Picture(s) :";
            // 
            // preview_button
            // 
            this.preview_button.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.preview_button.Location = new System.Drawing.Point(146, 3);
            this.preview_button.Name = "preview_button";
            this.preview_button.Size = new System.Drawing.Size(60, 32);
            this.preview_button.TabIndex = 1;
            this.preview_button.Text = "Preview";
            this.preview_button.UseVisualStyleBackColor = true;
            // 
            // nbr_picture_label
            // 
            this.nbr_picture_label.AutoSize = true;
            this.nbr_picture_label.Location = new System.Drawing.Point(54, 13);
            this.nbr_picture_label.Name = "nbr_picture_label";
            this.nbr_picture_label.Size = new System.Drawing.Size(19, 13);
            this.nbr_picture_label.TabIndex = 3;
            this.nbr_picture_label.Text = "15";
            // 
            // clear_button
            // 
            this.clear_button.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.clear_button.Location = new System.Drawing.Point(212, 3);
            this.clear_button.Name = "clear_button";
            this.clear_button.Size = new System.Drawing.Size(48, 32);
            this.clear_button.TabIndex = 7;
            this.clear_button.Text = "Clear";
            this.clear_button.UseVisualStyleBackColor = true;
            // 
            // picStatus
            // 
            this.picStatus.Location = new System.Drawing.Point(266, 3);
            this.picStatus.Name = "picStatus";
            this.picStatus.Size = new System.Drawing.Size(32, 32);
            this.picStatus.TabIndex = 8;
            this.picStatus.TabStop = false;
            // 
            // calibrateButton
            // 
            this.calibrateButton.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.calibrateButton.Location = new System.Drawing.Point(80, 4);
            this.calibrateButton.Name = "calibrateButton";
            this.calibrateButton.Size = new System.Drawing.Size(60, 31);
            this.calibrateButton.TabIndex = 9;
            this.calibrateButton.Text = "Calibrate";
            this.calibrateButton.UseVisualStyleBackColor = true;
            this.calibrateButton.Click += new System.EventHandler(this.calibrateButton_Click);
            // 
            // CameraRingCanonTextBox
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.Controls.Add(this.calibrateButton);
            this.Controls.Add(this.picStatus);
            this.Controls.Add(this.clear_button);
            this.Controls.Add(this.nbr_picture_label);
            this.Controls.Add(this.preview_button);
            this.Controls.Add(this.lblPictures);
            this.Name = "CameraRingCanonTextBox";
            this.Size = new System.Drawing.Size(306, 41);
            ((System.ComponentModel.ISupportInitialize)(this.picStatus)).EndInit();
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private Label lblPictures;
        private Button preview_button;
        private Label nbr_picture_label;
        private Button clear_button;
        private PictureBox picStatus;
        private Button calibrateButton;
    }
}
