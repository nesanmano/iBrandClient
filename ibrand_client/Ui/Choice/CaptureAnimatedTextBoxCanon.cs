using System;
using System.Collections.Generic;
using System.IO;
using System.Windows.Forms;
using Canon.Eos.Framework;
using Canon.Eos.Framework.Eventing;
using NLog;
using NomadLogic.Common.Ui;
using NomadLogic.Common.Ui.Dialog;
using NomadLogic.Common.Util;
using NomadLogic.iBrandDesktop.Util;

namespace NomadLogic.iBrandDesktop.Ui.Choice
{
    public class CaptureAnimatedTextBoxCanon : CaptureTextBoxCanon
    {
        private static readonly Logger log = LogManager.GetCurrentClassLogger();

        private int _numberOfPictureTaken;

        private readonly List<string> _fileList = new List<string>();

        protected string _dir_name;

        public CaptureAnimatedTextBoxCanon()
        {
            previewButton.Enabled = true;
        }

        public override string Text => _dir_name;

        public bool IsAnswered => _numberOfPictureTaken >= 4;


        protected override void OnCaptureButtonClick(object sender, EventArgs args)
        {
            try
            {
                if (_numberOfPictureTaken < 4)
                {
                    TakePicture();
                }
            }
            catch (System.Exception ex)
            {
                MsgBox.Show(WorkingArea.Container, ex.Message, "iBrand(tm)", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        protected override void OnPreviewButtonClick(object sender, EventArgs args)
        {
            try
            {
                if (_dir_name == null)
                {
                    MsgBox.Show(WorkingArea.Container, "No pictures found.", "iBrand (tm)", MessageBoxButtons.OK, MessageBoxIcon.Error, MessageBoxDefaultButton.Button1);
                    return;
                }

                new MultiplePicturesPreviewDlg(_dir_name).ShowDialog(WorkingArea.Container);
            }
            catch (System.Exception ex)
            {
                log.Fatal(ex);
            }
        }

        private void TakePicture()
        {
            CanonCameraManager.GetInstance().TakePicture();
        }

        private readonly object _syncLock = new object();

        protected override void OnFileCreatedOnCamera(object sender, IntPtr dirItem, EosCamera camera)
        {
            try
            {
                lock (_syncLock)
                {
                    if (_numberOfPictureTaken >= 4) return;
                    if (_dir_name == null)
                    {
                        var picturesDir = GenerateUniqueId.GetId();

                        _dir_name = DirectoryUtil.GetLocalMediaDir(DirectoryUtil.NomadLogicMediaDirectoriesEnum.pictures) + picturesDir;

                        Directory.CreateDirectory(_dir_name);
                    }


                    var fileName = _dir_name + Path.DirectorySeparatorChar + _numberOfPictureTaken + ".jpg";

                    if (File.Exists(fileName))
                        File.Delete(fileName);

                    camera.SavePicture(dirItem, fileName);
                }
            }
            catch (System.Exception ex)
            {
                log.Error(ex);
                MsgBox.Show(WorkingArea.Container, ex.Message, "iBrand(tm)", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }

        }

        protected override void OnFileSavedOnHost(object sender, EosFileEventArgs e)
        {
            if (Enabled)
            {
                try
                {
                    _fileList.Add(e.FilePath);

                    _numberOfPictureTaken++;

                    SetTextBoxText("Number of Picture Taken : " + _numberOfPictureTaken);
                }
                catch (System.Exception ex)
                {
                    log.Error(ex);
                    MsgBox.Show(WorkingArea.Container, ex.Message, "iBrand(tm)", MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
            }
        }

        public override void Reset()
        {
            textBox.Text = "";
            _fileList.Clear();
            _dir_name = null;
            _numberOfPictureTaken = 0;
            previewButton.Enabled = true;
        }

    }
}
