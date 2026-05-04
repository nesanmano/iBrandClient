namespace NomadLogic.Multimedia.Gpu.OpenCV
{
    partial class FileBundleCalibrationDialog
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
            this.label1 = new System.Windows.Forms.Label();
            this.panel1 = new System.Windows.Forms.Panel();
            this.label6 = new System.Windows.Forms.Label();
            this.boxC1R = new System.Windows.Forms.TextBox();
            this.label3 = new System.Windows.Forms.Label();
            this.boxC1H = new System.Windows.Forms.TextBox();
            this.label5 = new System.Windows.Forms.Label();
            this.boxC1Y = new System.Windows.Forms.TextBox();
            this.label4 = new System.Windows.Forms.Label();
            this.boxC1W = new System.Windows.Forms.TextBox();
            this.label2 = new System.Windows.Forms.Label();
            this.boxC1X = new System.Windows.Forms.TextBox();
            this.panel2 = new System.Windows.Forms.Panel();
            this.label7 = new System.Windows.Forms.Label();
            this.boxC2R = new System.Windows.Forms.TextBox();
            this.label8 = new System.Windows.Forms.Label();
            this.boxC2H = new System.Windows.Forms.TextBox();
            this.label9 = new System.Windows.Forms.Label();
            this.boxC2Y = new System.Windows.Forms.TextBox();
            this.label10 = new System.Windows.Forms.Label();
            this.boxC2W = new System.Windows.Forms.TextBox();
            this.label11 = new System.Windows.Forms.Label();
            this.boxC2X = new System.Windows.Forms.TextBox();
            this.label12 = new System.Windows.Forms.Label();
            this.btnGenerate = new System.Windows.Forms.Button();
            this.btnClear = new System.Windows.Forms.Button();
            this.btnPreview = new System.Windows.Forms.Button();
            this.btnClose = new System.Windows.Forms.Button();
            this.chkOnlyTranslation = new System.Windows.Forms.CheckBox();
            this.progBar = new System.Windows.Forms.ProgressBar();
            this.panel1.SuspendLayout();
            this.panel2.SuspendLayout();
            this.SuspendLayout();
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Location = new System.Drawing.Point(3, 0);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(67, 13);
            this.label1.TabIndex = 0;
            this.label1.Text = "Coordinate 1";
            // 
            // panel1
            // 
            this.panel1.Controls.Add(this.label6);
            this.panel1.Controls.Add(this.boxC1R);
            this.panel1.Controls.Add(this.label3);
            this.panel1.Controls.Add(this.boxC1H);
            this.panel1.Controls.Add(this.label5);
            this.panel1.Controls.Add(this.boxC1Y);
            this.panel1.Controls.Add(this.label4);
            this.panel1.Controls.Add(this.boxC1W);
            this.panel1.Controls.Add(this.label2);
            this.panel1.Controls.Add(this.boxC1X);
            this.panel1.Controls.Add(this.label1);
            this.panel1.Location = new System.Drawing.Point(12, 12);
            this.panel1.Name = "panel1";
            this.panel1.Size = new System.Drawing.Size(198, 96);
            this.panel1.TabIndex = 1;
            // 
            // label6
            // 
            this.label6.AutoSize = true;
            this.label6.Location = new System.Drawing.Point(3, 71);
            this.label6.Name = "label6";
            this.label6.Size = new System.Drawing.Size(40, 13);
            this.label6.TabIndex = 12;
            this.label6.Text = "Radius";
            // 
            // boxC1R
            // 
            this.boxC1R.Location = new System.Drawing.Point(44, 68);
            this.boxC1R.MaxLength = 5;
            this.boxC1R.Name = "boxC1R";
            this.boxC1R.Size = new System.Drawing.Size(47, 20);
            this.boxC1R.TabIndex = 5;
            this.boxC1R.Text = "100";
            // 
            // label3
            // 
            this.label3.AutoSize = true;
            this.label3.Location = new System.Drawing.Point(101, 45);
            this.label3.Name = "label3";
            this.label3.Size = new System.Drawing.Size(38, 13);
            this.label3.TabIndex = 10;
            this.label3.Text = "Height";
            // 
            // boxC1H
            // 
            this.boxC1H.Location = new System.Drawing.Point(142, 42);
            this.boxC1H.MaxLength = 5;
            this.boxC1H.Name = "boxC1H";
            this.boxC1H.Size = new System.Drawing.Size(47, 20);
            this.boxC1H.TabIndex = 4;
            this.boxC1H.Text = "0";
            // 
            // label5
            // 
            this.label5.AutoSize = true;
            this.label5.Location = new System.Drawing.Point(101, 19);
            this.label5.Name = "label5";
            this.label5.Size = new System.Drawing.Size(14, 13);
            this.label5.TabIndex = 8;
            this.label5.Text = "Y";
            // 
            // boxC1Y
            // 
            this.boxC1Y.Location = new System.Drawing.Point(142, 16);
            this.boxC1Y.MaxLength = 5;
            this.boxC1Y.Name = "boxC1Y";
            this.boxC1Y.Size = new System.Drawing.Size(47, 20);
            this.boxC1Y.TabIndex = 2;
            this.boxC1Y.Text = "0";
            // 
            // label4
            // 
            this.label4.AutoSize = true;
            this.label4.Location = new System.Drawing.Point(3, 45);
            this.label4.Name = "label4";
            this.label4.Size = new System.Drawing.Size(35, 13);
            this.label4.TabIndex = 6;
            this.label4.Text = "Width";
            // 
            // boxC1W
            // 
            this.boxC1W.Location = new System.Drawing.Point(44, 42);
            this.boxC1W.MaxLength = 5;
            this.boxC1W.Name = "boxC1W";
            this.boxC1W.Size = new System.Drawing.Size(47, 20);
            this.boxC1W.TabIndex = 3;
            this.boxC1W.Text = "0";
            // 
            // label2
            // 
            this.label2.AutoSize = true;
            this.label2.Location = new System.Drawing.Point(3, 19);
            this.label2.Name = "label2";
            this.label2.Size = new System.Drawing.Size(14, 13);
            this.label2.TabIndex = 2;
            this.label2.Text = "X";
            // 
            // boxC1X
            // 
            this.boxC1X.Location = new System.Drawing.Point(44, 16);
            this.boxC1X.MaxLength = 5;
            this.boxC1X.Name = "boxC1X";
            this.boxC1X.Size = new System.Drawing.Size(47, 20);
            this.boxC1X.TabIndex = 1;
            this.boxC1X.Text = "0";
            // 
            // panel2
            // 
            this.panel2.Controls.Add(this.label7);
            this.panel2.Controls.Add(this.boxC2R);
            this.panel2.Controls.Add(this.label8);
            this.panel2.Controls.Add(this.boxC2H);
            this.panel2.Controls.Add(this.label9);
            this.panel2.Controls.Add(this.boxC2Y);
            this.panel2.Controls.Add(this.label10);
            this.panel2.Controls.Add(this.boxC2W);
            this.panel2.Controls.Add(this.label11);
            this.panel2.Controls.Add(this.boxC2X);
            this.panel2.Controls.Add(this.label12);
            this.panel2.Location = new System.Drawing.Point(13, 114);
            this.panel2.Name = "panel2";
            this.panel2.Size = new System.Drawing.Size(198, 96);
            this.panel2.TabIndex = 13;
            // 
            // label7
            // 
            this.label7.AutoSize = true;
            this.label7.Location = new System.Drawing.Point(3, 71);
            this.label7.Name = "label7";
            this.label7.Size = new System.Drawing.Size(40, 13);
            this.label7.TabIndex = 12;
            this.label7.Text = "Radius";
            // 
            // boxC2R
            // 
            this.boxC2R.Location = new System.Drawing.Point(44, 68);
            this.boxC2R.MaxLength = 5;
            this.boxC2R.Name = "boxC2R";
            this.boxC2R.Size = new System.Drawing.Size(47, 20);
            this.boxC2R.TabIndex = 10;
            this.boxC2R.Text = "100";
            // 
            // label8
            // 
            this.label8.AutoSize = true;
            this.label8.Location = new System.Drawing.Point(101, 45);
            this.label8.Name = "label8";
            this.label8.Size = new System.Drawing.Size(38, 13);
            this.label8.TabIndex = 10;
            this.label8.Text = "Height";
            // 
            // boxC2H
            // 
            this.boxC2H.Location = new System.Drawing.Point(142, 42);
            this.boxC2H.MaxLength = 5;
            this.boxC2H.Name = "boxC2H";
            this.boxC2H.Size = new System.Drawing.Size(47, 20);
            this.boxC2H.TabIndex = 9;
            this.boxC2H.Text = "0";
            // 
            // label9
            // 
            this.label9.AutoSize = true;
            this.label9.Location = new System.Drawing.Point(101, 19);
            this.label9.Name = "label9";
            this.label9.Size = new System.Drawing.Size(14, 13);
            this.label9.TabIndex = 8;
            this.label9.Text = "Y";
            // 
            // boxC2Y
            // 
            this.boxC2Y.Location = new System.Drawing.Point(142, 16);
            this.boxC2Y.MaxLength = 5;
            this.boxC2Y.Name = "boxC2Y";
            this.boxC2Y.Size = new System.Drawing.Size(47, 20);
            this.boxC2Y.TabIndex = 7;
            this.boxC2Y.Text = "0";
            // 
            // label10
            // 
            this.label10.AutoSize = true;
            this.label10.Location = new System.Drawing.Point(3, 45);
            this.label10.Name = "label10";
            this.label10.Size = new System.Drawing.Size(35, 13);
            this.label10.TabIndex = 6;
            this.label10.Text = "Width";
            // 
            // boxC2W
            // 
            this.boxC2W.Location = new System.Drawing.Point(44, 42);
            this.boxC2W.MaxLength = 5;
            this.boxC2W.Name = "boxC2W";
            this.boxC2W.Size = new System.Drawing.Size(47, 20);
            this.boxC2W.TabIndex = 8;
            this.boxC2W.Text = "0";
            // 
            // label11
            // 
            this.label11.AutoSize = true;
            this.label11.Location = new System.Drawing.Point(3, 19);
            this.label11.Name = "label11";
            this.label11.Size = new System.Drawing.Size(14, 13);
            this.label11.TabIndex = 2;
            this.label11.Text = "X";
            // 
            // boxC2X
            // 
            this.boxC2X.Location = new System.Drawing.Point(44, 16);
            this.boxC2X.MaxLength = 5;
            this.boxC2X.Name = "boxC2X";
            this.boxC2X.Size = new System.Drawing.Size(47, 20);
            this.boxC2X.TabIndex = 6;
            this.boxC2X.Text = "0";
            // 
            // label12
            // 
            this.label12.AutoSize = true;
            this.label12.Location = new System.Drawing.Point(3, 0);
            this.label12.Name = "label12";
            this.label12.Size = new System.Drawing.Size(67, 13);
            this.label12.TabIndex = 0;
            this.label12.Text = "Coordinate 2";
            // 
            // btnGenerate
            // 
            this.btnGenerate.Location = new System.Drawing.Point(12, 268);
            this.btnGenerate.Name = "btnGenerate";
            this.btnGenerate.Size = new System.Drawing.Size(198, 23);
            this.btnGenerate.TabIndex = 14;
            this.btnGenerate.Text = "Generate Normalization file";
            this.btnGenerate.UseVisualStyleBackColor = true;
            this.btnGenerate.Click += new System.EventHandler(this.btnGenerate_Click);
            // 
            // btnClear
            // 
            this.btnClear.Location = new System.Drawing.Point(11, 239);
            this.btnClear.Name = "btnClear";
            this.btnClear.Size = new System.Drawing.Size(92, 23);
            this.btnClear.TabIndex = 12;
            this.btnClear.Text = "Clear";
            this.btnClear.UseVisualStyleBackColor = true;
            this.btnClear.Click += new System.EventHandler(this.btnClear_Click);
            // 
            // btnPreview
            // 
            this.btnPreview.Location = new System.Drawing.Point(109, 239);
            this.btnPreview.Name = "btnPreview";
            this.btnPreview.Size = new System.Drawing.Size(101, 23);
            this.btnPreview.TabIndex = 13;
            this.btnPreview.Text = "Preview";
            this.btnPreview.UseVisualStyleBackColor = true;
            this.btnPreview.Click += new System.EventHandler(this.btnPreview_Click);
            // 
            // btnClose
            // 
            this.btnClose.Location = new System.Drawing.Point(12, 298);
            this.btnClose.Name = "btnClose";
            this.btnClose.Size = new System.Drawing.Size(197, 23);
            this.btnClose.TabIndex = 15;
            this.btnClose.Text = "Close";
            this.btnClose.UseVisualStyleBackColor = true;
            this.btnClose.Click += new System.EventHandler(this.btnClose_Click);
            // 
            // chkOnlyTranslation
            // 
            this.chkOnlyTranslation.AutoSize = true;
            this.chkOnlyTranslation.Checked = true;
            this.chkOnlyTranslation.CheckState = System.Windows.Forms.CheckState.Checked;
            this.chkOnlyTranslation.Location = new System.Drawing.Point(11, 216);
            this.chkOnlyTranslation.Name = "chkOnlyTranslation";
            this.chkOnlyTranslation.Size = new System.Drawing.Size(98, 17);
            this.chkOnlyTranslation.TabIndex = 11;
            this.chkOnlyTranslation.Text = "Only translation";
            this.chkOnlyTranslation.UseVisualStyleBackColor = true;
            // 
            // progBar
            // 
            this.progBar.Location = new System.Drawing.Point(109, 216);
            this.progBar.Name = "progBar";
            this.progBar.Size = new System.Drawing.Size(100, 17);
            this.progBar.TabIndex = 20;
            // 
            // FileBundleCalibrationDialog
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(223, 330);
            this.ControlBox = false;
            this.Controls.Add(this.progBar);
            this.Controls.Add(this.chkOnlyTranslation);
            this.Controls.Add(this.btnClose);
            this.Controls.Add(this.btnPreview);
            this.Controls.Add(this.btnClear);
            this.Controls.Add(this.btnGenerate);
            this.Controls.Add(this.panel2);
            this.Controls.Add(this.panel1);
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedDialog;
            this.Name = "FileBundleCalibrationDialog";
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterParent;
            this.Text = "Enter Coordinates";
            this.panel1.ResumeLayout(false);
            this.panel1.PerformLayout();
            this.panel2.ResumeLayout(false);
            this.panel2.PerformLayout();
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.Panel panel1;
        private System.Windows.Forms.Label label6;
        private System.Windows.Forms.TextBox boxC1R;
        private System.Windows.Forms.Label label3;
        private System.Windows.Forms.TextBox boxC1H;
        private System.Windows.Forms.Label label5;
        private System.Windows.Forms.TextBox boxC1Y;
        private System.Windows.Forms.Label label4;
        private System.Windows.Forms.TextBox boxC1W;
        private System.Windows.Forms.Label label2;
        private System.Windows.Forms.TextBox boxC1X;
        private System.Windows.Forms.Panel panel2;
        private System.Windows.Forms.Label label7;
        private System.Windows.Forms.TextBox boxC2R;
        private System.Windows.Forms.Label label8;
        private System.Windows.Forms.TextBox boxC2H;
        private System.Windows.Forms.Label label9;
        private System.Windows.Forms.TextBox boxC2Y;
        private System.Windows.Forms.Label label10;
        private System.Windows.Forms.TextBox boxC2W;
        private System.Windows.Forms.Label label11;
        private System.Windows.Forms.TextBox boxC2X;
        private System.Windows.Forms.Label label12;
        private System.Windows.Forms.Button btnGenerate;
        private System.Windows.Forms.Button btnClear;
        private System.Windows.Forms.Button btnPreview;
        private System.Windows.Forms.Button btnClose;
        private System.Windows.Forms.CheckBox chkOnlyTranslation;
        private System.Windows.Forms.ProgressBar progBar;
    }
}