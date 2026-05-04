using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Windows.Forms;
using NLog;
using NomadLogic.iBrand.Common;
using NomadLogic.iBrand.Common.Services;
using NomadLogic.iBrand.Common.Util;
using NomadLogic.iBrand.Ui.CameraRing;
using NomadLogic.iBrand.Ui.Dialog;
using NomadLogic.iBrand.Util;

namespace NomadLogic.iBrand.Ui.Choice.CameraRing
{
    public partial class CameraRingNikonTextBox : CameraRingTextBox
    {
        private static readonly Logger log = LogManager.GetCurrentClassLogger();
        protected delegate void ShowMessageDelegate(String message);
        protected delegate void EnabledGetPicturesButtonDelegate(bool enable);

        private readonly char[] _letters = { 'a', 'b', 'c', 'd', 'e', 'f', 'g', 'h', 'i', 'j', 'k', 'l', 'm', 'n', 'o', 'p', 'q', 'r', 's', 't', 'u', 'v', 'w', 'x', 'y', 'z' };

        private String _picturesOrder;

        private String _lastGetPicturesDir;

        public CameraRingNikonTextBox()
        {
            InitializeComponent();

            textBox.Enabled = false;

            connectButton.Click += OnConnectButtonClick;
            getPicturesButton.Click += OnGetPicturesButtonClick;
            orderPicturesButton.Click += OnOrderPicturesButtonClick;
            previewButton.Click += OnPreviewButtonClick;

            getPicturesButton.Active = false;
            orderPicturesButton.Active = false;
            previewButton.Active = false;

            Configuration configuration = (Configuration)ServicesContainer.GetService(ServicesNames.CONFIGURATION);
            _picturesOrder = configuration.PicturesOrder;
        }

        public void OnConnectButtonClick(object sender, EventArgs args)
        {
            ConnectToCameras();
        }

        public void OnOrderPicturesButtonClick(object sender, EventArgs args)
        {
            try
            {
                if (_lastGetPicturesDir != null)
                {
                    //@"C:\dev\CSharp\iBrand_Client_C_Sharp_Desktop\ibrand_client\bin\Debug\pictures_camera_ring\13f508301871"
                    Cursor.Current = Cursors.WaitCursor;
                    OrderPicturesDlg orderPictureDlg = new OrderPicturesDlg(_lastGetPicturesDir, _picturesOrder);
                    Cursor.Current = Cursors.Default;

                    if (orderPictureDlg.ShowDialog() == DialogResult.OK)
                    {
                        _picturesOrder = orderPictureDlg.PicturesOrder;
                        Configuration configuration = (Configuration)ServicesContainer.GetService(ServicesNames.CONFIGURATION);
                        configuration.PicturesOrder = _picturesOrder;
                        configuration.SaveToFile();
                    }
                }
            }
            catch (System.Exception ex)
            {
                log.ErrorException("Error order pictures.", ex);
                log.Fatal(ex.Message);
            }
        }

        public void OnPreviewButtonClick(object sender, EventArgs args)
        {
            if (_dir_name == null)
            {
                MsgBox.Show(WorkingArea.Container, "No pictures found.", "iBrand (tm)", MessageBoxButtons.OK, MessageBoxIcon.Error, MessageBoxDefaultButton.Button1);
                return;
            }

            new MultiplePicturesPreviewDlg(_dir_name).ShowDialog(WorkingArea.Container);
        }

        private void ConnectToCameras()
        {
            try
            {
                Cursor.Current = Cursors.WaitCursor;

                getPicturesButton.Active = false;
                orderPicturesButton.Active = false;
                previewButton.Active = false;

                int nbrCamera;
                int nbrCameraConnected;

                NikonD5000.GetNumberOfCameras(out nbrCamera);

                if (nbrCamera <= 0)
                {
                    MsgBox.Show(WorkingArea.Container, "No Camera found.", "iBrand (tm)", MessageBoxButtons.OK, MessageBoxIcon.Error, MessageBoxDefaultButton.Button1);
                    ShowMessage("No Camera Connected");
                    return;
                }

                NikonD5000.ConnectCameras(nbrCamera, out nbrCameraConnected);

                ShowMessage("Number of Connected Camera(s) :" + nbrCameraConnected);

                if (nbrCameraConnected > 0)
                {
                    EnabledGetPicturesButton(true);
                }

            }
            catch (System.Exception ex)
            {
                log.ErrorException("Error connect to camera", ex);
                log.Fatal(ex.Message);
            }
            finally
            {
                Cursor.Current = Cursors.Default;
            }
        }

        public override void DisconnectCameras()
        {
            NikonD5000.DisconnectCameras();
        }

        private void EnabledGetPicturesButton(bool enable)
        {
            if (getPicturesButton.InvokeRequired)
            {
                getPicturesButton.Invoke(new EnabledGetPicturesButtonDelegate(EnabledGetPicturesButton), enable);
                return;
            }

            getPicturesButton.Active = enable;
        }

        private void ShowMessage(String message)
        {

            if (textBox.InvokeRequired)
            {
                // we were called on a worker thread
                // marshal the call to the user interface thread
                textBox.Invoke(new ShowMessageDelegate(ShowMessage), message);
                return;
            }

            textBox.Text = message;
            textBox.Invalidate();
        }

        public void OnGetPicturesButtonClick(object sender, EventArgs args)
        {
            Cursor.Current = Cursors.WaitCursor;

            int nbrPictureRetrieved;
            NikonD5000.GetPictures(out nbrPictureRetrieved);

            textBox.Text = "Number of Picture(s) Retrieved :" + nbrPictureRetrieved;

            if (nbrPictureRetrieved > 0)
                SavePictures(nbrPictureRetrieved);

            Cursor.Current = Cursors.Default;
        }

        private void SavePictures(int nbrPictures)
        {
            try
            {
                String[] pictures;

                if (_lastGetPicturesDir != null)
                {
                    if (Directory.Exists(_lastGetPicturesDir))
                    {
                        pictures = Directory.GetFiles(_lastGetPicturesDir);

                        for (int i = 0; i < pictures.Length; i++)
                        {
                            File.Delete(pictures[i]);
                        }

                        Directory.Delete(_lastGetPicturesDir);
                    }
                    _lastGetPicturesDir = null;
                }

                Configuration configuration = (Configuration)ServicesContainer.GetService(ServicesNames.CONFIGURATION);

                pictures = Directory.GetFiles(DirectoryUtil.GetLocalMediaDir(DirectoryUtil.NomadLogicMediaDirectoriesEnum.pictures_nikon));

                if (pictures.Length > nbrPictures)
                {
                    DeleteOlderPictures(DirectoryUtil.GetLocalMediaDir(DirectoryUtil.NomadLogicMediaDirectoriesEnum.pictures_nikon), nbrPictures);
                    pictures = Directory.GetFiles(DirectoryUtil.GetLocalMediaDir(DirectoryUtil.NomadLogicMediaDirectoriesEnum.pictures_nikon));
                }

                String pictureBundleDir = GenerateUniqueID.GetID();

                String tempDirName = DirectoryUtil.GetLocalMediaDir(DirectoryUtil.NomadLogicMediaDirectoriesEnum.camera_ring) + pictureBundleDir;
                Directory.CreateDirectory(tempDirName);

                //Save pictures...
                for (int i = 0; i < pictures.Length; i++)
                {
                    String pictureName = GetNewPictureName(StringUtil.GetFileName(pictures[i]));
                    File.Move(pictures[i], Path.Combine(tempDirName, pictureName));
                }

                _dir_name = tempDirName;

                _lastGetPicturesDir = tempDirName;

                orderPicturesButton.Active = true;
                previewButton.Active = true;
            }
            catch (System.Exception ex)
            {
                MsgBox.Show(WorkingArea.Container, "Error : Could not save pictures", "iBrand (tm)", MessageBoxButtons.OK, MessageBoxIcon.Error, MessageBoxDefaultButton.Button1);
                log.ErrorException("Error connect to camera", ex);
                log.Fatal(ex.Message);
            }
        }

        private String GetNewPictureName(String pictureName)
        {
            if (_picturesOrder == null)
                return pictureName;

            int indexUnderscore = pictureName.LastIndexOf('_');
            int indexDot = pictureName.LastIndexOf('.');

            if (indexUnderscore == -1)
                return pictureName;

            String cameraNo = pictureName.Substring(indexUnderscore + 1, indexDot - indexUnderscore - 1);

            String[] order = _picturesOrder.Split(';');

            for (int i = 0; i < order.Length; i++)
            {
                if (order[i].Equals(cameraNo))
                {
                    if (i < _letters.Length)
                        return _letters[i] + "_" + pictureName;
                    return i + "_" + pictureName;
                }
            }
            return pictureName;
        }


        protected void DeleteOlderPictures(String dir, int nbrPictureToKeep)
        {
            List<String> orderedFiles = new List<String>();

            String[] pictures = Directory.GetFiles(DirectoryUtil.GetLocalMediaDir(DirectoryUtil.NomadLogicMediaDirectoriesEnum.pictures_nikon));

            int nbrFile = pictures.Length;

            int y = 0;

            for (int i = 0; i < nbrFile; i++)
            {
                String file = pictures[i];
                for (y = 0; y < i; y++)
                {
                    String orderedFile = orderedFiles[y];
                    if (File.GetCreationTime(file) > Directory.GetCreationTime(orderedFile))
                        break;
                }
                orderedFiles.Insert(y, file);
            }

            for (int i = nbrPictureToKeep; i < orderedFiles.Count; i++)
            {
                String fileName = orderedFiles[i];
                File.Delete(fileName);
            }
        }

        public override void SetSize(Size size)
        {
            Size = size;

            int space = 1;

            textBox.Width = size.Width - connectButton.Width - getPicturesButton.Width - orderPicturesButton.Width - previewButton.Width - 3 * space - textBox.Location.X - 1;

            Point initialLocation = connectButton.Location;
            connectButton.Location = new Point(textBox.Width + textBox.Location.X + space, initialLocation.Y);

            getPicturesButton.Location = new Point(textBox.Width + connectButton.Width + textBox.Location.X + 2*space, initialLocation.Y);

            orderPicturesButton.Location = new Point(textBox.Width + connectButton.Width + getPicturesButton.Width + textBox.Location.X + 3 * space, initialLocation.Y);

            previewButton.Location = new Point(textBox.Width + connectButton.Width + getPicturesButton.Width + orderPicturesButton.Width + textBox.Location.X + 4 * space, initialLocation.Y);
        }

        public override void Reset()
        {
            textBox.Text = "";
            _dir_name = null;
            if (_lastGetPicturesDir != null)
            {
                //PasteBkgAndMovePictures pbmp = new PasteBkgAndMovePictures(last_get_pictures_dir, "C:\\NomadLogic\\Pictures\\");
                //new Thread(pbmp.DoWork).Start();
                _lastGetPicturesDir = null;
            }
        }

        public new bool Enabled
        {
            get
            {
                return base.Enabled;
            }

            set
            {
                textBox.Enabled = false;
                getPicturesButton.Active = value;
                base.Enabled = value;

                textBox.BackColor = Color.LightGray;
            }
        }
    }

    public class PasteBkgAndMovePictures
    {
        private static readonly Logger log = LogManager.GetCurrentClassLogger();
        readonly String _sourceDir;
        readonly String _destinationDir;

        public PasteBkgAndMovePictures(String sourceDir, String destinationDir)
        {
            _sourceDir = sourceDir;
            _destinationDir = destinationDir;
        }

        public void DoWork()
        {
            try
            {
                if (!Directory.Exists(_destinationDir))
                    Directory.CreateDirectory(_destinationDir);

                if (File.Exists(Constants.APP_DIR + "frame.jpg"))
                {
                    String tempDir = GenerateUniqueID.GetID();
                    Directory.CreateDirectory(tempDir);

                    //Paste Background.
                    String[] pictures = Directory.GetFiles(_sourceDir);

                    for (int i = 0; i < pictures.Length; i++)
                    {
                        Image background = Image.FromFile(Constants.APP_DIR + "frame.jpg");
                        Graphics g = Graphics.FromImage(background);

                        Image image = Image.FromFile(pictures[i]);
                        g.DrawImage(image, 349, 561, image.Width, image.Height);

                        String fileName = StringUtil.GetFileName(pictures[i]);

                        background.Save(tempDir + Path.DirectorySeparatorChar + fileName);

                        g.Dispose();
                        image.Dispose();
                        background.Dispose();

                        File.Delete(pictures[i]);
                    }

                    Directory.Move(tempDir, _destinationDir + StringUtil.GetFileName(_sourceDir));
                    Directory.Delete(_sourceDir);
                }
                else
                {
                    Directory.Move(_sourceDir, _destinationDir + StringUtil.GetFileName(_sourceDir));
                }
            }
            catch (System.Exception ex)
            {
                log.Fatal(ex);
            }
        }
    }
}
