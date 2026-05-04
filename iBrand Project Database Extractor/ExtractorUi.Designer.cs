namespace NomadLogic.DatabaseExporter
{
    partial class ExtractorUi
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
            this._cmbSelectTerminal = new System.Windows.Forms.ComboBox();
            this.groupBox1 = new System.Windows.Forms.GroupBox();
            this._lblLastDate = new System.Windows.Forms.Label();
            this.label5 = new System.Windows.Forms.Label();
            this._lblTotalRespondent = new System.Windows.Forms.Label();
            this.label1 = new System.Windows.Forms.Label();
            this._groupSelectionConfig = new System.Windows.Forms.GroupBox();
            this._chkIncludeMetadata = new System.Windows.Forms.CheckBox();
            this._chkGenerateHeaders = new System.Windows.Forms.CheckBox();
            this._txtDir = new System.Windows.Forms.TextBox();
            this._btnSelectDir = new System.Windows.Forms.Button();
            this.label3 = new System.Windows.Forms.Label();
            this._cmbReportType = new System.Windows.Forms.ComboBox();
            this.groupBox3 = new System.Windows.Forms.GroupBox();
            this._chkDestroyEverything = new System.Windows.Forms.CheckBox();
            this._chkDestroyLogs = new System.Windows.Forms.CheckBox();
            this._chkDestroyUnsynched = new System.Windows.Forms.CheckBox();
            this._chkDestroyFile = new System.Windows.Forms.CheckBox();
            this._chkDestroyData = new System.Windows.Forms.CheckBox();
            this._btnStartExtraction = new System.Windows.Forms.Button();
            this.groupBox1.SuspendLayout();
            this._groupSelectionConfig.SuspendLayout();
            this.groupBox3.SuspendLayout();
            this.SuspendLayout();
            // 
            // _cmbSelectProject
            // 
            this._cmbSelectTerminal.FormattingEnabled = true;
            this._cmbSelectTerminal.Location = new System.Drawing.Point(6, 19);
            this._cmbSelectTerminal.Name = "_cmbSelectTerminal";
            this._cmbSelectTerminal.Size = new System.Drawing.Size(304, 21);
            this._cmbSelectTerminal.TabIndex = 0;
            // 
            // groupBox1
            // 
            this.groupBox1.Controls.Add(this._lblLastDate);
            this.groupBox1.Controls.Add(this.label5);
            this.groupBox1.Controls.Add(this._lblTotalRespondent);
            this.groupBox1.Controls.Add(this.label1);
            this.groupBox1.Controls.Add(this._cmbSelectTerminal);
            this.groupBox1.Location = new System.Drawing.Point(12, 12);
            this.groupBox1.Name = "groupBox1";
            this.groupBox1.Size = new System.Drawing.Size(316, 125);
            this.groupBox1.TabIndex = 1;
            this.groupBox1.TabStop = false;
            this.groupBox1.Text = "Select project";
            // 
            // _lblLastDate
            // 
            this._lblLastDate.Location = new System.Drawing.Point(6, 89);
            this._lblLastDate.Name = "_lblLastDate";
            this._lblLastDate.Size = new System.Drawing.Size(304, 23);
            this._lblLastDate.TabIndex = 4;
            this._lblLastDate.Text = "DATE";
            this._lblLastDate.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
            // 
            // label5
            // 
            this.label5.Font = new System.Drawing.Font("Microsoft Sans Serif", 9.75F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.label5.Location = new System.Drawing.Point(6, 66);
            this.label5.Name = "label5";
            this.label5.Size = new System.Drawing.Size(163, 23);
            this.label5.TabIndex = 3;
            this.label5.Text = "Last respondent date :";
            this.label5.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
            // 
            // _lblTotalRespondent
            // 
            this._lblTotalRespondent.Location = new System.Drawing.Point(175, 43);
            this._lblTotalRespondent.Name = "_lblTotalRespondent";
            this._lblTotalRespondent.Size = new System.Drawing.Size(115, 23);
            this._lblTotalRespondent.TabIndex = 2;
            this._lblTotalRespondent.Text = "NUMBER";
            this._lblTotalRespondent.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
            // 
            // label1
            // 
            this.label1.Font = new System.Drawing.Font("Microsoft Sans Serif", 9.75F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.label1.Location = new System.Drawing.Point(6, 43);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(163, 23);
            this.label1.TabIndex = 1;
            this.label1.Text = "Total respondents :";
            this.label1.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
            // 
            // _groupSelectionConfig
            // 
            this._groupSelectionConfig.Controls.Add(this._chkIncludeMetadata);
            this._groupSelectionConfig.Controls.Add(this._chkGenerateHeaders);
            this._groupSelectionConfig.Controls.Add(this._txtDir);
            this._groupSelectionConfig.Controls.Add(this._btnSelectDir);
            this._groupSelectionConfig.Controls.Add(this.label3);
            this._groupSelectionConfig.Controls.Add(this._cmbReportType);
            this._groupSelectionConfig.Location = new System.Drawing.Point(12, 143);
            this._groupSelectionConfig.Name = "_groupSelectionConfig";
            this._groupSelectionConfig.Size = new System.Drawing.Size(525, 77);
            this._groupSelectionConfig.TabIndex = 3;
            this._groupSelectionConfig.TabStop = false;
            this._groupSelectionConfig.Text = "Extract to";
            // 
            // _chkIncludeMetadata
            // 
            this._chkIncludeMetadata.AutoSize = true;
            this._chkIncludeMetadata.Location = new System.Drawing.Point(405, 50);
            this._chkIncludeMetadata.Name = "_chkIncludeMetadata";
            this._chkIncludeMetadata.Size = new System.Drawing.Size(111, 17);
            this._chkIncludeMetadata.TabIndex = 6;
            this._chkIncludeMetadata.Text = "Include meta-data";
            this._chkIncludeMetadata.UseVisualStyleBackColor = true;
            // 
            // _chkGenerateHeaders
            // 
            this._chkGenerateHeaders.AutoSize = true;
            this._chkGenerateHeaders.Location = new System.Drawing.Point(288, 50);
            this._chkGenerateHeaders.Name = "_chkGenerateHeaders";
            this._chkGenerateHeaders.Size = new System.Drawing.Size(111, 17);
            this._chkGenerateHeaders.TabIndex = 4;
            this._chkGenerateHeaders.Text = "Generate headers";
            this._chkGenerateHeaders.UseVisualStyleBackColor = true;
            // 
            // _txtDir
            // 
            this._txtDir.Location = new System.Drawing.Point(135, 21);
            this._txtDir.Name = "_txtDir";
            this._txtDir.Size = new System.Drawing.Size(381, 20);
            this._txtDir.TabIndex = 5;
            // 
            // _btnSelectDir
            // 
            this._btnSelectDir.Location = new System.Drawing.Point(9, 19);
            this._btnSelectDir.Name = "_btnSelectDir";
            this._btnSelectDir.Size = new System.Drawing.Size(120, 23);
            this._btnSelectDir.TabIndex = 4;
            this._btnSelectDir.Text = "Select directory...";
            this._btnSelectDir.UseVisualStyleBackColor = true;
            this._btnSelectDir.Click += new System.EventHandler(this._btnSelectDir_Click);
            // 
            // label3
            // 
            this.label3.Font = new System.Drawing.Font("Microsoft Sans Serif", 9.75F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.label3.Location = new System.Drawing.Point(6, 48);
            this.label3.Name = "label3";
            this.label3.Size = new System.Drawing.Size(123, 23);
            this.label3.TabIndex = 3;
            this.label3.Text = "Report type :";
            this.label3.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
            // 
            // _cmbReportType
            // 
            this._cmbReportType.FormattingEnabled = true;
            this._cmbReportType.Items.AddRange(new object[] {
            "CSV",
            "Excel 97",
            "JSON"});
            this._cmbReportType.Location = new System.Drawing.Point(135, 48);
            this._cmbReportType.Name = "_cmbReportType";
            this._cmbReportType.Size = new System.Drawing.Size(135, 21);
            this._cmbReportType.TabIndex = 0;
            // 
            // groupBox3
            // 
            this.groupBox3.Controls.Add(this._chkDestroyEverything);
            this.groupBox3.Controls.Add(this._chkDestroyLogs);
            this.groupBox3.Controls.Add(this._chkDestroyUnsynched);
            this.groupBox3.Controls.Add(this._chkDestroyFile);
            this.groupBox3.Controls.Add(this._chkDestroyData);
            this.groupBox3.Location = new System.Drawing.Point(334, 12);
            this.groupBox3.Name = "groupBox3";
            this.groupBox3.Size = new System.Drawing.Size(343, 110);
            this.groupBox3.TabIndex = 3;
            this.groupBox3.TabStop = false;
            this.groupBox3.Text = "Use with caution !!!";
            // 
            // _chkDestroyEverything
            // 
            this._chkDestroyEverything.AutoSize = true;
            this._chkDestroyEverything.Location = new System.Drawing.Point(203, 49);
            this._chkDestroyEverything.Name = "_chkDestroyEverything";
            this._chkDestroyEverything.Size = new System.Drawing.Size(117, 17);
            this._chkDestroyEverything.TabIndex = 9;
            this._chkDestroyEverything.Text = "Destroy everything!";
            this._chkDestroyEverything.UseVisualStyleBackColor = true;
            // 
            // _chkDestroyLogs
            // 
            this._chkDestroyLogs.AutoSize = true;
            this._chkDestroyLogs.Location = new System.Drawing.Point(6, 87);
            this._chkDestroyLogs.Name = "_chkDestroyLogs";
            this._chkDestroyLogs.Size = new System.Drawing.Size(84, 17);
            this._chkDestroyLogs.TabIndex = 8;
            this._chkDestroyLogs.Text = "Destroy logs";
            this._chkDestroyLogs.UseVisualStyleBackColor = true;
            // 
            // _chkDestroyUnsynched
            // 
            this._chkDestroyUnsynched.AutoSize = true;
            this._chkDestroyUnsynched.Location = new System.Drawing.Point(6, 66);
            this._chkDestroyUnsynched.Name = "_chkDestroyUnsynched";
            this._chkDestroyUnsynched.Size = new System.Drawing.Size(144, 17);
            this._chkDestroyUnsynched.TabIndex = 7;
            this._chkDestroyUnsynched.Text = "Destroy un-synched data";
            this._chkDestroyUnsynched.UseVisualStyleBackColor = true;
            // 
            // _chkDestroyFile
            // 
            this._chkDestroyFile.AutoSize = true;
            this._chkDestroyFile.Location = new System.Drawing.Point(6, 43);
            this._chkDestroyFile.Name = "_chkDestroyFile";
            this._chkDestroyFile.Size = new System.Drawing.Size(193, 17);
            this._chkDestroyFile.TabIndex = 6;
            this._chkDestroyFile.Text = "Destroy associated files after export";
            this._chkDestroyFile.UseVisualStyleBackColor = true;
            // 
            // _chkDestroyData
            // 
            this._chkDestroyData.AutoSize = true;
            this._chkDestroyData.Location = new System.Drawing.Point(6, 21);
            this._chkDestroyData.Name = "_chkDestroyData";
            this._chkDestroyData.Size = new System.Drawing.Size(142, 17);
            this._chkDestroyData.TabIndex = 5;
            this._chkDestroyData.Text = "Destroy data after export";
            this._chkDestroyData.UseVisualStyleBackColor = true;
            // 
            // _btnStartExtraction
            // 
            this._btnStartExtraction.Location = new System.Drawing.Point(557, 172);
            this._btnStartExtraction.Name = "_btnStartExtraction";
            this._btnStartExtraction.Size = new System.Drawing.Size(120, 23);
            this._btnStartExtraction.TabIndex = 7;
            this._btnStartExtraction.Text = "Extract";
            this._btnStartExtraction.UseVisualStyleBackColor = true;
            this._btnStartExtraction.Click += new System.EventHandler(this._btnStartExtraction_Click);
            // 
            // ExtractorUi
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(692, 233);
            this.Controls.Add(this._btnStartExtraction);
            this.Controls.Add(this.groupBox3);
            this.Controls.Add(this._groupSelectionConfig);
            this.Controls.Add(this.groupBox1);
            this.MaximizeBox = false;
            this.Name = "ExtractorUi";
            this.Text = "iBrand Project report";
            this.Load += new System.EventHandler(this.ExtractorUi_Load);
            this.groupBox1.ResumeLayout(false);
            this._groupSelectionConfig.ResumeLayout(false);
            this._groupSelectionConfig.PerformLayout();
            this.groupBox3.ResumeLayout(false);
            this.groupBox3.PerformLayout();
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.ComboBox _cmbSelectTerminal;
        private System.Windows.Forms.GroupBox groupBox1;
        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.Label _lblTotalRespondent;
        private System.Windows.Forms.GroupBox _groupSelectionConfig;
        private System.Windows.Forms.Button _btnSelectDir;
        private System.Windows.Forms.Label label3;
        private System.Windows.Forms.ComboBox _cmbReportType;
        private System.Windows.Forms.Label _lblLastDate;
        private System.Windows.Forms.Label label5;
        private System.Windows.Forms.CheckBox _chkIncludeMetadata;
        private System.Windows.Forms.CheckBox _chkGenerateHeaders;
        private System.Windows.Forms.TextBox _txtDir;
        private System.Windows.Forms.GroupBox groupBox3;
        private System.Windows.Forms.CheckBox _chkDestroyEverything;
        private System.Windows.Forms.CheckBox _chkDestroyLogs;
        private System.Windows.Forms.CheckBox _chkDestroyUnsynched;
        private System.Windows.Forms.CheckBox _chkDestroyFile;
        private System.Windows.Forms.CheckBox _chkDestroyData;
        private System.Windows.Forms.Button _btnStartExtraction;
    }
}

