using System.ComponentModel;
using System.Windows.Forms;

namespace NomadLogic.iBrandDesktop.Ui
{
    partial class SetDateDlg
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
            this.date_label = new System.Windows.Forms.Label();
            this.datePicker = new System.Windows.Forms.DateTimePicker();
            this.timePicker = new System.Windows.Forms.DateTimePicker();
            this.time_label = new System.Windows.Forms.Label();
            this.time_zone_label = new System.Windows.Forms.Label();
            this.timeZonePicker = new System.Windows.Forms.ComboBox();
            this.okButton = new System.Windows.Forms.Button();
            this.cancelButton = new System.Windows.Forms.Button();
            this.SuspendLayout();
            // 
            // date_label
            // 
            this.date_label.AutoSize = true;
            this.date_label.Location = new System.Drawing.Point(13, 13);
            this.date_label.Name = "date_label";
            this.date_label.Size = new System.Drawing.Size(36, 13);
            this.date_label.TabIndex = 0;
            this.date_label.Text = "Date :";
            // 
            // datePicker
            // 
            this.datePicker.Format = System.Windows.Forms.DateTimePickerFormat.Short;
            this.datePicker.Location = new System.Drawing.Point(55, 9);
            this.datePicker.Name = "datePicker";
            this.datePicker.Size = new System.Drawing.Size(143, 20);
            this.datePicker.TabIndex = 1;
            // 
            // timePicker
            // 
            this.timePicker.Format = System.Windows.Forms.DateTimePickerFormat.Time;
            this.timePicker.Location = new System.Drawing.Point(55, 47);
            this.timePicker.Name = "timePicker";
            this.timePicker.Size = new System.Drawing.Size(143, 20);
            this.timePicker.TabIndex = 2;
            // 
            // time_label
            // 
            this.time_label.AutoSize = true;
            this.time_label.Location = new System.Drawing.Point(13, 54);
            this.time_label.Name = "time_label";
            this.time_label.Size = new System.Drawing.Size(36, 13);
            this.time_label.TabIndex = 3;
            this.time_label.Text = "Time :";
            // 
            // time_zone_label
            // 
            this.time_zone_label.AutoSize = true;
            this.time_zone_label.Location = new System.Drawing.Point(13, 83);
            this.time_zone_label.Name = "time_zone_label";
            this.time_zone_label.Size = new System.Drawing.Size(64, 13);
            this.time_zone_label.TabIndex = 4;
            this.time_zone_label.Text = "Time Zone :";
            // 
            // timeZonePicker
            // 
            this.timeZonePicker.FormattingEnabled = true;
            this.timeZonePicker.Location = new System.Drawing.Point(16, 109);
            this.timeZonePicker.Name = "timeZonePicker";
            this.timeZonePicker.Size = new System.Drawing.Size(182, 21);
            this.timeZonePicker.TabIndex = 5;
            // 
            // okButton
            // 
            this.okButton.Location = new System.Drawing.Point(16, 161);
            this.okButton.Name = "okButton";
            this.okButton.Size = new System.Drawing.Size(75, 23);
            this.okButton.TabIndex = 6;
            this.okButton.Text = "Ok";
            this.okButton.UseVisualStyleBackColor = true;
            // 
            // cancelButton
            // 
            this.cancelButton.Location = new System.Drawing.Point(123, 161);
            this.cancelButton.Name = "cancelButton";
            this.cancelButton.Size = new System.Drawing.Size(75, 23);
            this.cancelButton.TabIndex = 7;
            this.cancelButton.Text = "Cancel";
            this.cancelButton.UseVisualStyleBackColor = true;
            // 
            // SetDateDlg
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(213, 194);
            this.ControlBox = false;
            this.Controls.Add(this.cancelButton);
            this.Controls.Add(this.okButton);
            this.Controls.Add(this.timeZonePicker);
            this.Controls.Add(this.time_zone_label);
            this.Controls.Add(this.time_label);
            this.Controls.Add(this.timePicker);
            this.Controls.Add(this.datePicker);
            this.Controls.Add(this.date_label);
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedDialog;
            this.Name = "SetDateDlg";
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterParent;
            this.Text = "iBrand(tm)";
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private Label date_label;
        private DateTimePicker datePicker;
        private DateTimePicker timePicker;
        private Label time_label;
        private Label time_zone_label;
        private ComboBox timeZonePicker;
        private Button okButton;
        private Button cancelButton;
    }
}