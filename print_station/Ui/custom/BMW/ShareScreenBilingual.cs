using System;
using System.Diagnostics;
using System.Drawing;
using System.IO;
using System.Threading;
using System.Windows.Forms;
using NLog;
using NomadLogic.Common.Extentions;
using NomadLogic.Common.Respondent;
using NomadLogic.Common.Services;
using NomadLogic.Common.Util;
using NomadLogic.Common.Util.Math;
using NomadLogic.Common.Util.Print;
using NomadLogic.iBrandDesktop.Ui.Choice;
using NomadLogic.PrintStation.Services;
using NomadLogic.PrintStation.Ui.generic;

namespace NomadLogic.PrintStation.Ui.custom.BMW
{
    public class ShareScreenBilingual : AbstractPrintStationScreen
    {
        // ReSharper disable once UnusedMember.Local
        private static readonly Logger log = LogManager.GetCurrentClassLogger();
        readonly Rectangle _doneArea;
        readonly PictureBox _picture;
        private readonly Rectangle? _printArea;
        private readonly Rectangle? _mailArea;
        private readonly Rectangle? _smsArea;


        private string _photoFileName;

        /*private readonly Bitmap _twitterInactiveImage;
        private readonly Bitmap _facebookInactiveImage;
        private readonly Bitmap _mailInactiveImage;
        private readonly Bitmap _smsInactiveImage;
        private readonly Bitmap _printInactiveImage;*/
        private bool _socialBlocked;

        public ShareScreenBilingual(string backgroundImgEnSrc, PrintStationScreenConfig sc)
            : base(backgroundImgEnSrc, backgroundImgEnSrc, sc)
        {
            //PictureBox.MouseDown += MouseDownHandler;
            //PictureBox.Paint += PaintEvent;
            _picture = new PictureBox
            {
                Location = new Point(417, 50),
                Size = new Size(450, 450),
                BackColor = Color.White,
                Visible = true,
                SizeMode = PictureBoxSizeMode.Zoom
            };

            Controls.Add(_picture);

            _picture.BringToFront();

            _printArea = new Rectangle(new Point(1035, 89), new Size(125, 125));
            _smsArea = new Rectangle(new Point(1035, 228), new Size(125, 125));
            _mailArea = new Rectangle(new Point(1035, 386), new Size(125, 125));

            _doneArea = new Rectangle(new Point(540, 541), new Size(212, 80));
        }

        //protected override void MouseDownHandler(object sender, MouseEventArgs e)
        //{
        //    base.MouseDownHandler(sender, e);

        //    if (InArea(e.X, e.Y, _doneArea))
        //    {
        //        log.Trace("Next Area clicked");
        //        End();
        //    }
        //    else if (InArea(e.X, e.Y, _mailArea))
        //    {
        //        log.Trace("Mail Area clicked");

        //        if (_socialBlocked)
        //        {
        //            log.Trace("Social function currently blocked. Aborting Mail sequence.");
        //            return;
        //        }

        //        SuspendTimer();
        //        Next(1);
        //    }
        //    else if (InArea(e.X, e.Y, _printArea))
        //    {
        //        log.Trace("Print Area Clicked");

        //        if (_socialBlocked)
        //        {
        //            log.Trace("Social function currently blocked. Aborting print sequence.");
        //            return;
        //        }

        //        SetSocialBlock(true);

        //        log.Trace("Beguin Print sequence.");
        //        var t = new Thread(SendPrint);
        //        t.SetApartmentState(ApartmentState.STA);
        //        t.Start();

        //    }
        //    else if (InArea(e.X, e.Y, _smsArea))
        //    {
        //        log.Trace("SMS Area clicked");

        //        if (_socialBlocked)
        //        {
        //            log.Trace("Social function currently blocked. Aborting SMS sequence.");
        //            return;
        //        }

        //        SuspendTimer();
        //        Next(2);
        //    }

        //}

        public override void Recycle()
        {
            base.Recycle();

            _picture.Image?.Dispose();
            _picture.Image = null;

            _socialBlocked = false;
            _photoFileName = null;
        }


        public override void SetActivated()
        {
            base.SetActivated();

            log.Trace("SetActivated Called");

            if (_photoFileName != null)
                return;

            SetSocialBlock(false);

            try
            {
                var questionnaire = MainForm.GetInstance().Questionnaire;

                var selectedPicture = MainForm.GetInstance().SelectedPicture;

                //First Picture
                if (_screenConfig.QuestionFirstPictureNo != null)
                {
                    var questionFirstPicture = questionnaire.GetQuestion((int)_screenConfig.QuestionFirstPictureNo);

                    var choiceText = (ChoiceText)questionFirstPicture.GetChoice(1);

                    var filename = StringUtil.GetFileName(selectedPicture.Src);

                    var ibrandFileName = GetTerminalDirectoryDataFile(filename,
                        DirectoryUtil.ProjectDataDirectoriesEnum.pictures,
                        DirectoryUtil.ProjectDataDirectoriesModEnum.none);

                    if (!File.Exists(ibrandFileName))
                    {
                        var originalFilePath = selectedPicture.Src;
                        using (var bmp = FileUtil.LoadBitmap(originalFilePath))
                        {
                            if (!MathUtil.IsSizeCorrectRatio(bmp.Size, 1.5))
                            {
                                // Check if landscape
                                int newWidth, newHeight;
                                if (bmp.Width >= bmp.Height)
                                {
                                    newWidth = bmp.Width;
                                    newHeight = (int)(bmp.Width / 1.5);
                                }
                                else
                                {
                                    newWidth = (int)(bmp.Height / 1.5);
                                    newHeight = bmp.Height;
                                }

                                using (var modImage = bmp.Frame(newWidth, newHeight))
                                {
                                    modImage.Save(90, ibrandFileName);
                                }
                            }
                            else
                            {
                                File.Copy(originalFilePath, ibrandFileName);
                            }
                        }
                    }

//                    var answer = new Answer(-1, questionFirstPicture.No, choiceText.No) { Response = filename };
//                    choiceText.SetAnswer(answer);

                    _photoFileName = ibrandFileName;

                    _picture.Image?.Dispose();
                    _picture.Load(_photoFileName);
                }


                GC.Collect();
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message, @"iBrand (tm)", MessageBoxButtons.OK,
                    MessageBoxIcon.Error, MessageBoxDefaultButton.Button1);
            }
        }

        private void SendPrint()
        {
            var watch = new Stopwatch();
            watch.Start();

            try
            {
                var database = ServicesContainer.GetService<PrintStationDatabase>();

                var picture = MainForm.GetInstance().SelectedPicture;

                database.AddPrintCount(picture.ServerPostId);

                /*var fileToPrint = DirectoryUtil.GetLocalIbrandQuestionnaireDataDir(_screenConfig.ProjectId,
                         _screenConfig.QuestionnaireId,
                        // On utilise le EventID et le Feature ID comme équivalence au projet/questionnaire. 
                        DirectoryUtil.ProjectDataDirectoriesEnum.pictures,
                        DirectoryUtil.ProjectDataDirectoriesModEnum.print) + StringUtil.GetFileName(picture.Src);

                if (File.Exists(fileToPrint))
                    File.Delete(fileToPrint);

                var result = _activeLg.Equals(Constants.EN) ? 
                    PrintStationUtil.CreateFramedPhoto(picture.Src, picture, fileToPrint) : 
                    PrintStationUtil.CreateFramedPhotoFr(picture.Src, picture, fileToPrint);

                ImageUtil.SaveToJpegWithOptions(result, 95, fileToPrint);

                result.Dispose();*/

                if (PrintUtil.Print(picture.Src, null, this))
                {

                    var questionnaire = MainForm.GetInstance().Questionnaire;

                    if (_screenConfig.QuestionNumberOfPrintNo != null)
                    {
                        var questionPrint = questionnaire.GetQuestion((int) _screenConfig.QuestionNumberOfPrintNo);

                        var choiceText = (ChoiceText) questionPrint.GetChoice(1);

//                        var answer = new Answer(-1, questionPrint.No, choiceText.No) {Response = "1"};
//                        choiceText.SetAnswer(answer);
                    }

                    NewPrintEvent.GetInstance().NewItemEvent.Set();
                }
            }
            catch (Exception e)
            {
                log.Error(e, "Error during print sequence.");
            }

            watch.Stop();
            if (watch.ElapsedMilliseconds < MINIMUM_PRINT_WAIT_TIME)
            {
                Thread.Sleep(MINIMUM_PRINT_WAIT_TIME - (int)watch.ElapsedMilliseconds);
            }
            SetSocialBlock(false);
        }




        private void SetSocialBlock(bool enableBlock)
        {
            if (InvokeRequired)
            {
                Invoke(new Action(() => { SetSocialBlock(enableBlock); }));
                return;
            }

            _socialBlocked = enableBlock;
            Refresh();
            log.Trace("Social block Set to {0}", enableBlock);
        }
    }
}