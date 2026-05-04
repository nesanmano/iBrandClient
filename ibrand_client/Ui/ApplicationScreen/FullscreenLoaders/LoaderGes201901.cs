using System.Collections.Generic;
using System.Drawing;
using JetBrains.Annotations;
using NomadLogic.Common.Condition;
using NomadLogic.Common.Questionnaire;
using NomadLogic.Common.Respondent;
using NomadLogic.Common.Util;
using NomadLogic.iBrandDesktop.Ui.ApplicationScreen.Generic;
using NomadLogic.iBrandDesktop.Ui.ApplicationScreen.Generic.Helpers;
using NomadLogic.iBrandDesktop.Ui.ApplicationScreen.Generic.Helpers.ImageTransforms;
using NomadLogic.iBrandDesktop.Ui.ApplicationScreen.Generic.Helpers.ScreenDefinitions;
using NomadLogic.Multimedia;

namespace NomadLogic.iBrandDesktop.Ui.ApplicationScreen.FullscreenLoaders
{
    [PublicAPI]
    public class LoaderGes201901 : IFullscreenLoader
    {
        public void CreateScreens(TouchScreenController screensController, ScreenConfig sc)
        {
            var uiDir = DirectoryUtil.GetLocalIbrandTerminalDataDir(sc.ProjectId, sc.QuestionnaireId, sc.TerminalId,
                DirectoryUtil.ProjectDataDirectoriesEnum.photobooth_ui, DirectoryUtil.ProjectDataDirectoriesModEnum.none);
            var baseDir = DirectoryUtil.GetLocalIbrandTerminalDir(sc.ProjectId, sc.QuestionnaireId, sc.TerminalId);

            #region Start
            var home = new GenericScreen(uiDir + "1-start.jpg", null, sc)
            {
                DefinitionNavigation = new ScreenDefinitionNavigation
                {
                    ZoneList = new List<SectionSkipScreenData>
                    {
                        new SectionSkipScreenData
                        {
                            Zone = new Rectangle(0, 0, 1680, 1050),
                            PageChangeOptions = ScreenChangeOptions.GenerateBasicNextOptions()
                        }
                    }
                }
            };
            screensController.AddScreen(home);
            #endregion

            // N'est plus utilisé
            #region Choix de BKG
            /*
            var bkg = new GenericScreen(uiDir + "2-choose bkg.jpg", null, sc)
            {
                DefinitionAnswerZones = new ScreenDefinitionAnswerZones
                {
                    Mandatory = true,
                    UserMessages = new Dictionary<UserErrorEnum, IDictionary<string, string>>
                    {
                        {UserErrorEnum.Mandatory, new Dictionary<string, string>
                        {
                            {"fr", "Vous devez choisir un fond d'écran avant de continuer."}
                        } }
                    },
                    SelectionZones = new List<AnswerZone>
                    {
                        new AnswerZone
                        {
                            Zone = new Rectangle(476, 488, 330, 322),
                            PageChangeOptions = ScreenChangeOptions.GenerateBasicNextOptions(),
                            BackgroundImageFilePath = "preview_1.png",
                            AnswersUponSelection = new List<Answer>
                            {
                                new Answer
                                {
                                    QuestionNoInt = 3,
                                    ChoiceNoInt = 1,
                                    SubChoiceNoInt = 1
                                }
                            }
                        },
                        new AnswerZone
                        {
                            Zone = new Rectangle(876, 488, 330, 322),
                            PageChangeOptions = ScreenChangeOptions.GenerateBasicNextOptions(),
                            BackgroundImageFilePath = "preview_2.png",
                            AnswersUponSelection = new List<Answer>
                            {
                                new Answer
                                {
                                    QuestionNoInt = 3,
                                    ChoiceNoInt = 1,
                                    SubChoiceNoInt = 2
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
                            SkipValidation = true,
                            Zone = new Rectangle(1580, 0, 100, 100),
                            PageChangeOptions = ScreenChangeOptions.GenerateCancelOptions()
                        }
                    }
                }
            };
            screensController.AddScreen(bkg);
            */
            #endregion
            
            #region Prise photo

            var countSeq = new AnimatedImageSequence
            {
                Loop = false,
                DrawLocation = new Point(1275, 400),
                ImageGenerators = new List<IAnimatedImageSequenceGenerator>
                {
                    new AnimatedImageSequenceGeneratorNumbers
                    {
                        ReverseOrder = true,
                        MaxValue = 5,
                        ProceduralImageTextInfo = new ProceduralImageTextInfo
                        {
                            Color = Color.Yellow,
                            BitmapSize = new Size(200, 200),
                            Center = true,
                            FontSize = 150
                        }
                    }
                }
            };

            var transOpt = new ImageListTransformOptions()
            {
                FirstPictureOptions = new ImageListTransformOptionsFirstPicture
                {
                    SaveDataModel = new DataModelQuestionnaire
                    {
                        Identifier = new QuestionIdentifier(5)
                    },
                    CanvasSize = new Size(1080, 920),
                    NewSize = new Size(1380, 920),
                    NewPos = new Point(-150, 0),
                    OverlaySelector = new ImageSelectorSingle
                    {
                        ImageFile = baseDir + "overlay_1.png"
                    },
                    JpegSaveQuality = 90
                }
            };

            var picture = new TakePictureAutoScreen(sc, uiDir + "3-take picture.jpg", null,
                new Rectangle(-1, 80, 704, 600), 5)
            {
                SaveOriginalImage = new DataModelQuestionnaire {Identifier = new QuestionIdentifier(7)},
                ManualStartZone = new Rectangle(681, 810, 319, 81),
                CountImageSequence = countSeq,
                ImageListTranformOptions = transOpt
            };
            screensController.AddScreen(picture);
            #endregion

            #region Preview
            var preview = new PreviewScreen(uiDir + "4-preview.jpg", null, sc,
                new Rectangle(1580,0,100,100), // Cancel
                new Rectangle(873, 812, 315, 77), // Next
                new Rectangle(492, 812, 315, 77), // Back
                80, new Size(704, 600));
            screensController.AddScreen(preview);
            #endregion
            
            #region Questionnaire
            var questionnaire = new GenericScreen(uiDir + "5-questionnaire.jpg", null, sc)
            {
                DefinitionTextbox = new ScreenDefinitionTextBoxes
                {
                    DefaultFontDefinition = new FontDefinitionInfo
                    {
                        Size = 40,
                        FamilyName = "Arial"
                    },
                    TextboxDefinitions = new List<TextboxDefinition>
                    {
                        new TextboxDefinition
                        {
                            DataModels = new List<DataModelBase>{
                                new DataModelQuestionnaire
                            {
                                Identifier = new QuestionIdentifier(1)
                            }},
                            Mandatory = false,
                            AllowEmpty = true,
                            Validation = new TextValidationBasic
                            {
                                Option = TextValidationBasic.TextValidationBasicEnum.Sms
                            },
                            FieldName = "Sms",
                            Modifications = TextModificationsEnum.Trim,
                            Position = new Rectangle(356, 203, 1000, 37),
                            UserMessages = new Dictionary<UserErrorEnum, IDictionary<string, string>>
                            {
                                {UserErrorEnum.Validation, new Dictionary<string, string>
                                {
                                    {"fr", "Le numéro de téléphone est invalide."}
                                } }
                            }
                        },
                        new TextboxDefinition
                        {
                            DataModels = new List<DataModelBase>{
                                new DataModelQuestionnaire
                            {
                                Identifier = new QuestionIdentifier(2)
                            }},
                            Mandatory = false,
                            AllowEmpty = true,
                            Validation = new TextValidationBasic
                            {
                                Option = TextValidationBasic.TextValidationBasicEnum.Email
                            },
                            FieldName = "courriel",
                            Modifications = TextModificationsEnum.Trim,
                            Position = new Rectangle(356, 303, 1000, 37),
                            UserMessages = new Dictionary<UserErrorEnum, IDictionary<string, string>>
                            {
                                {UserErrorEnum.Validation, new Dictionary<string, string>
                                {
                                    {"fr", "Le courriel est invalide."}
                                } }
                            }
                        }
                    }
                },
                DefinitionKeyboard = new ScreenDefinitionKeyboard
                {
                    Position = new Rectangle(-1, 378, 1575, 389),
                    ColorContour = Color.Transparent,
                    ColorGradient = new List<Color>
                    {
                        Color.Gray, Color.Black
                    },
                    ColorMouseDown = Color.Goldenrod,
                    KeyboardType = KeyboardTypeEnum.Email,
                    Padding = 5,
                    FontDefinition = new FontDefinitionInfo
                    {
                        Size = 25,
                        Color = Color.Yellow,
                        FamilyName = "Arial",
                        Style = "Bold"
                    }
                },
                DefinitionNavigation = new ScreenDefinitionNavigation
                {
                    ZoneList = new List<SectionSkipScreenData>
                    {
                        new SectionSkipScreenData
                        {
                            SkipValidation = true,
                            Zone = new Rectangle(1580,0,100,100),
                            PageChangeOptions = ScreenChangeOptions.GenerateCancelOptions()
                        },
                        new SectionSkipScreenData
                        {
                            Zone = new Rectangle(681, 811, 318, 80),
                            PageChangeOptions = new ScreenChangeOptions
                            {
                                Options = new List<ScreenChangeOption>
                                {
                                    new ScreenChangeOption
                                    {
                                        ValidationTrigger = true,
                                        Condition = new MultipleCondition
                                        {
                                            Operator = BooleanOperatorEnum.OR,
                                            Conditions = new List<ICondition>
                                            {
                                                new QuestionCondition()
                                                {
                                                    QuestionNo = 1
                                                },
                                                new QuestionCondition()
                                                {
                                                    QuestionNo = 2
                                                }
                                            }
                                        }
                                    }
                                }
                            }
                        }
                    }
                }
            };
            screensController.AddScreen(questionnaire);
            #endregion
            
            #region Thanks
            var thanks = new ThanksEndScreen(uiDir + "6-thanks.jpg", sc);
            screensController.AddScreen(thanks);
            #endregion

        }
    }
}