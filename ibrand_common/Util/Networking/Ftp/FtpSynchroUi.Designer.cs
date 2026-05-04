namespace NomadLogic.Common.Util.Networking.Ftp
{
    partial class FtpSynchroUi
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
            this._progBarTotal = new System.Windows.Forms.ProgressBar();
            this._lblFilters = new System.Windows.Forms.Label();
            this._lblFileTotal = new System.Windows.Forms.Label();
            this.label4 = new System.Windows.Forms.Label();
            this._btnCancel = new System.Windows.Forms.Button();
            this._txtBoxFilters = new System.Windows.Forms.TextBox();
            this.SuspendLayout();
            // 
            // _progBarTotal
            // 
            this._progBarTotal.Location = new System.Drawing.Point(13, 157);
            this._progBarTotal.Name = "_progBarTotal";
            this._progBarTotal.Size = new System.Drawing.Size(316, 23);
            this._progBarTotal.TabIndex = 0;
            // 
            // _lblFilters
            // 
            this._lblFilters.Location = new System.Drawing.Point(12, 13);
            this._lblFilters.Name = "_lblFilters";
            this._lblFilters.Size = new System.Drawing.Size(56, 23);
            this._lblFilters.TabIndex = 1;
            this._lblFilters.Text = "Filters";
            this._lblFilters.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
            // 
            // _lblFileTotal
            // 
            this._lblFileTotal.Location = new System.Drawing.Point(159, 131);
            this._lblFileTotal.Name = "_lblFileTotal";
            this._lblFileTotal.Size = new System.Drawing.Size(100, 23);
            this._lblFileTotal.TabIndex = 4;
            this._lblFileTotal.Text = "NONE";
            this._lblFileTotal.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
            // 
            // label4
            // 
            this.label4.Location = new System.Drawing.Point(13, 131);
            this.label4.Name = "label4";
            this.label4.Size = new System.Drawing.Size(139, 23);
            this.label4.TabIndex = 3;
            this.label4.Text = "File total to download :";
            this.label4.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
            // 
            // _btnCancel
            // 
            this._btnCancel.Enabled = false;
            this._btnCancel.Location = new System.Drawing.Point(13, 186);
            this._btnCancel.Name = "_btnCancel";
            this._btnCancel.Size = new System.Drawing.Size(75, 23);
            this._btnCancel.TabIndex = 5;
            this._btnCancel.Text = "Close";
            this._btnCancel.UseVisualStyleBackColor = true;
            this._btnCancel.Click += new System.EventHandler(this._btnCancel_Click);
            // 
            // _txtBoxFilters
            // 
            this._txtBoxFilters.BackColor = System.Drawing.SystemColors.Control;
            this._txtBoxFilters.BorderStyle = System.Windows.Forms.BorderStyle.None;
            this._txtBoxFilters.Location = new System.Drawing.Point(75, 13);
            this._txtBoxFilters.Multiline = true;
            this._txtBoxFilters.Name = "_txtBoxFilters";
            this._txtBoxFilters.ReadOnly = true;
            this._txtBoxFilters.ScrollBars = System.Windows.Forms.ScrollBars.Vertical;
            this._txtBoxFilters.Size = new System.Drawing.Size(253, 115);
            this._txtBoxFilters.TabIndex = 6;
            this._txtBoxFilters.WordWrap = false;
            // 
            // FtpSynchroUi
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(341, 220);
            this.Controls.Add(this._txtBoxFilters);
            this.Controls.Add(this._btnCancel);
            this.Controls.Add(this._lblFileTotal);
            this.Controls.Add(this.label4);
            this.Controls.Add(this._lblFilters);
            this.Controls.Add(this._progBarTotal);
            this.MaximizeBox = false;
            this.MinimizeBox = false;
            this.Name = "FtpSynchroUi";
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterParent;
            this.Text = "Files downloading";
            this.FormClosing += new System.Windows.Forms.FormClosingEventHandler(this.FtpSynchroUi_FormClosing);
            this.Load += new System.EventHandler(this.FtpSynchroUi_Load);
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.ProgressBar _progBarTotal;
        private System.Windows.Forms.Label _lblFilters;
        private System.Windows.Forms.Label _lblFileTotal;
        private System.Windows.Forms.Label label4;
        private System.Windows.Forms.Button _btnCancel;
        private System.Windows.Forms.TextBox _txtBoxFilters;
    }
}