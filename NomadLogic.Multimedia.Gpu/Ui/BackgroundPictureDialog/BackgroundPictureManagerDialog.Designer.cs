namespace NomadLogic.Multimedia.Gpu.Ui.BackgroundPictureDialog
{
    partial class BackgroundPictureManagerDialog
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
            this.picCurrentBackground = new System.Windows.Forms.PictureBox();
            this.btnTakeBkg = new System.Windows.Forms.Button();
            this.picOverlay = new System.Windows.Forms.PictureBox();
            this.btnTakeOverlay = new System.Windows.Forms.Button();
            this.picComposite = new System.Windows.Forms.PictureBox();
            this.btnSaveComposite = new System.Windows.Forms.Button();
            this.btnViewFullScreen = new System.Windows.Forms.Button();
            this.btnLoadFromDisk = new System.Windows.Forms.Button();
            ((System.ComponentModel.ISupportInitialize)(this.picCurrentBackground)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.picOverlay)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.picComposite)).BeginInit();
            this.SuspendLayout();
            // 
            // picCurrentBackground
            // 
            this.picCurrentBackground.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            this.picCurrentBackground.Location = new System.Drawing.Point(12, 12);
            this.picCurrentBackground.Name = "picCurrentBackground";
            this.picCurrentBackground.Size = new System.Drawing.Size(346, 346);
            this.picCurrentBackground.TabIndex = 0;
            this.picCurrentBackground.TabStop = false;
            // 
            // btnTakeBkg
            // 
            this.btnTakeBkg.Location = new System.Drawing.Point(12, 364);
            this.btnTakeBkg.Name = "btnTakeBkg";
            this.btnTakeBkg.Size = new System.Drawing.Size(84, 23);
            this.btnTakeBkg.TabIndex = 1;
            this.btnTakeBkg.Text = "Capture Bkg";
            this.btnTakeBkg.UseVisualStyleBackColor = true;
            this.btnTakeBkg.Click += new System.EventHandler(this.OnBtnClick);
            // 
            // picOverlay
            // 
            this.picOverlay.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            this.picOverlay.Location = new System.Drawing.Point(364, 12);
            this.picOverlay.Name = "picOverlay";
            this.picOverlay.Size = new System.Drawing.Size(346, 346);
            this.picOverlay.TabIndex = 2;
            this.picOverlay.TabStop = false;
            // 
            // btnTakeOverlay
            // 
            this.btnTakeOverlay.Location = new System.Drawing.Point(364, 364);
            this.btnTakeOverlay.Name = "btnTakeOverlay";
            this.btnTakeOverlay.Size = new System.Drawing.Size(125, 23);
            this.btnTakeOverlay.TabIndex = 3;
            this.btnTakeOverlay.Text = "Take Test Overlay";
            this.btnTakeOverlay.UseVisualStyleBackColor = true;
            this.btnTakeOverlay.Click += new System.EventHandler(this.OnBtnClick);
            // 
            // picComposite
            // 
            this.picComposite.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            this.picComposite.Location = new System.Drawing.Point(716, 12);
            this.picComposite.Name = "picComposite";
            this.picComposite.Size = new System.Drawing.Size(346, 346);
            this.picComposite.TabIndex = 4;
            this.picComposite.TabStop = false;
            // 
            // btnSaveComposite
            // 
            this.btnSaveComposite.Location = new System.Drawing.Point(716, 364);
            this.btnSaveComposite.Name = "btnSaveComposite";
            this.btnSaveComposite.Size = new System.Drawing.Size(148, 23);
            this.btnSaveComposite.TabIndex = 5;
            this.btnSaveComposite.Text = "Save to Desktop";
            this.btnSaveComposite.UseVisualStyleBackColor = true;
            this.btnSaveComposite.Click += new System.EventHandler(this.OnBtnClick);
            // 
            // btnViewFullScreen
            // 
            this.btnViewFullScreen.Location = new System.Drawing.Point(932, 364);
            this.btnViewFullScreen.Name = "btnViewFullScreen";
            this.btnViewFullScreen.Size = new System.Drawing.Size(130, 23);
            this.btnViewFullScreen.TabIndex = 6;
            this.btnViewFullScreen.Text = "View Full Screen";
            this.btnViewFullScreen.UseVisualStyleBackColor = true;
            this.btnViewFullScreen.Click += new System.EventHandler(this.OnBtnClick);
            // 
            // btnLoadFromDisk
            // 
            this.btnLoadFromDisk.Location = new System.Drawing.Point(102, 364);
            this.btnLoadFromDisk.Name = "btnLoadFromDisk";
            this.btnLoadFromDisk.Size = new System.Drawing.Size(84, 23);
            this.btnLoadFromDisk.TabIndex = 7;
            this.btnLoadFromDisk.Text = "Load...";
            this.btnLoadFromDisk.UseVisualStyleBackColor = true;
            this.btnLoadFromDisk.Click += new System.EventHandler(this.OnBtnClick);
            // 
            // BackgroundPictureManagerDialog
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(1075, 420);
            this.Controls.Add(this.btnLoadFromDisk);
            this.Controls.Add(this.btnViewFullScreen);
            this.Controls.Add(this.btnSaveComposite);
            this.Controls.Add(this.picComposite);
            this.Controls.Add(this.btnTakeOverlay);
            this.Controls.Add(this.picOverlay);
            this.Controls.Add(this.btnTakeBkg);
            this.Controls.Add(this.picCurrentBackground);
            this.MaximizeBox = false;
            this.MaximumSize = new System.Drawing.Size(1078, 450);
            this.MinimizeBox = false;
            this.MinimumSize = new System.Drawing.Size(1078, 450);
            this.Name = "BackgroundPictureManagerDialog";
            this.ShowIcon = false;
            this.ShowInTaskbar = false;
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterParent;
            this.Text = "iBrand Background Picture Manager";
            ((System.ComponentModel.ISupportInitialize)(this.picCurrentBackground)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.picOverlay)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.picComposite)).EndInit();
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.PictureBox picCurrentBackground;
        private System.Windows.Forms.Button btnTakeBkg;
        private System.Windows.Forms.PictureBox picOverlay;
        private System.Windows.Forms.Button btnTakeOverlay;
        private System.Windows.Forms.PictureBox picComposite;
        private System.Windows.Forms.Button btnSaveComposite;
        private System.Windows.Forms.Button btnViewFullScreen;
        private System.Windows.Forms.Button btnLoadFromDisk;
    }
}