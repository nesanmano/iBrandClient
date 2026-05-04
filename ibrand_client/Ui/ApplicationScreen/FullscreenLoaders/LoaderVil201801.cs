using System.Collections.Generic;
using System.Drawing;
using JetBrains.Annotations;
using NomadLogic.Common.Util;
using NomadLogic.iBrandDesktop.Ui.ApplicationScreen.custom.VillageNoel;
using NomadLogic.iBrandDesktop.Ui.ApplicationScreen.Generic;
using NomadLogic.iBrandDesktop.Ui.ApplicationScreen.Generic.Helpers;
using NomadLogic.Multimedia;

namespace NomadLogic.iBrandDesktop.Ui.ApplicationScreen.FullscreenLoaders
{
    [PublicAPI]
    public class LoaderVil201801 : IFullscreenLoader
    {
        public void CreateScreens(TouchScreenController screensController, ScreenConfig sc)
        {
            var uiDir = DirectoryUtil.GetLocalIbrandTerminalDataDir(sc.ProjectId, sc.QuestionnaireId, sc.TerminalId,
                DirectoryUtil.ProjectDataDirectoriesEnum.photobooth_ui, DirectoryUtil.ProjectDataDirectoriesModEnum.none);
            var baseDir = DirectoryUtil.GetLocalIbrandTerminalDir(sc.ProjectId, sc.QuestionnaireId, sc.TerminalId);

            // Écran 1 - Bienvenue / Choix langue
            var start = new HomeScreen(uiDir + "1. start.jpg", sc, 
                new Rectangle(0,0,800, 900), // FR
                new Rectangle(800, 0, 800, 900) // EN
                )
            {
                QuestionLanguageNo = 7,
                QuestionLanguageChoiceNoEn = 1,
                QuestionLanguageChoiceNoFr = 2
            };
            screensController.AddScreen(start);

            // Écran 2 - Questionnaire
            var quest = new VillageQuestionnaireScreen(uiDir + "2. Saisi ANG.jpg", uiDir + "2. Saisi FR.jpg", sc)
            {
                ZoneList = new List<SectionSkipScreenData>()
                {
                    new SectionSkipScreenData()
                    {
                        PageChangeOptions = new ScreenChangeOptions()
                        {
                            Options = new List<ScreenChangeOption>()
                            {
                                new ScreenChangeOption()
                                {
                                    ValidationTrigger = true,
                                    Step = 1,
                                }
                            }
                        },
                        Zone = new Rectangle(1247, 822, 204, 65)
                    },
                    new SectionSkipScreenData()
                    {
                        SkipValidation = true,
                        PageChangeOptions = new ScreenChangeOptions()
                        {
                            Options = new List<ScreenChangeOption>()
                            {
                                new ScreenChangeOption()
                                {
                                    ValidationTrigger = false,
                                    Cancel = true
                                }
                            }
                        },
                        Zone = new Rectangle(147, 822, 204, 65)
                    },
                    new SectionSkipScreenData()
                    {
                        SkipValidation = true,
                        PageChangeOptions = new ScreenChangeOptions()
                        {
                            Options = new List<ScreenChangeOption>()
                            {
                                new ScreenChangeOption()
                                {
                                    ValidationTrigger = false,
                                    Cancel = true
                                }
                            }
                        },
                        Zone = new Rectangle(1512, 8, 80, 80)
                    }
                }
            };
            screensController.AddScreen(quest);

            var fontText = new ProceduralImageTextInfo()
            {
                Color = Color.White,
                Center = true,
                BitmapSize = new Size(874, 122),
                FontSize = 70
            };
            var drawLocation = new Point(366, 324);
            // Écran 3 - Prise vidéo
            var recordVideoScreen = new RecordVideoScreen(sc, uiDir + "3. Prise video ANG.jpg", uiDir + "3. Prise video FR.jpg",
                new Rectangle(336, 128, 929, 519), // Preview
                10, 40,
                null, // BMF
                new Rectangle(30, 324, 269, 117), // Start
                null, // cancel
                new Rectangle(1301, 329, 269, 117), // stop
                new Rectangle(704, 825, 195, 58), // next
                false, null)
            {
                ForceSaveVideoToQuestionNo = 6,
                UseCountdownDecorator = false,
                PreviewImageRotation = RotateFlipType.RotateNoneFlipNone,
                PreviewOnStates = new List<RecordVideoScreenStates>()
                {
                    RecordVideoScreenStates.StandBy,
                    RecordVideoScreenStates.PreRecording
                },
                StartVideoDuringCountdown = new VlcVideoCountdownInfo()
                {
                    StartTime = 5,
                    FilesToPlay = new Dictionary<string, string>()
                    {
                        {"en", "presentation_en_AVEC TRAME SONORE.mp4" },
                        {"fr", "presentation_fr_AVEC TRAME SONORE.mp4" }
                    }
                },
                ImageSequences = new List<AnimatedImageSequenceEnumTrigger<RecordVideoScreenStates>>()
                {
                    new AnimatedImageSequenceEnumTrigger<RecordVideoScreenStates>()
                    {
                        EnumFlags = new List<RecordVideoScreenStates>()
                        {
                            RecordVideoScreenStates.Transfering
                        },
                        ImageSequence = new AnimatedImageSequence()
                        {
                            Loop = true,
                            DrawLocation = drawLocation,
                            ImageGenerators = new List<IAnimatedImageSequenceGenerator>()
                            {
                                new AnimatedImageSequenceGeneratorFromText()
                                {
                                    TextsLang = new Dictionary<string, IList<string>>()
                                    {
                                        {"en", new List<string>() { "Transfering", "Transfering.", "Transfering..", "Transfering..." } },
                                        {"fr", new List<string>() { "Transfert", "Transfert.", "Transfert..", "Transfert..." } }
                                    },
                                    ProceduralImageTextInfo = fontText
                                }
                            }
                        }
                    },
                    new AnimatedImageSequenceEnumTrigger<RecordVideoScreenStates>()
                    {
                        EnumFlags = new List<RecordVideoScreenStates>()
                        {
                            RecordVideoScreenStates.Recording
                        },
                        ImageSequence = new AnimatedImageSequence()
                        {
                            Loop = true,
                            DrawLocation = drawLocation,
                            ImageGenerators = new List<IAnimatedImageSequenceGenerator>()
                            {
                                new AnimatedImageSequenceGeneratorFromText()
                                {
                                    TextsLang = new Dictionary<string, IList<string>>()
                                    {
                                        {"en", new List<string>() { "Recording", "Recording.", "Recording..", "Recording..." } },
                                        {"fr", new List<string>() { "Enregistrement", "Enregistrement.", "Enregistrement..", "Enregistrement..." } }
                                    },
                                    ProceduralImageTextInfo = fontText
                                }
                            }
                        }
                    },
                    new AnimatedImageSequenceEnumTrigger<RecordVideoScreenStates>()
                    {
                        EnumFlags = new List<RecordVideoScreenStates>()
                        {
                            RecordVideoScreenStates.PreRecording
                        },
                        ImageSequence = new AnimatedImageSequence()
                        {
                            Loop = false,
                            DrawLocation = new Point(333, 25),
                            ImageGenerators = new List<IAnimatedImageSequenceGenerator>()
                            {
                                new AnimatedImageSequenceGeneratorFromText()
                                {
                                    TextsLang = new Dictionary<string, IList<string>>()
                                    {
                                        {"en", new List<string>() { "Take a seat", "Take a seat.", "Take a seat.." } },
                                        {"fr", new List<string>() { "Prenez place", "Prenez place.", "Prenez place.." } }
                                    },
                                    ProceduralImageTextInfo = fontText
                                }
                            }
                        }
                    },
                    new AnimatedImageSequenceEnumTrigger<RecordVideoScreenStates>()
                    {
                        EnumFlags = new List<RecordVideoScreenStates>()
                        {
                            RecordVideoScreenStates.VideoFileReady
                        },
                        ImageSequence = new AnimatedImageSequence()
                        {
                            Loop = false,
                            DrawLocation = drawLocation,
                            ImageGenerators = new List<IAnimatedImageSequenceGenerator>()
                            {
                                new AnimatedImageSequenceGeneratorFromText()
                                {
                                    TextsLang = new Dictionary<string, IList<string>>()
                                    {
                                        {"en", new List<string>() { "Video file ready" } },
                                        {"fr", new List<string>() { "Vidéo prêt" } }
                                    },
                                    ProceduralImageTextInfo = fontText
                                }
                            }
                        }
                    }
                }
            };
            screensController.AddScreen(recordVideoScreen);
            
            /* Ecran 4 : Merci */
            var thxScreen = new ThanksEndScreen(uiDir + "4. Écran merci ANG.jpg", sc, uiDir + "4. Écran merci.jpg")
            {
                InactivityTimerInterval = 5 * 1000
            };
            screensController.AddScreen(thxScreen);

        }
    }
}