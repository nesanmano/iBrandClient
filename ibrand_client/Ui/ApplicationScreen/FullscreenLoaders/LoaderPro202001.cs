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
    public class LoaderPro202001 : IFullscreenLoader
    {
        private static readonly Logger log = LogManager.GetCurrentClassLogger();

        public void CreateScreens(TouchScreenController screensController, ScreenConfig sc)
        {
            var uiDir = DirectoryUtil.GetLocalIbrandTerminalDataDir(sc.ProjectId, sc.QuestionnaireId, sc.TerminalId,
                DirectoryUtil.ProjectDataDirectoriesEnum.photobooth_ui, DirectoryUtil.ProjectDataDirectoriesModEnum.none);

            sc.QuestionPictureToPrintNo = 3;
            sc.QuestionPictureTransformedNo = 3;
            sc.QuestionNumberOfPrintNo = 6;

            var cancel = new SectionSkipScreenData
            {
                PageChangeOptions = ScreenChangeOptions.GenerateBasicNextOptions(),
                Zone = new Rectangle(276, 456, 1129, 139)
            };

            #region 1- Choix Langue
            var home = new GenericScreen(uiDir + "1-Accueil.jpg", null, sc)
            {
                DefinitionLanguageZones = new ScreenDefinitionLanguageZones()
                {
                    Mandatory = true,
                    SelectionZones = new List<LanguageZone>
                    {
                        new LanguageZone
                        {
                            Zone = new Rectangle(280, 331, 384, 131),
                            PageChangeOptions = ScreenChangeOptions.GenerateBasicNextOptions(),
                            LanguageCode = "fr",
                            AnswersUponSelection = new List<Answer>
                            {
                                new Answer{QuestionType = QuestionTypeEnum.Combo, QuestionNo = 1, ChoiceNo = 2}
                            }
                        },
                        new LanguageZone
                        {
                            Zone = new Rectangle(1016, 331, 384, 131),
                            PageChangeOptions = ScreenChangeOptions.GenerateBasicNextOptions(),
                            LanguageCode = "en",
                            AnswersUponSelection = new List<Answer>
                            {
                                new Answer{QuestionType = QuestionTypeEnum.Combo, QuestionNo = 1, ChoiceNo = 1}
                            }
                        }
                    }
                }
            };
            screensController.AddScreen(home);
            #endregion

            #region 2- Start
            var start = new GenericScreen(uiDir + "2-Start_EN.jpg", uiDir + "2-Start_FR.jpg", sc)
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
                            uiDir + "5.png",
                            uiDir + "4.png",
                            uiDir + "3.png",
                            uiDir + "2.png",
                            uiDir + "1.png"
                        }
                    }
                }
            };

            var transOpt = new ImageListTransformOptions()
            {
                ImageMagickOptions = new List<ImageListTransformOptionsImageMagick>
                {
                    new ImageListTransformOptionsImageMagick
                    {
                        CommandFile = "image_script.txt",
                        ResultMapSaveModels = new Dictionary<string, DataModelBase>
                        {
                            {"square", new DataModelQuestionnaire {Identifier = new QuestionIdentifier(2)}},
                            {"print", new DataModelQuestionnaire {Identifier = new QuestionIdentifier(3)}}
                        }
                    }
                }
            };

            var picture = new TakePictureAutoScreen(sc, uiDir + "3-Décompte_EN.jpg", uiDir + "3-Décompte_FR.jpg", null)
            {
                CountImageSequence = countSeq,
                SaveOriginalImage = new DataModelQuestionnaire {Identifier = new QuestionIdentifier(2)},
                ImageListTranformOptions = transOpt
            };
            screensController.AddScreen(picture);
            #endregion

            #region Preview
            var preview = new PreviewScreen(uiDir + "4-Preview_EN.jpg", uiDir + "4-Preview_FR.jpg", sc,
                cancel.Zone, // Cancel
                new Rectangle(912, 842, 489, 79), // Next
                new Rectangle(282, 842, 489, 79), // Back
                100, new Size(470, 705));
            screensController.AddScreen(preview);
            #endregion

            #region Share
            var printScreen = new ShareScreen(uiDir + "5-Print_EN.jpg", sc, null, null, null,
                new Rectangle(1250, 362, 117, 119), // Print
                new Rectangle(321, 555, 117, 119), // MAIL
                new Rectangle(321, 296, 117, 119), // SMS
                null, null,
                new Rectangle(585, 840, 497, 89), // End
                100, new Size(470, 705),
                uiDir + "5-Print_FR.jpg")
            {
                DontShowInactiveImages = false,
                OverrideEndBySkip = 3,
                ForcePrintOnActivation = false,
                MailScreenSkip = 2,
                SmsScreenSkip = 1
            };
            screensController.AddScreen(printScreen);
            #endregion

            #region SMS
            var smsScreen = new GenericScreen(uiDir + "6-SMS_EN.jpg", uiDir + "6-SMS.jpg", sc)
            {
                DefinitionTextbox = new ScreenDefinitionTextBoxes
                {
                    TextboxDefinitions = new List<TextboxDefinition>
                    {
                        new TextboxDefinition
                        {
                            Position = new Rectangle(543, 233, 846, 55),
                            AllowEmpty = true,
                            DataModels = new List<DataModelBase>{
                                new DataModelQuestionnaire
                            {
                                Identifier = new QuestionIdentifier(5)
                            }},
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
                },
                DefinitionNavigation = new ScreenDefinitionNavigation
                {
                    ZoneList = new List<SectionSkipScreenData>
                    {
                        new SectionSkipScreenData
                        {
                            PageChangeOptions = ScreenChangeOptions.GenerateBasicBackOptions(),
                            Zone = new Rectangle(590, 837, 499, 90)
                        },
                        cancel
                    }
                },
                DefinitionKeyboard = new ScreenDefinitionKeyboard
                {
                    Position = new Rectangle(-1, 348, 450, 450),
                    KeyboardType = KeyboardTypeEnum.Number,
                    FontDefinition = new FontDefinitionInfo
                    {
                        Size = 35
                    }
                }
            };
            screensController.AddScreen(smsScreen);
            #endregion
            
            #region Courriel
            var mailScreen = new GenericScreen(uiDir + "7-Email_EN.jpg", uiDir + "7-Courriel_FR.jpg", sc)
            {
                DefinitionTextbox = new ScreenDefinitionTextBoxes
                {
                    TextboxDefinitions = new List<TextboxDefinition>
                    {
                        new TextboxDefinition
                        {
                            Position = new Rectangle(543, 233, 846, 55),
                            AllowEmpty = true,
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
                },
                DefinitionNavigation = new ScreenDefinitionNavigation
                {
                    ZoneList = new List<SectionSkipScreenData>
                    {
                        new SectionSkipScreenData
                        {
                            PageChangeOptions = ScreenChangeOptions.GenerateBasicBackOptions(-2),
                            Zone = new Rectangle(590, 837, 499, 90)
                        },
                        cancel
                    }
                },
                DefinitionKeyboard = new ScreenDefinitionKeyboard
                {
                    Position = new Rectangle(-1, 348, 1534, 452),
                    KeyboardType = KeyboardTypeEnum.Email,
                    FontDefinition = new FontDefinitionInfo
                    {
                        Size = 20
                    }
                }
            };
            screensController.AddScreen(mailScreen);
            #endregion

            #region Thanks
            var thanks = new ThanksEndScreen(uiDir + "8-Thanks.jpg", sc, uiDir + "8-Merci.jpg");
            screensController.AddScreen(thanks);
            #endregion
        }
    }
}