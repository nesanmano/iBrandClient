using System.ComponentModel;
using System.Windows.Forms;

namespace NomadLogic.iBrandDesktop.Ui.Choice
{
    partial class DateComboBoxes
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

        #region Component Designer generated code

        /// <summary> 
        /// Required method for Designer support - do not modify 
        /// the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            this.yearComboBox = new System.Windows.Forms.ComboBox();
            this.secondSlash = new System.Windows.Forms.Label();
            this.monthComboBox = new System.Windows.Forms.ComboBox();
            this.firstSlash = new System.Windows.Forms.Label();
            this.dayComboBox = new System.Windows.Forms.ComboBox();
            this.DayLabel = new System.Windows.Forms.Label();
            this.monthLabel = new System.Windows.Forms.Label();
            this.yearLabel = new System.Windows.Forms.Label();
            this.SuspendLayout();
            // 
            // yearComboBox
            // 
            this.yearComboBox.Location = new System.Drawing.Point(130, 17);
            this.yearComboBox.Name = "yearComboBox";
            this.yearComboBox.Size = new System.Drawing.Size(65, 22);
            this.yearComboBox.TabIndex = 9;
            // 
            // secondSlash
            // 
            this.secondSlash.Font = new System.Drawing.Font("Tahoma", 14F, System.Drawing.FontStyle.Regular);
            this.secondSlash.Location = new System.Drawing.Point(110, 15);
            this.secondSlash.Name = "secondSlash";
            this.secondSlash.Size = new System.Drawing.Size(15, 23);
            this.secondSlash.Text = "/";
            this.secondSlash.TextAlign = System.Drawing.ContentAlignment.TopCenter;
            // 
            // monthComboBox
            // 
            this.monthComboBox.Location = new System.Drawing.Point(65, 17);
            this.monthComboBox.Name = "monthComboBox";
            this.monthComboBox.Size = new System.Drawing.Size(40, 22);
            this.monthComboBox.TabIndex = 8;
            // 
            // firstSlash
            // 
            this.firstSlash.Font = new System.Drawing.Font("Tahoma", 14F, System.Drawing.FontStyle.Regular);
            this.firstSlash.Location = new System.Drawing.Point(45, 15);
            this.firstSlash.Name = "firstSlash";
            this.firstSlash.Size = new System.Drawing.Size(15, 23);
            this.firstSlash.Text = "/";
            this.firstSlash.TextAlign = System.Drawing.ContentAlignment.TopCenter;
            // 
            // dayComboBox
            // 
            this.dayComboBox.Location = new System.Drawing.Point(0, 17);
            this.dayComboBox.Name = "dayComboBox";
            this.dayComboBox.Size = new System.Drawing.Size(40, 22);
            this.dayComboBox.TabIndex = 7;
            // 
            // DayLabel
            // 
            this.DayLabel.Location = new System.Drawing.Point(0, 0);
            this.DayLabel.Name = "DayLabel";
            this.DayLabel.Size = new System.Drawing.Size(40, 15);
            this.DayLabel.Text = "Day";
            this.DayLabel.TextAlign = System.Drawing.ContentAlignment.TopCenter;
            // 
            // monthLabel
            // 
            this.monthLabel.Location = new System.Drawing.Point(65, 0);
            this.monthLabel.Name = "monthLabel";
            this.monthLabel.Size = new System.Drawing.Size(40, 15);
            this.monthLabel.Text = "Month";
            this.monthLabel.TextAlign = System.Drawing.ContentAlignment.TopCenter;
            // 
            // yearLabel
            // 
            this.yearLabel.Location = new System.Drawing.Point(130, 0);
            this.yearLabel.Name = "yearLabel";
            this.yearLabel.Size = new System.Drawing.Size(64, 15);
            this.yearLabel.Text = "Year";
            this.yearLabel.TextAlign = System.Drawing.ContentAlignment.TopCenter;
            // 
            // DateComboBoxes
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(96F, 96F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Dpi;
            this.Controls.Add(this.yearLabel);
            this.Controls.Add(this.monthLabel);
            this.Controls.Add(this.DayLabel);
            this.Controls.Add(this.yearComboBox);
            this.Controls.Add(this.secondSlash);
            this.Controls.Add(this.monthComboBox);
            this.Controls.Add(this.firstSlash);
            this.Controls.Add(this.dayComboBox);
            this.Name = "DateComboBoxes";
            this.Size = new System.Drawing.Size(197, 38);
            this.ResumeLayout(false);

        }

        #endregion

        private ComboBox yearComboBox;
        private Label secondSlash;
        private ComboBox monthComboBox;
        private Label firstSlash;
        private ComboBox dayComboBox;
        private Label DayLabel;
        private Label monthLabel;
        private Label yearLabel;
    }
}
