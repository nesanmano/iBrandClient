using NLog;
using NUnit.Framework;
using System;
using System.Collections.Generic;
using System.Drawing;
using NomadLogic.iBrandDesktop.Ui.ScreenModel;
using Newtonsoft.Json;
using System.Collections;

namespace NomadLogic.Test.iBrand.Ui.ApplicationScreen
{
    class HighLevelScreenModelTest : TestBaseNunit
    {

        // ReSharper disable once UnusedMember.Local
        private static readonly Logger log = LogManager.GetCurrentClassLogger();

        [Test]
        public void ObjectToJson()
        {
            IList<AbstractScreenModel> screenModel = new List<AbstractScreenModel>();

            LanguageScreenModel lang = new LanguageScreenModel();
            lang.Clazz="HomeScreen";
            lang.BackgroundImages = new Dictionary<string,string>();
            lang.BackgroundImages.Add("en", "2. Language.jpg");
            lang.BackgroundImages.Add("fr", "2. Language.jpg");

            lang.LanguageSelectionRectangles = new Dictionary<string,int[]>();

            int[] en = { 855, 408, 794, 254 };
            int[] fr = { 27, 408, 816, 254 };
            lang.LanguageSelectionRectangles.Add("en",en);
            lang.LanguageSelectionRectangles.Add("fr",fr);

            SaveAnswer saveQuestion = new SaveAnswer();
            saveQuestion.QuestionNo = 1;
            lang.Order = 1;
            lang.SaveToQuestion = saveQuestion;

            DataInputScreenModel input = new DataInputScreenModel();
            input.TextBoxes = new Dictionary<int, InputModel>();
            InputModel inputModel = new InputModel();

            int[] name = {1,1,1,1};
            inputModel.InputArea = name;
            inputModel.ErrorMessages = new Dictionary<string,string>();
            inputModel.ErrorMessages.Add("en","Name is mandatory");
            inputModel.ErrorMessages.Add("fr", "Nom est requis");
            SaveAnswer saveQuestion2 = new SaveAnswer();
            saveQuestion2.QuestionNo = 2;
            inputModel.SaveToQuestion = saveQuestion2;

            inputModel.Desc="Name";

            InputModel inputModel2 = new InputModel();

            inputModel2.InputArea = name;
            inputModel2.ErrorMessages = new Dictionary<string, string>();
            inputModel2.ErrorMessages.Add("en", "Check is mandatory");
            inputModel2.ErrorMessages.Add("fr", "Check est requis");
            SaveAnswer saveQuestion3 = new SaveAnswer();
            saveQuestion3.QuestionNo = 3;
            inputModel2.SaveToQuestion = saveQuestion3;

            inputModel2.Desc = "Opt-In 1";



            input.Clazz="Questionnaire";

            input.Keyboard = name;


            input.TextBoxes.Add(1, inputModel);
            input.CheckBoxes = new Dictionary<int,InputModel>();
            input.CheckBoxes.Add(1,inputModel2);

            input.Order = 2;
            input.RequiredJumpsToNextScreen =1;


            DataOutputScreenModel output = new DataOutputScreenModel();
            output.Clazz= "PreviewScreen";
            output.ContentPreviewRectangle = new Dictionary<int,int[]>();

            output.ContentInteractionsAreas = new Dictionary<int, int[]>();
            int[] tmp = {1,1,1,1 };
            output.ContentInteractionsAreas.Add(1,tmp);
            output.ContentInteractionsAreas.Add(2,tmp);
            output.ContentPreviewRectangle.Add(1,tmp);

            CameraInputScreenModel camera = new CameraInputScreenModel();
            camera.Clazz = "TakePictureAutoScreen";

            camera.CountImageSequenceFiles = new List<string>();
            camera.CountImageSequenceFiles.Add("1.png");
            camera.CountImageSequenceFiles.Add("2.png");
            camera.CountImageSequenceFiles.Add("3.png");
            camera.CountImageSequenceFiles.Add("4.png");
            camera.CountImageSequenceFiles.Add("5.png");
            camera.CountImageSequenceFiles.Add("6.png");

            int[] pos = {0,0};
            camera.OverlayPosition = pos;
            camera.OverlaySelector = new Dictionary<string,SaveAnswer>();
            camera.OverlaySelector.Add("Overlay_EN",new SaveAnswer());
            camera.OverlaySelector.Add("Overlay_FR", new SaveAnswer());

            camera.TransformationOptions = new Dictionary<string,int[]>();

            int[] canvasSize = {1000,1000};
            int[] newSize = {1000,1000};
            camera.TransformationOptions.Add("canvasSize",canvasSize);
            camera.TransformationOptions.Add("newSize",newSize);

            int[] livePreview ={230,234,200,400};
            camera.PreviewRectangle = livePreview;

            int[] countDownLocation = {230,230};
            int[] CountDownLocation = countDownLocation;

            camera.CountDownLocation =CountDownLocation;


            PrintScreenModel print = new PrintScreenModel();

            print.Clazz = "ShareScreen";

            int[] printBtn = {1000,1230,13,13};
            int[] printCounter = {1340,1450,45,45};

            print.PrintArea = printBtn;

            print.CounterArea = printCounter;


            DataOutputScreenModel thanks = new DataOutputScreenModel();
            thanks.Clazz = "PreviewScreen";
            thanks.InactivityInterval =5000;

            screenModel.Add(lang);
            screenModel.Add(input);
            screenModel.Add(output);
            screenModel.Add(camera);
            screenModel.Add(print);
            screenModel.Add(thanks);

            var str = JsonConvert.SerializeObject(screenModel);

            Console.WriteLine("Follwing is the produced json :"+ str);
      

        }


        [Test]
        public void JsonToObject()
        {

        }


        [Test]
        public void TestComparator()
        {
            LanguageScreenModel lang = new LanguageScreenModel();
            lang.Clazz = "HomeScreen";
            lang.BackgroundImages = new Dictionary<string, string>();
            lang.BackgroundImages.Add("en", "2. Language.jpg");
            lang.BackgroundImages.Add("fr", "2. Language.jpg");

            lang.LanguageSelectionRectangles = new Dictionary<string, int[]>();

            int[] en = { 855, 408, 794, 254 };
            int[] fr = { 27, 408, 816, 254 };
            lang.LanguageSelectionRectangles.Add("en", en);
            lang.LanguageSelectionRectangles.Add("fr", fr);

            SaveAnswer saveQuestion = new SaveAnswer();
            saveQuestion.QuestionNo = 1;
            lang.Order = 1;
            lang.SaveToQuestion = saveQuestion;


            LanguageScreenModel lang2 = new LanguageScreenModel();
            lang2.Clazz = "HomeScreen";
            lang2.BackgroundImages = new Dictionary<string, string>();
            lang2.BackgroundImages.Add("en", "2. Language.jpg");
            lang2.BackgroundImages.Add("fr", "2. Language.jpg");

            lang2.LanguageSelectionRectangles = new Dictionary<string, int[]>();

            int[] en2 = { 855, 408, 794, 254 };
            int[] fr2 = { 27, 408, 816, 254 };
            lang2.LanguageSelectionRectangles.Add("en", en2);
            lang2.LanguageSelectionRectangles.Add("fr", fr2);

            SaveAnswer saveQuestion2 = new SaveAnswer();
            saveQuestion2.QuestionNo = 1;
            lang2.Order = 2;
            lang2.SaveToQuestion = saveQuestion2;

            AbstractScreenModel[] screenModel = {lang2,lang};

            Assert.Greater(screenModel[0].Order,screenModel[1].Order);

            Assert.Less(screenModel[0].Compare(screenModel[0],screenModel[1]),0);

            Assert.Greater(screenModel[0].Order, screenModel[1].Order);
        }
    }
}
