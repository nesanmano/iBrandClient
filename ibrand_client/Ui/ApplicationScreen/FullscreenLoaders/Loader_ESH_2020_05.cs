using System.Collections.Generic;
using System.Drawing;
using JetBrains.Annotations;
using NLog;
using NomadLogic.Common.Respondent;
using NomadLogic.Common.Util;
using NomadLogic.iBrandDesktop.Ui.ApplicationScreen.Generic;
using NomadLogic.iBrandDesktop.Ui.ApplicationScreen.Generic.Helpers;
using NomadLogic.iBrandDesktop.Ui.ApplicationScreen.Generic.Helpers.ImageTransforms;
using NomadLogic.iBrandDesktop.Ui.ApplicationScreen.Generic.Helpers.ImageTransforms.Base;
using NomadLogic.iBrandDesktop.Ui.ApplicationScreen.Generic.Helpers.ScreenDefinitions;

namespace NomadLogic.iBrandDesktop.Ui.ApplicationScreen.FullscreenLoaders
{
    // ReSharper disable once InconsistentNaming
    [PublicAPI]
    public class Loader_ESH_2020_05 : IFullscreenLoader
    {
        [System.Diagnostics.CodeAnalysis.SuppressMessage("CodeQuality", "IDE0052:Remove unread private members", Justification = "Okazou")]
        private static readonly Logger log = LogManager.GetCurrentClassLogger();

        public void CreateScreens(TouchScreenController screensController, ScreenConfig sc)
        {
            
            var baseDir = DirectoryUtil.GetLocalIbrandTerminalDir(sc.ProjectId, sc.QuestionnaireId, sc.TerminalId);

            #region Scan/Start

            var scan = new BarcodeScreen(baseDir + "1-scan to start.jpg", null, sc, new Rectangle(572, 640, 540, 40))
            {
                ScanQuestionNo = 3,
                NextLocation = new Rectangle(504, 704, 672, 70),
                DontCheckCodeOnButton = false,
                ScanTriggerOnReturn = true
            };
            screensController.AddScreen(scan);

            #endregion

            #region Picture

            var picture = new TakePictureManualScreen(sc, baseDir + "2-take picture.jpg", null, 
                new Rectangle(378, 130, 900, 600), //preview
                new Rectangle(614, 815, 450, 70) // Take
                )
            {
                PreviewImageRotation = RotateFlipType.RotateNoneFlipNone,
                CancelZone = new Rectangle(1605, 0, 75, 75),
                LivePreviewOverlays = new List<PreviewOverlayInfo>
                {
                    new PreviewOverlayInfo
                    {
                        FileName = baseDir + "Overlay-preview.png"
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
                                SaveToQuestion = 2,
                                CanvasLayer = new CanvasLayer
                                {
                                    CanvasFileType = ImageFileTypeEnum.Jpeg,
                                    Size = new Size(1740, 1472),
                                    FileName = "background.jpg"
                                },
                                PictureLayer = new ImageLayer
                                {
                                    Crop = true,
                                    Scale = new Size(1598, 1065),
                                    Center = false,
                                    UseChromaKey = true,
                                    Position = new Point(70,70)
                                }
                            }
                        }
                    }
                }
            };
            screensController.AddScreen(picture);

            #endregion

            #region Preview

            var preview = new GenericScreen(baseDir + "3-preview.jpg", null, sc)
            {
                DefinitionEshotsEventDayTransfer = new ScreenDefinitionEshotsEventDayTransfer
                {
                    ShowErrorDialog = true,
                    EventDayIdQuestionNo = 1,
                    AllowRespondentToContinue = false
                },
                DefinitionNavigation = new ScreenDefinitionNavigation
                {
                    ZoneList = new List<SectionSkipScreenData>
                    {
                        new SectionSkipScreenData // Cancel
                        {
                            PageChangeOptions = ScreenChangeOptions.GenerateCancelOptions(),
                            Zone = new Rectangle(1605, 0, 75, 75),
                            SkipValidation = true
                        },
                        new SectionSkipScreenData
                        {
                            PageChangeOptions = ScreenChangeOptions.GenerateBasicNextOptions(),
                            Zone = new Rectangle(877, 819, 313, 67),
                            SkipValidation = false
                        },
                        new SectionSkipScreenData
                        {
                            PageChangeOptions = ScreenChangeOptions.GenerateBasicBackOptions(),
                            Zone = new Rectangle(494, 819, 313, 67),
                            SkipValidation = true
                        }
                    }
                },
                DefinitionImagePreviews =  new ScreenDefinitionPreviews
                {
                    CheckForReloadEachActivation = true,
                    PreviewDefinitions = new List<ScreenDefinitionPreviewImageBase>
                    {
                        new ScreenDefinitionPreviewImageQuestion
                        {
                            PreviewZone = new Rectangle(427, 64, 826, 700),
                            PreviewQuestionIdentifier = new QuestionIdentifier(2)
                        }
                    }
                }
            };
            screensController.AddScreen(preview);

            #endregion

            #region Thx

            var thanks = new GenericScreen(baseDir + "4-thank you.jpg", null, sc)
            {
                DefinitionNavigation = new ScreenDefinitionNavigation
                {
                    ZoneList = new List<SectionSkipScreenData>
                    {
                        new SectionSkipScreenData
                        {
                            PageChangeOptions = ScreenChangeOptions.GenerateBasicNextOptions(),
                            Zone = new Rectangle(0, 0, 1680, 1050)
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

            #endregion
        }
    }
}