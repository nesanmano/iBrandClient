// ------------------------------------------------------------------
// CaptureTest.cs
// Sample application to show the DirectX.Capture class library.
//
// History:
//	2003-Jan-25		BL		- created
//
// Copyright (c) 2003 Brian Low
//
//	2006-Mar-15		HV		- modified
// New:
//	- audio file saving in Windows Media Audio (Wma) format
//	- audio rendering for TV-cards which have no wired audio
//	  connection, but have an audio driver
//
//  2006-Apr-1      HV      - modified
// New:
//  - initialize audio source when there is audio but no selected source
//  - restore audio when audio gets lost for what ever reason
//	- video file saving in Windows Media Video (Wmv) format
//
//  2007-Jan-10     HV      - modified, added
// New:
//	- audio rendering for TV-cards which have no wired audio
//	  connection and have no audio driver.
//	- TV frequency fine tuning
//  - Added TV broadcast station selection
//
//  2007-Jun-1      HV       - modified, added
// New:
//  - Improved audio rendering for TV-cards having no wired audio
//  - Added SampleGrabber using HeFrame event (useful for grabbing all frames)
//  - Added FMRadio option
//  - Added VMR9 option
//  - Added De-Interlace option (using Alpary Deinterlace Filter)
//  - DirectShowLib/DShowNET compatibility added (via conditional DSHOWNET)
//  - Visual Studio 2003/2005 compatibility added (via conditional VS2003)
//  - Improved exception handling upon loss of Audio or Video sources and
//    property page information
//
// Note that the Audio via PCI, SampleGrabber, VMR9 and the de-interlace
// option values becomes active upon (re)selection of the video device.
// The default values are OFF. This is because some capture cards fail if
// an option has a value ON.
//
// This software has been tested with the following TV or video capture cards:
// Hauppauge Amity2, Hauppauge PVR150 MCE, Pinnacle PCTV 330eV, MX460 Vivo
//
// The VS2003 and VS2005 designer code might differ slightly. As result the
// file CaptureTest.cs can not be exchanged without changes between these
// visual studio versions.
// The signal this.Closed (VS2003) and this.FormClosed (VS2005) is such
// difference. No signal is attached, advise is to use CaptureTest_FormClosed
// as signal.
//
//   2009-Feb-27    HV       - modified
// New:
//  - Add color space (+ quick scan of available color spaces) selection
//    This color space implementation is partly based on Brian Low's
//    DirectX.Capture example.
//  - Add video standard selection (eg PAL_B)
//  - Use profile for Windows Media audio/video formats, profile should
//    be in the program directory.
//  - Add TV tuner initialization window to force user to provide input
//    on country dependent settings (without proper settings code may fail)
//  - Removed FM Radio, TV finetuning and SampleGrabber code to make code
//    example easier to understand for video file saving.
//
// Copyright (C) 2006, 2007, 2008, 2009 Hans Vosman
// ------------------------------------------------------------------

using System;
using System.Diagnostics; 
using System.Drawing;
using System.Windows.Forms;
using DirectX.Capture;
#if DSHOWNET
using DShowNET; // DsEvCode
#else
using DirectShowLib;
#endif
using System.Runtime.InteropServices; // Marshal.Free ...

namespace CaptureTest
{
	public class CaptureTest : System.Windows.Forms.Form
	{
		const int WM_GRAPHNOTIFY = 0x8000 + 1;
		private IMediaEventEx mediaEvent = null;
		private int CaptureResult;
		private Capture capture = null;
		private Filters filters = new Filters();

		private System.Windows.Forms.TextBox txtFilename;
		private System.Windows.Forms.Label label1;
		private System.Windows.Forms.Button btnStart;
		private System.Windows.Forms.Button btnStop;
		private System.Windows.Forms.Button btnExit;
		private System.Windows.Forms.MenuItem menuItem1;
		private System.Windows.Forms.MenuItem menuItem7;
		private System.Windows.Forms.MainMenu mainMenu;
		private System.Windows.Forms.MenuItem mnuExit;
		private System.Windows.Forms.MenuItem mnuDevices;
		private System.Windows.Forms.MenuItem mnuVideoDevices;
		private System.Windows.Forms.MenuItem mnuAudioDevices;
		private System.Windows.Forms.MenuItem mnuVideoCompressors;
		private System.Windows.Forms.MenuItem mnuAudioCompressors;
		private System.Windows.Forms.MenuItem mnuVideoSources;
		private System.Windows.Forms.MenuItem mnuAudioSources;
		private System.Windows.Forms.Panel panelVideo;
		private System.Windows.Forms.MenuItem menuItem4;
		private System.Windows.Forms.MenuItem mnuAudioChannels;
		private System.Windows.Forms.MenuItem mnuAudioSamplingRate;
		private System.Windows.Forms.MenuItem mnuAudioSampleSizes;
		private System.Windows.Forms.MenuItem menuItem5;
		private System.Windows.Forms.Button btnCue;
		private System.Windows.Forms.MenuItem menuItem6;
		private System.Windows.Forms.MenuItem mnuPreview;
		private System.Windows.Forms.MenuItem menuItem8;
		private System.Windows.Forms.MenuItem mnuPropertyPages;
		private System.Windows.Forms.MenuItem mnuAudioCaps;
		private System.Windows.Forms.MenuItem mnuChannel;
		private System.Windows.Forms.MenuItem menuItem3;
		private System.Windows.Forms.MenuItem mnuInputType;
		private System.Windows.Forms.MenuItem menuAudioViaPci1;
		private System.Windows.Forms.MenuItem menuItem2;
		private System.Windows.Forms.MenuItem menuItem9;
		private System.Windows.Forms.MenuItem menuAVRecFileModes;
		private System.Windows.Forms.MenuItem menuAsfFileFormat;
        private System.ComponentModel.IContainer components;
		private System.Windows.Forms.Label label3;
		private TVSelections tvSelections = new TVSelections();
		private System.Windows.Forms.PictureBox pictureBox1;
		private System.Windows.Forms.SaveFileDialog saveFileDialog1;
		private System.Windows.Forms.MenuItem menuTunerModes1;
		private AMTunerModeType tunerModeType = AMTunerModeType.TV;
		private int DefaultChannel = 88;
		private int DefaultCountryCode = 31;
		private int DefaultTuningSpace = 0;
		private bool noChannelChange = false;
		private System.Windows.Forms.MenuItem menuUseVMR9;
        private TunerInputType tunerInputType = TunerInputType.Cable;
		private System.Windows.Forms.MenuItem menuUseDeInterlace1;
		private System.Windows.Forms.MenuItem menuVideoRenderer1;
		private System.Windows.Forms.MenuItem menuDeInterlaceFilter1;
		private System.Windows.Forms.MenuItem menuVideoStandard1;
		private System.Windows.Forms.MenuItem menuVidCapSettings1;
		private System.Windows.Forms.MenuItem mnuFrameSizes;
		private System.Windows.Forms.MenuItem mnuFrameRates;
		private System.Windows.Forms.MenuItem menuColorSpace1;
		private System.Windows.Forms.MenuItem mnuVideoCaps;
		private System.Windows.Forms.MenuItem menuVidPrevSettings1;
		private System.Windows.Forms.MenuItem menuPreviewCaps;
		private System.Windows.Forms.MenuItem menuHelp1;
		private System.Windows.Forms.MenuItem menuInfo1;
		private System.Windows.Forms.NumericUpDown channelUpDown1;
		private System.Windows.Forms.Label labelTvChannel1;

        private AMTunerModeType TunerModeType
		{
			get { return this.tunerModeType; }
			set
			{
				this.tunerModeType = value;
				if((this.capture != null)&&(this.capture.Tuner != null))
				{
					this.capture.Tuner.AudioMode = value;
					this.capture.Tuner.InputType = this.tunerInputType;
					this.capture.Tuner.TuningSpace = this.DefaultTuningSpace;
					this.capture.Tuner.CountryCode = this.DefaultCountryCode;
				}

				this.capture.Tuner.Channel = this.DefaultChannel;
			}
		}

        TunerInputType TunerInputType
		{
			get { return this.tunerInputType; }
			set
			{
				this.tunerInputType = value;
				this.capture.Tuner.InputType = value;
			}
		}

		// Option to switch between audio via pci or wired audio connection
		// Default value is false (wired audio connection).
		private bool audioViaPci = false;

		/// <summary>
		/// Audio captured via Capture card in a PCI bus
		/// </summary>
		public bool AudioViaPci
		{
			get { return audioViaPci; }
			set
			{
				audioViaPci = value;
				menuAudioViaPci1.Checked = audioViaPci;
			}
		}

		// Media events are sent to use as windows messages
		protected override void WndProc(ref Message m)
		{
			switch (m.Msg)
			{
				// If this is a windows media message
				case WM_GRAPHNOTIFY:
#if DSHOWNET
					DsEvCode eventCode;
#else
					EventCode eventCode;
#endif
					int p1, p2, hr;

					hr = mediaEvent.GetEvent(out eventCode, out p1, out p2, 0);
					while (hr == 0)
					{
						// Handle the event.
						switch(eventCode)
						{
#if DSHOWNET
							case DsEvCode.ErrorAbort:
#else
							case EventCode.ErrorAbort:
#endif
								// The capture has been aborted
								CaptureResult = p1;
								break;
#if DSHOWNET
							case DsEvCode.FullScreenLost:
#else
							case EventCode.FullScreenLost:
#endif
								break;
							default:
								break;
						}

#if DEBUG
						Debug.WriteLine("Media Event " + eventCode.ToString() + " received ...");
#endif
						// Release parms
						mediaEvent.FreeEventParams(eventCode, p1, p2);

						// check for additional events
						hr = mediaEvent.GetEvent(out eventCode, out p1, out p2, 0);
					}
					break;

					// All other messages
				default:
					try
					{
						// unhandled window message
						base.WndProc(ref m);
					}
					catch
					{
						Debug.WriteLine("Fatal exception catching a message with WndProc()");
					}
					break;
			}
		}

		public CaptureTest()
		{
			//
			// Required for Windows Form Designer support
			//
			InitializeComponent();

			// Start with the first video/audio devices
			// Don't do this in the Release build in case the
			// first devices cause problems.
			this.AudioViaPci = this.menuAudioViaPci1.Checked;

			// Update the main menu
			// Much of the interesting work of this sample occurs here
			try
            {
                initMenu();
                updateMenu();
            } 
			catch {}
		}

		/// <summary>
		/// Clean up any resources being used.
		/// </summary>
		protected override void Dispose( bool disposing )
		{
			if( disposing )
			{
				if(components != null)
				{
					components.Dispose();
				}
			}
			base.Dispose( disposing );
		}

		// There is a difference between VS2003 and VS2005
		// in handling e.g. the main window closure.
		// The corresponding signal should be set to: CaptureTest_FormClosed
		// In VS2003 the designer code uses this.Closed
		// In VS2005 the designer code uses this.FormClosed
		// This causes compatibity problems, just keep that in mind!

		#region Windows Form Designer generated code
		/// <summary>
		/// Required method for Designer support - do not modify
		/// the contents of this method with the code editor.
		/// </summary>
		private void InitializeComponent()
		{
			System.Resources.ResourceManager resources = new System.Resources.ResourceManager(typeof(CaptureTest));
			this.txtFilename = new System.Windows.Forms.TextBox();
			this.label1 = new System.Windows.Forms.Label();
			this.btnStart = new System.Windows.Forms.Button();
			this.btnStop = new System.Windows.Forms.Button();
			this.btnExit = new System.Windows.Forms.Button();
			this.mainMenu = new System.Windows.Forms.MainMenu();
			this.menuItem1 = new System.Windows.Forms.MenuItem();
			this.menuAVRecFileModes = new System.Windows.Forms.MenuItem();
			this.menuAsfFileFormat = new System.Windows.Forms.MenuItem();
			this.menuItem9 = new System.Windows.Forms.MenuItem();
			this.mnuExit = new System.Windows.Forms.MenuItem();
			this.mnuDevices = new System.Windows.Forms.MenuItem();
			this.mnuVideoDevices = new System.Windows.Forms.MenuItem();
			this.mnuAudioDevices = new System.Windows.Forms.MenuItem();
			this.menuItem4 = new System.Windows.Forms.MenuItem();
			this.mnuVideoCompressors = new System.Windows.Forms.MenuItem();
			this.mnuAudioCompressors = new System.Windows.Forms.MenuItem();
			this.menuItem2 = new System.Windows.Forms.MenuItem();
			this.menuAudioViaPci1 = new System.Windows.Forms.MenuItem();
			this.menuUseVMR9 = new System.Windows.Forms.MenuItem();
			this.menuUseDeInterlace1 = new System.Windows.Forms.MenuItem();
			this.menuItem7 = new System.Windows.Forms.MenuItem();
			this.mnuPreview = new System.Windows.Forms.MenuItem();
			this.mnuChannel = new System.Windows.Forms.MenuItem();
			this.menuItem8 = new System.Windows.Forms.MenuItem();
			this.mnuVideoSources = new System.Windows.Forms.MenuItem();
			this.menuVideoStandard1 = new System.Windows.Forms.MenuItem();
			this.menuVidCapSettings1 = new System.Windows.Forms.MenuItem();
			this.mnuFrameSizes = new System.Windows.Forms.MenuItem();
			this.mnuFrameRates = new System.Windows.Forms.MenuItem();
			this.menuColorSpace1 = new System.Windows.Forms.MenuItem();
			this.mnuVideoCaps = new System.Windows.Forms.MenuItem();
			this.menuVidPrevSettings1 = new System.Windows.Forms.MenuItem();
			this.menuPreviewCaps = new System.Windows.Forms.MenuItem();
			this.menuItem5 = new System.Windows.Forms.MenuItem();
			this.mnuAudioSources = new System.Windows.Forms.MenuItem();
			this.mnuAudioChannels = new System.Windows.Forms.MenuItem();
			this.mnuAudioSamplingRate = new System.Windows.Forms.MenuItem();
			this.mnuAudioSampleSizes = new System.Windows.Forms.MenuItem();
			this.mnuAudioCaps = new System.Windows.Forms.MenuItem();
			this.menuItem3 = new System.Windows.Forms.MenuItem();
			this.mnuInputType = new System.Windows.Forms.MenuItem();
			this.menuTunerModes1 = new System.Windows.Forms.MenuItem();
			this.menuItem6 = new System.Windows.Forms.MenuItem();
			this.mnuPropertyPages = new System.Windows.Forms.MenuItem();
			this.menuVideoRenderer1 = new System.Windows.Forms.MenuItem();
			this.menuDeInterlaceFilter1 = new System.Windows.Forms.MenuItem();
			this.menuHelp1 = new System.Windows.Forms.MenuItem();
			this.menuInfo1 = new System.Windows.Forms.MenuItem();
			this.panelVideo = new System.Windows.Forms.Panel();
			this.pictureBox1 = new System.Windows.Forms.PictureBox();
			this.btnCue = new System.Windows.Forms.Button();
			this.label3 = new System.Windows.Forms.Label();
			this.saveFileDialog1 = new System.Windows.Forms.SaveFileDialog();
			this.channelUpDown1 = new System.Windows.Forms.NumericUpDown();
			this.labelTvChannel1 = new System.Windows.Forms.Label();
			((System.ComponentModel.ISupportInitialize)(this.channelUpDown1)).BeginInit();
			this.SuspendLayout();
			// 
			// txtFilename
			// 
			this.txtFilename.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
			this.txtFilename.Location = new System.Drawing.Point(72, 320);
			this.txtFilename.Name = "txtFilename";
			this.txtFilename.Size = new System.Drawing.Size(208, 20);
			this.txtFilename.TabIndex = 0;
			this.txtFilename.Text = "c:\\test.wmv";
			this.txtFilename.KeyDown += new System.Windows.Forms.KeyEventHandler(this.txtFilename_KeyDown);
			this.txtFilename.TextChanged += new System.EventHandler(this.txtFilename_TextChanged);
			this.txtFilename.DoubleClick += new System.EventHandler(this.txtFilename_DoubleClick);
			// 
			// label1
			// 
			this.label1.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
			this.label1.Location = new System.Drawing.Point(8, 320);
			this.label1.Name = "label1";
			this.label1.Size = new System.Drawing.Size(64, 16);
			this.label1.TabIndex = 1;
			this.label1.Text = "Filename:";
			// 
			// btnStart
			// 
			this.btnStart.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
			this.btnStart.Location = new System.Drawing.Point(104, 352);
			this.btnStart.Name = "btnStart";
			this.btnStart.Size = new System.Drawing.Size(80, 24);
			this.btnStart.TabIndex = 2;
			this.btnStart.Text = "Start";
			this.btnStart.Click += new System.EventHandler(this.btnStart_Click);
			// 
			// btnStop
			// 
			this.btnStop.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
			this.btnStop.Enabled = false;
			this.btnStop.Location = new System.Drawing.Point(200, 352);
			this.btnStop.Name = "btnStop";
			this.btnStop.Size = new System.Drawing.Size(80, 24);
			this.btnStop.TabIndex = 3;
			this.btnStop.Text = "Stop";
			this.btnStop.Click += new System.EventHandler(this.btnStop_Click);
			// 
			// btnExit
			// 
			this.btnExit.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
			this.btnExit.Location = new System.Drawing.Point(392, 352);
			this.btnExit.Name = "btnExit";
			this.btnExit.Size = new System.Drawing.Size(96, 24);
			this.btnExit.TabIndex = 5;
			this.btnExit.Text = "Exit";
			this.btnExit.Click += new System.EventHandler(this.btnExit_Click);
			// 
			// mainMenu
			// 
			this.mainMenu.MenuItems.AddRange(new System.Windows.Forms.MenuItem[] {
																					 this.menuItem1,
																					 this.mnuDevices,
																					 this.menuItem7,
																					 this.menuHelp1});
			// 
			// menuItem1
			// 
			this.menuItem1.Index = 0;
			this.menuItem1.MenuItems.AddRange(new System.Windows.Forms.MenuItem[] {
																					  this.menuAVRecFileModes,
																					  this.menuAsfFileFormat,
																					  this.menuItem9,
																					  this.mnuExit});
			this.menuItem1.Text = "File";
			// 
			// menuAVRecFileModes
			// 
			this.menuAVRecFileModes.Index = 0;
			this.menuAVRecFileModes.Text = "File recording &mode";
			// 
			// menuAsfFileFormat
			// 
			this.menuAsfFileFormat.Index = 1;
			this.menuAsfFileFormat.Text = "File audio/video format";
			this.menuAsfFileFormat.Click += new System.EventHandler(this.menuAsfFileFormat_Click);
			// 
			// menuItem9
			// 
			this.menuItem9.Index = 2;
			this.menuItem9.Text = "-";
			// 
			// mnuExit
			// 
			this.mnuExit.Index = 3;
			this.mnuExit.Text = "E&xit";
			this.mnuExit.Click += new System.EventHandler(this.mnuExit_Click);
			// 
			// mnuDevices
			// 
			this.mnuDevices.Index = 1;
			this.mnuDevices.MenuItems.AddRange(new System.Windows.Forms.MenuItem[] {
																					   this.mnuVideoDevices,
																					   this.mnuAudioDevices,
																					   this.menuItem4,
																					   this.mnuVideoCompressors,
																					   this.mnuAudioCompressors,
																					   this.menuItem2,
																					   this.menuAudioViaPci1,
																					   this.menuUseVMR9,
																					   this.menuUseDeInterlace1});
			this.mnuDevices.Text = "Devices";
			// 
			// mnuVideoDevices
			// 
			this.mnuVideoDevices.Index = 0;
			this.mnuVideoDevices.Text = "Video Devices";
			// 
			// mnuAudioDevices
			// 
			this.mnuAudioDevices.Index = 1;
			this.mnuAudioDevices.Text = "Audio Devices";
			// 
			// menuItem4
			// 
			this.menuItem4.Index = 2;
			this.menuItem4.Text = "-";
			// 
			// mnuVideoCompressors
			// 
			this.mnuVideoCompressors.Index = 3;
			this.mnuVideoCompressors.Text = "Video Compressors";
			// 
			// mnuAudioCompressors
			// 
			this.mnuAudioCompressors.Index = 4;
			this.mnuAudioCompressors.Text = "Audio Compressors";
			// 
			// menuItem2
			// 
			this.menuItem2.Index = 5;
			this.menuItem2.Text = "-";
			// 
			// menuAudioViaPci1
			// 
			this.menuAudioViaPci1.Index = 6;
			this.menuAudioViaPci1.Text = "Audio via Pci";
			this.menuAudioViaPci1.Click += new System.EventHandler(this.menuAudioViaPci1_Click);
			// 
			// menuUseVMR9
			// 
			this.menuUseVMR9.Index = 7;
			this.menuUseVMR9.Text = "Use VMR9";
			this.menuUseVMR9.Click += new System.EventHandler(this.menuUseVMR9_Click);
			// 
			// menuUseDeInterlace1
			// 
			this.menuUseDeInterlace1.Index = 8;
			this.menuUseDeInterlace1.Text = "Use DeInterlace";
			this.menuUseDeInterlace1.Click += new System.EventHandler(this.menuUseDeInterlace1_Click);
			// 
			// menuItem7
			// 
			this.menuItem7.Index = 2;
			this.menuItem7.MenuItems.AddRange(new System.Windows.Forms.MenuItem[] {
																					  this.mnuPreview,
																					  this.mnuChannel,
																					  this.menuItem8,
																					  this.mnuVideoSources,
																					  this.menuVideoStandard1,
																					  this.menuVidCapSettings1,
																					  this.menuVidPrevSettings1,
																					  this.menuItem5,
																					  this.mnuAudioSources,
																					  this.mnuAudioChannels,
																					  this.mnuAudioSamplingRate,
																					  this.mnuAudioSampleSizes,
																					  this.mnuAudioCaps,
																					  this.menuItem3,
																					  this.mnuInputType,
																					  this.menuTunerModes1,
																					  this.menuItem6,
																					  this.mnuPropertyPages,
																					  this.menuVideoRenderer1,
																					  this.menuDeInterlaceFilter1});
			this.menuItem7.Text = "Options";
			// 
			// mnuPreview
			// 
			this.mnuPreview.Index = 0;
			this.mnuPreview.Text = "Preview";
			this.mnuPreview.Click += new System.EventHandler(this.mnuPreview_Click);
			// 
			// mnuChannel
			// 
			this.mnuChannel.Index = 1;
			this.mnuChannel.Text = "TV Tuner Channel";
			// 
			// menuItem8
			// 
			this.menuItem8.Index = 2;
			this.menuItem8.Text = "-";
			// 
			// mnuVideoSources
			// 
			this.mnuVideoSources.Index = 3;
			this.mnuVideoSources.Text = "Video Sources";
			// 
			// menuVideoStandard1
			// 
			this.menuVideoStandard1.Index = 4;
			this.menuVideoStandard1.Text = "Video Standard";
			// 
			// menuVidCapSettings1
			// 
			this.menuVidCapSettings1.Index = 5;
			this.menuVidCapSettings1.MenuItems.AddRange(new System.Windows.Forms.MenuItem[] {
																								this.mnuFrameSizes,
																								this.mnuFrameRates,
																								this.menuColorSpace1,
																								this.mnuVideoCaps});
			this.menuVidCapSettings1.Text = "Video Capture Settings";
			this.menuVidCapSettings1.Click += new System.EventHandler(this.mnuVideoCaps_Click);
			// 
			// mnuFrameSizes
			// 
			this.mnuFrameSizes.Index = 0;
			this.mnuFrameSizes.Text = "Video Frame Size";
			// 
			// mnuFrameRates
			// 
			this.mnuFrameRates.Index = 1;
			this.mnuFrameRates.Text = "Video Frame Rate";
			// 
			// menuColorSpace1
			// 
			this.menuColorSpace1.Index = 2;
			this.menuColorSpace1.Text = "Color Space";
			// 
			// mnuVideoCaps
			// 
			this.mnuVideoCaps.Index = 3;
			this.mnuVideoCaps.Text = "Video Capabilities...";
			this.mnuVideoCaps.Click += new System.EventHandler(this.mnuVideoCaps_Click);
			// 
			// menuVidPrevSettings1
			// 
			this.menuVidPrevSettings1.Index = 6;
			this.menuVidPrevSettings1.MenuItems.AddRange(new System.Windows.Forms.MenuItem[] {
																								 this.menuPreviewCaps});
			this.menuVidPrevSettings1.Text = "Video Preview Settings";
			// 
			// menuPreviewCaps
			// 
			this.menuPreviewCaps.Index = 0;
			this.menuPreviewCaps.Text = "Video Capabilities";
			this.menuPreviewCaps.Click += new System.EventHandler(this.menuPreviewCaps_Click);
			// 
			// menuItem5
			// 
			this.menuItem5.Index = 7;
			this.menuItem5.Text = "-";
			// 
			// mnuAudioSources
			// 
			this.mnuAudioSources.Index = 8;
			this.mnuAudioSources.Text = "Audio Sources";
			// 
			// mnuAudioChannels
			// 
			this.mnuAudioChannels.Index = 9;
			this.mnuAudioChannels.Text = "Audio Channels";
			// 
			// mnuAudioSamplingRate
			// 
			this.mnuAudioSamplingRate.Index = 10;
			this.mnuAudioSamplingRate.Text = "Audio Sampling Rate";
			// 
			// mnuAudioSampleSizes
			// 
			this.mnuAudioSampleSizes.Index = 11;
			this.mnuAudioSampleSizes.Text = "Audio Sample Size";
			// 
			// mnuAudioCaps
			// 
			this.mnuAudioCaps.Index = 12;
			this.mnuAudioCaps.Text = "Audio Capabilities...";
			this.mnuAudioCaps.Click += new System.EventHandler(this.mnuAudioCaps_Click);
			// 
			// menuItem3
			// 
			this.menuItem3.Index = 13;
			this.menuItem3.Text = "-";
			// 
			// mnuInputType
			// 
			this.mnuInputType.Index = 14;
			this.mnuInputType.Text = "TV Tuner Input Type";
			this.mnuInputType.Click += new System.EventHandler(this.mnuInputType_Click);
			// 
			// menuTunerModes1
			// 
			this.menuTunerModes1.Index = 15;
			this.menuTunerModes1.Text = "TV Tuner Mode";
			// 
			// menuItem6
			// 
			this.menuItem6.Index = 16;
			this.menuItem6.Text = "-";
			// 
			// mnuPropertyPages
			// 
			this.mnuPropertyPages.Index = 17;
			this.mnuPropertyPages.Text = "PropertyPages";
			// 
			// menuVideoRenderer1
			// 
			this.menuVideoRenderer1.Index = 18;
			this.menuVideoRenderer1.Text = "Video Renderer";
			this.menuVideoRenderer1.Click += new System.EventHandler(this.menuVideoRenderer1_Click);
			// 
			// menuDeInterlaceFilter1
			// 
			this.menuDeInterlaceFilter1.Index = 19;
			this.menuDeInterlaceFilter1.Text = "DeInterlace Filter";
			this.menuDeInterlaceFilter1.Click += new System.EventHandler(this.menuDeInterlaceFilter1_Click);
			// 
			// menuHelp1
			// 
			this.menuHelp1.Index = 3;
			this.menuHelp1.MenuItems.AddRange(new System.Windows.Forms.MenuItem[] {
																					  this.menuInfo1});
			this.menuHelp1.Text = "Help";
			// 
			// menuInfo1
			// 
			this.menuInfo1.Index = 0;
			this.menuInfo1.Text = "Info";
			this.menuInfo1.Click += new System.EventHandler(this.menuInfo1_Click);
			// 
			// panelVideo
			// 
			this.panelVideo.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
				| System.Windows.Forms.AnchorStyles.Left) 
				| System.Windows.Forms.AnchorStyles.Right)));
			this.panelVideo.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
			this.panelVideo.Location = new System.Drawing.Point(8, 8);
			this.panelVideo.Name = "panelVideo";
			this.panelVideo.Size = new System.Drawing.Size(480, 296);
			this.panelVideo.TabIndex = 6;
			// 
			// pictureBox1
			// 
			this.pictureBox1.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
			this.pictureBox1.Location = new System.Drawing.Point(210, 316);
			this.pictureBox1.Name = "pictureBox1";
			this.pictureBox1.Size = new System.Drawing.Size(90, 60);
			this.pictureBox1.SizeMode = System.Windows.Forms.PictureBoxSizeMode.StretchImage;
			this.pictureBox1.TabIndex = 0;
			this.pictureBox1.TabStop = false;
			// 
			// btnCue
			// 
			this.btnCue.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
			this.btnCue.Location = new System.Drawing.Point(8, 352);
			this.btnCue.Name = "btnCue";
			this.btnCue.Size = new System.Drawing.Size(80, 24);
			this.btnCue.TabIndex = 8;
			this.btnCue.Text = "Cue";
			this.btnCue.Click += new System.EventHandler(this.btnCue_Click);
			// 
			// label3
			// 
			this.label3.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
			this.label3.Location = new System.Drawing.Point(304, 344);
			this.label3.Name = "label3";
			this.label3.Size = new System.Drawing.Size(80, 32);
			this.label3.TabIndex = 11;
			this.label3.Text = "Status";
			// 
			// channelUpDown1
			// 
			this.channelUpDown1.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
			this.channelUpDown1.Location = new System.Drawing.Point(392, 320);
			this.channelUpDown1.Name = "channelUpDown1";
			this.channelUpDown1.Size = new System.Drawing.Size(80, 20);
			this.channelUpDown1.TabIndex = 12;
			this.channelUpDown1.ValueChanged += new System.EventHandler(this.channelUpDown1_ValueChanged);
			// 
			// labelTvChannel1
			// 
			this.labelTvChannel1.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
			this.labelTvChannel1.Location = new System.Drawing.Point(312, 320);
			this.labelTvChannel1.Name = "labelTvChannel1";
			this.labelTvChannel1.Size = new System.Drawing.Size(72, 16);
			this.labelTvChannel1.TabIndex = 13;
			this.labelTvChannel1.Text = "TV Channel:";
			// 
			// CaptureTest
			// 
			this.AutoScaleBaseSize = new System.Drawing.Size(5, 13);
			this.ClientSize = new System.Drawing.Size(496, 385);
			this.Controls.Add(this.labelTvChannel1);
			this.Controls.Add(this.channelUpDown1);
			this.Controls.Add(this.txtFilename);
			this.Controls.Add(this.label3);
			this.Controls.Add(this.btnCue);
			this.Controls.Add(this.panelVideo);
			this.Controls.Add(this.btnExit);
			this.Controls.Add(this.btnStop);
			this.Controls.Add(this.btnStart);
			this.Controls.Add(this.label1);
			this.Controls.Add(this.pictureBox1);
			this.Icon = ((System.Drawing.Icon)(resources.GetObject("$this.Icon")));
			this.Menu = this.mainMenu;
			this.Name = "CaptureTest";
			this.Text = "CaptureTest";
			this.Closed += new System.EventHandler(this.CaptureTest_FormClosed);
			((System.ComponentModel.ISupportInitialize)(this.channelUpDown1)).EndInit();
			this.ResumeLayout(false);

		}
		#endregion

		/// <summary>
		/// The main entry point for the application.
		/// </summary>
		[STAThread]
		static void Main() 
		{
			AppDomain currentDomain = AppDomain.CurrentDomain;
			Application.Run(new CaptureTest());
		}

		private void btnExit_Click(object sender, System.EventArgs e)
		{
            this.ExitCaptureTest();
		}

        private void ExitCaptureTest()
        {
            if (capture != null)
            {
                capture.Stop();
                capture.Dispose();
            }
            Application.Exit();
        }

		private void btnCue_Click(object sender, System.EventArgs e)
		{
			try
			{
				if ( capture == null )
					throw new ApplicationException( "Please select a video and/or audio device." );
				if ( !capture.Cued )
					capture.Filename = txtFilename.Text;
				capture.Cue();
				btnCue.Enabled = false;
				btnStop.Enabled = true;
				btnStart.Enabled = true;
				MessageBox.Show( "Ready to capture.\n\nUse Cue() before Start() to " +
					"do all the preparation work that needs to be done to start a " +
					"capture. Now, when you click Start the capture will begin faster " +
					"than if you had just clicked Start. Using Cue() is completely " +
					"optional. The downside to using Cue() is the preview is disabled until " +
					"the capture begins." );
			}
			catch (Exception ex)
			{
				MessageBox.Show( ex.Message + "\n\n" + ex.ToString() );
			}
		}

		private void btnStart_Click(object sender, System.EventArgs e)
		{
			try
			{
				if ( capture == null )
					throw new ApplicationException( "Please select a video and/or audio device." );
				if ( !capture.Cued )
					capture.Filename = txtFilename.Text;
				capture.Start();
				btnCue.Enabled = false;
				btnStart.Enabled = false;
				btnStop.Enabled = true;
			}
			catch (Exception ex)
			{
				MessageBox.Show( ex.Message + "\n\n" + ex.ToString() );
			}
		}

		private void btnStop_Click(object sender, System.EventArgs e)
		{
			try
			{
				if ( capture == null )
					throw new ApplicationException( "Please select a video and/or audio device." );
				capture.Stop();
				btnCue.Enabled = true;
				btnStart.Enabled = true;
				btnStop.Enabled = false;
				btnStop.Enabled = true;
			}
			catch (Exception ex)
			{
				MessageBox.Show( ex.Message + "\n\n" + ex.ToString() );
			}
		}

        private void initMenu()
        {
			if (this.capture != null)
			{
				// Show County Dependent settings to user
				this.ShowCountryDep();

				// Set flag only if capture device is initialized
				this.capture.VideoSource = this.capture.VideoSource;
				this.capture.UseVMR9 = this.menuUseVMR9.Checked;
				this.menuUseDeInterlace1.Checked = this.FindDeinterlaceFilter(this.menuUseDeInterlace1.Checked);
			}
        }

		private void updateMenu()
		{
			MenuItem m;
			Source s;
			Source current;
			Filter f;
			PropertyPage p;
			Control oldPreviewWindow = null;
 
			// Disable preview to avoid additional flashes (optional)
			if ( capture != null )
			{
				oldPreviewWindow = capture.PreviewWindow;
				capture.PreviewWindow = null;
			}

			// Load video devices
			Filter videoDevice = null;
			if ( capture != null )
				videoDevice = capture.VideoDevice;
			mnuVideoDevices.MenuItems.Clear();
			m = new MenuItem( "(None)", new EventHandler( mnuVideoDevices_Click ) );
			m.Checked = ( videoDevice == null );
			mnuVideoDevices.MenuItems.Add( m );
			for ( int c = 0; c < filters.VideoInputDevices.Count; c++ )
			{
				f = filters.VideoInputDevices[c];
				m = new MenuItem( f.Name, new EventHandler( mnuVideoDevices_Click ) );
				m.Checked = ( videoDevice == f );
				mnuVideoDevices.MenuItems.Add( m );
			}
			mnuVideoDevices.Enabled = ( filters.VideoInputDevices.Count > 0 );

			// Load audio devices
			Filter audioDevice = null;
			if ( capture != null )
				audioDevice = capture.AudioDevice;
			mnuAudioDevices.MenuItems.Clear();
			m = new MenuItem( "(None)", new EventHandler( mnuAudioDevices_Click ) );
			m.Checked = ( audioDevice == null );
			mnuAudioDevices.MenuItems.Add( m );
			for ( int c = 0; c < filters.AudioInputDevices.Count; c++ )
			{
				f = filters.AudioInputDevices[c];
				m = new MenuItem( f.Name, new EventHandler( mnuAudioDevices_Click ) );
				m.Checked = ( audioDevice == f );
				mnuAudioDevices.MenuItems.Add( m );
			}
			mnuAudioDevices.Enabled = ( filters.AudioInputDevices.Count > 0 );


			// Load video compressors
			try
			{
				mnuVideoCompressors.MenuItems.Clear();
				m = new MenuItem( "(None)", new EventHandler( mnuVideoCompressors_Click ) );
				m.Checked = ( capture.VideoCompressor == null );
				mnuVideoCompressors.MenuItems.Add( m );
				for ( int c = 0; c < filters.VideoCompressors.Count; c++ )
				{
					f = filters.VideoCompressors[c];
					m = new MenuItem( f.Name, new EventHandler( mnuVideoCompressors_Click ) );
					m.Checked = ( capture.VideoCompressor == f );
					mnuVideoCompressors.MenuItems.Add( m );
				}
				mnuVideoCompressors.Enabled = ( ( capture.VideoDevice != null ) && ( filters.VideoCompressors.Count > 0 ) );
			}
			catch { mnuVideoCompressors.Enabled = false; }
			
			// Load audio compressors
			try
			{
				mnuAudioCompressors.MenuItems.Clear();
				m = new MenuItem( "(None)", new EventHandler( mnuAudioCompressors_Click ) );
				m.Checked = ( capture.AudioCompressor == null );
				mnuAudioCompressors.MenuItems.Add( m );
				for ( int c = 0; c < filters.AudioCompressors.Count; c++ )
				{
					f = filters.AudioCompressors[c];
					m = new MenuItem( f.Name, new EventHandler( mnuAudioCompressors_Click ) );
					m.Checked = ( capture.AudioCompressor == f );
					mnuAudioCompressors.MenuItems.Add( m );
				}
				mnuAudioCompressors.Enabled = ( ( capture.AudioDevice != null ) && ( filters.AudioCompressors.Count > 0 ) );
			}
			catch { mnuAudioCompressors.Enabled = false; }

			// Load video sources
			try 
			{
				mnuVideoSources.MenuItems.Clear();
				capture.VideoSources = null;
				current = capture.VideoSource;
				for ( int c = 0; c < capture.VideoSources.Count; c++ )
				{
					s = capture.VideoSources[c];
					m = new MenuItem( s.Name, new EventHandler( mnuVideoSources_Click ) );
					m.Checked = ( current == s );
					mnuVideoSources.MenuItems.Add( m );
				}
				mnuVideoSources.Enabled = ( capture.VideoSources.Count > 0 );
				if(current != null)
				{
					capture.VideoSource = current;
				}
			}
			catch { mnuVideoSources.Enabled = false; }

			// Load audio sources
			try
			{
				mnuAudioSources.MenuItems.Clear();
#if TEST
				if(this.capture.AudioSource == null)
				{
					this.capture.AudioSources = null;
					if(this.capture.AudioSources.Count > 0)
					{
					}
				}
#else
				this.capture.AudioSources = null;
#endif
				current = capture.AudioSource;
				for ( int c = 0; c < capture.AudioSources.Count; c++ )
				{
					s = capture.AudioSources[c];
					m = new MenuItem( s.Name, new EventHandler( mnuAudioSources_Click ) );
					m.Checked = ( current == s );
					mnuAudioSources.MenuItems.Add( m );
				}
				mnuAudioSources.Enabled = ( capture.AudioSources.Count > 0 );
			}
			catch { mnuAudioSources.Enabled = false; }

// Start of new Brian's Low code, with some modifcations to make it more usable,
// such as listing the available video standards and color spaces only.
			// Load video standards
			menuVideoStandard1.MenuItems.Clear();
			if( (this.capture != null)&&
				(this.capture.dxUtils != null)&&(this.capture.dxUtils.VideoDecoderAvail) )
			{
				try
				{
					menuVideoStandard1.MenuItems.Clear();
					AnalogVideoStandard currentStandard = capture.dxUtils.VideoStandard;
					AnalogVideoStandard availableStandards = capture.dxUtils.AvailableVideoStandards;
					int mask = 1;
					while(mask <= (int)AnalogVideoStandard.PAL_N_COMBO)
					{
						int avs = mask & (int)availableStandards;
						if(avs != 0)
						{
							m = new MenuItem( ((AnalogVideoStandard)avs).ToString(), new EventHandler( menuVideoStandard1_Click ) );
							m.Checked = (currentStandard == (AnalogVideoStandard)avs);
							menuVideoStandard1.MenuItems.Add(m);
						}
						mask *= 2;
					}
					menuVideoStandard1.Enabled = true;
				}
				catch { menuVideoStandard1.Enabled = false; }
			}
			else
			{
				menuVideoStandard1.Enabled = false;
			}

			// Load color spaces
			menuColorSpace1.MenuItems.Clear();
			if( (this.capture != null)&&(this.capture.dxUtils != null) )
			{
				try
				{
					DxUtils.ColorSpaceEnum currentColorSpace = capture.ColorSpace;
					string[] names = this.capture.dxUtils.SubTypeList;
					foreach (string name in names )
					{
						m = new MenuItem( name, new EventHandler(menuColorSpace1_Click));
						m.Checked = ( currentColorSpace == (DxUtils.ColorSpaceEnum) Enum.Parse(typeof(DxUtils.ColorSpaceEnum), name) );
						menuColorSpace1.MenuItems.Add( m );
					}
					menuColorSpace1.Enabled = true;
				}
				catch { menuColorSpace1.Enabled = false; }
			}
			else
			{
				menuColorSpace1.Enabled = false;
			}

// End of new Brian's Low code

			// Load frame rates
			try
			{
				mnuFrameRates.MenuItems.Clear();
				int frameRate = (int) (capture.FrameRate * 1000);
				m = new MenuItem( "15 fps", new EventHandler( mnuFrameRates_Click ) );
				m.Checked = ( frameRate == 15000 );
				mnuFrameRates.MenuItems.Add( m );
				m = new MenuItem( "24 fps (Film)", new EventHandler( mnuFrameRates_Click ) );
				m.Checked = ( frameRate == 24000 );
				mnuFrameRates.MenuItems.Add( m );
				m = new MenuItem( "25 fps (PAL)", new EventHandler( mnuFrameRates_Click ) );
				m.Checked = ( frameRate == 25000 );
				mnuFrameRates.MenuItems.Add( m );
				m = new MenuItem( "29.997 fps (NTSC)", new EventHandler( mnuFrameRates_Click ) );
				m.Checked = ( frameRate == 29997 );
				mnuFrameRates.MenuItems.Add( m );
				m = new MenuItem( "30 fps (~NTSC)", new EventHandler( mnuFrameRates_Click ) );
				m.Checked = ( frameRate == 30000 );
				mnuFrameRates.MenuItems.Add( m );
				m = new MenuItem( "59.994 fps (2xNTSC)", new EventHandler( mnuFrameRates_Click ) );
				m.Checked = ( frameRate == 59994 );
				mnuFrameRates.MenuItems.Add( m );
				mnuFrameRates.Enabled = true;
			}
			catch { mnuFrameRates.Enabled = false; }

			// Load frame sizes
			try
			{
				mnuFrameSizes.MenuItems.Clear();
				Size frameSize = capture.FrameSize;
				m = new MenuItem( "160 x 120", new EventHandler( mnuFrameSizes_Click ) );
				m.Checked = ( frameSize == new Size( 160, 120 ) );
				mnuFrameSizes.MenuItems.Add( m );
				m = new MenuItem( "320 x 240", new EventHandler( mnuFrameSizes_Click ) );
				m.Checked = ( frameSize == new Size( 320, 240 ) );
				mnuFrameSizes.MenuItems.Add( m );

				// Added a Pal format ...
				m = new MenuItem( "352 x 288", new EventHandler( mnuFrameSizes_Click ) );
				m.Checked = ( frameSize == new Size( 352, 288 ) );
				mnuFrameSizes.MenuItems.Add( m );
				
				m = new MenuItem( "640 x 480", new EventHandler( mnuFrameSizes_Click ) );
				m.Checked = ( frameSize == new Size( 640, 480 ) );
				mnuFrameSizes.MenuItems.Add( m );

				// Added a Ntsc format ...
				m = new MenuItem( "720 x 480", new EventHandler( mnuFrameSizes_Click ) );
				m.Checked = ( frameSize == new Size( 720, 480 ) );
				mnuFrameSizes.MenuItems.Add( m );
				mnuFrameSizes.Enabled = true;

				// Added some Pal formats ...
				m = new MenuItem( "720 x 576", new EventHandler( mnuFrameSizes_Click ) );
				m.Checked = ( frameSize == new Size( 720, 576 ) );
				mnuFrameSizes.MenuItems.Add( m );
				m = new MenuItem( "768 x 576", new EventHandler( mnuFrameSizes_Click ) );
				m.Checked = ( frameSize == new Size( 768, 576 ) );
				mnuFrameSizes.MenuItems.Add( m );
			}
			catch { mnuFrameSizes.Enabled = false; }

			// Load audio channels
			try
			{
				mnuAudioChannels.MenuItems.Clear();
				short audioChannels = capture.AudioChannels;
				m = new MenuItem( "Mono", new EventHandler( mnuAudioChannels_Click ) );
				m.Checked = ( audioChannels == 1 );
				mnuAudioChannels.MenuItems.Add( m );
				m = new MenuItem( "Stereo", new EventHandler( mnuAudioChannels_Click ) );
				m.Checked = ( audioChannels == 2 );
				mnuAudioChannels.MenuItems.Add( m );
				mnuAudioChannels.Enabled = true;
                capture.AudioSources = null;
			}
			catch { mnuAudioChannels.Enabled = false; }

			// Load audio sampling rate
			try
			{
				mnuAudioSamplingRate.MenuItems.Clear();
				int samplingRate = capture.AudioSamplingRate;
				m = new MenuItem( "8 kHz", new EventHandler( mnuAudioSamplingRate_Click ) );
				m.Checked = ( samplingRate == 8000 );
				mnuAudioSamplingRate.MenuItems.Add( m );
				m = new MenuItem( "11,025 kHz", new EventHandler( mnuAudioSamplingRate_Click ) );
				m.Checked = ( capture.AudioSamplingRate == 11025 );
				mnuAudioSamplingRate.MenuItems.Add( m );
				m = new MenuItem( "22,05 kHz", new EventHandler( mnuAudioSamplingRate_Click ) );
				m.Checked = ( capture.AudioSamplingRate == 22050 );
				mnuAudioSamplingRate.MenuItems.Add( m );
				m = new MenuItem( "32 kHz", new EventHandler( mnuAudioSamplingRate_Click ) );
				m.Checked = ( capture.AudioSamplingRate == 32000 );
				mnuAudioSamplingRate.MenuItems.Add( m );
				m = new MenuItem( "44,1 kHz", new EventHandler( mnuAudioSamplingRate_Click ) );
				m.Checked = ( capture.AudioSamplingRate == 44100 );
				mnuAudioSamplingRate.MenuItems.Add( m );
				m = new MenuItem( "48 kHz", new EventHandler( mnuAudioSamplingRate_Click ) );
				m.Checked = ( capture.AudioSamplingRate == 48000 );
				mnuAudioSamplingRate.MenuItems.Add( m );
				mnuAudioSamplingRate.Enabled = true;
			}
			catch { mnuAudioSamplingRate.Enabled = false; } 

			// Load audio sample sizes
			try
			{
				mnuAudioSampleSizes.MenuItems.Clear();
				short sampleSize = capture.AudioSampleSize;
				m = new MenuItem( "8 bit", new EventHandler( mnuAudioSampleSizes_Click ) );
				m.Checked = ( sampleSize == 8 );
				mnuAudioSampleSizes.MenuItems.Add( m );
				m = new MenuItem( "16 bit", new EventHandler( mnuAudioSampleSizes_Click ) );
				m.Checked = ( sampleSize == 16 );
				mnuAudioSampleSizes.MenuItems.Add( m );
				mnuAudioSampleSizes.Enabled = true;
			}
			catch { mnuAudioSampleSizes.Enabled = false; }

            // Load property pages
			try
			{
				this.mnuPropertyPages.MenuItems.Clear();
				for (int c = 0; c < this.capture.PropertyPages.Count; c++)
				{
					p = this.capture.PropertyPages[c];
					m = new MenuItem(p.Name + "...", new EventHandler(this.mnuPropertyPages_Click));
					this.mnuPropertyPages.MenuItems.Add(m);
				}
				this.mnuPropertyPages.Enabled = (this.capture.PropertyPages.Count > 0);
			}
			catch
			{ 
				this.mnuPropertyPages.Enabled = false;
			}
			// Load TV Tuner channels
			try
			{
				mnuChannel.MenuItems.Clear();
                if (this.TunerModeType == AMTunerModeType.TV)
				{
					int current_channel = this.tvSelections.CurrentChannel;

					for ( int c = 1; c <= this.tvSelections.NbrTunerChannels; c++ )
					{
						this.tvSelections.CurrentChannel = c;
						m = new MenuItem( this.tvSelections.GetChannelName.ToString(), new EventHandler( mnuChannel_Click ) );
						m.Checked = ( (current_channel == c)&&(this.capture.Tuner.Channel == this.tvSelections.GetChannelNumber) );
						mnuChannel.MenuItems.Add( m );
					}

					this.tvSelections.CurrentChannel = current_channel;
					//this.capture.Tuner.Channel = this.tvSelections.GetChannelNumber;

					mnuChannel.Enabled = true;
				}
				else
				{
					mnuChannel.Enabled = false;
				}
			}
			catch { mnuChannel.Enabled = false; }

			// Load Tuner Modes (such as TV and FM Radio
			try
			{
				this.menuTunerModes1.MenuItems.Clear();
				if( (this.capture.Tuner.AvailableAudioModes.TV)&& 
					(this.capture.Tuner.AvailableAudioModes.FMRadio) )
				{
                    m = new MenuItem(AMTunerModeType.TV.ToString(), new EventHandler(menuTunerModes1_Click));
                    m.Checked = (this.TunerModeType == AMTunerModeType.TV);
					this.menuTunerModes1.MenuItems.Add( m );
                    m = new MenuItem(AMTunerModeType.FMRadio.ToString(), new EventHandler(menuTunerModes1_Click));
                    m.Checked = (this.TunerModeType == AMTunerModeType.FMRadio);
					m.Enabled = false;
					this.menuTunerModes1.MenuItems.Add( m );

					this.menuTunerModes1.Enabled = true;
					this.menuTunerModes1.Visible = true;

					//this.tvSelections.CurrentChannel = current_channel;
					//this.capture.Tuner.SetFrequency(this.tvSelections.GetChannelFrequency);
				}
				else
				{
					this.menuTunerModes1.Enabled = false;
					this.menuTunerModes1.Visible = false;
				}

			}
			catch
			{
				this.menuTunerModes1.Enabled = false;
				this.menuTunerModes1.Visible = false;
			}

			// Load TV Tuner input types
			try
			{
				mnuInputType.MenuItems.Clear();
                m = new MenuItem(TunerInputType.Cable.ToString(), new EventHandler(mnuInputType_Click));
                m.Checked = (capture.Tuner.InputType == TunerInputType.Cable);
				mnuInputType.MenuItems.Add( m );
                m = new MenuItem(TunerInputType.Antenna.ToString(), new EventHandler(mnuInputType_Click));
                m.Checked = (capture.Tuner.InputType == TunerInputType.Antenna);
				mnuInputType.MenuItems.Add( m );
				mnuInputType.Enabled = true;
			}
			catch { mnuInputType.Enabled = false; }

			// Load audio/video recording file modes
			try
			{
				menuAVRecFileModes.MenuItems.Clear();

				// Fill in all file modes, use enumerations also as string (and file extension)
				for(int i = 0; i < 3; i++)
				{
					m = new MenuItem(((DirectX.Capture.Capture.RecFileModeType)i).ToString(), new EventHandler(menuAVRecFileModes_Click));
					m.Checked = (i == (int)capture.RecFileMode);
					menuAVRecFileModes.MenuItems.Add(m);
				}
				menuAVRecFileModes.Enabled = true;
			}
			catch
			{
				menuAVRecFileModes.Enabled = false;
			}

			// Enable/disable caps
			mnuVideoCaps.Enabled = ( ( capture != null ) && ( capture.VideoCaps != null ) );
			menuPreviewCaps.Enabled = (( capture != null)&&(capture.PreviewCaps != null));
			mnuAudioCaps.Enabled = ( ( capture != null ) && ( capture.AudioCaps != null ) );

			// Check Preview menu option
			mnuPreview.Checked = ( oldPreviewWindow != null );
			mnuPreview.Enabled = ( capture != null );

			// Reenable preview if it was enabled before
			if ( capture != null )
				capture.PreviewWindow = oldPreviewWindow;

			// Determine if the tuner status should be shown. Status is not
			// stable upon setting the TV broadcast frequency. Therefore it
			// the tuner status is shown at the end of this function.
			if(mnuChannel.Enabled)
			{
				string name = this.tvSelections.GetChannelName;
				if((this.capture != null)&&(this.capture.Tuner != null))
				{
					if(this.capture.Tuner != null)
					{
						int[] minmax = new int[2];
						this.noChannelChange = true;
						minmax = this.capture.Tuner.ChanelMinMax;
						this.channelUpDown1.Maximum = minmax[1];
						this.channelUpDown1.Minimum = minmax[0];
						this.channelUpDown1.Increment = 1;
						if(this.DefaultChannel < minmax[0])
						{
							this.DefaultChannel = minmax[0];
						}
						if(this.DefaultChannel > minmax[1])
						{
							this.DefaultChannel = minmax[1];
						}
						this.channelUpDown1.Value = this.DefaultChannel;
						this.noChannelChange = false;
					}

				}
				if(this.DefaultChannel != this.tvSelections.GetChannelNumber)
				{
					name = "Free choice";
				}
				this.SetTunerStatus(name);

			}
        }

		private void mnuVideoDevices_Click(object sender, System.EventArgs e)
		{
			try
			{
				// Get current devices and dispose of capture object
				// because the video and audio device can only be changed
				// by creating a new Capture object.
				Filter videoDevice = null;
				Filter audioDevice = null;

				if ( capture != null )
				{
					videoDevice = capture.VideoDevice;
					audioDevice = capture.AudioDevice;
					capture.Dispose();
					capture = null;
				}

				// Get new video device
				MenuItem m = sender as MenuItem;
				videoDevice = ( m.Index>0 ? filters.VideoInputDevices[m.Index-1] : null );

				// Create capture object
				if ( ( videoDevice != null ) || ( audioDevice != null ) )
				{
					capture = new Capture(videoDevice, audioDevice, AudioViaPci );
					capture.CaptureComplete += new EventHandler( OnCaptureComplete );
					capture.Filename = this.txtFilename.Text;
                    this.initMenu();
				}

				// Update the menu
				updateMenu();
			}
			catch (Exception ex)
			{ 
				MessageBox.Show( "Video device not supported.\n\n" + ex.Message + "\n\n" + ex.ToString() );
			}
		}

		private void mnuAudioDevices_Click(object sender, System.EventArgs e)
		{
			try
			{
				// Get current devices and dispose of capture object
				// because the video and audio device can only be changed
				// by creating a new Capture object.
				Filter videoDevice = null;
				Filter audioDevice = null;

				if ( capture != null )
				{
					videoDevice = capture.VideoDevice;
					audioDevice = capture.AudioDevice;
					capture.Dispose();
					capture = null;
				}

				// Get new audio device
				MenuItem m = sender as MenuItem;
				audioDevice = ( m.Index>0 ? filters.AudioInputDevices[m.Index-1] : null );

				// Create capture object
				if ( ( videoDevice != null ) || ( audioDevice != null ) )
				{
					capture = new Capture(videoDevice, audioDevice, AudioViaPci );
					capture.CaptureComplete += new EventHandler( OnCaptureComplete );
					capture.Filename = this.txtFilename.Text;
                    this.initMenu();
                }

				// Update the menu
				updateMenu();
			}
			catch (Exception ex)
			{ 
				MessageBox.Show( "Audio device not supported.\n\n" + ex.Message + "\n\n" + ex.ToString() );
			}
		}

		private void mnuVideoCompressors_Click(object sender, System.EventArgs e)
		{
			try
			{
				// Change the video compressor
				// We subtract 1 from m.Index beacuse the first item is (None)
				MenuItem m = sender as MenuItem;
				capture.VideoCompressor = ( m.Index>0 ? filters.VideoCompressors[m.Index-1] : null );
				updateMenu();
			}
			catch (Exception ex)
			{ 
				MessageBox.Show( "Video compressor not supported.\n\n" + ex.Message + "\n\n" + ex.ToString() );
			}

		}

		private void mnuAudioCompressors_Click(object sender, System.EventArgs e)
		{
			try
			{
				// Change the audio compressor
				// We subtract 1 from m.Index beacuse the first item is (None)
				MenuItem m = sender as MenuItem;
				capture.AudioCompressor = ( m.Index>0 ? filters.AudioCompressors[m.Index-1] : null );
				updateMenu();
			}
			catch (Exception ex)
			{ 
				MessageBox.Show( "Audio compressor not supported.\n\n" + ex.Message + "\n\n" + ex.ToString() );
			}
		}

		private void mnuVideoSources_Click(object sender, System.EventArgs e)
		{
			try
			{
				// Choose the video source
				// If the device only has one source, this menu item will be disabled
				MenuItem m = sender as MenuItem;
				this.capture.VideoSources = null;
				capture.VideoSource = capture.VideoSources[m.Index];
				updateMenu();
            }
			catch (Exception ex)
			{ 
				MessageBox.Show( "Unable to set video source. Please submit bug report.\n\n" + ex.Message + "\n\n" + ex.ToString() );
			}
		}

		private void mnuAudioSources_Click(object sender, System.EventArgs e)
		{
			try
			{
				// Choose the audio source
				// If the device only has one source, this menu item will be disabled
				MenuItem m = sender as MenuItem;
				this.capture.AudioSources = null;
				capture.AudioSource = capture.AudioSources[m.Index];
				updateMenu();
			}
			catch (Exception ex)
			{ 
				MessageBox.Show( "Unable to set audio source. Please submit bug report.\n\n" + ex.Message + "\n\n" + ex.ToString() );
			}
		}


		private void mnuExit_Click(object sender, System.EventArgs e)
		{
            this.ExitCaptureTest();
        }

		private void mnuFrameSizes_Click(object sender, System.EventArgs e)
		{
			try
			{
				// Disable preview to avoid additional flashes (optional)
				bool preview = (capture.PreviewWindow != null);
				capture.PreviewWindow = null;

				// Update the frame size
				MenuItem m = sender as MenuItem;
				string[] s = m.Text.Split( 'x' );
				Size size = new Size( int.Parse( s[0] ), int.Parse( s[1] ) );
				capture.FrameSize = size;

				// Restore previous preview setting
				capture.PreviewWindow = ( preview ? panelVideo : null );

				// Update the menu
				updateMenu();
			}
			catch (Exception ex)
			{ 
				MessageBox.Show( "Frame size not supported.\n\n" + ex.Message + "\n\n" + ex.ToString() );
			}
		}

		private void mnuFrameRates_Click(object sender, System.EventArgs e)
		{
			try
			{
				MenuItem m = sender as MenuItem;
				string[] s = m.Text.Split( ' ' );
				capture.FrameRate = double.Parse( s[0] );
				updateMenu();
			}
			catch (Exception ex)
			{ 
				MessageBox.Show( "Frame rate not supported.\n\n" + ex.Message + "\n\n" + ex.ToString() );
			}
		}


		private void mnuAudioChannels_Click(object sender, System.EventArgs e)
		{
			try
			{
				MenuItem m = sender as MenuItem;
				capture.AudioChannels = (short) Math.Pow( 2, m.Index );
				updateMenu();
			}
			catch (Exception ex)
			{ 
				MessageBox.Show( "Number of audio channels not supported.\n\n" + ex.Message + "\n\n" + ex.ToString() );
			}
		}

		private void mnuAudioSamplingRate_Click(object sender, System.EventArgs e)
		{
			try
			{
				MenuItem m = sender as MenuItem;
				string[] s = m.Text.Split( ' ' );
				// Parsing is country dependent, in European countries a number
				// with a fraction are separated with a komma, in US this is a dot:
				// 44,1 kHz in Europe versus 44.1 kHz in US
				int samplingRate = (int) (double.Parse( s[0] ) * 1000);
				capture.AudioSamplingRate = samplingRate;
				updateMenu();
			}
			catch (Exception ex)
			{ 
				MessageBox.Show( "Audio sampling rate not supported.\n\n" + ex.Message + "\n\n" + ex.ToString() );
			}
		}

		private void mnuAudioSampleSizes_Click(object sender, System.EventArgs e)
		{
			try
			{
				MenuItem m = sender as MenuItem;
				string[] s = m.Text.Split( ' ' );
				short sampleSize = short.Parse( s[0] );
				capture.AudioSampleSize = sampleSize;
				updateMenu();
			}
			catch (Exception ex)
			{ 
				MessageBox.Show( "Audio sample size not supported.\n\n" + ex.Message + "\n\n" + ex.ToString() );
			}
		}

		private void mnuPreview_Click(object sender, System.EventArgs e)
		{
			try
			{
				if ( capture.PreviewWindow == null )
				{
					capture.PreviewWindow = panelVideo;
					mnuPreview.Checked = true;
				}
				else
				{
					capture.PreviewWindow = null;
					mnuPreview.Checked = false;
				}
			}
			catch (Exception ex)
			{ 
				MessageBox.Show( "Unable to enable/disable preview. Please submit a bug report.\n\n" + ex.Message + "\n\n" + ex.ToString() );
			}
		}

		private void mnuPropertyPages_Click(object sender, System.EventArgs e)
		{
			try
			{
				MenuItem m = sender as MenuItem;
				this.capture.PropertyPages = null;
				capture.PropertyPages[m.Index].Show( this );

				if(this.mnuPropertyPages.MenuItems[m.Index].Text == "TV Tuner...")
				{
					this.DefaultChannel = this.capture.Tuner.Channel;
					this.DefaultCountryCode = this.capture.Tuner.CountryCode;
					this.DefaultTuningSpace = this.capture.Tuner.TuningSpace;
					this.tunerInputType = this.capture.Tuner.InputType;
				}

				updateMenu();
			}
			catch (Exception ex)
			{ 
				MessageBox.Show( "Unable display property page. Please submit a bug report.\n\n" + ex.Message + "\n\n" + ex.ToString() );
			}
		}

		private void mnuChannel_Click(object sender, System.EventArgs e)
		{
			try
			{
				MenuItem m = sender as MenuItem;
				this.tvSelections.CurrentChannel = m.Index+1;
				if((this.capture != null)&&(this.capture.Tuner != null))
				{
					this.capture.Tuner.Channel = this.tvSelections.GetChannelNumber;
					this.DefaultChannel = this.capture.Tuner.Channel;
				}

				updateMenu();
			}
			catch (Exception ex)
			{ 
				MessageBox.Show( "Unable change channel. Please submit a bug report.\n\n" + ex.Message + "\n\n" + ex.ToString() );
			}
		}

		private void menuTunerModes1_Click(object sender, System.EventArgs e)
		{
			// Select selected Tuner Mode
			MenuItem m = sender as MenuItem;
            if (m.Text.ToString() == AMTunerModeType.TV.ToString())
			{
				// This is TV
                this.TunerModeType = AMTunerModeType.TV;
			}

			// Temporary fix ...
			this.updateMenu();
		}

		private void mnuInputType_Click(object sender, System.EventArgs e)
		{
			try
			{
				MenuItem m = sender as MenuItem;
                this.TunerInputType = (TunerInputType)m.Index;
                updateMenu();
			}
			catch (Exception ex)
			{ 
				MessageBox.Show( "Unable change tuner input type. Please submit a bug report.\n\n" + ex.Message + "\n\n" + ex.ToString() );
			}
		}

		private void mnuVideoCaps_Click(object sender, System.EventArgs e)
		{
			try
			{
				string s;
				s = String.Format(
					"Video Device Capabilities\n" +
					"--------------------------------\n\n" +
					"Input Size:\t\t{0} x {1}\n" +
					"\n" +
					"Min Frame Size:\t\t{2} x {3}\n" +
					"Max Frame Size:\t\t{4} x {5}\n" +
					"Frame Size Granularity X:\t{6}\n" +
					"Frame Size Granularity Y:\t{7}\n" +
					"\n" +
					"Min Frame Rate:\t\t{8:0.000} fps\n" +
					"Max Frame Rate:\t\t{9:0.000} fps\n" +
					"Video modes: {10}\n",
					capture.VideoCaps.InputSize.Width, capture.VideoCaps.InputSize.Height,
					capture.VideoCaps.MinFrameSize.Width, capture.VideoCaps.MinFrameSize.Height,
					capture.VideoCaps.MaxFrameSize.Width, capture.VideoCaps.MaxFrameSize.Height,
					capture.VideoCaps.FrameSizeGranularityX,
					capture.VideoCaps.FrameSizeGranularityY,
					capture.VideoCaps.MinFrameRate,
					capture.VideoCaps.MaxFrameRate,
					capture.VideoCaps.AnalogVideoStandard.ToString());
				MessageBox.Show( s );

			}
			catch (Exception ex)
			{
				MessageBox.Show( "Unable display video capabilities. Please submit a bug report.\n\n" + ex.Message + "\n\n" + ex.ToString() );
			}
		}

		private void mnuAudioCaps_Click(object sender, System.EventArgs e)
		{
			try
			{
				string s;
				s = String.Format(
					"Audio Device Capabilities\n" +
					"--------------------------------\n\n" +
					"Min Channels:\t\t{0}\n" +
					"Max Channels:\t\t{1}\n" +
					"Channels Granularity:\t{2}\n" +
					"\n" +
					"Min Sample Size:\t\t{3}\n" +
					"Max Sample Size:\t\t{4}\n" +
					"Sample Size Granularity:\t{5}\n" +
					"\n" +
					"Min Sampling Rate:\t\t{6}\n" +
					"Max Sampling Rate:\t\t{7}\n" +
					"Sampling Rate Granularity:\t{8}\n",
					capture.AudioCaps.MinimumChannels,
					capture.AudioCaps.MaximumChannels,
					capture.AudioCaps.ChannelsGranularity,
					capture.AudioCaps.MinimumSampleSize, 
					capture.AudioCaps.MaximumSampleSize,
					capture.AudioCaps.SampleSizeGranularity,
					capture.AudioCaps.MinimumSamplingRate,
					capture.AudioCaps.MaximumSamplingRate,
					capture.AudioCaps.SamplingRateGranularity );
				MessageBox.Show( s );

			}
			catch (Exception ex)
			{
				MessageBox.Show( "Unable display audio capabilities. Please submit a bug report.\n\n" + ex.Message + "\n\n" + ex.ToString() );
			}		
		}

		private void OnCaptureComplete(object sender, EventArgs e)
		{
			// Demonstrate the Capture.CaptureComplete event.
			Debug.WriteLine( "Capture complete." );
		}

		private void menuAudioViaPci1_Click(object sender, System.EventArgs e)
		{
			// Turn on/off option for getting Audio via Pci bus.
			// This option will work properly only after reloading
			// the audio or video device.
			this.AudioViaPci = !this.AudioViaPci;
		}

		private void menuAVRecFileModes_Click(object sender, System.EventArgs e)
		{
			try
			{
				MenuItem m = sender as MenuItem;
				capture.RecFileMode = (DirectX.Capture.Capture.RecFileModeType)m.Index;
				this.txtFilename.Text = this.capture.Filename;
				switch(capture.RecFileMode)
				{
					case DirectX.Capture.Capture.RecFileModeType.Wmv:
					case DirectX.Capture.Capture.RecFileModeType.Wma:
						menuAsfFileFormat.Enabled = true;
						break;
					case DirectX.Capture.Capture.RecFileModeType.Avi:
						menuAsfFileFormat.Enabled = false;
						break;
					default:
						break;
				}

				updateMenu();
			}
			catch (Exception ex)
			{ 
				MessageBox.Show( "Audio sample size not supported.\n\n" + ex.Message + "\n\n" + ex.ToString() );
			}
			updateMenu();
		}

		private void menuAsfFileFormat_Click(object sender, System.EventArgs e)
		{
			if(capture != null)
			{
				// Show form for Windows Media formats only
				switch(capture.RecFileMode)
				{
					case DirectX.Capture.Capture.RecFileModeType.Wmv:
					case DirectX.Capture.Capture.RecFileModeType.Wma:
						AsfForm asfForm = new AsfForm(capture);
						asfForm.ShowDialog(this);
						updateMenu();
						break;
					default:
						break;
				}
			}
		
		}

		private void txtFilename_TextChanged(object sender, System.EventArgs e)
		{
			this.capture.Filename = this.txtFilename.Text;
		}

		private void SetTunerStatus(String channelName)
		{
			if(this.capture != null)
			{
				if(this.capture.Tuner != null)
				{
					this.label3.Text = channelName;

					try
					{
						// SignalPresent fails if this feature is not available
						// or the tuner input is not used. Catch error so
						// progrom will not fail.
						if(this.capture.Tuner.SignalPresent)
						{
							this.label3.Text = this.label3.Text + " (Signal)";
						}
					}
					catch
					{
						// No signal present
#if DEBUG
						Debug.WriteLine( "This tuner does not support signal present!" );
#endif
					}
				}
			}
		}

		private void txtFilename_DoubleClick(object sender, System.EventArgs e)
		{
			txtFilename_TextChanged(sender, e);
		}

		private void txtFilename_KeyDown(object sender, System.Windows.Forms.KeyEventArgs e)
		{
			txtFilename_TextChanged(sender, e);
		}

#if VS2003
	    private void CaptureTest_FormClosed(object sender, System.EventArgs e)
#else
        private void CaptureTest_FormClosed(object sender, FormClosedEventArgs e)
#endif
        {
            this.ExitCaptureTest();
        }

		private void menuUseVMR9_Click(object sender, System.EventArgs e)
		{
			this.menuUseVMR9.Checked = !this.menuUseVMR9.Checked;
		}

		private bool FindDeinterlaceFilter(bool addFilter)
		{
			if (addFilter)
			{
				string filterName = "Alparysoft Deinterlace Filter";

				for (int i = 0; i < this.filters.LegacyFilters.Count; i++)
				{
					if (filters.LegacyFilters[i].Name.StartsWith(filterName))
					{
						this.capture.DeInterlace = filters.LegacyFilters[i];
						return true;
					}
				}
			}

			// Either no de-interlace filter needs to be found
			// or no de-interlace filter could be found
			this.capture.DeInterlace = null;
			return false;
		}

		private void menuUseDeInterlace1_Click(object sender, System.EventArgs e)
		{
			this.menuUseDeInterlace1.Checked = !this.menuUseDeInterlace1.Checked;
		}

//#if NEWCODE
		private void menuVideoRenderer1_Click(object sender, System.EventArgs e)
		{
			// Try to show property page video renderer
			if(this.capture != null)
			{
				this.capture.ShowPropertyPage(1, this);
			}
		}

		private void menuDeInterlaceFilter1_Click(object sender, System.EventArgs e)
		{
			// Try to show property page DeInterlace Filter if selected
			if(this.capture != null)
			{
				this.capture.ShowPropertyPage(2, this);
			}
		}

		private void initVideoDevice()
		{
		}

// Start of new Brian's Low code
		private void menuVideoStandard1_Click(object sender, System.EventArgs e)
		{
			if( (this.capture == null)||(this.capture.dxUtils == null) )
			{
				return;
			}
			try
			{
				MenuItem m = sender as MenuItem;
				AnalogVideoStandard a = (AnalogVideoStandard) Enum.Parse(typeof(AnalogVideoStandard), m.Text);
				capture.dxUtils.VideoStandard = a;
				updateMenu();
			}
			catch (Exception ex)
			{ 
				MessageBox.Show( "Unable to set video standard.\n\n" + ex.Message + "\n\n" + ex.ToString() );
			}		
		}

		private void menuColorSpace1_Click(object sender, System.EventArgs e)
		{
			try
			{
				MenuItem m = sender as MenuItem;
				DxUtils.ColorSpaceEnum c = (DxUtils.ColorSpaceEnum) Enum.Parse(typeof(DxUtils.ColorSpaceEnum), m.Text);
				this.capture.ColorSpace = c;
				this.updateMenu();
			}
			catch (Exception ex)
			{ 
				MessageBox.Show( "Unable to set color space.\n\n" + ex.Message + "\n\n" + ex.ToString() );
			}				
		}

		private void menuPreviewCaps_Click(object sender, System.EventArgs e)
		{
			try
			{
				string s;
				s = String.Format(
					"Video Preview Capabilities\n" +
					"--------------------------------\n\n" +
					"Input Size:\t\t{0} x {1}\n" +
					"\n" +
					"Min Frame Size:\t\t{2} x {3}\n" +
					"Max Frame Size:\t\t{4} x {5}\n" +
					"Frame Size Granularity X:\t{6}\n" +
					"Frame Size Granularity Y:\t{7}\n" +
					"\n" +
					"Min Frame Rate:\t\t{8:0.000} fps\n" +
					"Max Frame Rate:\t\t{9:0.000} fps\n", 
					capture.PreviewCaps.InputSize.Width, capture.PreviewCaps.InputSize.Height,
					capture.PreviewCaps.MinFrameSize.Width, capture.PreviewCaps.MinFrameSize.Height, 
					capture.PreviewCaps.MaxFrameSize.Width, capture.PreviewCaps.MaxFrameSize.Height, 
					capture.PreviewCaps.FrameSizeGranularityX, 
					capture.PreviewCaps.FrameSizeGranularityY,
					capture.PreviewCaps.MinFrameRate, 
					capture.PreviewCaps.MaxFrameRate );
				MessageBox.Show( s );

			}
			catch (Exception ex)
			{
				MessageBox.Show( "Unable display video capabilities. Please submit a bug report.\n\n" + ex.Message + "\n\n" + ex.ToString() );
			}
		}
// End of new Brian's Low code

		private void menuInfo1_Click(object sender, System.EventArgs e)
		{
			// Open the About form in Dialog Mode
			frmAbout frm = new frmAbout();
			frm.ShowDialog(this);
			frm.Dispose();
		}

		private void ShowCountryDep()
		{
			if((this.capture != null)&&(this.capture.VideoDevice != null))
			{
				if( (this.capture.Tuner == null)&&
					((this.capture.dxUtils == null)||
					 ((this.capture.dxUtils != null)&&
					  (!this.capture.dxUtils.VideoDecoderAvail))) )
				{
					return; // Nothing to do
				}
				CountryDep countryDep = new CountryDep();
				countryDep.Capture = this.capture;
				countryDep.DefaultChannel = this.DefaultChannel;
				countryDep.DefaultTuningSpace = this.DefaultTuningSpace;
				countryDep.DefaultCountryCode = this.DefaultCountryCode;
				countryDep.DefaultInputType = this.tunerInputType;
				countryDep.ShowDialog(this);
				this.DefaultTuningSpace = countryDep.DefaultTuningSpace;
				this.DefaultCountryCode = countryDep.DefaultCountryCode;
				this.DefaultChannel = countryDep.DefaultChannel;
				this.tunerInputType = countryDep.DefaultInputType;
				countryDep.Dispose();
			}
		}

		private void channelUpDown1_ValueChanged(object sender, System.EventArgs e)
		{
			if(this.noChannelChange)
			{
				return;
			}
			if((this.capture != null)&&(this.capture.Tuner != null))
			{
				this.DefaultChannel = (int)this.channelUpDown1.Value;
				this.capture.Tuner.Channel = this.DefaultChannel;
				this.updateMenu();
			}
		}
//#endif
	}
}
