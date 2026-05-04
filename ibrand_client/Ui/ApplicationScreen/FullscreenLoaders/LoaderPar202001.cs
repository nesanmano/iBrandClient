using System.Collections.Generic;
using System.Drawing;
using JetBrains.Annotations;
using NLog;
using NomadLogic.Common;
using NomadLogic.Common.Questionnaire;
using NomadLogic.Common.Respondent;
using NomadLogic.Common.Util;
using NomadLogic.iBrandDesktop.Ui.ApplicationScreen.Generic;
using NomadLogic.iBrandDesktop.Ui.ApplicationScreen.Generic.Helpers;
using NomadLogic.iBrandDesktop.Ui.ApplicationScreen.Generic.Helpers.ImageTransforms;
using NomadLogic.iBrandDesktop.Ui.ApplicationScreen.Generic.Helpers.ScreenDefinitions;

#pragma warning disable 612

namespace NomadLogic.iBrandDesktop.Ui.ApplicationScreen.FullscreenLoaders
{
    [PublicAPI]
    public class LoaderPar202001 : IFullscreenLoader
    {
        [System.Diagnostics.CodeAnalysis.SuppressMessage("CodeQuality", "IDE0052:Remove unread private members", Justification = "Okazou")]
        private static readonly Logger log = LogManager.GetCurrentClassLogger();

        public void CreateScreens(TouchScreenController screensController, ScreenConfig sc)
        {
            var baseDir = DirectoryUtil.GetLocalIbrandTerminalDir(sc.ProjectId, sc.QuestionnaireId, sc.TerminalId);
            sc.QuestionFirstPictureNo = 1;

            var cancel = new SectionSkipScreenData
            {
                PageChangeOptions = ScreenChangeOptions.GenerateCancelOptions(),
                Zone = new Rectangle(1580, 0, 100, 100),
                SkipValidation = true
            };

            #region 1- Choix Langue
            var home = new GenericScreen(baseDir + "1. Tap to start.jpg", null, sc)
            {
                DefinitionLanguageZones = new ScreenDefinitionLanguageZones()
                {
                    Mandatory = true,
                    SelectionZones = new List<LanguageZone>
                    {
                        new LanguageZone
                        {
                            Zone = new Rectangle(270, 746, 414, 139),
                            PageChangeOptions = ScreenChangeOptions.GenerateBasicNextOptions(),
                            LanguageCode = "fr",
                            AnswersUponSelection = new List<Answer>
                            {
                                new Answer{QuestionType = QuestionTypeEnum.Combo, QuestionNo = 7, ChoiceNo = 2}
                            }
                        },
                        new LanguageZone
                        {
                            Zone = new Rectangle(993, 746, 414, 139),
                            PageChangeOptions = ScreenChangeOptions.GenerateBasicNextOptions(),
                            LanguageCode = "en",
                            AnswersUponSelection = new List<Answer>
                            {
                                new Answer{QuestionType = QuestionTypeEnum.Combo, QuestionNo = 7, ChoiceNo = 1}
                            }
                        }
                    }
                }
            };
            screensController.AddScreen(home);
            #endregion
            
            #region 2- Start
            var start = new GenericScreen(baseDir + "2. Start EN.jpg", baseDir + "2. Start FR.jpg", sc)
            {
                DefinitionNavigation = new ScreenDefinitionNavigation
                {
                    ZoneList = new List<SectionSkipScreenData>
                    {
                        new SectionSkipScreenData
                        {
                            PageChangeOptions = ScreenChangeOptions.GenerateBasicNextOptions(),
                            Zone = new Rectangle(276, 456, 1129, 139)
                        },
                        cancel
                    }
                }
            };
            screensController.AddScreen(start);
            #endregion

            #region Prise photo

            var countSeq = new AnimatedImageSequence
            {
                Loop = false,
                DrawLocation = new Point(783, 610),
                ImageGenerators = new List<IAnimatedImageSequenceGenerator>
                {
                    new AnimatedImageSequenceGeneratorBasicFiles
                    {
                        FilePaths = new List<string>
                        {
                            baseDir + "5.png",
                            baseDir + "4.png",
                            baseDir + "3.png",
                            baseDir + "2.png",
                            baseDir + "1.png"
                        }
                    }
                }
            };

            var transOpt = new ImageListTransformOptions()
            {
                FirstPictureOptions = new ImageListTransformOptionsFirstPicture()
                {
                    NewSize = new Size(1920, 1280),
                    OverlaySelector = new ImageSelectorSingle()
                    {
                        ImageFile = baseDir + "overlay.png"
                    },
                    SaveDataModel = new DataModelQuestionnaire
                    {
                        Identifier = new QuestionIdentifier(1)
                    },
                }
            };

            var picture = new TakePictureAutoScreen(sc, baseDir + "3. Countdown EN.jpg", baseDir + "3. Countdown FR.jpg", null)
            {
                CountImageSequence = countSeq,
                ImageListTranformOptions = transOpt,
                
            };
            screensController.AddScreen(picture);
            #endregion

            #region Preview
            var preview = new PreviewScreen(baseDir + "4. Preview EN.jpg", baseDir + "4. Preview FR.jpg", sc,
                cancel.Zone, // Cancel
                new Rectangle(908, 912, 498 ,86), // Next
                new Rectangle(278, 912, 498 ,86), // Back
                170, new Size(1050, 700));
            screensController.AddScreen(preview);
            #endregion
            
            #region Opt-Ins
            var optScreen = new GenericScreen(baseDir + "5. Opt-in EN.jpg", baseDir + "5. Opt-in FR.jpg", sc)
            {
                DefinitionCheckBoxes = new ScreenDefinitionCheckBoxes
                {
                    CheckImageFileName = "check2.png",
                    CheckboxDefinitions = new List<ScreenCheckOption>
                    {
                        new ScreenCheckOption
                        {
                            DataModels = new List<DataModelBase>{
                                new DataModelQuestionnaire
                            {
                                Identifier = new QuestionIdentifier(5)
                            }},
                            Mandatory = true,
                            CheckLocation = new Point(125, 256),
                            Position = new Rectangle(119, 251, 1440, 55),
                            FieldName = "discharge",
                            UserMessages = new Dictionary<UserErrorEnum, IDictionary<string, string>>
                            {
                                {UserErrorEnum.Mandatory, new Dictionary<string, string>
                                {
                                    {"en", "You must agree to let Parc Safari use your picture for promotional purposes." },
                                    {"fr", "Vous devez accepter que Parc Safari utilise votre photo à des fins promotionelles." }
                                }}
                            }
                        },
                        new ScreenCheckOption
                        {
                            DataModels = new List<DataModelBase>{
                                new DataModelQuestionnaire
                            {
                                Identifier = new QuestionIdentifier(6)
                            }},
                            Mandatory = true,
                            CheckLocation = new Point(125, 378),
                            Position = new Rectangle(119, 372, 1440, 111),
                            FieldName = "opt-in",
                            UserMessages = new Dictionary<UserErrorEnum, IDictionary<string, string>>
                            {
                                {UserErrorEnum.Mandatory, new Dictionary<string, string>
                                {
                                    {"en", "You must agree to the opt-in from Parc Safari to continue." },
                                    {"fr", "Vous devez accepter les offres promotionnelles de Parc Safari pour continuer." }
                                }}
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
                            Zone = new Rectangle(590, 912, 499, 80)
                        },
                        cancel
                    }
                }
            };
            screensController.AddScreen(optScreen);
            #endregion
            
            #region Infos
            var infoScreen = new GenericScreen(baseDir + "6. Info EN.jpg", baseDir + "6. Info FR.jpg", sc)
            {
                DefinitionTextbox = new ScreenDefinitionTextBoxes
                {
                    TextboxDefinitions = new List<TextboxDefinition>
                    {
                        new TextboxDefinition
                        {
                            Position = new Rectangle(296, 258, 1104, 42),
                            AllowEmpty = false,
                            DataModels = new List<DataModelBase>{
                                new DataModelQuestionnaire
                            {
                                Identifier = new QuestionIdentifier(2)
                            }},
                            FieldName = "name",
                            Validation = new TextValidationBasic
                            {
                                Option = TextValidationBasic.TextValidationBasicEnum.Name
                            },
                            Modifications = TextModificationsEnum.Trim,
                            Mandatory = true,
                            UserMessages = new Dictionary<UserErrorEnum, IDictionary<string, string>>
                            {
                                { UserErrorEnum.Validation, new Dictionary<string, string>
                                    {
                                        { "en", "That name is invalid." },
                                        { "fr", "Ce nom est invalide." }
                                    }
                                },
                                {UserErrorEnum.Mandatory, new Dictionary<string, string>
                                {
                                    { "en", "You must enter your name." },
                                    { "fr", "Vous devez entrer votre nom." }
                                }}
                            }
                        },
                        new TextboxDefinition
                        {
                            Position = new Rectangle(296, 317, 1104, 42),
                            AllowEmpty = true,
                            DataModels = new List<DataModelBase>{
                                new DataModelQuestionnaire
                            {
                                Identifier = new QuestionIdentifier(3)
                            }},
                            FieldName = "courriel",
                            Validation = new TextValidationBasic
                            {
                                Option = TextValidationBasic.TextValidationBasicEnum.Email
                            },
                            Modifications = TextModificationsEnum.Trim,
                            Mandatory = true,
                            UserMessages = new Dictionary<UserErrorEnum, IDictionary<string, string>>
                            {
                                { UserErrorEnum.Validation, new Dictionary<string, string>
                                    {
                                        { "en", "The email is not valid." },
                                        { "fr", "Le courriel est invalide." }
                                    }
                                },
                                {UserErrorEnum.Mandatory, new Dictionary<string, string>
                                {
                                    {"en", "You must provide an email to continue." },
                                    {"fr", "Vous devez entrer un courriel pour continuer." }
                                }}
                            }
                        },
                        new TextboxDefinition
                        {
                            Position = new Rectangle(296, 383, 1104, 42),
                            AllowEmpty = true,
                            DataModels = new List<DataModelBase>{
                                new DataModelQuestionnaire
                            {
                                Identifier = new QuestionIdentifier(4)
                            }},
                            FieldName = "sms",
                            Validation = new TextValidationBasic
                            {
                                Option = TextValidationBasic.TextValidationBasicEnum.Sms
                            },
                            Modifications = TextModificationsEnum.Trim,
                            Mandatory = true,
                            UserMessages = new Dictionary<UserErrorEnum, IDictionary<string, string>>
                            {
                                { UserErrorEnum.Validation, new Dictionary<string, string>
                                    {
                                        { "en", "The mobile number is not valid." },
                                        { "fr", "Le numéro de téléphone est invalide." }
                                    }
                                },
                                {UserErrorEnum.Mandatory, new Dictionary<string, string>
                                {
                                    {"en", "You must enter a phone number to continue." },
                                    {"fr", "Vous devez entrer un numéro de téléphone pour continuer." }
                                }}
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
                            Zone = new Rectangle(590, 912, 499, 80)
                        },
                        cancel
                    }
                },
                DefinitionKeyboard = new ScreenDefinitionKeyboard
                {
                    Position = new Rectangle(-1, 491, 1540, 399),
                    KeyboardType = KeyboardTypeEnum.Multilingual,
                    FontDefinition = new FontDefinitionInfo
                    {
                        Size = 50,
                        FamilyName = "Segoe Print"
                    },
                    ColorGradient = new List<Color> {Color.White, Color.OldLace},
                    ColorContour = Color.Black,
                    WidthContour = 3.5f,
                    Padding = -2
                }
            };
            screensController.AddScreen(infoScreen);
            #endregion
            
            #region Thanks

            var thanks = new GenericScreen(baseDir + "7. Thank you EN.jpg", baseDir + "7. Thank you FR.jpg", sc)
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
            screensController.AddScreen(thanks);
            #endregion
        }
    }
}