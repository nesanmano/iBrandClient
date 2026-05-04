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
using NomadLogic.Common.Util.String;
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
    /// Projet NL-22 pour SUper Aquaclub 2022
    /// </summary>
    [PublicAPI]
    public class Loader_NL22_T2 : IFullscreenLoader
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
                Zone = new Rectangle(1592,24,65,61)
            };
            
            #region Config File

            var configFile = new FileInfo(Path.Combine(baseDir, "scan_request_config.json"));
            GalleryScanRequestConfig scanConfig;
            if (!configFile.Exists)
            {
                scanConfig = new GalleryScanRequestConfig
                {
                    TerminalId = 297,
                    QuestionnaireId = 231,
                    DataFilters = new List<string> {"roaming"}
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
                new Rectangle(536,627,600,50))
            {
                NextLocation = new Rectangle(536,437,600,172),
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
                    SourceQuestionsFilter = new List<string> // On veut la réponse du UEI (pour vérif)
                    {
                        "q1"
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
                        LayoutRectangle = new Rectangle(603,162,471,719),
                        CustomTimeFormat = "HH:mm",
                        ThmbSize = new Size(250, 250),
                        AssetsDirectory = baseDir,
                        ThumbnailDefaultFileName = @"photobooth_ui\not_availiable.png",
                        DateFontDefinition = null, // Pas de date.
                        CenterRow = false
                    },
                    CustomWorkFileDirectory = DirectoryUtil.GetLocalIbrandTerminalCustomDir(sc.ProjectId, sc.QuestionnaireId, sc.TerminalId, "gallery_thumbnail"),
                    StateMessageLocation = new Rectangle(25,810,550,50),
                    StateMessageImageDefinition = new ProceduralImageTextInfo
                    {
                        BackgroundColor = backColor,
                        BitmapSize = new Size(550, 50),
                        Color = Color.Navy,
                        FontGraphicUnit = GraphicsUnit.Pixel,
                        FontSize = 37,
                        FontFamilly = "Helvetica"
                    },
                    StateMessageCustomTexts = new Dictionary<MultimediaEventListingStateEnum, IDictionary<string, string>>
                    {
                        {MultimediaEventListingStateEnum.Ready, new Dictionary<string, string> {{"en", "Images prêtes."}}}
                        ,{MultimediaEventListingStateEnum.NoData, new Dictionary<string, string> {{"en", "Aucune image trouvée."}}}
                        ,{MultimediaEventListingStateEnum.WaitingForData, new Dictionary<string, string> {{"en", "Téléchargement en cours"}}}
                        ,{MultimediaEventListingStateEnum.Error, new Dictionary<string, string> {{"en", "Erreur de téléchargement. Réessayez."}}}
                    },
                    BackButtonZone = new Rectangle(1190,162,491,77),
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
                        /*
                        new SectionSkipScreenData // Back (retour à la gallerie)
                        {
                            PageChangeOptions = ScreenChangeOptions.GenerateBasicBackOptions(),
                            Zone = new Rectangle(337,914,469,75)
                        },
                        */
                        new SectionSkipScreenData // Next avec options
                        {
                            Zone = new Rectangle(597,802,491,77),
                            PageChangeOptions = new ScreenChangeOptions
                            {
                                Options = new List<ScreenChangeOption>
                                {
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
                            PreviewZone = new Rectangle(521,149,640,640),
                            PreviewHeaderDataKey = "print"
                        }
                    }
                },
                DefinitionImpressions = new ScreenDefinitionImpressions
                {
                    ImpressionDefinitions = new List<ScreenDefinitionImpressionBase>
                    {
                        new ScreenDefinitionImpressionPutOnCanvas
                        {
                            ButtonZone = new Rectangle(1317,423,199,199),
                            ButtonImageFileName = "printer.png",
                            ButtonBackgroundColor = backColor,
                            ArtificialDelayMili = 600,
                            DataModelSource = new DataModelDynamicHeader
                            {
                                DataKey = "print"
                            },
                            BaseDirectory = new DirectoryInfo(DirectoryUtil.GetLocalIbrandTerminalDir(sc.GetTerminalInfo())),
                            CanvasImageFileName = "Cadre_photo.jpg",
                            ImagePosition = new Rectangle(45,60,975,975)
                        }
                    }
                }
            };
            screensController.AddScreen(printScreen);

            #endregion
        }
    }
}