using System.ComponentModel;
using System.Windows.Forms;

namespace NomadLogic.iBrandDesktop.Ui
{
    partial class StartProjectDialog
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
            this.questionnaire_label = new System.Windows.Forms.Label();
            this.questionnaireComboBox = new System.Windows.Forms.ComboBox();
            this.event_label = new System.Windows.Forms.Label();
            this.eventComboBox = new System.Windows.Forms.ComboBox();
            this.userNameTextBox = new System.Windows.Forms.TextBox();
            this.userNameLabel = new System.Windows.Forms.Label();
            this.okButton = new System.Windows.Forms.Button();
            this.cancelButton = new System.Windows.Forms.Button();
            this.SuspendLayout();
            // 
            // questionnaire_label
            // 
            this.questionnaire_label.AutoSize = true;
            this.questionnaire_label.Location = new System.Drawing.Point(10, 13);
            this.questionnaire_label.Name = "questionnaire_label";
            this.questionnaire_label.Size = new System.Drawing.Size(105, 17);
            this.questionnaire_label.TabIndex = 0;
            this.questionnaire_label.Text = "Questionnaire :";
            // 
            // questionnaireComboBox
            // 
            this.questionnaireComboBox.FormattingEnabled = true;
            this.questionnaireComboBox.Location = new System.Drawing.Point(13, 30);
            this.questionnaireComboBox.Name = "questionnaireComboBox";
            this.questionnaireComboBox.Size = new System.Drawing.Size(168, 24);
            this.questionnaireComboBox.TabIndex = 1;
            // 
            // event_label
            // 
            this.event_label.AutoSize = true;
            this.event_label.Location = new System.Drawing.Point(10, 58);
            this.event_label.Name = "event_label";
            this.event_label.Size = new System.Drawing.Size(52, 17);
            this.event_label.TabIndex = 2;
            this.event_label.Text = "Event :";
            // 
            // eventComboBox
            // 
            this.eventComboBox.FormattingEnabled = true;
            this.eventComboBox.Location = new System.Drawing.Point(13, 74);
            this.eventComboBox.Name = "eventComboBox";
            this.eventComboBox.Size = new System.Drawing.Size(165, 24);
            this.eventComboBox.TabIndex = 3;
            // 
            // userNameTextBox
            // 
            this.userNameTextBox.Location = new System.Drawing.Point(13, 124);
            this.userNameTextBox.Name = "userNameTextBox";
            this.userNameTextBox.Size = new System.Drawing.Size(165, 22);
            this.userNameTextBox.TabIndex = 5;
            // 
            // userNameLabel
            // 
            this.userNameLabel.AutoSize = true;
            this.userNameLabel.Location = new System.Drawing.Point(10, 107);
            this.userNameLabel.Name = "userNameLabel";
            this.userNameLabel.Size = new System.Drawing.Size(87, 17);
            this.userNameLabel.TabIndex = 4;
            this.userNameLabel.Text = "User Name :";
            // 
            // okButton
            // 
            this.okButton.Location = new System.Drawing.Point(13, 163);
            this.okButton.Name = "okButton";
            this.okButton.Size = new System.Drawing.Size(75, 23);
            this.okButton.TabIndex = 6;
            this.okButton.Text = "Ok";
            this.okButton.UseVisualStyleBackColor = true;
            // 
            // cancelButton
            // 
            this.cancelButton.Location = new System.Drawing.Point(102, 162);
            this.cancelButton.Name = "cancelButton";
            this.cancelButton.Size = new System.Drawing.Size(75, 23);
            this.cancelButton.TabIndex = 7;
            this.cancelButton.Text = "Cancel";
            this.cancelButton.UseVisualStyleBackColor = true;
            // 
            // StartProjectDialog
            // 
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.None;
            this.ClientSize = new System.Drawing.Size(190, 195);
            this.ControlBox = false;
            this.Controls.Add(this.cancelButton);
            this.Controls.Add(this.okButton);
            this.Controls.Add(this.userNameTextBox);
            this.Controls.Add(this.eventComboBox);
            this.Controls.Add(this.event_label);
            this.Controls.Add(this.questionnaireComboBox);
            this.Controls.Add(this.questionnaire_label);
            this.Controls.Add(this.userNameLabel);
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedDialog;
            this.MaximizeBox = false;
            this.Name = "StartProjectDialog";
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterParent;
            this.Text = "iBrand(tm)";
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private Label questionnaire_label;
        private ComboBox questionnaireComboBox;
        private Label event_label;
        private ComboBox eventComboBox;
        private TextBox userNameTextBox;
        private Label userNameLabel;
        private Button okButton;
        private Button cancelButton;
    }
}