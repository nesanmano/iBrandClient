using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using JetBrains.Annotations;
using Newtonsoft.Json;
using NLog;
using NomadLogic.Common;
using NomadLogic.Common.Condition;
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
    /// Chargeur d'écran pour le terminal 3 (Kiosque impression)
    /// </summary>
    [PublicAPI]
    public class Loader_CO1356_T3 : IFullscreenLoader
    {
        private static readonly Logger log = LogManager.GetCurrentClassLogger();
        
        public void CreateScreens(TouchScreenController screensController, ScreenConfig sc)
        {
            var baseDir = DirectoryUtil.GetLocalIbrandTerminalDir(sc.ProjectId, sc.QuestionnaireId, sc.TerminalId);

            var cancelZoneOptions = new SectionSkipScreenData
            {
                PageChangeOptions = ScreenChangeOptions.GenerateCancelOptions(),
                SkipValidation = true,
                Zone = new Rectangle(1813, 0, 103, 119)
            };
            
            #region Config File

            var configFile = new FileInfo(Path.Combine(baseDir, "scan_request_config.json"));
            GalleryScanRequestConfig scanConfig;
            if (!configFile.Exists)
            {
                scanConfig = new GalleryScanRequestConfig
                {
                    TerminalId = 295,
                    QuestionnaireId = 229,
                    DataFilters = new List<string> {"roaming","ride"}
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
                new Rectangle(330, 880, 1263, 84))
            {
                NextLocation = new Rectangle(597, 782, 731, 79),
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
                    SourceTerminalId = scanConfig.TerminalId,
                    DateMinDefinition = new DateTimeCalculatorFixed
                    {
                        FixedDate = DateTime.MinValue
                    },
                    DateMaxDefinition = new DateTimeCalculatorFromNow
                    {
                        ReduceByTimespan = TimeSpan.FromDays(-1) // Demain
                    },
                    SourceDataKeyFilters = scanConfig.DataFilters,
                    SourceQuestionsFilter = new List<string> // On veut la réponse du UEI (pour vérif) et si le répondant à déjà acheté le paquet digital.
                    {
                        "q1", "q2"
                    }
                }
            };
            screensController.AddScreen(barscanScreen);
            
            #endregion

            #region Gallery

            var backColor = ColorTranslator.FromHtml("#0E426A"); //Color.FromArgb(11, 72, 117);
            /*
            var fontDef = new FontDefinitionInfo
            {
                Color = Color.White,
                Size = 25,
                FamilyName = "Helvetica",
                Style = "Bold",
                GraphicsUnit = GraphicsUnit.Pixel
            };
            */

            var galleryScreen = new CaptureEventListingScreen(baseDir + "2-gallery.jpg", baseDir + "2-gallery.jpg", sc)
            {
                ScreenTag = "Gallerie (2)",
                CaptureEventList = new ScreenDefinitionMultimediaEventListing
                {
                    EventDataConverter = _dataConverter, // Possibilité d'utiliser un lot d'image de tests ici.
                    TriggerChangeOnMinimumSelectedItems = 1,
                    PresentationData = new MultimediaEventItemPresentation
                    {
                        MainLayoutBackgroundColor = backColor,
                        LayoutRectangle = new Rectangle(462, 42, 1304, 915),
                        CustomTimeFormat = "HH:mm",
                        ThmbSize = new Size(900, 600),
                        AssetsDirectory = baseDir,
                        ThumbnailDefaultFileName = "photobooth_ui\\not_availiable.png",
                        DateFontDefinition = null, // Pas de date.
                        CenterRow = true
                    },
                    CustomWorkFileDirectory = DirectoryUtil.GetLocalIbrandTerminalCustomDir(sc.ProjectId, sc.QuestionnaireId, sc.TerminalId, "gallery_thumbnail"),
                    StateMessageLocation = new Rectangle(462, 980, 1304, 69),
                    StateMessageImageDefinition = new ProceduralImageTextInfo
                    {
                        BackgroundColor = backColor,
                        BitmapSize = new Size(1304, 69),
                        Color = Color.White,
                        FontGraphicUnit = GraphicsUnit.Pixel,
                        FontSize = 40,
                        FontFamilly = "Helvetica"
                    },
                    StateMessageCustomTexts = new Dictionary<MultimediaEventListingStateEnum, IDictionary<string, string>>
                    {
                        {MultimediaEventListingStateEnum.Ready, new Dictionary<string, string> {{"en", "Pictures ready"}}}
                        ,{MultimediaEventListingStateEnum.NoData, new Dictionary<string, string> {{"en", "No pictures found."}}}
                        ,{MultimediaEventListingStateEnum.WaitingForData, new Dictionary<string, string> {{"en", "Downloading"}}}
                        ,{MultimediaEventListingStateEnum.Error, new Dictionary<string, string> {{"en", "Error while downloading. Please try again."}}}
                    },
                    //StateMessageImageDefinition = aquaConfig.GalleryStateMessageImageDefinition,
                    BackButtonZone = new Rectangle(46, 463, 380, 65),
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

            var printScreen = new GenericScreen(baseDir + "3-print.jpg", baseDir + "3-print.jpg", sc)
            {
                DefinitionNavigation = new ScreenDefinitionNavigation
                {
                    ZoneList = new List<SectionSkipScreenData>
                    {
                        cancelZoneOptions,
                        new SectionSkipScreenData // Back (retour à la gallerie)
                        {
                            PageChangeOptions = ScreenChangeOptions.GenerateBasicBackOptions(),
                            Zone = new Rectangle(337,914,469,75)
                        },
                        new SectionSkipScreenData // Next avec options
                        {
                            Zone = new Rectangle(1114, 914, 469, 75),
                            PageChangeOptions = new ScreenChangeOptions
                            {
                                Options = new List<ScreenChangeOption>
                                {
                                    new ScreenChangeOption // Déjà répondue à l'achat digital
                                    {
                                        Step = 2, // On saute par dessus les deux écrans
                                        Condition = new QuestionCondition(2)
                                    },
                                    new ScreenChangeOption // Sinon, on passe simplement au prochain
                                    {
                                        Step = 1
                                    }
                                }
                            }
                        }
                    }
                },
                DefinitionImagePreviews = new ScreenDefinitionPreviews
                {
                    CheckForReloadEachActivation = true,
                    PreviewDefinitions = new List<ScreenDefinitionPreviewImageBase>
                    {
                        new ScreenDefinitionPreviewImageDynamicHeader
                        {
                            PreviewZone = new Rectangle(510, 200, 900, 600),
                            PreviewHeaderDataKey = "print"
                        }
                    }
                },
                DefinitionImpressions = new ScreenDefinitionImpressions
                {
                    ImpressionDefinitions = new List<ScreenDefinitionImpressionBase>
                    {
                        new ScreenDefinitionImpressionBase
                        {
                            ButtonZone = new Rectangle(1583, 405, 190, 190),
                            ButtonImageFileName = "printer.png",
                            ButtonBackgroundColor = backColor,
                            ArtificialDelayMili = 600,
                            DataModelSource = new DataModelDynamicHeader
                            {
                                DataKey = "print"
                            } 
                        }
                    }
                }
            };
            screensController.AddScreen(printScreen);

            #endregion
            
            #region Buy digital package

            var buyScreen = new GenericScreen(baseDir + "4-digital content.jpg", baseDir + "4-digital content.jpg", sc)
            {
                DefinitionTimer = new ScreenDefinitionTimerOptions
                {
                    TimerAction = TimerActionEnum.End,
                    InactivityTimerInterval = 20 * 1000
                },
                DefinitionNavigation = new ScreenDefinitionNavigation
                {
                    ZoneList = new List<SectionSkipScreenData>
                    {
                        cancelZoneOptions,
                        new SectionSkipScreenData // On ne va que vers l'avant. Mais pas obligé de cocher.
                        {
                            SkipValidation = false,
                            PageChangeOptions = ScreenChangeOptions.GenerateBasicNextOptions(),
                            Zone = new Rectangle(726,914,469,74)
                        }
                    }
                },
                DefinitionCheckBoxes = new ScreenDefinitionCheckBoxes
                {
                    CheckImageFileName = "check2.png",
                    CheckboxDefinitions = new List<ScreenCheckOption>
                    {
                        new ScreenCheckOption
                        {
                            Mandatory = false,
                            ToggleOnly = false,
                            FieldName = "digital",
                            Position = new Rectangle(195, 485 ,1535 ,64),
                            CheckLocation = new Point(199, 494),
                            DataModels = new List<DataModelBase>{
                                new DataModelQuestionnaire
                            {
                                Identifier = new QuestionIdentifier(2, 1, QuestionTypeEnum.Check)
                            }}
                        }
                    }
                }
            };
            screensController.AddScreen(buyScreen);

            #endregion

            #region Download Usb Data

            var usbScreen = new GenericScreen(baseDir + "5-download copy USB.jpg", baseDir + "5-download copy USB.jpg", sc)
            {
                DefinitionTimer = new ScreenDefinitionTimerOptions
                {
                    InactivityTimerInterval = 60*1000,
                    TimerAction = TimerActionEnum.End
                },
                DefinitionNavigation = new ScreenDefinitionNavigation
                {
                    ZoneList = new List<SectionSkipScreenData>
                    {
                        new SectionSkipScreenData
                        {
                            SkipValidation = false,
                            PageChangeOptions = ScreenChangeOptions.GenerateBasicNextOptions(),
                            Zone = new Rectangle(726,914,469,74)
                        }
                    }
                },
                DefinitionFolderCopy = new ScreenDefinitionFolderCopy
                {
                    ButtonImageFileName = "download.png",
                    ButtonBackgroundColor = backColor,
                    ButtonZone = new Rectangle(865, 640,190,190),
                    FileSource = new FileSourceDownloadService()
                }
            };
            screensController.AddScreen(usbScreen);

            #endregion
        }
    }
}