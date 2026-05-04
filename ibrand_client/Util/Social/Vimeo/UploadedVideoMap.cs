using System.Collections.Generic;

namespace NomadLogic.iBrandDesktop.Util.Social.Vimeo
{
    public class UploadedVideoMap
    {
        private volatile Dictionary<string, string> _uploadedVideoMap = new Dictionary<string, string>();
        private volatile Dictionary<string, string> _thumnailsMap = new Dictionary<string, string>();

        private static UploadedVideoMap  _videoMap;

        private SyncEvents _syncEvents = new SyncEvents();

        private UploadedVideoMap()
        {

        }

        public static UploadedVideoMap GetUploadedVideoMap()
        {
            if (_videoMap == null)
            {
                _videoMap = new UploadedVideoMap();
            }

            return _videoMap;    
        }

        public bool IsAlreadyUploaded(string videoName)
        {
            return _uploadedVideoMap.ContainsKey(videoName);
        }

        public string GetVideoId(string videoName)
        {
            return _uploadedVideoMap[videoName];
        }

        public string GetThumnail(string videoName)
        {
            return _thumnailsMap[videoName];
        }

        public void Insert(string videoName, string videoId)
        {
            _uploadedVideoMap.Add(videoName, videoId);
        }

        public void InsertThumbnail(string videoName, string uploadedVideoMap)
        {
            if (!IsThumbnailAlreadyInserted(videoName))
                _thumnailsMap.Add(videoName, uploadedVideoMap);
        }

        public bool IsThumbnailAlreadyInserted(string videoName)
        {
            return _thumnailsMap.ContainsKey(videoName);
        }
    }
}
