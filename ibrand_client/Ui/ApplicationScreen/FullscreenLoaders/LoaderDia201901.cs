using System;
using System.Collections.Generic;
using System.Drawing;
using JetBrains.Annotations;
using NomadLogic.Common.Condition;
using NomadLogic.Common.Questionnaire;
using NomadLogic.Common.Respondent;
using NomadLogic.Common.Util;
using NomadLogic.iBrandDesktop.Ui.ApplicationScreen.custom.Diamond;
using NomadLogic.iBrandDesktop.Ui.ApplicationScreen.Generic;
using NomadLogic.iBrandDesktop.Ui.ApplicationScreen.Generic.Helpers;
using NomadLogic.iBrandDesktop.Ui.ApplicationScreen.Generic.Helpers.ImageTransforms;
using NomadLogic.Multimedia;

namespace NomadLogic.iBrandDesktop.Ui.ApplicationScreen.FullscreenLoaders
{
    [PublicAPI]
    public class LoaderDia201901 : IFullscreenLoader
    {
        public void CreateScreens(TouchScreenController screensController, ScreenConfig sc)
        {
            // Les écrans sont en format 1050x1680, orientation portrait
            // La prévisualisation est de 675x945, orientation portrait.

            var uiDir = DirectoryUtil.GetLocalIbrandTerminalDataDir(sc.ProjectId, sc.QuestionnaireId, sc.TerminalId,
                DirectoryUtil.ProjectDataDirectoriesEnum.photobooth_ui, DirectoryUtil.ProjectDataDirectoriesModEnum.none);

            #region Constantes

            var E_00_EN = uiDir + "1-language.jpg";
            //var E_00_FR = uiDir + "1-language_fr.jpg"; // Pas utilisé
            var E_01_EN = uiDir + "2-select quote.jpg";
            var E_01_FR = uiDir + "2-select quote_fr.jpg";
            var E_02_EN = uiDir + "3-a-terms and conditions.jpg";
            var E_02_FR = uiDir + "3-a-terms and conditions_fr.jpg";
            var E_03_EN = uiDir + "3-b-are you sure.jpg";
            var E_03_FR = uiDir + "3-b-are you sure_fr.jpg";
            var E_04_EN = uiDir + "4-take picture.jpg";
            var E_04_FR = uiDir + "4-take picture_fr.jpg";
            var E_05_EN = uiDir + "5-preview.jpg";
            var E_05_FR = uiDir + "5-preview_fr.jpg";
            var E_10_EN = uiDir + "6-thank you.jpg";
            var E_10_FR = uiDir + "6-thank you.jpg";

            var FIRSTPIC_IMAGE_SIZE = new Size(1065, 1490);
            var DISPLAY_BKG_FILENAME = uiDir + "bkg.jpg";
            const double RATIO_5x7 = 5.0 / 7.0;
            const double RATIO_SPACE = 0.1d; // 10% espace du top
            var DISPLAY_CANVAS_SIZE = new Size(1920, 1080);
            var IMAGE_CANDIDATE_HEIGHT = DISPLAY_CANVAS_SIZE.Height * (1.0 - RATIO_SPACE);
            var DISPLAY_IMAGE_FRAME_SIZE = new Size((int)(IMAGE_CANDIDATE_HEIGHT * RATIO_5x7), (int)IMAGE_CANDIDATE_HEIGHT);
            var DISPLAY_POS_IMAGE_FRAME = new Point(
                (int)((DISPLAY_CANVAS_SIZE.Width - DISPLAY_IMAGE_FRAME_SIZE.Width) / 2.0),
                (int)(DISPLAY_CANVAS_SIZE.Height * RATIO_SPACE / 2.0));

            var tabOverlays = new string[6, 1, 2];
            // Mieux que d'avoir 40 lignes d'initialisation...
            for (var quote = 0; quote < 6; quote++)
            {
                for (var lg = 0; lg < 1; lg++)
                {
                    for (var prev = 0; prev < 2; prev++)
                    {
                        var prevStr = prev == 0 ? "" : "_p";
                        tabOverlays[quote, lg, prev] = $"{uiDir}overlay_0{quote + 1}{prevStr}.png";
                    }
                }
            }

            const int PREVIEW_W = 675;
            const int PREVIEW_H = 944;
            const int PREVIEW_Y = 265;
            var UPPER_LEFT = new Point(0, 0);

            const int END_INACTIVITY = 10 * 1000;

            // Indices Y des positions du checkmark des citations
            var tabYindex = new[]
            {
                240, 308, 375, 443, 510, 577
            };

            #endregion

            #region 00 - Ecran A - Bienvenue

            {
                var home = new HomeScreen(E_00_EN, sc,
                        null, //new Rectangle(534, 767, 339, 74), // FR
                        new Rectangle(0,0,1050,1680)) // EN
                {
                    QuestionLanguageNo = 1
                };
                screensController.AddScreen(home);
            }

            #endregion

            #region 01- Ecran B - Citation 

            {
                const int BKG_SEL_X = 219;
                const int BKG_SEL_W = 700;
                const int BKG_SEL_H = 60;

                var selections = new List<AnswerZone>();
                var bmp = FileUtil.LoadBitmap(uiDir + "selection.png");

                // Plutôt que à la main...
                for (var i = 0; i < 6; i++)
                {
                    var bkgOption = new AnswerZone()
                    {
                        SelectionActivatedImage = bmp,
                        Zone = new Rectangle(BKG_SEL_X, tabYindex[i], BKG_SEL_W, BKG_SEL_H),
                        AnswersUponSelection = new List<Answer>
                        {
                            //new Answer(3, 2, 1, i+1)
                        }
                    };
                    selections.Add(bkgOption);
                }

                var citation = new BackgroundSelectionScreen_Dia201901(E_01_EN, E_01_FR, sc)
                {
                    MessageNoSelectionEn = "You must choose one quote, or make your own.",
                    MessageNoSelectionFr = "Vous devez choisir une citation, ou en fournir une.",
                    NextArea = new Rectangle(536,1310,336,73),
                    BackgroundSelectionOptions = selections
                };
                screensController.AddScreen(citation);
            }

            #endregion

            #region 02- Ecran C - Termes et Conditions

            {
                var terms = new Questionnaire_Dia201901(E_02_EN, E_02_FR, sc)
                {
                };
                screensController.AddScreen(terms);
            }

            #endregion

            #region 03- Ecran C2 - Confirmation Déclinaison

            {
                var skipDefs = new List<SectionSkipScreenData>
                {
                    new SectionSkipScreenData
                    {
                        Zone = new Rectangle(532, 926, 343, 77),
                        PageChangeOptions = ScreenChangeOptions.GenerateBasicBackOptions()
                    },
                    new SectionSkipScreenData
                    {
                        Zone = new Rectangle(186, 926, 343, 77),
                        PageChangeOptions = ScreenChangeOptions.GenerateCancelOptions()
                    }
                };

                var skip = new SectionSkipScreen(E_03_EN, E_03_FR, sc)
                {
                    ZoneList = skipDefs
                };
                screensController.AddScreen(skip);
            }

            #endregion

            #region 04- Ecran D - Prise photo Manuelle (avec Live Preview)
            {
                var liveOverlays = new List<PreviewOverlayInfo>();

                // Pas mal plus rapide que de les faire à la main...
                for (var quote = 0; quote < 6; quote++)
                {
                    for (var lg = 0; lg < 1; lg++)
                    {
                        // Live Preview
                        //var prevInfo = new PreviewOverlayInfo(tabOverlays[quote, lg, 1], UPPER_LEFT, null, new QuestionCondition(2, 1, (quote + 1)));

                        //liveOverlays.Add(prevInfo);
                    }
                }

                var transforms = new ImageListTransformOptions
                {
                    ImageMagickOptions = new List<ImageListTransformOptionsImageMagick> { 
                        // Si quotation personalisée inexistante
                        new ImageListTransformOptionsImageMagick() {
                            Condition = new QuestionCondition(5, 1, true),
                            CommandFile = "imgMgk_trans_01.txt",
                            ResultMapSaveModels = new Dictionary<string, DataModelBase> { {"1", new DataModelQuestionnaire {Identifier = new QuestionIdentifier(3)} } },
                            Timeout = TimeSpan.FromSeconds(20)
                        },
                        // Si avec quotation personalisée
                        new ImageListTransformOptionsImageMagick() {
                            Condition = new QuestionCondition(5, 1),
                            CommandFile = "imgMgk_trans_02.txt",
                            ResultMapSaveModels = new Dictionary<string, DataModelBase> { {"1", new DataModelQuestionnaire {Identifier = new QuestionIdentifier(3)} } }
                        }
                    },
                    // Assume que le transformation de Display on TV est après celle de ImageMagick.
                    FirstPictureOptions = new ImageListTransformOptionsFirstPicture()
                    {
                        Condition = new QuestionCondition(7, 1),
                        NewSize = DISPLAY_IMAGE_FRAME_SIZE,
                        NewPos = DISPLAY_POS_IMAGE_FRAME,
                        CanvasSize = DISPLAY_CANVAS_SIZE,
                        InputFromQuestion = 3,
                        BackgroundImage = DISPLAY_BKG_FILENAME,
                        SaveDataModel = new DataModelQuestionnaire
                        {
                            Identifier = new QuestionIdentifier(8)
                        },
                    }
                };

                var picture = new TakePictureManualScreen(sc, E_04_EN, E_04_FR,
                    new Rectangle(-1, PREVIEW_Y, PREVIEW_W, PREVIEW_H), //Preview
                    new Rectangle(525, 1305, 354, 87) // Take
                )
                {
                    SaveOriginalImage = new DataModelQuestionnaire {Identifier = new QuestionIdentifier(3)},
                    LivePreviewOverlays = liveOverlays,
                    ImageListTranformOptions = transforms,
                    PreviewImageRotation = RotateFlipType.Rotate90FlipNone,
                    CreatingImageSequence = new AnimatedImageSequence
                    {
                        Loop = true,
                        DrawLocation = new Point(100, 1300),
                        ImageGenerators = new List<IAnimatedImageSequenceGenerator>
                        {
                            new AnimatedImageSequenceGeneratorFromText
                            {
                                ProceduralImageTextInfo = new ProceduralImageTextInfo
                                {
                                    Color = Color.FromArgb(100, 58, 0),
                                    FontSize = 40,
                                    BitmapSize = new Size(200, 50),
                                    Center = true,
                                    FontGraphicUnit = GraphicsUnit.Pixel
                                },
                                Texts = new List<string>
                                {
                                    ".", "..", "..."
                                }
                            }
                        }
                    }
                };
                screensController.AddScreen(picture);
            }

            #endregion

            #region 05- Ecran E - Preview photo finale

            {
                var preview = new PreviewScreen(E_05_EN, E_05_FR, sc, null,
                    new Rectangle(527, 1310, 344, 76), // Next
                    new Rectangle(183, 1310, 344, 76), // Back
                    PREVIEW_Y, new Size(PREVIEW_W, PREVIEW_H))
                {
                    NextPageOptions = ScreenChangeOptions.GenerateBasicNextOptions(),
                    BackPageOptions = ScreenChangeOptions.GenerateBasicBackOptions()
                };
                screensController.AddScreen(preview);
            }

            #endregion

            #region 10- Ecran H - (Fin) Déclinaison, avec impression
            {
                var thxScreenH = new ThanksEndScreen(E_10_EN, sc, E_10_FR)
                {
                    InactivityTimerInterval = END_INACTIVITY,
                    ForcePrintOnActivation = true
                };
                screensController.AddScreen(thxScreenH);
            }
            #endregion

        }
    }
}