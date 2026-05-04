namespace iBrand_SnapShot.UI
{
    partial class MainFormUi
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
            this.components = new System.ComponentModel.Container();
            this.EventsLogLabel = new System.Windows.Forms.Label();
            this.EventsLog = new System.Windows.Forms.TextBox();
            this.TriggerTestBtn = new System.Windows.Forms.Button();
            this.CamStatusLbl = new System.Windows.Forms.Label();
            this.CamStatus = new System.Windows.Forms.Label();
            this.FtpStatusLbl = new System.Windows.Forms.Label();
            this.FtpStatus = new System.Windows.Forms.Label();
            this.FtpTestBtn = new System.Windows.Forms.Button();
            this.CamTestBtn = new System.Windows.Forms.Button();
            this.TrigStatusLbl = new System.Windows.Forms.Label();
            this.TrigStatus = new System.Windows.Forms.Label();
            this.MainTabControl = new System.Windows.Forms.TabControl();
            this.TabStatus = new System.Windows.Forms.TabPage();
            this.DisplayAlive = new System.Windows.Forms.Label();
            this.CustomWorkerStatus = new System.Windows.Forms.Label();
            this.CustomWorkerStatusLbl = new System.Windows.Forms.Label();
            this.ProcessFilesBtn = new System.Windows.Forms.Button();
            this.CamActivateBtn = new System.Windows.Forms.Button();
            this.SettingsCameraTab = new System.Windows.Forms.TabPage();
            this.SettingsWorkerNumberCustomLbl = new System.Windows.Forms.Label();
            this.SettingsWorkerNumberCustom = new System.Windows.Forms.TextBox();
            this.SettingsWorkerNumberFtpLbl = new System.Windows.Forms.Label();
            this.SettingsWorkerNumberFtp = new System.Windows.Forms.TextBox();
            this.SettingsEnableFtp = new System.Windows.Forms.CheckBox();
            this.SettingsEnableFtpLbl = new System.Windows.Forms.Label();
            this.SettingsTriggerTimeLbl = new System.Windows.Forms.Label();
            this.SettingsTriggerTime = new System.Windows.Forms.TextBox();
            this.SettingsWorkerClassLbl = new System.Windows.Forms.Label();
            this.SettingsWorkerClass = new System.Windows.Forms.TextBox();
            this.SettingsVerbose = new System.Windows.Forms.CheckBox();
            this.SettingsVerboseLbl = new System.Windows.Forms.Label();
            this.SettingsFfmpegLocationLbl = new System.Windows.Forms.Label();
            this.SettingsFfmpegLocation = new System.Windows.Forms.TextBox();
            this.SettingsFtpServerBkp = new System.Windows.Forms.TextBox();
            this.SettingsFtpServerBkpLbl = new System.Windows.Forms.Label();
            this.SettingsTrigParamsLbl = new System.Windows.Forms.Label();
            this.SettingsFtpServer = new System.Windows.Forms.TextBox();
            this.panel1 = new System.Windows.Forms.Panel();
            this.SettingsCameraVideoFileSize = new System.Windows.Forms.TextBox();
            this.SettingsCameraVideoFileSizeLbl = new System.Windows.Forms.Label();
            this.SettingsCameraVideoThumbnail = new System.Windows.Forms.TextBox();
            this.SettingsCameraVideoThumbnailLbl = new System.Windows.Forms.Label();
            this.SettingsCameraVideoDuration = new System.Windows.Forms.TextBox();
            this.SettingsCameraOptionsLbl = new System.Windows.Forms.Label();
            this.SettingsCameraRadioPicture = new System.Windows.Forms.RadioButton();
            this.SettingsCameraRadioVideo = new System.Windows.Forms.RadioButton();
            this.SettingsCameraVideoDurationLbl = new System.Windows.Forms.Label();
            this.SettingsTrigParams = new System.Windows.Forms.TextBox();
            this.SettingsFtpDir = new System.Windows.Forms.TextBox();
            this.SettingsFtpArchiveLbl = new System.Windows.Forms.Label();
            this.SettingsTrigClass = new System.Windows.Forms.TextBox();
            this.SettingsFtpDirLbl = new System.Windows.Forms.Label();
            this.SettingsFtpArchive = new System.Windows.Forms.TextBox();
            this.SettingsTrigClassLbl = new System.Windows.Forms.Label();
            this.SettingsWriteToFileBtn = new System.Windows.Forms.Button();
            this.SettingsResetButton = new System.Windows.Forms.Button();
            this.SettingsFtpServerLbl = new System.Windows.Forms.Label();
            this.ToolTip = new System.Windows.Forms.ToolTip(this.components);
            this.MainTabControl.SuspendLayout();
            this.TabStatus.SuspendLayout();
            this.SettingsCameraTab.SuspendLayout();
            this.panel1.SuspendLayout();
            this.SuspendLayout();
            // 
            // EventsLogLabel
            // 
            this.EventsLogLabel.AutoSize = true;
            this.EventsLogLabel.Location = new System.Drawing.Point(485, 6);
            this.EventsLogLabel.Name = "EventsLogLabel";
            this.EventsLogLabel.Size = new System.Drawing.Size(61, 13);
            this.EventsLogLabel.TabIndex = 0;
            this.EventsLogLabel.Text = "Events Log";
            // 
            // EventsLog
            // 
            this.EventsLog.Location = new System.Drawing.Point(488, 22);
            this.EventsLog.Multiline = true;
            this.EventsLog.Name = "EventsLog";
            this.EventsLog.ReadOnly = true;
            this.EventsLog.ScrollBars = System.Windows.Forms.ScrollBars.Both;
            this.EventsLog.Size = new System.Drawing.Size(453, 402);
            this.EventsLog.TabIndex = 1;
            this.EventsLog.TabStop = false;
            this.EventsLog.WordWrap = false;
            // 
            // TriggerTestBtn
            // 
            this.TriggerTestBtn.Location = new System.Drawing.Point(368, 65);
            this.TriggerTestBtn.Name = "TriggerTestBtn";
            this.TriggerTestBtn.Size = new System.Drawing.Size(100, 23);
            this.TriggerTestBtn.TabIndex = 8;
            this.TriggerTestBtn.Text = "Test Trigger";
            this.ToolTip.SetToolTip(this.TriggerTestBtn, "Switch between test and normal mode for the trigger mechanism. Test mode only wri" +
        "tes event logs.");
            this.TriggerTestBtn.UseVisualStyleBackColor = true;
            this.TriggerTestBtn.Click += new System.EventHandler(this.TriggerTestBtn_Click);
            // 
            // CamStatusLbl
            // 
            this.CamStatusLbl.AutoSize = true;
            this.CamStatusLbl.Location = new System.Drawing.Point(13, 11);
            this.CamStatusLbl.Name = "CamStatusLbl";
            this.CamStatusLbl.Size = new System.Drawing.Size(82, 13);
            this.CamStatusLbl.TabIndex = 2;
            this.CamStatusLbl.Text = "Camera Status :";
            this.CamStatusLbl.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
            // 
            // CamStatus
            // 
            this.CamStatus.AutoSize = true;
            this.CamStatus.ForeColor = System.Drawing.Color.DarkRed;
            this.CamStatus.Location = new System.Drawing.Point(116, 11);
            this.CamStatus.Name = "CamStatus";
            this.CamStatus.Size = new System.Drawing.Size(53, 13);
            this.CamStatus.TabIndex = 3;
            this.CamStatus.Text = "Unknown";
            // 
            // FtpStatusLbl
            // 
            this.FtpStatusLbl.AutoSize = true;
            this.FtpStatusLbl.Location = new System.Drawing.Point(34, 40);
            this.FtpStatusLbl.Name = "FtpStatusLbl";
            this.FtpStatusLbl.Size = new System.Drawing.Size(61, 13);
            this.FtpStatusLbl.TabIndex = 4;
            this.FtpStatusLbl.Text = "Ftp Status :";
            this.FtpStatusLbl.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
            // 
            // FtpStatus
            // 
            this.FtpStatus.AutoSize = true;
            this.FtpStatus.ForeColor = System.Drawing.Color.DarkRed;
            this.FtpStatus.Location = new System.Drawing.Point(116, 40);
            this.FtpStatus.Name = "FtpStatus";
            this.FtpStatus.Size = new System.Drawing.Size(53, 13);
            this.FtpStatus.TabIndex = 5;
            this.FtpStatus.Text = "Unknown";
            // 
            // FtpTestBtn
            // 
            this.FtpTestBtn.Location = new System.Drawing.Point(368, 36);
            this.FtpTestBtn.Name = "FtpTestBtn";
            this.FtpTestBtn.Size = new System.Drawing.Size(100, 23);
            this.FtpTestBtn.TabIndex = 7;
            this.FtpTestBtn.Text = "Test Ftp connection";
            this.ToolTip.SetToolTip(this.FtpTestBtn, "If not working, re-tests the Ftp configuration.");
            this.FtpTestBtn.UseVisualStyleBackColor = true;
            this.FtpTestBtn.Click += new System.EventHandler(this.FtpTestBtn_Click);
            // 
            // CamTestBtn
            // 
            this.CamTestBtn.Location = new System.Drawing.Point(368, 7);
            this.CamTestBtn.Name = "CamTestBtn";
            this.CamTestBtn.Size = new System.Drawing.Size(100, 23);
            this.CamTestBtn.TabIndex = 6;
            this.CamTestBtn.Text = "Test Camera";
            this.ToolTip.SetToolTip(this.CamTestBtn, "Checks if the camera is accessible.");
            this.CamTestBtn.UseVisualStyleBackColor = true;
            this.CamTestBtn.Click += new System.EventHandler(this.CamTestBtn_Click);
            // 
            // TrigStatusLbl
            // 
            this.TrigStatusLbl.AutoSize = true;
            this.TrigStatusLbl.Location = new System.Drawing.Point(16, 70);
            this.TrigStatusLbl.Name = "TrigStatusLbl";
            this.TrigStatusLbl.Size = new System.Drawing.Size(79, 13);
            this.TrigStatusLbl.TabIndex = 9;
            this.TrigStatusLbl.Text = "Trigger Status :";
            this.TrigStatusLbl.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
            // 
            // TrigStatus
            // 
            this.TrigStatus.AutoSize = true;
            this.TrigStatus.ForeColor = System.Drawing.Color.DarkRed;
            this.TrigStatus.Location = new System.Drawing.Point(116, 70);
            this.TrigStatus.Name = "TrigStatus";
            this.TrigStatus.Size = new System.Drawing.Size(53, 13);
            this.TrigStatus.TabIndex = 10;
            this.TrigStatus.Text = "Unknown";
            // 
            // MainTabControl
            // 
            this.MainTabControl.Controls.Add(this.TabStatus);
            this.MainTabControl.Controls.Add(this.SettingsCameraTab);
            this.MainTabControl.Location = new System.Drawing.Point(0, 0);
            this.MainTabControl.Name = "MainTabControl";
            this.MainTabControl.SelectedIndex = 0;
            this.MainTabControl.Size = new System.Drawing.Size(482, 424);
            this.MainTabControl.TabIndex = 13;
            // 
            // TabStatus
            // 
            this.TabStatus.Controls.Add(this.DisplayAlive);
            this.TabStatus.Controls.Add(this.CustomWorkerStatus);
            this.TabStatus.Controls.Add(this.CustomWorkerStatusLbl);
            this.TabStatus.Controls.Add(this.ProcessFilesBtn);
            this.TabStatus.Controls.Add(this.CamActivateBtn);
            this.TabStatus.Controls.Add(this.TrigStatus);
            this.TabStatus.Controls.Add(this.TriggerTestBtn);
            this.TabStatus.Controls.Add(this.FtpTestBtn);
            this.TabStatus.Controls.Add(this.CamStatusLbl);
            this.TabStatus.Controls.Add(this.FtpStatus);
            this.TabStatus.Controls.Add(this.CamStatus);
            this.TabStatus.Controls.Add(this.CamTestBtn);
            this.TabStatus.Controls.Add(this.TrigStatusLbl);
            this.TabStatus.Controls.Add(this.FtpStatusLbl);
            this.TabStatus.Location = new System.Drawing.Point(4, 22);
            this.TabStatus.Name = "TabStatus";
            this.TabStatus.Padding = new System.Windows.Forms.Padding(3);
            this.TabStatus.Size = new System.Drawing.Size(474, 398);
            this.TabStatus.TabIndex = 0;
            this.TabStatus.Text = "Status";
            this.TabStatus.UseVisualStyleBackColor = true;
            // 
            // DisplayAlive
            // 
            this.DisplayAlive.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
            this.DisplayAlive.AutoSize = true;
            this.DisplayAlive.Font = new System.Drawing.Font("Courier New", 15.75F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.DisplayAlive.Location = new System.Drawing.Point(3, 372);
            this.DisplayAlive.Name = "DisplayAlive";
            this.DisplayAlive.Size = new System.Drawing.Size(23, 23);
            this.DisplayAlive.TabIndex = 16;
            this.DisplayAlive.Text = "*";
            this.DisplayAlive.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
            // 
            // CustomWorkerStatus
            // 
            this.CustomWorkerStatus.AutoSize = true;
            this.CustomWorkerStatus.ForeColor = System.Drawing.Color.DarkRed;
            this.CustomWorkerStatus.Location = new System.Drawing.Point(114, 99);
            this.CustomWorkerStatus.Name = "CustomWorkerStatus";
            this.CustomWorkerStatus.Size = new System.Drawing.Size(53, 13);
            this.CustomWorkerStatus.TabIndex = 15;
            this.CustomWorkerStatus.Text = "Unknown";
            // 
            // CustomWorkerStatusLbl
            // 
            this.CustomWorkerStatusLbl.AutoSize = true;
            this.CustomWorkerStatusLbl.Location = new System.Drawing.Point(8, 99);
            this.CustomWorkerStatusLbl.Name = "CustomWorkerStatusLbl";
            this.CustomWorkerStatusLbl.Size = new System.Drawing.Size(88, 13);
            this.CustomWorkerStatusLbl.TabIndex = 14;
            this.CustomWorkerStatusLbl.Text = "Custom workers :";
            this.CustomWorkerStatusLbl.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
            // 
            // ProcessFilesBtn
            // 
            this.ProcessFilesBtn.Enabled = false;
            this.ProcessFilesBtn.Location = new System.Drawing.Point(233, 369);
            this.ProcessFilesBtn.Name = "ProcessFilesBtn";
            this.ProcessFilesBtn.Size = new System.Drawing.Size(129, 23);
            this.ProcessFilesBtn.TabIndex = 12;
            this.ProcessFilesBtn.Text = "Process leftover files";
            this.ToolTip.SetToolTip(this.ProcessFilesBtn, "Queue every media file (pictures and videos) not yet processed.");
            this.ProcessFilesBtn.UseVisualStyleBackColor = true;
            this.ProcessFilesBtn.Click += new System.EventHandler(this.UploadFilesBtn_Click);
            // 
            // CamActivateBtn
            // 
            this.CamActivateBtn.Enabled = false;
            this.CamActivateBtn.Location = new System.Drawing.Point(368, 369);
            this.CamActivateBtn.Name = "CamActivateBtn";
            this.CamActivateBtn.Size = new System.Drawing.Size(99, 23);
            this.CamActivateBtn.TabIndex = 11;
            this.CamActivateBtn.Text = "Activate Camera";
            this.ToolTip.SetToolTip(this.CamActivateBtn, "Manually trigger the camera.");
            this.CamActivateBtn.UseVisualStyleBackColor = true;
            this.CamActivateBtn.Click += new System.EventHandler(this.CamActivateBtn_Click);
            // 
            // SettingsCameraTab
            // 
            this.SettingsCameraTab.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            this.SettingsCameraTab.Controls.Add(this.SettingsWorkerNumberCustomLbl);
            this.SettingsCameraTab.Controls.Add(this.SettingsWorkerNumberCustom);
            this.SettingsCameraTab.Controls.Add(this.SettingsWorkerNumberFtpLbl);
            this.SettingsCameraTab.Controls.Add(this.SettingsWorkerNumberFtp);
            this.SettingsCameraTab.Controls.Add(this.SettingsEnableFtp);
            this.SettingsCameraTab.Controls.Add(this.SettingsEnableFtpLbl);
            this.SettingsCameraTab.Controls.Add(this.SettingsTriggerTimeLbl);
            this.SettingsCameraTab.Controls.Add(this.SettingsTriggerTime);
            this.SettingsCameraTab.Controls.Add(this.SettingsWorkerClassLbl);
            this.SettingsCameraTab.Controls.Add(this.SettingsWorkerClass);
            this.SettingsCameraTab.Controls.Add(this.SettingsVerbose);
            this.SettingsCameraTab.Controls.Add(this.SettingsVerboseLbl);
            this.SettingsCameraTab.Controls.Add(this.SettingsFfmpegLocationLbl);
            this.SettingsCameraTab.Controls.Add(this.SettingsFfmpegLocation);
            this.SettingsCameraTab.Controls.Add(this.SettingsFtpServerBkp);
            this.SettingsCameraTab.Controls.Add(this.SettingsFtpServerBkpLbl);
            this.SettingsCameraTab.Controls.Add(this.SettingsTrigParamsLbl);
            this.SettingsCameraTab.Controls.Add(this.SettingsFtpServer);
            this.SettingsCameraTab.Controls.Add(this.panel1);
            this.SettingsCameraTab.Controls.Add(this.SettingsTrigParams);
            this.SettingsCameraTab.Controls.Add(this.SettingsFtpDir);
            this.SettingsCameraTab.Controls.Add(this.SettingsFtpArchiveLbl);
            this.SettingsCameraTab.Controls.Add(this.SettingsTrigClass);
            this.SettingsCameraTab.Controls.Add(this.SettingsFtpDirLbl);
            this.SettingsCameraTab.Controls.Add(this.SettingsFtpArchive);
            this.SettingsCameraTab.Controls.Add(this.SettingsTrigClassLbl);
            this.SettingsCameraTab.Controls.Add(this.SettingsWriteToFileBtn);
            this.SettingsCameraTab.Controls.Add(this.SettingsResetButton);
            this.SettingsCameraTab.Controls.Add(this.SettingsFtpServerLbl);
            this.SettingsCameraTab.Location = new System.Drawing.Point(4, 22);
            this.SettingsCameraTab.Name = "SettingsCameraTab";
            this.SettingsCameraTab.Padding = new System.Windows.Forms.Padding(3);
            this.SettingsCameraTab.Size = new System.Drawing.Size(474, 398);
            this.SettingsCameraTab.TabIndex = 1;
            this.SettingsCameraTab.Text = "Settings";
            this.SettingsCameraTab.UseVisualStyleBackColor = true;
            // 
            // SettingsWorkerNumberCustomLbl
            // 
            this.SettingsWorkerNumberCustomLbl.AutoSize = true;
            this.SettingsWorkerNumberCustomLbl.Location = new System.Drawing.Point(266, 301);
            this.SettingsWorkerNumberCustomLbl.Name = "SettingsWorkerNumberCustomLbl";
            this.SettingsWorkerNumberCustomLbl.Size = new System.Drawing.Size(117, 13);
            this.SettingsWorkerNumberCustomLbl.TabIndex = 31;
            this.SettingsWorkerNumberCustomLbl.Text = "Concurrency (Worker) :";
            this.SettingsWorkerNumberCustomLbl.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
            // 
            // SettingsWorkerNumberCustom
            // 
            this.SettingsWorkerNumberCustom.Location = new System.Drawing.Point(426, 298);
            this.SettingsWorkerNumberCustom.Name = "SettingsWorkerNumberCustom";
            this.SettingsWorkerNumberCustom.Size = new System.Drawing.Size(40, 20);
            this.SettingsWorkerNumberCustom.TabIndex = 30;
            this.SettingsWorkerNumberCustom.Text = "1";
            this.ToolTip.SetToolTip(this.SettingsWorkerNumberCustom, "Number of simultaneous custom workers.");
            this.SettingsWorkerNumberCustom.TextChanged += new System.EventHandler(this.SettingModified);
            this.SettingsWorkerNumberCustom.KeyPress += new System.Windows.Forms.KeyPressEventHandler(this.NumberOnly_KeyPress);
            // 
            // SettingsWorkerNumberFtpLbl
            // 
            this.SettingsWorkerNumberFtpLbl.AutoSize = true;
            this.SettingsWorkerNumberFtpLbl.Location = new System.Drawing.Point(266, 281);
            this.SettingsWorkerNumberFtpLbl.Name = "SettingsWorkerNumberFtpLbl";
            this.SettingsWorkerNumberFtpLbl.Size = new System.Drawing.Size(102, 13);
            this.SettingsWorkerNumberFtpLbl.TabIndex = 29;
            this.SettingsWorkerNumberFtpLbl.Text = "Concurrency (FTP) :";
            this.SettingsWorkerNumberFtpLbl.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
            // 
            // SettingsWorkerNumberFtp
            // 
            this.SettingsWorkerNumberFtp.Location = new System.Drawing.Point(426, 278);
            this.SettingsWorkerNumberFtp.Name = "SettingsWorkerNumberFtp";
            this.SettingsWorkerNumberFtp.Size = new System.Drawing.Size(40, 20);
            this.SettingsWorkerNumberFtp.TabIndex = 28;
            this.SettingsWorkerNumberFtp.Text = "5";
            this.ToolTip.SetToolTip(this.SettingsWorkerNumberFtp, "Number of simultaneous FTP jobs.");
            this.SettingsWorkerNumberFtp.TextChanged += new System.EventHandler(this.SettingModified);
            this.SettingsWorkerNumberFtp.KeyPress += new System.Windows.Forms.KeyPressEventHandler(this.NumberOnly_KeyPress);
            // 
            // SettingsEnableFtp
            // 
            this.SettingsEnableFtp.AutoSize = true;
            this.SettingsEnableFtp.Location = new System.Drawing.Point(104, 35);
            this.SettingsEnableFtp.Name = "SettingsEnableFtp";
            this.SettingsEnableFtp.Size = new System.Drawing.Size(15, 14);
            this.SettingsEnableFtp.TabIndex = 27;
            this.SettingsEnableFtp.UseVisualStyleBackColor = true;
            // 
            // SettingsEnableFtpLbl
            // 
            this.SettingsEnableFtpLbl.AutoSize = true;
            this.SettingsEnableFtpLbl.Location = new System.Drawing.Point(37, 36);
            this.SettingsEnableFtpLbl.Name = "SettingsEnableFtpLbl";
            this.SettingsEnableFtpLbl.Size = new System.Drawing.Size(64, 13);
            this.SettingsEnableFtpLbl.TabIndex = 26;
            this.SettingsEnableFtpLbl.Text = "Enable Ftp :";
            this.SettingsEnableFtpLbl.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
            // 
            // SettingsTriggerTimeLbl
            // 
            this.SettingsTriggerTimeLbl.AutoSize = true;
            this.SettingsTriggerTimeLbl.Location = new System.Drawing.Point(32, 213);
            this.SettingsTriggerTimeLbl.Name = "SettingsTriggerTimeLbl";
            this.SettingsTriggerTimeLbl.Size = new System.Drawing.Size(68, 13);
            this.SettingsTriggerTimeLbl.TabIndex = 25;
            this.SettingsTriggerTimeLbl.Text = "Trigger time :";
            this.SettingsTriggerTimeLbl.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
            // 
            // SettingsTriggerTime
            // 
            this.SettingsTriggerTime.Location = new System.Drawing.Point(103, 210);
            this.SettingsTriggerTime.Name = "SettingsTriggerTime";
            this.SettingsTriggerTime.Size = new System.Drawing.Size(363, 20);
            this.SettingsTriggerTime.TabIndex = 24;
            this.SettingsTriggerTime.Text = "(None)";
            this.ToolTip.SetToolTip(this.SettingsTriggerTime, "Start time and end time for triggers each day (24 hour format). HH:MM-HH:MM)");
            this.SettingsTriggerTime.TextChanged += new System.EventHandler(this.SettingModified);
            // 
            // SettingsWorkerClassLbl
            // 
            this.SettingsWorkerClassLbl.AutoSize = true;
            this.SettingsWorkerClassLbl.Location = new System.Drawing.Point(25, 177);
            this.SettingsWorkerClassLbl.Name = "SettingsWorkerClassLbl";
            this.SettingsWorkerClassLbl.Size = new System.Drawing.Size(76, 13);
            this.SettingsWorkerClassLbl.TabIndex = 23;
            this.SettingsWorkerClassLbl.Text = "Worker Class :";
            this.SettingsWorkerClassLbl.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
            // 
            // SettingsWorkerClass
            // 
            this.SettingsWorkerClass.Location = new System.Drawing.Point(103, 174);
            this.SettingsWorkerClass.Name = "SettingsWorkerClass";
            this.SettingsWorkerClass.Size = new System.Drawing.Size(363, 20);
            this.SettingsWorkerClass.TabIndex = 22;
            this.SettingsWorkerClass.Text = "(None)";
            this.ToolTip.SetToolTip(this.SettingsWorkerClass, "Name of the custom worker class for each file captured.");
            this.SettingsWorkerClass.TextChanged += new System.EventHandler(this.SettingModified);
            // 
            // SettingsVerbose
            // 
            this.SettingsVerbose.AutoSize = true;
            this.SettingsVerbose.Location = new System.Drawing.Point(103, 5);
            this.SettingsVerbose.Name = "SettingsVerbose";
            this.SettingsVerbose.Size = new System.Drawing.Size(15, 14);
            this.SettingsVerbose.TabIndex = 21;
            this.SettingsVerbose.UseVisualStyleBackColor = true;
            // 
            // SettingsVerboseLbl
            // 
            this.SettingsVerboseLbl.AutoSize = true;
            this.SettingsVerboseLbl.Location = new System.Drawing.Point(31, 6);
            this.SettingsVerboseLbl.Name = "SettingsVerboseLbl";
            this.SettingsVerboseLbl.Size = new System.Drawing.Size(69, 13);
            this.SettingsVerboseLbl.TabIndex = 20;
            this.SettingsVerboseLbl.Text = "Verbose log :";
            this.SettingsVerboseLbl.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
            // 
            // SettingsFfmpegLocationLbl
            // 
            this.SettingsFfmpegLocationLbl.AutoSize = true;
            this.SettingsFfmpegLocationLbl.Location = new System.Drawing.Point(2, 156);
            this.SettingsFfmpegLocationLbl.Name = "SettingsFfmpegLocationLbl";
            this.SettingsFfmpegLocationLbl.Size = new System.Drawing.Size(99, 13);
            this.SettingsFfmpegLocationLbl.TabIndex = 19;
            this.SettingsFfmpegLocationLbl.Text = "FF MPEG location :";
            this.SettingsFfmpegLocationLbl.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
            // 
            // SettingsFfmpegLocation
            // 
            this.SettingsFfmpegLocation.Location = new System.Drawing.Point(103, 153);
            this.SettingsFfmpegLocation.Name = "SettingsFfmpegLocation";
            this.SettingsFfmpegLocation.Size = new System.Drawing.Size(363, 20);
            this.SettingsFfmpegLocation.TabIndex = 18;
            this.SettingsFfmpegLocation.Text = "(None)";
            this.ToolTip.SetToolTip(this.SettingsFfmpegLocation, "Location of \"ffmpeg.exe\"");
            this.SettingsFfmpegLocation.TextChanged += new System.EventHandler(this.SettingModified);
            // 
            // SettingsFtpServerBkp
            // 
            this.SettingsFtpServerBkp.Enabled = false;
            this.SettingsFtpServerBkp.Location = new System.Drawing.Point(103, 73);
            this.SettingsFtpServerBkp.Name = "SettingsFtpServerBkp";
            this.SettingsFtpServerBkp.ReadOnly = true;
            this.SettingsFtpServerBkp.Size = new System.Drawing.Size(363, 20);
            this.SettingsFtpServerBkp.TabIndex = 17;
            this.SettingsFtpServerBkp.TabStop = false;
            this.SettingsFtpServerBkp.Text = "Unknown";
            this.ToolTip.SetToolTip(this.SettingsFtpServerBkp, "The URI of the backup Ftp server.");
            // 
            // SettingsFtpServerBkpLbl
            // 
            this.SettingsFtpServerBkpLbl.AutoSize = true;
            this.SettingsFtpServerBkpLbl.Location = new System.Drawing.Point(14, 76);
            this.SettingsFtpServerBkpLbl.Name = "SettingsFtpServerBkpLbl";
            this.SettingsFtpServerBkpLbl.Size = new System.Drawing.Size(87, 13);
            this.SettingsFtpServerBkpLbl.TabIndex = 16;
            this.SettingsFtpServerBkpLbl.Text = "Ftp (bkp) server :";
            this.SettingsFtpServerBkpLbl.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
            // 
            // SettingsTrigParamsLbl
            // 
            this.SettingsTrigParamsLbl.AutoSize = true;
            this.SettingsTrigParamsLbl.Location = new System.Drawing.Point(17, 255);
            this.SettingsTrigParamsLbl.Name = "SettingsTrigParamsLbl";
            this.SettingsTrigParamsLbl.Size = new System.Drawing.Size(84, 13);
            this.SettingsTrigParamsLbl.TabIndex = 15;
            this.SettingsTrigParamsLbl.Text = "Trigger Params :";
            this.SettingsTrigParamsLbl.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
            // 
            // SettingsFtpServer
            // 
            this.SettingsFtpServer.Enabled = false;
            this.SettingsFtpServer.Location = new System.Drawing.Point(103, 52);
            this.SettingsFtpServer.Name = "SettingsFtpServer";
            this.SettingsFtpServer.ReadOnly = true;
            this.SettingsFtpServer.Size = new System.Drawing.Size(363, 20);
            this.SettingsFtpServer.TabIndex = 1;
            this.SettingsFtpServer.TabStop = false;
            this.SettingsFtpServer.Text = "Unknown";
            this.ToolTip.SetToolTip(this.SettingsFtpServer, "The URI of the Ftp server.");
            // 
            // panel1
            // 
            this.panel1.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            this.panel1.Controls.Add(this.SettingsCameraVideoFileSize);
            this.panel1.Controls.Add(this.SettingsCameraVideoFileSizeLbl);
            this.panel1.Controls.Add(this.SettingsCameraVideoThumbnail);
            this.panel1.Controls.Add(this.SettingsCameraVideoThumbnailLbl);
            this.panel1.Controls.Add(this.SettingsCameraVideoDuration);
            this.panel1.Controls.Add(this.SettingsCameraOptionsLbl);
            this.panel1.Controls.Add(this.SettingsCameraRadioPicture);
            this.panel1.Controls.Add(this.SettingsCameraRadioVideo);
            this.panel1.Controls.Add(this.SettingsCameraVideoDurationLbl);
            this.panel1.Location = new System.Drawing.Point(10, 276);
            this.panel1.Name = "panel1";
            this.panel1.Size = new System.Drawing.Size(250, 114);
            this.panel1.TabIndex = 7;
            // 
            // SettingsCameraVideoFileSize
            // 
            this.SettingsCameraVideoFileSize.Enabled = false;
            this.SettingsCameraVideoFileSize.Location = new System.Drawing.Point(183, 85);
            this.SettingsCameraVideoFileSize.Name = "SettingsCameraVideoFileSize";
            this.SettingsCameraVideoFileSize.Size = new System.Drawing.Size(58, 20);
            this.SettingsCameraVideoFileSize.TabIndex = 12;
            this.ToolTip.SetToolTip(this.SettingsCameraVideoFileSize, "Duration, in seconds, of the videos taken by the camera.");
            this.SettingsCameraVideoFileSize.TextChanged += new System.EventHandler(this.SettingModified);
            // 
            // SettingsCameraVideoFileSizeLbl
            // 
            this.SettingsCameraVideoFileSizeLbl.AutoSize = true;
            this.SettingsCameraVideoFileSizeLbl.Location = new System.Drawing.Point(70, 88);
            this.SettingsCameraVideoFileSizeLbl.Name = "SettingsCameraVideoFileSizeLbl";
            this.SettingsCameraVideoFileSizeLbl.Size = new System.Drawing.Size(107, 13);
            this.SettingsCameraVideoFileSizeLbl.TabIndex = 11;
            this.SettingsCameraVideoFileSizeLbl.Text = "Target File Size (ko) :";
            this.SettingsCameraVideoFileSizeLbl.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
            // 
            // SettingsCameraVideoThumbnail
            // 
            this.SettingsCameraVideoThumbnail.Enabled = false;
            this.SettingsCameraVideoThumbnail.Location = new System.Drawing.Point(183, 63);
            this.SettingsCameraVideoThumbnail.Name = "SettingsCameraVideoThumbnail";
            this.SettingsCameraVideoThumbnail.Size = new System.Drawing.Size(58, 20);
            this.SettingsCameraVideoThumbnail.TabIndex = 10;
            this.ToolTip.SetToolTip(this.SettingsCameraVideoThumbnail, "Duration, in seconds, of the videos taken by the camera.");
            this.SettingsCameraVideoThumbnail.TextChanged += new System.EventHandler(this.SettingModified);
            // 
            // SettingsCameraVideoThumbnailLbl
            // 
            this.SettingsCameraVideoThumbnailLbl.AutoSize = true;
            this.SettingsCameraVideoThumbnailLbl.Location = new System.Drawing.Point(89, 66);
            this.SettingsCameraVideoThumbnailLbl.Name = "SettingsCameraVideoThumbnailLbl";
            this.SettingsCameraVideoThumbnailLbl.Size = new System.Drawing.Size(88, 13);
            this.SettingsCameraVideoThumbnailLbl.TabIndex = 9;
            this.SettingsCameraVideoThumbnailLbl.Text = "Thumbnail (sec) :";
            this.SettingsCameraVideoThumbnailLbl.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
            // 
            // SettingsCameraVideoDuration
            // 
            this.SettingsCameraVideoDuration.Enabled = false;
            this.SettingsCameraVideoDuration.Location = new System.Drawing.Point(183, 42);
            this.SettingsCameraVideoDuration.Name = "SettingsCameraVideoDuration";
            this.SettingsCameraVideoDuration.Size = new System.Drawing.Size(58, 20);
            this.SettingsCameraVideoDuration.TabIndex = 8;
            this.ToolTip.SetToolTip(this.SettingsCameraVideoDuration, "Duration, in seconds, of the videos taken by the camera.");
            this.SettingsCameraVideoDuration.TextChanged += new System.EventHandler(this.SettingModified);
            // 
            // SettingsCameraOptionsLbl
            // 
            this.SettingsCameraOptionsLbl.AutoSize = true;
            this.SettingsCameraOptionsLbl.Location = new System.Drawing.Point(4, 4);
            this.SettingsCameraOptionsLbl.Name = "SettingsCameraOptionsLbl";
            this.SettingsCameraOptionsLbl.Size = new System.Drawing.Size(82, 13);
            this.SettingsCameraOptionsLbl.TabIndex = 7;
            this.SettingsCameraOptionsLbl.Text = "Camera settings";
            // 
            // SettingsCameraRadioPicture
            // 
            this.SettingsCameraRadioPicture.AutoSize = true;
            this.SettingsCameraRadioPicture.Location = new System.Drawing.Point(7, 20);
            this.SettingsCameraRadioPicture.Name = "SettingsCameraRadioPicture";
            this.SettingsCameraRadioPicture.Size = new System.Drawing.Size(63, 17);
            this.SettingsCameraRadioPicture.TabIndex = 5;
            this.SettingsCameraRadioPicture.TabStop = true;
            this.SettingsCameraRadioPicture.Text = "Pictures";
            this.ToolTip.SetToolTip(this.SettingsCameraRadioPicture, "Indicates that the camera is taking pictures.");
            this.SettingsCameraRadioPicture.UseVisualStyleBackColor = true;
            this.SettingsCameraRadioPicture.CheckedChanged += new System.EventHandler(this.SettingModified);
            // 
            // SettingsCameraRadioVideo
            // 
            this.SettingsCameraRadioVideo.AutoSize = true;
            this.SettingsCameraRadioVideo.Location = new System.Drawing.Point(7, 43);
            this.SettingsCameraRadioVideo.Name = "SettingsCameraRadioVideo";
            this.SettingsCameraRadioVideo.Size = new System.Drawing.Size(57, 17);
            this.SettingsCameraRadioVideo.TabIndex = 6;
            this.SettingsCameraRadioVideo.TabStop = true;
            this.SettingsCameraRadioVideo.Text = "Videos";
            this.ToolTip.SetToolTip(this.SettingsCameraRadioVideo, "Indicates that the camera is taking videos.");
            this.SettingsCameraRadioVideo.UseVisualStyleBackColor = true;
            this.SettingsCameraRadioVideo.CheckedChanged += new System.EventHandler(this.SettingsCameraRadioVideo_CheckedChanged);
            // 
            // SettingsCameraVideoDurationLbl
            // 
            this.SettingsCameraVideoDurationLbl.AutoSize = true;
            this.SettingsCameraVideoDurationLbl.Location = new System.Drawing.Point(70, 45);
            this.SettingsCameraVideoDurationLbl.Name = "SettingsCameraVideoDurationLbl";
            this.SettingsCameraVideoDurationLbl.Size = new System.Drawing.Size(107, 13);
            this.SettingsCameraVideoDurationLbl.TabIndex = 4;
            this.SettingsCameraVideoDurationLbl.Text = "Video duration (sec) :";
            this.SettingsCameraVideoDurationLbl.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
            // 
            // SettingsTrigParams
            // 
            this.SettingsTrigParams.Location = new System.Drawing.Point(103, 252);
            this.SettingsTrigParams.Name = "SettingsTrigParams";
            this.SettingsTrigParams.Size = new System.Drawing.Size(363, 20);
            this.SettingsTrigParams.TabIndex = 14;
            this.SettingsTrigParams.Text = "(None)";
            this.ToolTip.SetToolTip(this.SettingsTrigParams, "Parameters for the instance of the detectionMechanism. Is a sequence of strings s" +
        "eparated with \",\".");
            this.SettingsTrigParams.TextChanged += new System.EventHandler(this.SettingModified);
            // 
            // SettingsFtpDir
            // 
            this.SettingsFtpDir.Location = new System.Drawing.Point(103, 94);
            this.SettingsFtpDir.Name = "SettingsFtpDir";
            this.SettingsFtpDir.Size = new System.Drawing.Size(363, 20);
            this.SettingsFtpDir.TabIndex = 3;
            this.SettingsFtpDir.Text = "(None)";
            this.ToolTip.SetToolTip(this.SettingsFtpDir, "This is the directory where the media files are being put in the FTP server.");
            this.SettingsFtpDir.TextChanged += new System.EventHandler(this.SettingModified);
            this.SettingsFtpDir.Leave += new System.EventHandler(this.SettingModified);
            // 
            // SettingsFtpArchiveLbl
            // 
            this.SettingsFtpArchiveLbl.AutoSize = true;
            this.SettingsFtpArchiveLbl.Location = new System.Drawing.Point(34, 118);
            this.SettingsFtpArchiveLbl.Name = "SettingsFtpArchiveLbl";
            this.SettingsFtpArchiveLbl.Size = new System.Drawing.Size(66, 13);
            this.SettingsFtpArchiveLbl.TabIndex = 8;
            this.SettingsFtpArchiveLbl.Text = "Ftp archive :";
            this.SettingsFtpArchiveLbl.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
            // 
            // SettingsTrigClass
            // 
            this.SettingsTrigClass.Location = new System.Drawing.Point(103, 231);
            this.SettingsTrigClass.Name = "SettingsTrigClass";
            this.SettingsTrigClass.Size = new System.Drawing.Size(363, 20);
            this.SettingsTrigClass.TabIndex = 13;
            this.SettingsTrigClass.Text = "(None)";
            this.ToolTip.SetToolTip(this.SettingsTrigClass, "The class name of the trigger mechanism. Can be empty or from a very selective li" +
        "st.");
            this.SettingsTrigClass.TextChanged += new System.EventHandler(this.SettingModified);
            // 
            // SettingsFtpDirLbl
            // 
            this.SettingsFtpDirLbl.AutoSize = true;
            this.SettingsFtpDirLbl.Location = new System.Drawing.Point(30, 97);
            this.SettingsFtpDirLbl.Name = "SettingsFtpDirLbl";
            this.SettingsFtpDirLbl.Size = new System.Drawing.Size(71, 13);
            this.SettingsFtpDirLbl.TabIndex = 2;
            this.SettingsFtpDirLbl.Text = "Ftp directory :";
            this.SettingsFtpDirLbl.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
            // 
            // SettingsFtpArchive
            // 
            this.SettingsFtpArchive.Location = new System.Drawing.Point(103, 115);
            this.SettingsFtpArchive.Multiline = true;
            this.SettingsFtpArchive.Name = "SettingsFtpArchive";
            this.SettingsFtpArchive.Size = new System.Drawing.Size(363, 20);
            this.SettingsFtpArchive.TabIndex = 9;
            this.SettingsFtpArchive.Text = "(None)";
            this.ToolTip.SetToolTip(this.SettingsFtpArchive, "If not blank, this is where the media files go when succesfully uploaded to the F" +
        "tp server. Can be in the form \"./DIR_NAME\" or absolute path.");
            this.SettingsFtpArchive.TextChanged += new System.EventHandler(this.SettingModified);
            this.SettingsFtpArchive.Leave += new System.EventHandler(this.SettingModified);
            // 
            // SettingsTrigClassLbl
            // 
            this.SettingsTrigClassLbl.AutoSize = true;
            this.SettingsTrigClassLbl.Location = new System.Drawing.Point(27, 234);
            this.SettingsTrigClassLbl.Name = "SettingsTrigClassLbl";
            this.SettingsTrigClassLbl.Size = new System.Drawing.Size(74, 13);
            this.SettingsTrigClassLbl.TabIndex = 12;
            this.SettingsTrigClassLbl.Text = "Trigger Class :";
            this.SettingsTrigClassLbl.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
            // 
            // SettingsWriteToFileBtn
            // 
            this.SettingsWriteToFileBtn.Location = new System.Drawing.Point(344, 367);
            this.SettingsWriteToFileBtn.Name = "SettingsWriteToFileBtn";
            this.SettingsWriteToFileBtn.Size = new System.Drawing.Size(122, 23);
            this.SettingsWriteToFileBtn.TabIndex = 10;
            this.SettingsWriteToFileBtn.Text = "Write to settings file";
            this.ToolTip.SetToolTip(this.SettingsWriteToFileBtn, "Write the settings to file, overriding those currently used. Unless invalidated.");
            this.SettingsWriteToFileBtn.UseVisualStyleBackColor = true;
            this.SettingsWriteToFileBtn.Click += new System.EventHandler(this.SettingsWriteToFileBtn_Click);
            // 
            // SettingsResetButton
            // 
            this.SettingsResetButton.Location = new System.Drawing.Point(344, 340);
            this.SettingsResetButton.Name = "SettingsResetButton";
            this.SettingsResetButton.Size = new System.Drawing.Size(122, 23);
            this.SettingsResetButton.TabIndex = 11;
            this.SettingsResetButton.Text = "Reset settings";
            this.ToolTip.SetToolTip(this.SettingsResetButton, "Return the text boxes to the currently used settings.");
            this.SettingsResetButton.UseVisualStyleBackColor = true;
            this.SettingsResetButton.Click += new System.EventHandler(this.ResetSettingsValues);
            // 
            // SettingsFtpServerLbl
            // 
            this.SettingsFtpServerLbl.AutoSize = true;
            this.SettingsFtpServerLbl.Location = new System.Drawing.Point(39, 55);
            this.SettingsFtpServerLbl.Name = "SettingsFtpServerLbl";
            this.SettingsFtpServerLbl.Size = new System.Drawing.Size(62, 13);
            this.SettingsFtpServerLbl.TabIndex = 0;
            this.SettingsFtpServerLbl.Text = "Ftp Server :";
            this.SettingsFtpServerLbl.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
            // 
            // ToolTip
            // 
            this.ToolTip.AutomaticDelay = 0;
            // 
            // MainFormUI
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(944, 426);
            this.Controls.Add(this.MainTabControl);
            this.Controls.Add(this.EventsLogLabel);
            this.Controls.Add(this.EventsLog);
            this.Name = "MainFormUi";
            this.Text = "iBrand SnapShot";
            this.FormClosing += new System.Windows.Forms.FormClosingEventHandler(this.MainFormUI_FormClosing);
            this.MainTabControl.ResumeLayout(false);
            this.TabStatus.ResumeLayout(false);
            this.TabStatus.PerformLayout();
            this.SettingsCameraTab.ResumeLayout(false);
            this.SettingsCameraTab.PerformLayout();
            this.panel1.ResumeLayout(false);
            this.panel1.PerformLayout();
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.Label EventsLogLabel;
        private System.Windows.Forms.TextBox EventsLog;
        private System.Windows.Forms.Button TriggerTestBtn;
        private System.Windows.Forms.Label TrigStatus;
        private System.Windows.Forms.Label CamStatusLbl;
        private System.Windows.Forms.Label CamStatus;
        private System.Windows.Forms.Label FtpStatusLbl;
        private System.Windows.Forms.Label TrigStatusLbl;
        private System.Windows.Forms.Label FtpStatus;
        private System.Windows.Forms.Button FtpTestBtn;
        private System.Windows.Forms.Button CamTestBtn;
        private System.Windows.Forms.TabControl MainTabControl;
        private System.Windows.Forms.TabPage TabStatus;
        private System.Windows.Forms.TabPage SettingsCameraTab;
        private System.Windows.Forms.TextBox SettingsFtpServer;
        private System.Windows.Forms.Label SettingsFtpServerLbl;
        private System.Windows.Forms.Label SettingsCameraVideoDurationLbl;
        private System.Windows.Forms.TextBox SettingsFtpDir;
        private System.Windows.Forms.Label SettingsFtpDirLbl;
        private System.Windows.Forms.Panel panel1;
        private System.Windows.Forms.TextBox SettingsCameraVideoDuration;
        private System.Windows.Forms.Label SettingsCameraOptionsLbl;
        private System.Windows.Forms.RadioButton SettingsCameraRadioPicture;
        private System.Windows.Forms.RadioButton SettingsCameraRadioVideo;
        private System.Windows.Forms.TextBox SettingsFtpArchive;
        private System.Windows.Forms.Label SettingsFtpArchiveLbl;
        private System.Windows.Forms.Button SettingsWriteToFileBtn;
        private System.Windows.Forms.Button SettingsResetButton;
        private System.Windows.Forms.Button CamActivateBtn;
        private System.Windows.Forms.Label SettingsTrigParamsLbl;
        private System.Windows.Forms.TextBox SettingsTrigParams;
        private System.Windows.Forms.TextBox SettingsTrigClass;
        private System.Windows.Forms.Label SettingsTrigClassLbl;
        private System.Windows.Forms.Button ProcessFilesBtn;
        private System.Windows.Forms.ToolTip ToolTip;
        private System.Windows.Forms.TextBox SettingsFtpServerBkp;
        private System.Windows.Forms.Label SettingsFtpServerBkpLbl;
        private System.Windows.Forms.Label SettingsFfmpegLocationLbl;
        private System.Windows.Forms.TextBox SettingsFfmpegLocation;
        private System.Windows.Forms.Label SettingsVerboseLbl;
        private System.Windows.Forms.CheckBox SettingsVerbose;
        private System.Windows.Forms.TextBox SettingsCameraVideoThumbnail;
        private System.Windows.Forms.Label SettingsCameraVideoThumbnailLbl;
        private System.Windows.Forms.TextBox SettingsCameraVideoFileSize;
        private System.Windows.Forms.Label SettingsCameraVideoFileSizeLbl;
        private System.Windows.Forms.Label CustomWorkerStatus;
        private System.Windows.Forms.Label CustomWorkerStatusLbl;
        private System.Windows.Forms.Label SettingsTriggerTimeLbl;
        private System.Windows.Forms.TextBox SettingsTriggerTime;
        private System.Windows.Forms.Label SettingsWorkerClassLbl;
        private System.Windows.Forms.TextBox SettingsWorkerClass;
        private System.Windows.Forms.CheckBox SettingsEnableFtp;
        private System.Windows.Forms.Label SettingsEnableFtpLbl;
        private System.Windows.Forms.Label SettingsWorkerNumberCustomLbl;
        private System.Windows.Forms.TextBox SettingsWorkerNumberCustom;
        private System.Windows.Forms.Label SettingsWorkerNumberFtpLbl;
        private System.Windows.Forms.TextBox SettingsWorkerNumberFtp;
        private System.Windows.Forms.Label DisplayAlive;
    }
}

