using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using JetBrains.Annotations;
using NLog;
using NomadLogic.Common.Questionnaire;
using NomadLogic.Common.Respondent;
using NomadLogic.Common.Util;
using NomadLogic.Common.Util.Json;
using NomadLogic.Common.Util.Networking.Ftp;
using NomadLogic.iBrandDesktop.Ui.ApplicationScreen.Generic;
using NomadLogic.iBrandDesktop.Ui.ApplicationScreen.Generic.Helpers;
using NomadLogic.iBrandDesktop.Ui.ApplicationScreen.Generic.Helpers.ImageTransforms;
using NomadLogic.iBrandDesktop.Ui.ApplicationScreen.Generic.Helpers.ScreenDefinitions;
using NomadLogic.Multimedia;

// ReSharper disable InconsistentNaming

namespace NomadLogic.iBrandDesktop.Ui.ApplicationScreen.FullscreenLoaders
{
    [PublicAPI]
    public class Loader_NL4_T1 : IFullscreenLoader
    {
        private static readonly Logger log = LogManager.GetCurrentClassLogger();

        public void CreateScreens(TouchScreenController screensController, ScreenConfig sc)
        {
            var baseDir = DirectoryUtil.GetLocalIbrandTerminalDir(sc.ProjectId, sc.QuestionnaireId, sc.TerminalId);

            var ftpConfFile = new FileInfo(baseDir + "ftp.json");
            FtpConfigInfo ftpConf;

            #region Ftp config read.
            if (!File.Exists(ftpConfFile.FullName))
            {
                ftpConf = new FtpConfigInfo
                {
                    Password = "1234",
                    User = "local_test",
                    SshPrivateKeyFilePath = "private_ftp.ppk",
                    SshHostFingerprint = "ssh-rsa 2048 UH7oa2wzOi0BLvFeJg/doGVJcKeICmXA2zWBGeAexW8=",
                    MaxSearchTime = TimeSpan.FromSeconds(10),
                    Url = "192.168.0.99",
                    Port = 22,
                };
                JsonUtil.WriteJsonFile(ftpConfFile, ftpConf);
                ftpConfFile.Refresh();
            }

            ftpConf = JsonUtil.ReadJsonFile<FtpConfigInfo>(ftpConfFile);
            if (!File.Exists(ftpConf.SshPrivateKeyFilePath)) // Ajout du fullpath au besoin
            {
                ftpConf.SshPrivateKeyFilePath = baseDir + ftpConf.SshPrivateKeyFilePath;
            }
            ftpConf.SshPrivateKeyFileInfo = new FileInfo(ftpConf.SshPrivateKeyFilePath);
            #endregion


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

            var cancelZoneOptions = new SectionSkipScreenData
            {
                PageChangeOptions = ScreenChangeOptions.GenerateCancelOptions(),
                SkipValidation = true,
                Zone = new Rectangle(1920 - 50, 0, 50, 50)
            };

            var backColor = Color.White;

            FileInfo debugfileCheck = null; //new FileInfo(baseDir + "get_request_debug_check.txt");
            FileInfo debugfileAssoc = null; //new FileInfo(baseDir + "get_request_debug_assoc.txt");
            FileInfo debugfilePrint1 = null; //new FileInfo(baseDir + "get_request_debug_print_6x8.txt");
            FileInfo debugfilePrint2 = null; //new FileInfo(baseDir + "get_request_debug_print_4x6.txt");

            #region Écran 1 - Choix image

            var pickImageFtp = new GenericScreen(baseDir + "1-Enter number.jpg", baseDir + "1-Enter number.jpg", sc)
            {
                Tag = "Ftp Choice (1)",
                DefinitionFtpImageSource = new ScreenDefinitionFtpFileFinder
                {
                    // Requis pour la transformation via ImageMagick
                    OriginalFileDataModel = new DataModelQuestionnaire
                    {
                        Identifier = new QuestionIdentifier(3)
                    },
                    ImageListTransformOptions = new ImageListTransformOptions
                    {
                        ImageMagickOptions = new List<ImageListTransformOptionsImageMagick>
                        {
                            new ImageListTransformOptionsImageMagick
                            {
                                ResultMapSaveModels = new Dictionary<string, DataModelBase>
                                {
                                    {"1", new DataModelDynamicHeader
                                        {
                                            AllowNullOrEmptyText = false,
                                            DataKey = "ride",
                                            ForceValueType = DataValueType.FileLocal,
                                            InsertMode = DynamicHeaderInsertMode.Replace
                                        }
                                    }
                                },
                                CommandFile = "transform.txt"
                            }
                        }
                    },
                    Position = new Rectangle(471,600,977,56),
                    FieldName = "FtpFile",
                    Mandatory = true,
                    AutomaticFileExtension = ".jpg",
                    FtpConfiguration = ftpConf,
                    TriggerOnMinCharacters = 4,
                    TriggerOnReturn = true,
                    ValidateButtonPosition = new Rectangle(716, 914, 469, 75),
                    ValidateButtonFileName = baseDir + "check_next.png",
                    WorkSequence = new AnimatedImageSequence
                    {
                        Loop = true,
                        DrawLocation = new Point(1448, 914),
                        ImageGenerators = new List<IAnimatedImageSequenceGenerator>
                        {
                            new AnimatedImageSequenceGeneratorFromText
                            {
                                Texts = new List<string>
                                {
                                    "Working.","Working..","Working...","Working...."
                                },
                                ProceduralImageTextInfo = new ProceduralImageTextInfo
                                {
                                    BackgroundColor = backColor,
                                    Color = Color.DarkRed,
                                    BitmapSize = new Size(421, 75),
                                    Center = false,
                                    FontFamilly = "Helvetica",
                                    FontSize = 50,
                                    FontGraphicUnit = GraphicsUnit.Pixel
                                }
                            }
                        }
                    },
                    ActionOnSuccess = ScreenChangeOptions.GenerateBasicNextOptions()
                },
                DefinitionKeyboard = new ScreenDefinitionKeyboard
                {
                    Position = new Rectangle(50, 686, 421, 346),
                    FontDefinition = new FontDefinitionInfo
                    {
                        Size = 20,
                        Color = Color.White,
                        FamilyName = "Helvetica",
                        GraphicsUnit = GraphicsUnit.Pixel,
                        Style = "Bold"
                    },
                    KeyboardType = KeyboardTypeEnum.Number,
                    Padding = 0,
                    ColorContour = Color.Black,
                    ColorGradient = new List<Color>{backColor, Color.Crimson},
                    ColorSelected = Color.DarkRed,
                    WidthContour = 4,
                    ColorMouseDown = Color.Red
                }
            };
            screensController.AddScreen(pickImageFtp);

            #endregion
            
            #region Preview (Écran 2)

            // Preview
            var previewScreen = new GenericScreen(baseDir + "2-preview.jpg", baseDir + "2-preview.jpg", sc)
            {
                Tag = "Preview (2)",
                DefinitionNavigation = new ScreenDefinitionNavigation
                {
                    ZoneList = new List<SectionSkipScreenData>
                    {
                        cancelZoneOptions,
                        new SectionSkipScreenData
                        {
                            PageChangeOptions = ScreenChangeOptions.GenerateBasicBackOptions(),
                            SkipValidation = true,
                            Zone = new Rectangle(340, 914, 469, 74)
                        },
                        new SectionSkipScreenData
                        {
                            PageChangeOptions = ScreenChangeOptions.GenerateBasicNextOptions(),
                            SkipValidation = true,
                            Zone = new Rectangle(1108, 914, 469, 74)
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
                            PreviewHeaderDataKey = "ride",
                            PreviewZone = new Rectangle(629, 221, 675, 675)
                        }
                    }
                }
            };
            screensController.AddScreen(previewScreen);

            #endregion

            #region Écran 3 - Association (scan)
            
            var scanScreen = new GenericUpdateServerScreen(baseDir + "3-scan.jpg", baseDir + "3-scan.jpg", sc)
            {
                LoadAnswersFromServer = true,
                LoadDynamicHeadersFromServer = false,
                // Juste la question deux en gardant les réponses existantes (q1 => external unique identifier)
                LoadKeepExistingAnswers = true,
                LoadQuestionFilters = new List<string> {"q2"},
                SendSkipAnswers = true,
                Tag = "Scan (3)",
                DefinitionRfidScan = new ScreenDefinitionRfidMultipleScan
                {
                    DebugFile = debugfileAssoc,
                    DataModels = new List<DataModelBase>{ 
                        new DataModelDynamicHeader
                        {
                            AllowNullOrEmptyText = false,
                            DataKey = "rfid",
                            ForceValueType = DataValueType.RawText,
                            InsertMode = DynamicHeaderInsertMode.Add
                        },
                        new DataModelQuestionnaire(1)
                    },
                    Position = new Rectangle(495, 555, 921, 48),
                    CheckImagePosition = new Point(1078, 914),
                    CheckImageFileName = "check_next.png",
                    ScanTriggerOnReturn = true,
                    FontDefinition = new FontDefinitionInfo
                    {
                        Size = 40,
                        Color = Color.Black,
                        FamilyName = "Helvetica",
                        GraphicsUnit = GraphicsUnit.Pixel
                    },
                    FieldName = "rfid",
                    MinimumScanCount = 0,
                    MaximumScanCount = 1,
                    MaximumScanCountTriggerNavigationAction = ScreenChangeOptions.GenerateBasicNextOptions(),
                    Mandatory = false,
                    RfidReaderConfigFile = "rfid_config.json",
                    ScanCountTextBoxZone = new Rectangle(809, 642, 299, 48),
                    WaitSequence = null,
                    ErrorImage = new ImageGeneratorBasicFile
                    {
                        FilePath = baseDir + "red_signal.png"
                    },
                    ErrorImagePosition = new Rectangle(1538, 555, 57, 48),
                    UserMessages = new Dictionary<UserErrorEnum, IDictionary<string, string>>
                    {
                        {UserErrorEnum.Mandatory, new Dictionary<string, string>
                        {
                            {"fr", "Vous devez inclure au moins un code."},{"en", "You must include at least one code."}
                        }}
                    },
                    ClearOnError = true
                },
                DefinitionNavigation = new ScreenDefinitionNavigation
                {
                    ZoneList = new List<SectionSkipScreenData>
                    {
                        new SectionSkipScreenData
                        {
                            PageChangeOptions = ScreenChangeOptions.GenerateCancelOptions(),
                            Zone = new Rectangle(340, 914, 469, 74)
                        }
                    }
                },
                DefinitionTimer = new ScreenDefinitionTimerOptions
                {
                    InactivityTimerInterval = 4 *60* 1000,
                    TimerAction = TimerActionEnum.Cancel
                }
            };
            screensController.AddScreen(scanScreen);
            #endregion
            
            #region Écran 4 - Impression et achat
            
            var printScreen = new GenericUpdateServerScreen(baseDir + "4-print-buy.jpg", baseDir + "4-print-buy.jpg", sc)
            {
                LoadAnswersFromServer = false,
                LoadDynamicHeadersFromServer = false,
                LoadKeepExistingAnswers = true,
                SendSkipAnswers = false,
                SendSkipDynamicHeaders = true,
                Tag = "Print-Buy (4)",
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
                                DataKey = "ride"
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
                                DataKey = "ride"
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