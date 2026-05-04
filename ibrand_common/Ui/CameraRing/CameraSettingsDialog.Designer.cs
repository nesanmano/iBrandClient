using System.ComponentModel;
using System.Windows.Forms;

namespace NomadLogic.Common.Ui.CameraRing
{
    partial class CameraSettingsDialog
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
            this.label2 = new System.Windows.Forms.Label();
            this.isoComboBox = new System.Windows.Forms.ComboBox();
            this.label1 = new System.Windows.Forms.Label();
            this.label3 = new System.Windows.Forms.Label();
            this.label4 = new System.Windows.Forms.Label();
            this.label5 = new System.Windows.Forms.Label();
            this.label6 = new System.Windows.Forms.Label();
            this.label7 = new System.Windows.Forms.Label();
            this.aeModeComboBox = new System.Windows.Forms.ComboBox();
            this.tvComboBox = new System.Windows.Forms.ComboBox();
            this.avComboBox = new System.Windows.Forms.ComboBox();
            this.meteringComboBox = new System.Windows.Forms.ComboBox();
            this.exposureCompComboBox = new System.Windows.Forms.ComboBox();
            this.imageQualityComboBox = new System.Windows.Forms.ComboBox();
            this.pictureBox = new System.Windows.Forms.PictureBox();
            this.startEvfButton = new System.Windows.Forms.Button();
            this.stopEvfButton = new System.Windows.Forms.Button();
            this.takePictureButton = new System.Windows.Forms.Button();
            this.label8 = new System.Windows.Forms.Label();
            this.focus2LeftButton = new System.Windows.Forms.Button();
            this.focusLeftButton = new System.Windows.Forms.Button();
            this.focus2RightButton = new System.Windows.Forms.Button();
            this.focusRightButton = new System.Windows.Forms.Button();
            this.label9 = new System.Windows.Forms.Label();
            this.cameraNoTextBox = new System.Windows.Forms.TextBox();
            this.label10 = new System.Windows.Forms.Label();
            this.whiteBalanceComboBox = new System.Windows.Forms.ComboBox();
            this.copyPropertiesButton = new System.Windows.Forms.Button();
            ((System.ComponentModel.ISupportInitialize)(this.pictureBox)).BeginInit();
            this.SuspendLayout();
            // 
            // label2
            // 
            this.label2.AutoSize = true;
            this.label2.Font = new System.Drawing.Font("Microsoft Sans Serif", 11.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.label2.Location = new System.Drawing.Point(10, 47);
            this.label2.Name = "label2";
            this.label2.Size = new System.Drawing.Size(45, 18);
            this.label2.TabIndex = 1;
            this.label2.Text = "ISO : ";
            // 
            // isoComboBox
            // 
            this.isoComboBox.Font = new System.Drawing.Font("Microsoft Sans Serif", 12F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.isoComboBox.FormattingEnabled = true;
            this.isoComboBox.Location = new System.Drawing.Point(110, 43);
            this.isoComboBox.Name = "isoComboBox";
            this.isoComboBox.Size = new System.Drawing.Size(121, 28);
            this.isoComboBox.TabIndex = 2;
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Font = new System.Drawing.Font("Microsoft Sans Serif", 11.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.label1.Location = new System.Drawing.Point(9, 80);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(73, 18);
            this.label1.TabIndex = 3;
            this.label1.Text = "AEMode :";
            // 
            // label3
            // 
            this.label3.AutoSize = true;
            this.label3.Font = new System.Drawing.Font("Microsoft Sans Serif", 12F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.label3.Location = new System.Drawing.Point(10, 115);
            this.label3.Name = "label3";
            this.label3.Size = new System.Drawing.Size(33, 20);
            this.label3.TabIndex = 4;
            this.label3.Text = "Tv :";
            // 
            // label4
            // 
            this.label4.AutoSize = true;
            this.label4.Font = new System.Drawing.Font("Microsoft Sans Serif", 12F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.label4.Location = new System.Drawing.Point(10, 148);
            this.label4.Name = "label4";
            this.label4.Size = new System.Drawing.Size(35, 20);
            this.label4.TabIndex = 5;
            this.label4.Text = "Av :";
            // 
            // label5
            // 
            this.label5.AutoSize = true;
            this.label5.Font = new System.Drawing.Font("Microsoft Sans Serif", 9.75F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.label5.Location = new System.Drawing.Point(9, 182);
            this.label5.Name = "label5";
            this.label5.Size = new System.Drawing.Size(104, 16);
            this.label5.TabIndex = 6;
            this.label5.Text = "Metering Mode :";
            // 
            // label6
            // 
            this.label6.AutoSize = true;
            this.label6.Font = new System.Drawing.Font("Microsoft Sans Serif", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.label6.Location = new System.Drawing.Point(8, 217);
            this.label6.Name = "label6";
            this.label6.Size = new System.Drawing.Size(104, 15);
            this.label6.TabIndex = 7;
            this.label6.Text = "Exposure Comp. :";
            // 
            // label7
            // 
            this.label7.AutoSize = true;
            this.label7.Font = new System.Drawing.Font("Microsoft Sans Serif", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.label7.Location = new System.Drawing.Point(9, 251);
            this.label7.Name = "label7";
            this.label7.Size = new System.Drawing.Size(88, 15);
            this.label7.TabIndex = 8;
            this.label7.Text = "Image Quality :";
            // 
            // aeModeComboBox
            // 
            this.aeModeComboBox.Font = new System.Drawing.Font("Microsoft Sans Serif", 12F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.aeModeComboBox.FormattingEnabled = true;
            this.aeModeComboBox.Location = new System.Drawing.Point(110, 76);
            this.aeModeComboBox.Name = "aeModeComboBox";
            this.aeModeComboBox.Size = new System.Drawing.Size(121, 28);
            this.aeModeComboBox.TabIndex = 9;
            // 
            // tvComboBox
            // 
            this.tvComboBox.Font = new System.Drawing.Font("Microsoft Sans Serif", 12F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.tvComboBox.FormattingEnabled = true;
            this.tvComboBox.Location = new System.Drawing.Point(110, 109);
            this.tvComboBox.Name = "tvComboBox";
            this.tvComboBox.Size = new System.Drawing.Size(121, 28);
            this.tvComboBox.TabIndex = 10;
            // 
            // avComboBox
            // 
            this.avComboBox.Font = new System.Drawing.Font("Microsoft Sans Serif", 12F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.avComboBox.FormattingEnabled = true;
            this.avComboBox.Location = new System.Drawing.Point(110, 143);
            this.avComboBox.Name = "avComboBox";
            this.avComboBox.Size = new System.Drawing.Size(121, 28);
            this.avComboBox.TabIndex = 11;
            // 
            // meteringComboBox
            // 
            this.meteringComboBox.Font = new System.Drawing.Font("Microsoft Sans Serif", 12F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.meteringComboBox.FormattingEnabled = true;
            this.meteringComboBox.Location = new System.Drawing.Point(110, 177);
            this.meteringComboBox.Name = "meteringComboBox";
            this.meteringComboBox.Size = new System.Drawing.Size(121, 28);
            this.meteringComboBox.TabIndex = 12;
            // 
            // exposureCompComboBox
            // 
            this.exposureCompComboBox.Font = new System.Drawing.Font("Microsoft Sans Serif", 12F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.exposureCompComboBox.FormattingEnabled = true;
            this.exposureCompComboBox.Location = new System.Drawing.Point(110, 211);
            this.exposureCompComboBox.Name = "exposureCompComboBox";
            this.exposureCompComboBox.Size = new System.Drawing.Size(121, 28);
            this.exposureCompComboBox.TabIndex = 13;
            // 
            // imageQualityComboBox
            // 
            this.imageQualityComboBox.Font = new System.Drawing.Font("Microsoft Sans Serif", 12F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.imageQualityComboBox.FormattingEnabled = true;
            this.imageQualityComboBox.Location = new System.Drawing.Point(110, 245);
            this.imageQualityComboBox.Name = "imageQualityComboBox";
            this.imageQualityComboBox.Size = new System.Drawing.Size(121, 28);
            this.imageQualityComboBox.TabIndex = 14;
            // 
            // pictureBox
            // 
            this.pictureBox.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            this.pictureBox.Location = new System.Drawing.Point(245, 9);
            this.pictureBox.Name = "pictureBox";
            this.pictureBox.Size = new System.Drawing.Size(296, 201);
            this.pictureBox.TabIndex = 15;
            this.pictureBox.TabStop = false;
            // 
            // startEvfButton
            // 
            this.startEvfButton.Location = new System.Drawing.Point(9, 316);
            this.startEvfButton.Name = "startEvfButton";
            this.startEvfButton.Size = new System.Drawing.Size(116, 38);
            this.startEvfButton.TabIndex = 16;
            this.startEvfButton.Text = "Start Live View";
            this.startEvfButton.UseVisualStyleBackColor = true;
            // 
            // stopEvfButton
            // 
            this.stopEvfButton.Enabled = false;
            this.stopEvfButton.Location = new System.Drawing.Point(131, 316);
            this.stopEvfButton.Name = "stopEvfButton";
            this.stopEvfButton.Size = new System.Drawing.Size(108, 38);
            this.stopEvfButton.TabIndex = 17;
            this.stopEvfButton.Text = "Stop Live View";
            this.stopEvfButton.UseVisualStyleBackColor = true;
            // 
            // takePictureButton
            // 
            this.takePictureButton.Location = new System.Drawing.Point(245, 316);
            this.takePictureButton.Name = "takePictureButton";
            this.takePictureButton.Size = new System.Drawing.Size(104, 38);
            this.takePictureButton.TabIndex = 18;
            this.takePictureButton.Text = "Take Picture";
            this.takePictureButton.UseVisualStyleBackColor = true;
            // 
            // label8
            // 
            this.label8.AutoSize = true;
            this.label8.Font = new System.Drawing.Font("Microsoft Sans Serif", 11.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.label8.Location = new System.Drawing.Point(259, 222);
            this.label8.Name = "label8";
            this.label8.Size = new System.Drawing.Size(58, 18);
            this.label8.TabIndex = 19;
            this.label8.Text = "Focus :";
            // 
            // focus2LeftButton
            // 
            this.focus2LeftButton.Location = new System.Drawing.Point(323, 214);
            this.focus2LeftButton.Name = "focus2LeftButton";
            this.focus2LeftButton.Size = new System.Drawing.Size(43, 36);
            this.focus2LeftButton.TabIndex = 20;
            this.focus2LeftButton.Text = "<<";
            this.focus2LeftButton.UseVisualStyleBackColor = true;
            // 
            // focusLeftButton
            // 
            this.focusLeftButton.Location = new System.Drawing.Point(372, 214);
            this.focusLeftButton.Name = "focusLeftButton";
            this.focusLeftButton.Size = new System.Drawing.Size(41, 36);
            this.focusLeftButton.TabIndex = 21;
            this.focusLeftButton.Text = "<";
            this.focusLeftButton.UseVisualStyleBackColor = true;
            // 
            // focus2RightButton
            // 
            this.focus2RightButton.Location = new System.Drawing.Point(465, 214);
            this.focus2RightButton.Name = "focus2RightButton";
            this.focus2RightButton.Size = new System.Drawing.Size(43, 36);
            this.focus2RightButton.TabIndex = 23;
            this.focus2RightButton.Text = ">>";
            this.focus2RightButton.UseVisualStyleBackColor = true;
            // 
            // focusRightButton
            // 
            this.focusRightButton.Location = new System.Drawing.Point(419, 214);
            this.focusRightButton.Name = "focusRightButton";
            this.focusRightButton.Size = new System.Drawing.Size(40, 36);
            this.focusRightButton.TabIndex = 22;
            this.focusRightButton.Text = ">";
            this.focusRightButton.UseVisualStyleBackColor = true;
            // 
            // label9
            // 
            this.label9.AutoSize = true;
            this.label9.Font = new System.Drawing.Font("Microsoft Sans Serif", 11.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.label9.Location = new System.Drawing.Point(9, 11);
            this.label9.Name = "label9";
            this.label9.Size = new System.Drawing.Size(93, 18);
            this.label9.TabIndex = 24;
            this.label9.Text = "Camera No :";
            // 
            // cameraNoTextBox
            // 
            this.cameraNoTextBox.Enabled = false;
            this.cameraNoTextBox.Font = new System.Drawing.Font("Microsoft Sans Serif", 12F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.cameraNoTextBox.Location = new System.Drawing.Point(110, 11);
            this.cameraNoTextBox.Name = "cameraNoTextBox";
            this.cameraNoTextBox.Size = new System.Drawing.Size(120, 26);
            this.cameraNoTextBox.TabIndex = 25;
            // 
            // label10
            // 
            this.label10.AutoSize = true;
            this.label10.Font = new System.Drawing.Font("Microsoft Sans Serif", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.label10.Location = new System.Drawing.Point(9, 285);
            this.label10.Name = "label10";
            this.label10.Size = new System.Drawing.Size(92, 15);
            this.label10.TabIndex = 26;
            this.label10.Text = "White Balance :";
            // 
            // whiteBalanceComboBox
            // 
            this.whiteBalanceComboBox.Font = new System.Drawing.Font("Microsoft Sans Serif", 12F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.whiteBalanceComboBox.FormattingEnabled = true;
            this.whiteBalanceComboBox.Location = new System.Drawing.Point(110, 279);
            this.whiteBalanceComboBox.Name = "whiteBalanceComboBox";
            this.whiteBalanceComboBox.Size = new System.Drawing.Size(120, 28);
            this.whiteBalanceComboBox.TabIndex = 27;
            // 
            // copyPropertiesButton
            // 
            this.copyPropertiesButton.Location = new System.Drawing.Point(355, 316);
            this.copyPropertiesButton.Name = "copyPropertiesButton";
            this.copyPropertiesButton.Size = new System.Drawing.Size(182, 38);
            this.copyPropertiesButton.TabIndex = 28;
            this.copyPropertiesButton.Text = "Copy Properties To All Cameras";
            this.copyPropertiesButton.UseVisualStyleBackColor = true;
            // 
            // CameraSettingsDialog
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(549, 359);
            this.Controls.Add(this.copyPropertiesButton);
            this.Controls.Add(this.whiteBalanceComboBox);
            this.Controls.Add(this.label10);
            this.Controls.Add(this.cameraNoTextBox);
            this.Controls.Add(this.label9);
            this.Controls.Add(this.focus2RightButton);
            this.Controls.Add(this.focusRightButton);
            this.Controls.Add(this.focusLeftButton);
            this.Controls.Add(this.focus2LeftButton);
            this.Controls.Add(this.label8);
            this.Controls.Add(this.takePictureButton);
            this.Controls.Add(this.stopEvfButton);
            this.Controls.Add(this.startEvfButton);
            this.Controls.Add(this.pictureBox);
            this.Controls.Add(this.imageQualityComboBox);
            this.Controls.Add(this.exposureCompComboBox);
            this.Controls.Add(this.meteringComboBox);
            this.Controls.Add(this.avComboBox);
            this.Controls.Add(this.tvComboBox);
            this.Controls.Add(this.aeModeComboBox);
            this.Controls.Add(this.label7);
            this.Controls.Add(this.label6);
            this.Controls.Add(this.label5);
            this.Controls.Add(this.label4);
            this.Controls.Add(this.label3);
            this.Controls.Add(this.label1);
            this.Controls.Add(this.isoComboBox);
            this.Controls.Add(this.label2);
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedDialog;
            this.MaximizeBox = false;
            this.MinimizeBox = false;
            this.Name = "CameraSettingsDialog";
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterParent;
            this.Text = "iBrand (tm)";
            ((System.ComponentModel.ISupportInitialize)(this.pictureBox)).EndInit();
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private Label label2;
        private ComboBox isoComboBox;
        private Label label1;
        private Label label3;
        private Label label4;
        private Label label5;
        private Label label6;
        private Label label7;
        private ComboBox aeModeComboBox;
        private ComboBox tvComboBox;
        private ComboBox avComboBox;
        private ComboBox meteringComboBox;
        private ComboBox exposureCompComboBox;
        private ComboBox imageQualityComboBox;
        private PictureBox pictureBox;
        private Button startEvfButton;
        private Button stopEvfButton;
        private Button takePictureButton;
        private Label label8;
        private Button focus2LeftButton;
        private Button focusLeftButton;
        private Button focus2RightButton;
        private Button focusRightButton;
        private Label label9;
        private TextBox cameraNoTextBox;
        private Label label10;
        private ComboBox whiteBalanceComboBox;
        private Button copyPropertiesButton;
    }
}