using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Windows.Forms;

namespace NomadLogic.Common.Ui.CameraRing
{
    public delegate void CameraViewClick(string cameraNo); 
    
    public partial class CameraRingConfigurationDlg : Form
    {
        public CameraViewClick _on_camera_view_click;
        private readonly int _maxCamerasShow = 3;
        
        List<CameraView> _cameraViewList;
        
        public CameraRingConfigurationDlg(string dirName)
        {
            InitializeComponent();
            AddCameraViews(dirName);
        }

        private void AddCameraViews(string dir)
        {
            var di = new DirectoryInfo(dir);
            var files = di.GetFileSystemInfos();

            if (!files.Any()) return;
            _cameraViewList = new List<CameraView>(files.Length);
                
            var orderedFiles = files.OrderBy(f => f.Name);

            foreach (var cameraView in orderedFiles.Select(file => new CameraView(file.FullName)))
            {
                cameraView.OnPictureClick += OnCameraClick;
                    
                if (cameraView.CameraNoInt != -1)
                {

                    var i = 0;
                    for (; i < _cameraViewList.Count; i++)
                    {
                        var cv = _cameraViewList[i];
                        if (cv.CameraNoInt > cameraView.CameraNoInt)
                            break;
                    }

                    _cameraViewList.Insert(i, cameraView);
                }
                else
                {
                    _cameraViewList.Insert(_cameraViewList.Count - 1, cameraView);
                }
            }

            const int spaceBetweenCam = 5;

            pictures_panel.Width = _cameraViewList[0].Width * _cameraViewList.Count + ((_cameraViewList.Count - 1) * spaceBetweenCam);
            pictures_panel.Height = _cameraViewList[0].Height;
            pictures_panel.Location = new Point(0, 0);

            var viewWidth = _maxCamerasShow * _cameraViewList[0].Width + (_cameraViewList.Count - 1) * spaceBetweenCam;
            var viewHeight = pictures_panel.Height;

            if (viewWidth > pictures_panel.Width)
            {
                viewWidth = pictures_panel.Width;
            }
            else
            {
                viewHeight += SystemInformation.HorizontalScrollBarHeight;
            }

            ClientSize = new Size(viewWidth, viewHeight);

            const int y = 2;
            var x = 0;

            foreach (var view in _cameraViewList)
            {
                view.Location = new Point(x, y);

                pictures_panel.Controls.Add(view);

                x += view.Width + spaceBetweenCam;
            }
        }

        public void OnCameraClick(string cameraNo)
        {
            _on_camera_view_click?.Invoke(cameraNo);
        }
    }
}
