using System.ComponentModel;
using System.Windows.Forms;

namespace NomadLogic.iBrandDesktop.Ui
{
    partial class VideoGallery
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
            this.GalleryContainer = new System.Windows.Forms.FlowLayoutPanel();
            this.CancelVideoGalleryButton = new System.Windows.Forms.Button();
            this.SuspendLayout();
            // 
            // GalleryContainer
            // 
            this.GalleryContainer.AutoScroll = true;
            this.GalleryContainer.Location = new System.Drawing.Point(0, 0);
            this.GalleryContainer.Name = "GalleryContainer";
            this.GalleryContainer.Size = new System.Drawing.Size(829, 492);
            this.GalleryContainer.TabIndex = 0;
            // 
            // CancelVideoGalleryButton
            // 
            this.CancelVideoGalleryButton.BackColor = System.Drawing.SystemColors.ButtonFace;
            this.CancelVideoGalleryButton.Location = new System.Drawing.Point(12, 498);
            this.CancelVideoGalleryButton.Name = "CancelVideoGalleryButton";
            this.CancelVideoGalleryButton.Size = new System.Drawing.Size(75, 23);
            this.CancelVideoGalleryButton.TabIndex = 1;
            this.CancelVideoGalleryButton.Text = "Cancel";
            this.CancelVideoGalleryButton.UseVisualStyleBackColor = false;
            // 
            // VideoGallery
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.BackColor = System.Drawing.Color.Black;
            this.ClientSize = new System.Drawing.Size(828, 533);
            this.Controls.Add(this.CancelVideoGalleryButton);
            this.Controls.Add(this.GalleryContainer);
            this.Name = "VideoGallery";
            this.Text = "VideoGallery";
            this.ResumeLayout(false);

        }

        #endregion

        private FlowLayoutPanel GalleryContainer;
        private Button CancelVideoGalleryButton;
    }
}