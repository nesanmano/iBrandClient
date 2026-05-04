using System.ComponentModel;
using System.Windows.Forms;

namespace NomadLogic.iBrandDesktop.Ui
{
    partial class DeleteProjectsDlg
    {
        /// <summary>
        /// Required designer variable.
        /// </summary>
        private IContainer components = null;
        private MainMenu mainMenu;

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
            this.components = new System.ComponentModel.Container();
            this.mainMenu = new System.Windows.Forms.MainMenu(this.components);
            this.ProjectListView = new System.Windows.Forms.ListView();
            this.SuspendLayout();
            // 
            // project_list_view
            // 
            this.ProjectListView.CheckBoxes = true;
            this.ProjectListView.HeaderStyle = System.Windows.Forms.ColumnHeaderStyle.Nonclickable;
            this.ProjectListView.Location = new System.Drawing.Point(4, 4);
            this.ProjectListView.Name = "ProjectListView";
            this.ProjectListView.Size = new System.Drawing.Size(233, 229);
            this.ProjectListView.TabIndex = 0;
            this.ProjectListView.UseCompatibleStateImageBehavior = false;
            // 
            // DeleteProjectsDlg
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(96F, 96F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Dpi;
            this.AutoScroll = true;
            this.ClientSize = new System.Drawing.Size(240, 268);
            this.ControlBox = false;
            this.Controls.Add(this.ProjectListView);
            this.Menu = this.mainMenu;
            this.Name = "DeleteProjectsDlg";
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterParent;
            this.Text = "iBrand(tm)";
            this.ResumeLayout(false);

        }

        #endregion
    }
}