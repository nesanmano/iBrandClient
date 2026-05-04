using System.Collections.Generic;
using System.Drawing;
using JetBrains.Annotations;
using NomadLogic.Common.Questionnaire;
using NomadLogic.Common.Respondent;
using NomadLogic.Common.Util;
using NomadLogic.iBrandDesktop.Ui.ApplicationScreen.custom.Influence;
using NomadLogic.iBrandDesktop.Ui.ApplicationScreen.Generic;
using NomadLogic.iBrandDesktop.Ui.ApplicationScreen.Generic.Helpers;
using NomadLogic.iBrandDesktop.Ui.ApplicationScreen.Generic.Helpers.ImageTransforms;
using NomadLogic.Multimedia;

namespace NomadLogic.iBrandDesktop.Ui.ApplicationScreen.FullscreenLoaders
{
    [PublicAPI]
    public class LoaderInf201801 : IFullscreenLoader
    {
        public void CreateScreens(TouchScreenController screensController, ScreenConfig sc)
        {
            var uiDir = DirectoryUtil.GetLocalIbrandTerminalDataDir(sc.ProjectId, sc.QuestionnaireId, sc.TerminalId,
                DirectoryUtil.ProjectDataDirectoriesEnum.photobooth_ui, DirectoryUtil.ProjectDataDirectoriesModEnum.none);
            var baseDir = DirectoryUtil.GetLocalIbrandTerminalDir(sc.ProjectId, sc.QuestionnaireId, sc.TerminalId);

            #region Écran de bienvenu
            var home = new HomeScreen(uiDir + "1. Choose you language.jpg", sc,
                new Rectangle(527, 486, 224, 70), // FR
                new Rectangle(527, 395, 224, 70)) // EN
            {
                QuestionLanguageNo = 1
            };
            screensController.AddScreen(home);
            #endregion

            #region Terms and conditions
            var terms = new SectionSkipScreen(uiDir + "2. Terms and conditions EN.jpg", uiDir + "2. Terms and conditions FR.jpg", sc,
                new Rectangle(437, 484, 188, 75)) // Cancel
            {
                ZoneList = new List<SectionSkipScreenData>()
                {
                    new SectionSkipScreenData()
                    {
                        SkipValidation = false,
                        Zone = new Rectangle(657, 484, 188, 75),
                        PageChangeOptions = ScreenChangeOptions.GenerateBasicNextOptions()
                    }
                }
            };
            screensController.AddScreen(terms);
            #endregion

            #region Choix de transformation
            var confirm = new BackgroundSelectionScreen(uiDir + "3. Choose your flavor EN.jpg", uiDir + "3. Choose your flavor FR.jpg", sc)
            {
                BackgroundSelectionOptions = new List<AnswerZone>()
                {
                    new AnswerZone()
                    {
                        PageChangeOptions = ScreenChangeOptions.GenerateBasicNextOptions(),
                        Zone = new Rectangle(346, 205, 237, 358),
                        AnswersUponSelection = new List<Answer>()
                        {
                            new Answer()
                            {
                                QuestionNo = 4,
                                Response = "109,93,161"
                            },
                            new Answer()
                            {
                                QuestionNo = 5,
                                Response = "135,200,209"
                            },
                            new Answer()
                            {
                                QuestionNo = 6,
                                Response = "tint_1.jpg"
                            }
                        }
                    },
                    new AnswerZone()
                    {
                        PageChangeOptions = ScreenChangeOptions.GenerateBasicNextOptions(),
                        Zone = new Rectangle(697, 205, 237, 358),
                        AnswersUponSelection = new List<Answer>()
                        {
                            new Answer()
                            {
                                QuestionNo = 4,
                                Response = "208,78,40"
                            },
                            new Answer()
                            {
                                QuestionNo = 5,
                                Response = "148,190,87"
                            },
                            new Answer()
                            {
                                QuestionNo = 6,
                                Response = "tint_2.jpg"
                            }
                        }
                    }
                }
            };
            screensController.AddScreen(confirm);
            #endregion

            #region Écrans de prise photo
            var fromTop = 15;
            var w = 310;
            var h = 465;

            var countdownPoint = new Point(840, 180);
            var generators = new List<IAnimatedImageSequenceGenerator>
            {
                new AnimatedImageSequenceGeneratorNumbers() {
                    ProceduralImageTextInfo = new ProceduralImageTextInfo
                    {
                        BitmapSize = new Size(250, 250),
                        Color = Color.FromArgb(12,134,185),
                        FontFamilly = FontFamily.GenericSerif.ToString(),
                        FontSize = 200,
                        FontStyle = FontStyle.Bold,
                        FontGraphicUnit = GraphicsUnit.Pixel
                    },
                    MaxValue = 5,
                    ReverseOrder = true
                },
                new AnimatedImageSequenceGeneratorBasicFiles
                {
                    FilePaths = new List<string> { uiDir + "vide.png" }
                }
            };
            var manualStart = new Rectangle(375, 500, 531, 75);
            
            var freezeBmp = new ImageGeneratorBasicFile()
            {
                FilePath = uiDir + "sablier.png"
            };

            var creatingSequence = new AnimatedImageSequence()
            {
                DrawLocation = new Point(379, 498),
                ImageGenerators = new List<IAnimatedImageSequenceGenerator>() { 
                    new AnimatedImageSequenceGeneratorBasicFiles()
                    {
                        FilePathsLang = new Dictionary<string, IList<string>>() 
                        {
                            {"en", new List<string>() { uiDir + "please wait.png" } },
                            {"fr", new List<string>() { uiDir + "veuillez attendre.png" } }
                        }
                    }
                }
            };

            // Photo 1
            var takePicture1 = new TakePictureAutoScreen(sc, uiDir + "4. Take photo 1 EN.jpg", uiDir + "4. Take photo 1 FR.jpg", 
                new Rectangle(-1, fromTop, w, h) // Preview
                )
            {
                ManualStartZone = manualStart,
                SaveOriginalImage = new DataModelQuestionnaire {Identifier = new QuestionIdentifier(2)},
                FreezeBitmapDefinition = freezeBmp,
                CountImageSequence = new AnimatedImageSequence
                {
                    Loop = false,
                    DrawLocation = countdownPoint,
                    ImageGenerators = generators
                }
            };
            screensController.AddScreen(takePicture1);

            // Photo 2
            var takePicture2 = new TakePictureAutoScreen(sc, uiDir + "5. Take photo 2 EN.jpg", uiDir + "5. Take photo 2 FR.jpg",
                new Rectangle(-1, fromTop, w, h) // Preview
            )
            {
                ManualStartZone = manualStart,
                SaveOriginalImage = new DataModelQuestionnaire {Identifier = new QuestionIdentifier(3)},
                FreezeBitmapDefinition = freezeBmp,
                CreatingImageSequence = creatingSequence,
                CountImageSequence = new AnimatedImageSequence
                {
                    Loop = false,
                    DrawLocation = countdownPoint,
                    ImageGenerators = generators
                },
                ImageListTranformOptions = new ImageListTransformOptions()
                {
                    ImageMagickOptions = new List<ImageListTransformOptionsImageMagick>() { 
                        new ImageListTransformOptionsImageMagick
                        {
                            CommandFile = "duotone_im_cli.txt",
                            ResultMapSaveModels = new Dictionary<string, DataModelBase>
                            {
                                {"1", new DataModelQuestionnaire {Identifier = new QuestionIdentifier(8)}}
                            }
                        }
                    }
                }
            };
            screensController.AddScreen(takePicture2);
            #endregion

            #region Écran de Preview

            var preview = new PreviewScreen(uiDir + "6. Preview composite photo EN.jpg", uiDir + "6. Preview composite photo FR.jpg", sc, null, 
                new Rectangle(885, 261, 304, 75), // Next
                new Rectangle(92, 261, 304, 75), // Back
                15, new Size(350, 525))
            {
                BackPageOptions = ScreenChangeOptions.GenerateBasicBackOptions(-2)
            };
            screensController.AddScreen(preview);

            #endregion

            #region Écran questionnaire

            var palmbayQuestionnaire = new PalmBayQuestionnaireScreen(uiDir + "7. Email and SMS sharing EN.jpg", uiDir + "7. Email and SMS sharing FR.jpg", sc)
            {
                CheckBoxZone = new List<ScreenCheckOption>()
                {
                    new ScreenCheckOption(new Rectangle(325, 233, 728, 27), 11, 1, false),
                    new ScreenCheckOption(new Rectangle(325, 366, 728, 27), 14, 1, false),
                },
                ZoneList = new List<SectionSkipScreenData>()
                {
                    new SectionSkipScreenData()
                    {
                        PageChangeOptions = ScreenChangeOptions.GenerateBasicNextOptions(),
                        Zone = new Rectangle(539, 711, 200, 74)
                    }
                }
            };
            screensController.AddScreen(palmbayQuestionnaire);

            #endregion

            #region Écran de remerciement
            var thxSCreen = new ThanksEndScreen(uiDir + "8. Thank you EN.jpg", sc, uiDir + "8. Thank you FR.jpg")
            {
                InactivityTimerInterval = 5 * 1000
            };
            screensController.AddScreen(thxSCreen);
            #endregion
        
        }
    }
}