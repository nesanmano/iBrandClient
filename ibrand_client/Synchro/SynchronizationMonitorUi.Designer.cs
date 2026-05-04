namespace NomadLogic.iBrandDesktop.Synchro
{
    partial class SynchronizationMonitorUi
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
            this.panel1 = new System.Windows.Forms.Panel();
            this.groupBox2 = new System.Windows.Forms.GroupBox();
            this._txtMessages = new System.Windows.Forms.TextBox();
            this.dataGridView1 = new System.Windows.Forms.DataGridView();
            this.colQuest = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.colTotal = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.colSubmit = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.colSyncReady = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this._btnPause = new System.Windows.Forms.Button();
            this.groupBox1 = new System.Windows.Forms.GroupBox();
            this.label11 = new System.Windows.Forms.Label();
            this._prbCurrentUpload = new System.Windows.Forms.ProgressBar();
            this._lblTotalData = new System.Windows.Forms.Label();
            this._lblTotalSinceStart = new System.Windows.Forms.Label();
            this._lblErrorNumber = new System.Windows.Forms.Label();
            this._lblRemainToSync = new System.Windows.Forms.Label();
            this.label6 = new System.Windows.Forms.Label();
            this.label5 = new System.Windows.Forms.Label();
            this.label4 = new System.Windows.Forms.Label();
            this.label3 = new System.Windows.Forms.Label();
            this._chkLiveSynchro = new System.Windows.Forms.CheckBox();
            this.label2 = new System.Windows.Forms.Label();
            this._lblStatusInfo = new System.Windows.Forms.Label();
            this.label1 = new System.Windows.Forms.Label();
            this.panel2 = new System.Windows.Forms.Panel();
            this._lblSubmitActionClassName = new System.Windows.Forms.Label();
            this.label7 = new System.Windows.Forms.Label();
            this.panel1.SuspendLayout();
            this.groupBox2.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.dataGridView1)).BeginInit();
            this.groupBox1.SuspendLayout();
            this.panel2.SuspendLayout();
            this.SuspendLayout();
            // 
            // panel1
            // 
            this.panel1.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            this.panel1.Controls.Add(this.groupBox2);
            this.panel1.Controls.Add(this.dataGridView1);
            this.panel1.Controls.Add(this._btnPause);
            this.panel1.Controls.Add(this.groupBox1);
            this.panel1.Controls.Add(this._chkLiveSynchro);
            this.panel1.Controls.Add(this.label2);
            this.panel1.Controls.Add(this._lblStatusInfo);
            this.panel1.Controls.Add(this.label1);
            this.panel1.Location = new System.Drawing.Point(12, 12);
            this.panel1.Name = "panel1";
            this.panel1.Size = new System.Drawing.Size(328, 464);
            this.panel1.TabIndex = 0;
            // 
            // groupBox2
            // 
            this.groupBox2.Controls.Add(this._txtMessages);
            this.groupBox2.Location = new System.Drawing.Point(6, 308);
            this.groupBox2.Name = "groupBox2";
            this.groupBox2.Size = new System.Drawing.Size(314, 151);
            this.groupBox2.TabIndex = 7;
            this.groupBox2.TabStop = false;
            this.groupBox2.Text = "Messages";
            // 
            // _txtMessages
            // 
            this._txtMessages.Location = new System.Drawing.Point(6, 19);
            this._txtMessages.Multiline = true;
            this._txtMessages.Name = "_txtMessages";
            this._txtMessages.ReadOnly = true;
            this._txtMessages.ScrollBars = System.Windows.Forms.ScrollBars.Vertical;
            this._txtMessages.Size = new System.Drawing.Size(302, 126);
            this._txtMessages.TabIndex = 0;
            // 
            // dataGridView1
            // 
            this.dataGridView1.AllowUserToAddRows = false;
            this.dataGridView1.AllowUserToDeleteRows = false;
            this.dataGridView1.ColumnHeadersHeightSizeMode = System.Windows.Forms.DataGridViewColumnHeadersHeightSizeMode.AutoSize;
            this.dataGridView1.Columns.AddRange(new System.Windows.Forms.DataGridViewColumn[] {
            this.colQuest,
            this.colTotal,
            this.colSubmit,
            this.colSyncReady});
            this.dataGridView1.Location = new System.Drawing.Point(6, 203);
            this.dataGridView1.MultiSelect = false;
            this.dataGridView1.Name = "dataGridView1";
            this.dataGridView1.ReadOnly = true;
            this.dataGridView1.ScrollBars = System.Windows.Forms.ScrollBars.Vertical;
            this.dataGridView1.Size = new System.Drawing.Size(314, 99);
            this.dataGridView1.TabIndex = 6;
            // 
            // colQuest
            // 
            this.colQuest.AutoSizeMode = System.Windows.Forms.DataGridViewAutoSizeColumnMode.None;
            this.colQuest.FillWeight = 55F;
            this.colQuest.HeaderText = "Quest";
            this.colQuest.Name = "colQuest";
            this.colQuest.ReadOnly = true;
            this.colQuest.Resizable = System.Windows.Forms.DataGridViewTriState.False;
            this.colQuest.Width = 90;
            // 
            // colTotal
            // 
            this.colTotal.AutoSizeMode = System.Windows.Forms.DataGridViewAutoSizeColumnMode.None;
            this.colTotal.FillWeight = 15F;
            this.colTotal.HeaderText = "Tot.";
            this.colTotal.Name = "colTotal";
            this.colTotal.ReadOnly = true;
            this.colTotal.Resizable = System.Windows.Forms.DataGridViewTriState.False;
            this.colTotal.Width = 51;
            // 
            // colSubmit
            // 
            this.colSubmit.AutoSizeMode = System.Windows.Forms.DataGridViewAutoSizeColumnMode.None;
            this.colSubmit.FillWeight = 15F;
            this.colSubmit.HeaderText = "Sub. Act.";
            this.colSubmit.Name = "colSubmit";
            this.colSubmit.ReadOnly = true;
            this.colSubmit.Resizable = System.Windows.Forms.DataGridViewTriState.False;
            this.colSubmit.Width = 76;
            // 
            // colSyncReady
            // 
            this.colSyncReady.AutoSizeMode = System.Windows.Forms.DataGridViewAutoSizeColumnMode.None;
            this.colSyncReady.FillWeight = 15F;
            this.colSyncReady.HeaderText = "Rdy.";
            this.colSyncReady.Name = "colSyncReady";
            this.colSyncReady.ReadOnly = true;
            this.colSyncReady.Resizable = System.Windows.Forms.DataGridViewTriState.False;
            this.colSyncReady.Width = 54;
            // 
            // _btnPause
            // 
            this._btnPause.Location = new System.Drawing.Point(253, 32);
            this._btnPause.Name = "_btnPause";
            this._btnPause.Size = new System.Drawing.Size(67, 23);
            this._btnPause.TabIndex = 5;
            this._btnPause.Text = "Pause";
            this._btnPause.UseVisualStyleBackColor = true;
            this._btnPause.Click += new System.EventHandler(this.BtnPause_Click);
            // 
            // groupBox1
            // 
            this.groupBox1.Controls.Add(this.label11);
            this.groupBox1.Controls.Add(this._prbCurrentUpload);
            this.groupBox1.Controls.Add(this._lblTotalData);
            this.groupBox1.Controls.Add(this._lblTotalSinceStart);
            this.groupBox1.Controls.Add(this._lblErrorNumber);
            this.groupBox1.Controls.Add(this._lblRemainToSync);
            this.groupBox1.Controls.Add(this.label6);
            this.groupBox1.Controls.Add(this.label5);
            this.groupBox1.Controls.Add(this.label4);
            this.groupBox1.Controls.Add(this.label3);
            this.groupBox1.Location = new System.Drawing.Point(6, 59);
            this.groupBox1.Name = "groupBox1";
            this.groupBox1.Size = new System.Drawing.Size(314, 137);
            this.groupBox1.TabIndex = 4;
            this.groupBox1.TabStop = false;
            this.groupBox1.Text = "Statistics";
            // 
            // label11
            // 
            this.label11.Location = new System.Drawing.Point(6, 108);
            this.label11.Name = "label11";
            this.label11.Size = new System.Drawing.Size(116, 23);
            this.label11.TabIndex = 13;
            this.label11.Text = "Current upload :";
            this.label11.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
            // 
            // _prbCurrentUpload
            // 
            this._prbCurrentUpload.Location = new System.Drawing.Point(128, 111);
            this._prbCurrentUpload.Name = "_prbCurrentUpload";
            this._prbCurrentUpload.Size = new System.Drawing.Size(180, 20);
            this._prbCurrentUpload.TabIndex = 12;
            // 
            // _lblTotalData
            // 
            this._lblTotalData.Font = new System.Drawing.Font("Microsoft Sans Serif", 9.75F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this._lblTotalData.Location = new System.Drawing.Point(168, 85);
            this._lblTotalData.Name = "_lblTotalData";
            this._lblTotalData.Size = new System.Drawing.Size(106, 23);
            this._lblTotalData.TabIndex = 11;
            this._lblTotalData.Text = "STATUS";
            this._lblTotalData.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
            // 
            // _lblTotalSinceStart
            // 
            this._lblTotalSinceStart.Font = new System.Drawing.Font("Microsoft Sans Serif", 9.75F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this._lblTotalSinceStart.Location = new System.Drawing.Point(168, 62);
            this._lblTotalSinceStart.Name = "_lblTotalSinceStart";
            this._lblTotalSinceStart.Size = new System.Drawing.Size(106, 23);
            this._lblTotalSinceStart.TabIndex = 10;
            this._lblTotalSinceStart.Text = "STATUS";
            this._lblTotalSinceStart.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
            // 
            // _lblErrorNumber
            // 
            this._lblErrorNumber.Font = new System.Drawing.Font("Microsoft Sans Serif", 9.75F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this._lblErrorNumber.Location = new System.Drawing.Point(168, 39);
            this._lblErrorNumber.Name = "_lblErrorNumber";
            this._lblErrorNumber.Size = new System.Drawing.Size(106, 23);
            this._lblErrorNumber.TabIndex = 9;
            this._lblErrorNumber.Text = "STATUS";
            this._lblErrorNumber.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
            // 
            // _lblRemainToSync
            // 
            this._lblRemainToSync.Font = new System.Drawing.Font("Microsoft Sans Serif", 9.75F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this._lblRemainToSync.Location = new System.Drawing.Point(168, 16);
            this._lblRemainToSync.Name = "_lblRemainToSync";
            this._lblRemainToSync.Size = new System.Drawing.Size(106, 23);
            this._lblRemainToSync.TabIndex = 5;
            this._lblRemainToSync.Text = "STATUS";
            this._lblRemainToSync.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
            // 
            // label6
            // 
            this.label6.Location = new System.Drawing.Point(6, 85);
            this.label6.Name = "label6";
            this.label6.Size = new System.Drawing.Size(156, 23);
            this.label6.TabIndex = 8;
            this.label6.Text = "Data sent (files) :";
            this.label6.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
            // 
            // label5
            // 
            this.label5.Location = new System.Drawing.Point(6, 62);
            this.label5.Name = "label5";
            this.label5.Size = new System.Drawing.Size(156, 23);
            this.label5.TabIndex = 7;
            this.label5.Text = "Total syncs since start :";
            this.label5.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
            // 
            // label4
            // 
            this.label4.Location = new System.Drawing.Point(6, 39);
            this.label4.Name = "label4";
            this.label4.Size = new System.Drawing.Size(156, 23);
            this.label4.TabIndex = 6;
            this.label4.Text = "Number of errors :";
            this.label4.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
            // 
            // label3
            // 
            this.label3.Location = new System.Drawing.Point(6, 16);
            this.label3.Name = "label3";
            this.label3.Size = new System.Drawing.Size(156, 23);
            this.label3.TabIndex = 5;
            this.label3.Text = "Respondents to sync :";
            this.label3.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
            // 
            // _chkLiveSynchro
            // 
            this._chkLiveSynchro.AutoSize = true;
            this._chkLiveSynchro.Location = new System.Drawing.Point(175, 36);
            this._chkLiveSynchro.Name = "_chkLiveSynchro";
            this._chkLiveSynchro.Size = new System.Drawing.Size(71, 17);
            this._chkLiveSynchro.TabIndex = 3;
            this._chkLiveSynchro.Text = "Activated";
            this._chkLiveSynchro.UseVisualStyleBackColor = true;
            this._chkLiveSynchro.CheckedChanged += new System.EventHandler(this.ChkLiveSynchro_CheckedChanged);
            // 
            // label2
            // 
            this.label2.Location = new System.Drawing.Point(3, 32);
            this.label2.Name = "label2";
            this.label2.Size = new System.Drawing.Size(165, 23);
            this.label2.TabIndex = 2;
            this.label2.Text = "Live synchronization :";
            this.label2.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
            // 
            // _lblStatusInfo
            // 
            this._lblStatusInfo.Font = new System.Drawing.Font("Microsoft Sans Serif", 9.75F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this._lblStatusInfo.Location = new System.Drawing.Point(174, 9);
            this._lblStatusInfo.Name = "_lblStatusInfo";
            this._lblStatusInfo.Size = new System.Drawing.Size(165, 23);
            this._lblStatusInfo.TabIndex = 1;
            this._lblStatusInfo.Text = "STATUS";
            this._lblStatusInfo.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
            // 
            // label1
            // 
            this.label1.Location = new System.Drawing.Point(3, 9);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(165, 23);
            this.label1.TabIndex = 0;
            this.label1.Text = "Synchronization service status :";
            this.label1.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
            // 
            // panel2
            // 
            this.panel2.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            this.panel2.Controls.Add(this._lblSubmitActionClassName);
            this.panel2.Controls.Add(this.label7);
            this.panel2.Location = new System.Drawing.Point(346, 12);
            this.panel2.Name = "panel2";
            this.panel2.Size = new System.Drawing.Size(442, 464);
            this.panel2.TabIndex = 1;
            // 
            // _lblSubmitActionClassName
            // 
            this._lblSubmitActionClassName.Font = new System.Drawing.Font("Microsoft Sans Serif", 9.75F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this._lblSubmitActionClassName.Location = new System.Drawing.Point(174, 9);
            this._lblSubmitActionClassName.Name = "_lblSubmitActionClassName";
            this._lblSubmitActionClassName.Size = new System.Drawing.Size(263, 23);
            this._lblSubmitActionClassName.TabIndex = 8;
            this._lblSubmitActionClassName.Text = "STATUS";
            this._lblSubmitActionClassName.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
            // 
            // label7
            // 
            this.label7.Location = new System.Drawing.Point(3, 9);
            this.label7.Name = "label7";
            this.label7.Size = new System.Drawing.Size(165, 23);
            this.label7.TabIndex = 8;
            this.label7.Text = "Submit Action Class :";
            this.label7.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
            // 
            // SynchronizationMonitorUi
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(800, 488);
            this.Controls.Add(this.panel2);
            this.Controls.Add(this.panel1);
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedDialog;
            this.MaximizeBox = false;
            this.MinimizeBox = false;
            this.Name = "SynchronizationMonitorUi";
            this.Text = "Synchronization and Post-processing Monitor";
            this.FormClosing += new System.Windows.Forms.FormClosingEventHandler(this.SynchronizationMonitorUi_FormClosing);
            this.Load += new System.EventHandler(this.SynchronizationMonitorUi_Load);
            this.panel1.ResumeLayout(false);
            this.panel1.PerformLayout();
            this.groupBox2.ResumeLayout(false);
            this.groupBox2.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)(this.dataGridView1)).EndInit();
            this.groupBox1.ResumeLayout(false);
            this.panel2.ResumeLayout(false);
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.Panel panel1;
        private System.Windows.Forms.GroupBox groupBox1;
        private System.Windows.Forms.CheckBox _chkLiveSynchro;
        private System.Windows.Forms.Label label2;
        private System.Windows.Forms.Label _lblStatusInfo;
        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.Button _btnPause;
        private System.Windows.Forms.Label label11;
        private System.Windows.Forms.ProgressBar _prbCurrentUpload;
        private System.Windows.Forms.Label _lblTotalData;
        private System.Windows.Forms.Label _lblTotalSinceStart;
        private System.Windows.Forms.Label _lblErrorNumber;
        private System.Windows.Forms.Label _lblRemainToSync;
        private System.Windows.Forms.Label label6;
        private System.Windows.Forms.Label label5;
        private System.Windows.Forms.Label label4;
        private System.Windows.Forms.Label label3;
        private System.Windows.Forms.GroupBox groupBox2;
        private System.Windows.Forms.TextBox _txtMessages;
        private System.Windows.Forms.DataGridView dataGridView1;
        private System.Windows.Forms.DataGridViewTextBoxColumn colQuest;
        private System.Windows.Forms.DataGridViewTextBoxColumn colTotal;
        private System.Windows.Forms.DataGridViewTextBoxColumn colSubmit;
        private System.Windows.Forms.DataGridViewTextBoxColumn colSyncReady;
        private System.Windows.Forms.Panel panel2;
        private System.Windows.Forms.Label label7;
        private System.Windows.Forms.Label _lblSubmitActionClassName;
    }
}