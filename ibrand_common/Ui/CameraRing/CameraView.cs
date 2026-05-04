using System;
using System.Drawing;
using System.Drawing.Imaging;
using System.Windows.Forms;
using NomadLogic.Common.Ui.Dialog;
using NomadLogic.Common.Util;

namespace NomadLogic.Common.Ui.CameraRing
{
    public partial class CameraView : UserControl
    {
        public event CameraViewClick OnPictureClick;

        public CameraView(string pictureFileName)
        {
            InitializeComponent();
            
            CameraNo = StringUtil.RemoveExtension(StringUtil.GetFileName(pictureFileName));

            camera_name_label.Text = @"Camera : " + CameraNo;

            if (StringUtil.IsInteger(CameraNo))
                CameraNoInt = int.Parse(CameraNo);

            var image = new Bitmap(pictureFileName);
            
            var c = image.Width / picture_box.Width;
            
            var finalBitmap = new Bitmap(image.Width / c, image.Height / c, PixelFormat.Format24bppRgb);
            
            var g = Graphics.FromImage(finalBitmap);
            g.DrawImage(image, 0, 0, image.Width / c, image.Height / c);
            g.Dispose();
            
            image.Dispose();

            picture_box.Image = finalBitmap;

            picture_box.MouseEnter += (s, e) => picture_box.Cursor = Cursors.Hand;
            picture_box.MouseLeave += (s, e) => picture_box.Cursor = Cursors.Arrow;
        }

        /*protected override void OnVisibleChanged(EventArgs e)
        {
            base.OnVisibleChanged(e);

            if (Visible && !Disposing)
            {  
                try
                {
                    _camera.TakePicture();
                }
                catch (System.Exception ex)
                {
                    log.Error("Error while taking picture", ex.Message);
                }
            }
        }*/

        public string CameraNo { get; }

        public int CameraNoInt { get; } = -1;

        /* private void HandlePictureUpdate(object sender, EosImageEventArgs e)
        {
            this.UpdatePicture(e.GetImage());
        }

        private void UpdatePicture(Image image)
        {
            if (this.InvokeRequired)
                this.Invoke(new Action(() => this.UpdatePicture(image)));
            else
            {
                int c = image.Width / picture_box.Width;
                Bitmap final_bitmap = new Bitmap(image.Width / c, image.Height / c, PixelFormat.Format24bppRgb);
                Graphics g = Graphics.FromImage(final_bitmap);
                g.DrawImage(image, 0, 0, image.Width / c, image.Height / c);

                g.Dispose();
                picture_box.Image = final_bitmap;
            }
        }*/

        private void picture_box_Click(object sender, EventArgs e)
        {
            try
            {
                OnPictureClick?.Invoke(CameraNo);
                /*CameraSettingsDialog dlg = new CameraSettingsDialog(_camera);
                dlg.OnCopyCameraSettings += CopyCameraProperties;
                dlg.ShowDialog(WorkingArea.Container);
                dlg.OnCopyCameraSettings -= OnCopyCameraProperties;*/
            }
            catch (Exception ex)
            {
                MsgBox.Show(this, ex.Message, "iBrand (tm)", MessageBoxButtons.OK, MessageBoxIcon.Error, MessageBoxDefaultButton.Button1);
            }
        }

       /*public void CopyCameraProperties(List<AbstractCameraProperty> properties)
        {
            if (OnCopyCameraProperties != null)
                OnCopyCameraProperties(properties);
        }*/   

      
    }
}
