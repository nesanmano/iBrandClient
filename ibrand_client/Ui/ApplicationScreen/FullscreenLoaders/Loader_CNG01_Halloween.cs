using System.Collections.Generic;
using System.Drawing;
using System.IO;
using JetBrains.Annotations;
using NomadLogic.Common.Condition;
using NomadLogic.Common.Questionnaire;
using NomadLogic.Common.Respondent;
using NomadLogic.Common.Util;
using NomadLogic.iBrandDesktop.Ui.ApplicationScreen.Generic;
using NomadLogic.iBrandDesktop.Ui.ApplicationScreen.Generic.Helpers;
using NomadLogic.iBrandDesktop.Ui.ApplicationScreen.Generic.Helpers.ImageTransforms;
using NomadLogic.iBrandDesktop.Ui.ApplicationScreen.Generic.Helpers.ImageTransforms.Base;
using NomadLogic.iBrandDesktop.Ui.ApplicationScreen.Generic.Helpers.ScreenDefinitions;
using NomadLogic.Multimedia;

namespace NomadLogic.iBrandDesktop.Ui.ApplicationScreen.FullscreenLoaders
{
    [PublicAPI]
    // ReSharper disable once InconsistentNaming
    public class Loader_CNG01_Halloween : IFullscreenLoader
    {
        public void CreateScreens(TouchScreenController screensController, ScreenConfig sc)
        {
            var baseDir = DirectoryUtil.GetLocalIbrandTerminalDir(sc.ProjectId, sc.QuestionnaireId, sc.TerminalId);
            var tmbDir = DirectoryUtil.GetLocalIbrandTerminalDataDir(sc.ProjectId, sc.QuestionnaireId, sc.TerminalId, 
                DirectoryUtil.ProjectDataDirectoriesEnum.pictures,
                DirectoryUtil.ProjectDataDirectoriesModEnum.thumbnail);

            sc.QuestionPictureToPrintNo = 1;

            var kbdFont = new FontDefinitionInfo
            {
                Size = 18,
                GraphicsUnit = GraphicsUnit.Pixel,
                Color = Color.Black,
                FamilyName = "Segoe Print",
                Style = "Bold"
            };

            var cancelZone = new SectionSkipScreenData
            {
                PageChangeOptions = ScreenChangeOptions.GenerateCancelOptions(),
                SkipValidation = true,
                Zone = new Rectangle(1280-50, 0, 50, 50)
            };

            // Start
            var screenStart = new GenericScreen(baseDir + "1-tap to start.jpg", baseDir + "1-tap to start.jpg", sc)
            {
                DefinitionNavigation = new ScreenDefinitionNavigation
                {
                    ZoneList = new List<SectionSkipScreenData>
                    {
                        new SectionSkipScreenData
                        {
                            PageChangeOptions = ScreenChangeOptions.GenerateBasicNextOptions(),
                            Zone = new Rectangle(0,0, 1280, 800)
                        }
                    }
                }
            };
            screensController.AddScreen(screenStart);

            // Select BKG
            var bkgScreen = new GenericScreen(baseDir + "2-select background.jpg", baseDir + "2-select background.jpg", sc)
            {
                DefinitionNavigation = new ScreenDefinitionNavigation
                {
                    ZoneList = new List<SectionSkipScreenData>
                    {
                        cancelZone,
                        new SectionSkipScreenData
                        {
                            PageChangeOptions = ScreenChangeOptions.GenerateBasicNextOptions()
                        }
                    }
                },
                DefinitionAnswerZones = new ScreenDefinitionAnswerZones
                {
                    Mandatory = true,
                    ThumbnailDirectory = new DirectoryInfo(tmbDir),
                    SelectionZones = new List<AnswerZone>
                    {
                        new AnswerZone
                        {
                            PageChangeOptions = ScreenChangeOptions.GenerateBasicNextOptions(),
                            BackgroundImageFilePath = baseDir + "background1.png",
                            AnswersUponSelection = new List<Answer>
                            {
                                new Answer
                                {
                                    QuestionIdentifier = new QuestionIdentifier(4, 1)
                                }
                            },
                            Zone = new Rectangle(27, 290, 390, 260)
                        },
                        new AnswerZone
                        {
                            PageChangeOptions = ScreenChangeOptions.GenerateBasicNextOptions(),
                            BackgroundImageFilePath = baseDir + "background2.png",
                            AnswersUponSelection = new List<Answer>
                            {
                                new Answer
                                {
                                    QuestionIdentifier = new QuestionIdentifier(4, 2)
                                }
                            },
                            Zone = new Rectangle(443, 290, 390, 260)
                        },
                        new AnswerZone
                        {
                            PageChangeOptions = ScreenChangeOptions.GenerateBasicNextOptions(),
                            BackgroundImageFilePath = baseDir + "background3.png",
                            AnswersUponSelection = new List<Answer>
                            {
                                new Answer
                                {
                                    QuestionIdentifier = new QuestionIdentifier(4, 3)
                                }
                            },
                            Zone = new Rectangle(860, 290, 390, 260)
                        }
                    }
                }
            };
            screensController.AddScreen(bkgScreen);

            // Take auto picture
            var takePictScreen = new TakePictureAutoScreen(sc, baseDir + "3-countdown.jpg", baseDir + "3-countdown.jpg",
                new Rectangle(266, 60, 728, 480) // preview
                )
            {
                CancelZone = cancelZone.Zone,
                ImageListTranformOptions = new ImageListTransformOptions
                {
                    ChromaOptions = new ImageListTransformOptionsChroma
                    {
                        Transforms = new List<ImageTransformChromaKey>
                        {
                            new ImageTransformChromaKey
                            {
                                Condition = new QuestionCondition(4,1),
                                SaveToQuestion = 1,
                                CanvasLayer = new CanvasLayer
                                {
                                    CanvasFileType = ImageFileTypeEnum.Png,
                                    FileName = baseDir + "background1.png",
                                    Size = new Size(1598, 1065)
                                },
                                PictureLayer = new ImageLayer
                                {
                                    UseChromaKey = true,
                                    Scale = new Size(1598, 1065)
                                },
                                Frame = new ImageFileLayer
                                {
                                    FilePath = baseDir + "overlay.png"
                                }
                            },
                            new ImageTransformChromaKey
                            {
                                Condition = new QuestionCondition(4,2),
                                SaveToQuestion = 1,
                                CanvasLayer = new CanvasLayer
                                {
                                    CanvasFileType = ImageFileTypeEnum.Png,
                                    FileName = baseDir + "background2.png",
                                    Size = new Size(1598, 1065)
                                },
                                PictureLayer = new ImageLayer
                                {
                                    UseChromaKey = true,
                                    Scale = new Size(1598, 1065)
                                },
                                Frame = new ImageFileLayer
                                {
                                    FilePath = baseDir + "overlay.png"
                                }
                            },
                            new ImageTransformChromaKey
                            {
                                Condition = new QuestionCondition(4,3),
                                SaveToQuestion = 1,
                                CanvasLayer = new CanvasLayer
                                {
                                    CanvasFileType = ImageFileTypeEnum.Png,
                                    FileName = baseDir + "background3.png",
                                    Size = new Size(1598, 1065)
                                },
                                PictureLayer = new ImageLayer
                                {
                                    UseChromaKey = true,
                                    Scale = new Size(1598, 1065)
                                },
                                Frame = new ImageFileLayer
                                {
                                    FilePath = baseDir + "overlay.png"
                                }
                            }
                        }
                    }
                },
                CountImageSequence = new AnimatedImageSequence
                {
                    Loop = false,
                    DrawLocation = new Point(565, 620),
                    ImageGenerators = new List<IAnimatedImageSequenceGenerator>
                    {
                        new AnimatedImageSequenceGeneratorNumbers
                        {
                            ProceduralImageTextInfo = new ProceduralImageTextInfo
                            {
                                BitmapSize = new Size(150, 150),
                                FontFamilly = FontFamily.GenericSerif.ToString(),
                                FontSize = 80,
                                FontStyle = FontStyle.Bold,
                                Color = Color.FromArgb(238, 142, 0)
                            },
                            MaxValue = 5,
                            ReverseOrder = true
                        }
                        /*
                        , new AnimatedImageSequenceGeneratorBasicFiles
                        {
                            FilePaths = new List<string> { DirectoryUtil.GetLocalPhotoBoothDefaultAssetsDir() + "flash.png" } // 96x102
                        }
                        */
                    }
                }
            };
            screensController.AddScreen(takePictScreen);

            // Preview
            var previewScreen = new GenericScreen(baseDir + "4-preview.jpg", baseDir + "4-preview.jpg", sc)
            {
                DefinitionNavigation = new ScreenDefinitionNavigation
                {
                    ZoneList = new List<SectionSkipScreenData>
                    {
                        cancelZone,
                        new SectionSkipScreenData
                        {
                            PageChangeOptions = ScreenChangeOptions.GenerateBasicBackOptions(),
                            SkipValidation = true,
                            Zone = new Rectangle(308,602,285,44)
                        },
                        new SectionSkipScreenData
                        {
                            PageChangeOptions = ScreenChangeOptions.GenerateBasicNextOptions(),
                            SkipValidation = true,
                            Zone = new Rectangle(688,602,285,44)
                        }
                    }
                },
                DefinitionImagePreviews = new ScreenDefinitionPreviews
                {
                    CheckForReloadEachActivation = true,
                    PreviewDefinitions = new List<ScreenDefinitionPreviewImageBase>
                    {
                        new ScreenDefinitionPreviewImageQuestion
                        {
                            PreviewQuestionIdentifier = new QuestionIdentifier(1),
                            PreviewZone = new Rectangle(266, 60, 728, 480)
                        }
                    }
                }
            };
            screensController.AddScreen(previewScreen);

            // Share
            var shareScreen = new ShareScreen(baseDir + "5-sharing.jpg", sc, null, null, null,
                new Rectangle(1037, 279, 143, 148), null, null,
                null, null,
                new Rectangle(550, 602, 180, 44),
                new Rectangle(266, 60, 728, 480), null,
                baseDir + "5-sharing.jpg"
            )
            {
                InactivityTimerInterval = 30*1000,
                OverrideEndBySkip = 1,
                DontShowInactiveImages = true
            };
            screensController.AddScreen(shareScreen);

            #region No longer used
            /*
            // Mail screen
            var mailScreen = new GenericScreen(baseDir + "6-e-mail.jpg", baseDir + "6-e-mail.jpg", sc)
            {
                DefinitionNavigation = new ScreenDefinitionNavigation
                {
                    ZoneList = new List<SectionSkipScreenData>
                    {
                        new SectionSkipScreenData
                        {
                            PageChangeOptions = ScreenChangeOptions.GenerateBasicNextOptions(-1),
                            Zone = new Rectangle(549, 190, 180, 44)
                        }
                    }
                },
                DefinitionTextbox = new ScreenDefinitionTextBoxes
                {
                    TextboxDefinitions = new List<TextboxDefinition>
                    {
                        new TextboxDefinition
                        {
                            DataModel = new DataModelQuestionnaire
                            {
                                Identifier = new QuestionIdentifier(3)
                            },
                            Position = new Rectangle(350, 96, 578, 31),
                            FieldName = "courriel",
                            Mandatory = false,
                            AllowEmpty = true,
                            Modifications = TextModificationsEnum.Trim,
                            Validation = new TextValidationBasic
                            {
                                Option = TextValidationBasic.TextValidationBasicEnum.Email
                            }
                        }
                    }
                },
                DefinitionKeyboard = new ScreenDefinitionKeyboard
                {
                    Position = new Rectangle(45, 266, 1190, 425),
                    Padding = 0,
                    ColorContour = Color.Black,
                    KeyboardType = KeyboardTypeEnum.Email,
                    ColorGradient = new List<Color>
                    {
                        Color.FromArgb(238, 142, 0), Color.FromArgb(119, 71, 0)
                    },
                    ColorMouseDown = Color.FromArgb(51, 35, 0),
                    ColorSelected = Color.FromArgb(238, 221, 0),
                    FontDefinition = kbdFont
                }
            };
            screensController.AddScreen(mailScreen);

            // SMS screen
            var smsScreen = new GenericScreen(baseDir + "7-sms.jpg", baseDir + "7-sms.jpg", sc)
            {
                DefinitionNavigation = new ScreenDefinitionNavigation
                {
                    ZoneList = new List<SectionSkipScreenData>
                    {
                        new SectionSkipScreenData
                        {
                            PageChangeOptions = ScreenChangeOptions.GenerateBasicNextOptions(-2),
                            Zone = new Rectangle(549, 190, 180, 44)
                        }
                    }
                },
                DefinitionTextbox = new ScreenDefinitionTextBoxes
                {
                    TextboxDefinitions = new List<TextboxDefinition>
                    {
                        new TextboxDefinition
                        {
                            DataModel = new DataModelQuestionnaire
                            {
                                Identifier = new QuestionIdentifier(2)
                            },
                            Position = new Rectangle(350, 96, 578, 31),
                            FieldName = "sms",
                            Mandatory = false,
                            AllowEmpty = true,
                            Modifications = TextModificationsEnum.Trim,
                            Validation = new TextValidationBasic
                            {
                                Option = TextValidationBasic.TextValidationBasicEnum.Sms
                            }
                        }
                    }
                },
                DefinitionKeyboard = new ScreenDefinitionKeyboard
                {
                    Position = new Rectangle(349,266,578,425),
                    Padding = 0,
                    ColorContour = Color.Black,
                    KeyboardType = KeyboardTypeEnum.Number,
                    ColorGradient = new List<Color>
                    {
                        Color.FromArgb(238, 142, 0), Color.FromArgb(119, 71, 0)
                    },
                    ColorMouseDown = Color.FromArgb(51, 35, 0),
                    ColorSelected = Color.FromArgb(238, 221, 0),
                    FontDefinition = kbdFont
                }
            };
            screensController.AddScreen(smsScreen);
            */
            #endregion

            // Thx!
            var thxScreen = new GenericScreen(baseDir + "8-merci.jpg", baseDir + "8-merci.jpg", sc)
            {
                DefinitionTimer = new ScreenDefinitionTimerOptions
                {
                    InactivityTimerInterval = 10*1000,
                    TimerAction = TimerActionEnum.End
                },
                DefinitionNavigation = new ScreenDefinitionNavigation
                {
                    ZoneList = new List<SectionSkipScreenData>
                    {
                        new SectionSkipScreenData
                        {
                            PageChangeOptions = ScreenChangeOptions.GenerateBasicNextOptions(),
                            Zone = new Rectangle(0,0,1598,1065)
                        }
                    }
                }
            };
            screensController.AddScreen(thxScreen);
        }
    }
}