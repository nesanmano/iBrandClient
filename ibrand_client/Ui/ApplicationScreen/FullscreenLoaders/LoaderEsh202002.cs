using System.Collections.Generic;
using System.Drawing;
using System.IO;
using Newtonsoft.Json;
using NomadLogic.Common;
using NomadLogic.Common.Questionnaire;
using NomadLogic.Common.Respondent;
using NomadLogic.Common.Util;
using NomadLogic.Common.Util.Json;
using NomadLogic.iBrandDesktop.Ui.ApplicationScreen.Generic;
using NomadLogic.iBrandDesktop.Ui.ApplicationScreen.Generic.Helpers;
using NomadLogic.iBrandDesktop.Ui.ApplicationScreen.Generic.Helpers.ScreenDefinitions;
using NomadLogic.Multimedia;

// ReSharper disable UnusedMember.Global

#pragma warning disable 612
namespace NomadLogic.iBrandDesktop.Ui.ApplicationScreen.FullscreenLoaders
{
    public class LoaderEsh202002 : IFullscreenLoader
    {
        public void CreateScreens(TouchScreenController screensController, ScreenConfig sc)
        {
            var uiDir = DirectoryUtil.GetLocalIbrandTerminalDataDir(sc.ProjectId, sc.QuestionnaireId, sc.TerminalId, DirectoryUtil.ProjectDataDirectoriesEnum.photobooth_ui, DirectoryUtil.ProjectDataDirectoriesModEnum.none);
            var baseDir = DirectoryUtil.GetLocalIbrandTerminalDir(sc.ProjectId, sc.QuestionnaireId, sc.TerminalId);
            var videoDir = DirectoryUtil.GetLocalIbrandTerminalCustomDir(sc.ProjectId, sc.QuestionnaireId, sc.TerminalId, "files", DirectoryUtil.ProjectDataDirectoriesModEnum.none);

            var cancelRectangle = new Rectangle(1580, 0, 100, 100);
            var cancel = new SectionSkipScreenData
            {
                PageChangeOptions = ScreenChangeOptions.GenerateBasicNextOptions(),
                Zone = cancelRectangle
            };

            var fileConfig = new Esh202002Config();
            var configFilePath = new FileInfo(baseDir + "test_config.json");
            if (configFilePath.Exists)
            {
                fileConfig = JsonUtil.ReadJsonFile<Esh202002Config>(configFilePath);
            }

            var videoFile = new FileInfo(videoDir + fileConfig.VideoFileName);
            if (!videoFile.Exists)
            {
                throw new System.Exception("Illegal video file: " + videoFile.FullName);
            }

            #region 1- Start
            var start = new GenericScreen(uiDir + "ui_01.jpg", null, sc)
            {
                DefinitionNavigation = new ScreenDefinitionNavigation
                {
                    ZoneList = new List<SectionSkipScreenData>
                    {
                        new SectionSkipScreenData
                        {
                            PageChangeOptions = ScreenChangeOptions.GenerateBasicNextOptions(),
                            Zone = new Rectangle(0,0,1680,1050)
                        }
                    }
                }
            };
            screensController.AddScreen(start);
            #endregion

            #region Courriel

            if (!fileConfig.HideEmail)
            {
                var mailScreen = new GenericScreen(uiDir + "ui_03.jpg", null, sc)
                {
                    DefinitionEshotsEventDayTransfer = new ScreenDefinitionEshotsEventDayTransfer
                    {
                        EventDayIdQuestionNo = 3,
                        ShowErrorDialog = true
                    },
                    DefinitionTextbox = new ScreenDefinitionTextBoxes
                    {
                        TextboxDefinitions = new List<TextboxDefinition>
                        {
                            new TextboxDefinition
                            {
                                Position = new Rectangle(373, 236, 1167, 37),
                                AllowEmpty = false,
                                DataModels = new List<DataModelBase>{
                                    new DataModelQuestionnaire
                                {
                                    Identifier = new QuestionIdentifier(4)
                                }},
                                FieldName = "mail",
                                Validation = new TextValidationBasic
                                {
                                    Option = TextValidationBasic.TextValidationBasicEnum.Email
                                },
                                Modifications = TextModificationsEnum.Trim,
                                Mandatory = true,
                                UserMessages = new Dictionary<UserErrorEnum, IDictionary<string, string>>
                                {
                                    {
                                        UserErrorEnum.Validation, new Dictionary<string, string>
                                        {
                                            {"en", "The email is not valid."},
                                            {"fr", "Le courriel est invalide."}
                                        }
                                    },
                                    {
                                        UserErrorEnum.Mandatory, new Dictionary<string, string>
                                        {
                                            {"en", "You must enter your email."},
                                            {"fr", "Vous devez entrer votre courriel."}
                                        }
                                    }
                                }
                            }
                        }
                    },
                    DefinitionNavigation = new ScreenDefinitionNavigation
                    {
                        ZoneList = new List<SectionSkipScreenData>
                        {
                            new SectionSkipScreenData
                            {
                                PageChangeOptions = ScreenChangeOptions.GenerateBasicNextOptions(),
                                Zone = new Rectangle(596, 894, 486, 78)
                            },
                            cancel
                        }
                    },
                    DefinitionKeyboard = new ScreenDefinitionKeyboard
                    {
                        Position = new Rectangle(-1, 336, 1500, 520),
                        KeyboardType = KeyboardTypeEnum.Email,
                        FontDefinition = new FontDefinitionInfo
                        {
                            Size = 20
                        }
                    }
                };
                screensController.AddScreen(mailScreen);
            }

            #endregion

            #region 2- Take Video

            IList<AnimatedImageSequenceEnumTrigger<RecordVideoScreenStates>> sequences = new List<AnimatedImageSequenceEnumTrigger<RecordVideoScreenStates>>
            {
                new AnimatedImageSequenceEnumTrigger<RecordVideoScreenStates>
                {
                    EnumFlags = new List<RecordVideoScreenStates>
                    {
                        RecordVideoScreenStates.Transfering
                    },
                    ImageSequence = new AnimatedImageSequence
                    {
                        DrawLocation = new Point(318, 25),
                        Loop = true,
                        ImageGenerators = new List<IAnimatedImageSequenceGenerator>
                        {
                            new AnimatedImageSequenceGeneratorFromText
                            {
                                Texts = new List<string>
                                {
                                    "Transferring.",
                                    "Transferring..",
                                    "Transferring..."
                                },
                                ProceduralImageTextInfo = new ProceduralImageTextInfo
                                {
                                    Color = Color.White,
                                    FontSize = 30,
                                    FontFamilly = "Arial",
                                    BitmapSize = new Size(500, 50)
                                }
                            }
                        }
                    }
                },
                new AnimatedImageSequenceEnumTrigger<RecordVideoScreenStates>
                {
                    EnumFlags = new List<RecordVideoScreenStates>
                    {
                        RecordVideoScreenStates.PreRecording
                    },
                    ImageSequence = new AnimatedImageSequence
                    {
                        DrawLocation = new Point(318, 25),
                        Loop = false,
                        ImageGenerators = new List<IAnimatedImageSequenceGenerator>
                        {
                            new AnimatedImageSequenceGeneratorFromText
                            {
                                Texts = new List<string>
                                {
                                    "Prepare yourself"
                                },
                                ProceduralImageTextInfo = new ProceduralImageTextInfo
                                {
                                    Color = Color.White,
                                    FontSize = 30,
                                    FontFamilly = "Arial",
                                    BitmapSize = new Size(500, 50)
                                }
                            }
                        }
                    }
                }
            };

            var picture = new RecordVideoWithPresentation(sc, uiDir + "ui_02.jpg", null, fileConfig.CountDown, cancelRectangle, false)
            {
                VideoZone = fileConfig.VideoZone,
                ExternalVideoFile = videoFile,
                ForceSaveVideoToQuestionNo = 1,
                AutomaticStart = true,
                ImageSequences = sequences,
                UseCountdownDecorator = true,
                CountdownDecorator = new CountdownDecoratorBasicImages
                {
                    DoNotShowWhenRecording = true,
                    MaxTimeCountdown = fileConfig.CountDown,
                    UpperCenterPointMessage = new Point(880, 90),
                    UpperCenterPointNumber = new Point(1200, 90),
                    ImageSize = new Size(130, 130)
                }
            };
            screensController.AddScreen(picture);

            #endregion

            #region Thanks
            var thanks = new GenericScreen(uiDir + "ui_04.jpg", null, sc)
            {
                DefinitionNavigation = new ScreenDefinitionNavigation
                {
                    ZoneList = new List<SectionSkipScreenData>
                    {
                        new SectionSkipScreenData
                        {
                            PageChangeOptions = ScreenChangeOptions.GenerateBasicNextOptions(),
                            Zone = new Rectangle(0, 0, 1680, 1050)
                        }
                    }
                },
                DefinitionTimer = new ScreenDefinitionTimerOptions
                {
                    TimerAction = TimerActionEnum.End,
                    InactivityTimerInterval = 5000
                }
            };
            
            if (fileConfig.HideEmail)
            {
                thanks.ScreenDefinitionDebug = new ScreenDefinitionDebug
                {
                    DefaultAnswers = new List<Answer>
                    {
                        new Answer{QuestionType = QuestionTypeEnum.Email, QuestionNo = 4, ChoiceNo = 1, Response = "debugvalue@nomadlogic.com"}
                    }
                };
                thanks.DefinitionEshotsEventDayTransfer = new ScreenDefinitionEshotsEventDayTransfer
                {
                    EventDayIdQuestionNo = 3,
                    ShowErrorDialog = true
                };
            }
            screensController.AddScreen(thanks);
            #endregion
        }
    }

    public class Esh202002Config
    {
        public bool HideEmail { get; set; }
        public int CountDown { get; set; } = 5;
        [JsonRequired]
        public string VideoFileName { get; set; } = @"medium.mp4";
        [JsonRequired]
        public Rectangle VideoZone { get; set; } = new Rectangle(200, 200, 720, 460);
    }
}