using System.ComponentModel;
using System.Windows.Forms;

namespace NomadLogic.Multimedia.Gpu.Ui.ChromaKeyDialogs
{
    public abstract partial class PreviewDialogChromaKeyAbstract<TChromaKeyType> : Form where TChromaKeyType : ChromaKeyInfoAbstract
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
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(PreviewDialogChromaKeyAbstract<TChromaKeyType>));
            this.pbOriginal = new System.Windows.Forms.PictureBox();
            this.ButtonSaveToDisk = new System.Windows.Forms.Button();
            this.ButtonDefaultGreenScreen = new System.Windows.Forms.Button();
            this.ButtonClose = new System.Windows.Forms.Button();
            this.ButtonApply = new System.Windows.Forms.Button();
            this.panelOriginalPicture = new System.Windows.Forms.Panel();
            this.cameraVendorComboBox = new System.Windows.Forms.ComboBox();
            this.btnTakePicture = new System.Windows.Forms.Button();
            this.btnLoadPicture = new System.Windows.Forms.Button();
            this.lblOriginalPicture = new System.Windows.Forms.Label();
            this.panelKeyedPicture = new System.Windows.Forms.Panel();
            this.lblKeyedPicture = new System.Windows.Forms.Label();
            this.pbTransformed = GetTransformedPictureBox();
            this.ButtonDefaultBlueScreen = new System.Windows.Forms.Button();
            ((System.ComponentModel.ISupportInitialize)(this.pbOriginal)).BeginInit();
            this.panelOriginalPicture.SuspendLayout();
            this.panelKeyedPicture.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.pbTransformed)).BeginInit();
            this.SuspendLayout();
            // 
            // pbOriginal
            // 
            this.pbOriginal.Location = new System.Drawing.Point(0, 42);
            this.pbOriginal.Name = "pbOriginal";
            this.pbOriginal.Size = new System.Drawing.Size(260, 260);
            this.pbOriginal.TabIndex = 0;
            this.pbOriginal.TabStop = false;
            // 
            // ButtonSaveToDisk
            // 
            this.ButtonSaveToDisk.Location = new System.Drawing.Point(842, 360);
            this.ButtonSaveToDisk.Name = "ButtonSaveToDisk";
            this.ButtonSaveToDisk.Size = new System.Drawing.Size(173, 40);
            this.ButtonSaveToDisk.TabIndex = 14;
            this.ButtonSaveToDisk.Text = "Save values to disk";
            this.ButtonSaveToDisk.UseVisualStyleBackColor = true;
            this.ButtonSaveToDisk.Click += new System.EventHandler(this.ButtonSaveToDisk_Click);
            // 
            // ButtonDefaultGreenScreen
            // 
            this.ButtonDefaultGreenScreen.Location = new System.Drawing.Point(604, 359);
            this.ButtonDefaultGreenScreen.Name = "ButtonDefaultGreenScreen";
            this.ButtonDefaultGreenScreen.Size = new System.Drawing.Size(163, 40);
            this.ButtonDefaultGreenScreen.TabIndex = 13;
            this.ButtonDefaultGreenScreen.Text = "Reset (Green)";
            this.ButtonDefaultGreenScreen.UseVisualStyleBackColor = true;
            this.ButtonDefaultGreenScreen.Click += new System.EventHandler(this.ButtonReset_Click);
            // 
            // ButtonClose
            // 
            this.ButtonClose.Location = new System.Drawing.Point(162, 359);
            this.ButtonClose.Name = "ButtonClose";
            this.ButtonClose.Size = new System.Drawing.Size(133, 41);
            this.ButtonClose.TabIndex = 15;
            this.ButtonClose.Text = "Close";
            this.ButtonClose.UseVisualStyleBackColor = true;
            this.ButtonClose.Click += new System.EventHandler(this.ButtonClose_Click);
            // 
            // ButtonApply
            // 
            this.ButtonApply.Location = new System.Drawing.Point(12, 359);
            this.ButtonApply.Name = "ButtonApply";
            this.ButtonApply.Size = new System.Drawing.Size(134, 40);
            this.ButtonApply.TabIndex = 8;
            this.ButtonApply.Text = "Apply keying to picture";
            this.ButtonApply.UseVisualStyleBackColor = true;
            this.ButtonApply.Click += new System.EventHandler(this.ButtonApply_Click);
            // 
            // panelOriginalPicture
            // 
            this.panelOriginalPicture.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            this.panelOriginalPicture.Controls.Add(this.cameraVendorComboBox);
            this.panelOriginalPicture.Controls.Add(this.btnTakePicture);
            this.panelOriginalPicture.Controls.Add(this.btnLoadPicture);
            this.panelOriginalPicture.Controls.Add(this.lblOriginalPicture);
            this.panelOriginalPicture.Controls.Add(this.pbOriginal);
            this.panelOriginalPicture.Location = new System.Drawing.Point(11, 12);
            this.panelOriginalPicture.Name = "panelOriginalPicture";
            this.panelOriginalPicture.Size = new System.Drawing.Size(262, 341);
            this.panelOriginalPicture.TabIndex = 2;
            // 
            // cameraVendorComboBox
            // 
            this.cameraVendorComboBox.FormattingEnabled = true;
            this.cameraVendorComboBox.Location = new System.Drawing.Point(187, 308);
            this.cameraVendorComboBox.Name = "cameraVendorComboBox";
            this.cameraVendorComboBox.Size = new System.Drawing.Size(64, 21);
            this.cameraVendorComboBox.TabIndex = 3;
            // 
            // btnTakePicture
            // 
            this.btnTakePicture.Location = new System.Drawing.Point(3, 308);
            this.btnTakePicture.Name = "btnTakePicture";
            this.btnTakePicture.Size = new System.Drawing.Size(85, 23);
            this.btnTakePicture.TabIndex = 2;
            this.btnTakePicture.Text = "Take Picture";
            this.btnTakePicture.UseVisualStyleBackColor = true;
            this.btnTakePicture.Click += new System.EventHandler(this.btnTakePicture_Click);
            // 
            // btnLoadPicture
            // 
            this.btnLoadPicture.Location = new System.Drawing.Point(91, 308);
            this.btnLoadPicture.Name = "btnLoadPicture";
            this.btnLoadPicture.Size = new System.Drawing.Size(60, 23);
            this.btnLoadPicture.TabIndex = 3;
            this.btnLoadPicture.Text = "Load...";
            this.btnLoadPicture.UseVisualStyleBackColor = true;
            this.btnLoadPicture.Click += new System.EventHandler(this.btnLoadPicture_Click);
            // 
            // lblOriginalPicture
            // 
            this.lblOriginalPicture.Location = new System.Drawing.Point(0, 4);
            this.lblOriginalPicture.Name = "lblOriginalPicture";
            this.lblOriginalPicture.Size = new System.Drawing.Size(260, 35);
            this.lblOriginalPicture.TabIndex = 1;
            this.lblOriginalPicture.Text = "Original Picture";
            this.lblOriginalPicture.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
            // 
            // panelKeyedPicture
            // 
            this.panelKeyedPicture.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            this.panelKeyedPicture.Controls.Add(this.lblKeyedPicture);
            this.panelKeyedPicture.Controls.Add(this.pbTransformed);
            this.panelKeyedPicture.Location = new System.Drawing.Point(280, 12);
            this.panelKeyedPicture.Name = "panelKeyedPicture";
            this.panelKeyedPicture.Size = new System.Drawing.Size(264, 341);
            this.panelKeyedPicture.TabIndex = 16;
            // 
            // lblKeyedPicture
            // 
            this.lblKeyedPicture.Location = new System.Drawing.Point(1, 4);
            this.lblKeyedPicture.Name = "lblKeyedPicture";
            this.lblKeyedPicture.Size = new System.Drawing.Size(259, 35);
            this.lblKeyedPicture.TabIndex = 1;
            this.lblKeyedPicture.Text = "Keyed Picture";
            this.lblKeyedPicture.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
            // 
            // pbTransformed
            // 
            this.pbTransformed.Location = new System.Drawing.Point(0, 42);
            this.pbTransformed.Name = "pbTransformed";
            this.pbTransformed.Size = new System.Drawing.Size(260, 260);
            this.pbTransformed.SizeMode = System.Windows.Forms.PictureBoxSizeMode.CenterImage;
            this.pbTransformed.TabIndex = 0;
            this.pbTransformed.TabStop = false;
            // 
            // ButtonDefaultBlueScreen
            // 
            this.ButtonDefaultBlueScreen.Location = new System.Drawing.Point(435, 360);
            this.ButtonDefaultBlueScreen.Name = "ButtonDefaultBlueScreen";
            this.ButtonDefaultBlueScreen.Size = new System.Drawing.Size(163, 40);
            this.ButtonDefaultBlueScreen.TabIndex = 17;
            this.ButtonDefaultBlueScreen.Text = "Reset (Blue)";
            this.ButtonDefaultBlueScreen.UseVisualStyleBackColor = true;
            this.ButtonDefaultBlueScreen.Click += new System.EventHandler(this.ButtonReset_Click);
            
            // 
            // PreviewDialogChromaKeyAbstract
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(1037, 405);
            this.Controls.Add(this.ButtonDefaultBlueScreen);
            this.Controls.Add(this.panelKeyedPicture);
            this.Controls.Add(this.panelOriginalPicture);
            this.Controls.Add(this.ButtonClose);
            this.Controls.Add(this.ButtonApply);
            this.Controls.Add(this.ButtonDefaultGreenScreen);
            this.Controls.Add(this.ButtonSaveToDisk);
            this.MaximizeBox = false;
            this.MaximumSize = new System.Drawing.Size(1053, 444);
            this.MinimizeBox = false;
            this.MinimumSize = new System.Drawing.Size(1053, 444);
            this.Name = "PreviewDialogChromaKeyAbstract";
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterParent;
            this.Text = "Chromatic Keying preview";
            this.FormClosing += new System.Windows.Forms.FormClosingEventHandler(this.PreviewDialogTransformation_FormClosing);
            ((System.ComponentModel.ISupportInitialize)(this.pbOriginal)).EndInit();
            this.panelOriginalPicture.ResumeLayout(false);
            this.panelOriginalPicture.PerformLayout();
            this.panelKeyedPicture.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)(this.pbTransformed)).EndInit();
            this.ResumeLayout(false);
        }

        #endregion

        protected PictureBox pbOriginal;
        protected Button ButtonApply;
        protected Button ButtonDefaultGreenScreen;
        private Button ButtonClose;
        private Button ButtonSaveToDisk;
        private Panel panelOriginalPicture;
        private Button btnTakePicture;
        private Button btnLoadPicture;
        private Label lblOriginalPicture;
        protected Panel panelKeyedPicture;
        private Label lblKeyedPicture;
        protected PictureBox pbTransformed;
        protected Button ButtonDefaultBlueScreen;
        private ComboBox cameraVendorComboBox;
    }
}