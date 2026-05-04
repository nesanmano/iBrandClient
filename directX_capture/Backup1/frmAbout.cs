// ------------------------------------------------------------------
// RTVCapture
//
// History:
// 2009-Feb-27 HV - created
//
// Copyright (C) 2009 Hans Vosman
// ------------------------------------------------------------------
using System;
using System.Windows.Forms;

public class frmAbout : System.Windows.Forms.Form
{

#region " Windows Form Designer generated code "

	public frmAbout() 
	{
		//This call is required by the Windows Form Designer.
		InitializeComponent();
		//Add any initialization after the InitializeComponent() call
	}

	//Form overrides dispose to clean up the component list.
	protected override void Dispose(bool disposing) {
		if (disposing) {
			if (components != null) {
				components.Dispose();
			}
		}
		base.Dispose(disposing);
	}

	//Required by the Windows Form Designer
	private System.ComponentModel.IContainer components = null;
	//NOTE: The following procedure is required by the Windows Form Designer
	//It can be modified using the Windows Form Designer.  
	//Do not modify it using the code editor.
	private System.Windows.Forms.PictureBox pbIcon;
	private System.Windows.Forms.Label labelTitle;
	private System.Windows.Forms.Label labelVersion;
	private System.Windows.Forms.Label labelDescription;
	private System.Windows.Forms.Label labelCopyright;
	private System.Windows.Forms.Label labelCodebase;
	private System.Windows.Forms.Label labelWindowsVersion;

	private System.Windows.Forms.Button cmdOK;

	private void InitializeComponent() {
		System.Resources.ResourceManager resources = new System.Resources.ResourceManager(typeof(frmAbout));
		this.pbIcon = new System.Windows.Forms.PictureBox();
		this.labelTitle = new System.Windows.Forms.Label();
		this.labelVersion = new System.Windows.Forms.Label();
		this.labelDescription = new System.Windows.Forms.Label();
		this.cmdOK = new System.Windows.Forms.Button();
		this.labelCopyright = new System.Windows.Forms.Label();
		this.labelCodebase = new System.Windows.Forms.Label();
		this.labelWindowsVersion = new System.Windows.Forms.Label();
		this.SuspendLayout();
		// 
		// pbIcon
		// 
		this.pbIcon.BorderStyle = System.Windows.Forms.BorderStyle.Fixed3D;
		this.pbIcon.Image = ((System.Drawing.Image)(resources.GetObject("pbIcon.Image")));
		this.pbIcon.ImeMode = System.Windows.Forms.ImeMode.NoControl;
		this.pbIcon.Location = new System.Drawing.Point(8, 16);
		this.pbIcon.Name = "pbIcon";
		this.pbIcon.Size = new System.Drawing.Size(48, 48);
		this.pbIcon.SizeMode = System.Windows.Forms.PictureBoxSizeMode.StretchImage;
		this.pbIcon.TabIndex = 0;
		this.pbIcon.TabStop = false;
		this.pbIcon.Click += new System.EventHandler(this.pbIcon_Click);
		// 
		// labelTitle
		// 
		this.labelTitle.ImeMode = System.Windows.Forms.ImeMode.NoControl;
		this.labelTitle.Location = new System.Drawing.Point(72, 16);
		this.labelTitle.Name = "labelTitle";
		this.labelTitle.Size = new System.Drawing.Size(360, 16);
		this.labelTitle.TabIndex = 1;
		this.labelTitle.Text = "TV/Video capture code example";
		// 
		// labelVersion
		// 
		this.labelVersion.ImeMode = System.Windows.Forms.ImeMode.NoControl;
		this.labelVersion.Location = new System.Drawing.Point(72, 40);
		this.labelVersion.Name = "labelVersion";
		this.labelVersion.Size = new System.Drawing.Size(360, 16);
		this.labelVersion.TabIndex = 2;
		this.labelVersion.Text = "Version 1.5";
		// 
		// labelDescription
		// 
		this.labelDescription.ImeMode = System.Windows.Forms.ImeMode.NoControl;
		this.labelDescription.Location = new System.Drawing.Point(72, 80);
		this.labelDescription.Name = "labelDescription";
		this.labelDescription.Size = new System.Drawing.Size(360, 40);
		this.labelDescription.TabIndex = 3;
		this.labelDescription.Text = "The TV/Video capture code example, originally developed for Hauppauge Amity2 TV-c" +
			"ard. The program might be useable for other TV-cards.  Additional information ca" +
			"n be found at www.pcpret.nl ";
		// 
		// cmdOK
		// 
		this.cmdOK.DialogResult = System.Windows.Forms.DialogResult.OK;
		this.cmdOK.ImeMode = System.Windows.Forms.ImeMode.NoControl;
		this.cmdOK.Location = new System.Drawing.Point(352, 200);
		this.cmdOK.Name = "cmdOK";
		this.cmdOK.TabIndex = 4;
		this.cmdOK.Text = "OK";
		// 
		// labelCopyright
		// 
		this.labelCopyright.ImeMode = System.Windows.Forms.ImeMode.NoControl;
		this.labelCopyright.Location = new System.Drawing.Point(72, 56);
		this.labelCopyright.Name = "labelCopyright";
		this.labelCopyright.Size = new System.Drawing.Size(360, 16);
		this.labelCopyright.TabIndex = 5;
		this.labelCopyright.Text = "Copyright (C) 2005, 2006, 2007, 2008, 2009 by H.Vosman";
		// 
		// labelCodebase
		// 
		this.labelCodebase.ImeMode = System.Windows.Forms.ImeMode.NoControl;
		this.labelCodebase.Location = new System.Drawing.Point(72, 136);
		this.labelCodebase.Name = "labelCodebase";
		this.labelCodebase.Size = new System.Drawing.Size(360, 48);
		this.labelCodebase.TabIndex = 6;
		this.labelCodebase.Text = "The TV/Video capture example makes use of the DirectShow C# example DirextX.Captu" +
			"re written by Brian Low. Additional information on these code examples can be fo" +
			"und at the www.thecodeproject.com";
		// 
		// labelWindowsVersion
		// 
		this.labelWindowsVersion.Location = new System.Drawing.Point(72, 200);
		this.labelWindowsVersion.Name = "labelWindowsVersion";
		this.labelWindowsVersion.Size = new System.Drawing.Size(264, 16);
		this.labelWindowsVersion.TabIndex = 7;
		this.labelWindowsVersion.Text = "Windows version";
		// 
		// frmAbout
		// 
		this.AutoScaleBaseSize = new System.Drawing.Size(5, 13);
		this.ClientSize = new System.Drawing.Size(440, 232);
		this.Controls.Add(this.labelWindowsVersion);
		this.Controls.Add(this.labelCodebase);
		this.Controls.Add(this.labelCopyright);
		this.Controls.Add(this.cmdOK);
		this.Controls.Add(this.labelDescription);
		this.Controls.Add(this.labelVersion);
		this.Controls.Add(this.labelTitle);
		this.Controls.Add(this.pbIcon);
		this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedDialog;
		this.Icon = ((System.Drawing.Icon)(resources.GetObject("$this.Icon")));
		this.MaximizeBox = false;
		this.MinimizeBox = false;
		this.Name = "frmAbout";
		this.ShowInTaskbar = false;
		this.StartPosition = System.Windows.Forms.FormStartPosition.CenterParent;
		this.Text = "About ...";
		this.Load += new System.EventHandler(this.frmAbout_Load);
		this.ResumeLayout(false);

	}

#endregion

	// Note: Because this form is opened by frmMain using the ShowDialog command, we simply set the
	// DialogResult property of cmdOK to OK which causes the form to close when clicked.
	private void frmAbout_Load(object sender, System.EventArgs e) {
		try {
			// Set this Form's Text + Icon properties by using values from the parent form
			this.Text = "About " + this.Owner.Text;
			this.Icon = this.Owner.Icon;
			// Set this Form's Picture Box's image using the parent's icon 
			// However, we need to convert it to a Bitmap since the Picture Box Control
			// will not accept a raw Icon.
			this.pbIcon.Image = this.Owner.Icon.ToBitmap();

			OperatingSystem opSys   = new OperatingSystem(
				Environment.OSVersion.Platform,
				Environment.OSVersion.Version);
			PlatformID platform     = opSys.Platform;
			Version version         = opSys.Version;
			this.labelWindowsVersion.Text = "Platform: " + platform.ToString() + " (version: " + version.ToString() + ")";
		}
		catch(System.Exception exp) {
			// This catch will trap any unexpected error.
			MessageBox.Show(exp.Message, this.Text, MessageBoxButtons.OK, MessageBoxIcon.Stop);
		}
	}

	private void pbIcon_Click(object sender, System.EventArgs e)
	{
	
	}
}

