using System.Collections.Generic;
using System.Drawing;
using JetBrains.Annotations;
using NomadLogic.Common.Questionnaire;
using NomadLogic.Common.Respondent;
using NomadLogic.Common.Util;
using NomadLogic.iBrandDesktop.Ui.ApplicationScreen.Generic;
using NomadLogic.iBrandDesktop.Ui.ApplicationScreen.Generic.Helpers;
using NomadLogic.iBrandDesktop.Ui.ApplicationScreen.Generic.Helpers.ImageTransforms;
using NomadLogic.iBrandDesktop.Ui.ApplicationScreen.Generic.Helpers.ScreenDefinitions;

// ReSharper disable InconsistentNaming

namespace NomadLogic.iBrandDesktop.Ui.ApplicationScreen.FullscreenLoaders
{
    [PublicAPI]
    public class LoaderCCS202001 : IFullscreenLoader
    {
#pragma warning disable 612
        public void CreateScreens(TouchScreenController screensController, ScreenConfig sc)
#pragma warning restore 612
        {
            var uiDir = DirectoryUtil.GetLocalIbrandTerminalDataDir(sc.ProjectId, sc.QuestionnaireId, sc.TerminalId, DirectoryUtil.ProjectDataDirectoriesEnum.photobooth_ui, DirectoryUtil.ProjectDataDirectoriesModEnum.none);
            var baseDir = DirectoryUtil.GetLocalIbrandTerminalDir(sc.ProjectId, sc.QuestionnaireId, sc.TerminalId);

            sc.QuestionFirstPictureNo = 3;

            #region 1- Start
            var hello = new GenericScreen(uiDir + "1-Accueil.jpg", null, sc)
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
            screensController.AddScreen(hello);

            var start = new GenericScreen(uiDir + "2-Start_FR.jpg", null, sc)
            {
                DefinitionNavigation = new ScreenDefinitionNavigation
                {
                    ZoneList = new List<SectionSkipScreenData>
                    {
                        new SectionSkipScreenData
                        {
                            PageChangeOptions = ScreenChangeOptions.GenerateBasicNextOptions(),
                            Zone = new Rectangle(270, 452, 1130, 146)
                        }
                    }
                }
            };
            screensController.AddScreen(start);
            #endregion

            #region Take Photo

            // 4- Prise photo
            var generators = new List<IAnimatedImageSequenceGenerator>
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


            var picture = new TakePictureAutoScreen(sc, uiDir + "3-Décompte_FR.jpg", null, null)
            {
                ImageListTranformOptions = new ImageListTransformOptions()
                {
                    FirstPictureOptions = new ImageListTransformOptionsFirstPicture
                    {
                        //SaveToQuestion = 3,
                        SaveDataModel = new DataModelQuestionnaire
                        {
                            Identifier = new QuestionIdentifier(3)
                        },
                        CanvasSize = new Size(1555,1079),
                        NewSize = new Size(1555,1079),
                        OverlaySelector = new ImageSelectorSingle(baseDir + "Printout_landscape.png"),
                        OverlayPos = new Point(0,0)
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
            #endregion
            
            #region Preview Screen
            var preview = new PreviewScreen(uiDir + "4-Preview_FR.jpg", null, sc,
                null, // Cancel
                new Rectangle(910, 837, 495, 90), // Next
                new Rectangle(280, 837, 495, 90), // Back
                200, new Size(777, 540));
            screensController.AddScreen(preview);
            #endregion
            
            #region Courriel+SMS
            var mailScreen = new GenericScreen(uiDir + "5-Infos_FR.jpg", null, sc)
                {
                    DefinitionTextbox = new ScreenDefinitionTextBoxes
                    {
                        TextboxDefinitions = new List<TextboxDefinition>
                        {
                            new TextboxDefinition
                            {
                                Position = new Rectangle(411, 259, 810, 42),
                                AllowEmpty = true,
                                DataModels = new List<DataModelBase>{
                                    new DataModelQuestionnaire
                                {
                                    Identifier = new QuestionIdentifier(1)
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
                            },
                            new TextboxDefinition
                            {
                                Position = new Rectangle(411, 348, 810, 42),
                                AllowEmpty = true,
                                DataModels = new List<DataModelBase>{
                                    new DataModelQuestionnaire
                                {
                                    Identifier = new QuestionIdentifier(2)
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
                                    {
                                        UserErrorEnum.Validation, new Dictionary<string, string>
                                        {
                                            {"en", "The sms is not valid."},
                                            {"fr", "Le SMS est invalide."}
                                        }
                                    },
                                    {
                                        UserErrorEnum.Mandatory, new Dictionary<string, string>
                                        {
                                            {"en", "You must enter your sms."},
                                            {"fr", "Vous devez entrer votre numéro de SMS."}
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
                                Zone = new Rectangle(588, 837, 504, 90)
                            }
                        }
                    },
                    DefinitionKeyboard = new ScreenDefinitionKeyboard
                    {
                        Position = new Rectangle(-1, 399, 1500, 416),
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
                var thanks = new GenericScreen(uiDir + "7-Merci.jpg", null, sc)
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