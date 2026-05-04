using System.ComponentModel;
using System.Windows.Forms;

namespace NomadLogic.iBrandDesktop.Ui.ApplicationScreen
{
    partial class TouchModeConfigurationDlg
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
            this.okButton = new System.Windows.Forms.Button();
            this.closeAppButton = new System.Windows.Forms.Button();
            this.cameraSettingsButton = new System.Windows.Forms.Button();
            this.languageLabel = new System.Windows.Forms.Label();
            this.languageComboBox = new System.Windows.Forms.ComboBox();
            this.printOffsetXTextBox = new System.Windows.Forms.TextBox();
            this.printOffsetXLabel = new System.Windows.Forms.Label();
            this.buttonChromaKeySettings = new System.Windows.Forms.Button();
            this.btnPrinterSettings = new System.Windows.Forms.Button();
            this.btnLocalImageTransform = new System.Windows.Forms.Button();
            this.btnBasePictureSettings = new System.Windows.Forms.Button();
            this.buttonEshotsAPI = new System.Windows.Forms.Button();
            this._btnSynchroMonitor = new System.Windows.Forms.Button();
            this.UnloadAndCloseButton = new System.Windows.Forms.Button();
            this.button_export_activity_data = new System.Windows.Forms.Button();
            this.SuspendLayout();
            // 
            // okButton
            // 
            this.okButton.Font = new System.Drawing.Font("Microsoft Sans Serif", 12F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.okButton.Location = new System.Drawing.Point(129, 441);
            this.okButton.Name = "okButton";
            this.okButton.Size = new System.Drawing.Size(88, 44);
            this.okButton.TabIndex = 64;
            this.okButton.Text = "OK";
            this.okButton.UseVisualStyleBackColor = true;
            // 
            // closeAppButton
            // 
            this.closeAppButton.Font = new System.Drawing.Font("Microsoft Sans Serif", 12F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.closeAppButton.Location = new System.Drawing.Point(176, 369);
            this.closeAppButton.Name = "closeAppButton";
            this.closeAppButton.Size = new System.Drawing.Size(150, 44);
            this.closeAppButton.TabIndex = 78;
            this.closeAppButton.Text = "Close iBrand(tm)";
            this.closeAppButton.UseVisualStyleBackColor = true;
            // 
            // cameraSettingsButton
            // 
            this.cameraSettingsButton.Font = new System.Drawing.Font("Microsoft Sans Serif", 9.75F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.cameraSettingsButton.Location = new System.Drawing.Point(16, 206);
            this.cameraSettingsButton.Name = "cameraSettingsButton";
            this.cameraSettingsButton.Size = new System.Drawing.Size(152, 40);
            this.cameraSettingsButton.TabIndex = 80;
            this.cameraSettingsButton.Text = "Camera settings...";
            this.cameraSettingsButton.UseVisualStyleBackColor = true;
            // 
            // languageLabel
            // 
            this.languageLabel.Font = new System.Drawing.Font("Microsoft Sans Serif", 14.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.languageLabel.Location = new System.Drawing.Point(16, 43);
            this.languageLabel.Name = "languageLabel";
            this.languageLabel.Size = new System.Drawing.Size(174, 32);
            this.languageLabel.TabIndex = 82;
            this.languageLabel.Text = "Language :";
            // 
            // languageComboBox
            // 
            this.languageComboBox.Font = new System.Drawing.Font("Microsoft Sans Serif", 14.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.languageComboBox.FormattingEnabled = true;
            this.languageComboBox.Location = new System.Drawing.Point(197, 40);
            this.languageComboBox.Name = "languageComboBox";
            this.languageComboBox.Size = new System.Drawing.Size(119, 32);
            this.languageComboBox.TabIndex = 83;
            // 
            // printOffsetXTextBox
            // 
            this.printOffsetXTextBox.Location = new System.Drawing.Point(197, 78);
            this.printOffsetXTextBox.Name = "printOffsetXTextBox";
            this.printOffsetXTextBox.Size = new System.Drawing.Size(119, 20);
            this.printOffsetXTextBox.TabIndex = 90;
            // 
            // printOffsetXLabel
            // 
            this.printOffsetXLabel.Font = new System.Drawing.Font("Microsoft Sans Serif", 9.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.printOffsetXLabel.Location = new System.Drawing.Point(17, 78);
            this.printOffsetXLabel.Name = "printOffsetXLabel";
            this.printOffsetXLabel.Size = new System.Drawing.Size(174, 23);
            this.printOffsetXLabel.TabIndex = 91;
            this.printOffsetXLabel.Text = "Printer Side Offset :";
            // 
            // buttonChromaKeySettings
            // 
            this.buttonChromaKeySettings.Font = new System.Drawing.Font("Microsoft Sans Serif", 9.75F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.buttonChromaKeySettings.Location = new System.Drawing.Point(16, 112);
            this.buttonChromaKeySettings.Name = "buttonChromaKeySettings";
            this.buttonChromaKeySettings.Size = new System.Drawing.Size(152, 40);
            this.buttonChromaKeySettings.TabIndex = 92;
            this.buttonChromaKeySettings.Text = "Chroma Key settings...";
            this.buttonChromaKeySettings.UseVisualStyleBackColor = true;
            // 
            // btnPrinterSettings
            // 
            this.btnPrinterSettings.Font = new System.Drawing.Font("Microsoft Sans Serif", 9.75F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.btnPrinterSettings.Location = new System.Drawing.Point(16, 252);
            this.btnPrinterSettings.Name = "btnPrinterSettings";
            this.btnPrinterSettings.Size = new System.Drawing.Size(152, 40);
            this.btnPrinterSettings.TabIndex = 93;
            this.btnPrinterSettings.Text = "Printer settings...";
            this.btnPrinterSettings.UseVisualStyleBackColor = true;
            // 
            // btnLocalImageTransform
            // 
            this.btnLocalImageTransform.Font = new System.Drawing.Font("Microsoft Sans Serif", 9.75F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.btnLocalImageTransform.Location = new System.Drawing.Point(16, 298);
            this.btnLocalImageTransform.Name = "btnLocalImageTransform";
            this.btnLocalImageTransform.Size = new System.Drawing.Size(152, 40);
            this.btnLocalImageTransform.TabIndex = 94;
            this.btnLocalImageTransform.Text = "Custom image transform...";
            this.btnLocalImageTransform.UseVisualStyleBackColor = true;
            // 
            // btnBasePictureSettings
            // 
            this.btnBasePictureSettings.Font = new System.Drawing.Font("Microsoft Sans Serif", 9.75F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.btnBasePictureSettings.Location = new System.Drawing.Point(16, 160);
            this.btnBasePictureSettings.Name = "btnBasePictureSettings";
            this.btnBasePictureSettings.Size = new System.Drawing.Size(152, 40);
            this.btnBasePictureSettings.TabIndex = 95;
            this.btnBasePictureSettings.Text = "Base picture settings...";
            this.btnBasePictureSettings.UseVisualStyleBackColor = true;
            // 
            // buttonEshotsAPI
            // 
            this.buttonEshotsAPI.Font = new System.Drawing.Font("Microsoft Sans Serif", 9.75F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.buttonEshotsAPI.Location = new System.Drawing.Point(174, 112);
            this.buttonEshotsAPI.Name = "buttonEshotsAPI";
            this.buttonEshotsAPI.Size = new System.Drawing.Size(152, 40);
            this.buttonEshotsAPI.TabIndex = 96;
            this.buttonEshotsAPI.Text = "eShots API settings...";
            this.buttonEshotsAPI.UseVisualStyleBackColor = true;
            // 
            // _btnSynchroMonitor
            // 
            this._btnSynchroMonitor.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this._btnSynchroMonitor.Location = new System.Drawing.Point(18, 12);
            this._btnSynchroMonitor.Name = "_btnSynchroMonitor";
            this._btnSynchroMonitor.Size = new System.Drawing.Size(152, 28);
            this._btnSynchroMonitor.TabIndex = 97;
            this._btnSynchroMonitor.Text = "Synchronization Monitor";
            this._btnSynchroMonitor.UseVisualStyleBackColor = true;
            this._btnSynchroMonitor.Click += new System.EventHandler(this._btnSynchroMonitor_Click);
            // 
            // UnloadAndCloseButton
            // 
            this.UnloadAndCloseButton.Font = new System.Drawing.Font("Microsoft Sans Serif", 9.75F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.UnloadAndCloseButton.Location = new System.Drawing.Point(16, 369);
            this.UnloadAndCloseButton.Name = "UnloadAndCloseButton";
            this.UnloadAndCloseButton.Size = new System.Drawing.Size(152, 44);
            this.UnloadAndCloseButton.TabIndex = 98;
            this.UnloadAndCloseButton.Text = "Unload questionnaire";
            this.UnloadAndCloseButton.UseVisualStyleBackColor = true;
            // 
            // button_export_activity_data
            // 
            this.button_export_activity_data.Font = new System.Drawing.Font("Microsoft Sans Serif", 9.75F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.button_export_activity_data.Location = new System.Drawing.Point(174, 298);
            this.button_export_activity_data.Name = "button_export_activity_data";
            this.button_export_activity_data.Size = new System.Drawing.Size(152, 40);
            this.button_export_activity_data.TabIndex = 99;
            this.button_export_activity_data.Text = "Export Activity Data";
            this.button_export_activity_data.UseVisualStyleBackColor = true;
            // 
            // TouchModeConfigurationDlg
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(344, 497);
            this.ControlBox = false;
            this.Controls.Add(this.button_export_activity_data);
            this.Controls.Add(this.UnloadAndCloseButton);
            this.Controls.Add(this._btnSynchroMonitor);
            this.Controls.Add(this.buttonEshotsAPI);
            this.Controls.Add(this.btnBasePictureSettings);
            this.Controls.Add(this.btnLocalImageTransform);
            this.Controls.Add(this.btnPrinterSettings);
            this.Controls.Add(this.buttonChromaKeySettings);
            this.Controls.Add(this.printOffsetXLabel);
            this.Controls.Add(this.printOffsetXTextBox);
            this.Controls.Add(this.languageComboBox);
            this.Controls.Add(this.languageLabel);
            this.Controls.Add(this.cameraSettingsButton);
            this.Controls.Add(this.closeAppButton);
            this.Controls.Add(this.okButton);
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedSingle;
            this.MaximizeBox = false;
            this.Name = "TouchModeConfigurationDlg";
            this.StartPosition = System.Windows.Forms.FormStartPosition.Manual;
            this.Text = "iBrand(tm)";
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private Button okButton;
        private Button closeAppButton;
        private Button cameraSettingsButton;
        private Label languageLabel;
        private ComboBox languageComboBox;
        private TextBox printOffsetXTextBox;
        private Label printOffsetXLabel;
        private Button buttonChromaKeySettings;
        private Button btnPrinterSettings;
        private Button btnLocalImageTransform;
        private Button btnBasePictureSettings;
        private Button buttonEshotsAPI;
        private Button _btnSynchroMonitor;
        private Button UnloadAndCloseButton;
        private Button button_export_activity_data;
    }
}