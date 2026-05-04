using System.ComponentModel;
using System.Windows.Forms;

namespace NomadLogic.PrintStation.Ui
{
    partial class ConfigurationDialog
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
            this.ok_button = new System.Windows.Forms.Button();
            this.label2 = new System.Windows.Forms.Label();
            this.serverAdrTextBox = new System.Windows.Forms.TextBox();
            this.serverPortTextBox = new System.Windows.Forms.TextBox();
            this.label3 = new System.Windows.Forms.Label();
            this.lastPostCheckBox = new System.Windows.Forms.CheckBox();
            this.listenForFilesCheckBox = new System.Windows.Forms.CheckBox();
            this.ipAddressLabel = new System.Windows.Forms.Label();
            this.dontLookForPost = new System.Windows.Forms.CheckBox();
            this.automaticPrint = new System.Windows.Forms.CheckBox();
            this.printer_x_offset_textbox = new System.Windows.Forms.TextBox();
            this.Print_offset_label = new System.Windows.Forms.Label();
            this.ssdpPortLabel = new System.Windows.Forms.Label();
            this.btnPrinterSettings = new System.Windows.Forms.Button();
            this.label1 = new System.Windows.Forms.Label();
            this.camerasComboBox = new System.Windows.Forms.ComboBox();
            this.btnSynchroMonitor = new System.Windows.Forms.Button();
            this.btnEshotsApi = new System.Windows.Forms.Button();
            this.SuspendLayout();
            // 
            // ok_button
            // 
            this.ok_button.Font = new System.Drawing.Font("Arial", 9.75F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.ok_button.Location = new System.Drawing.Point(96, 344);
            this.ok_button.Name = "ok_button";
            this.ok_button.Size = new System.Drawing.Size(75, 30);
            this.ok_button.TabIndex = 0;
            this.ok_button.Text = "Ok";
            this.ok_button.UseVisualStyleBackColor = true;
            // 
            // label2
            // 
            this.label2.AutoSize = true;
            this.label2.Font = new System.Drawing.Font("Arial", 9.75F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.label2.Location = new System.Drawing.Point(7, 12);
            this.label2.Name = "label2";
            this.label2.Size = new System.Drawing.Size(103, 16);
            this.label2.TabIndex = 3;
            this.label2.Text = "Server Address :";
            // 
            // serverAdrTextBox
            // 
            this.serverAdrTextBox.Location = new System.Drawing.Point(144, 12);
            this.serverAdrTextBox.Name = "serverAdrTextBox";
            this.serverAdrTextBox.Size = new System.Drawing.Size(151, 20);
            this.serverAdrTextBox.TabIndex = 4;
            // 
            // serverPortTextBox
            // 
            this.serverPortTextBox.Location = new System.Drawing.Point(144, 38);
            this.serverPortTextBox.Name = "serverPortTextBox";
            this.serverPortTextBox.Size = new System.Drawing.Size(151, 20);
            this.serverPortTextBox.TabIndex = 6;
            // 
            // label3
            // 
            this.label3.AutoSize = true;
            this.label3.Font = new System.Drawing.Font("Arial", 9.75F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.label3.Location = new System.Drawing.Point(7, 38);
            this.label3.Name = "label3";
            this.label3.Size = new System.Drawing.Size(80, 16);
            this.label3.TabIndex = 5;
            this.label3.Text = "Server Port :";
            // 
            // lastPostCheckBox
            // 
            this.lastPostCheckBox.Font = new System.Drawing.Font("Arial", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.lastPostCheckBox.Location = new System.Drawing.Point(8, 181);
            this.lastPostCheckBox.Name = "lastPostCheckBox";
            this.lastPostCheckBox.Size = new System.Drawing.Size(196, 24);
            this.lastPostCheckBox.TabIndex = 73;
            this.lastPostCheckBox.Text = "Start Looking From Last Post";
            this.lastPostCheckBox.UseVisualStyleBackColor = true;
            // 
            // listenForFilesCheckBox
            // 
            this.listenForFilesCheckBox.Font = new System.Drawing.Font("Arial", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.listenForFilesCheckBox.Location = new System.Drawing.Point(8, 208);
            this.listenForFilesCheckBox.Name = "listenForFilesCheckBox";
            this.listenForFilesCheckBox.Size = new System.Drawing.Size(206, 24);
            this.listenForFilesCheckBox.TabIndex = 74;
            this.listenForFilesCheckBox.Text = "Listen For Pictures From iBrand";
            this.listenForFilesCheckBox.UseVisualStyleBackColor = true;
            // 
            // ipAddressLabel
            // 
            this.ipAddressLabel.AutoSize = true;
            this.ipAddressLabel.Font = new System.Drawing.Font("Microsoft Sans Serif", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.ipAddressLabel.Location = new System.Drawing.Point(7, 296);
            this.ipAddressLabel.Name = "ipAddressLabel";
            this.ipAddressLabel.Size = new System.Drawing.Size(104, 15);
            this.ipAddressLabel.TabIndex = 75;
            this.ipAddressLabel.Text = "Local IP Address :";
            // 
            // dontLookForPost
            // 
            this.dontLookForPost.Font = new System.Drawing.Font("Arial", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.dontLookForPost.Location = new System.Drawing.Point(8, 236);
            this.dontLookForPost.Name = "dontLookForPost";
            this.dontLookForPost.Size = new System.Drawing.Size(206, 24);
            this.dontLookForPost.TabIndex = 76;
            this.dontLookForPost.Text = "Don\'t Look For Post ";
            this.dontLookForPost.UseVisualStyleBackColor = true;
            // 
            // automaticPrint
            // 
            this.automaticPrint.Font = new System.Drawing.Font("Arial", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.automaticPrint.Location = new System.Drawing.Point(8, 266);
            this.automaticPrint.Name = "automaticPrint";
            this.automaticPrint.Size = new System.Drawing.Size(206, 24);
            this.automaticPrint.TabIndex = 77;
            this.automaticPrint.Text = "Automatic HasPrint";
            this.automaticPrint.UseVisualStyleBackColor = true;
            // 
            // printer_x_offset_textbox
            // 
            this.printer_x_offset_textbox.Location = new System.Drawing.Point(144, 64);
            this.printer_x_offset_textbox.Name = "printer_x_offset_textbox";
            this.printer_x_offset_textbox.Size = new System.Drawing.Size(151, 20);
            this.printer_x_offset_textbox.TabIndex = 79;
            // 
            // Print_offset_label
            // 
            this.Print_offset_label.AutoSize = true;
            this.Print_offset_label.Font = new System.Drawing.Font("Arial", 9.75F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.Print_offset_label.Location = new System.Drawing.Point(7, 64);
            this.Print_offset_label.Name = "Print_offset_label";
            this.Print_offset_label.Size = new System.Drawing.Size(99, 16);
            this.Print_offset_label.TabIndex = 78;
            this.Print_offset_label.Text = "Printer X Offset:";
            // 
            // ssdpPortLabel
            // 
            this.ssdpPortLabel.AutoSize = true;
            this.ssdpPortLabel.Font = new System.Drawing.Font("Microsoft Sans Serif", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.ssdpPortLabel.Location = new System.Drawing.Point(7, 324);
            this.ssdpPortLabel.Name = "ssdpPortLabel";
            this.ssdpPortLabel.Size = new System.Drawing.Size(66, 15);
            this.ssdpPortLabel.TabIndex = 80;
            this.ssdpPortLabel.Text = "Ssdp Port :";
            // 
            // btnPrinterSettings
            // 
            this.btnPrinterSettings.Location = new System.Drawing.Point(163, 120);
            this.btnPrinterSettings.Name = "btnPrinterSettings";
            this.btnPrinterSettings.Size = new System.Drawing.Size(132, 23);
            this.btnPrinterSettings.TabIndex = 81;
            this.btnPrinterSettings.Text = "Printer settings";
            this.btnPrinterSettings.UseVisualStyleBackColor = true;
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Font = new System.Drawing.Font("Arial", 9.75F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.label1.Location = new System.Drawing.Point(5, 93);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(138, 16);
            this.label1.TabIndex = 82;
            this.label1.Text = "Connected Camera(s):";
            // 
            // camerasComboBox
            // 
            this.camerasComboBox.FormattingEnabled = true;
            this.camerasComboBox.Location = new System.Drawing.Point(144, 93);
            this.camerasComboBox.Name = "camerasComboBox";
            this.camerasComboBox.Size = new System.Drawing.Size(151, 21);
            this.camerasComboBox.TabIndex = 83;
            // 
            // btnSynchroMonitor
            // 
            this.btnSynchroMonitor.Location = new System.Drawing.Point(8, 120);
            this.btnSynchroMonitor.Name = "btnSynchroMonitor";
            this.btnSynchroMonitor.Size = new System.Drawing.Size(115, 23);
            this.btnSynchroMonitor.TabIndex = 84;
            this.btnSynchroMonitor.Text = "Synchronization Monitor";
            this.btnSynchroMonitor.UseVisualStyleBackColor = true;
            this.btnSynchroMonitor.Click += new System.EventHandler(this.BtnSynchroMonitor_Click);
            // 
            // btnEshotsApi
            // 
            this.btnEshotsApi.Location = new System.Drawing.Point(8, 152);
            this.btnEshotsApi.Name = "btnEshotsApi";
            this.btnEshotsApi.Size = new System.Drawing.Size(115, 23);
            this.btnEshotsApi.TabIndex = 85;
            this.btnEshotsApi.Text = "eShots API settings";
            this.btnEshotsApi.UseVisualStyleBackColor = true;
            // 
            // ConfigurationDialog
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(307, 394);
            this.ControlBox = false;
            this.Controls.Add(this.btnEshotsApi);
            this.Controls.Add(this.btnSynchroMonitor);
            this.Controls.Add(this.camerasComboBox);
            this.Controls.Add(this.label1);
            this.Controls.Add(this.btnPrinterSettings);
            this.Controls.Add(this.ssdpPortLabel);
            this.Controls.Add(this.printer_x_offset_textbox);
            this.Controls.Add(this.Print_offset_label);
            this.Controls.Add(this.automaticPrint);
            this.Controls.Add(this.dontLookForPost);
            this.Controls.Add(this.ipAddressLabel);
            this.Controls.Add(this.listenForFilesCheckBox);
            this.Controls.Add(this.lastPostCheckBox);
            this.Controls.Add(this.serverPortTextBox);
            this.Controls.Add(this.label3);
            this.Controls.Add(this.serverAdrTextBox);
            this.Controls.Add(this.label2);
            this.Controls.Add(this.ok_button);
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedSingle;
            this.Name = "ConfigurationDialog";
            this.StartPosition = System.Windows.Forms.FormStartPosition.Manual;
            this.Text = "LeNomad";
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private Button ok_button;
        private Label label2;
        private TextBox serverAdrTextBox;
        private TextBox serverPortTextBox;
        private Label label3;
        private CheckBox lastPostCheckBox;
        private CheckBox listenForFilesCheckBox;
        private Label ipAddressLabel;
        private CheckBox dontLookForPost;
        private CheckBox automaticPrint;
        private TextBox printer_x_offset_textbox;
        private Label Print_offset_label;
        private Label ssdpPortLabel;
        private Button btnPrinterSettings;
        private Label label1;
        private ComboBox camerasComboBox;
        private Button btnSynchroMonitor;
        private Button btnEshotsApi;
    }
}