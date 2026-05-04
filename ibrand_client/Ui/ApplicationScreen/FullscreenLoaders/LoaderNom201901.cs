using System.Collections.Generic;
using System.Drawing;
using JetBrains.Annotations;
using NomadLogic.Common.Questionnaire;
using NomadLogic.Common.Respondent;
using NomadLogic.Common.Util;
using NomadLogic.iBrandDesktop.Ui.ApplicationScreen.Generic;
using NomadLogic.iBrandDesktop.Ui.ApplicationScreen.Generic.Helpers;
using NomadLogic.iBrandDesktop.Ui.ApplicationScreen.Generic.Helpers.ScreenDefinitions;

namespace NomadLogic.iBrandDesktop.Ui.ApplicationScreen.FullscreenLoaders
{
    [PublicAPI]
    public class LoaderNom201901 : IFullscreenLoader
    {
        public void CreateScreens(TouchScreenController screensController, ScreenConfig sc)
        {
            var uiDir = DirectoryUtil.GetLocalIbrandTerminalDataDir(sc.ProjectId, sc.QuestionnaireId, sc.TerminalId, 
                DirectoryUtil.ProjectDataDirectoriesEnum.photobooth_ui, DirectoryUtil.ProjectDataDirectoriesModEnum.none);


            var home = new HomeScreen(uiDir + "1-Tap to start.jpg", sc)
            {
            };
            screensController.AddScreen(home);
            
            var scanScreen = new RfidScannerScreen(uiDir + "2-Scan.jpg", null, sc,
                new Rectangle((1280 - 130) / 2, 500, 525, 100))
            {
                ScanQuestionNo = 1,
                NextLocation = new Rectangle(558, 363, 567, 57),
                WaitSequence = null,
                ErrorImagePosition = new Point((1280 - 900) / 2, 625),
                WebFacadeConfigFile = "webRequestFacadeConfig.json",
                
            };
            screensController.AddScreen(scanScreen);
            
            var questionnaire = new GenericScreen(uiDir + "3-confirm info.jpg", null, sc)
            {
                DefinitionKeyboard = new ScreenDefinitionKeyboard
                {
                    ConfigFileName = "03_key.json",
                    Position = new Rectangle(-1, 562, 1590, 448),
                    FontDefinition = new FontDefinitionInfo
                    {
                        Size = 25
                    },
                    KeyboardType = KeyboardTypeEnum.Multilingual,
                    Padding = 3
                },
                DefinitionTextbox = new ScreenDefinitionTextBoxes
                {
                    ConfigFileName = "03_text.json",
                    TextboxDefinitions = new List<TextboxDefinition>
                    {
                        new TextboxDefinition
                        {
                            Position = new Rectangle(298, 319, 594, 40),
                            Mandatory = false,
                            AllowEmpty = true,
                            DataModels = new List<DataModelBase>{
                                new DataModelQuestionnaire
                            {
                                Identifier = new QuestionIdentifier(8)
                            }},
                            FieldName = "First name"
                        },
                        new TextboxDefinition
                        {
                            Position = new Rectangle(1039, 319, 594, 40),
                            Mandatory = false,
                            AllowEmpty = true,
                            DataModels = new List<DataModelBase>{
                                new DataModelQuestionnaire
                            {
                                Identifier = new QuestionIdentifier(9)
                            }},
                            FieldName = "Last name"
                        },
                        new TextboxDefinition
                        {
                            Position = new Rectangle(298, 385, 594, 40),
                            Mandatory = false,
                            AllowEmpty = true,
                            DataModels = new List<DataModelBase>{
                                new DataModelQuestionnaire
                            {
                                Identifier = new QuestionIdentifier(6)
                            }},
                            FieldName = "Email",
                            Validation = new TextValidationBasic
                            {
                                Option = TextValidationBasic.TextValidationBasicEnum.Email
                            },
                            UserMessages = new Dictionary<UserErrorEnum, IDictionary<string, string>>
                            {
                                {UserErrorEnum.Validation, new Dictionary<string, string>{{"en", "Le courriel est invalide." }}}
                            }
                        },
                        new TextboxDefinition
                        {
                            Position = new Rectangle(1039, 385, 594, 40),
                            Mandatory = false,
                            AllowEmpty = true,
                            DataModels = new List<DataModelBase>{
                                new DataModelQuestionnaire
                            {
                                Identifier = new QuestionIdentifier(5)
                            }},
                            FieldName = "Mobile",
                            Validation = new TextValidationBasic
                            {
                                Option = TextValidationBasic.TextValidationBasicEnum.Sms
                            },
                            UserMessages = new Dictionary<UserErrorEnum, IDictionary<string, string>>
                            {
                                {UserErrorEnum.Validation, new Dictionary<string, string>{{"en", "Le numéro de téléphone mobile est invalide." }}}
                            }
                        }
                    }
                },
                DefinitionNavigation = new ScreenDefinitionNavigation
                {
                    ConfigFileName = "03_nav.json",
                    ZoneList = new List<SectionSkipScreenData>
                    {
                        new SectionSkipScreenData
                        {
                            SkipValidation = true,
                            Zone = new Rectangle(1580, 0, 100, 100),
                            PageChangeOptions = ScreenChangeOptions.GenerateCancelOptions()
                        },
                        new SectionSkipScreenData
                        {
                            SkipValidation = false,
                            Zone = new Rectangle(156, 476, 314, 66),
                            PageChangeOptions = ScreenChangeOptions.GenerateBasicNextOptions()
                        }
                    }
                }
            };
            screensController.AddScreen(questionnaire);

            var recordVideoScreen = new RecordVideoSonyScreen(uiDir + "4-Prise video.jpg", null, sc,
                new Rectangle(1382, 492, 209, 94), // Start
                new Rectangle(1382, 661, 209, 94), // Stop
                new Rectangle(1580, 0, 100, 100), // Cancel
                new Size(749, 424), 411)
            {
                NoLanguageDependency = true
                // DEBUG SEULEMENT
                //,DebugMovieFile = @"C:\Users\François Coupal\Documents\projets\projets internes\FFMPEG_test_zoom\cd.mp4"
            };
            screensController.AddScreen(recordVideoScreen);

            var previewScreen = new PreviewAndCutVideoScreen(uiDir + "5-Edition video.jpg", null, sc,
                null, // cancel
                new Rectangle(1379, 642, 209, 127), // next
                new Rectangle(1379, 474, 209, 127), // back
                new Rectangle(95, 474, 209, 127), // play
                new Rectangle(95, 642, 209, 127), // pause
                new Rectangle(324, 933, 1032, 36), // Bar Zone
                411, new Size(749, 424), false)
            {
                CurrentTimePosition = new Point(95, 870),
                TotalTimePosition = new Point(1380, 870),
                SaveInactiveDrawPoint = new Point(1379, 642),
                DisableTimer = true,
                //DebugVideoProbeFile = baseDir + "debug_video_metadata.txt",
                NextPageOptions = ScreenChangeOptions.GenerateBasicNextOptions(1),
                NoLanguageDependency = true
            };
            screensController.AddScreen(previewScreen);

            var thxScreen = new ThanksEndScreen(uiDir + "6-Thank you.jpg", sc)
            {
                InactivityTimerInterval = 3000
            };
            screensController.AddScreen(thxScreen);
        }
    }
}