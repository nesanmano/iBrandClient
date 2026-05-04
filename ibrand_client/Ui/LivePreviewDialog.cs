using System;
using System.Drawing;
using System.Windows.Forms;
using Canon.Eos.Framework.Eventing;
using NomadLogic.Common.Extentions;
using NomadLogic.Common.Util;

namespace NomadLogic.iBrandDesktop.Ui
{
    public partial class LivePreviewDialog : Form
    {
        readonly PictureBox _panelPreview;
        
        public LivePreviewDialog()
        {
            InitializeComponent();

            _panelPreview = new PictureBox
            {
                Location = new Point(0, 0),
                Size = Size,
                BackColor = Color.White
            };

            Controls.Add(_panelPreview);

            var canonManager = CanonCameraManager.GetInstance();
            canonManager.LiveViewUpdate += HandlePictureUpdate;
            canonManager.StartLiveView();
        }

        private void HandlePictureUpdate(object sender, EosImageEventArgs e)
        {
            if (!IsDisposed)
                UpdatePicture(e.GetImage());
        }

        private void UpdatePicture(Image image)
        {
            try
            {
                if (InvokeRequired)
                {
                    Invoke(new Action(() => UpdatePicture(image)));
                }
                else
                {
                    var bmp = new Bitmap(image);
                    if (bmp.Width != 720)
                        bmp = bmp.Resize(720, 480);

                    var rotBitmap = bmp.RotateToNewSize(-90);
                    var finalBitmap = rotBitmap.Crop(_panelPreview.Width, _panelPreview.Height);
                    
                    _panelPreview.Image = finalBitmap;
                    rotBitmap.Dispose();
                }
            }
            catch (System.Exception)
            {
                //Do nothing
            }
        }

        protected override void OnClosed(EventArgs e)
        {
            base.OnClosed(e);

            var canonManager = CanonCameraManager.GetInstance();
            canonManager.LiveViewUpdate -= HandlePictureUpdate;
            canonManager.StopLiveView();
        }
    }
}
