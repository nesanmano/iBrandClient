using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using JetBrains.Annotations;
using Newtonsoft.Json;
using NLog;
using NomadLogic.Common.Questionnaire;
using NomadLogic.Common.Respondent;
using NomadLogic.Common.Util;
using NomadLogic.Common.Util.DateAndTime;
using NomadLogic.Common.Util.Json;
using NomadLogic.iBrandDesktop.Services.RespondentEventDataDownload.DownloadWorkers;
using NomadLogic.iBrandDesktop.Ui.ApplicationScreen.Generic;
using NomadLogic.iBrandDesktop.Ui.ApplicationScreen.Generic.Helpers;
using NomadLogic.iBrandDesktop.Ui.ApplicationScreen.Generic.Helpers.ScreenDefinitions;
using NomadLogic.iBrandDesktop.Ui.ApplicationScreen.Generic.Helpers.ScreenDefinitions.MultimediaEventListing;
using NomadLogic.Multimedia;

// ReSharper disable InconsistentNaming

namespace NomadLogic.iBrandDesktop.Ui.ApplicationScreen.FullscreenLoaders
{
    /// <summary>
    /// Chargeur d'écran pour le terminal 2 (Kiosque impression)
    /// Projet NL-4 pour Playland Vancouver 2022
    /// </summary>
    [PublicAPI]
    public class Loader_NL4_T3 : IFullscreenLoader
    {
        private static readonly Logger log = LogManager.GetCurrentClassLogger();
        
#pragma warning disable CS0612
        public void CreateScreens(TouchScreenController screensController, ScreenConfig sc)
#pragma warning restore CS0612
        {
            var baseDir = DirectoryUtil.GetLocalIbrandTerminalDir(sc.ProjectId, sc.QuestionnaireId, sc.TerminalId);

            var cancelZoneOptions = new SectionSkipScreenData
            {
                PageChangeOptions = ScreenChangeOptions.GenerateCancelOptions(),
                SkipValidation = true,
                Zone = new Rectangle(1920-60,0,60,60)
            };
            
            var copyConfFile = new FileInfo(baseDir + "copy_print_path.json");
            DirectoryInfo copyPath;
            #region Copy config read.

            if (!File.Exists(copyConfFile.FullName))
            {
                copyPath = new DirectoryInfo("C:\\print_copy");
                
                JsonUtil.WriteJsonFile(copyConfFile, copyPath);
                copyPath.Refresh();
            }

            copyPath = JsonUtil.ReadJsonFile<DirectoryInfo>(copyConfFile);
            copyPath.Create();

            #endregion

            FileInfo debugfileCheck = null; //new FileInfo(baseDir + "get_request_debug_check.txt");
            FileInfo debugfileAssoc = null; //new FileInfo(baseDir + "get_request_debug_assoc.txt");
            FileInfo debugfilePrint1 = null; //new FileInfo(baseDir + "get_request_debug_print_6x8.txt");
            FileInfo debugfilePrint2 = null; //new FileInfo(baseDir + "get_request_debug_print_4x6.txt");

            #region Config File

            var configFile = new FileInfo(Path.Combine(baseDir, "scan_request_config.json"));
            GalleryScanRequestConfig scanConfig;
            if (!configFile.Exists)
            {
                scanConfig = new GalleryScanRequestConfig
                {
                    QuestionnaireId = 233,
                    DataFilters = new List<string> {"roaming", "ride"}
                };
                JsonUtil.WriteJsonFile(configFile, scanConfig, new JsonSerializerSettings
                {
                    NullValueHandling = NullValueHandling.Include
                });
                log.Warn("No scan screen configuration file found. Baseline one generated and written to disk. Continuing..");
            }
            else
            {
                scanConfig = JsonUtil.CreateInstanceFromJsonConfigFile<GalleryScanRequestConfig>(configFile);
            }

            // Classe de conversion de données
            MultimediaEventListRawConverter _dataConverter;
            if (scanConfig.UseTestConverter)
            {
                _dataConverter = new MultimediaEventListRawConverterMockSingleImage();
            }
            else
            {
                _dataConverter = new MultimediaEventListRawConverterBase();
            }

            #endregion

            #region Scan to start
            
            // Scan avec appel vers le serveur. Voir Aqualand. Mettre question 2 dans questionnaire si possible, télécharger les images.
            var barscanScreen = new GalleryScanRequestScreen(baseDir + "1-scan.jpg", baseDir + "1-scan.jpg", sc,
                new Rectangle(540, 545, 845, 52))
            {
                NextLocation = new Rectangle(540, 365, 845, 58),
                ScanTriggerOnReturn = true,
                ScreenTag = "Scan (1)",
                SaveValueDataModel = new DataModelQuestionnaire
                {
                    Identifier = new QuestionIdentifier(1)
                },
                RequireCorrectCustomerId = false,
                RfidReaderConfigFile = "rfid_config.json",
                Worker = new DownloadWorkerBase
                {
                    SourceQuestionnaireId = scanConfig.QuestionnaireId,
                    DateMinDefinition = new DateTimeCalculatorFixed
                    {
                        FixedDate = DateTime.Today
                    },
                    DateMaxDefinition = new DateTimeCalculatorFromNow
                    {
                        ReduceByTimespan = TimeSpan.FromDays(-1) // Demain
                    },
                    SourceDataKeyFilters = scanConfig.DataFilters,
                    SourceQuestionsFilter = new List<string> // On veut la réponse du UEI et du déblocage de la gallerie digitale.
                    {
                        "q1", "q2"
                    }
                }
            };
            screensController.AddScreen(barscanScreen);
            
            #endregion

            #region Gallery

            var backColor = Color.White;

            var galleryScreen = new CaptureEventListingScreen(baseDir + "2-gallery.jpg", baseDir + "2-gallery.jpg", sc)
            {
                ScreenTag = "Galerie (2)",
                CaptureEventList = new ScreenDefinitionMultimediaEventListing
                {
                    EventDataConverter = _dataConverter, // Possibilité d'utiliser un lot d'image de tests ici.
                    TriggerChangeOnMinimumSelectedItems = 1,
                    PresentationData = new MultimediaEventItemPresentation
                    {
                        MainLayoutBackgroundColor = backColor,
                        LayoutRectangle = new Rectangle(36,36,1540,1011),
                        CustomTimeFormat = "HH:mm",
                        ThmbSize = new Size(1455, 970),
                        AssetsDirectory = baseDir,
                        ThumbnailDefaultFileName = @"photobooth_ui\not_availiable.png",
                        DateFontDefinition = null, // Pas de date.
                        CenterRow = false
                    },
                    CustomWorkFileDirectory = DirectoryUtil.GetLocalIbrandTerminalCustomDir(sc.ProjectId, sc.QuestionnaireId, sc.TerminalId, "gallery_thumbnail"),
                    StateMessageLocation = new Rectangle(1575,901,308,50),
                    StateMessageImageDefinition = new ProceduralImageTextInfo
                    {
                        BackgroundColor = backColor,
                        BitmapSize = new Size(308, 50),
                        Color = Color.DarkRed,
                        FontGraphicUnit = GraphicsUnit.Pixel,
                        FontSize = 30,
                        FontFamilly = "Helvetica"
                    },
                    StateMessageCustomTexts = new Dictionary<MultimediaEventListingStateEnum, IDictionary<string, string>>
                    {
                        {MultimediaEventListingStateEnum.Ready, new Dictionary<string, string> {{"en", "Please pick an image."}}}
                        ,{MultimediaEventListingStateEnum.NoData, new Dictionary<string, string> {{"en", "No image found."}}}
                        ,{MultimediaEventListingStateEnum.WaitingForData, new Dictionary<string, string> {{"en", "Downloading. "}}}
                        ,{MultimediaEventListingStateEnum.Error, new Dictionary<string, string> {{"en", "Error detected."}}}
                    },
                    BackButtonZone = new Rectangle(1627,522,213,74),
                    BackButtonImageFileName = "back_button.png",
                    SaveDynamicHeadersCommonDataKey = "print",
                    SaveDynamicHeadersReplaceOriginalValueWithLocalFile = true,
                    ResetSelectionOnActivation = true
                },
                DefinitionNavigation = new ScreenDefinitionNavigation
                {
                    ZoneList = new List<SectionSkipScreenData>
                    {
                        cancelZoneOptions
                    }
                }
            };
            screensController.AddScreen(galleryScreen);
            
            #endregion
            
            #region Impression

            var printScreen = new GenericUpdateServerScreen(baseDir + "3-print-buy.jpg", baseDir + "3-print-buy.jpg", sc)
            {
                LoadAnswersFromServer = false,
                LoadDynamicHeadersFromServer = false,
                LoadKeepExistingAnswers = true,
                SendSkipAnswers = false,
                SendSkipDynamicHeaders = true,
                Tag = "Print-Buy (3)",
                DefinitionNavigation = new ScreenDefinitionNavigation
                {
                    ZoneList = new List<SectionSkipScreenData>
                    {
                        cancelZoneOptions,
                        new SectionSkipScreenData // Finish
                        {
                            Zone = new Rectangle(1108, 913, 469, 74),
                            PageChangeOptions = ScreenChangeOptions.GenerateBasicNextOptions()
                        }
                    }
                },
                DefinitionImageCopy = new ScreenDefinitionImageCopyList
                {
                    ImageCopyDefinitions = new List<ScreenDefinitionImageCopyBase>
                    {
                        // 6x8
                        new ScreenDefinitionImageCopyBase
                        {
                            UpdateToServer = true,
                            UpdateToServerDebugFile = debugfilePrint1,
                            ActivitySaveLogName = "Print digital package (6x8) buy",
                            ButtonZone = new Rectangle(1245, 311, 190, 190),
                            ButtonImageFileName = "printer.png",
                            ButtonBackgroundColor = backColor,
                            ArtificialDelayMili = 600,
                            DataModelSource = new DataModelDynamicHeader
                            {
                                DataKey = "print"
                            },
                            SaveFixedDataOnActivation = new List<(DataModelBase, object)>
                            {
                                (new DataModelQuestionnaire {Identifier = new QuestionIdentifier(2)}, true)
                            },
                            DesiredSize = new Size(2458, 1844),
                            CopyDirectoryPath = copyPath,
                            RequiredFileFormatExtension = "bmp",
                            CompressionValue = 90
                        },
                        // 4x6
                        new ScreenDefinitionImageCopyBase
                        {
                            UpdateToServer = true,
                            UpdateToServerDebugFile = debugfilePrint2,
                            ActivitySaveLogName = "Print digital package (4x6) buy",
                            ButtonZone = new Rectangle(1245, 650, 190, 190),
                            ButtonImageFileName = "printer.png",
                            ButtonBackgroundColor = backColor,
                            ArtificialDelayMili = 600,
                            DataModelSource = new DataModelDynamicHeader
                            {
                                DataKey = "print"
                            },
                            SaveFixedDataOnActivation = new List<(DataModelBase, object)>
                            {
                                (new DataModelQuestionnaire {Identifier = new QuestionIdentifier(2)}, true)
                            },
                            DesiredSize = new Size(1844, 1240),
                            CopyDirectoryPath = copyPath,
                            RequiredFileFormatExtension = "bmp",
                            CompressionValue = 90
                        }
                    }
                },
                DefinitionCheckBoxes = new ScreenDefinitionCheckBoxes
                {
                    UpdateToServer = true,
                    UpdateToServerDebugFile = debugfileCheck,
                    ActivitySaveLogName = "Digital pack save",
                    CheckImageFileName = baseDir + "checkmark3.png",
                    CheckboxDefinitions = new List<ScreenCheckOption>
                    {
                        new ScreenCheckOption
                        {
                            ToggleOnly = true,
                            CheckLocation = new Point(83, 404),
                            Position = new Rectangle(80,397,50,50),
                            DataModels = new List<DataModelBase>
                            {
                                new DataModelQuestionnaire(2)
                            }
                        }
                    }
                },
                DefinitionTimer = new ScreenDefinitionTimerOptions
                {
                    InactivityTimerInterval = 4 *60* 1000,
                    TimerAction = TimerActionEnum.Cancel
                }
            };
            screensController.AddScreen(printScreen);

            #endregion
        }
    }
}