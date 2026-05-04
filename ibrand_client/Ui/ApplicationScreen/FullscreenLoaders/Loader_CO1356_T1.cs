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
    public class Loader_CO1356_T1 : IFullscreenLoader
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

            var cancelZoneOptions = new SectionSkipScreenData
            {
                PageChangeOptions = ScreenChangeOptions.GenerateCancelOptions(),
                SkipValidation = true,
                Zone = new Rectangle(1920 - 50, 0, 50, 50)
            };

            var backColor = ColorTranslator.FromHtml("#0E426A");

            #region Écran 1 - Choix image

            var pickImageFtp = new GenericScreen(baseDir + "1-Enter number.jpg", baseDir + "1-Enter number.jpg", sc)
            {
                DefinitionFtpImageSource = new ScreenDefinitionFtpFileFinder
                {
                    ImageListTransformOptions = new ImageListTransformOptions
                    {
                        FirstPictureOptions = new ImageListTransformOptionsFirstPicture
                        {
                            OverlaySelector = new ImageSelectorSingle
                            {
                                ImageFile = baseDir + "overlay.png"
                            },
                            NewSize = new Size(1598, 1065),
                            JpegSaveQuality = 90,
                            SaveDataModel = new DataModelDynamicHeader
                            {
                                AllowNullOrEmptyText = false,
                                DataKey = "ride",
                                ForceValueType = DataValueType.FileLocal,
                                InsertMode = DynamicHeaderInsertMode.Replace
                            }
                        }
                    },
                    Position = new Rectangle(690, 582, 713, 57),
                    FieldName = "FtpFile",
                    Mandatory = true,
                    AutomaticFileExtension = ".jpg",
                    FtpConfiguration = ftpConf,
                    TriggerOnMinCharacters = 4,
                    TriggerOnReturn = true,
                    ValidateButtonPosition = new Rectangle(988, 672, 44, 44),
                    ValidateButtonFileName = baseDir + "check.png",
                    WorkSequence = new AnimatedImageSequence
                    {
                        Loop = true,
                        DrawLocation = new Point(988, 988),
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
                                    Color = Color.White,
                                    BitmapSize = new Size(890, 60),
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
                    Position = new Rectangle(33, 582, 603, 465),
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
                    ColorContour = Color.White,
                    ColorGradient = new List<Color>{backColor, Color.Aqua},
                    ColorSelected = Color.CornflowerBlue,
                    WidthContour = 4,
                    ColorMouseDown = Color.DarkCyan
                }
            };
            screensController.AddScreen(pickImageFtp);

            #endregion

            /*
            #region Écran 2 - Présentation

            var preview = new GenericScreen(baseDir + "2-preview.jpg", baseDir + "2-preview.jpg", sc)
            {
                Tag = "Preview (2)",
                DefinitionTimer = new ScreenDefinitionTimerOptions
                {
                    InactivityTimerInterval = 4 *60* 1000,
                    TimerAction = TimerActionEnum.Cancel
                },
                DefinitionNavigation = new ScreenDefinitionNavigation
                {
                    ZoneList = new List<SectionSkipScreenData>
                    {
                        cancelZoneOptions,
                        new SectionSkipScreenData
                        {
                            PageChangeOptions = ScreenChangeOptions.GenerateBasicNextOptions(),
                            Zone = new Rectangle(1055, 895, 769, 74)
                        },
                        new SectionSkipScreenData
                        {
                            PageChangeOptions = ScreenChangeOptions.GenerateBasicBackOptions(),
                            SkipValidation = true,
                            Zone = new Rectangle(396, 895, 469, 74)
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
                            PreviewZone = new Rectangle(509, 153, 900, 600),
                            PreviewHeaderDataKey = "ride"
                        }
                    }
                }
            };
            screensController.AddScreen(preview);
            #endregion
            */

            #region Écran 3 - Impression

            var printScreen = new GenericScreen(baseDir + "3-print.jpg", baseDir + "3-print.jpg", sc)
            {
                Tag = "Print (3)",
                DefinitionNavigation = new ScreenDefinitionNavigation
                {
                    ZoneList = new List<SectionSkipScreenData>
                    {
                        cancelZoneOptions,
                        new SectionSkipScreenData // Back (retour à la saisie d'image)
                        {
                            PageChangeOptions = ScreenChangeOptions.GenerateBasicBackOptions(),
                            Zone = new Rectangle(337,914,469,74)
                        },
                        new SectionSkipScreenData // Next avec options
                        {
                            Zone = new Rectangle(1114, 914, 469, 74),
                            PageChangeOptions = new ScreenChangeOptions
                            {
                                Options = new List<ScreenChangeOption>
                                {
                                    new ScreenChangeOption
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
                            PreviewZone = new Rectangle(510, 150, 900, 600),
                            PreviewHeaderDataKey = "ride"
                        }
                    }
                },
                DefinitionImpressions = new ScreenDefinitionImpressions
                {
                    ImpressionDefinitions = new List<ScreenDefinitionImpressionBase>
                    {
                        new ScreenDefinitionImpressionBase
                        {
                            ButtonZone = new Rectangle(1623, 354, 190, 190),
                            ButtonImageFileName = "printer.png",
                            ButtonBackgroundColor = backColor,
                            ArtificialDelayMili = 600,
                            DataModelSource = new DataModelDynamicHeader
                            {
                                DataKey = "ride"
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

            #region Écran 4 - Association (scan)
            
            var scanScreen = new GenericScreen(baseDir + "4-scan.jpg", baseDir + "4-scan.jpg", sc)
            {
                Tag = "Scan (4)",
                DefinitionRfidScan = new ScreenDefinitionRfidMultipleScan
                {
                    DataModels = new List<DataModelBase>{
                        new DataModelDynamicHeader
                    {
                        AllowNullOrEmptyText = false,
                        DataKey = "rfid",
                        ForceValueType = DataValueType.RawText,
                        InsertMode = DynamicHeaderInsertMode.Add
                    }},
                    Position = new Rectangle(495, 555, 921, 48),
                    CheckImagePosition = new Point(1439, 555),
                    CheckImageFileName = "check.png",
                    ScanTriggerOnReturn = true,
                    //CrossCheckListFileName = "rfid_list.txt",
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
                    MaximumScanCountTriggerNavigationAction = null,
                    Mandatory = false,
                    RfidReaderConfigFile = "rfid_config.json",
                    ScanCountTextBoxZone = new Rectangle(818, 635, 283, 46),
                    WaitSequence = null,
                    ErrorImage = new ImageGeneratorBasicFile
                    {
                        FilePath = baseDir + "red_signal.png"
                    },
                    ErrorImagePosition = new Rectangle(425, 555, 48, 48),
                    UserMessages = new Dictionary<UserErrorEnum, IDictionary<string, string>>
                    {
                        {UserErrorEnum.Mandatory, new Dictionary<string, string>
                        {
                            {"fr", "Vous devez inclure au moins un code."},{"en", "You must included at least une code."}
                        }}
                    },
                    ClearOnError = true
                },
                DefinitionNavigation = new ScreenDefinitionNavigation
                {
                    ZoneList = new List<SectionSkipScreenData>
                    {
                        cancelZoneOptions,
                        new SectionSkipScreenData
                        {
                            PageChangeOptions = ScreenChangeOptions.GenerateBasicNextOptions(),
                            Zone = new Rectangle(1114, 914, 469, 74)
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
        }
    }
}