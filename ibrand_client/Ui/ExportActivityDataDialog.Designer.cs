namespace NomadLogic.iBrandDesktop.Ui
{
    partial class ExportActivityDataDialog
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
            this._cmbSelectedTerminal = new System.Windows.Forms.ComboBox();
            this.label1 = new System.Windows.Forms.Label();
            this._btnStartExtraction = new System.Windows.Forms.Button();
            this._txtDir = new System.Windows.Forms.TextBox();
            this._btnSelectDir = new System.Windows.Forms.Button();
            this.SuspendLayout();
            // 
            // _cmbSelectedTerminal
            // 
            this._cmbSelectedTerminal.FormattingEnabled = true;
            this._cmbSelectedTerminal.Location = new System.Drawing.Point(12, 28);
            this._cmbSelectedTerminal.Name = "_cmbSelectedTerminal";
            this._cmbSelectedTerminal.Size = new System.Drawing.Size(306, 21);
            this._cmbSelectedTerminal.TabIndex = 0;
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Location = new System.Drawing.Point(13, 9);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(69, 13);
            this.label1.TabIndex = 1;
            this.label1.Text = "Terminal filter";
            // 
            // _btnStartExtraction
            // 
            this._btnStartExtraction.Location = new System.Drawing.Point(399, 28);
            this._btnStartExtraction.Name = "_btnStartExtraction";
            this._btnStartExtraction.Size = new System.Drawing.Size(120, 23);
            this._btnStartExtraction.TabIndex = 10;
            this._btnStartExtraction.Text = "Extract";
            this._btnStartExtraction.UseVisualStyleBackColor = true;
            // 
            // _txtDir
            // 
            this._txtDir.Location = new System.Drawing.Point(138, 85);
            this._txtDir.Name = "_txtDir";
            this._txtDir.Size = new System.Drawing.Size(381, 20);
            this._txtDir.TabIndex = 9;
            // 
            // _btnSelectDir
            // 
            this._btnSelectDir.Location = new System.Drawing.Point(12, 85);
            this._btnSelectDir.Name = "_btnSelectDir";
            this._btnSelectDir.Size = new System.Drawing.Size(120, 23);
            this._btnSelectDir.TabIndex = 8;
            this._btnSelectDir.Text = "Select directory...";
            this._btnSelectDir.UseVisualStyleBackColor = true;
            // 
            // ExportActivityDataDialog
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(534, 137);
            this.Controls.Add(this._btnStartExtraction);
            this.Controls.Add(this._txtDir);
            this.Controls.Add(this._btnSelectDir);
            this.Controls.Add(this.label1);
            this.Controls.Add(this._cmbSelectedTerminal);
            this.Name = "ExportActivityDataDialog";
            this.Text = "Export Activity Data";
            this.Load += new System.EventHandler(this.OnLoad);
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.ComboBox _cmbSelectedTerminal;
        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.Button _btnStartExtraction;
        private System.Windows.Forms.TextBox _txtDir;
        private System.Windows.Forms.Button _btnSelectDir;
    }
}