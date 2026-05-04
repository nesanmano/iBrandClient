using System;
using System.Collections.Generic;
using System.Drawing;
using System.Runtime.InteropServices;
using System.Windows.Forms;
using NomadLogic.Common.Services;
using NomadLogic.iBrandDesktop.Services;
using NomadLogic.iBrandDesktop.Ui.Choice;
using NomadLogic.iBrandDesktop.Util;

namespace NomadLogic.iBrandDesktop.Ui.ApplicationScreen.custom.BestBuy
{
    public class ShareScreen : AbstractScreen
    {
        [DllImport("user32.dll")]
        private static extern bool EnableWindow(IntPtr hWnd, bool enable);

        private delegate void UiDelegate();
        private delegate void UiDelegateIbrand(bool success, Dictionary<string, string> results);

        readonly PictureBox _picture;

        readonly Button _printButton;
        readonly Bitmap _printImage;
        readonly Bitmap _printImageClick;

        readonly Button _smsButton;
        readonly Bitmap _smsImage;
        readonly Bitmap _smsImageClick;

        readonly Bitmap _smsDlgBackground;
        readonly Bitmap _smsDlgCheckmark;

        readonly Rectangle _doneArea;

        Respondent _respondent;

        readonly PictureBox _uploadingPicture;

        private string _photoFileName;

        public ShareScreen(string backgroundImgEnSrc)
            : base(backgroundImgEnSrc, backgroundImgEnSrc)
        {
            //PictureBox.MouseDown += MouseDownHandler;
            //PictureBox.Paint += PaintEvent;
            _picture = new PictureBox();

            _picture.Location = new Point(400, 45);
            _picture.Size = new Size(480, 480);
            _picture.Visible = true;

            Controls.Add(_picture);

            _picture.BringToFront();

            _uploadingPicture = new PictureBox();

            _uploadingPicture.Location = new Point(512, 577);
            _uploadingPicture.Size = new Size(254, 97);
            _uploadingPicture.Image = GetBitmap("sending.png");

            Controls.Add(_uploadingPicture);

            _smsImageClick = GetNoneLanguageBitmap("sms_button_click.png");
            _smsImage = GetNoneLanguageBitmap("sms_button.png");

            _smsButton = new Button();
            _smsButton.Location = new Point(890, 45);
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

            _printImageClick = GetNoneLanguageBitmap("print_button_click.png");
            _printImage = GetNoneLanguageBitmap("print_button.png");

            _printButton = new Button();
            _printButton.Location = new Point(890, 144);
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

            _smsDlgBackground = GetBitmap("sms_bkg.jpg");
            _smsDlgCheckmark = GetNoneLanguageBitmap("checkmark.png");

            _doneArea = new Rectangle(new Point(513, 576), new Size(253, 100));
        }

       

        public void MouseUpHandler(object sender, MouseEventArgs e)
        {
            if (sender.Equals(_smsButton))
            {
                _smsButton.Image = _smsImage;

                Refresh();
            }
            else if (sender.Equals(_printButton))
            {
                _printButton.Image = _printImage;

                Refresh();
            }
           
        }

        public void MouseDownButtonHandler(object sender, MouseEventArgs e)
        {
            if (sender.Equals(_smsButton))
            {
                _smsButton.Image = _smsImageClick;

                Refresh();
            }
            else if (sender.Equals(_printButton))
            {
                _printButton.Image = _printImageClick;

                Refresh();
            }    
            
        }

        public void MouseClickHandler(object sender, MouseEventArgs e)
        {
            /*if (sender.Equals(_printButton))
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
                var fileName = //abstractConfiguration.PicturesDir + 
                    choiceText.GetAnswerText();

                Cursor.Current = Cursors.WaitCursor;

                PrintUtil.Print(fileName, null, this);

                Cursor.Current = Cursors.Default;

                ResumeTimer();
            }
            else if (sender.Equals(_smsButton))
            {
                SuspendTimer();

                var smsDlg = new SmsConfirmDialog(_smsDlgBackground,
                   new Point(12, 235), new Size(300, 40),
                   new Point(12, 313), new Size(300, 40),
                   new Rectangle(10, 27, 268, 44),
                   _smsDlgCheckmark,
                   new Rectangle(new Point(392, 285), new Size(95, 86)), new Rectangle(new Point(429, 8), new Size(62, 56)),
                   new Font(FontFamily.GenericSansSerif, 20));

                smsDlg.ShowDialog();

                if (smsDlg.DialogResult == DialogResult.OK)
                {
                        var phoneNumber = smsDlg.GetSms();
                        if (phoneNumber != null)
                        {

                            var db = ServicesContainer.GetService<IbrandDatabase>();

                            var questionnaire = ScreensControllerAccessor.ScreensController.Questionnaire;
                            var question = questionnaire.GetQuestion(ConstantsPhotoBooth._question_sms_no);
                            var choiceText = (ChoiceText)question.GetChoice(1);

                            var answer = new Answer(-1, question.No, choiceText.No);

                            answer.Response = phoneNumber;

                            db.DeleteAnswer(_respondent.Id, answer);
                            db.InsertAnswer(_respondent.Id, answer);
                        }

                }
                ResumeTimer();
            }*/
           
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

            _photoFileName = null;
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
                
                _photoFileName = //abstractConfiguration.PicturesDir + 
                    choiceText.GetAnswerText();
                var originale = new Bitmap(_photoFileName);
                
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
