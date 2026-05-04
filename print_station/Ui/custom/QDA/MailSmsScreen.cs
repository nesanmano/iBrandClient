using System;
using System.Drawing;
using System.Drawing.Imaging;
using System.Threading.Tasks;
using System.Windows.Forms;
using NLog;
using NomadLogic.Common.Extentions;
using NomadLogic.Common.Ui;
using NomadLogic.Common.Util;
using NomadLogic.iBrandDesktop.Ui.ApplicationScreen.Generic.Helpers;
using NomadLogic.iBrandDesktop.Util.SubmitAction.BaseClasses;
using NomadLogic.Multimedia;
using NomadLogic.PrintStation.Ui.generic;

namespace NomadLogic.PrintStation.Ui.custom.QDA
{
    public class MailSmsScreen : AbstractPrintStationScreen
    {
        private static readonly Logger log = LogManager.GetCurrentClassLogger();

        private readonly Rectangle _nextArea;
        private readonly Rectangle _cancel;
        private TextBox _emailTextBox;
        private TextBox _phoneTextBox;
        private TextBox _nameTextBox;
        private TextBox _lastNameTextBox;
        private TextBox _orgTextBox;
        private readonly PictureBox _picture;
        private readonly QuestionnaireController _questionnaireController;
        private Bitmap _previewHourglass;
        private readonly Bitmap _checkmark;
        private ScreenCheckOption _optIn;
        private ProceduralExtendedImageDefinition? _peid;

        public MailSmsScreen(string backgroundImgEnSrc, string backgroundImgFrSrc, PrintStationScreenConfig sc, QuestionnaireController qc)
            : base(backgroundImgEnSrc, backgroundImgFrSrc, sc)
        {
            _questionnaireController = qc;

            //PictureBox.MouseClick += MouseDownHandler;

            AddTextBoxs();
            AddKeyboard();
            
            _nextArea = new Rectangle(412, 314, 196, 51);
            _cancel = new Rectangle(670, 314, 196, 51);

            _picture = new PictureBox
            {
                Size = new Size(218, 327),
                Location = new Point(129, 36),
                BackColor = Color.White,
                Visible = true,
                SizeMode = PictureBoxSizeMode.StretchImage
            };
            //PictureBox.Paint += PaintEvent;
            Controls.Add(_picture);
            _picture.BringToFront();

            _checkmark = GetNoneLanguageBitmap("checkmark.png");
            _optIn = new ScreenCheckOption(new Rectangle(354, 252, 800, 49), 8, 1, false, new Point(358, 244));
        }

        //protected override void PaintEvent(object sender, PaintEventArgs e)
        //{
        //    base.PaintEvent(sender, e);
        //    if (_optIn.AnswerState)
        //    {
        //        e.Graphics.DrawImageUnscaled(_checkmark, _optIn.CheckLocation);
        //    }
        //}

        public override void Recycle()
        {
            base.Recycle();
            _picture.Image?.Dispose();
            _picture.Image = null;
            _optIn.Recycle();
            _phoneTextBox.Text = "";
            _emailTextBox.Text = "";
            _nameTextBox.Text = "";
            _lastNameTextBox.Text = "";
            _orgTextBox.Text = "";
        }
        
        private void AddKeyboard()
        {
            AddKeyboardToControls(35,380,1206, 387);
        }
        
        private bool SetAnswersToQuestionnaire()
        {
            if (!string.IsNullOrWhiteSpace(_phoneTextBox.Text))
            {
                if (!ValidatePhoneNumber(7, 1, _phoneTextBox.Text))
                {
                    return false;
                }
            }
            else
            {
                var mBox = new MessageDialog();
                var lg = GetCurrentQuestionnaire().LanguageCurrent;
                mBox.ShowMessage(lg.Equals("en", StringComparison.InvariantCultureIgnoreCase) ?
                    "Your phone number is mandatory." :
                    "Votre numéro de téléphone est obligatoire.");
                return false;
            }

            if (!string.IsNullOrWhiteSpace(_emailTextBox.Text))
            {
                if (!ValidateEmailAddress(6, _emailTextBox.Text))
                {
                    return false;
                }
            }
            else
            {
                var mBox = new MessageDialog();
                var lg = GetCurrentQuestionnaire().LanguageCurrent;
                mBox.ShowMessage(lg.Equals("en", StringComparison.InvariantCultureIgnoreCase) ?
                    "Your email is mandatory." :
                    "Votre courriel est obligatoire.");
                return false;
            }

            if (string.IsNullOrWhiteSpace(_nameTextBox.Text))
            {
                var mBox = new MessageDialog();
                var lg = GetCurrentQuestionnaire().LanguageCurrent;
                mBox.ShowMessage(lg.Equals("en", StringComparison.InvariantCultureIgnoreCase) ?
                    "You must tell us your name." :
                    "Votre prénom est obligatoire.");
                return false;
            }
            //SaveAnswerToQuestion(3, 1, _nameTextBox.Text);

            if (string.IsNullOrWhiteSpace(_lastNameTextBox.Text))
            {
                var mBox = new MessageDialog();
                var lg = GetCurrentQuestionnaire().LanguageCurrent;
                mBox.ShowMessage(lg.Equals("en", StringComparison.InvariantCultureIgnoreCase) ?
                    "You must tell us your last name." :
                    "Votre nom de famille est obligatoire.");
                return false;
            }
            //SaveAnswerToQuestion(4, 1, _lastNameTextBox.Text);

            if (string.IsNullOrWhiteSpace(_orgTextBox.Text))
            {
                var mBox = new MessageDialog();
                var lg = GetCurrentQuestionnaire().LanguageCurrent;
                mBox.ShowMessage(lg.Equals("en", StringComparison.InvariantCultureIgnoreCase) ?
                    "Which organization do you work for?" :
                    "Pour quelle organisation travaillez-vous?");
                return false;
            }
            //SaveAnswerToQuestion(5, 1, _orgTextBox.Text);

            if (!_optIn.AnswerState)
            {
                var mBox = new MessageDialog();
                var lg = GetCurrentQuestionnaire().LanguageCurrent;
                mBox.ShowMessage(lg.Equals("en", StringComparison.InvariantCultureIgnoreCase) ?
                    "You must agree to receive information to continue." :
                    "Pour continuer, vous devez accepter de recevoir des informations.");
                return false;
            }

            return true;
        }

        public override void OnTextChanged(object sender, EventArgs e)
        {
            if (_activated)
                ResetTimer();
        }

        private void AddTextBoxs()
        {
            var index = 0;
            
            SetTextBox(out _nameTextBox,     ++index, 514, 071, 451, 25);
            SetTextBox(out _lastNameTextBox, ++index, 514, 107, 451, 25);
            SetTextBox(out _orgTextBox,      ++index, 514, 144, 451, 25);
            SetTextBox(out _emailTextBox,    ++index, 514, 181, 451, 25);
            SetTextBox(out _phoneTextBox,    ++index, 514, 218, 451, 25);
        }

        //protected override void MouseDownHandler(object sender, MouseEventArgs e)
        //{
        //    if (InArea(e.X, e.Y, _nextArea))
        //    {
        //        if (SetAnswersToQuestionnaire())
        //            Next();
        //    }
        //    else if (InArea(e.X, e.Y, _cancel))
        //    {
        //        Close();
        //    }
        //    else if (InArea(e.X, e.Y, _optIn.ClickZone))
        //    {
        //        _optIn.AnswerState = !_optIn.AnswerState;
        //        SaveAnswerToQuestionCheck(_optIn.QuestionNo, _optIn.ChoiceNo, _optIn.AnswerState);
        //        Refresh();
        //    }
        //}

        public ProceduralExtendedImageDefinition? ProceduralHourglassImageDefinition
        {
            get { return _peid; }
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

        public override void SetActivated()
        {
            base.SetActivated();
            
            //SaveAnswerToQuestion(1, 1, GetSelectedPicture().Src);
            
            var resp = _questionnaireController.GetValidatedCurrentRespondent();

            if (_questionnaireController.Questionnaire.SubmitAction is AbstractSubmitActionPictureTransformation submitAction)
            {
                CreateHourglassBmp(_peid);

                SetPreviewPicture(_picture, _previewHourglass, _previewHourglass);

                var t = new Task(() =>
                {
                    try
                    {
                        var transformations = submitAction.PictureTransformationController.GetTransformations(resp, false);

                        foreach (var trans in transformations)
                        {
                            int.TryParse(trans.OutputNameQuestionNo, out var qId);
                            var transPicture = trans.TransformImage();
                            string fileName;
                            if (_screenConfig.QuestionPictureTransformedNo != null)
                                fileName = GenerateUniqueId.GetId() + "." + trans.OutputFormat;
                            else
                                fileName = StringUtil.GetFileName(GetTextAnswerFromQuestion(qId));

                            var transformedPicture = GetTerminalDirectoryDataFile(fileName,
                                                         DirectoryUtil.ProjectDataDirectoriesEnum.pictures,
                                                         DirectoryUtil.ProjectDataDirectoriesModEnum.transformed);

                            if (trans.OutputFormat.ToLower().Equals("png"))
                            {
                                transPicture.Save(ImageFormat.Png, 9, transformedPicture);
                            }
                            else
                            {
                                transPicture.Save(ImageFormat.Jpeg, 90, transformedPicture);
                            }

                            //SaveAnswerToQuestion(qId, 1, transformedPicture);
                        }
                    }
                    catch (Exception e)
                    {
                        log.Error(e, "Error during picture transformation.");
                    }
                    finally
                    {
                        SetPicture(1);
                    }
                });
                t.Start();
            }
            
            _emailTextBox.Focus();
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
            SetPreviewPicture(_picture, _previewHourglass, FileUtil.LoadBitmap(GetTextAnswerFromQuestion(questionNo, 1)));
        }
    }
}