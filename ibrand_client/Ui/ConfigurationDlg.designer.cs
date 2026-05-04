using System.ComponentModel;
using System.Windows.Forms;

namespace NomadLogic.iBrandDesktop.Ui
{
    partial class ConfigurationDlg
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
        protected override void InitializeComponent()
        {
            this.server_adr_label = new System.Windows.Forms.Label();
            this.serverAdrTextBox = new System.Windows.Forms.TextBox();
            this.server_port_label = new System.Windows.Forms.Label();
            this.serverPortTextBox = new System.Windows.Forms.TextBox();
            this.stream_port_label = new System.Windows.Forms.Label();
            this.streamPortTextBox = new System.Windows.Forms.TextBox();
            this.db_file_name_label = new System.Windows.Forms.Label();
            this.dbTextBox = new System.Windows.Forms.TextBox();
            this.db_button = new System.Windows.Forms.Button();
            this.picture_label = new System.Windows.Forms.Label();
            this.pictureDirTextBox = new System.Windows.Forms.TextBox();
            this.pictureDirButton = new System.Windows.Forms.Button();
            this.device_label = new System.Windows.Forms.Label();
            this.deviceTextBox = new System.Windows.Forms.TextBox();
            this.secure_port_label = new System.Windows.Forms.Label();
            this.securePortTextBox = new System.Windows.Forms.TextBox();
            this.secureCheckBox = new System.Windows.Forms.CheckBox();
            this.language_label = new System.Windows.Forms.Label();
            this.languageDropDown = new System.Windows.Forms.ComboBox();
            this.deletePictureCheckBox = new System.Windows.Forms.CheckBox();
            this.okButton = new System.Windows.Forms.Button();
            this.cancelButton = new System.Windows.Forms.Button();
            this.showKeyboardCheckBox = new System.Windows.Forms.CheckBox();
            this.videoDirButton = new System.Windows.Forms.Button();
            this.videoDirTextBox = new System.Windows.Forms.TextBox();
            this.video_label = new System.Windows.Forms.Label();
            this.bkDirButton = new System.Windows.Forms.Button();
            this.bkDeviceTextBox = new System.Windows.Forms.TextBox();
            this.bk_folder_label = new System.Windows.Forms.Label();
            this.videoSelectionButton = new System.Windows.Forms.Button();
            this.videoSelectiontextBox = new System.Windows.Forms.TextBox();
            this.video_selection_label = new System.Windows.Forms.Label();
            this.showPreviewOnTVCheckBox = new System.Windows.Forms.CheckBox();
            this.ppc_label = new System.Windows.Forms.Label();
            this.ppcTextBox = new System.Windows.Forms.TextBox();
            this.PrintOffsetXTextBox = new System.Windows.Forms.TextBox();
            this.PrintOffsetX_label = new System.Windows.Forms.Label();
            this.btnAddCustomPrintStation = new System.Windows.Forms.Button();
            this.btnPrinterSettings = new System.Windows.Forms.Button();
            this.btnEshotsSettings = new System.Windows.Forms.Button();
            this.SuspendLayout();
            // 
            // server_adr_label
            // 
            this.server_adr_label.Location = new System.Drawing.Point(5, 8);
            this.server_adr_label.Name = "server_adr_label";
            this.server_adr_label.Size = new System.Drawing.Size(179, 20);
            this.server_adr_label.TabIndex = 61;
            this.server_adr_label.Tag = "ServerAdr";
            // 
            // serverAdrTextBox
            // 
            this.serverAdrTextBox.Location = new System.Drawing.Point(230, 6);
            this.serverAdrTextBox.Name = "serverAdrTextBox";
            this.serverAdrTextBox.Size = new System.Drawing.Size(109, 20);
            this.serverAdrTextBox.TabIndex = 1;
            // 
            // server_port_label
            // 
            this.server_port_label.Location = new System.Drawing.Point(5, 33);
            this.server_port_label.Name = "server_port_label";
            this.server_port_label.Size = new System.Drawing.Size(179, 20);
            this.server_port_label.TabIndex = 60;
            // 
            // serverPortTextBox
            // 
            this.serverPortTextBox.Location = new System.Drawing.Point(230, 33);
            this.serverPortTextBox.Name = "serverPortTextBox";
            this.serverPortTextBox.Size = new System.Drawing.Size(109, 20);
            this.serverPortTextBox.TabIndex = 3;
            // 
            // stream_port_label
            // 
            this.stream_port_label.Location = new System.Drawing.Point(5, 63);
            this.stream_port_label.Name = "stream_port_label";
            this.stream_port_label.Size = new System.Drawing.Size(179, 20);
            this.stream_port_label.TabIndex = 59;
            // 
            // streamPortTextBox
            // 
            this.streamPortTextBox.Location = new System.Drawing.Point(230, 60);
            this.streamPortTextBox.Name = "streamPortTextBox";
            this.streamPortTextBox.Size = new System.Drawing.Size(109, 20);
            this.streamPortTextBox.TabIndex = 5;
            // 
            // db_file_name_label
            // 
            this.db_file_name_label.Location = new System.Drawing.Point(5, 140);
            this.db_file_name_label.Name = "db_file_name_label";
            this.db_file_name_label.Size = new System.Drawing.Size(179, 20);
            this.db_file_name_label.TabIndex = 58;
            // 
            // dbTextBox
            // 
            this.dbTextBox.Location = new System.Drawing.Point(197, 140);
            this.dbTextBox.Name = "dbTextBox";
            this.dbTextBox.Size = new System.Drawing.Size(104, 20);
            this.dbTextBox.TabIndex = 7;
            // 
            // db_button
            // 
            this.db_button.Location = new System.Drawing.Point(307, 140);
            this.db_button.Name = "db_button";
            this.db_button.Size = new System.Drawing.Size(32, 20);
            this.db_button.TabIndex = 8;
            this.db_button.Text = "...";
            // 
            // picture_label
            // 
            this.picture_label.Location = new System.Drawing.Point(5, 167);
            this.picture_label.Name = "picture_label";
            this.picture_label.Size = new System.Drawing.Size(179, 20);
            this.picture_label.TabIndex = 57;
            // 
            // pictureDirTextBox
            // 
            this.pictureDirTextBox.Location = new System.Drawing.Point(197, 167);
            this.pictureDirTextBox.Name = "pictureDirTextBox";
            this.pictureDirTextBox.Size = new System.Drawing.Size(104, 20);
            this.pictureDirTextBox.TabIndex = 10;
            // 
            // pictureDirButton
            // 
            this.pictureDirButton.Location = new System.Drawing.Point(307, 167);
            this.pictureDirButton.Name = "pictureDirButton";
            this.pictureDirButton.Size = new System.Drawing.Size(32, 20);
            this.pictureDirButton.TabIndex = 11;
            this.pictureDirButton.Text = "...";
            // 
            // device_label
            // 
            this.device_label.Location = new System.Drawing.Point(5, 115);
            this.device_label.Name = "device_label";
            this.device_label.Size = new System.Drawing.Size(179, 20);
            this.device_label.TabIndex = 56;
            // 
            // deviceTextBox
            // 
            this.deviceTextBox.Location = new System.Drawing.Point(213, 113);
            this.deviceTextBox.Name = "deviceTextBox";
            this.deviceTextBox.Size = new System.Drawing.Size(126, 20);
            this.deviceTextBox.TabIndex = 18;
            // 
            // secure_port_label
            // 
            this.secure_port_label.Location = new System.Drawing.Point(5, 86);
            this.secure_port_label.Name = "secure_port_label";
            this.secure_port_label.Size = new System.Drawing.Size(179, 20);
            this.secure_port_label.TabIndex = 55;
            // 
            // securePortTextBox
            // 
            this.securePortTextBox.Location = new System.Drawing.Point(230, 86);
            this.securePortTextBox.Name = "securePortTextBox";
            this.securePortTextBox.Size = new System.Drawing.Size(109, 20);
            this.securePortTextBox.TabIndex = 26;
            // 
            // secureCheckBox
            // 
            this.secureCheckBox.Location = new System.Drawing.Point(5, 362);
            this.secureCheckBox.Name = "secureCheckBox";
            this.secureCheckBox.Size = new System.Drawing.Size(333, 20);
            this.secureCheckBox.TabIndex = 34;
            // 
            // language_label
            // 
            this.language_label.Location = new System.Drawing.Point(5, 275);
            this.language_label.Name = "language_label";
            this.language_label.Size = new System.Drawing.Size(219, 21);
            this.language_label.TabIndex = 54;
            // 
            // languageDropDown
            // 
            this.languageDropDown.Items.AddRange(new object[] {
            "English",
            "Français"});
            this.languageDropDown.Location = new System.Drawing.Point(230, 275);
            this.languageDropDown.Name = "languageDropDown";
            this.languageDropDown.Size = new System.Drawing.Size(109, 21);
            this.languageDropDown.TabIndex = 44;
            // 
            // deletePictureCheckBox
            // 
            this.deletePictureCheckBox.Location = new System.Drawing.Point(5, 386);
            this.deletePictureCheckBox.Name = "deletePictureCheckBox";
            this.deletePictureCheckBox.Size = new System.Drawing.Size(333, 20);
            this.deletePictureCheckBox.TabIndex = 53;
            // 
            // okButton
            // 
            this.okButton.Location = new System.Drawing.Point(57, 614);
            this.okButton.Name = "okButton";
            this.okButton.Size = new System.Drawing.Size(88, 23);
            this.okButton.TabIndex = 62;
            this.okButton.Text = "OK";
            this.okButton.UseVisualStyleBackColor = true;
            // 
            // cancelButton
            // 
            this.cancelButton.Location = new System.Drawing.Point(197, 614);
            this.cancelButton.Name = "cancelButton";
            this.cancelButton.Size = new System.Drawing.Size(90, 23);
            this.cancelButton.TabIndex = 63;
            this.cancelButton.Text = "Cancel";
            this.cancelButton.UseVisualStyleBackColor = true;
            // 
            // showKeyboardCheckBox
            // 
            this.showKeyboardCheckBox.Location = new System.Drawing.Point(5, 411);
            this.showKeyboardCheckBox.Name = "showKeyboardCheckBox";
            this.showKeyboardCheckBox.Size = new System.Drawing.Size(333, 20);
            this.showKeyboardCheckBox.TabIndex = 64;
            // 
            // videoDirButton
            // 
            this.videoDirButton.Location = new System.Drawing.Point(306, 193);
            this.videoDirButton.Name = "videoDirButton";
            this.videoDirButton.Size = new System.Drawing.Size(32, 20);
            this.videoDirButton.TabIndex = 66;
            this.videoDirButton.Text = "...";
            // 
            // videoDirTextBox
            // 
            this.videoDirTextBox.Location = new System.Drawing.Point(197, 193);
            this.videoDirTextBox.Name = "videoDirTextBox";
            this.videoDirTextBox.Size = new System.Drawing.Size(103, 20);
            this.videoDirTextBox.TabIndex = 65;
            // 
            // video_label
            // 
            this.video_label.Location = new System.Drawing.Point(4, 193);
            this.video_label.Name = "video_label";
            this.video_label.Size = new System.Drawing.Size(180, 20);
            this.video_label.TabIndex = 67;
            // 
            // bkDirButton
            // 
            this.bkDirButton.Location = new System.Drawing.Point(306, 220);
            this.bkDirButton.Name = "bkDirButton";
            this.bkDirButton.Size = new System.Drawing.Size(32, 20);
            this.bkDirButton.TabIndex = 69;
            this.bkDirButton.Text = "...";
            // 
            // bkDeviceTextBox
            // 
            this.bkDeviceTextBox.Location = new System.Drawing.Point(197, 220);
            this.bkDeviceTextBox.Name = "bkDeviceTextBox";
            this.bkDeviceTextBox.Size = new System.Drawing.Size(103, 20);
            this.bkDeviceTextBox.TabIndex = 68;
            // 
            // bk_folder_label
            // 
            this.bk_folder_label.Location = new System.Drawing.Point(4, 220);
            this.bk_folder_label.Name = "bk_folder_label";
            this.bk_folder_label.Size = new System.Drawing.Size(180, 20);
            this.bk_folder_label.TabIndex = 70;
            // 
            // videoSelectionButton
            // 
            this.videoSelectionButton.Location = new System.Drawing.Point(306, 248);
            this.videoSelectionButton.Name = "videoSelectionButton";
            this.videoSelectionButton.Size = new System.Drawing.Size(32, 20);
            this.videoSelectionButton.TabIndex = 73;
            this.videoSelectionButton.Text = "...";
            // 
            // videoSelectiontextBox
            // 
            this.videoSelectiontextBox.Location = new System.Drawing.Point(197, 248);
            this.videoSelectiontextBox.Name = "videoSelectiontextBox";
            this.videoSelectiontextBox.Size = new System.Drawing.Size(103, 20);
            this.videoSelectiontextBox.TabIndex = 72;
            // 
            // video_selection_label
            // 
            this.video_selection_label.Location = new System.Drawing.Point(4, 248);
            this.video_selection_label.Name = "video_selection_label";
            this.video_selection_label.Size = new System.Drawing.Size(180, 20);
            this.video_selection_label.TabIndex = 74;
            // 
            // showPreviewOnTVCheckBox
            // 
            this.showPreviewOnTVCheckBox.Location = new System.Drawing.Point(5, 491);
            this.showPreviewOnTVCheckBox.Name = "showPreviewOnTVCheckBox";
            this.showPreviewOnTVCheckBox.Size = new System.Drawing.Size(333, 20);
            this.showPreviewOnTVCheckBox.TabIndex = 76;
            this.showPreviewOnTVCheckBox.Text = "Preview on tv";
            this.showPreviewOnTVCheckBox.UseVisualStyleBackColor = true;
            // 
            // ppc_label
            // 
            this.ppc_label.Location = new System.Drawing.Point(7, 304);
            this.ppc_label.Name = "ppc_label";
            this.ppc_label.Size = new System.Drawing.Size(217, 20);
            this.ppc_label.TabIndex = 77;
            // 
            // ppcTextBox
            // 
            this.ppcTextBox.Location = new System.Drawing.Point(230, 304);
            this.ppcTextBox.Name = "ppcTextBox";
            this.ppcTextBox.Size = new System.Drawing.Size(108, 20);
            this.ppcTextBox.TabIndex = 78;
            // 
            // PrintOffsetXTextBox
            // 
            this.PrintOffsetXTextBox.Location = new System.Drawing.Point(230, 332);
            this.PrintOffsetXTextBox.Name = "PrintOffsetXTextBox";
            this.PrintOffsetXTextBox.Size = new System.Drawing.Size(108, 20);
            this.PrintOffsetXTextBox.TabIndex = 93;
            // 
            // PrintOffsetX_label
            // 
            this.PrintOffsetX_label.Location = new System.Drawing.Point(7, 332);
            this.PrintOffsetX_label.Name = "PrintOffsetX_label";
            this.PrintOffsetX_label.Size = new System.Drawing.Size(217, 20);
            this.PrintOffsetX_label.TabIndex = 92;
            // 
            // btnAddCustomPrintStation
            // 
            this.btnAddCustomPrintStation.Location = new System.Drawing.Point(197, 517);
            this.btnAddCustomPrintStation.Name = "btnAddCustomPrintStation";
            this.btnAddCustomPrintStation.Size = new System.Drawing.Size(140, 32);
            this.btnAddCustomPrintStation.TabIndex = 94;
            this.btnAddCustomPrintStation.Text = "Custom Printstations";
            this.btnAddCustomPrintStation.UseVisualStyleBackColor = true;
            this.btnAddCustomPrintStation.Click += new System.EventHandler(this.btnAddCustomPrintStation_Click);
            // 
            // btnPrinterSettings
            // 
            this.btnPrinterSettings.Location = new System.Drawing.Point(5, 517);
            this.btnPrinterSettings.Name = "btnPrinterSettings";
            this.btnPrinterSettings.Size = new System.Drawing.Size(140, 32);
            this.btnPrinterSettings.TabIndex = 95;
            this.btnPrinterSettings.Text = "Printer settings";
            this.btnPrinterSettings.UseVisualStyleBackColor = true;
            // 
            // btnEshotsSettings
            // 
            this.btnEshotsSettings.Location = new System.Drawing.Point(5, 555);
            this.btnEshotsSettings.Name = "btnEshotsSettings";
            this.btnEshotsSettings.Size = new System.Drawing.Size(140, 32);
            this.btnEshotsSettings.TabIndex = 96;
            this.btnEshotsSettings.Text = "Eshots settings";
            this.btnEshotsSettings.UseVisualStyleBackColor = true;
            // 
            // ConfigurationDlg
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(96F, 96F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Dpi;
            this.ClientSize = new System.Drawing.Size(352, 649);
            this.ControlBox = false;
            this.Controls.Add(this.btnEshotsSettings);
            this.Controls.Add(this.btnPrinterSettings);
            this.Controls.Add(this.btnAddCustomPrintStation);
            this.Controls.Add(this.PrintOffsetXTextBox);
            this.Controls.Add(this.PrintOffsetX_label);
            this.Controls.Add(this.ppcTextBox);
            this.Controls.Add(this.ppc_label);
            this.Controls.Add(this.showPreviewOnTVCheckBox);
            this.Controls.Add(this.videoSelectionButton);
            this.Controls.Add(this.videoSelectiontextBox);
            this.Controls.Add(this.video_selection_label);
            this.Controls.Add(this.bkDirButton);
            this.Controls.Add(this.bkDeviceTextBox);
            this.Controls.Add(this.bk_folder_label);
            this.Controls.Add(this.videoDirButton);
            this.Controls.Add(this.videoDirTextBox);
            this.Controls.Add(this.video_label);
            this.Controls.Add(this.showKeyboardCheckBox);
            this.Controls.Add(this.cancelButton);
            this.Controls.Add(this.okButton);
            this.Controls.Add(this.deletePictureCheckBox);
            this.Controls.Add(this.languageDropDown);
            this.Controls.Add(this.language_label);
            this.Controls.Add(this.secureCheckBox);
            this.Controls.Add(this.securePortTextBox);
            this.Controls.Add(this.secure_port_label);
            this.Controls.Add(this.deviceTextBox);
            this.Controls.Add(this.device_label);
            this.Controls.Add(this.pictureDirButton);
            this.Controls.Add(this.pictureDirTextBox);
            this.Controls.Add(this.picture_label);
            this.Controls.Add(this.db_button);
            this.Controls.Add(this.dbTextBox);
            this.Controls.Add(this.db_file_name_label);
            this.Controls.Add(this.streamPortTextBox);
            this.Controls.Add(this.stream_port_label);
            this.Controls.Add(this.serverPortTextBox);
            this.Controls.Add(this.server_port_label);
            this.Controls.Add(this.serverAdrTextBox);
            this.Controls.Add(this.server_adr_label);
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedDialog;
            this.Name = "ConfigurationDlg";
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterParent;
            this.Text = "iBrand (tm)";
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private Label server_adr_label;
        private TextBox serverAdrTextBox;
        private Label server_port_label;
        private TextBox serverPortTextBox;
        private Label stream_port_label;
        private TextBox streamPortTextBox;
        private Label db_file_name_label;
        private TextBox dbTextBox;
        private Button db_button;
        private Label picture_label;
        private TextBox pictureDirTextBox;
        private Button pictureDirButton;
        //private Microsoft.WindowsCE.Forms.InputPanel inputPanel;
        private Label device_label;
        private TextBox deviceTextBox;
        private Label secure_port_label;
        private TextBox securePortTextBox;
        private CheckBox secureCheckBox;
        private Label language_label;
        private ComboBox languageDropDown;
        private CheckBox deletePictureCheckBox;
        private Button okButton;
        private Button cancelButton;
        private CheckBox showKeyboardCheckBox;
        private Button videoDirButton;
        private TextBox videoDirTextBox;
        private Label video_label;
        private Button bkDirButton;
        private TextBox bkDeviceTextBox;
        private Label bk_folder_label;
        private Button videoSelectionButton;
        private TextBox videoSelectiontextBox;
        private Label video_selection_label;
        private CheckBox showPreviewOnTVCheckBox;
        private Label ppc_label;
        private TextBox ppcTextBox;
        private TextBox PrintOffsetXTextBox;
        private Label PrintOffsetX_label;
        private Button btnAddCustomPrintStation;
        private Button btnPrinterSettings;
        private Button btnEshotsSettings;
    }
}