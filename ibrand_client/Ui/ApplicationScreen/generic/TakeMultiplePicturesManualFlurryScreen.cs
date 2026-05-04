using System;
using System.Drawing;
using System.IO;
using System.Windows.Forms;
using NLog;
using NomadLogic.Common;
using NomadLogic.Common.Respondent;
using NomadLogic.Common.Util;
using NomadLogic.iBrandDesktop.Ui.ApplicationScreen.Generic.Helpers;
using Label = System.Windows.Forms.Label;

namespace NomadLogic.iBrandDesktop.Ui.ApplicationScreen.Generic
{
    class TakeMultiplePicturesManualFlurryScreen : TakePictureScreenAbstract,IDisposable
    {
        // ReSharper disable once UnusedMember.Local
        private static readonly Logger _log = LogManager.GetCurrentClassLogger();

        /// Zone pour déclencher la prise de photo.
        private readonly Rectangle? _takePictureRectangle;

        /// Zone pour déclencher la prise de photo.
        private readonly Rectangle? _stopPictureRectangle;

        /// Zone pour déclencher la prise de photo.
        private readonly Rectangle? _resetPictureRectangle;

        public bool AutomaticTransition { get; set; }

        /// Zone pour revenir en arrière.
        private readonly Rectangle? _backRectangle;

        private readonly Rectangle? _nextRectangle;

        private readonly Rectangle? _cancelRectangle;

        private readonly Rectangle? _imageCollectedRectangle;

        private Bitmap _validateBtn;

        private Bitmap _retakeBtn;

        private Bitmap _nextBtn;

        private int _countCollected;

        /// Empêche l'activation de la caméra quand on prend une image ou que l'on fait une transformation.
        protected bool _blockInputs;

        //private IList<string> _listPhotoFinal;

        private bool _readyToReceivePhotos;

        private bool _stopped;

        private bool _readyForNext;

        private bool _inactif;


        private Label _labelCounter;

        public FontDefinitionInfo fontDefinition { set; get; }

        private string _bundleFolder;

        private string _newPath;

        public string FlurryCameraConfigFileName { get; set; }

        private ManualFlurryConfigLoader manualFlurry;


        /// <summary>
        /// Construteur par défaut.
        /// </summary>
        /// <param name="sc">Paramètres d'écrans globaux.</param>
        /// <param name="backgroundImgEnSrc">Fond d'image anglais.</param>
        /// <param name="backgroundImgFrSrc">Fond d'image français.</param>
        /// <param name="previewRectangle">Rectangle pour la zone de prévisualisation. Si null, on n'utilise pas le preview.</param>
        /// <param name="takePictureRectangle">Zone pour provoquer la prise de photo.</param>
        /// <param name="backRectangleRectangle">ZOne pour revenir en arrière.</param>
        public TakeMultiplePicturesManualFlurryScreen(ScreenConfig sc, string backgroundImgEnSrc, string backgroundImgFrSrc,
            Rectangle? previewRectangle = null, Rectangle? takePictureRectangle = null, Rectangle? stopPictureRectangle = null, Rectangle? resetPictureRectangle = null,
            Rectangle? backRectangleRectangle = null,Rectangle? nextRectangle = null, Rectangle? cancelRectangle = null, Rectangle? imgCollectedRectangle = null)
            : base(backgroundImgEnSrc, backgroundImgFrSrc, sc, previewRectangle)
        {
            if (sc.QuestionPhotoBundleNo == null || sc.QuestionPhotoBundleNo <= 0)
            {
                throw new System.Exception("Invalid screen configuration for flurry picture set. You must specify a question bundle identifier.");
            }

            _takePictureRectangle = takePictureRectangle;
            _stopPictureRectangle = stopPictureRectangle;
            _resetPictureRectangle = resetPictureRectangle;
            _backRectangle = backRectangleRectangle;
            _nextRectangle = nextRectangle;
            _cancelRectangle = cancelRectangle;
            _imageCollectedRectangle = imgCollectedRectangle;

        }


        /// <inheritdoc/>
        protected override bool OnMouseDownHandler(int x, int y)
        {
            if (!base.OnMouseDownHandler(x, y)) return false;
            if (_blockInputs) return false;


            if (InArea(x, y, _stopPictureRectangle))
            {
                _readyToReceivePhotos = false;
                
                if (!_stopped)
                    _stopped = true;

                _retakeBtn = GetBitmap("retake.png");
                
                _validateBtn = null;

                
                if (_countCollected >= _nbrPicturesToTake)
                {
                    
                    _readyForNext = true;
                    _nextBtn = GetBitmap("next.png");
                    
                    // ReSharper disable once PossibleInvalidOperationException
                    SaveAnswerToQuestion(new Answer
                    {
                        QuestionNo = _screenConfig.QuestionPhotoBundleNo.Value, 
                        Response = _newPath, 
                        QuestionType = QuestionTypeEnum.PhotoBundle
                    });

                }

                Refresh();

                if (AutomaticTransition && _readyForNext)
                {
                    _stopped = false;
                    Next();
                    _activated = false;
                }

            }
            if (InArea(x, y, _backRectangle))
            {
                _readyToReceivePhotos = false;
                _picturesTaken.Clear();
                
                _stopped = false;
                Back();
                _activated = false;
                _countCollected = 0;
                return false;

            }

            if (InArea(x, y, _resetPictureRectangle) && _retakeBtn!=null)
            {
                _readyToReceivePhotos = true;
                _countCollected = 0;
                Refresh();
                _picturesTaken.Clear();
                
                _stopped = false;

                _retakeBtn = null;
                _nextBtn = null;
                _validateBtn = GetBitmap("validate.png");

                Refresh();
            }

            if (InArea(x, y, _nextRectangle))
            {
                
                if (_readyForNext)
                {
                    _stopped = false;
                    Next();
                    _activated = false;
                }
                else if(!_stopped)
                {
                    MessageBox.Show("Cannot perform next from current state: Listening.", @"iBrand(tm)", MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
                else if (_stopped &&!(_picturesTaken.Count >= _nbrPicturesToTake))
                {
                    MessageBox.Show("Cannot perform next from current state: stopped and not enough.", @"iBrand(tm)", MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
            }

            if (InArea(x, y, _cancelRectangle))
            {
                _readyToReceivePhotos = false;
                _picturesTaken.Clear();
                
                _stopped = false;
                _activated = false;
                _countCollected = 0;
                Back(2);
                return false;
            }

            return true;
        }


        /// <inheritdoc/>
        protected override void OnFileSavedOnHostDo(string filePath)
        {

            DirectoryInfo file = new DirectoryInfo(_newPath);
            if (!file.Exists)
                file.Create();

            try
            {
                if (!_activated) return;
                if (!_stopped && _readyToReceivePhotos)
                {
                    //_picturesTaken.Add(filePath);
                    _countCollected++;
                    var tmp = _newPath + "\\DSC" + (_countCollected).ToString("D4") + "_0013.jpg";
                    if (new FileInfo(tmp).Exists)
                    {
                        new FileInfo(tmp).Delete();
                    }
                    File.Move(filePath, tmp);
                    
                    Refresh();
                }
            }
            catch (System.Exception ex)
            {
                MessageBox.Show(ex.Message, @"iBrand(tm)", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }


        protected override void OnPaintEvent(Graphics g)
        {
            base.OnPaintEvent(g);

            if (_imageCollectedRectangle != null)
            {

                _labelCounter.Text = _countCollected.ToString();
                _labelCounter.BringToFront();

                DrawImage(g, _validateBtn, _stopPictureRectangle);

                DrawImage(g, _nextBtn, _nextRectangle);

                DrawImage(g, _retakeBtn, _resetPictureRectangle);
            }

        }

        protected override void SetActivatedCustom()
        {

            if (FlurryCameraConfigFileName != null)
            {
                ManualFlurryConfigLoader flurryConfigLoader= new ManualFlurryConfigLoader();

                flurryConfigLoader.LoadConfig(FlurryCameraConfigFileName);

                _nbrPicturesToTake = flurryConfigLoader.GetMinShotsRequired();

            }
            else
            {
                _nbrPicturesToTake = 10;
            }

            // determine l'emplacement ou les photos prise depuis cet ecran seront sauvegarde.
            _bundleFolder = GenerateUniqueId.GetId();

            _newPath = GetTerminalDirectoryDataFile(_bundleFolder,
                           DirectoryUtil.ProjectDataDirectoriesEnum.pictures,
                           DirectoryUtil.ProjectDataDirectoriesModEnum.bundle);
           

            _labelCounter = new Label
            {
                Location = new Point(554, 327),
                Size = new Size(576,410),
                Font = new Font(fontDefinition.FamilyName,fontDefinition.Size,FontStyle.Bold,fontDefinition.GraphicsUnit),
                BackColor = Color.Transparent,
                ForeColor = Color.White,
                TextAlign = ContentAlignment.MiddleCenter

            };
            _labelCounter.Parent = PictureBox;
            _labelCounter.Text = _countCollected.ToString();
            _labelCounter.BringToFront();

            _countCollected = 0;
            _readyForNext = false;
            _blockInputs = false;

            _readyToReceivePhotos = true; // 1er etat. On y est par defaut!
            _validateBtn = GetBitmap("validate.png");
            _nextBtn = null;
            _retakeBtn = null;
            Refresh();
        }

        protected override void OnUpdatePictureDo(System.Drawing.Image image, bool force)
        {
            throw new NotImplementedException();
        }

        //
        private int _nbrPicturesToTake;

        public void Dispose()
        {
            _labelCounter?.Dispose();
        }

    }
}
