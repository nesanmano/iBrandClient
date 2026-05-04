using System;
using System.Drawing;
using System.Runtime.InteropServices;
using System.Windows.Forms;
using NomadLogic.Common.Services;
using NomadLogic.iBrandDesktop.Services;
using NomadLogic.iBrandDesktop.Ui.Choice;
using NomadLogic.iBrandDesktop.Util;

namespace NomadLogic.iBrandDesktop.Ui.ApplicationScreen.custom.Hornitos
{
    public class ShareScreen : AbstractScreen
    {
        [DllImport("user32.dll")]
        private static extern bool EnableWindow(IntPtr hWnd, bool enable);

        private delegate void UiDelegate();

        readonly PictureBox _picture;

        readonly Button _emailButton;
        Bitmap _emailImage;
        Bitmap _emailImageClick;

        readonly Button _smsButton;
        readonly Bitmap _smsImage;
        readonly Bitmap _smsImageClick;

        readonly Rectangle _doneArea;

        Respondent _respondent;

        readonly Rectangle _configurationArea;

        public ShareScreen(string backgroundImgEnSrc, string backgroundImageSrc)
            : base(backgroundImgEnSrc, backgroundImageSrc)
        {
            PictureBox.MouseDown += MouseDownHandler;

            _picture = new PictureBox {Location = new Point(400, 45), Size = new Size(480, 480), Visible = true};


            Controls.Add(_picture);

            _picture.BringToFront();

            _emailImageClick = GetBitmap("email_button_click.png");
            _emailImage = GetBitmap("email_button.png");

            _emailButton = new Button
            {
                Location = new Point(890, 45),
                Size = _emailImage.Size,
                Visible = true,
                Image = _emailImage,
                FlatStyle = FlatStyle.Flat
            };
            _emailButton.FlatAppearance.BorderSize = 0;
            _emailButton.MouseClick += MouseClickHandler;
            _emailButton.MouseUp += MouseUpHandler;
            _emailButton.MouseDown += MouseDownButtonHandler;
            
            Controls.Add(_emailButton);
            _emailButton.BringToFront();

            _smsImageClick = GetBitmap("sms_button_click.png");
            _smsImage = GetBitmap("sms_button.png");

            _smsButton = new Button
            {
                Location = new Point(890, 144),
                Size = _smsImage.Size,
                Visible = true,
                Image = _smsImage,
                FlatStyle = FlatStyle.Flat
            };
            _smsButton.FlatAppearance.BorderSize = 0;
            _smsButton.MouseClick += MouseClickHandler;
            _smsButton.MouseUp += MouseUpHandler;
            _smsButton.MouseDown += MouseDownButtonHandler;

            Controls.Add(_smsButton);
            _smsButton.BringToFront();

           

            _doneArea = new Rectangle(new Point(513, 540), new Size(253, 100));
            _configurationArea = new Rectangle(new Point(0, 650), new Size(70, 70));
        }

        public override bool ChangeLanguage(string language)
        {
            var change = base.ChangeLanguage(language);
            if (!change) return false;

            _emailButton.Image = null;

            if (_emailImageClick != null)
                _emailImageClick.Dispose();

            if (_emailImage != null)
                _emailImage.Dispose();

            _emailImageClick = GetBitmap("email_button_click.png");
            _emailImage = GetBitmap("email_button.png");

            _emailButton.Image = _emailImage;
            return true;
        }

        public void MouseUpHandler(object sender, MouseEventArgs e)
        {
           
            if (sender.Equals(_emailButton))
            {
                _emailButton.Image = _emailImage;

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
            if (sender.Equals(_emailButton))
            {
                _emailButton.Image = _emailImageClick;

                Refresh();
            }
            else if (sender.Equals(_smsButton))
            {
                _smsButton.Image = _smsImageClick;

                Refresh();
            }
           
        }

        public void MouseClickHandler(object sender, MouseEventArgs e)
        {
            if (sender.Equals(_emailButton))
            {
                SuspendTimer();
                
                var emailDlg = new EmailDialog();

                emailDlg.ShowDialog();

                if (emailDlg.DialogResult == DialogResult.OK)
                {
                    /*
                    var emailList = emailDlg.GetEmailList();
                    
                    if (emailList.Count > 0)
                    {
                        var db = ServicesContainer.GetService<IbrandDatabase>();

                        var questionnaire = ScreensControllerAccessor.ScreensController.Questionnaire;
                        var question = questionnaire.GetQuestion(ConstantsPhotoBooth._question_emails_to_send_no);
                        var choiceText = (ChoiceText)question.GetChoice(1);

                        //var answer = new Answer(-1, question.No, choiceText.No);

                        var dico = db.GetAnswersMap(_respondent.Id);

                        if (dico.ContainsKey(ConstantsPhotoBooth._question_emails_to_send_no))
                        {
                            var answerEmails = dico[ConstantsPhotoBooth._question_emails_to_send_no][1];
                            answer.Response = answerEmails.Response + ":";
                        }

                        for (var i = 0; i < emailList.Count; i++ )
                        {
                            var email = emailList[i];
                            answer.Response += email;
                            if (i < emailList.Count-1)
                                answer.Response += ":";
                        }

                        db.DeleteAnswer(_respondent.Id, answer);
                        db.InsertAnswer(_respondent.Id, answer);
                    }
                    */
                }
                ResumeTimer();
            }
            else if (sender.Equals(_smsButton))
            {
                SuspendTimer();

                var smsDlg = new SmsDialog();

                smsDlg.ShowDialog();

                if (smsDlg.DialogResult == DialogResult.OK)
                {
                    /*
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
                    */
                }
                ResumeTimer();
            }
            
        }

        

        public new void MouseDownHandler(object sender, MouseEventArgs e)
        {
            if (InArea(e.X, e.Y, _configurationArea))
            {
                var pwDlg = new PasswordDialog(_activeLg);
                pwDlg.ShowDialog();
                if (pwDlg.DialogResult == DialogResult.OK)
                {
                    new TouchModeConfigurationDlg().ShowDialog();
                }
            }
            else if (InArea(e.X, e.Y, _doneArea))
            {
                Next();
                //ScreensControllerAccessor.ScreensController.SetNewRespondentEvent();
            }
        }

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
                var question = questionnaire.GetQuestion(ConstantsPhotoBooth._question_gif_no);
                var choiceText = (ChoiceText)question.GetChoice(1);
                
                var photoName = //abstractConfiguration.PicturesDir + 
                    choiceText.GetAnswerText();
                var originale = new Bitmap(photoName);
                _picture.Image = originale;
                _picture.SizeMode = PictureBoxSizeMode.StretchImage;

                _respondent = ScreensControllerAccessor.ScreensController.GetValidatedCurrentRespondent() as Respondent;

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
