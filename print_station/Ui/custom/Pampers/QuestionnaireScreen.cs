using System.Drawing;
using System.Windows.Forms;
using NomadLogic.Common.Services;
using NomadLogic.Common.Ui;
using NomadLogic.Common.Ui.Forms;
using NomadLogic.Common.Util;
using NomadLogic.iBrandDesktop.Services;
using NomadLogic.iBrandDesktop.Util;
using NomadLogic.PrintStation.Ui.generic;

namespace NomadLogic.PrintStation.Ui.custom.Pampers
{
    /// <summary>
    /// Classe de questionnaire pour Pampers 2015
    /// </summary>
    public class QuestionnaireScreen : AbstractPrintStationScreen
    {
        private readonly Rectangle _nextArea;
        private readonly Rectangle _closeArea;
        private readonly Rectangle _privacyArea;
        private TextBox _smsTextBox;
        private TextBox _emailTextBox;
        private Label _uniqueCode;
        private int _optInCheckedPrivacy;
        private int _optInCheckedWaiver;
        private readonly Rectangle _optInAreaPrivacy;
        private readonly Rectangle _optInAreaWaiver;
        private readonly Bitmap _checkmark;

        /// <summary>
        /// Constructeur principal.
        /// </summary>
        /// <param name="backgroundImgSrc">Image de fond</param>
        /// <param name="sc">Paramètres de projet.</param>
        public QuestionnaireScreen(string backgroundImgSrc, PrintStationScreenConfig sc)
            : base(backgroundImgSrc, backgroundImgSrc, sc)
        {
            //PictureBox.MouseClick += MouseDownHandler;

            AddTextBoxs();
            AddKeyboard();

            _checkmark = GetBitmap("checkmark.png");

            _optInAreaPrivacy = new Rectangle(new Point(45, 40), new Size(320, 40));
            _optInAreaWaiver = new Rectangle(new Point(45, 100), new Size(1125, 75));

            _nextArea = new Rectangle(new Point(570, 350), new Size(180, 60));
            _closeArea = new Rectangle(new Point(1180, 5), new Size(55, 55));
            _privacyArea = new Rectangle(new Point(360, 40), new Size(450, 40));
        }

        /// <summary>
        /// Vide la forme.
        /// </summary>
        public override void Recycle()
        {
            base.Recycle();

            _smsTextBox.Text = "";
            _emailTextBox.Text = "";
            _uniqueCode.Text = "";

            _optInCheckedPrivacy = 0;
            _optInCheckedWaiver = 0;
        }

        ///// <summary>
        ///// Événement de dessein principal.
        ///// </summary>
        ///// <param name="sender">Envoyeur</param>
        ///// <param name="e">Paramètres</param>
        //protected override void PaintEvent(object sender, PaintEventArgs e)
        //{
        //    base.PaintEvent(sender, e);

        //    if (_optInCheckedPrivacy == 1)
        //    {
        //        e.Graphics.DrawImageUnscaled(_checkmark, new Point(55, 42));
        //    }

        //    if (_optInCheckedWaiver == 1)
        //    {
        //        e.Graphics.DrawImageUnscaled(_checkmark, new Point(55, 102));
        //    }
        //}

        private void AddKeyboard()
        {
            AddKeyboardToControls(50,420,1240, 350);
        }

        private bool SetAnswersToQuestionnaire()
        {
            var mBox = new MessageDialog();

            // Il faut avoir coché les termes de privacy, au minimum.
            if (_optInCheckedPrivacy != 1)
            {
                mBox.ShowMessage("You must agree to the privacy statement.");
                return false;
            }

            if (!string.IsNullOrWhiteSpace(_emailTextBox.Text) && !ValidateEmailAddress(5, _emailTextBox.Text))
            {
                return false;
            }

            if (!string.IsNullOrWhiteSpace(_smsTextBox.Text) && !ValidatePhoneNumber(6, 1, _smsTextBox.Text))
            {
                return false;
            }

            SaveAnswerToQuestionCheck(3, 1, _optInCheckedPrivacy == 1);
            SaveAnswerToQuestionCheck(4, 1, _optInCheckedWaiver == 1);

            if (!string.IsNullOrWhiteSpace(_uniqueCode.Text))
            {
                //SaveAnswerToQuestion(2, 1, _uniqueCode.Text);
            }

            // La question photo est automatique. Voir la classe QuestionnaireController

            return true;
        }

        private void AddTextBoxs()
        {
            _smsTextBox = new TextBox
            {
                Location = new Point(970, 290),
                Size = new Size(260, 35),
                TabIndex = 0,
                Font = new Font(FontFamily.GenericSansSerif, 23)
            };
            _smsTextBox.TextChanged += OnTextChanged;

            Controls.Add(_smsTextBox);
            _smsTextBox.BringToFront();

            _emailTextBox = new TextBox
            {
                Location = new Point(800, 220),
                Size = new Size(430, 35),
                TabIndex = 1,
                Font = new Font(FontFamily.GenericSansSerif, 23)
            };
            _emailTextBox.TextChanged += OnTextChanged;

            Controls.Add(_emailTextBox);
            _emailTextBox.BringToFront();

            _uniqueCode = new Label
            {
                Location = new Point(91, 175),
                Size = new Size(225, 45),
                Font = new Font(FontFamily.GenericMonospace, 30)
            };
            Controls.Add(_uniqueCode);
            _uniqueCode.BringToFront();
        }

        ///// <summary>
        ///// Événement souris.
        ///// </summary>
        ///// <param name="sender">L'envoyeur.</param>
        ///// <param name="e">Les paramètre de l'événement.</param>
        //protected override void MouseDownHandler(object sender, MouseEventArgs e)
        //{
        //    if (InArea(e.X, e.Y, _closeArea))
        //    {
        //        Close();
        //    }
        //    else if (InArea(e.X, e.Y, _nextArea))
        //    {
        //        if (SetAnswersToQuestionnaire())
        //        {
        //            Next();
        //        }
        //    }
        //    else if (InArea(e.X, e.Y, _optInAreaWaiver))
        //    {
        //        _optInCheckedWaiver = _optInCheckedWaiver == 0 ? 1 : 0;
        //        GenerateUniqueCode();
        //        Refresh();
        //    }
        //    else if (InArea(e.X, e.Y, _optInAreaPrivacy))
        //    {
        //        _optInCheckedPrivacy = _optInCheckedPrivacy == 0 ? 1 : 0;
        //        Refresh();
        //    }
        //    else if (InArea(e.X, e.Y, _privacyArea))
        //    {
        //        SuspendTimer();
        //        var file = DirectoryUtil.GetLocalIbrandQuestionnaireDataDir(_screenConfig.ProjectId, _screenConfig.QuestionnaireId, 
        //            DirectoryUtil.ProjectDataDirectoriesEnum.photobooth_ui, DirectoryUtil.ProjectDataDirectoriesModEnum.none) + "privacy.html";
        //        new WaiverHtmlDialog(file).ShowDialog();
        //        ResumeTimer();
        //    }
        //}

        private void GenerateUniqueCode()
        {
            if (_optInCheckedWaiver == 1)
            {
                var candidateCode = Generate5DigitsCode.GetCode();

                var database = ServicesContainer.GetService<IbrandDatabase>();

                // Recherche un code qui n'est pas déjà utilisé.
                while (database.HasResponseWithAnswer(candidateCode))
                {
                    candidateCode = Generate5DigitsCode.GetCode();
                }

                _uniqueCode.Text = candidateCode;
            }
            else
            {
                _uniqueCode.Text = "";
            }
            Refresh();
        }

        /// <summary>
        /// Active la forme.
        /// </summary>
        public override void SetActivated()
        {
            base.SetActivated();
            _smsTextBox.Focus();
        }
    }
}
