using System.Collections.Generic;
using System.Drawing;
using System.IO;
using JetBrains.Annotations;
using NLog;
using NomadLogic.Common.Questionnaire;
using NomadLogic.Common.Respondent;
using NomadLogic.Common.Util;
using NomadLogic.Common.Util.Json;
using NomadLogic.iBrandDesktop.Ui.ApplicationScreen.Generic;
using NomadLogic.iBrandDesktop.Ui.ApplicationScreen.Generic.Helpers;
using NomadLogic.iBrandDesktop.Ui.ApplicationScreen.Generic.Helpers.ImageTransforms;
using NomadLogic.iBrandDesktop.Ui.ApplicationScreen.Generic.Helpers.ScreenDefinitions;

// ReSharper disable InconsistentNaming

namespace NomadLogic.iBrandDesktop.Ui.ApplicationScreen.FullscreenLoaders
{
    /// <summary>
    /// Chargeur d'écran pour le terminal 2 (Caméra ambulante)
    /// </summary>
    [PublicAPI]
    public class Loader_CO1356_T2 : IFullscreenLoader
    {
        private static readonly Logger log = LogManager.GetCurrentClassLogger();
        
        public void CreateScreens(TouchScreenController screensController, ScreenConfig sc)
        {
            var baseDir = DirectoryUtil.GetLocalIbrandTerminalDir(sc.ProjectId, sc.QuestionnaireId, sc.TerminalId);

            var cancelZoneOptions = new SectionSkipScreenData
            {
                PageChangeOptions = ScreenChangeOptions.GenerateCancelOptions(),
                SkipValidation = true,
                Zone = new Rectangle(1920-50, 0, 50, 50)
            };

            var baseRectanglePreview = new Rectangle(278,31,1352,901);

            // Configuration externe du preview.
            var previewConfig = new FileInfo(baseDir + "preview_config.json");
            if (!previewConfig.Exists)
            {
                if (!JsonUtil.WriteJsonFile(previewConfig, baseRectanglePreview))
                {
                    log.Error("Could not create external config file pour preview.");
                }
                previewConfig.Refresh();
            }

            var previewSize = JsonUtil.ReadJsonFile<Rectangle>(previewConfig);

            #region Prise photo
            
            var picture = new TakePictureManualScreen(sc, baseDir + "1-take picture.jpg", baseDir + "1-take picture.jpg", null, 
                new Rectangle(879, 263, 158, 120))
            {
                //CancelZone = cancelZoneOptions.Zone,
                ImageListTranformOptions = new ImageListTransformOptions
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
                            DataKey = "roaming",
                            AllowNullOrEmptyText = false,
                            ForceValueType = DataValueType.FileLocal,
                            InsertMode = DynamicHeaderInsertMode.Replace
                        }
                    }
                },
                Tag = "TakePicture (1)"
            };
            screensController.AddScreen(picture);
            #endregion

            #region Preview

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
                            Zone = new Rectangle(1055, 962, 469, 74)
                        },
                        new SectionSkipScreenData
                        {
                            PageChangeOptions = ScreenChangeOptions.GenerateBasicBackOptions(),
                            SkipValidation = true,
                            Zone = new Rectangle(396, 962, 469, 74)
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
                            PreviewZone = previewSize,
                            PreviewHeaderDataKey = "roaming"
                        }
                    }
                }
            };
            screensController.AddScreen(preview);
            
            #endregion
            
            #region Scan RFID

            var scanScreen = new GenericScreen(baseDir + "3-scan.jpg", baseDir + "3-scan.jpg", sc)
            {
                Tag = "Rfid (3)",
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
                    Position = new Rectangle(480, 426, 956, 50),
                    CheckImagePosition = new Point(1462, 429),
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
                    MinimumScanCount = 1,
                    Mandatory = true,
                    RfidReaderConfigFile = "rfid_config.json",
                    ScanCountTextBoxZone = new Rectangle(879, 532, 170, 38),
                    WaitSequence = null,
                    ErrorImage = new ImageGeneratorBasicFile
                    {
                        FilePath = baseDir + "red_signal.png"
                    },
                    ErrorImagePosition = new Rectangle(388, 426, 50, 50),
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
                            Zone = new Rectangle(726, 895, 469, 74)
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