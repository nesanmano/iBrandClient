using System;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Linq;
using System.Windows.Forms;
using Canon.Eos.Framework;
using Canon.Eos.Framework.Eventing;
using NLog;
using NomadLogic.Common.Services;
using NomadLogic.Common.Services.Configuration;
using NomadLogic.Common.Ui;
using NomadLogic.Common.Ui.Choice;
using NomadLogic.Common.Ui.Dialog;
using NomadLogic.Common.Util;
using NomadLogic.Common.Util.Print;
using NomadLogic.iBrandDesktop.Util;
using NomadLogic.iBrandDesktop.Util.SubmitAction.BaseClasses;
#pragma warning disable 618

namespace NomadLogic.iBrandDesktop.Ui.Choice
{
    public partial class CaptureTextBoxCanon : UserControl, IPreviewDialogTransformationReceiver
    {
        private static readonly Logger log = LogManager.GetCurrentClassLogger();

        private delegate void SetTextDelegate(string text);

        private delegate void SetCameraDelegate(bool enables);

        private string _fileName;

        private string _transformedFileName;

        private PreviewOnTv _previewOnTv;

        LivePreviewDialog _livePreviewDialog;

#pragma warning disable 618
        CanonCameraManager _canonManager;
#pragma warning restore 618

        public CaptureTextBoxCanon()
        {
            InitializeComponent();

            textBox.Enabled = false;

            previewButton.Enabled = false;

            printButton.Enabled = false;
            printButton.Visible = false;
            CanPrint = false;

            livePreviewButton.Enabled = false;
            livePreviewButton.Enabled = false;
            livePreviewButton.Visible = false;
            LivePreview = false;
        }

        public void Initialize()
        {
            previewButton.Click += OnPreviewButtonClick;
            captureButton.Click += OnCaptureButtonClick;
            printButton.Click += OnPrintButtonClick;
            livePreviewButton.Click += OnLivePreviewButtonClick;
            clearButton.Click += OnClearButtonClick;

            _canonManager = CanonCameraManager.GetInstance();
            _canonManager.FileCreatedOnCamera += OnFileCreatedOnCamera;
            _canonManager.FileSavedOnHost += OnFileSavedOnHost;
            _canonManager.CameraShutdown += OnCameraShutdown;
            _canonManager.CameraAdded += OnCameraAdded;

            SetCameraConnected(_canonManager.IsCameraConnected());
        }

        public void SetTransformedPicture(string transformedPicture)
        {
            _transformedFileName = transformedPicture;
            if (_transformedFileName != null)
            {
                SetTextBoxText(StringUtil.GetFileName(transformedPicture));
            }
        }

        private void OnCameraShutdown(object sender, EventArgs args)
        {
            SetCameraConnected(false);
        }

        private void OnCameraAdded(object sender, EventArgs args)
        {
            SetCameraConnected(true);
        }

        protected virtual void OnCaptureButtonClick(object sender, EventArgs args)
        {
            _canonManager.TakePicture();
        }

        private void OnClearButtonClick(object sender, EventArgs args)
        {
            Reset();
        }

        protected virtual void OnPreviewButtonClick(object sender, EventArgs args)
        {
            try
            {
                if (_fileName == null || !File.Exists(_fileName)) return;

                var configuration = ServicesContainer.GetService<NomadConfiguration>();
                Bitmap picture = null;
                // On dispose si on lit l'image du disque.  Sinon, elle est dans le pictureController
                var hasToDispose = false;
                Form previewDialog = null;

                var sa = QuestionnaireControllerAccessor.QuestionnaireController.Questionnaire.SubmitAction;
                if (sa is AbstractSubmitActionPictureTransformation)
                {
                    _transformedFileName = null;

                    Cursor.Current = Cursors.WaitCursor;
                    var resp = QuestionnaireControllerAccessor.QuestionnaireController.GetValidatedCurrentRespondent();
                    if (resp != null)
                    {
                        Cursor.Current = Cursors.Default;

                        var submitAction = sa as AbstractSubmitActionPictureTransformation;
                        var action = submitAction;

                        if (submitAction.PictureTransformationController != null)
                        {
                            previewDialog = new PreviewDialogTransformation(this, resp,
                                action.PictureTransformationController);
                        }
                        else
                        {
                            var tmp = submitAction.TransformPictures(resp, true);
                            if (!tmp?.Any() ?? true)
                            {
                                throw new System.Exception("No transformation selected. Check transformation setting file.");    
                            }

                            picture = tmp[0];
                            hasToDispose = true;
                            previewDialog = new PreviewDialog(picture);
                        }
                    }
                }
                else
                {
                    picture = new Bitmap(_fileName);
                    hasToDispose = true;
                    previewDialog = new PreviewDialog(picture);
                }

                if (previewDialog != null)
                {
                    if (configuration.Ibrand.ShowPreviewOnTv && picture != null)
                    {
                        if (Screen.AllScreens.Length > 1)
                        {
                            _previewOnTv = new PreviewOnTv();

                            _previewOnTv.ShowImage(picture);
                            _previewOnTv.Show();
                        }
                        else
                        {
                            MsgBox.Show(WorkingArea.Container, "No secondary screen detected.", "iBrand(tm)", MessageBoxButtons.OK, MessageBoxIcon.Error);
                        }
                    }
                    
                        previewDialog.Owner = WorkingArea.Container;

                        // This ensures that the preview is shown on the primary screen
                        var screens = Screen.AllScreens;
                        SetFormLocation(previewDialog, screens[0]);

                        previewDialog.WindowState = FormWindowState.Maximized;

                        previewDialog.ShowDialog();

                    if (hasToDispose)
                        picture?.Dispose();

                    if (_previewOnTv != null)
                    {
                        _previewOnTv.Dispose();
                        _previewOnTv = null;
                    }
                }

                previewDialog?.Dispose();
            }
            catch (System.Exception ex)
            {
                log.Fatal(ex);
            }
        }

        private void OnLivePreviewButtonClick(object sender, EventArgs args)
        {
            if (!CanonCameraManager.GetInstance().IsCameraConnected())
            {
                MessageBox.Show(this, @"Camera is not connected.", @"iBrand(tm)", MessageBoxButtons.OK, MessageBoxIcon.Error, MessageBoxDefaultButton.Button1);
                return;
            }

            if (_livePreviewDialog != null) return;

            _livePreviewDialog = new LivePreviewDialog();
            _livePreviewDialog.Disposed += OnLivePreviewDialogDisposed;
            _livePreviewDialog.Show();
        }


        private void OnLivePreviewDialogDisposed(object sender, EventArgs args)
        {
            _livePreviewDialog = null;
        }

        private void OnPrintButtonClick(object sender, EventArgs args)
        {
            if (_fileName == null || !File.Exists(_fileName)) return;

            try
            {
                string toPrint = null;

                if (QuestionnaireControllerAccessor.QuestionnaireController.Questionnaire.SubmitAction != null)
                {
                    if (_transformedFileName == null)
                    {
                        Cursor.Current = Cursors.WaitCursor;
                        var resp = QuestionnaireControllerAccessor.QuestionnaireController.GetValidatedCurrentRespondent();
                        if (resp != null)
                        {
                            Image picture = null;
                            if (PictureController.GetInstance().PictureTransformed)
                            {
                                picture = PictureController.GetInstance().TransformedPicture;
                            }
                            else
                            {
                                if (QuestionnaireControllerAccessor.QuestionnaireController.Questionnaire.SubmitAction is 
                                    AbstractSubmitActionPictureTransformation submitAction)
                                {
                                    var bmps = submitAction.TransformPictures(resp, false);

                                    if (bmps.Any())
                                    {
                                        picture = bmps[0];
                                        PictureController.GetInstance().TransformedPicture = bmps[0];
                                    }
                                }
                                else
                                {
                                    picture = FileUtil.LoadBitmap(_fileName);
                                }
                            }

                            if (picture != null)
                            {
                                toPrint = DirectoryUtil.CreateLocalTemporaryFile("tmp.jpg");
                                picture.Save(toPrint, ImageFormat.Jpeg);
                            }
                        }
                        Cursor.Current = Cursors.Default;
                    }
                    else
                    {
                        toPrint = _transformedFileName;
                    }
                }
                else
                {
                    toPrint = _fileName;
                }
                PrintUtil.Print(toPrint, null, this);
            }
            catch (System.Exception ex)
            {
                log.Fatal(ex.Message);
            }
        }

        /*
        private class ShowPictureOnTvThread
        {
            PreviewOnTv _previewOnTv;
            readonly Bitmap _picture;

            public ShowPictureOnTvThread(Bitmap picture)
            {
                _picture = picture;
            }

            public void ShowPictureOnTv()
            {
                if (Screen.AllScreens.Length > 1)
                {
                    _previewOnTv = new PreviewOnTv();

                    _previewOnTv.ShowImage(_picture);
                    _previewOnTv.Show();
                }
                else
                {
                    MsgBox.Show(WorkingArea.Container, "No secondary screen detected.", "iBrand(tm)", MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
            }
        }
        */

        protected virtual void OnFileCreatedOnCamera(object sender, IntPtr dirItem, EosCamera camera)
        {
            var fileName = DirectoryUtil.GetLocalMediaDir(DirectoryUtil.NomadLogicMediaDirectoriesEnum.pictures) + GenerateUniqueId.GetId() + ".jpg";

            if (File.Exists(fileName))
            {
                File.Delete(fileName);
            }

            camera.SavePicture(dirItem, fileName);
        }

        protected virtual void OnFileSavedOnHost(object sender, EosFileEventArgs e)
        {
            if (!Enabled) return;
            _fileName = e.FilePath;
            SetTextBoxText(StringUtil.GetFileName(e.FilePath));
            previewButton.Enabled = true;
            printButton.Enabled = true;
            PictureController.GetInstance().OnAnswerChanged();

            if (_livePreviewDialog != null)
            {
                CanonCameraManager.GetInstance().StartLiveView();
            }
        }

        protected void SetTextBoxText(string text)
        {
            if (textBox.InvokeRequired)
            {
                // we were called on a worker thread
                // marshal the call to the user interface thread
                Invoke(new SetTextDelegate(SetTextBoxText), text);
                return;
            }

            textBox.Text = text;

        }

        private void SetStatusTextBoxText(string text)
        {
            if (status_text_box.InvokeRequired)
            {
                // we were called on a worker thread
                // marshal the call to the user interface thread
                Invoke(new SetTextDelegate(SetStatusTextBoxText), text);
                return;
            }

            status_text_box.Text = text;

        }

        public void SetSize(Size size)
        {
            Size = size;

            textBox.Width = size.Width - status_text_box.Width - captureButton.Width - previewButton.Width - clearButton.Width - 20;

            if (CanPrint)
            {
                textBox.Width -= (printButton.Width + 5);
            }

            if (LivePreview)
            {
                textBox.Width -= (livePreviewButton.Width + 5);
            }

            var initialLocation = captureButton.Location;

            status_text_box.Location = new Point(textBox.Width + 5, textBox.Location.Y);

            captureButton.Location = new Point(textBox.Width + status_text_box.Width + 10, initialLocation.Y);

            previewButton.Location = new Point(textBox.Width + status_text_box.Width + captureButton.Width + 15, initialLocation.Y);

            clearButton.Location = new Point(textBox.Width + status_text_box.Width + captureButton.Width + previewButton.Width + 20, initialLocation.Y);

            var printButtonWidth = 0;

            if (CanPrint)
            {
                printButton.Location = new Point(textBox.Width + status_text_box.Width + captureButton.Width + previewButton.Width + clearButton.Width + 25, initialLocation.Y);
                printButton.Visible = true;
                printButtonWidth = previewButton.Width;
            }

            if (LivePreview)
            {
                livePreviewButton.Location = new Point(textBox.Width + status_text_box.Width + captureButton.Width + previewButton.Width + printButtonWidth + 10, initialLocation.Y);
                livePreviewButton.Visible = true;
                livePreviewButton.Enabled = true;
            }
        }

        private void SetCameraConnected(bool isConnected)
        {
            if (captureButton.InvokeRequired)
            {
                Invoke(new SetCameraDelegate(SetCameraConnected), isConnected);
                return;
            }

            if (isConnected)
            {
                SetStatusTextBoxText("Camera connected.");
                captureButton.Enabled = true;

            }
            else
            {
                SetStatusTextBoxText("No camera connected.");
                captureButton.Enabled = false;
            }
        }



        public virtual void Reset()
        {
            textBox.Text = "";
            _fileName = null;
            _transformedFileName = null;
            previewButton.Enabled = false;
            printButton.Enabled = false;
            PictureController.GetInstance().TransformedPicture = null;
        }

        private void SetFormLocation(Control form, Screen screen)
        {
            var bounds = screen.Bounds;
            form.SetBounds(bounds.X, bounds.Y, bounds.Width, bounds.Height);
        }

        public override string Text => _transformedFileName ?? _fileName;

        public new bool Enabled
        {
            protected internal get
            {
                return base.Enabled;
            }
            set
            {
                captureButton.Enabled = value;
                base.Enabled = value;
            }
        }

        public bool CanPrint { private get; set; }

        public bool LivePreview
        {
            private get;
            set;
        }

        public void DisposeCamrera()
        {
            if (_canonManager == null) return;

            _canonManager.FileCreatedOnCamera -= OnFileCreatedOnCamera;
            _canonManager.FileSavedOnHost -= OnFileSavedOnHost;
            _canonManager.CameraShutdown -= OnCameraShutdown;
            _canonManager.CameraAdded -= OnCameraAdded;
            _canonManager.ShutDown();
            _canonManager = null;
        }
    }
}
