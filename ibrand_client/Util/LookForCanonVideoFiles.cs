using System;
using System.IO;
using System.Threading;
using NomadLogic.Common.Util;

namespace NomadLogic.iBrandDesktop.Util
{
    public class LookForCanonVideoFiles
    {
        private Thread _lookForPictureThread;

        private bool _stopRequested;

        private volatile Mutex _mutex = new Mutex();

        public event FileFound OnVideoFileFound;

        private readonly SyncEvents _syncEvents;

        public LookForCanonVideoFiles(SyncEvents syncEvents)
        {
            _syncEvents = syncEvents;
        }

        public void StartThread()
        {
            if (_lookForPictureThread != null)
                StopThread();

            _stopRequested = false;
            _lookForPictureThread = new Thread(LookForVideoInDir);
            _lookForPictureThread.Start();
        }

        public bool Stop
        {
            set
            {
                _stopRequested = value;
            }
        }

        private void LookForVideoInDir()
        {
            _mutex.WaitOne();
            
            var videoDir = DirectoryUtil.GetLocalMediaDir(DirectoryUtil.NomadLogicMediaDirectoriesEnum.videos);

            var lastFileFound = "";

            while (!_stopRequested)
            {
                var di = new DirectoryInfo(videoDir);
                var fiArray = di.GetFiles();
                Array.Sort(fiArray, (x, y) => StringComparer.OrdinalIgnoreCase.Compare(x.LastWriteTime, y.LastWriteTime));

                if (fiArray.Length > 0)
                {
                    var fi = fiArray[fiArray.Length - 1];
                    var fileName = fi.FullName;

                    var temp = StringUtil.GetFileName(fileName);

                    if (!temp.Equals(lastFileFound))
                    {
                        lastFileFound = temp;
                        if (OnVideoFileFound != null)
                        {
                            OnVideoFileFound(fileName);
                            //Wait For File Selected
                            if (!_stopRequested)
                                _syncEvents.NewItemEvent.WaitOne();
                        }
                    }
                }

                Thread.Sleep(1000);
            }

            _mutex.ReleaseMutex();
        }

        public void StopThread()
        {
            _stopRequested = true;
            //Wait for thread to stop
            _mutex.WaitOne();
            _lookForPictureThread = null;
        }
    }
}
