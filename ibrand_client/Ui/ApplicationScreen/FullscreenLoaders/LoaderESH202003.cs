using System.Collections.Generic;
using System.Drawing;
using System.IO;
using JetBrains.Annotations;
using Newtonsoft.Json;
using NomadLogic.Common.Questionnaire;
using NomadLogic.Common.Respondent;
using NomadLogic.Common.Util;
using NomadLogic.Common.Util.Json;
using NomadLogic.iBrandDesktop.Ui.ApplicationScreen.Generic;
using NomadLogic.iBrandDesktop.Ui.ApplicationScreen.Generic.Helpers;
using NomadLogic.iBrandDesktop.Ui.ApplicationScreen.Generic.Helpers.Gallery;
using NomadLogic.iBrandDesktop.Ui.ApplicationScreen.Generic.Helpers.ScreenDefinitions;
using NomadLogic.Multimedia;

// ReSharper disable ConditionIsAlwaysTrueOrFalse

namespace NomadLogic.iBrandDesktop.Ui.ApplicationScreen.FullscreenLoaders
{
    [PublicAPI]
    public class LoaderEsh202003: IFullscreenLoader
    {
        public void CreateScreens(TouchScreenController screensController, ScreenConfig sc)
        {
            var uiDir = DirectoryUtil.GetLocalIbrandTerminalDataDir(sc.ProjectId, sc.QuestionnaireId, sc.TerminalId,
                DirectoryUtil.ProjectDataDirectoriesEnum.photobooth_ui, DirectoryUtil.ProjectDataDirectoriesModEnum.none);
            var baseDir = DirectoryUtil.GetLocalIbrandTerminalDir(sc.ProjectId, sc.QuestionnaireId, sc.TerminalId);

            var debugFile = new FileInfo(baseDir + "debug.json");
            var debugConfig = new Esh202003DebugConfig();
            if (debugFile.Exists)
            {
                debugConfig = JsonUtil.ReadJsonFile<Esh202003DebugConfig>(debugFile);
            }

            // 1- Home
            var homeScreen = new HomeScreen(uiDir + "1.tap to start.jpg", sc);
            screensController.AddScreen(homeScreen);

            var baseCancelRectangle = new Rectangle(1269, 47, 362, 117);
            
            #region 4- Take pictures

            if (!debugConfig.DebugEnabled)
            {
                var pictScreen = new TakeMultiplePicturesManualFlurryScreen(sc, uiDir + "2.take picture.jpg", null,
                    null, null,
                new Rectangle(142, 425, 343, 104), new Rectangle(1195, 425, 343, 104), null,
                null, new Rectangle(1269, 47, 362, 117), new Rectangle(557,164,568,610))
                {
                    AutomaticTransition = true,
                    fontDefinition = new FontDefinitionInfo
                    {
                        Color = Color.White,
                        Size = 290,
                        FontFile = baseDir + "Oswald-Bold.otf",
                        FamilyName = "Oswald",
                        GraphicsUnit = GraphicsUnit.Pixel

                    },
                    FlurryCameraConfigFileName = baseDir+"shootingConfig.json"
                };
                screensController.AddScreen(pictScreen);
            }

            #endregion

            var defNavig = new ScreenDefinitionNavigation
            {
                ZoneList = new List<SectionSkipScreenData>
                {
                    new SectionSkipScreenData
                    {
                        Zone = baseCancelRectangle,
                        SkipValidation = true,
                        PageChangeOptions = ScreenChangeOptions.GenerateCancelOptions()
                    },
                    new SectionSkipScreenData
                    {
                        Zone = new Rectangle(546, 804, 594, 201),
                        SkipValidation = true,
                        PageChangeOptions = ScreenChangeOptions.GenerateBasicBackOptions()
                    }
                }
            };

            var loadSeq = new AnimatedImageSequence
            {
                Loop = true,
                DrawLocation = new Point(50, 927),
                ImageGenerators = new List<IAnimatedImageSequenceGenerator>
                {
                    new AnimatedImageSequenceGeneratorFromText
                    {
                        ProceduralImageTextInfo = new ProceduralImageTextInfo
                        {
                            Color = Color.White,
                            FontSize = 80,
                            BitmapSize = new Size(650, 100),
                            FontFamilly = "Arial"
                        },
                        Texts = new List<string>
                        {
                            "Loading.",
                            "Loading..",
                            "Loading...",
                            "Loading...."
                        }
                    }
                }
            };

            var errorSeq = new AnimatedImageSequence
            {
                Loop = false,
                DrawLocation = new Point(1030, 922),
                ImageGenerators = new List<IAnimatedImageSequenceGenerator>
                {
                    new AnimatedImageSequenceGeneratorFromText
                    {
                        ProceduralImageTextInfo = new ProceduralImageTextInfo
                        {
                            Color = Color.White,
                            FontSize = 30,
                            BitmapSize = new Size(650, 100),
                            FontFamilly = "Arial"
                        },
                        Texts = new List<string>
                        {
                            "Error has occured. Retake images."
                        }
                    }
                }
            };

            var saveFormatter = new SaveGallerySelectionAsIndex
            {
                IndexOffset = 1,
                QuestionNo = debugConfig.IndexQuestionNo,
            };

            IGallerySource<ComparableFileInfo> galSource;
            if (debugConfig.DebugEnabled)
            {
                galSource = new GallerySourceFixedDirectory
                {
                    WorkDirectory = new DirectoryInfo(debugConfig.GalleryDir)
                };
            }
            else
            {
                galSource = new GallerySourceCurrentAnswerAsDirectory
                {
                    SaveFormatter = new SaveGallerySelectionAsIndex
                    {
                        QuestionNo = 2,
                        IndexOffset = 1
                    }
                };
            }

            galSource.SaveFormatter = saveFormatter;

            #region 3 - Preview
            var gallery = new GenericScreen(uiDir + "3. Preview.jpg", null, sc)
            {
                DefinitionNavigation = defNavig,
                DefinitionGallery = new ScreenDefinitionGalleryFromFiles
                {
                    Mandatory = true,
                    LoadingImageSequence = loadSeq,
                    ErrorImageSequence = errorSeq,
                    GalleryZone = new Rectangle(50, 174, 1584, 726),
                    PresentationSize = new Size(5,1),
                    SelectionPageChangeOptions = ScreenChangeOptions.GenerateBasicNextOptions(),
                    PaginationUi = null,
                    PaddingRatio = 0.05,
                    GallerySource = galSource
                }
            };

            if (debugConfig.DebugEnabled)
            {
                gallery.ScreenDefinitionDebug = new ScreenDefinitionDebug
                {
                    DefaultAnswers = new List<Answer>
                    {
                        new Answer {QuestionNo = debugConfig.BundleQuestionNo, Response = debugConfig.GalleryDir}
                    }
                };
                if (debugConfig.GalleryUseThumbnail && !string.IsNullOrWhiteSpace(debugConfig.GalleryThumbnailDirectory))
                {
                    var dirInfo = new DirectoryInfo(debugConfig.GalleryThumbnailDirectory);
                    if (!dirInfo.Exists)
                    {
                        dirInfo = new DirectoryInfo(Path.Combine(baseDir, debugConfig.GalleryThumbnailDirectory));
                        dirInfo.Refresh();
                        if (!dirInfo.Exists)
                        {
                            throw new System.Exception("Could not find thumbnail directory: " + debugConfig.GalleryThumbnailDirectory);
                        }
                    }

                    gallery.DefinitionGallery.UseThumbnails = true;
                    gallery.DefinitionGallery.ThumbnailDirectory = dirInfo;
                }

                // Pagination experimentale

                if (!debugConfig.HidePagination)
                {
                    var fontDef = new FontDefinitionInfo
                    {
                        Size = 70,
                        Color = Color.Crimson,
                        Style = "Bold",
                        FamilyName = FontFamily.GenericMonospace.Name,
                        GraphicsUnit = GraphicsUnit.Pixel
                    };

                    var pagi = new PaginationUiElements<ComparableFileInfo>
                    {
                        DefaultFontDefinition = fontDef,
                        CondensedNumberOfPagesOptions = 3,
                        CondensedPaginationControls = new Rectangle(407, 26, 854, 126),
                        CondensedUseFullJumps = true,
                        NextZone = new ScreenClickZoneText
                        {
                            Zone = new Rectangle(1285, 852, 149, 135),
                            FontDefinition = fontDef,
                            TextDictionary = new Dictionary<string, string>
                            {
                                {"en", PaginationUiElements<ComparableFileInfo>.NEXT_CHARACTER }
                            }
                        },
                        PreviousZone = new ScreenClickZoneText
                        {
                            Zone = new Rectangle(290, 852, 149, 135),
                            FontDefinition = fontDef,
                            TextDictionary = new Dictionary<string, string>
                            {
                                {"en", PaginationUiElements<ComparableFileInfo>.PREVIOUS_CHARACTER }
                            }
                        }
                    };
                    
                    gallery.DefinitionGallery.PaginationUi = pagi;
                }
            }

            screensController.AddScreen(gallery);
            #endregion
            
            #region Courriel

            if (!debugConfig.DebugEnabled)
            {
                var mailScreen = new GenericScreen(uiDir + "4.email.jpg", null, sc)
                {
                    DefinitionEshotsEventDayTransfer = new ScreenDefinitionEshotsEventDayTransfer()
                    {
                        EventDayIdQuestionNo = 3,
                        ShowErrorDialog = true,
                        AllowRespondentToContinue = true
                    },
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
                                Mandatory = true,
                                AllowEmpty = false,
                                Validation = new TextValidationBasic
                                {
                                    Option = TextValidationBasic.TextValidationBasicEnum.Email
                                },
                                FieldName = "email",
                                Modifications = TextModificationsEnum.Trim,
                                Position = new Rectangle(486, 257, 1023, 101),
                                UserMessages = new Dictionary<UserErrorEnum, IDictionary<string, string>>
                                {
                                    {
                                        UserErrorEnum.Validation, new Dictionary<string, string>
                                        {
                                            {"fr", "Le courriel est invalide."},
                                            {"en", "The email address is invalid."}
                                        }
                                    }
                                }
                            }
                        }
                    },
                    DefinitionKeyboard = new ScreenDefinitionKeyboard
                    {
                        Position = new Rectangle(-1, 378, 1575, 389),
                        ColorContour = Color.Transparent,
                        KeyboardType = KeyboardTypeEnum.Email,
                        Padding = 5,
                        FontDefinition = new FontDefinitionInfo
                        {
                            Size = 25,
                            Color = Color.Black,
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
                                PageChangeOptions = ScreenChangeOptions.GenerateBasicNextOptions(),
                                Zone = new Rectangle(668, 885, 348, 111)
                            },
                            new SectionSkipScreenData
                            {
                                PageChangeOptions = ScreenChangeOptions.GenerateBasicBackOptions(),
                                Zone = new Rectangle(1263, 54, 371, 102)
                            }
                        }

                    }
                };
                screensController.AddScreen(mailScreen);
            }

            #endregion

            var thanks = new ThanksEndScreen(uiDir + "5.thank you.jpg", sc)
            {
                InactivityTimerInterval = 3500,
                ForcePrintOnActivation = true
            };
            screensController.AddScreen(thanks);

        }
    }
    internal class Esh202003DebugConfig
    {
        [JsonRequired]
        public bool DebugEnabled { get; set; } = false;
        public string GalleryDir { get; set; }
        public int BundleQuestionNo { get; set; } = 2;
        public int IndexQuestionNo { get; set; } = 1;
        public bool GalleryUseThumbnail { get; set; } = false;
        public string GalleryThumbnailDirectory { get; set; }
        public bool HidePagination { get; set; }
    }
}