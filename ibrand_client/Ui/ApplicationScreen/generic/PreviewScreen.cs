using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Imaging;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Forms;
using JetBrains.Annotations;
using NLog;
using NomadLogic.Common.Extentions;
using NomadLogic.Common.Respondent;
using NomadLogic.Common.Ui.Dialog;
using NomadLogic.Common.Util;
using NomadLogic.iBrandDesktop.Ui.ApplicationScreen.Generic.Helpers;
using NomadLogic.iBrandDesktop.Util;
using NomadLogic.iBrandDesktop.Util.SubmitAction.BaseClasses;
using NomadLogic.Multimedia;

namespace NomadLogic.iBrandDesktop.Ui.ApplicationScreen.Generic
{
    /// <summary>
    /// Écran de prévisualisation d'une image. Avec option de transformation.
    /// </summary>
    [PublicAPI]
    [Obsolete]
    public class PreviewScreen : PreviewScreenAbstract
    {
        // ReSharper disable once UnusedMember.Local
        private static readonly Logger log = LogManager.GetCurrentClassLogger();
        private PictureBox _picture;
        private Bitmap _previewHourglass;
        private readonly Bitmap _checkmark;
        private readonly List<ScreenCheckOption> _zones = new List<ScreenCheckOption>();
        /// <summary>
        /// Option d'image procédurale.
        /// </summary>
        protected ProceduralExtendedImageDefinition? _peid;

        public PreviewScreen(string backgroundImgEnSrc, string backgroundImageFrSrc, ScreenConfig sc,
            Rectangle? cancelZone, Rectangle? nextZone, Rectangle? backZone, Point previewLocation, Size previewSize)
            : base(backgroundImgEnSrc, backgroundImageFrSrc ?? backgroundImgEnSrc, sc, cancelZone, nextZone, backZone, previewLocation, previewSize)
        {
            // rien de plus
        }

        public PreviewScreen(string backgroundImgEnSrc, string backgroundImageFrSrc, ScreenConfig sc,
            Rectangle? cancelZone, Rectangle? nextZone, Rectangle? backZone, int fromTop, Size previewSize, IEnumerable<ScreenCheckOption> optInOptions = null)
            : base(backgroundImgEnSrc, backgroundImageFrSrc ?? backgroundImgEnSrc, sc, cancelZone, nextZone, backZone, fromTop, previewSize)
        {
            if (optInOptions == null) return;
            _zones.AddRange(optInOptions);
            if (_zones.Any())
            {
                _checkmark = GetNoneLanguageBitmap("checkmark.png");
            }
        }

        public ProceduralExtendedImageDefinition? ProceduralHourglassImageDefinition
        {
            get => _peid;

            set
            {
                _previewHourglass?.Dispose();
                CreateHourglassBmp(value);

                _peid = value;
            }
        }

        private void CreateHourglassBmp(ProceduralExtendedImageDefinition? peid)
        {
            _previewHourglass = peid == null ?
                GetNoneLanguageBitmap("hourglass_view.png") :
                ImageUtil.CreateProceduralExtendedBitmap(GetNoneLanguageBitmap("hourglass_view.png"), peid.Value);
        }

        /// <inheritdoc />
        public override void Recycle()
        {
            base.Recycle();
            RecycleCheckBoxs(_zones);

            _picture.Image?.Dispose();
            _picture.Image = null;
        }

        /// <inheritdoc />
        protected override void OnPaintEvent(Graphics g)
        {
            base.OnPaintEvent(g);
            PaintCheckBoxList(g, _zones, _checkmark);
        }

        /// <inheritdoc />
        protected override bool OnMouseDownHandler(int x, int y)
        {
            if (!base.OnMouseDownHandler(x, y)) return false;
            return EvaluateCheckBoxsMouseDown(x, y, _zones);
        }

        /// <inheritdoc />
        public override void SetActivated()
        {
            base.SetActivated();
            if (_screenConfig.QuestionGifNo != null)
            {
                _picture.SizeMode = PictureBoxSizeMode.StretchImage;
                _picture.ImageLocation = GetTextAnswerFromQuestion(_screenConfig.QuestionGifNo.Value);
            }
            else if (_screenConfig.QuestionFirstPictureNo != null)
            {
                var touchController = ScreensControllerAccessor.ScreensController;
                var resp = touchController.GetValidatedCurrentRespondent();
                if (resp != null)
                {
                    Cursor.Current = Cursors.Default;

                    if (touchController.Questionnaire.SubmitAction is ISubmitActionPictureTransformation submitAction)
                    {
                        CreateHourglassBmp(_peid);

                        SetPreviewPicture(_picture, _previewHourglass, _previewHourglass);

                        var t = new Task(() =>
                        {
                            _blockMouseEvents = true;
                            var questionNo = _screenConfig.QuestionFirstPictureNo.Value;
                            try
                            {
                                var transformations = submitAction.PictureTransformationController.GetTransformations(resp, false);
                                
                                if (!transformations?.Any() ?? true) throw new System.Exception("No selected preview transformation. Check transformation configuration file.");

                                // Fait les transformations et les sauvegardes.
                                foreach (var trans in transformations)
                                {
                                    trans.Respondent = resp;
                                    var transPicture = trans.TransformImage();
                                    string fileName;
                                    if (_screenConfig.QuestionPictureTransformedNo != null)
                                        fileName = GenerateUniqueId.GetId() + ".jpg";
                                    else
                                        fileName =
                                            StringUtil.GetFileName(
                                                GetTextAnswerFromQuestion(_screenConfig.QuestionFirstPictureNo.Value));

                                    var transformedPicture = DirectoryUtil.GetLocalIbrandQuestionnaireDataDir(
                                                                 _screenConfig.ProjectId, _screenConfig.QuestionnaireId,
                                                                 DirectoryUtil.ProjectDataDirectoriesEnum.pictures,
                                                                 DirectoryUtil.ProjectDataDirectoriesModEnum
                                                                     .transformed) + fileName;

                                    if (trans.OutputFormat.ToLower().Equals("png"))
                                    {
                                        transPicture.Save(ImageFormat.Png, 9, transformedPicture);
                                    }
                                    else
                                    {
                                        transPicture.Save(ImageFormat.Jpeg, 90, transformedPicture);
                                    }

                                    if (_screenConfig.QuestionPictureTransformedNo != null)
                                        questionNo = _screenConfig.QuestionPictureTransformedNo.Value;

                                    SaveAnswerToQuestion(new Answer {QuestionNo = questionNo, Response = transformedPicture});
                                }
                            }
                            catch (System.Exception e)
                            {
                                log.Error(e, "Error during picture transformation.");
                            }
                            finally
                            {
                                _blockMouseEvents = false;
                                SetPicture(questionNo);
                            }
                        });
                        t.Start();
                    }
                    else
                    {
                        SetPicture(_screenConfig.QuestionFirstPictureNo.Value);
                    }
                }
                else
                {
                    SetPicture(_screenConfig.QuestionFirstPictureNo.Value);
                }
            }
            else if (_screenConfig.QuestionPictureToPrintNo != null)
            {
                SetPicture(_screenConfig.QuestionPictureToPrintNo.Value);
            }
            else if (_screenConfig.QuestionPictureTransformedNo != null)
            {
                SetPicture(_screenConfig.QuestionPictureTransformedNo.Value);
            }
        }

        private void SetPicture(int questionNo)
        {
            if (InvokeRequired)
            {
                Invoke(new Action(() => { SetPictureDo(questionNo); }));
                return;
            }

            SetPictureDo(questionNo);
        }

        private void SetPictureDo(int questionNo)
        {
            var filePath = GetTextAnswerFromQuestion(questionNo);
            var tmp = FileUtil.LoadBitmap(filePath);
            if (tmp != null)
            {
                SetPreviewPicture(_picture, _previewHourglass, tmp.Frame(_picture.Width, _picture.Height));
            }
        }

        /// <inheritdoc />
        protected override void AddPreviewControl(Size previewSize, Point previewLocation)
        {
            if (previewLocation.X < 0)
            {
                previewLocation.X = (PictureBox.Width - previewSize.Width) / 2;
            }

            _picture = new PictureBox
            {
                Location = previewLocation,
                Size = previewSize,
                BackColor = Color.Black,
                Visible = true
            };

            Controls.Add(_picture);
            _picture.BringToFront();
        }

        protected override void ActionOnNext()
        {
            if (AllMandatoryChecks(_zones))
            {
                // Si tout est beau on exécute l'action de base.
                base.ActionOnNext();
            }
            else
            {
                MsgBox.Show(this, Resources.iBrand.NotAllMandatoryCheckEnabled, Resources.iBrand.AppName);
            }
        }
    }
}
