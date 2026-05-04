using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using JetBrains.Annotations;
using Newtonsoft.Json;
using NLog;
using NomadLogic.Common.Questionnaire;
using NomadLogic.Common.Respondent;
using NomadLogic.Common.Ui;
using NomadLogic.Common.Util;
using NomadLogic.Common.Util.DateAndTime;
using NomadLogic.Common.Util.Json;
using NomadLogic.iBrandDesktop.Services.RespondentEventDataDownload.DownloadWorkers;
using NomadLogic.iBrandDesktop.Ui.ApplicationScreen.custom.AqualandMoravia;
using NomadLogic.iBrandDesktop.Ui.ApplicationScreen.Generic;
using NomadLogic.iBrandDesktop.Ui.ApplicationScreen.Generic.Helpers;
using NomadLogic.iBrandDesktop.Ui.ApplicationScreen.Generic.Helpers.ScreenDefinitions;
using NomadLogic.iBrandDesktop.Ui.ApplicationScreen.Generic.Helpers.ScreenDefinitions.MultimediaEventListing;
using NomadLogic.Multimedia;
#pragma warning disable 612

namespace NomadLogic.iBrandDesktop.Ui.ApplicationScreen.FullscreenLoaders
{
    // ReSharper disable once InconsistentNaming
    [PublicAPI]
    public class Loader_AQA_2021_01_B : IFullscreenLoader
    {
        private static readonly Logger log = LogManager.GetCurrentClassLogger();

        public void CreateScreens(TouchScreenController screensController, ScreenConfig sc)
        {
            var baseDir = DirectoryUtil.GetLocalIbrandTerminalDir(sc.ProjectId, sc.QuestionnaireId, sc.TerminalId);

            #region config File

            // Chargement de la configuration locale
            var configFile = new FileInfo(Path.Combine(baseDir, "aqualand_config.json"));
            AqualandPresentationConfiguration aquaConfig;
            if (!configFile.Exists)
            {
                var overlayImagesDefinitions = new Dictionary<ThumbnailOverlaySelectorState, ThumbnailOverlaySelectorImageInfo>
                {
                    {
                        ThumbnailOverlaySelectorState.Selected, new ThumbnailOverlaySelectorImageInfo
                        {
                            ImageFileName = "Overlay_Checkbox.png"
                        }
                    },
                    {
                        ThumbnailOverlaySelectorState.CannotBeSelected, new ThumbnailOverlaySelectorImageInfo
                        {
                            ImageFileName = "Overlay_GRIS.png"
                        }
                    },
                    {
                        ThumbnailOverlaySelectorState.NoAvailiable, new ThumbnailOverlaySelectorImageInfo
                        {
                            ImageFileName = "Overlay_GRIS.png"
                        }
                    }
                };

                var overlayImagesDefinitionsBig = new Dictionary<ThumbnailOverlaySelectorState, ThumbnailOverlaySelectorImageInfo>
                {
                    {
                        ThumbnailOverlaySelectorState.Selected, new ThumbnailOverlaySelectorImageInfo
                        {
                            ImageFileName = "Overlay_Checkbox_big.png"
                        }
                    },
                    {
                        ThumbnailOverlaySelectorState.CannotBeSelected, new ThumbnailOverlaySelectorImageInfo
                        {
                            ImageFileName = "Overlay_GRIS_big.png"
                        }
                    },
                    {
                        ThumbnailOverlaySelectorState.NoAvailiable, new ThumbnailOverlaySelectorImageInfo
                        {
                            ImageFileName = "Overlay_GRIS_big.png"
                        }
                    }
                };

                // Configurations de base
                aquaConfig = new AqualandPresentationConfiguration
                {
                    DownloadManagerRequestMainConfiguration = new DownloadWorkerAqualand
                    {
                        SourceTerminalId = -1,
                        SourceQuestionnaireId = -1,
                        ProcessedTerminalId = -1,
                        ProcessedQuestionnaireId = -1,
                        DateMinDefinition = new DateTimeCalculatorFixed
                        {
                            FixedDate = DateTime.Now.AddMonths(-4)
                        },
                        DateMaxDefinition = new DateTimeCalculatorFromNow
                        {
                            StartFromDateOnly = true,
                            ReduceByTimespan = TimeSpan.FromDays(-1)
                        },
                        SourceDataKeyFilters = new List<string>
                        {
                            "tmp1", "tmp2", "tmp3"
                        },
                        ProcessedDataKeyFilters = new List<string>
                        {
                            "tmp1", "tmp2", "tmp3"
                        }
                    },
                    ScanRequireCorrectRespondentId = false,
                    GalleryNumberSelectedTextLocation = new Rectangle(35, 980, 795, 85),
                    GalleryNumberSelectedTextGenerator = new ImageGeneratorFromText
                    {
                        ProceduralImageTextInfo = new ProceduralImageTextInfo
                        {
                            BitmapSize = new Size(795, 85),
                            Color = Color.CadetBlue,
                            BackgroundColor = Color.White,
                            FontGraphicUnit = GraphicsUnit.Pixel,
                            FontSize = 50,
                            FontStyle = FontStyle.Bold,
                            FontFamilly = "Helvetica"
                        },
                        Texts = new Dictionary<string, string>
                        {
                            {"en", "There is {0} images selected."}
                            ,{"fr", "Il y a {0} images sélectionnées."}
                        }
                    },
                    PresentationConfig = new MultimediaEventItemPresentation
                    {
                        LayoutRectangle = new Rectangle(35, 120, 1850, 850),
                        CustomTimeFormat = "HH:mm",
                        ThmbSize = new Size(320, 180),
                        FullScreenViewPresentation = new MultimediaEventItemPresentationDetailSingle
                        {
                            PresentationZone = new Rectangle(530, 210, 800, 450),
                            BackgroundImageFileName = "screen_base.png",
                            ThumbnailDefaultFileName = "photobooth_ui\\not_availiable.png",
                            ThumbnailOverlayImagesDefinitions = overlayImagesDefinitionsBig,
                            QuitButtonImageFileName = "quit_button.png",
                            QuitButtonPosition = new Rectangle(578, 787, 74, 77),
                            QuitButtonBackColor = Color.White
                        }
                        ,QuickSelectionConfig = new MultimediaEventItemPresentationQuickSelection
                        {
                            Size = new Size(74, 77),
                            SelectionActivatedImageFileName = "checkbox.png",
                            SelectionUnselectedImageFileName = "checkbox_GRIS.png",
                        },
                        AssetsDirectory = baseDir,
                        ThumbnailDefaultFileName = "photobooth_ui\\not_availiable.png",
                        DateFontDefinition = new FontDefinitionInfo
                        {
                            Color = Color.Blue,
                            Size = 50,
                            FamilyName = "Helvetica",
                            Style = "Bold",
                            GraphicsUnit = GraphicsUnit.Pixel
                        },
                        ThumbnailOverlayImagesDefinitions = overlayImagesDefinitions,
                        FullScreenVimeoPresentation = new MultimediaEventVimeoPresentation
                        {
                            QuitButtonBackColor = Color.White,
                            QuitButtonImageFileName = "back_button.png",
                            QuitButtonPosition = new Rectangle(710, 825, 500, 100),
                            BackgroundImageFileName = "screen_base.png",
                            WebControlZone = new Rectangle(50, 130, 1820, 675),
                            SelectionStatusButtonZone = new Rectangle(70, 825, 100, 100),
                            SelectionActivatedImageFileName = "checkbox.png",
                            SelectionUnselectedImageFileName = "checkbox_GRIS.png"
                        }
                    },
                    InfoEmailMessages = new Dictionary<UserErrorEnum, IDictionary<string, string>>
                    {
                        {UserErrorEnum.Mandatory, new Dictionary<string, string>
                        {
                            {"fr", "Le courriel est obligatoire."},
                            {"en", "The e-mail is mandatory."}
                        }},
                        {UserErrorEnum.Validation, new Dictionary<string, string>
                        {
                            {"fr", "Le courriel entré est invalide."},
                            {"en", "The e-mail is currently invalid."}
                        }},
                        {UserErrorEnum.System, new Dictionary<string, string>
                        {
                            {"fr", "Un problème empêche la saisie du courriel."},
                            {"en", "A problem currently prevents e-mail validation."}
                        }}
                    },
                    InfoPhoneMessages = new Dictionary<UserErrorEnum, IDictionary<string, string>>
                    {
                        {UserErrorEnum.Mandatory, new Dictionary<string, string>
                        {
                            {"fr", "Le numéro de téléphone est obligatoire."},
                            {"en", "The phone number is mandatory."}
                        }},
                        {UserErrorEnum.Validation, new Dictionary<string, string>
                        {
                            {"fr", "Le numéro de téléphone entré est invalide."},
                            {"en", "The phone number is currently invalid."}
                        }},
                        {UserErrorEnum.System, new Dictionary<string, string>
                        {
                            {"fr", "Un problème empêche la saisie du numéro de téléphone."},
                            {"en", "A problem currently prevents phone number validation."}
                        }}
                    },
                    InfoLabelDefinitions = new List<ScreenDefinitionQuestionLabel>
                    {
                        new ScreenDefinitionQuestionLabel
                        {
                            TokenString = new Dictionary<string, string>
                            {
                                {"en", "You have selected {q5} items, for a total of:"}, 
                                {"fr", "Vous avez choisi {q5} fichiers, pour un total de:"}
                            },
                            Position = new RectangleF(171, 11, 938, 54),
                            FontDefinition = new FontDefinitionInfo
                            {
                                Color = Color.DarkCyan,
                                FamilyName = "Helvetica",
                                Size = 40
                            }
                        },
                        new ScreenDefinitionQuestionLabel
                        {
                            TokenString = new Dictionary<string, string>
                            {
                                {"en", "{q6} €"}, 
                                {"fr", "{q6} €"}
                            },
                            Position = new RectangleF(563, 172, 128, 55),
                            FontDefinition = new FontDefinitionInfo
                            {
                                Color = Color.DarkCyan,
                                FamilyName = "Helvetica",
                                Size = 60,
                                Style = "Bold"
                            }
                        }
                    },
                    LargeTextRulesAndReg = new Dictionary<string, string>
                    {
                        {"en", "rules_en.txt"}, {"fr", "rules_fr.txt"}
                    },
                    GalleryStateMessageLocation = new Rectangle(392,20,1275,85),
                    GalleryStateMessages = new Dictionary<MultimediaEventListingStateEnum, IDictionary<string, string>>
                    {
                        {MultimediaEventListingStateEnum.Ready, new Dictionary<string, string> {{"en","Gallery is loaded."}, {"fr","La gallerie est chargée."}}},
                        {MultimediaEventListingStateEnum.WaitingForData, new Dictionary<string, string> {{"en","Gathering data..."}, {"fr","Recherche en cours..."}}},
                        {MultimediaEventListingStateEnum.NoData, new Dictionary<string, string> {{"en","No data is found. Please try again later."}, {"fr","Aucune donnée trouvée. Essayez plus tard."}}},
                        {MultimediaEventListingStateEnum.Error, new Dictionary<string, string> {{"en","An error has occured. Please check with a clerk."}, {"fr","Une erreur s'est produite. Vérifiez avec un employé."}}}
                    },
                    GalleryStateMessageImageDefinition = new ProceduralImageTextInfo
                    {
                        BitmapSize = new Size(1275,85),
                        Center = true,
                        Color = Color.BlueViolet,
                        FontGraphicUnit = GraphicsUnit.Pixel,
                        BackgroundColor = Color.White,
                        FontStyle = FontStyle.Italic,
                        FontSize = 60,
                        FontFamilly = "Times"
                    },
                    PlaceHolderAssetFileName = "photobooth_ui\\not_availiable.png",
                    BackButtonZone = new Rectangle(1400, 950, 500, 100),
                    BackButtonAlwaysShow = false,
                    BackButtonImageFileName = "back_button.png",
                    NextButtonZone = new Rectangle(900, 950, 500, 100),
                    NextButtonAlwaysShow = false,
                    NextButtonImageFileName = "next_button.png",
                    NextButtonPageSkip = 1,
                    DynamicHeadersCommonDataKey = "gallery"
                };
                JsonUtil.WriteJsonFile(configFile, aquaConfig, new JsonSerializerSettings
                {
                    NullValueHandling = NullValueHandling.Include
                });
                log.Warn("No Aqualand configuration file found. Baseline one generated and written to disk. Continuing..");
            }
            else
            {
                aquaConfig = JsonUtil.CreateInstanceFromJsonConfigFile<AqualandPresentationConfiguration>(configFile);
            }

            // Assignation des infos locales au projet.
            aquaConfig.PresentationConfig.AssetsDirectory = baseDir;

            #endregion

            var cancelZoneOptions = new SectionSkipScreenData
            {
                PageChangeOptions = ScreenChangeOptions.GenerateCancelOptions(),
                SkipValidation = true,
                Zone = new Rectangle(1870, 0, 50, 50)
            };

            #region 1# Scan Initial

            var barscanScreen = new RfidAqualandScreen(baseDir + "screen_scan.jpg", baseDir + "screen_scan.jpg", sc,
                new Rectangle(206, 384, 500, 40))
            {
                ScreenTag = "Scan (1)",
                RequireCorrectCustomerId = aquaConfig.ScanRequireCorrectRespondentId,
                Worker = aquaConfig.DownloadManagerRequestMainConfiguration
                // TODO Ajouter config pour RFID
            };
            screensController.AddScreen(barscanScreen);

            #endregion

            #region 2# Gallery

            var galleryScreen = new CaptureEventListingScreen(baseDir + "screen_base.png", baseDir + "screen_base.png", sc)
            {
                ScreenTag = "Gallerie (2)",
                CaptureEventList = new ScreenDefinitionMultimediaEventListing
                {
                    CustomErrorPlaceHolderFileName = Path.Combine(DirectoryUtil.GetLocalNomadLogicAssetsDir(), aquaConfig.PlaceHolderAssetFileName),
                    PresentationData = aquaConfig.PresentationConfig,
                    CustomWorkFileDirectory = DirectoryUtil.GetLocalIbrandTerminalCustomDir(sc.ProjectId, sc.QuestionnaireId, sc.TerminalId, "gallery_thumbnail"),
                    NumberSelectedTextLocation = aquaConfig.GalleryNumberSelectedTextLocation,
                    NumberSelectedTextImageGenerator = aquaConfig.GalleryNumberSelectedTextGenerator,
                    StateMessageLocation = aquaConfig.GalleryStateMessageLocation,
                    StateMessageCustomTexts = aquaConfig.GalleryStateMessages,
                    StateMessageImageDefinition = aquaConfig.GalleryStateMessageImageDefinition,
                    BackButtonZone = aquaConfig.BackButtonZone,
                    BackButtonImageFileName = aquaConfig.BackButtonImageFileName,
                    BackButtonAlwaysShow = aquaConfig.BackButtonAlwaysShow,
                    NextButtonZone = aquaConfig.NextButtonZone,
                    NextButtonImageFileName = aquaConfig.NextButtonImageFileName,
                    NextButtonAlwaysShow = aquaConfig.NextButtonAlwaysShow,
                    NextButtonPageSkip = aquaConfig.NextButtonPageSkip,
                    SaveDynamicHeadersCommonDataKey = aquaConfig.DynamicHeadersCommonDataKey
                },
                DefinitionNavigation = new ScreenDefinitionNavigation
                {
                    ZoneList = new List<SectionSkipScreenData>
                    {
                        cancelZoneOptions,
                        new SectionSkipScreenData
                        {
                            PageChangeOptions = ScreenChangeOptions.GenerateBasicNextOptions(),
                            SkipValidation = false,
                            Zone = new Rectangle(718,507,296,27) // TODO
                        }
                    }
                }
            };
            screensController.AddScreen(galleryScreen);

            #endregion

            #region 3# Confirm information

            var confirmScreen = new GenericScreen(baseDir + "screen_base.png", baseDir + "screen_base.png", sc)
            {
                ScreenTag = "Confirm info (3)",
                DefinitionTextbox = new ScreenDefinitionTextBoxes
                {
                    TextboxDefinitions = new List<TextboxDefinition>
                    {
                        new TextboxDefinition
                        {
                            Position = new Rectangle(389,258,508,43),
                            AllowEmpty = false,
                            DataModels = new List<DataModelBase>{
                                new DataModelQuestionnaire
                            {
                                Identifier = new QuestionIdentifier(2)
                            }},
                            FieldName = "e-mail",
                            Mandatory = true,
                            Validation = new TextValidationBasic
                            {
                                Option = TextValidationBasic.TextValidationBasicEnum.Email
                            },
                            UserMessages = aquaConfig.InfoEmailMessages
                        },
                        new TextboxDefinition
                        {
                            Position = new Rectangle(389,258,508,43),
                            AllowEmpty = false,
                            DataModels = new List<DataModelBase>{
                                new DataModelQuestionnaire
                            {
                                Identifier = new QuestionIdentifier(3)
                            }},
                            FieldName = "phone",
                            Mandatory = true,
                            UserMessages = aquaConfig.InfoPhoneMessages,
                            Validation = new TextValidationBasic
                            {
                                Option = TextValidationBasic.TextValidationBasicEnum.Sms
                            }
                        }
                    },
                },
                DefinitionCheckBoxes = new ScreenDefinitionCheckBoxes
                {
                    CheckboxDefinitions = new List<ScreenCheckOption>
                    {
                        new ScreenCheckOption
                        {
                            DataModels = new List<DataModelBase>{
                                new DataModelQuestionnaire
                            {
                                Identifier = new QuestionIdentifier(4)
                            }},
                            Position = new Rectangle(691, 413, 96, 24),
                            FieldName = "send e-mail",
                            CheckLocation = new Point(760,413)
                        },
                        new ScreenCheckOption
                        {
                            DataModels = new List<DataModelBase>{
                                new DataModelQuestionnaire
                            {
                                Identifier = new QuestionIdentifier(5)
                            }},
                            Position = new Rectangle(822, 413, 75, 24),
                            FieldName = "send sms",
                            CheckLocation = new Point(871,413)
                        }
                    }
                },
                DefinitionNavigation = new ScreenDefinitionNavigation
                {
                    ZoneList = new List<SectionSkipScreenData>
                    {
                        cancelZoneOptions,
                        new SectionSkipScreenData
                        {
                            PageChangeOptions = ScreenChangeOptions.GenerateBasicNextOptions(2),
                            SkipValidation = false,
                            Zone = new Rectangle(389,451,227,49)
                        },
                        new SectionSkipScreenData
                        {
                            PageChangeOptions = ScreenChangeOptions.GenerateBasicBackOptions(),
                            SkipValidation = false,
                            Zone = new Rectangle(671,451,227,49)
                        },
                        // Rules And Regs
                        new SectionSkipScreenData
                        {
                            PageChangeOptions = ScreenChangeOptions.GenerateBasicNextOptions(),
                            SkipValidation = false,
                            Zone = new Rectangle(718,507,296,27)
                        }
                    }
                },
                DefinitionKeyboard = new ScreenDefinitionKeyboard
                {
                    Position = new Rectangle(1717,542,938,240),
                    Padding = 0,
                    KeyboardType = KeyboardTypeEnum.Multilingual
                },
                DefinitionQuestionLabels = new ScreenDefinitionQuestionLabels
                {
                    LabelDefinitions = aquaConfig.InfoLabelDefinitions
                }
            };
            screensController.AddScreen(confirmScreen);

            #endregion

            #region #4 Rules And Regs

            var rulesScreen = new GenericScreen(baseDir + "screen_regs.jpg", baseDir + "screen_regs.jpg", sc)
            {
                ScreenTag = "Rules and regs (4)",
                DefinitionLargeText = new ScreenDefinitionLargeText
                {
                    TextZone = new Rectangle(43, 112, 1198, 328),
                    LargeTextFileDictionary = aquaConfig.LargeTextRulesAndReg
                },
                DefinitionNavigation = new ScreenDefinitionNavigation
                {
                    ZoneList = new List<SectionSkipScreenData>
                    {
                        cancelZoneOptions,
                        new SectionSkipScreenData
                        {
                            PageChangeOptions = ScreenChangeOptions.GenerateBasicBackOptions(),
                            SkipValidation = false,
                            Zone = new Rectangle(671,451,227,49)
                        }
                    }
                }
            };
            screensController.AddScreen(rulesScreen);

            #endregion

            #region #5 Confirm Scan

            var barscanConfirmScreen = new RfidConfirmAqualandScreen(baseDir + "screen_scan.jpg", baseDir + "screen_scan.jpg", sc,
                new Rectangle(356, 269, 584, 40))
            {
                ScreenTag = "Confirm scan (5)",
                // TODO Ajouter config pour RFID
            };
            screensController.AddScreen(barscanConfirmScreen);

            #endregion

            #region #6 Thanx

            var thxScreen = new GenericScreen(baseDir + "screen_thx.jpg", baseDir + "screen_thx.jpg", sc)
            {
                ScreenTag = "Thanks (6)",
                DefinitionTimer = new ScreenDefinitionTimerOptions
                {
                    InactivityTimerInterval = 15000,
                    TimerAction = TimerActionEnum.End
                },
                DefinitionNavigation = new ScreenDefinitionNavigation
                {
                    ZoneList = new List<SectionSkipScreenData>
                    {
                        new SectionSkipScreenData
                        {
                            PageChangeOptions = ScreenChangeOptions.GenerateBasicNextOptions(),
                            Zone = new Rectangle(0,0,1920, 1080)
                        }
                    }
                }
            };
            screensController.AddScreen(thxScreen);

            #endregion

        }
    }

    public class AqualandPresentationConfiguration : IJsonConfig
    {
        public string DefaultConfigFileName => "aqualand_config.json";
        public bool ScanRequireCorrectRespondentId { get; set; } = true;

        [JsonRequired]
        public MultimediaEventItemPresentation PresentationConfig { get; set; }

        public Rectangle? GalleryNumberSelectedTextLocation { get; set; }
        public ImageGeneratorFromText GalleryNumberSelectedTextGenerator { get; set; }
        public DownloadWorkerAqualand DownloadManagerRequestMainConfiguration { get; set; }
        public IDictionary<UserErrorEnum, IDictionary<string, string>> InfoEmailMessages { get; set; }
        public Dictionary<UserErrorEnum, IDictionary<string, string>> InfoPhoneMessages { get; set; }
        public IList<ScreenDefinitionQuestionLabel> InfoLabelDefinitions { get; set; }
        public Dictionary<string, string> LargeTextRulesAndReg { get; set; }
        public Rectangle? GalleryStateMessageLocation { get; set; }
        public IDictionary<MultimediaEventListingStateEnum, IDictionary<string, string>> GalleryStateMessages { get; set; }
        public ProceduralImageTextInfo GalleryStateMessageImageDefinition { get; set; }
        public string PlaceHolderAssetFileName { get; set; }
        public Rectangle? BackButtonZone { get; set; }
        public string BackButtonImageFileName { get; set; }
        public bool BackButtonAlwaysShow { get; set; }
        public Rectangle? NextButtonZone { get; set; }
        public bool NextButtonAlwaysShow { get; set; }
        public string NextButtonImageFileName { get; set; }
        public int NextButtonPageSkip { get; set; }
        public string DynamicHeadersCommonDataKey { get; set; }
    }
}