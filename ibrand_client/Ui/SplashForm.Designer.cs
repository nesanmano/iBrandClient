using System.ComponentModel;
using AForge.Imaging;
using NomadLogic.Common.Util;
using NomadLogic.Multimedia;

namespace NomadLogic.iBrandDesktop.Ui
{
    partial class SplashForm
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
            var resources = new System.ComponentModel.ComponentResourceManager(typeof(SplashForm));
            this.SuspendLayout();
            System.Drawing.Image img = null;
            var splashImageFile = DirectoryUtil.GetLocalNomadLogicAssetsDir() + "nomad_logo.png";
            if (splashImageFile != null)
            {
                img = FileUtil.LoadImage(splashImageFile);
            }
            
            // 
            // SplashForm
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            
            if (img != null)
            {
                this.BackgroundImage = img;
                this.ClientSize = img.Size;
            }

            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.None;
            this.Name = "SplashForm";
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterScreen;
            this.Text = "SplashForm";
            this.ResumeLayout(false);

        }

        #endregion
    }
}