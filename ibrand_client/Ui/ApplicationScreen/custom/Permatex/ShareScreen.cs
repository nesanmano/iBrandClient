using System;
using System.Drawing;
using System.Runtime.InteropServices;
using System.Windows.Forms;
using NomadLogic.Common.Services;
using NomadLogic.iBrandDesktop.Services;
using NomadLogic.iBrandDesktop.Ui.Choice;
using NomadLogic.iBrandDesktop.Util;

namespace NomadLogic.iBrandDesktop.Ui.ApplicationScreen.custom.Permatex
{
    public class ShareScreen : AbstractScreen
    {
        [DllImport("user32.dll")]
        private static extern bool EnableWindow(IntPtr hWnd, bool enable);

        private delegate void UiDelegate();

        readonly PictureBox _picture;

        readonly Button _printButton;
        Bitmap _printImage;
        Bitmap _printImageClick;

        readonly Button _smsButton;
        readonly Bitmap _smsImage;
        readonly Bitmap _smsImageClick;

        readonly Rectangle _doneArea;

        Respondent _respondent;


        public ShareScreen(string backgroundImgEnSrc, string backgroundImageSrc)
            : base(backgroundImgEnSrc, backgroundImageSrc)
        {
            //PictureBox.MouseDown += MouseDownHandler;

            _picture = new PictureBox();

            _picture.Location = new Point(286, 45);
            _picture.Size = new Size(720, 480);
            _picture.Visible = true;

            Controls.Add(_picture);

            _picture.BringToFront();

            _printImageClick = GetBitmap("print_button_click.png");
            _printImage = GetBitmap("print_button.png");
            
            _printButton = new Button();
            _printButton.Location = new Point(1030,45);
            _printButton.Size = _printImage.Size;
            _printButton.Visible = true;
            _printButton.Image = _printImage;
            _printButton.FlatStyle = FlatStyle.Flat;
            _printButton.FlatAppearance.BorderSize = 0;
            _printButton.MouseClick += MouseClickHandler;
            _printButton.MouseUp += MouseUpHandler;
            _printButton.MouseDown += MouseDownButtonHandler;
            
            Controls.Add(_printButton);

            _printButton.BringToFront();

            _smsImageClick = GetBitmap("sms_button_click.png");
            _smsImage = GetBitmap("sms_button.png");

            _smsButton = new Button();
            _smsButton.Location = new Point(1030, 144);
            _smsButton.Size = _smsImage.Size;
            _smsButton.Visible = true;
            _smsButton.Image = _smsImage;
            _smsButton.FlatStyle = FlatStyle.Flat;
            _smsButton.FlatAppearance.BorderSize = 0;
            _smsButton.MouseClick += MouseClickHandler;
            _smsButton.MouseUp += MouseUpHandler;
            _smsButton.MouseDown += MouseDownButtonHandler;

            Controls.Add(_smsButton);
            _smsButton.BringToFront();

            _doneArea = new Rectangle(new Point(503, 576), new Size(267, 112));
        }

        public override bool ChangeLanguage(string language)
        {
            var change = base.ChangeLanguage(language);
            if (!change) return false;

            _printButton.Image = null;

            _printImageClick?.Dispose();

            _printImage?.Dispose();

            _printImageClick = GetBitmap("print_button_click.png");
            _printImage = GetBitmap("print_button.png");

            _printButton.Image = _printImage;

            return true;
        }

        public void MouseUpHandler(object sender, MouseEventArgs e)
        {
            if (sender.Equals(_printButton))
            {
                _printButton.Image = _printImage;

                Refresh();
            }
           
            else if (sender.Equals(_smsButton))
            {
                _smsButton.Image = _smsImage;

                Refresh();
            }
           
        }

        public void MouseDownButtonHandler(object sender, MouseEventArgs e)
        {
            if (sender.Equals(_printButton))
            {
                _printButton.Image = _printImageClick;

                Refresh();
            }         
            else if (sender.Equals(_smsButton))
            {
                _smsButton.Image = _smsImageClick;

                Refresh();
            }
           
        }

        public void MouseClickHandler(object sender, MouseEventArgs e)
        {/*
            if (sender.Equals(_printButton))
            {
                SuspendTimer();

                var questionnaire = ScreensControllerAccessor.ScreensController.Questionnaire;
                
                var db = ServicesContainer.GetService<IbrandDatabase>();

                var question = questionnaire.GetQuestion(ConstantsPhotoBooth._question_number_of_print_no);
                var choiceText = (ChoiceText)question.GetChoice(1);

                var answer = new Answer(-1, question.No, choiceText.No);

                var dico = db.GetAnswersMap(_respondent.Id);

                if (dico.ContainsKey(ConstantsPhotoBooth._question_number_of_print_no))
                {
                    var answerPrint = dico[ConstantsPhotoBooth._question_number_of_print_no][1];//Question print
                    var nbrPrint = int.Parse(answerPrint.Response);
                    nbrPrint++;
                    answer.Response = nbrPrint.ToString();

                    db.DeleteAnswer(_respondent.Id, answer);
                }
                else
                    answer.Response = "1";

                db.InsertAnswer(_respondent.Id, answer);
                
                question = questionnaire.GetQuestion(ConstantsPhotoBooth._question_picture_to_print_no);
                choiceText = (ChoiceText)question.GetChoice(1);
                var fileName = DirectoryUtil.GetLocalMediaDir(DirectoryUtil.NomadLogicMediaDirectoriesEnum.pictures) + choiceText.GetAnswerText();

                Cursor.Current = Cursors.WaitCursor;

                PrintUtil.Print(fileName, null, this);

                Cursor.Current = Cursors.Default;

                ResumeTimer();
            }
           
            else if (sender.Equals(_smsButton))
            {
                SuspendTimer();

                var smsDlg = new SmsDialog();

                smsDlg.ShowDialog();

                if (smsDlg.DialogResult == DialogResult.OK)
                {
                    var smsList = smsDlg.GetSmsList();

                    if (smsList.Count > 0)
                    {
                        var db = ServicesContainer.GetService<IbrandDatabase>();

                        var questionnaire = ScreensControllerAccessor.ScreensController.Questionnaire;
                        var question = questionnaire.GetQuestion(ConstantsPhotoBooth._question_sms_no);
                        var choiceText = (ChoiceText)question.GetChoice(1);

                        var answer = new Answer(-1, question.No, choiceText.No);

                        var dico = db.GetAnswersMap(_respondent.Id);

                        if (dico.ContainsKey(ConstantsPhotoBooth._question_sms_no))
                        {
                            var answerSms = dico[ConstantsPhotoBooth._question_sms_no][1];//Question sms
                            answer.Response = answerSms.Response + ":";
                        }

                        for (var i = 0; i < smsList.Count; i++)
                        {
                            var phone = smsList[i];
                            answer.Response += phone;
                            if (i < smsList.Count - 1)
                                answer.Response += ":";
                        }

                        db.DeleteAnswer(_respondent.Id, answer);
                        db.InsertAnswer(_respondent.Id, answer);
                    }

                }
                ResumeTimer();
            }
            */

        }

        //protected override void MouseDownHandler(object sender, MouseEventArgs e)
        //{
        //    base.MouseDownHandler(sender, e);
 
        //    if (InArea(e.X, e.Y, _doneArea))
        //    {
        //        Next();
        //        ScreensControllerAccessor.ScreensController.SetSyncEvent();
        //    }
        //}

        public override void Recycle()
        {
            base.Recycle();
            if (_picture.Image != null)
            {
                _picture.Image.Dispose();
                _picture.Image = null;
            }
        }

        public override void SetActivated()
        {
            base.SetActivated();
            try
            {
                var questionnaire = ScreensControllerAccessor.ScreensController.Questionnaire;

                Question.Question question = null;
                if (ConstantsPhotoBooth._question_gif_no != -1)
                    question = questionnaire.GetQuestion(ConstantsPhotoBooth._question_gif_no);
                else
                    question = questionnaire.GetQuestion(ConstantsPhotoBooth._question_first_picture_no);
                
                var choiceText = (ChoiceText)question.GetChoice(1);
                
                var photoName = //abstractConfiguration.PicturesDir + 
                    choiceText.GetAnswerText();
                var originale = new Bitmap(photoName);
                _picture.Image = originale;
                _picture.SizeMode = PictureBoxSizeMode.StretchImage;

                _respondent = ScreensControllerAccessor.ScreensController.GetValidatedCurrentRespondent() as Respondent;
                _respondent.Language = _activeLg;
                _respondent.Answers = ScreensControllerAccessor.ScreensController.Questionnaire.GetAnswersToSaveInDb();

                var db = ServicesContainer.GetService<IbrandDatabase>();
                db.InsertRespondent(_respondent, true);
            }
            catch (System.Exception ex)
            {
                MessageBox.Show(WorkingArea.Container, ex.Message, "iBrand (tm)", MessageBoxButtons.OK, MessageBoxIcon.Error, MessageBoxDefaultButton.Button1);
            }
        }
    }
}
