using System.ComponentModel;
using System.Windows.Forms;
using NomadLogic.iBrand.Common.Ui;

namespace NomadLogic.iBrand.Ui.Choice.CameraRing
{
    partial class CameraRingNikonTextBox
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
            this.textBox = new System.Windows.Forms.TextBox();
            this.connectButton = new ColorButton();
            this.getPicturesButton = new ColorButton();
            this.orderPicturesButton = new ColorButton();
            this.previewButton = new ColorButton();
            this.SuspendLayout();
            // 
            // textBox
            // 
            this.textBox.Location = new System.Drawing.Point(-1, 2);
            this.textBox.Name = "textBox";
            this.textBox.Size = new System.Drawing.Size(126, 20);
            this.textBox.TabIndex = 0;
            // 
            // connectButton
            // 
            this.connectButton.Active = true;
            this.connectButton.Font = new System.Drawing.Font("Arial", 8F);
            this.connectButton.GradientStyle = ColorButton.GradientStyles.Vertical;
            this.connectButton.HoverBorderColor = System.Drawing.Color.FromArgb(((int)(((byte)(42)))), ((int)(((byte)(58)))), ((int)(((byte)(86)))));
            this.connectButton.HoverColorA = System.Drawing.Color.FromArgb(((int)(((byte)(42)))), ((int)(((byte)(58)))), ((int)(((byte)(86)))));
            this.connectButton.HoverColorB = System.Drawing.Color.White;
            this.connectButton.Location = new System.Drawing.Point(136, 1);
            this.connectButton.Name = "connectButton";
            this.connectButton.NormalBorderColor = System.Drawing.Color.FromArgb(((int)(((byte)(156)))), ((int)(((byte)(170)))), ((int)(((byte)(193)))));
            this.connectButton.NormalColorA = System.Drawing.Color.FromArgb(((int)(((byte)(156)))), ((int)(((byte)(170)))), ((int)(((byte)(193)))));
            this.connectButton.NormalColorB = System.Drawing.Color.White;
            this.connectButton.Size = new System.Drawing.Size(63, 23);
            this.connectButton.SmoothingQuality = ColorButton.SmoothingQualities.AntiAlias;
            this.connectButton.TabIndex = 1;
            this.connectButton.Text = "Connect";
            // 
            // getPicturesButton
            // 
            this.getPicturesButton.Active = true;
            this.getPicturesButton.Font = new System.Drawing.Font("Arial", 8F);
            this.getPicturesButton.GradientStyle = ColorButton.GradientStyles.Vertical;
            this.getPicturesButton.HoverBorderColor = System.Drawing.Color.FromArgb(((int)(((byte)(42)))), ((int)(((byte)(58)))), ((int)(((byte)(86)))));
            this.getPicturesButton.HoverColorA = System.Drawing.Color.FromArgb(((int)(((byte)(42)))), ((int)(((byte)(58)))), ((int)(((byte)(86)))));
            this.getPicturesButton.HoverColorB = System.Drawing.Color.White;
            this.getPicturesButton.Location = new System.Drawing.Point(201, 1);
            this.getPicturesButton.Name = "getPicturesButton";
            this.getPicturesButton.NormalBorderColor = System.Drawing.Color.FromArgb(((int)(((byte)(156)))), ((int)(((byte)(170)))), ((int)(((byte)(193)))));
            this.getPicturesButton.NormalColorA = System.Drawing.Color.FromArgb(((int)(((byte)(156)))), ((int)(((byte)(170)))), ((int)(((byte)(193)))));
            this.getPicturesButton.NormalColorB = System.Drawing.Color.White;
            this.getPicturesButton.Size = new System.Drawing.Size(74, 23);
            this.getPicturesButton.SmoothingQuality = ColorButton.SmoothingQualities.AntiAlias;
            this.getPicturesButton.TabIndex = 2;
            this.getPicturesButton.Text = "Get Pictures";
            // 
            // orderPicturesButton
            // 
            this.orderPicturesButton.Active = true;
            this.orderPicturesButton.Font = new System.Drawing.Font("Arial", 8F);
            this.orderPicturesButton.GradientStyle = ColorButton.GradientStyles.Vertical;
            this.orderPicturesButton.HoverBorderColor = System.Drawing.Color.FromArgb(((int)(((byte)(42)))), ((int)(((byte)(58)))), ((int)(((byte)(86)))));
            this.orderPicturesButton.HoverColorA = System.Drawing.Color.FromArgb(((int)(((byte)(42)))), ((int)(((byte)(58)))), ((int)(((byte)(86)))));
            this.orderPicturesButton.HoverColorB = System.Drawing.Color.White;
            this.orderPicturesButton.Location = new System.Drawing.Point(278, 1);
            this.orderPicturesButton.Name = "orderPicturesButton";
            this.orderPicturesButton.NormalBorderColor = System.Drawing.Color.FromArgb(((int)(((byte)(156)))), ((int)(((byte)(170)))), ((int)(((byte)(193)))));
            this.orderPicturesButton.NormalColorA = System.Drawing.Color.FromArgb(((int)(((byte)(156)))), ((int)(((byte)(170)))), ((int)(((byte)(193)))));
            this.orderPicturesButton.NormalColorB = System.Drawing.Color.White;
            this.orderPicturesButton.Size = new System.Drawing.Size(99, 23);
            this.orderPicturesButton.SmoothingQuality = ColorButton.SmoothingQualities.AntiAlias;
            this.orderPicturesButton.TabIndex = 3;
            this.orderPicturesButton.Text = "Order Pictures...";
            // 
            // previewButton
            // 
            this.previewButton.Active = true;
            this.previewButton.Font = new System.Drawing.Font("Arial", 8F);
            this.previewButton.GradientStyle = ColorButton.GradientStyles.Vertical;
            this.previewButton.HoverBorderColor = System.Drawing.Color.FromArgb(((int)(((byte)(42)))), ((int)(((byte)(58)))), ((int)(((byte)(86)))));
            this.previewButton.HoverColorA = System.Drawing.Color.FromArgb(((int)(((byte)(42)))), ((int)(((byte)(58)))), ((int)(((byte)(86)))));
            this.previewButton.HoverColorB = System.Drawing.Color.White;
            this.previewButton.Location = new System.Drawing.Point(380, 1);
            this.previewButton.Name = "previewButton";
            this.previewButton.NormalBorderColor = System.Drawing.Color.FromArgb(((int)(((byte)(156)))), ((int)(((byte)(170)))), ((int)(((byte)(193)))));
            this.previewButton.NormalColorA = System.Drawing.Color.FromArgb(((int)(((byte)(156)))), ((int)(((byte)(170)))), ((int)(((byte)(193)))));
            this.previewButton.NormalColorB = System.Drawing.Color.White;
            this.previewButton.Size = new System.Drawing.Size(60, 23);
            this.previewButton.SmoothingQuality = ColorButton.SmoothingQualities.AntiAlias;
            this.previewButton.TabIndex = 4;
            this.previewButton.Text = "Preview";
            // 
            // CameraRingNikonTextBox
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.Controls.Add(this.previewButton);
            this.Controls.Add(this.orderPicturesButton);
            this.Controls.Add(this.getPicturesButton);
            this.Controls.Add(this.connectButton);
            this.Controls.Add(this.textBox);
            this.Name = "CameraRingNikonTextBox";
            this.Size = new System.Drawing.Size(449, 26);
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private TextBox textBox;
        private ColorButton connectButton;
        private ColorButton getPicturesButton;
        private ColorButton orderPicturesButton;
        private ColorButton previewButton;
    }
}
