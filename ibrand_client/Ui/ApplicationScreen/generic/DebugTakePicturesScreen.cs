using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Imaging;
using System.Windows.Forms;
using NomadLogic.Common;
using NomadLogic.Common.Extentions;
using NomadLogic.Common.Respondent;
using NomadLogic.Common.Util;
using NomadLogic.iBrandDesktop.Ui.Choice;
using NomadLogic.iBrandDesktop.Util;
using NomadLogic.Multimedia.Gif;

namespace NomadLogic.iBrandDesktop.Ui.ApplicationScreen.Generic
{
    /// <summary>
    /// Cette classe prend une liste d'images en paramètre et en fait automatiquement un GIF, comme si on avait fait
    /// une capture de quatres images et en faisant un GIF avec. Va automatiquement à la page suivante par la suite.
    /// </summary>
    public class DebugTakePicturesScreen : AbstractScreen
    {
        private readonly PictureBox _panelPreview;
        private readonly List<string> _fileList;
        
        public DebugTakePicturesScreen(string backgroundImgEnSrc, ScreenConfig sc, List<string> fakeFiles)
            : base(backgroundImgEnSrc, backgroundImgEnSrc, sc)
        {
            _fileList = fakeFiles;

            _panelPreview = new PictureBox
            {
                Location = new Point(400, 45),
                Size = new Size(480, 480),
                BackColor = Color.White
            };

            Controls.Add(_panelPreview);

            _panelPreview.BringToFront();
        }

        public override void SetActivated()
        {
            base.SetActivated();
            MakeGifAndPictureToPrint();
        }

        private void MakeGifAndPictureToPrint()
        {

            if (_fileList.Count > 0)
            {
                var questionnaire = ScreensControllerAccessor.ScreensController.Questionnaire;
                if (_screenConfig.QuestionFirstPictureNo != null)
                {
                    var question = questionnaire.GetQuestion(_screenConfig.QuestionFirstPictureNo.Value);
                    var choiceText = (ChoiceText)question.GetChoice(1);

                    var pictureName = GenerateUniqueId.GetId() + ".jpg";

                    var outputFilePath = GetTerminalDirectoryDataFile(pictureName,
                                             DirectoryUtil.ProjectDataDirectoriesEnum.pictures, DirectoryUtil.ProjectDataDirectoriesModEnum.none);

                    var image = Image.FromFile(_fileList[0]);

                    if (image.Width != 720)
                        image = image.Resize(720, 480);

                    var b = image.Frame(image.Height, image.Height);

                    b.Save(outputFilePath, ImageFormat.Jpeg);
                    b.Dispose();

                    var answer = new Answer
                    {
                        QuestionType = QuestionTypeEnum.Photo, 
                        QuestionNo = question.No, 
                        ChoiceNo = choiceText.No, 
                        Response = outputFilePath
                    };
                    choiceText.SetAnswer(answer);
                }
            }

            CreateAnimatedGif();

            //Image to print
            CreatePictureToPrint();

            Next();
        }

        private void CreateAnimatedGif()
        {
            var pictureName = GenerateUniqueId.GetId() + ".gif";

            var outputFilePath = GetTerminalDirectoryDataFile(pictureName,
                    DirectoryUtil.ProjectDataDirectoriesEnum.pictures, DirectoryUtil.ProjectDataDirectoriesModEnum.none);

            var a = new AnimatedGifEncoder();
            a.Start(outputFilePath);
            a.SetDelay(350);
            //-1:no repeat,0:always repeat
            a.SetRepeat(0);
            a.SetQuality(1);

            //Bitmap logo = GetBitmap("logo.png");

            foreach (var fileName in _fileList)
            {
                var image = Image.FromFile(fileName);

                if (image.Width != 720)
                    image = image.Resize(720, 480);

                var b = image.Frame(image.Height, image.Height);

                //b.RotateFlip(RotateFlipType.RotateNoneFlipX);
                a.AddFrame(b);
                b.Dispose();
            }

            a.Finish();

            //logo.Dispose();

            var questionnaire = ScreensControllerAccessor.ScreensController.Questionnaire;
            if (_screenConfig.QuestionGifNo != null)
            {
                var question = questionnaire.GetQuestion(_screenConfig.QuestionGifNo.Value);
                var choiceText = (ChoiceText)question.GetChoice(1);

                var answer = new Answer {QuestionType = QuestionTypeEnum.Photo, QuestionNo = question.No, ChoiceNo = choiceText.No, Response = pictureName};
                choiceText.SetAnswer(answer);
            }
        }

        private void CreatePictureToPrint()
        {
            var pictureName = GenerateUniqueId.GetId() + ".jpg";

            var outputFilePath = GetTerminalDirectoryDataFile(pictureName,
                    DirectoryUtil.ProjectDataDirectoriesEnum.pictures, DirectoryUtil.ProjectDataDirectoriesModEnum.none);

            var finalImage = GetNoneLanguageBitmap("frame.jpg");
            finalImage.SetResolution(72f, 72f);

            var offset = 55F;

            var g = Graphics.FromImage(finalImage);

            var count = 0;
            var row = 0;
            var mod = 0;

            foreach (var fileName in _fileList)
            {
                var image = Image.FromFile(fileName);

                if (image.Width != 720)
                    image = image.Resize(720, 480);

                var b = image.Frame(_panelPreview.Width, _panelPreview.Height);
                //b.RotateFlip(RotateFlipType.RotateNoneFlipX);

                g.DrawImage(b, new PointF(offset + ((mod * b.Width) + (mod * 35)), offset + 10 + ((row * b.Height) + (row * 35))));

                b.Dispose();

                count++;

                mod = (count % 2);

                if (mod == 0)
                    row++;
            }

            g.Dispose();

            finalImage.Save(outputFilePath, ImageFormat.Jpeg);
            finalImage.Dispose();

            var questionnaire = ScreensControllerAccessor.ScreensController.Questionnaire;
            var question = questionnaire.GetQuestion(_screenConfig.QuestionPictureToPrintNo ?? -1);
            var choiceText = (ChoiceText)question.GetChoice(1);

            var answer = new Answer {QuestionType = QuestionTypeEnum.Photo, QuestionNo = question.No, ChoiceNo = choiceText.No, Response = pictureName};
            choiceText.SetAnswer(answer);
        }
    }
}
