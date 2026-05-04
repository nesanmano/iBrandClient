using System;
using System.Threading;
using Canon.Eos.Framework.Eventing;
using Canon.Eos.Framework.Helper;
using Canon.Eos.Framework.Internal;
using Canon.Eos.Framework.Internal.SDK;
using Canon.Eos.Framework.Threading;
using NLog;

namespace Canon.Eos.Framework
{
    partial class EosCamera
    {
        private static Logger log = LogManager.GetCurrentClassLogger();
        private bool _liveMode;
        private static readonly string EVF_IMAGE_FAIL = "Failed to download evf image.";

        private void OnLiveViewStarted(EventArgs eventArgs)
        {
            LiveViewStarted?.Invoke(this, eventArgs);
        }

        private void OnLiveViewStopped(EventArgs eventArgs)
        {
            LiveViewStopped?.Invoke(this, eventArgs);
        }

        private void OnLiveViewUpdate(EosLiveImageEventArgs eventArgs)
        {
            LiveViewUpdate?.Invoke(this, eventArgs);
        }

        private bool DownloadEvf()
        {
            var memoryStream = IntPtr.Zero;

            try
            {
                if ((LiveViewDevice & EosLiveViewDevice.Host) == EosLiveViewDevice.None || !_liveMode)
                    return false;

                Util.Assert(Edsdk.EdsCreateMemoryStream(0, out memoryStream), "Failed to create memory stream.");

                try
                {
                    using (var image = EosLiveImage.CreateFromStream(memoryStream))
                    {
                        Util.Assert(Edsdk.EdsDownloadEvfImage(Handle, image.Handle), EVF_IMAGE_FAIL);
                        var converter = new EosConverter();
                        OnLiveViewUpdate(new EosLiveImageEventArgs(converter.ConvertImageStreamToBytes(memoryStream)));
                    }
                }
                catch (EosException)
                {
                    // On ne log pas les erreurs de ce bout de code, car il "fail" de façon normale.
                }
            }
            catch (EosException eosEx)
            {
                log.Fatal(eosEx);
                return false;
            }
            finally
            {
                if (memoryStream != IntPtr.Zero)
                    Edsdk.EdsRelease(memoryStream);
            }

            return true;
        }

        private void StartDownloadEvfInBackGround()
        {
            var backgroundWorker = new BackgroundWorker();
            backgroundWorker.Work(() =>
            {
                while (DownloadEvf())
                    Thread.Sleep(WAIT_TIMEOUT_FOR_NEXT_LIVE_DOWNLOAD);
            });
        }

        private void OnPropertyEventPropertyEvfOutputDeviceChanged(uint param, IntPtr context)
        {
            if (!_liveMode && (LiveViewDevice & EosLiveViewDevice.Host) != EosLiveViewDevice.None)
            {
                _liveMode = true;
                OnLiveViewStarted(EventArgs.Empty);
                StartDownloadEvfInBackGround();
            }
            else if (_liveMode && (LiveViewDevice & EosLiveViewDevice.Host) == EosLiveViewDevice.None)
            {
                _liveMode = false;
                OnLiveViewStopped(EventArgs.Empty);
            }
        }

        private void OnPropertyEventPropertyChanged(uint propertyId, uint param, IntPtr context)
        {
            EosFramework.LogInstance.Debug("OnPropertyEventPropertyChanged: " + propertyId);
            switch (propertyId)
            {
                case Edsdk.PropID_Evf_OutputDevice:
                    OnPropertyEventPropertyEvfOutputDeviceChanged(param, context);
                    break;
            }
        }

        private void OnPropertyEventPropertyDescChanged(uint propertyId, uint param, IntPtr context)
        {
            EosFramework.LogInstance.Debug("OnPropertyEventPropertyDescChanged: " + propertyId);
        }

        private uint HandlePropertyEvent(uint propertyEvent, uint propertyId, uint param, IntPtr context)
        {
            EosFramework.LogInstance.Debug("HandlePropertyEvent fired: " + propertyEvent + ", id: " + propertyId);
            switch (propertyEvent)
            {
                case Edsdk.PropertyEvent_PropertyChanged:
                    OnPropertyEventPropertyChanged(propertyId, param, context);
                    break;

                case Edsdk.PropertyEvent_PropertyDescChanged:
                    OnPropertyEventPropertyDescChanged(propertyId, param, context);
                    break;
            }
            return Edsdk.EDS_ERR_OK;
        }
    }
}
