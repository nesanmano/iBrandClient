using System.ComponentModel;
using System.Windows.Forms;
using NomadLogic.Common.Ui;
using NomadLogic.Common.Ui.Dialog;
using NomadLogic.Common.Util;

namespace NomadLogic.iBrandDesktop.Ui
{
    partial class KioskForm
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
            if (this.ActiveQuestionnaire)
            {
                MsgBox.Show(this, Resources.iBrand.UI_ExitFromProjectBeforeClosing, Resources.iBrand.AppName, 
                    MessageBoxButtons.OK, MessageBoxIcon.Error, MessageBoxDefaultButton.Button1);
                return;
            }

            if (CanonCameraManager.IsInitialized())
            {
                CanonCameraManager.GetInstance().ShutDown();
            }


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
            this.components = new System.ComponentModel.Container();
            var resources = new System.ComponentModel.ComponentResourceManager(typeof(KioskForm));
            this.terminalsLauncherItemsList = new ListView();
            this.launcherImages = new ImageList(this.components);
            this.menuStrip = new Vs2008MenuStrip();
            this.statusStrip = new VS2008StatusStrip();
            this.SuspendLayout();
            // 
            // projectsLauncherItemsList
            // 
            this.terminalsLauncherItemsList.Activation = System.Windows.Forms.ItemActivation.OneClick;
            this.terminalsLauncherItemsList.Dock = System.Windows.Forms.DockStyle.Fill;
            this.terminalsLauncherItemsList.Location = new System.Drawing.Point(0, 24);
            this.terminalsLauncherItemsList.Margin = new System.Windows.Forms.Padding(5);
            this.terminalsLauncherItemsList.Name = "terminalsLauncherItemsList";
            this.terminalsLauncherItemsList.Size = new System.Drawing.Size(645, 556);
            this.terminalsLauncherItemsList.TabIndex = 0;
            this.terminalsLauncherItemsList.UseCompatibleStateImageBehavior = false;
            this.terminalsLauncherItemsList.ItemActivate += new System.EventHandler(this.TerminalItemActivate);
            // 
            // launcherImages
            // 
            this.launcherImages.ColorDepth = System.Windows.Forms.ColorDepth.Depth8Bit;
            this.launcherImages.ImageSize = new System.Drawing.Size(16, 16);
            this.launcherImages.TransparentColor = System.Drawing.Color.Transparent;
            // 
            // menuStrip
            // 
            this.menuStrip.BackColor = System.Drawing.SystemColors.Control;
            this.menuStrip.ForeColor = System.Drawing.Color.Black;
            this.menuStrip.Location = new System.Drawing.Point(0, 0);
            this.menuStrip.Name = "menuStrip";
            this.menuStrip.Padding = new System.Windows.Forms.Padding(2, 2, 0, 2);
            this.menuStrip.Size = new System.Drawing.Size(645, 24);
            this.menuStrip.TabIndex = 1;
            this.menuStrip.Text = "menuStrip1";
            // 
            // statusStrip
            // 
            this.statusStrip.ForeColor = System.Drawing.Color.Black;
            this.statusStrip.Location = new System.Drawing.Point(0, 558);
            this.statusStrip.Name = "statusStrip";
            this.statusStrip.Size = new System.Drawing.Size(645, 22);
            this.statusStrip.TabIndex = 2;
            this.statusStrip.Text = "vS2008StatusStrip1";
            // 
            // KioskForm
            // 
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.None;
            this.AutoScroll = false;
            this.BackColor = System.Drawing.SystemColors.Control;
            this.ClientSize = new System.Drawing.Size(645, 580);
            this.Controls.Add(this.statusStrip);
            this.Controls.Add(this.terminalsLauncherItemsList);
            this.Controls.Add(this.menuStrip);
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedSingle;
            this.Icon = ((System.Drawing.Icon)(resources.GetObject("$this.Icon")));
            this.MainMenuStrip = this.menuStrip;
            this.Margin = new System.Windows.Forms.Padding(6);
            this.MaximizeBox = false;
            this.Name = "KioskForm";
            this.Text = "iBrand (tm)";
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private ListView terminalsLauncherItemsList;
        private ImageList launcherImages;
        private VS2008StatusStrip statusStrip;
        private Vs2008MenuStrip menuStrip;
    }
}

