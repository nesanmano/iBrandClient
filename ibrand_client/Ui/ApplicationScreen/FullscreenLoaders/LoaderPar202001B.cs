using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using JetBrains.Annotations;
using NLog;
using NomadLogic.Common;
using NomadLogic.Common.Questionnaire;
using NomadLogic.Common.Respondent;
using NomadLogic.Common.Services;
using NomadLogic.Common.Util;
using NomadLogic.Common.Util.DateAndTime;
using NomadLogic.Common.Util.Json;
using NomadLogic.iBrandDesktop.Services;
using NomadLogic.iBrandDesktop.Ui.ApplicationScreen.Generic;
using NomadLogic.iBrandDesktop.Ui.ApplicationScreen.Generic.Helpers;
using NomadLogic.iBrandDesktop.Ui.ApplicationScreen.Generic.Helpers.Gallery;
using NomadLogic.iBrandDesktop.Ui.ApplicationScreen.Generic.Helpers.ScreenDefinitions;
using NomadLogic.Multimedia;

#pragma warning disable 612

namespace NomadLogic.iBrandDesktop.Ui.ApplicationScreen.FullscreenLoaders
{
    [PublicAPI]
    public class LoaderPar202001B : IFullscreenLoader
    {
        private static readonly Logger log = LogManager.GetCurrentClassLogger();

        public void CreateScreens(TouchScreenController screensController, ScreenConfig sc)
        {
            var baseDir = DirectoryUtil.GetLocalIbrandTerminalDir(sc.ProjectId, sc.QuestionnaireId, sc.TerminalId);
            sc.QuestionFirstPictureNo = 11;
            sc.QuestionPictureToPrintNo = 11;
            sc.QuestionNumberOfPrintNo = 10;
            
            #region Gallerie
            
            var loadSeq = new AnimatedImageSequence
            {
                Loop = true,
                DrawLocation = new Point(340, 8),
                ImageGenerators = new List<IAnimatedImageSequenceGenerator>
                {
                    new AnimatedImageSequenceGeneratorFromText
                    {
                        ProceduralImageTextInfo = new ProceduralImageTextInfo
                        {
                            Color = Color.Black,
                            FontSize = 15,
                            BitmapSize = new Size(200, 37),
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
                DrawLocation = new Point(340, 8),
                ImageGenerators = new List<IAnimatedImageSequenceGenerator>
                {
                    new AnimatedImageSequenceGeneratorFromText
                    {
                        ProceduralImageTextInfo = new ProceduralImageTextInfo
                        {
                            Color = Color.Black,
                            FontSize = 15,
                            BitmapSize = new Size(400, 37),
                            FontFamilly = "Arial"
                        },
                        Texts = new List<string>
                        {
                            "No images could be loaded from server."
                        }
                    }
                }
            };

            var saveFormatter = new SaveGallerySelectionFromQuestionnaireData
            {
                CondensedAnswerDatabase = ServicesContainer.GetService<IbrandDatabase>(),
                PivotQuestion = new QuestionIdentifier(8),
                QuestionMapping = new Dictionary<QuestionIdentifier, QuestionIdentifier>
                {
                    {new QuestionIdentifier(1), new QuestionIdentifier(11) {QuestionType = QuestionTypeEnum.PhotoText} }
                }
            };
            
            #region data minimale de la gallerie

            DatetimeCalculator dateCalculator;

            // Test si une configuration JSON existe:
            var dateCalcConfigFile = new FileInfo(baseDir + "gallery_date_config.json");
            if (!dateCalcConfigFile.Exists)
            {
                // Par défaut, uniquement aujourd'hui à partir de minuit.
                dateCalculator = new DateTimeCalculatorFromNow
                {
                    StartFromDateOnly = true
                }; 

                var s = JsonUtil.WriteJsonFile(dateCalcConfigFile, dateCalculator); 
                if (!s) throw new System.Exception("Could not write to disk gallery local configuration file.");
                dateCalcConfigFile.Refresh();
            }
            
            // Mise à jour via configuration JSON
            dateCalculator = JsonUtil.ReadJsonFile<DatetimeCalculator>(dateCalcConfigFile);

            #endregion
            var galleryJson = new FileInfo(baseDir + "gallery.properties");
            
            var prop = new PropertiesHolder(galleryJson);

            if (!prop.EnsureExistingValues("srcQId", "srcTId"))
            {
                throw new System.Exception("Invalid value or missing properties file required to run the gallery.");
            }

            var srcQId = prop.GetValue("srcQId");
            var srcTId = prop.GetValue("srcTId");

            var galSource = new GallerySourceFromServerRespondantData
            {
                ServerSourceQuestionaireId = int.Parse(srcQId),
                ServerSourceTerminalId = int.Parse(srcTId),
                WorkDirectory = new DirectoryInfo(DirectoryUtil.GetLocalIbrandTerminalCustomDir(sc.ProjectId, sc.QuestionnaireId, sc.TerminalId, "gallery_local_file")),
                LocalCurrentQuestionaireId = sc.QuestionnaireId,
                ReferenceDate = dateCalculator,
                SaveFormatter = saveFormatter,
                QuestionIdentifierGalleryImage = new QuestionIdentifier(1),
                QuestionFetchIdentifiers = new List<QuestionIdentifier>
                {
                    new QuestionIdentifier(1)
                },
                MaxDownloadRequests = 20,
                UpdateEventTime = TimeSpan.FromSeconds(10),
                Filters = new List<ComparableFileInfoFilterBase>
                {
                    new ComparableFileInfoFilterServerDate
                    {
                        MinimalDate = dateCalculator
                    }
                }
            };

            var pagiUi = new PaginationUiElements<ComparableFileInfo>
            {
                CondensedPaginationControls = new Rectangle(340, 790, 1000, 70),
                CondensedUseFullJumps = true,
                DefaultFontDefinition = new FontDefinitionInfo
                {
                    Color = Color.Black,
                    Size = 50,
                    Style = "Bold",
                    FamilyName = "Arial"
                },
                CondensedNumberOfPagesOptions = 5,
                ShowZeroBased = false,
                //PreviousZone = new ScreenClickZoneImage
                //{
                //   Zone = new Rectangle(50, 350, 77, 77),
                //    ImageFile = baseDir + "left arrow.png"
                //},
                //NextZone = new ScreenClickZoneImage
                //{
                //    Zone = new Rectangle(1553, 350, 77, 77),
                //    ImageFile = baseDir + "right arrow.png"
                //},
                CondensedFirstCommandImagePath = baseDir + "First.png",
                CondensedPreviousCommandImagePath = baseDir + "Back1.png",
                CondensedNextCommandImagePath = baseDir + "Next1.png",
                CondensedLastCommandImagePath = baseDir + "Last.png"
            };
            
            var home = new GenericScreen(baseDir + "1. Gallery.jpg", null, sc)
            {
                VisualDebugMode = false,
                DefinitionGallery = new ScreenDefinitionGalleryFromFiles
                {
                    Mandatory = true,
                    LoadingImageSequence = loadSeq,
                    ErrorImageSequence = errorSeq,
                    GalleryZone = new Rectangle(140, 80, 1400, 680),
                    GalleryImageRatio = 1.5f,
                    PresentationSize = new Size(3,2),
                    SelectionPageChangeOptions = ScreenChangeOptions.GenerateBasicNextOptions(),
                    PaginationUi = pagiUi,
                    PaddingRatio = 0.03,
                    GallerySource = galSource,
                    UseThumbnails = true,
                    ShowReversed = true
                }
            };
            screensController.AddScreen(home);
            #endregion

            #region 2- Share

            var checks = new List<ScreenCheckOption>
            {
                new ScreenCheckOption
                {
                    Position = new Rectangle(1336, 446, 142, 141),
                    FieldName = "enable digital send",
                    Mandatory = false,
                    DataModels = new List<DataModelBase>{
                        new DataModelQuestionnaire
                    {
                        Identifier = new QuestionIdentifier(9)
                    }},
                    ToggleOnly = true
                }
            };

            var start = new ShareScreen(baseDir + "2. Print et envoi digital.jpg", sc, null, null, null, 
                new Rectangle(1337, 198, 142, 141), // Print
                null, null, null, null,
                new Rectangle(587, 737, 506, 82), //End
                new Rectangle(366, 75, 900, 600), //preview
                checks
                )
            {
                CheckmarkImagePath = baseDir + "bouton email gris.png"
            };
            screensController.AddScreen(start);
            #endregion
        }
    }
}