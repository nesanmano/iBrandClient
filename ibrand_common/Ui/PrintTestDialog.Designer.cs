namespace NomadLogic.Common.Ui
{
    partial class PrintTestDialog
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
        protected override void InitializeComponent()
        {
            this.btnCancel = new System.Windows.Forms.Button();
            this.pict4x6 = new System.Windows.Forms.PictureBox();
            this.pict4x6x2 = new System.Windows.Forms.PictureBox();
            this.pict8_5x11 = new System.Windows.Forms.PictureBox();
            this.pict5x7 = new System.Windows.Forms.PictureBox();
            this._btnCustomFile = new System.Windows.Forms.Button();
            ((System.ComponentModel.ISupportInitialize)(this.pict4x6)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.pict4x6x2)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.pict8_5x11)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.pict5x7)).BeginInit();
            this.SuspendLayout();
            // 
            // btnCancel
            // 
            this.btnCancel.Location = new System.Drawing.Point(12, 12);
            this.btnCancel.Name = "btnCancel";
            this.btnCancel.Size = new System.Drawing.Size(75, 23);
            this.btnCancel.TabIndex = 0;
            this.btnCancel.Text = "Cancel";
            this.btnCancel.UseVisualStyleBackColor = true;
            this.btnCancel.Click += new System.EventHandler(this.BtnCancel_Click);
            // 
            // pict4x6
            // 
            this.pict4x6.Location = new System.Drawing.Point(12, 41);
            this.pict4x6.Name = "pict4x6";
            this.pict4x6.Size = new System.Drawing.Size(134, 200);
            this.pict4x6.TabIndex = 1;
            this.pict4x6.TabStop = false;
            // 
            // pict4x6x2
            // 
            this.pict4x6x2.Location = new System.Drawing.Point(152, 42);
            this.pict4x6x2.Name = "pict4x6x2";
            this.pict4x6x2.Size = new System.Drawing.Size(134, 200);
            this.pict4x6x2.TabIndex = 2;
            this.pict4x6x2.TabStop = false;
            // 
            // pict8_5x11
            // 
            this.pict8_5x11.Location = new System.Drawing.Point(292, 42);
            this.pict8_5x11.Name = "pict8_5x11";
            this.pict8_5x11.Size = new System.Drawing.Size(155, 200);
            this.pict8_5x11.TabIndex = 3;
            this.pict8_5x11.TabStop = false;
            // 
            // pict5x7
            // 
            this.pict5x7.Location = new System.Drawing.Point(453, 42);
            this.pict5x7.Name = "pict5x7";
            this.pict5x7.Size = new System.Drawing.Size(143, 200);
            this.pict5x7.TabIndex = 4;
            this.pict5x7.TabStop = false;
            // 
            // _btnCustomFile
            // 
            this._btnCustomFile.Location = new System.Drawing.Point(361, 12);
            this._btnCustomFile.Name = "_btnCustomFile";
            this._btnCustomFile.Size = new System.Drawing.Size(236, 23);
            this._btnCustomFile.TabIndex = 5;
            this._btnCustomFile.Text = "Select image file...";
            this._btnCustomFile.UseVisualStyleBackColor = true;
            this._btnCustomFile.Click += new System.EventHandler(this.BtnCustomFile_Click);
            // 
            // PrintTestDialog
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(609, 254);
            this.Controls.Add(this._btnCustomFile);
            this.Controls.Add(this.pict5x7);
            this.Controls.Add(this.pict8_5x11);
            this.Controls.Add(this.pict4x6x2);
            this.Controls.Add(this.pict4x6);
            this.Controls.Add(this.btnCancel);
            this.Name = "PrintTestDialog";
            ((System.ComponentModel.ISupportInitialize)(this.pict4x6)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.pict4x6x2)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.pict8_5x11)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.pict5x7)).EndInit();
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.Button btnCancel;
        private System.Windows.Forms.PictureBox pict4x6;
        private System.Windows.Forms.PictureBox pict4x6x2;
        private System.Windows.Forms.PictureBox pict8_5x11;
        private System.Windows.Forms.PictureBox pict5x7;
        private System.Windows.Forms.Button _btnCustomFile;
    }
}