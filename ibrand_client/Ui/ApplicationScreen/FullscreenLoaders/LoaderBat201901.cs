using System.Collections.Generic;
using System.Drawing;
using System.IO;
using JetBrains.Annotations;
using NomadLogic.Common.Condition;
using NomadLogic.Common.Questionnaire;
using NomadLogic.Common.Respondent;
using NomadLogic.Common.Util;
using NomadLogic.Common.Util.Json;
using NomadLogic.iBrandDesktop.Ui.ApplicationScreen.custom.Batimo2019;
using NomadLogic.iBrandDesktop.Ui.ApplicationScreen.Generic;
using NomadLogic.iBrandDesktop.Ui.ApplicationScreen.Generic.Helpers;
using NomadLogic.iBrandDesktop.Ui.ApplicationScreen.Generic.Helpers.ImageTransforms;
using NomadLogic.iBrandDesktop.Ui.ApplicationScreen.Generic.Helpers.ImageTransforms.Base;
using NomadLogic.iBrandDesktop.Ui.ApplicationScreen.Generic.Helpers.ScreenDefinitions;
using NomadLogic.Multimedia;

namespace NomadLogic.iBrandDesktop.Ui.ApplicationScreen.FullscreenLoaders
{
    [PublicAPI]
    public class LoaderBat201901 : IFullscreenLoader
    {
        public void CreateScreens(TouchScreenController screensController, ScreenConfig sc)
        {
            var uiDir = DirectoryUtil.GetLocalIbrandTerminalDataDir(sc.ProjectId, sc.QuestionnaireId, sc.TerminalId,
                DirectoryUtil.ProjectDataDirectoriesEnum.photobooth_ui, DirectoryUtil.ProjectDataDirectoriesModEnum.none);
            var baseDir = DirectoryUtil.GetLocalIbrandTerminalDir(sc.ProjectId, sc.QuestionnaireId, sc.TerminalId);

            var configFile = new FileInfo(baseDir + "terminal.json");
            const bool debugWritePropertie = false; // ENLEVER LORSQU'ON NE DEBUG PAS

            var uiNames = new string[]
            {
                uiDir + "1-Langues.jpg",
                uiDir + "1.1- choixBackground.jpg",
                uiDir + "1.1- choixBackground_fr.jpg",
                uiDir + "2-Start.jpg",
                uiDir + "2-Start_fr.jpg",
                uiDir + "3-Countdown.jpg",
                uiDir + "3-Countdown_fr.jpg",
                uiDir + "4-Preview.jpg",
                uiDir + "4-Preview_fr.jpg",
                uiDir + "5-Share.jpg",
                uiDir + "5-Share_fr.jpg",
                uiDir + "6.1-Courriel.jpg",
                uiDir + "6.1-Courriel_fr.jpg",
                uiDir + "6.2-SMS.jpg",
                uiDir + "6.2-SMS_fr.jpg",
                uiDir + "7-Merci.jpg",
                uiDir + "7-Merci_fr.jpg"
            };

            var langIndex = 0;
            var bkgIndex = 1;
            var startIndex = 3;
            var countIndex = 5;
            var previewIndex = 7;
            var shareIndex = 9;
            var mailIndex = 11;
            var smsIndex = 13;
            var thxIndex = 15;

            #region DEBUG
            // DEBUG création du fichier de config original
            if (!configFile.Exists)
            {
                var config = new Bat201901Config
                {
                    UseLanguageSelection = true,
                    UseBackgroundSelection = true,
                    BackGroundSelectionConfigFile = "bkg_sel.json",
                    PreviewLocation = new Rectangle(-1, 140, 400, 300),
                    CountDownLocation = new Point(780, 630),
                    TransformationOptionsFile = "transformation_config.json",
                    TransformPreviewLocation = new Rectangle(-1, 187, 900, 600),
                    UseQuestionnaireScreen = true
                };
                JsonUtil.WriteJsonFile(configFile, config);
                configFile.Refresh();

                var bkgSel = new FileInfo(baseDir + config.BackGroundSelectionConfigFile);
                if (!bkgSel.Exists)
                {
                    var bkgConfig = new List<AnswerZone>
                    {
                        new AnswerZone
                        {
                            PageChangeOptions = ScreenChangeOptions.GenerateBasicNextOptions(),
                            BackgroundImageFilePath = "background1_p.jpg",
                            Zone = new Rectangle(200, 100, 100, 100)
                        }
                    };
                    JsonUtil.WriteJsonFile(bkgSel, bkgConfig);
                }

                var transOptions = new FileInfo(baseDir + config.TransformationOptionsFile);
                if (!transOptions.Exists)
                {
                    var transConfig = new ImageListTransformOptionsChroma
                    {
                        Transforms = new List<ImageTransformChromaKey>
                        {
                            new ImageTransformChromaKey
                            {
                                Condition = new MultipleCondition
                                {
                                    Operator = BooleanOperatorEnum.AND,
                                    Conditions = new List<ICondition>
                                    {
                                        new QuestionCondition
                                        {
                                            QuestionNo = 1,
                                            ChoiceNo = 1,
                                            //SubChoiceNo = 1
                                        },
                                        new QuestionCondition
                                        {
                                            QuestionNo = 3,
                                            ChoiceNo = 1,
                                            //SubChoiceNo = 1,
                                            Not = true
                                        }
                                    }
                                },
                                SaveToQuestion = 6,
                                CanvasLayer = new CanvasLayer
                                {
                                    CanvasFileType = ImageFileTypeEnum.Jpeg,
                                    Size = new Size(1598, 1065),
                                    FileName = "bkg.jpg"
                                },
                                PictureLayer = new ImageLayer
                                {
                                    UseChromaKey = true,
                                    Scale = new Size(1598, 1065),
                                    Position = new Point(0, 0),
                                    Center = true
                                },
                                Frame = new ImageFileLayer 
                                {
                                    Position = new Point(0, 0),
                                    Scale = new Size(1598, 1065),
                                    FilePath = "overlay_fr.png"
                                }
                            },
                            new ImageTransformChromaKey
                            {
                                Condition = new MultipleCondition
                                {
                                    Operator = BooleanOperatorEnum.AND,
                                    Conditions = new List<ICondition>
                                    {
                                        new QuestionCondition
                                        {
                                            QuestionNo = 1,
                                            ChoiceNo = 1,
                                            //SubChoiceNo = 2
                                        },
                                        new QuestionCondition
                                        {
                                            QuestionNo = 3,
                                            ChoiceNo = 1,
                                            //SubChoiceNo = 1,
                                            Not = true
                                        }
                                    }
                                },
                                SaveToQuestion = 6,
                                CanvasLayer = new CanvasLayer
                                {
                                    CanvasFileType = ImageFileTypeEnum.Jpeg,
                                    Size = new Size(1598, 1065),
                                    FileName = "bkg.jpg"
                                },
                                PictureLayer = new ImageLayer
                                {
                                    UseChromaKey = true,
                                    Scale = new Size(1598, 1065),
                                    Position = new Point(0, 0),
                                    Center = true
                                },
                                Frame = new ImageFileLayer 
                                {
                                    Position = new Point(0, 0),
                                    Scale = new Size(1598, 1065),
                                    FilePath = "overlay_en.png"
                                }
                            }
                        }
                    };
                    JsonUtil.WriteJsonFile(transOptions, transConfig);
                }
            }
            #endregion

            var screenOptions = JsonUtil.ReadJsonFile<Bat201901Config>(configFile);

            #region Langue (optionel)

            if (screenOptions.UseLanguageSelection)
            {
                var lang = new GenericScreen(uiNames[langIndex], uiNames[langIndex], sc)
                {
                    DebugWritePropertiesToFiles = debugWritePropertie,
                    DefinitionLanguageZones = new ScreenDefinitionLanguageZones
                    {
                        ConfigFileName = "01_lang.json",
                        /*
                        SelectionZones = new List<LanguageZone>
                        {
                            new LanguageZone
                            {
                                LanguageCode = "fr",
                                Zone = new Rectangle(279, 523, 1127, 139),
                                PageChangeOptions = ScreenChangeOptions.GenerateBasicNextOptions(),
                                AnswersUponSelection = new List<Answer>
                                {
                                    new Answer(3, 1, 1, 1, "fr")
                                }
                            }
                        }
                        */
                    }
                };
                screensController.AddScreen(lang);
            }

            #endregion

            #region Background selection (Optionel)

            if (screenOptions.UseBackgroundSelection)
            {
                var bsts = new BackgroundSelectionScreen(uiNames[bkgIndex], uiNames[bkgIndex+1], sc)
                {
                    BackgroundSelectionOptionsConfigFile = screenOptions.BackGroundSelectionConfigFile
                };
                screensController.AddScreen(bsts);
            }

            #endregion

            #region Start Screen

            var startSCreen = new GenericScreen(uiNames[startIndex], uiNames[startIndex+1], sc)
            {
                DebugWritePropertiesToFiles = debugWritePropertie,
                DefinitionNavigation = new ScreenDefinitionNavigation
                {
                    ConfigFileName = "02_nav.json",
                    /*
                    ZoneList = new List<SectionSkipScreenData>
                    {
                        new SectionSkipScreenData
                        {
                            PageChangeOptions = ScreenChangeOptions.GenerateBasicNextOptions(),
                            Zone = new Rectangle(398, 575, 880, 141)
                        },
                        new SectionSkipScreenData
                        {
                            PageChangeOptions = ScreenChangeOptions.GenerateCancelOptions(),
                            Zone = new Rectangle(1530, 0, 150, 150),
                            SkipValidation = true
                        }
                    }
                    */
                }
            };
            screensController.AddScreen(startSCreen);

            #endregion

            #region TakePicture + Transforms)

            var transFile = new FileInfo(baseDir + screenOptions.TransformationOptionsFile);
            var transforms = JsonUtil.ReadJsonFile<ImageListTransformOptionsChroma>(transFile);

            var previewZone = screenOptions.PreviewLocation;
            var picture = new TakePictureAutoScreen(sc, uiNames[countIndex], uiNames[countIndex+1],
                previewZone, //Preview
                5
            )
            {
                CancelZone = new Rectangle(1530, 0, 150, 150),
                //SaveOriginalImageToQuestion = 2,
                SaveOriginalImage = new DataModelQuestionnaire {Identifier = new QuestionIdentifier(2)},
                ImageListTranformOptions = new ImageListTransformOptions()
                {
                    ChromaOptions = transforms
                },
                CountImageSequence = new AnimatedImageSequence
                {
                    Loop = false,
                    DrawLocation = screenOptions.CountDownLocation,
                    ImageGenerators = new List<IAnimatedImageSequenceGenerator>
                    {
                        new AnimatedImageSequenceGeneratorNumbers
                        {
                            MaxValue = 5,
                            ReverseOrder = true,
                            ProceduralImageTextInfo = new ProceduralImageTextInfo
                            {
                                Color = Color.Black,
                                FontSize = 100,
                                BackgroundColor = Color.Transparent
                            }
                        }
                    }
                }
            };
            screensController.AddScreen(picture);

            #endregion

            // 5- Preview
            #region Preview Screen
            var preview = new PreviewScreen(uiNames[previewIndex], uiNames[previewIndex+1], sc,
                new Rectangle(1530, 0, 150, 150), // Cancel
                new Rectangle(908, 837, 499, 90), // Next
                new Rectangle(278, 837, 499, 90), // Back
                screenOptions.TransformPreviewLocation.Y,
                screenOptions.TransformPreviewLocation.Size,
                null)
            {
            };
            screensController.AddScreen(preview);
            #endregion
            
            // On imprime maintenant sans demander quoi que ce soit: Un Share Screen
            var printScreen = new ShareScreen(uiNames[shareIndex], sc, null, null, null,
                new Rectangle(1396, 149, 132, 132), // Print
                new Rectangle(1396, 549, 132, 132), // MAIL
                new Rectangle(1396, 349, 132, 132), // SMS
                null, null,
                new Rectangle(716, 770, 248, 79), // End
                screenOptions.TransformPreviewLocation,
                null,
                uiNames[shareIndex+1])
            {
                DontShowInactiveImages = true,
                OverrideEndBySkip = 3,
                ForcePrintOnActivation = false,
                MailScreenSkip = 2,
                SmsScreenSkip = 1
            };
            screensController.AddScreen(printScreen);

            #region SMS+Courriel

            var smsScreen = new GenericScreen(uiNames[smsIndex], uiNames[smsIndex+1], sc)
            {
                DebugWritePropertiesToFiles = debugWritePropertie,
                DefinitionTextbox = new ScreenDefinitionTextBoxes
                {
                    ConfigFileName = "sms_text.json",
                    /*
                    TextboxDefinitions = new List<TextboxDefinition>
                    {
                        new TextboxDefinition
                        {
                            Position = new Rectangle(420, 162, 888, 54),
                            AllowEmpty = true,
                            QuestionNo = 5,
                            FieldName = "sms",
                            Validation = new TextValidationBasic
                            {
                                Option = TextValidationBasic.TextValidationBasicEnum.Sms
                            },
                            Modifications = TextModificationsEnum.Trim,
                            Mandatory = false,
                            UserMessages = new Dictionary<UserErrorEnum, IDictionary<string, string>>
                            {
                                { UserErrorEnum.Validation, new Dictionary<string, string>
                                    {
                                        { "en", "The mobile number is not valid." },
                                        { "fr", "Le numéro de téléphone est invalide." }
                                    }
                                }
                            }
                        }
                    }
                    */
                },
                DefinitionNavigation = new ScreenDefinitionNavigation
                {
                    ConfigFileName = "sms_nav.json",
                    /*
                    ZoneList = new List<SectionSkipScreenData>
                    {
                        new SectionSkipScreenData
                        {
                            PageChangeOptions = ScreenChangeOptions.GenerateBasicBackOptions(),
                            Zone = new Rectangle(712, 320, 256, 80)
                        }
                    }
                    */
                },
                DefinitionKeyboard = new ScreenDefinitionKeyboard
                {
                    ConfigFileName = "sms_key.json",
                    /*
                    Position = new Rectangle(-1, 430, 888, 576),
                    KeyboardType = KeyboardTypeEnum.Number
                    */
                    FontDefinition = new FontDefinitionInfo
                    {
                        Size = 15
                    }
                }
            };
            screensController.AddScreen(smsScreen);

            var mailScreen = new GenericScreen(uiNames[mailIndex], uiNames[mailIndex+1], sc)
            {
                DebugWritePropertiesToFiles = debugWritePropertie,
                DefinitionTextbox = new ScreenDefinitionTextBoxes
                {
                    ConfigFileName = "mail_text.json",
                    /*
                    TextboxDefinitions = new List<TextboxDefinition>
                    {
                        new TextboxDefinition
                        {
                            Position = new Rectangle(420, 162, 888, 54),
                            AllowEmpty = true,
                            QuestionNo = 4,
                            FieldName = "mail",
                            Validation = new TextValidationBasic
                            {
                                Option = TextValidationBasic.TextValidationBasicEnum.Email
                            },
                            Modifications = TextModificationsEnum.Trim,
                            Mandatory = false,
                            UserMessages = new Dictionary<UserErrorEnum, IDictionary<string, string>>
                            {
                                { UserErrorEnum.Validation, new Dictionary<string, string>
                                    {
                                        { "en", "The email is not valid." },
                                        { "fr", "Le courriel est invalide." }
                                    }
                                }
                            }
                        }
                    }
                    */
                },
                DefinitionNavigation = new ScreenDefinitionNavigation
                {
                    ConfigFileName = "mail_nav.json",
                    /*
                    ZoneList = new List<SectionSkipScreenData>
                    {
                        new SectionSkipScreenData
                        {
                            PageChangeOptions = ScreenChangeOptions.GenerateBasicBackOptions(-2),
                            Zone = new Rectangle(712, 320, 256, 80)
                        }
                    }
                    */
                },
                DefinitionKeyboard = new ScreenDefinitionKeyboard
                {
                    ConfigFileName = "mail_key.json",
                    /*
                    Position = new Rectangle(-1, 430, 1564, 576),
                    KeyboardType = KeyboardTypeEnum.Email
                    */
                }
            };
            screensController.AddScreen(mailScreen);
            #endregion

            // Thx
            var thxScreen = new ThanksEndScreen(uiNames[thxIndex], sc, uiNames[thxIndex+1])
            {
                InactivityTimerInterval = 5000
            };
            screensController.AddScreen(thxScreen);

        }
    }
}