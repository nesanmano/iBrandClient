using System;
using System.Runtime.InteropServices;
using Canon.Eos.Framework.Eventing;
using Canon.Eos.Framework.Helper;
using Canon.Eos.Framework.Internal.SDK;
using System.Collections.Generic;
using System.Linq;
using static Canon.Eos.Framework.Internal.SDK.Edsdk;

namespace Canon.Eos.Framework
{
    public delegate void DirItemEventHandler(object sender, IntPtr dirItem, EosCamera camera);
    public delegate void DownloadHandler(object sender, IntPtr dirItem, EosCamera camera);

    public sealed partial class EosCamera : EosObject
    {
        const int WaitTimeoutForNextLiveDownload = 60;//125;
        const int MaximumCopyrightLengthInBytes = 64;
        const int MaximumArtistLengthInBytes = 64;
        const int MaximumOwnerNameLengthInBytes = 32;

        private Edsdk.EdsDeviceInfo _deviceInfo;
        private Edsdk.EdsObjectEventHandler _edsObjectEventHandler;
        private Edsdk.EdsPropertyEventHandler _edsPropertyEventHandler;
        private Edsdk.EdsStateEventHandler _edsStateEventHandler;        

        public event EventHandler LiveViewStarted;
        public event EventHandler LiveViewStopped;
        public event EventHandler<EosLiveImageEventArgs> LiveViewUpdate;
        public event DirItemEventHandler FileCreatedOnCamera;
        public event EventHandler<EosFileEventArgs> FileSavedOnHost;


        public event EventHandler Shutdown;        
        public event EventHandler<EosVolumeInfoEventArgs> VolumeInfoChanged;
        public event EventHandler<EosExceptionEventArgs> Error;

        private string _author = null;
        private int WAIT_TIMEOUT_FOR_NEXT_LIVE_DOWNLOAD = 500;

        internal EosCamera(IntPtr camera)
            : base(camera)
        {
            Util.Assert(Edsdk.EdsGetDeviceInfo(Handle, out _deviceInfo), 
                "Failed to get device info.");                  
            SetEventHandlers();
            EnsureOpenSession();

            _author = Artist;
        }
            
        /// <summary>
        /// Gets or sets the artist.
        /// </summary>
        /// <value>
        /// The artist.
        /// </value>
        public new string Artist
        {
            get { return base.Artist; }
            set { 
                SetPropertyStringData(Edsdk.PropID_Artist, value, 
                MaximumArtistLengthInBytes); 
            }
        }

        public string Author
        {
            get
            {
                return _author;
            }
        }

        /// <summary>
        /// Gets or sets the battery level.
        /// </summary>
        /// <value>
        /// The battery level.
        /// </value>
        [EosProperty(Edsdk.PropID_BatteryQuality)]
        public uint BatteryLevel
        {
            get { return GetPropertyIntegerData(Edsdk.PropID_BatteryLevel); }
            set { SetPropertyIntegerData(Edsdk.PropID_BatteryLevel, value); }
        }

        /// <summary>
        /// Gets or sets the battery quality.
        /// </summary>
        /// <value>
        /// The battery quality.
        /// </value>
        public EosBatteryQuality BatteryQuality
        {
            get { return (EosBatteryQuality)GetPropertyIntegerData(Edsdk.PropID_BatteryQuality); }
            set { SetPropertyIntegerData(Edsdk.PropID_BatteryQuality, (uint)value); }
        }

        /// <summary>
        /// Gets or sets the copyright.
        /// </summary>
        /// <value>
        /// The copyright.
        /// </value>
        public new string Copyright
        {
            get { return base.Copyright; }
            set { SetPropertyStringData(Edsdk.PropID_Copyright, value, 
                MaximumCopyrightLengthInBytes); }
        }

        /// <summary>
        /// Gets or sets a value indicating whether depth of field preview is enabled.
        /// </summary>
        /// <value>
        /// 	<c>true</c> if depth of field preview is enabled; otherwise, <c>false</c>.
        /// </value>
        [EosProperty(Edsdk.PropID_Evf_DepthOfFieldPreview)]        
        public bool DepthOfFieldPreview
        {
            get { return GetPropertyIntegerData(Edsdk.PropID_Evf_DepthOfFieldPreview) != 0; }
            set { SetPropertyIntegerData(Edsdk.PropID_Evf_DepthOfFieldPreview, (uint) (value ? 1 : 0)); }
        }

        /// <summary>
        /// Gets the device description.
        /// </summary>
        public string DeviceDescription
        {
            get { return _deviceInfo.szDeviceDescription; }
        }

        /// <summary>
        /// Gets or sets the image quality.
        /// </summary>
        /// <value>
        /// The image quality.
        /// </value>
        [EosProperty(Edsdk.PropID_ImageQuality)]        
        public EosImageQuality ImageQuality
        {
            get { return EosImageQuality.Create(GetPropertyIntegerData(Edsdk.PropID_ImageQuality)); }
            set { SetPropertyIntegerData(Edsdk.PropID_ImageQuality, value.ToBitMask()); }
        }

        public bool IsErrorTolerantMode { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether this instance is in live view mode.
        /// </summary>
        /// <value>
        /// 	<c>true</c> if this instance is in live view mode; otherwise, <c>false</c>.
        /// </value>
        [EosProperty(Edsdk.PropID_Evf_Mode)]        
        public bool IsInLiveViewMode
        {
            get { return GetPropertyIntegerData(Edsdk.PropID_Evf_Mode) != 0; }                    
            set { SetPropertyIntegerData(Edsdk.PropID_Evf_Mode, (uint) (value ? 1 : 0)); }
        }

        /// <summary>
        /// Gets a value indicating whether this instance is in host live view mode.
        /// </summary>
        /// <value>
        /// 	<c>true</c> if this instance is in host live view mode; otherwise, <c>false</c>.
        /// </value>
        public bool IsInHostLiveViewMode
        {
            get { return IsInLiveViewMode && LiveViewDevice.HasFlag(EosLiveViewDevice.Host); }
        }

        /// <summary>
        /// Gets a value indicating whether session to the camera is open.
        /// </summary>
        /// <value>
        /// 	<c>true</c> if the session is open; otherwise, <c>false</c>.
        /// </value>
        public bool IsSessionOpen { get; private set; }

        /// <summary>
        /// Gets or sets the live view auto focus.
        /// </summary>
        /// <value>
        /// The live view auto focus.
        /// </value>
        [EosProperty(Edsdk.PropID_Evf_AFMode)]
        public EosLiveViewAutoFocus LiveViewAutoFocus
        {
            get { return (EosLiveViewAutoFocus)GetPropertyIntegerData(Edsdk.PropID_Evf_AFMode); }
            set { SetPropertyIntegerData(Edsdk.PropID_Evf_AFMode, (uint)value); }
        }

        /// <summary>
        /// Gets or sets the live view color temperature.
        /// </summary>
        /// <value>
        /// The live view color temperature.
        /// </value>
        [EosProperty(Edsdk.PropID_Evf_ColorTemperature)]
        public uint LiveViewColorTemperature
        {
            get { return GetPropertyIntegerData(Edsdk.PropID_Evf_ColorTemperature); }
            set { SetPropertyIntegerData(Edsdk.PropID_Evf_ColorTemperature, value); }
        }

        /// <summary>
        /// Gets or sets the live view device.
        /// </summary>
        /// <value>
        /// The live view device.
        /// </value>
        [EosProperty(Edsdk.PropID_Evf_OutputDevice)]
        public EosLiveViewDevice LiveViewDevice
        {
            get { return (EosLiveViewDevice)GetPropertyIntegerData(Edsdk.PropID_Evf_OutputDevice); }
            set { SetPropertyIntegerData(Edsdk.PropID_Evf_OutputDevice, (uint)value); }
        }

        /// <summary>
        /// Gets or sets the live view white balance.
        /// </summary>
        /// <value>
        /// The live view white balance.
        /// </value>
        [EosProperty(Edsdk.PropID_Evf_WhiteBalance)]
        public EosWhiteBalance LiveViewWhiteBalance
        {
            get { return (EosWhiteBalance)GetPropertyIntegerData(Edsdk.PropID_Evf_WhiteBalance); }
            set { SetPropertyIntegerData(Edsdk.PropID_Evf_WhiteBalance, (uint)value); }
        }

        /// <summary>
        /// Gets a value indicating whether this instance is legacy.
        /// </summary>
        /// <value>
        ///   <c>true</c> if this instance is legacy; otherwise, <c>false</c>.
        /// </value>
        public bool IsLegacy
        {
            get { return _deviceInfo.DeviceSubType == 0; }
        }

        /// <summary>
        /// Gets a value indicating whether this instance is locked.
        /// </summary>
        /// <value>
        ///   <c>true</c> if this instance is locked; otherwise, <c>false</c>.
        /// </value>
        public bool IsLocked { get; private set; }

        /// <summary>
        /// Gets or sets the name of the owner.
        /// </summary>
        /// <value>
        /// The name of the owner.
        /// </value>
        public new string OwnerName
        {
            get { return base.OwnerName; }
            set { SetPropertyStringData(Edsdk.PropID_OwnerName, value, 
                MaximumOwnerNameLengthInBytes); }
        }

        /// <summary>
        /// Gets the name of the port.
        /// </summary>
        /// <value>
        /// The name of the port.
        /// </value>
        public string PortName
        {
            get { return _deviceInfo.szPortName; }
        }

        [Flags]
        private enum SaveLocation { Camera = 1, Host = 2 };
 
        private void ChangePicturesSaveLocation(SaveLocation saveLocation)
        {
            CheckDisposed();

            EnsureOpenSession();

            Util.Assert(Edsdk.EdsSetPropertyData(Handle, Edsdk.PropID_SaveTo, 0, Marshal.SizeOf(typeof(int)), 
                (int)saveLocation), "Failed to set SaveTo location.");
            
            if(!IsLegacy)
            {
                LockAndExceute(() =>
                {
                    var capacity = new Edsdk.EdsCapacity { NumberOfFreeClusters = 0x7FFFFFFF, BytesPerSector = 0x1000, Reset = 1 };
                    Util.Assert(Edsdk.EdsSetCapacity(Handle, capacity), "Failed to set capacity.");
                });                
            }            
        }
                
        protected internal override void DisposeUnmanaged()
        {
            if (IsSessionOpen)
            {
                Edsdk.EdsCloseSession(Handle);
            }
            base.DisposeUnmanaged();
        }

        private void EnsureOpenSession()
        {
            CheckDisposed();
            if (!IsSessionOpen)
            {
                Util.Assert(Edsdk.EdsOpenSession(Handle), "Failed to open session.");
                IsSessionOpen = true;
            }
        }

        protected override TResult ExecuteGetter<TResult>(Func<TResult> function)
        {
            try
            {
                if (IsLegacy && !IsLocked)
                    return LockAndExceute(function);
                return base.ExecuteGetter(function);
            }
            catch (Exception ex)
            {
                OnError(ex);
                return default(TResult);
            }
        }

        protected override void ExecuteSetter(Action action)
        {
            try
            {
                if (IsLegacy && !IsLocked)
                {
                    LockAndExceute(action);
                    return;
                }

                base.ExecuteSetter(action);
            }
            catch (Exception ex)
            {
                OnError(ex);
            }
        }

        private void Lock()
        {
            CheckDisposed();

            if (!IsLocked)
            {
                Util.Assert(Edsdk.EdsSendStatusCommand(Handle, Edsdk.CameraState_UILock, 0),
                    "Failed to lock camera.");
                IsLocked = true;
            }
        }

        private void LockAndExceute(Action action)
        {
            Lock();
            try { action(); }
            finally { Unlock(); }
        }

        private TResult LockAndExceute<TResult>(Func<TResult> function)
        {
            Lock();
            try { return function(); }
            finally { Unlock(); }
        }

        private void OnError(Exception exception)
        {
            if (!IsErrorTolerantMode)
                throw exception;
            OnError(new EosExceptionEventArgs { Exception = exception });
        }

        private void OnError(EosExceptionEventArgs args)
        {
            if (Error != null)
                Error(this, args);
        }
        
        /// <summary>
        /// Saves the pictures to camera.
        /// </summary>
        public void SavePicturesToCamera()
        {
            CheckDisposed();
            ChangePicturesSaveLocation(SaveLocation.Camera);
        }

        /// <summary>
        /// Saves the pictures to host.
        /// </summary>
        /// <param name="pathFolder">The path folder.</param>
        public void SavePicturesToHost(string pathFolder)
        {
            SavePicturesToHost(pathFolder, SaveLocation.Host);
        }

        /// <summary>
        /// Saves the pictures to host and camera.
        /// </summary>
        /// <param name="pathFolder">The path folder.</param>
        public void SavePicturesToHostAndCamera(string pathFolder)
        {
            SavePicturesToHost(pathFolder, SaveLocation.Camera);
        }

        private void SavePicturesToHost(string pathFolder, SaveLocation saveLocation)
        {
            if (string.IsNullOrWhiteSpace(pathFolder))
                throw new ArgumentException("Cannot be null or white space.", "pathFolder");

            CheckDisposed();

            ChangePicturesSaveLocation(saveLocation | SaveLocation.Host);
        }        

        public uint SendCommand(uint command, int parameter = 0)
        {
            EnsureOpenSession();            
            return Edsdk.EdsSendCommand(Handle, command, parameter);
        }

        private void SetEventHandlers()
        {   
            _edsStateEventHandler = HandleStateEvent;
            Util.Assert(Edsdk.EdsSetCameraStateEventHandler(Handle, Edsdk.StateEvent_All, 
                _edsStateEventHandler, IntPtr.Zero), "Failed to set state handler.");                     

            _edsObjectEventHandler = HandleObjectEvent;            
            Util.Assert(Edsdk.EdsSetObjectEventHandler(Handle, Edsdk.ObjectEvent_All, 
                _edsObjectEventHandler, IntPtr.Zero), "Failed to set object handler.");

            _edsPropertyEventHandler = HandlePropertyEvent;
            Util.Assert(Edsdk.EdsSetPropertyEventHandler(Handle, Edsdk.PropertyEvent_All, 
                _edsPropertyEventHandler, IntPtr.Zero), "Failed to set object handler.");            
        }

        /// <summary>
        /// Starts the live view.
        /// </summary>
        /// <returns></returns>
        public EosLiveViewAutoFocus StartLiveView()
        {
            if (!IsInLiveViewMode)
                IsInLiveViewMode = true;
            var device = LiveViewDevice;
            device = device | EosLiveViewDevice.Host;
            LiveViewDevice = device;
            return LiveViewAutoFocus;
        }

        /// <summary>
        /// Starts the live view with special auto focus.
        /// </summary>
        /// <param name="autoFocus">The auto focus.</param>
        /// <returns></returns>
        public EosLiveViewAutoFocus StartLiveView(EosLiveViewAutoFocus autoFocus)
        {
            StartLiveView();
            LiveViewAutoFocus = autoFocus;
            return LiveViewAutoFocus;
        }

        /// <summary>
        /// Stops the live view.
        /// </summary>
        public void StopLiveView()
        {
            IsInLiveViewMode = false; 
            LiveViewDevice = EosLiveViewDevice.None;
            SetPropertyIntegerData(Edsdk.PropID_Evf_OutputDevice, (uint)EosLiveViewDevice.None);
        }

        public CameraFileEntry[] GetAllImagesSub()
        {
            CameraFileEntry[] volumes = GetAllVolumesSub();
            List<CameraFileEntry> ImageList = new List<CameraFileEntry>();

            for (int i = 0; i < volumes.Length; i++)
            {
                int ChildCount;
                Util.Assert(Edsdk.EdsGetChildCount(volumes[i].Reference, out ChildCount), "Failed get children count");
                CameraFileEntry[] ventries = new CameraFileEntry[0];

                for (int j = 0; j < ChildCount; j++)
                {
                    CameraFileEntry entry = GetChild(volumes[i].Reference, j);
                    if (entry.IsFolder && entry.Name == "DCIM") { ventries = GetChildren(entry.Reference); break; }
                }

                foreach (CameraFileEntry ve in ventries) { if (ve.IsFolder && ve.Entries != null) ImageList.AddRange(ve.Entries.Where(t => !t.IsFolder)); }
            }

            return ImageList.ToArray();
        }

        private CameraFileEntry[] GetAllVolumesSub()
        {
            int VolumeCount;
            IntPtr ChildPtr;
            EdsVolumeInfo vinfo;
            List<CameraFileEntry> VolumeEntries = new List<CameraFileEntry>();

            //MainThread.Invoke(() =>
            //{
                Util.Assert(Edsdk.EdsGetChildCount(Handle, out VolumeCount), "Failed to get child count.");

                for (int i = 0; i < VolumeCount; i++)
                {
                Util.Assert(Edsdk.EdsGetChildAtIndex(Handle, i, out ChildPtr), "Failed to get child at.");
                    Util.Assert(Edsdk.EdsGetVolumeInfo(ChildPtr, out vinfo), "Failed to get volume info.");
                    if (vinfo.szVolumeLabel != "HDD") VolumeEntries.Add(new CameraFileEntry(ChildPtr, "Volume" + i + "(" + vinfo.szVolumeLabel + ")", true, true));
                }
            //});

            return VolumeEntries.ToArray();
        }

        /// <summary>
        /// Gets information about children of a folder in the cameras file system
        /// </summary>
        /// <param name="ptr">Pointer to the folder</param>
        /// <param name="index">Index of the child in the folder</param>
        /// <returns>A camera file entry of this child</returns>
        /// <exception cref="SDKException">An SDK call failed</exception>
        private CameraFileEntry GetChild(IntPtr ptr, int index)
        {

                IntPtr ChildPtr;
                EdsDirectoryItemInfo ChildInfo;
                Util.Assert(Edsdk.EdsGetChildAtIndex(ptr, index, out ChildPtr), "Failed get child at.");
                Util.Assert(Edsdk.EdsGetDirectoryItemInfo(ChildPtr, out ChildInfo), "Failed get directory item info.");

                CameraFileEntry outEntry = new CameraFileEntry(ChildPtr, ChildInfo.szFileName, ChildInfo.isFolder, false);
                /*if (!outEntry.IsFolder)
                {
                    using (var stream = new SDKStream(0))
                    {
                        ErrorHandler.CheckError(this, CanonSDK.EdsDownloadThumbnail(ChildPtr, stream.Reference));
                        outEntry.SetThumb(stream.Reference);
                    }
                }*/
                return outEntry;

        }

        /// <summary>
        /// Gets information about camera file entries recursively
        /// </summary>
        /// <param name="ptr">Pointer to the current file object</param>
        /// <returns>An array of file entries</returns>
        /// <exception cref="SDKException">An SDK call failed</exception>
        private CameraFileEntry[] GetChildren(IntPtr ptr)
        {
            int ChildCount;
            Util.Assert(Edsdk.EdsGetChildCount(ptr, out ChildCount), "Failed to get child count");
            if (ChildCount > 0)
            {
                CameraFileEntry[] MainEntry = new CameraFileEntry[ChildCount];
                for (int i = 0; i < ChildCount; i++)
                {
                    MainEntry[i] = GetChild(ptr, i);

                    if (MainEntry[i].IsFolder)
                    {
                        CameraFileEntry[] retval = GetChildren(MainEntry[i].Reference);
                        if (retval != null) MainEntry[i].Entries = retval;
                    }
                }
                return MainEntry;
            }
            else return null;
        }

        /// <summary>
        /// Takes the picture.
        /// </summary>
        public void TakePicture()
        {
            
            EnsureOpenSession();
            
            if (IsLegacy && !IsLocked)
            {
                LockAndExceute(TakePicture);
                return;
            }

                Util.Assert(SendCommand(Edsdk.CameraCommand_TakePicture),
                    "Failed to take picture.");
        }

        public void PressShutterButton()
        {

            EnsureOpenSession();

            if (IsLegacy && !IsLocked)
            {
                LockAndExceute(PressShutterButton);
                return;
            }

            Util.Assert(SendCommand(Edsdk.CameraCommand_PressShutterButton, 3),
                "Failed to take picture.");
        }

        public void ReleaseShutterButton()
        {

            EnsureOpenSession();

            if (IsLegacy && !IsLocked)
            {
                LockAndExceute(ReleaseShutterButton);
                return;
            }

            Util.Assert(SendCommand(Edsdk.CameraCommand_PressShutterButton),
                "Failed to take picture.");
        }

        public void StartRecord()
        {
            uint record_start = 4; // Begin movie shooting
            SetPropertyIntegerData(Edsdk.PropID_Record, record_start);
        }

        public void StopRecord()
        {
            uint record_start = 0; // Begin movie shooting
            SetPropertyIntegerData(Edsdk.PropID_Record, record_start);
        }

        public override string ToString()
        {
            return DeviceDescription ?? string.Empty;
        }
        
        private void Unlock()
        {
            if (IsLocked)
            {
                Edsdk.EdsSendStatusCommand(Handle, Edsdk.CameraState_UIUnLock, 0);
                IsLocked = false;
            }
        }                        
    }
}
