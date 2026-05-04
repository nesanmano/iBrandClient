using System.Drawing;
using System.IO;
using JetBrains.Annotations;
using NomadLogic.Common.Util;
using NomadLogic.iBrandDesktop.Ui.ApplicationScreen.custom.Simple_Project;
using NomadLogic.iBrandDesktop.Ui.ApplicationScreen.Generic;
using NomadLogic.iBrandDesktop.Util;

namespace NomadLogic.iBrandDesktop.Ui.ApplicationScreen.FullscreenLoaders
{
    [PublicAPI]
    public class LoaderProjetSimple2 : IFullscreenLoader
    {
        public void CreateScreens(TouchScreenController screensController, ScreenConfig sc)
        {
            sc.QuestionFirstPictureNo = 3;
            
            var uiDir = DirectoryUtil.GetLocalIbrandTerminalDataDir(sc.ProjectId, sc.QuestionnaireId, sc.TerminalId,
                DirectoryUtil.ProjectDataDirectoriesEnum.photobooth_ui, DirectoryUtil.ProjectDataDirectoriesModEnum.none);
            var baseDir = DirectoryUtil.GetLocalIbrandTerminalDir(sc.ProjectId, sc.QuestionnaireId, sc.TerminalId);
            
            var simpleConfigFile = new FileInfo(baseDir+"simpleProjectConfig.json");

            if (!simpleConfigFile.Exists)
            {
                throw new System.Exception("Missing config file for simple project: " + simpleConfigFile.FullName);
            }

            var simpleProjectConfigLoader = new  SimpleProjectConfigLoader();

            simpleProjectConfigLoader.LoadConfigFile(baseDir+"simpleProjectConfig.json");

            #region homeScreen
            var home = new HomeScreen(baseDir + "1-Accueil.jpg", sc,
                new Rectangle(157,569,1410,167),// fr
                    null) //en
            {
                //QuestionLanguageNo = 1
            };
            screensController.AddScreen(home);
            #endregion

            #region Start Countdown
           /* var PrePhotoGetReady = new PreviewScreen(uiDir + "2-Welcome.jpg",uiDir + "2-Bienvenue.jpg", sc,
                null, //cancel
                new Rectangle(396, 708, 906, 173),//next
                null,
                0,
                new Size(0, 0)
            )
            {
                Parent = screen_panel
            };
            screensController.AddScreen(PrePhotoGetReady);*/


            /*var PrePhotoGetReady = new SectionSkipScreen(baseDir + "2-Start_en.jpg", baseDir + "2-Start_fr.jpg",sc,
                null)
            {
                
                ZoneList = new List<SectionSkipScreenData>(){
                    /*new SectionSkipScreenData
                    {
                        PageChangeOptions = ScreenChangeOptions.GenerateBasicNextOptions(1),
                        Zone = new Rectangle(new Point(981, 638), new Size(286,41)),
                        SkipValidation = true
                    },*/
                    /*new SectionSkipScreenData
                    {
                        PageChangeOptions = ScreenChangeOptions.GenerateBasicNextOptions(1),
                        Zone = _simpleProjectConfigLoader.GetTakePictureRectangle()
                    }
                }
            };
            screensController.AddScreen(PrePhotoGetReady);*/


            #endregion

            #region disclaimer
            /*var DislaimerScreen = new DisclaimerScreen(
                uiDir + "8-terms and conditions_EN.jpg",uiDir + "8-terms and conditions_FR.jpg", sc,
                new Rectangle(675,825,318,67),//next/back
                null)
            {
                GoBackInsteadOfNext = true
            };
            screensController.AddScreen(DislaimerScreen);*/
            #endregion

            /*#region Take Photo*/


            // 4- Prise photo
          /*  var generators = new List<IAnimatedImageSequenceGenerator>
            {
                new AnimatedImageSequenceGeneratorBasicFiles
                {
                    FilePaths = new List<string>
                    {
                        uiDir + "5.png",
                        uiDir + "4.png",
                        uiDir + "3.png",
                        uiDir + "2.png",
                        uiDir + "1.png"
                    }
                }
            };


            var picture = new TakePictureAutoScreen(sc, baseDir + "3-Countdown_en.jpg", baseDir + "3-Countdown_fr.jpg",
               _simpleProjectConfigLoader.GetTakePictureRectangle(), //Preview
                5
            )
            {
                ImageListTranformOptions = new ImageListTransformOptions()
                {
                    FirstPictureOptions = new ImageListTransformOptionsFirstPicture
                    {
                        SaveToQuestion = 3,
                        CanvasSize = new Size(1534,1065),
                        NewSize = new Size(1534,1065),
                        OverlaySelector = new ImageSelectorQuestionnaire<QuestionCondition>(new HashSet<ConditionFileTuple<QuestionCondition>>
                        {
                            new ConditionFileTuple<QuestionCondition> { Condition = new QuestionCondition(1, 1, 1), File = baseDir + "Overlay_EN.png"},
                            new ConditionFileTuple<QuestionCondition> { Condition = new QuestionCondition(1, 1, 2), File = baseDir + "Overlay_FR.png"}
                        }),OverlayPos = new Point(0,0)

                    }
                },
                CountImageSequence = new AnimatedImageSequence
                {
                    Loop = false,
                    DrawLocation = new Point(746, 671),
                    ImageGenerators = generators
                 
                }
            };
            screensController.AddScreen(picture);
            #endregion*/


            // 5- Preview
           /* #region Preview Screen*/
          /*  var preview = new PreviewScreen(baseDir + "4-Preview_en.jpg", baseDir + "4-Preview_fr.jpg", sc,
                null, // Cancel
                _simpleProjectConfigLoader.GetPreviewScreenRectangle()["next"], // Next
                _simpleProjectConfigLoader.GetPreviewScreenRectangle()["back"], // Back
                (1050-(_simpleProjectConfigLoader.GetPreviewScreenRectangle()["first"].Height+234)), _simpleProjectConfigLoader.GetPreviewScreenRectangle()["first"].Size);
            screensController.AddScreen(preview);
            #endregion
            */
            // 2- Info

            #region Questionnaire
            if (!simpleProjectConfigLoader.DoSkipQuestionnaire())
            {
                var questionnaire = new SimpleProjectQuestionnaire(baseDir + "5-Info_en.jpg", baseDir + "5-Info_fr.jpg",
                    sc,
                    simpleProjectConfigLoader.GetQuestionnaireScreenUiFeatures()["sms"],
                    simpleProjectConfigLoader.GetQuestionnaireScreenUiFeatures()["email"], null, null,simpleProjectConfigLoader.GetQuestionnaireScreenUiFeatures()["keyboard"])
                {
                    Print = simpleProjectConfigLoader.GetPrintOption()
                };
                screensController.AddScreen(questionnaire);
            }

            #endregion


            // On imprime maintenant sans demander quoi que ce soit: Un Share Screen
            /*var printScreen = new ShareScreen(uiDir + "6-Print_EN.jpg", sc, null, null, null,
                _simpleProjectConfigLoader.GetPrintScreenRectangles()["printArea"], // Print
                null, null, null, null,
                new Rectangle(476, 842, 721, 123), // End
                1050-(_simpleProjectConfigLoader.GetPreviewScreenRectangle()["first"].Height+243), _simpleProjectConfigLoader.GetPreviewScreenRectangle()["first"].Size, uiDir + "6-Print_FR.jpg")
            {
                DontShowInactiveImages = false,
                OverrideEndBySkip = 1,
                ForcePrintOnActivation =false,
               
                PrintoutCounter = new AdvancedScreenPrintConfiguration()
                {
                    Point =  new Point(_simpleProjectConfigLoader.GetPrintScreenRectangles()["counter"].X, _simpleProjectConfigLoader.GetPrintScreenRectangles()["counter"].Y),
                    MaxPrintOuts = _simpleProjectConfigLoader.GetPrintLimit(),
                    FontDefinitionInfo = new FontDefinitionInfo()
                    {
                        Color = Color.Black,
                        Size = 40
                    }
                }
            };
            screensController.AddScreen(printScreen);*/

            // Thx
            var thxScreen = new ThanksEndScreen(baseDir + "5-Thanks_en.jpg", sc, baseDir + "5-Thanks_fr.jpg")
            {
                InactivityTimerInterval = 5000
            };
            screensController.AddScreen(thxScreen);

        }
    }
}