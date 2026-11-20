using JetBrains.Annotations;
using NomadLogic.Common.Condition;
using NomadLogic.Common.Questionnaire;
using NomadLogic.Common.Respondent;
using NomadLogic.Common.Util;
using NomadLogic.Common.Util.Json;
using NomadLogic.iBrandDesktop.Ui.ApplicationScreen.Generic;
using NomadLogic.iBrandDesktop.Ui.ApplicationScreen.Generic.Helpers;
using NomadLogic.iBrandDesktop.Ui.ApplicationScreen.Generic.Helpers.ImageTransforms;
using NomadLogic.iBrandDesktop.Ui.ApplicationScreen.Generic.Helpers.ImageTransforms.Base;
using NomadLogic.iBrandDesktop.Ui.ApplicationScreen.Generic.Helpers.ScreenDefinitions;
using NomadLogic.Multimedia;
using System.Collections.Generic;
using System.Drawing;
using System.IO;

namespace NomadLogic.iBrandDesktop.Ui.ApplicationScreen.FullscreenLoaders
{
    [PublicAPI]
    public class Loader_Hydro_QC : IFullscreenLoader
    {
        public void CreateScreens(TouchScreenController screensController, ScreenConfig sc)
        {
            var baseDir = DirectoryUtil.GetLocalIbrandTerminalDir(sc.ProjectId, sc.QuestionnaireId, sc.TerminalId);
            sc.QuestionFirstPictureNo = 9;

            var cancel = new SectionSkipScreenData
            {
                PageChangeOptions = ScreenChangeOptions.GenerateCancelOptions(),
                Zone = new Rectangle(1580, 0, 100, 100),
                SkipValidation = true
            };

            // 1 — HOME SCREEN
            var home = new GenericScreen(baseDir + "bkg_01.jpg", baseDir + "bkg_01.jpg", sc)
            {
                DefinitionNavigation = new ScreenDefinitionNavigation
                {
                    ZoneList = new List<SectionSkipScreenData>
                    {
                        new SectionSkipScreenData
                        {
                            PageChangeOptions = ScreenChangeOptions.GenerateBasicNextOptions(),
                            Zone = new Rectangle(757, 457, 1160, 840)
                        }
                    }
                }
            };
            screensController.AddScreen(home);

            // 2 — OPT-IN SCREEN
            var optin = new GenericScreen(baseDir + "bkg_02.jpg", baseDir + "bkg_02.jpg", sc)
            {

                DefinitionNavigation = new ScreenDefinitionNavigation
                {
                    ZoneList = new List<SectionSkipScreenData>
                    {
                        new SectionSkipScreenData
                        {
                            PageChangeOptions = ScreenChangeOptions.GenerateBasicNextOptions(),
                            Zone = new Rectangle(468, 640, 795, 736)
                        },
                        new SectionSkipScreenData
                        {
                            PageChangeOptions = ScreenChangeOptions.GenerateBasicBackOptions(),
                            Zone = new Rectangle(1128, 638, 1453, 729),
                            SkipValidation = true
                        }
                    }
                }
            };
            screensController.AddScreen(optin);

            // 3 — EMAIL SCREEN
            var email = new GenericScreen(baseDir + "bkg_03.jpg", baseDir + "bkg_03.jpg", sc)
            {
                DefinitionTextbox = new ScreenDefinitionTextBoxes
                {
                    TextboxDefinitions = new List<TextboxDefinition>
                    {
                        new TextboxDefinition
                        {
                            Position = new Rectangle(300, 350, 1100, 60),
                            AllowEmpty = false,
                            DataModels = new List<DataModelBase>
                            {
                                new DataModelQuestionnaire { Identifier = new QuestionIdentifier(4) }
                            },
                            FieldName = "email",
                            Validation = new TextValidationBasic
                            {
                                Option = TextValidationBasic.TextValidationBasicEnum.Email
                            },
                            Mandatory = true
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
                            Zone = new Rectangle(785, 390, 1133, 483)
                        },
                        cancel
                    }
                }
            };
            screensController.AddScreen(email);

            // 4 — BACKGROUND SELECTION
            var background = new GenericScreen(baseDir + "bkg_11b.jpg", baseDir + "bkg_11b.jpg", sc)
            {

                /*DefinitionCheckBoxes = new ScreenDefinitionCheckBoxes
                {
                    CheckImageFileName = "check2.png",
                    CheckboxDefinitions = new List<ScreenCheckOption>
                    {
                        new ScreenCheckOption
                        {
                            DataModels = new List<DataModelBase>{
                                new DataModelQuestionnaire
                            {
                                Identifier = new QuestionIdentifier(1,1)
                            }},
                            Mandatory = false,
                            CheckLocation = new Point(220, 754),
                            Position = new Rectangle(215, 738, 261, 784),
                            FieldName = "helicoptere"
                        },
                        new ScreenCheckOption
                        {
                            DataModels = new List<DataModelBase>{
                                new DataModelQuestionnaire
                            {
                                Identifier = new QuestionIdentifier(1,2)
                            }},
                            Mandatory = false,
                            CheckLocation = new Point(700, 754),
                            Position = new Rectangle(697, 738, 741, 784),
                            FieldName = "village"
                        }
                        ,
                        new ScreenCheckOption
                        {
                            DataModels = new List<DataModelBase>{
                                new DataModelQuestionnaire
                            {
                                Identifier = new QuestionIdentifier(1,3)
                            }},
                            Mandatory = false,
                            CheckLocation = new Point(1173, 754),
                            Position = new Rectangle(1172, 741, 1218, 785),
                            FieldName = "ciment"
                        }
                        ,
                        new ScreenCheckOption
                        {
                            DataModels = new List<DataModelBase>{
                                new DataModelQuestionnaire
                            {
                                Identifier = new QuestionIdentifier(1,4)
                            }},
                            Mandatory = false,
                            CheckLocation = new Point(1649, 754),
                            Position = new Rectangle(1647, 740, 1693, 785),
                            FieldName = "camion",

                        }
                    }
                },*/

                DefinitionAnswerZones = new ScreenDefinitionAnswerZones
                {
                    Mandatory = true,
                    SelectionZones = new List<AnswerZone>
                    {
                            new AnswerZone
                            {
                                PageChangeOptions = ScreenChangeOptions.GenerateBasicNextOptions(),
                                BackgroundImageFilePath = baseDir + "vue_helico_thumb.png",
                                AnswersUponSelection = new List<Answer>
                                {
                                    new Answer { QuestionIdentifier = new QuestionIdentifier(1, 1),Response = baseDir + "vue_helico_thumb.png" },
                                    new Answer { QuestionIdentifier = new QuestionIdentifier(12, 1), Response ="",QuestionType= Common.QuestionTypeEnum.ExternalUniqueIdentifier  }
                                },
                                Zone = new Rectangle(142, 420, 400, 300)
                            },
                            new AnswerZone
                            {
                                PageChangeOptions = ScreenChangeOptions.GenerateBasicNextOptions(),
                                BackgroundImageFilePath = baseDir + "village_thumb.png",
                                AnswersUponSelection = new List<Answer>
                                {
                                    new Answer { QuestionIdentifier = new QuestionIdentifier(1, 2), Response = baseDir + "village_thumb.png" },
                                    new Answer { QuestionIdentifier = new QuestionIdentifier(12, 1), Response ="",QuestionType= Common.QuestionTypeEnum.ExternalUniqueIdentifier  }
                                },
                                Zone = new Rectangle(552, 420, 400, 300)
                            },
                            new AnswerZone
                            {
                                PageChangeOptions = ScreenChangeOptions.GenerateBasicNextOptions(),
                                BackgroundImageFilePath = baseDir + "ciment_thumb.png",
                                AnswersUponSelection = new List<Answer>
                                {
                                    new Answer { QuestionIdentifier = new QuestionIdentifier(1, 3), Response = baseDir + "ciment_thumb.png"  },
                                    new Answer { QuestionIdentifier = new QuestionIdentifier(12, 1), Response ="",QuestionType= Common.QuestionTypeEnum.ExternalUniqueIdentifier }
                                },
                                Zone = new Rectangle(962, 420, 400, 300)
                            },
                            new AnswerZone
                            {
                                PageChangeOptions = ScreenChangeOptions.GenerateBasicNextOptions(),
                                BackgroundImageFilePath = baseDir + "camion_thumb.png",
                                AnswersUponSelection = new List<Answer>
                                {
                                    new Answer { QuestionIdentifier = new QuestionIdentifier(1, 4), Response = baseDir + "camion_thumb.png" },
                                    new Answer { QuestionIdentifier = new QuestionIdentifier(12, 1), Response ="",QuestionType= Common.QuestionTypeEnum.ExternalUniqueIdentifier  }
                                },
                                Zone = new Rectangle(1372, 420, 400, 300)
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
                            Zone = new Rectangle(785, 879, 1132, 972),
                            SkipValidation = false
                        },
                        cancel
                    }
                }
            };
            screensController.AddScreen(background);

            var generators = new List<IAnimatedImageSequenceGenerator>
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
            };


            // 5 — TAKE PICTURE
            var picture = new TakePictureAutoScreen(sc,
                baseDir + "bkg_11.jpg",
                baseDir + "bkg_11.jpg",
                null)
            {
                    CancelZone = new Rectangle(1530, 0, 150, 150),
                    SaveOriginalImage = new DataModelQuestionnaire { Identifier = new QuestionIdentifier(2) },
                    CountImageSequence = new AnimatedImageSequence
                    {
                        Loop = false,
                        DrawLocation = new Point(799, 294),
                        ImageGenerators = new List<IAnimatedImageSequenceGenerator>
                        {
                            new AnimatedImageSequenceGeneratorNumbers
                            {
                                ProceduralImageTextInfo = new ProceduralImageTextInfo
                                {
                                    BitmapSize = new Size(300, 300),
                                    FontFamilly = FontFamily.GenericSerif.ToString(),
                                    FontSize = 300,
                                    FontStyle = FontStyle.Bold,
                                    Color = Color.FromArgb(255, 255, 255)
                                },
                                MaxValue = 5,
                                ReverseOrder = true
                            }
                        }
                    },
                    ImageListTranformOptions = new ImageListTransformOptions
                    {

                        ChromaOptions = new ImageListTransformOptionsChroma
                        {
                            Transforms = new List<ImageTransformChromaKey>
                            {
                                new ImageTransformChromaKey
                                {
                                    Condition = new QuestionCondition(1,1),
                                    SaveToQuestion = 9,
                                    CanvasLayer = new CanvasLayer
                                    {
                                        CanvasFileType = ImageFileTypeEnum.Png,
                                        FileName = baseDir + "vue_helico.png",
                                        Size = new Size(1598, 1065)
                                    },
                                    PictureLayer = new ImageLayer
                                    {
                                        UseChromaKey = true,
                                        Scale = new Size(1598, 1065)
                                    }
                                },
                                new ImageTransformChromaKey
                                {
                                    Condition = new QuestionCondition(1,2),
                                    SaveToQuestion = 9,
                                    CanvasLayer = new CanvasLayer
                                    {
                                        CanvasFileType = ImageFileTypeEnum.Png,
                                        FileName = baseDir + "village.png",
                                        Size = new Size(1598, 1065)
                                    },
                                    PictureLayer = new ImageLayer
                                    {
                                        UseChromaKey = true,
                                        Scale = new Size(1598, 1065)
                                    }
                                },
                                new ImageTransformChromaKey
                                {
                                    Condition = new QuestionCondition(1,3),
                                    SaveToQuestion = 9,
                                    CanvasLayer = new CanvasLayer
                                    {
                                        CanvasFileType = ImageFileTypeEnum.Png,
                                        FileName = baseDir + "ciment.png",
                                        Size = new Size(1598, 1065)
                                    },
                                    PictureLayer = new ImageLayer
                                    {
                                        UseChromaKey = true,
                                        Scale = new Size(1598, 1065)
                                    }
                                },
                                 new ImageTransformChromaKey
                                {
                                    Condition = new QuestionCondition(1,4),
                                    SaveToQuestion = 9,
                                    CanvasLayer = new CanvasLayer
                                    {
                                        CanvasFileType = ImageFileTypeEnum.Png,
                                        FileName = baseDir + "camion.png",
                                        Size = new Size(1598, 1065)
                                    },
                                    PictureLayer = new ImageLayer
                                    {
                                        UseChromaKey = true,
                                        Scale = new Size(1598, 1065)
                                    }
                                }
                            }
                        }

                    },
                    CreatingImageSequence = new AnimatedImageSequence
                    {
                        DrawLocation = new Point(733, 614),
                        Loop = true,
                        ImageGenerators = new List<IAnimatedImageSequenceGenerator>()
                        {
                            new AnimatedImageSequenceGeneratorFromText()
                            {
                                ProceduralImageTextInfo = new ProceduralImageTextInfo
                                {
                                    Color = Color.White,
                                    FontSize = 50,
                                    BitmapSize = new Size(600,200),
                                    Center = false
                                },
                                TextsLang = new Dictionary<string, IList<string>>
                                {
                                    {"fr", new List<string>
                                        {
                                            "Traitement.",
                                            "Traitement..",
                                            "Traitement..."
                                        }
                                    },
                                    {"en", new List<string>
                                        {
                                            "Working.",
                                            "Working..",
                                            "Working..."
                                        }
                                    }
                                }
                            }
                        }
                    }
                };
            screensController.AddScreen(picture);


            var previewScreen = new GenericScreen(baseDir + "bkg_10.jpg", baseDir + "bkg_10.jpg", sc)
            {
                DefinitionNavigation = new ScreenDefinitionNavigation
                {
                    ZoneList = new List<SectionSkipScreenData>
                    {
                        new SectionSkipScreenData
                        {
                            PageChangeOptions = ScreenChangeOptions.GenerateCancelOptions(),
                            Zone = new Rectangle(1605, 0, 75, 75),
                            SkipValidation = true
                        },
                        new SectionSkipScreenData
                        {
                            PageChangeOptions = ScreenChangeOptions.GenerateBasicBackOptions(),
                            SkipValidation = true,
                            Zone = new Rectangle(610, 854, 342, 127)
                        },
                        new SectionSkipScreenData
                        {
                            PageChangeOptions = ScreenChangeOptions.GenerateBasicNextOptions(),
                            SkipValidation = true,
                            Zone = new Rectangle(968, 853, 342, 127)
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
                            PreviewQuestionIdentifier = new QuestionIdentifier(9),
                            PreviewZone = new Rectangle(606, 81, 707, 707)
                        }
                    }
                }
            };
            screensController.AddScreen(previewScreen);


            // Share
            var shareScreen = new ShareScreen(baseDir + "bkg_07.jpg", sc, null, null, null,
                new Rectangle(964, 859, 341, 128),
                null,
                null,
                null,
                null,
                new Rectangle(605, 859, 341, 128),
                new Rectangle(590, 246, 735, 582),
                null,
                baseDir + "bkg_07.jpg"
            )
            {
                PrintoutCounter = new AdvancedScreenPrintConfiguration()
                {
                  
                    MaxPrintOuts = 2
                }

            };
            screensController.AddScreen(shareScreen);



            // 7 — THANK YOU
            var thanks = new GenericScreen(baseDir + "bkg_12.jpg", baseDir + "bkg_12.jpg", sc)
            {
                DefinitionNavigation = new ScreenDefinitionNavigation
                {
                    ZoneList = new List<SectionSkipScreenData>
                    {
                        new SectionSkipScreenData
                        {
                            PageChangeOptions = ScreenChangeOptions.GenerateBasicNextOptions(),
                            Zone = new Rectangle(0, 0, 1920, 1080)
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
        }
    }
}
