using System;
using System.Drawing;
using System.Windows.Forms;
using NomadLogic.Common;
using NomadLogic.Common.Ui;
using NomadLogic.Common.Ui.Keyboard;
using NomadLogic.PrintStation.Ui.generic;

// ReSharper disable InconsistentNaming

namespace NomadLogic.PrintStation.Ui.custom.MakeUpForever
{
    /// <summary>
    /// Questionnaire pour le projet Make Up Forever MAK-2017-01B
    /// </summary>
    public class QuestionnaireScreenMak_2017_01 : AbstractPrintStationScreen
    {
        private readonly Rectangle _nextArea;
        private readonly Rectangle _cancelArea;
        private readonly Rectangle _optInArea1;
        private readonly Rectangle _optInArea2;
        private readonly Bitmap _checkmark;
        private KeyboardControl _keyboard;
        private readonly PictureBox _pictureBox;
        private bool _optInChecked1;
        private bool _optInChecked2;
        //private readonly TextBox _textStandFor;

        /// <summary>
        /// Constructeur principal.
        /// </summary>
        public QuestionnaireScreenMak_2017_01(string backgroundImgEnSrc, string backgroundImgFrSrc, PrintStationScreenConfig sc)
            : base(backgroundImgEnSrc, backgroundImgFrSrc, sc)
        {
            //PictureBox.MouseClick += MouseDownHandler;

            AddKeyboard();

            _pictureBox = new PictureBox
            {
                Size = new Size(460, 350),
                Location = new Point(21, 21),
                BackColor = Color.White,
                BorderStyle = BorderStyle.None,
                SizeMode = PictureBoxSizeMode.Zoom
            };
            Controls.Add(_pictureBox);

            _checkmark = GetNoneLanguageBitmap("checkmark.png");
            _optInArea1 = new Rectangle(497, 37, 758, 129);
            _optInArea2 = new Rectangle(497, 176, 697, 53);

            _nextArea = new Rectangle(507, 323, 200, 50);
            _cancelArea = new Rectangle(780, 323, 200, 50);

            /*_textStandFor = new TextBox
            {
                Size = new Size(500, 40),
                Location = new Point(752, 245),
                Font = new Font(FontFamily.GenericSansSerif, 24),
                MaxLength = 15
            };
            _textStandFor.TextChanged += OnTextChanged;
            Controls.Add(_textStandFor);
            _textStandFor.BringToFront();*/

            _pictureBox.BringToFront();
        }

        /// <inheritdoc/>
        public override void Recycle()
        {
            base.Recycle();

            _optInChecked1 = false;
            _optInChecked2 = false;

            //_textStandFor.Text = "";
        }

        ///// <inheritdoc/>
        //protected override void PaintEvent(object sender, PaintEventArgs e)
        //{
        //    base.PaintEvent(sender, e);

        //    if (_optInChecked1)
        //    {
        //        e.Graphics.DrawImageUnscaled(_checkmark, new Point(497, 21));
        //    }

        //    if (_optInChecked2)
        //    {
        //        e.Graphics.DrawImageUnscaled(_checkmark, new Point(497, 176));
        //    }
        //}

        private void AddKeyboard()
        {
            _keyboard = AddKeyboardToControlsUsa((Size.Width - 1236) / 2, 390, 1236, 387);
            _keyboard.CustomLabelInfo = new CustomPaintLabelInfo {
                                                                     ShapeContainerMethod = label => CustomPathFactory.CreateRoundedRectangle(label, 8),
                                                                     ContourPen = null,
                                                                     Font = new Font(FontFamily.GenericSansSerif, 15, FontStyle.Bold),
                                                                     ColorPalette = {Color.LightGray, Color.Gray},
                                                                     TextBrush = new SolidBrush(Color.White)
                                                                 };
        }

        private bool SetAnswersToQuestionnaire()
        {
            /*if (_textStandFor.Text.Length == 0)
            {
                var mBox = new MessageDialog();
                mBox.ShowMessage(_activeLg.Equals(Constants.FR) ? "Vous devez dire ce à quoi vous aspirez." : "You must tell the world what you stand out for.");
                return false;
            }*/

            if (!_optInChecked1)
            {
                var mBox = new MessageDialog();
                mBox.ShowMessage(_activeLg.Equals(Constants.FR) ? "Consentement pour matériel promotionel obligatoire." : "Opt-in for promotional material is mandatory.");
                return false;
            }

            SaveAnswerToQuestionCheck(5, 1, _optInChecked1);
            SaveAnswerToQuestionCheck(6, 1, _optInChecked2);
            //SaveAnswerToQuestion(5, 1, _textStandFor.Text);

            return true;
        }

        /// <inheritdoc/>
        public override void OnTextChanged(object sender, EventArgs e)
        {
            if (_activated)
                ResetTimer();
        }

        ///// <inheritdoc/>
        //protected override void MouseDownHandler(object sender, MouseEventArgs e)
        //{
        //    if (InArea(e.X, e.Y, _cancelArea))
        //    {
        //        Close();
        //    }
        //    else if (InArea(e.X, e.Y, _nextArea))
        //    {
        //        if (SetAnswersToQuestionnaire())
        //            Next();
        //    }
        //    else if (InArea(e.X, e.Y, _optInArea1))
        //    {
        //        _optInChecked1 = !_optInChecked1;

        //        Refresh();
        //    }
        //    else if (InArea(e.X, e.Y, _optInArea2))
        //    {
        //        _optInChecked2 = !_optInChecked2;

        //        Refresh();
        //    }
        //}

        /// <inheritdoc/>
        public override void SetActivated()
        {
            base.SetActivated();

            // On va chercher le GIF de sauvé dans le répertoire du Picture.
            /*var baseDir = StringUtil.GetDirName(GetSelectedPicture().Src);
            var quest = GetCurrentQuestionnaire();
            var gifFile = baseDir + Path.DirectorySeparatorChar + StringUtil.GetFileName(quest.GetQuestionAnswers(4).First().Response);
            if (!File.Exists(gifFile)) { throw new Exception("Could not find gif file in post directory: " + gifFile); }

            _pictureBox.ImageLocation = null;
            _pictureBox.Image?.Dispose();

            _pictureBox.ImageLocation = gifFile;*/

            var pictureInfo = GetSelectedPicture();

            _pictureBox.ImageLocation = null;
            _pictureBox.Image?.Dispose();
            _pictureBox.ImageLocation = pictureInfo.Src;

            Refresh();
        }
    }
}
