using System.Windows.Forms;

namespace NomadLogic.iBrand.CameraRingClient
{
    partial class CameraRingClientForm
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
            this.statusStripMain = new System.Windows.Forms.StatusStrip();
            this.toolStripStatusLabelStatusNetwork = new System.Windows.Forms.ToolStripStatusLabel();
            this.toolStripProgressBarSendToServer = new System.Windows.Forms.ToolStripProgressBar();
            this.toolStripStatusLabelLastTimeStamp = new System.Windows.Forms.ToolStripStatusLabel();
            this.toolStripStatusDuplicate = new System.Windows.Forms.ToolStripStatusLabel();
            this.panel1 = new System.Windows.Forms.Panel();
            this.NextCameraButton = new System.Windows.Forms.Button();
            this.PreviousCameraButton = new System.Windows.Forms.Button();
            this.camerasComboBox = new System.Windows.Forms.ComboBox();
            this.copyPropertiesButton = new System.Windows.Forms.Button();
            this.whiteBalanceComboBox = new System.Windows.Forms.ComboBox();
            this.label10 = new System.Windows.Forms.Label();
            this.label9 = new System.Windows.Forms.Label();
            this.focus2RightButton = new System.Windows.Forms.Button();
            this.focusRightButton = new System.Windows.Forms.Button();
            this.focusLeftButton = new System.Windows.Forms.Button();
            this.focus2LeftButton = new System.Windows.Forms.Button();
            this.label8 = new System.Windows.Forms.Label();
            this.stopEvfButton = new System.Windows.Forms.Button();
            this.startEvfButton = new System.Windows.Forms.Button();
            this.pictureBox = new System.Windows.Forms.PictureBox();
            this.imageQualityComboBox = new System.Windows.Forms.ComboBox();
            this.exposureCompComboBox = new System.Windows.Forms.ComboBox();
            this.meteringComboBox = new System.Windows.Forms.ComboBox();
            this.avComboBox = new System.Windows.Forms.ComboBox();
            this.tvComboBox = new System.Windows.Forms.ComboBox();
            this.aeModeComboBox = new System.Windows.Forms.ComboBox();
            this.label7 = new System.Windows.Forms.Label();
            this.label6 = new System.Windows.Forms.Label();
            this.label5 = new System.Windows.Forms.Label();
            this.label4 = new System.Windows.Forms.Label();
            this.label3 = new System.Windows.Forms.Label();
            this.label1 = new System.Windows.Forms.Label();
            this.isoComboBox = new System.Windows.Forms.ComboBox();
            this.label2 = new System.Windows.Forms.Label();
            this.ConfigurationButton = new System.Windows.Forms.Button();
            this.statusStripMain.SuspendLayout();
            this.panel1.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.pictureBox)).BeginInit();
            this.SuspendLayout();
            // 
            // statusStripMain
            // 
            this.statusStripMain.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.toolStripStatusLabelStatusNetwork,
            this.toolStripProgressBarSendToServer,
            this.toolStripStatusLabelLastTimeStamp,
            this.toolStripStatusDuplicate});
            this.statusStripMain.Location = new System.Drawing.Point(0, 471);
            this.statusStripMain.Name = "statusStripMain";
            this.statusStripMain.Size = new System.Drawing.Size(979, 24);
            this.statusStripMain.TabIndex = 2;
            this.statusStripMain.Text = "statusStripMain";
            // 
            // toolStripStatusLabelStatusNetwork
            // 
            this.toolStripStatusLabelStatusNetwork.BorderSides = ((System.Windows.Forms.ToolStripStatusLabelBorderSides)((((System.Windows.Forms.ToolStripStatusLabelBorderSides.Left | System.Windows.Forms.ToolStripStatusLabelBorderSides.Top) 
            | System.Windows.Forms.ToolStripStatusLabelBorderSides.Right) 
            | System.Windows.Forms.ToolStripStatusLabelBorderSides.Bottom)));
            this.toolStripStatusLabelStatusNetwork.Name = "toolStripStatusLabelStatusNetwork";
            this.toolStripStatusLabelStatusNetwork.Size = new System.Drawing.Size(45, 19);
            this.toolStripStatusLabelStatusNetwork.Text = "IpStrip";
            // 
            // toolStripProgressBarSendToServer
            // 
            this.toolStripProgressBarSendToServer.Name = "toolStripProgressBarSendToServer";
            this.toolStripProgressBarSendToServer.Size = new System.Drawing.Size(100, 18);
            // 
            // toolStripStatusLabelLastTimeStamp
            // 
            this.toolStripStatusLabelLastTimeStamp.Name = "toolStripStatusLabelLastTimeStamp";
            this.toolStripStatusLabelLastTimeStamp.Size = new System.Drawing.Size(88, 19);
            this.toolStripStatusLabelLastTimeStamp.Text = "LastTimestamp";
            // 
            // toolStripStatusDuplicate
            // 
            this.toolStripStatusDuplicate.Name = "toolStripStatusDuplicate";
            this.toolStripStatusDuplicate.Size = new System.Drawing.Size(92, 19);
            this.toolStripStatusDuplicate.Text = "Duplicate Status";
            // 
            // panel1
            // 
            this.panel1.Controls.Add(this.ConfigurationButton);
            this.panel1.Controls.Add(this.NextCameraButton);
            this.panel1.Controls.Add(this.PreviousCameraButton);
            this.panel1.Controls.Add(this.camerasComboBox);
            this.panel1.Controls.Add(this.copyPropertiesButton);
            this.panel1.Controls.Add(this.whiteBalanceComboBox);
            this.panel1.Controls.Add(this.label10);
            this.panel1.Controls.Add(this.label9);
            this.panel1.Controls.Add(this.focus2RightButton);
            this.panel1.Controls.Add(this.focusRightButton);
            this.panel1.Controls.Add(this.focusLeftButton);
            this.panel1.Controls.Add(this.focus2LeftButton);
            this.panel1.Controls.Add(this.label8);
            this.panel1.Controls.Add(this.stopEvfButton);
            this.panel1.Controls.Add(this.startEvfButton);
            this.panel1.Controls.Add(this.pictureBox);
            this.panel1.Controls.Add(this.imageQualityComboBox);
            this.panel1.Controls.Add(this.exposureCompComboBox);
            this.panel1.Controls.Add(this.meteringComboBox);
            this.panel1.Controls.Add(this.avComboBox);
            this.panel1.Controls.Add(this.tvComboBox);
            this.panel1.Controls.Add(this.aeModeComboBox);
            this.panel1.Controls.Add(this.label7);
            this.panel1.Controls.Add(this.label6);
            this.panel1.Controls.Add(this.label5);
            this.panel1.Controls.Add(this.label4);
            this.panel1.Controls.Add(this.label3);
            this.panel1.Controls.Add(this.label1);
            this.panel1.Controls.Add(this.isoComboBox);
            this.panel1.Controls.Add(this.label2);
            this.panel1.Location = new System.Drawing.Point(9, 12);
            this.panel1.Name = "panel1";
            this.panel1.Size = new System.Drawing.Size(958, 455);
            this.panel1.TabIndex = 29;
            // 
            // NextCameraButton
            // 
            this.NextCameraButton.Image = global::NomadLogic.iBrand.CameraRingClient.Properties.Resources.next_arrow;
            this.NextCameraButton.Location = new System.Drawing.Point(822, 351);
            this.NextCameraButton.Name = "NextCameraButton";
            this.NextCameraButton.Size = new System.Drawing.Size(80, 80);
            this.NextCameraButton.TabIndex = 58;
            this.NextCameraButton.UseVisualStyleBackColor = true;
            this.NextCameraButton.Click += new System.EventHandler(this.NextCameraButton_Click);
            // 
            // PreviousCameraButton
            // 
            this.PreviousCameraButton.Image = global::NomadLogic.iBrand.CameraRingClient.Properties.Resources.previous_arrow;
            this.PreviousCameraButton.Location = new System.Drawing.Point(397, 351);
            this.PreviousCameraButton.Name = "PreviousCameraButton";
            this.PreviousCameraButton.Size = new System.Drawing.Size(80, 80);
            this.PreviousCameraButton.TabIndex = 57;
            this.PreviousCameraButton.UseVisualStyleBackColor = true;
            this.PreviousCameraButton.Click += new System.EventHandler(this.PreviousCameraButton_Click);
            // 
            // camerasComboBox
            // 
            this.camerasComboBox.Font = new System.Drawing.Font("Microsoft Sans Serif", 12F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.camerasComboBox.FormattingEnabled = true;
            this.camerasComboBox.Location = new System.Drawing.Point(113, 8);
            this.camerasComboBox.Name = "camerasComboBox";
            this.camerasComboBox.Size = new System.Drawing.Size(227, 28);
            this.camerasComboBox.TabIndex = 29;
            // 
            // copyPropertiesButton
            // 
            this.copyPropertiesButton.Location = new System.Drawing.Point(158, 361);
            this.copyPropertiesButton.Name = "copyPropertiesButton";
            this.copyPropertiesButton.Size = new System.Drawing.Size(182, 38);
            this.copyPropertiesButton.TabIndex = 56;
            this.copyPropertiesButton.Text = "Copy Properties To All Cameras";
            this.copyPropertiesButton.UseVisualStyleBackColor = true;
            // 
            // whiteBalanceComboBox
            // 
            this.whiteBalanceComboBox.Font = new System.Drawing.Font("Microsoft Sans Serif", 12F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.whiteBalanceComboBox.FormattingEnabled = true;
            this.whiteBalanceComboBox.Location = new System.Drawing.Point(113, 279);
            this.whiteBalanceComboBox.Name = "whiteBalanceComboBox";
            this.whiteBalanceComboBox.Size = new System.Drawing.Size(226, 28);
            this.whiteBalanceComboBox.TabIndex = 55;
            // 
            // label10
            // 
            this.label10.AutoSize = true;
            this.label10.Font = new System.Drawing.Font("Microsoft Sans Serif", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.label10.Location = new System.Drawing.Point(12, 285);
            this.label10.Name = "label10";
            this.label10.Size = new System.Drawing.Size(92, 15);
            this.label10.TabIndex = 54;
            this.label10.Text = "White Balance :";
            // 
            // label9
            // 
            this.label9.AutoSize = true;
            this.label9.Font = new System.Drawing.Font("Microsoft Sans Serif", 11.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.label9.Location = new System.Drawing.Point(12, 11);
            this.label9.Name = "label9";
            this.label9.Size = new System.Drawing.Size(93, 18);
            this.label9.TabIndex = 53;
            this.label9.Text = "Camera No :";
            // 
            // focus2RightButton
            // 
            this.focus2RightButton.Location = new System.Drawing.Point(269, 319);
            this.focus2RightButton.Name = "focus2RightButton";
            this.focus2RightButton.Size = new System.Drawing.Size(47, 36);
            this.focus2RightButton.TabIndex = 52;
            this.focus2RightButton.Text = ">>";
            this.focus2RightButton.UseVisualStyleBackColor = true;
            // 
            // focusRightButton
            // 
            this.focusRightButton.Location = new System.Drawing.Point(223, 319);
            this.focusRightButton.Name = "focusRightButton";
            this.focusRightButton.Size = new System.Drawing.Size(44, 36);
            this.focusRightButton.TabIndex = 51;
            this.focusRightButton.Text = ">";
            this.focusRightButton.UseVisualStyleBackColor = true;
            // 
            // focusLeftButton
            // 
            this.focusLeftButton.Location = new System.Drawing.Point(176, 319);
            this.focusLeftButton.Name = "focusLeftButton";
            this.focusLeftButton.Size = new System.Drawing.Size(45, 36);
            this.focusLeftButton.TabIndex = 50;
            this.focusLeftButton.Text = "<";
            this.focusLeftButton.UseVisualStyleBackColor = true;
            // 
            // focus2LeftButton
            // 
            this.focus2LeftButton.Location = new System.Drawing.Point(127, 319);
            this.focus2LeftButton.Name = "focus2LeftButton";
            this.focus2LeftButton.Size = new System.Drawing.Size(47, 36);
            this.focus2LeftButton.TabIndex = 49;
            this.focus2LeftButton.Text = "<<";
            this.focus2LeftButton.UseVisualStyleBackColor = true;
            // 
            // label8
            // 
            this.label8.AutoSize = true;
            this.label8.Font = new System.Drawing.Font("Microsoft Sans Serif", 11.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.label8.Location = new System.Drawing.Point(11, 327);
            this.label8.Name = "label8";
            this.label8.Size = new System.Drawing.Size(58, 18);
            this.label8.TabIndex = 48;
            this.label8.Text = "Focus :";
            // 
            // stopEvfButton
            // 
            this.stopEvfButton.Enabled = false;
            this.stopEvfButton.Location = new System.Drawing.Point(14, 405);
            this.stopEvfButton.Name = "stopEvfButton";
            this.stopEvfButton.Size = new System.Drawing.Size(108, 38);
            this.stopEvfButton.TabIndex = 46;
            this.stopEvfButton.Text = "Stop Live View";
            this.stopEvfButton.UseVisualStyleBackColor = true;
            // 
            // startEvfButton
            // 
            this.startEvfButton.Location = new System.Drawing.Point(14, 361);
            this.startEvfButton.Name = "startEvfButton";
            this.startEvfButton.Size = new System.Drawing.Size(108, 38);
            this.startEvfButton.TabIndex = 45;
            this.startEvfButton.Text = "Start Live View";
            this.startEvfButton.UseVisualStyleBackColor = true;
            // 
            // pictureBox
            // 
            this.pictureBox.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            this.pictureBox.Location = new System.Drawing.Point(397, 9);
            this.pictureBox.Name = "pictureBox";
            this.pictureBox.Size = new System.Drawing.Size(505, 336);
            this.pictureBox.TabIndex = 44;
            this.pictureBox.TabStop = false;
            // 
            // imageQualityComboBox
            // 
            this.imageQualityComboBox.Font = new System.Drawing.Font("Microsoft Sans Serif", 12F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.imageQualityComboBox.FormattingEnabled = true;
            this.imageQualityComboBox.Location = new System.Drawing.Point(113, 245);
            this.imageQualityComboBox.Name = "imageQualityComboBox";
            this.imageQualityComboBox.Size = new System.Drawing.Size(227, 28);
            this.imageQualityComboBox.TabIndex = 43;
            // 
            // exposureCompComboBox
            // 
            this.exposureCompComboBox.Font = new System.Drawing.Font("Microsoft Sans Serif", 12F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.exposureCompComboBox.FormattingEnabled = true;
            this.exposureCompComboBox.Location = new System.Drawing.Point(113, 211);
            this.exposureCompComboBox.Name = "exposureCompComboBox";
            this.exposureCompComboBox.Size = new System.Drawing.Size(227, 28);
            this.exposureCompComboBox.TabIndex = 42;
            // 
            // meteringComboBox
            // 
            this.meteringComboBox.Font = new System.Drawing.Font("Microsoft Sans Serif", 12F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.meteringComboBox.FormattingEnabled = true;
            this.meteringComboBox.Location = new System.Drawing.Point(113, 177);
            this.meteringComboBox.Name = "meteringComboBox";
            this.meteringComboBox.Size = new System.Drawing.Size(227, 28);
            this.meteringComboBox.TabIndex = 41;
            // 
            // avComboBox
            // 
            this.avComboBox.Font = new System.Drawing.Font("Microsoft Sans Serif", 12F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.avComboBox.FormattingEnabled = true;
            this.avComboBox.Location = new System.Drawing.Point(113, 143);
            this.avComboBox.Name = "avComboBox";
            this.avComboBox.Size = new System.Drawing.Size(227, 28);
            this.avComboBox.TabIndex = 40;
            // 
            // tvComboBox
            // 
            this.tvComboBox.Font = new System.Drawing.Font("Microsoft Sans Serif", 12F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.tvComboBox.FormattingEnabled = true;
            this.tvComboBox.Location = new System.Drawing.Point(113, 109);
            this.tvComboBox.Name = "tvComboBox";
            this.tvComboBox.Size = new System.Drawing.Size(227, 28);
            this.tvComboBox.TabIndex = 39;
            // 
            // aeModeComboBox
            // 
            this.aeModeComboBox.Font = new System.Drawing.Font("Microsoft Sans Serif", 12F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.aeModeComboBox.FormattingEnabled = true;
            this.aeModeComboBox.Location = new System.Drawing.Point(113, 76);
            this.aeModeComboBox.Name = "aeModeComboBox";
            this.aeModeComboBox.Size = new System.Drawing.Size(227, 28);
            this.aeModeComboBox.TabIndex = 38;
            // 
            // label7
            // 
            this.label7.AutoSize = true;
            this.label7.Font = new System.Drawing.Font("Microsoft Sans Serif", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.label7.Location = new System.Drawing.Point(12, 251);
            this.label7.Name = "label7";
            this.label7.Size = new System.Drawing.Size(88, 15);
            this.label7.TabIndex = 37;
            this.label7.Text = "Image Quality :";
            // 
            // label6
            // 
            this.label6.AutoSize = true;
            this.label6.Font = new System.Drawing.Font("Microsoft Sans Serif", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.label6.Location = new System.Drawing.Point(11, 217);
            this.label6.Name = "label6";
            this.label6.Size = new System.Drawing.Size(104, 15);
            this.label6.TabIndex = 36;
            this.label6.Text = "Exposure Comp. :";
            // 
            // label5
            // 
            this.label5.AutoSize = true;
            this.label5.Font = new System.Drawing.Font("Microsoft Sans Serif", 9.75F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.label5.Location = new System.Drawing.Point(12, 182);
            this.label5.Name = "label5";
            this.label5.Size = new System.Drawing.Size(104, 16);
            this.label5.TabIndex = 35;
            this.label5.Text = "Metering Mode :";
            // 
            // label4
            // 
            this.label4.AutoSize = true;
            this.label4.Font = new System.Drawing.Font("Microsoft Sans Serif", 12F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.label4.Location = new System.Drawing.Point(13, 148);
            this.label4.Name = "label4";
            this.label4.Size = new System.Drawing.Size(35, 20);
            this.label4.TabIndex = 34;
            this.label4.Text = "Av :";
            // 
            // label3
            // 
            this.label3.AutoSize = true;
            this.label3.Font = new System.Drawing.Font("Microsoft Sans Serif", 12F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.label3.Location = new System.Drawing.Point(13, 115);
            this.label3.Name = "label3";
            this.label3.Size = new System.Drawing.Size(33, 20);
            this.label3.TabIndex = 33;
            this.label3.Text = "Tv :";
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Font = new System.Drawing.Font("Microsoft Sans Serif", 11.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.label1.Location = new System.Drawing.Point(12, 80);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(73, 18);
            this.label1.TabIndex = 32;
            this.label1.Text = "AEMode :";
            // 
            // isoComboBox
            // 
            this.isoComboBox.Font = new System.Drawing.Font("Microsoft Sans Serif", 12F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.isoComboBox.FormattingEnabled = true;
            this.isoComboBox.Location = new System.Drawing.Point(113, 43);
            this.isoComboBox.Name = "isoComboBox";
            this.isoComboBox.Size = new System.Drawing.Size(227, 28);
            this.isoComboBox.TabIndex = 31;
            // 
            // label2
            // 
            this.label2.Font = new System.Drawing.Font("Microsoft Sans Serif", 11.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.label2.Location = new System.Drawing.Point(13, 47);
            this.label2.Name = "label2";
            this.label2.Size = new System.Drawing.Size(45, 18);
            this.label2.TabIndex = 30;
            this.label2.Text = "ISO : ";
            // 
            // ConfigurationButton
            // 
            this.ConfigurationButton.Location = new System.Drawing.Point(158, 405);
            this.ConfigurationButton.Name = "ConfigurationButton";
            this.ConfigurationButton.Size = new System.Drawing.Size(182, 38);
            this.ConfigurationButton.TabIndex = 59;
            this.ConfigurationButton.Text = "Configurations";
            this.ConfigurationButton.UseVisualStyleBackColor = true;
            this.ConfigurationButton.Click += new System.EventHandler(this.ConfigurationButton_Click);
            // 
            // CameraRingClientForm
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(979, 495);
            this.Controls.Add(this.panel1);
            this.Controls.Add(this.statusStripMain);
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedSingle;
            this.Name = "CameraRingClientForm";
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterScreen;
            this.Text = "Nomad Logic Camera Ring client manager";
            this.statusStripMain.ResumeLayout(false);
            this.statusStripMain.PerformLayout();
            this.panel1.ResumeLayout(false);
            this.panel1.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)(this.pictureBox)).EndInit();
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion
        private System.Windows.Forms.StatusStrip statusStripMain;
        private System.Windows.Forms.ToolStripStatusLabel toolStripStatusLabelStatusNetwork;
        private System.Windows.Forms.ToolStripStatusLabel toolStripStatusLabelLastTimeStamp;
        private System.Windows.Forms.ToolStripProgressBar toolStripProgressBarSendToServer;
        private Panel panel1;
        private ComboBox camerasComboBox;
        private Button copyPropertiesButton;
        private ComboBox whiteBalanceComboBox;
        private Label label10;
        private Label label9;
        private Button focus2RightButton;
        private Button focusRightButton;
        private Button focusLeftButton;
        private Button focus2LeftButton;
        private Label label8;
        private Button stopEvfButton;
        private Button startEvfButton;
        private PictureBox pictureBox;
        private ComboBox imageQualityComboBox;
        private ComboBox exposureCompComboBox;
        private ComboBox meteringComboBox;
        private ComboBox avComboBox;
        private ComboBox tvComboBox;
        private ComboBox aeModeComboBox;
        private Label label7;
        private Label label6;
        private Label label5;
        private Label label4;
        private Label label3;
        private Label label1;
        private ComboBox isoComboBox;
        private Label label2;
        private Button NextCameraButton;
        private Button PreviousCameraButton;
        private ToolStripStatusLabel toolStripStatusDuplicate;
        private Button ConfigurationButton;
    }
}

