using System.Drawing;
using System.Windows.Forms;

namespace NomadLogic.iBrandDesktop.Ui.ApplicationScreen.custom.Bethenny
{
    public partial class QuestionnaireScreen2 : AbstractScreen
    {
        readonly Rectangle _nextArea;
        readonly Rectangle _backArea;

        PictureBox _rulesPictureBox;
        bool _rulesChecked;
        readonly Rectangle _rulesArea;

        readonly Rectangle _seeRulesArea;

        PictureBox _eightteenPictureBox;
        bool _eightteenChecked;
        readonly Rectangle _eightteenArea;

        PictureBox _signMePictureBox;
        bool _signMeChecked;
        readonly Rectangle _signMeArea;

        PictureBox _sendMePictureBox;
        bool _sendMeChecked;
        readonly Rectangle _sendMeArea;

        PictureBox _vacationPictureBox;
        bool _vacationChecked;
        readonly Rectangle _vacationArea;

        PictureBox _sharePictureBox;
        bool _shareChecked;
        readonly Rectangle _shareArea;

        public QuestionnaireScreen2(string backgroundImgEnSrc)
            : base(backgroundImgEnSrc, backgroundImgEnSrc)
        {
            PictureBox.MouseDown += MouseDownHandler;

            AddPictureBoxs();

            _rulesArea = new Rectangle(new Point(231, 87), new Size(506, 60));
            _seeRulesArea = new Rectangle(new Point(792, 87), new Size(263, 60));

            _eightteenArea = new Rectangle(new Point(231, 169), new Size(571, 60));
            _signMeArea = new Rectangle(new Point(231, 246), new Size(1159, 60));
            _sendMeArea = new Rectangle(new Point(231, 324), new Size(1461, 95));
            _vacationArea = new Rectangle(new Point(231, 454), new Size(1470, 95));
            _shareArea = new Rectangle(new Point(231, 580), new Size(1470, 95));

            _nextArea = new Rectangle(new Point(996, 727), new Size(353, 128));
            _backArea = new Rectangle(new Point(613, 727), new Size(353, 128));
        }

        public override void Recycle()
        {
            base.Recycle();

            _rulesChecked = false;
            _rulesPictureBox.Visible = false;

            _eightteenChecked = false;
            _eightteenPictureBox.Visible = false;

            _signMeChecked = false;
            _signMePictureBox.Visible = false;

            _sendMeChecked = false;
            _sendMePictureBox.Visible = false;

            _vacationChecked = false;
            _vacationPictureBox.Visible = false;

            _shareChecked = false;
            _sharePictureBox.Visible = false;
        }

        private void AddPictureBoxs()
        {
            var checkImage = GetNoneLanguageBitmap("checked.png");

            _rulesPictureBox = new PictureBox();
            _rulesPictureBox.Location = new Point(229, 91);
            _rulesPictureBox.Size = new Size(41, 40);
            _rulesPictureBox.Visible = false;

            _rulesPictureBox.Image = checkImage;
            _rulesPictureBox.MouseDown += MouseDownHandler;

            Controls.Add(_rulesPictureBox);

            _rulesPictureBox.BringToFront();

            _eightteenPictureBox = new PictureBox();
            _eightteenPictureBox.Location = new Point(229, 168);
            _eightteenPictureBox.Size = new Size(41, 40);
            _eightteenPictureBox.Visible = false;

            _eightteenPictureBox.Image = checkImage;
            _eightteenPictureBox.MouseDown += MouseDownHandler;

            Controls.Add(_eightteenPictureBox);

            _eightteenPictureBox.BringToFront();

            _signMePictureBox = new PictureBox();
            _signMePictureBox.Location = new Point(229, 244);
            _signMePictureBox.Size = new Size(41, 40);
            _signMePictureBox.Visible = false;

            _signMePictureBox.Image = checkImage;
            _signMePictureBox.MouseDown += MouseDownHandler;

            Controls.Add(_signMePictureBox);

            _signMePictureBox.BringToFront();

            _sendMePictureBox = new PictureBox();
            _sendMePictureBox.Location = new Point(229, 321);
            _sendMePictureBox.Size = new Size(41, 40);
            _sendMePictureBox.Visible = false;

            _sendMePictureBox.Image = checkImage;
            _sendMePictureBox.MouseDown += MouseDownHandler;
            Controls.Add(_sendMePictureBox);

            _sendMePictureBox.BringToFront();

            _vacationPictureBox = new PictureBox();
            _vacationPictureBox.Location = new Point(229, 449);
            _vacationPictureBox.Size = new Size(41, 40);
            _vacationPictureBox.Visible = false;

            _vacationPictureBox.Image = checkImage;
            _vacationPictureBox.MouseDown += MouseDownHandler;
            Controls.Add(_vacationPictureBox);

            _vacationPictureBox.BringToFront();

            _sharePictureBox = new PictureBox();
            _sharePictureBox.Location = new Point(229, 576);
            _sharePictureBox.Size = new Size(41, 40);
            _sharePictureBox.Visible = false;
            _sharePictureBox.Image = checkImage;
            _sharePictureBox.MouseDown += MouseDownHandler;

            Controls.Add(_sharePictureBox);

            _sharePictureBox.BringToFront();
        }


        public new void MouseDownHandler(object sender, MouseEventArgs e)
        {
            /*
            if (InArea(e.X, e.Y, _nextArea) && !_rulesChecked)
            {
                var mBox = new MessageDialog();
                mBox.ShowMessage("You must have read and accept official rules to continue.");
                return;
            }

            if (InArea(e.X, e.Y, _backArea))
            {
                Back();
                return;
            }

            if (InArea(e.X, e.Y, _seeRulesArea))
            {
                SuspendTimer();
                var d = new RulesDialog();
                d.ShowDialog();
                ResetTimer();
                return;
            }

            if (InArea(e.X, e.Y, _nextArea) && _rulesChecked)
            {
                var questionnaire = ScreensControllerAccessor.ScreensController.Questionnaire;
                var question = questionnaire.GetQuestion(11);
                var choiceCheck = (ChoiceCheck)question.GetChoice(1);

                choiceCheck.SetAnswer(new Answer(QuestionTypeEnum.Check, 11, 1));

                if (_eightteenChecked)
                {
                    question = questionnaire.GetQuestion(12);
                    choiceCheck = (ChoiceCheck)question.GetChoice(1);

                    choiceCheck.SetAnswer(new Answer(QuestionTypeEnum.Check, 12, 1));
                }

                if (_signMeChecked)
                {
                    question = questionnaire.GetQuestion(13);
                    choiceCheck = (ChoiceCheck)question.GetChoice(1);

                    choiceCheck.SetAnswer(new Answer(QuestionTypeEnum.Check, 13, 1));
                }

                if (_sendMeChecked)
                {
                    question = questionnaire.GetQuestion(14);
                    choiceCheck = (ChoiceCheck)question.GetChoice(1);

                    choiceCheck.SetAnswer(new Answer(QuestionTypeEnum.Check, 14, 1));
                }

                if (_vacationChecked)
                {
                    question = questionnaire.GetQuestion(15);
                    choiceCheck = (ChoiceCheck)question.GetChoice(1);

                    choiceCheck.SetAnswer(new Answer(QuestionTypeEnum.Check, 15, 1));
                }

                if (_shareChecked)
                {
                    question = questionnaire.GetQuestion(16);
                    choiceCheck = (ChoiceCheck)question.GetChoice(1);

                    choiceCheck.SetAnswer(new Answer(QuestionTypeEnum.Check, 16, 1));
                }

                Next();
            }
            else if (InArea(e.X, e.Y, _rulesArea) || sender.Equals(_rulesPictureBox))
            {
                if (!_rulesChecked)
                {
                    _rulesPictureBox.Visible = true;
                    _rulesChecked = true;
                }
                else
                {
                    _rulesPictureBox.Visible = false;
                    _rulesChecked = false;
                }
            }
            else if (InArea(e.X, e.Y, _eightteenArea) || sender.Equals(_eightteenPictureBox))
            {
                if (!_eightteenChecked)
                {
                    _eightteenPictureBox.Visible = true;
                    _eightteenChecked = true;
                }
                else
                {
                    _eightteenPictureBox.Visible = false;
                    _eightteenChecked = false;
                }
            }
            else if (InArea(e.X, e.Y, _signMeArea) || sender.Equals(_signMePictureBox))
            {
                if (!_signMeChecked)
                {
                    _signMePictureBox.Visible = true;
                    _signMeChecked = true;
                }
                else
                {
                    _signMePictureBox.Visible = false;
                    _signMeChecked = false;
                }
            }
            else if (InArea(e.X, e.Y, _sendMeArea) || sender.Equals(_sendMePictureBox))
            {
                if (!_sendMeChecked)
                {
                    _sendMePictureBox.Visible = true;
                    _sendMeChecked = true;
                }
                else
                {
                    _sendMePictureBox.Visible = false;
                    _sendMeChecked = false;
                }
            }
            else if (InArea(e.X, e.Y, _vacationArea) || sender.Equals(_vacationPictureBox))
            {
                if (!_vacationChecked)
                {
                    _vacationPictureBox.Visible = true;
                    _vacationChecked = true;
                }
                else
                {
                    _vacationPictureBox.Visible = false;
                    _vacationChecked = false;
                }
            }
            else if (InArea(e.X, e.Y, _shareArea) || sender.Equals(_sharePictureBox))
            {
                if (!_shareChecked)
                {
                    _sharePictureBox.Visible = true;
                    _shareChecked = true;
                }
                else
                {
                    _sharePictureBox.Visible = false;
                    _shareChecked = false;
                }
            }*/

        }
    }
}
