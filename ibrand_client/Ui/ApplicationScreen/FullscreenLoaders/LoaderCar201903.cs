using System.Collections.Generic;
using System.Drawing;
using System.IO;
using JetBrains.Annotations;
using NomadLogic.Common.Util;
using NomadLogic.iBrandDesktop.Ui.ApplicationScreen.custom.Cartel;
using NomadLogic.iBrandDesktop.Ui.ApplicationScreen.Generic;
using NomadLogic.iBrandDesktop.Ui.ApplicationScreen.Generic.Helpers;
using NomadLogic.Multimedia;

namespace NomadLogic.iBrandDesktop.Ui.ApplicationScreen.FullscreenLoaders
{
    [PublicAPI]
    public class LoaderCar201903 : IFullscreenLoader
    {
        public void CreateScreens(TouchScreenController screensController, ScreenConfig sc)
        {
            var uiDir = DirectoryUtil.GetLocalIbrandTerminalDataDir(sc.ProjectId, sc.QuestionnaireId, sc.TerminalId,
                DirectoryUtil.ProjectDataDirectoriesEnum.photobooth_ui, DirectoryUtil.ProjectDataDirectoriesModEnum.none);
            var baseDir = DirectoryUtil.GetLocalIbrandTerminalDir(sc.ProjectId, sc.QuestionnaireId, sc.TerminalId);

            var home = new HomeScreen(uiDir + "1. APPUYEZ POUR COMMENCER.jpg", sc,
                new Rectangle(0, 0, 1280, 800))
            {
                QuestionLanguageNo = 8
            };
            screensController.AddScreen(home);

            // Écran 2 - Prise vidéo
            var recordVideoScreen = new RecordVideoScreen(sc, uiDir + "2. PRISE VIDEO.jpg", uiDir + "2. PRISE VIDEO.jpg",
                new Rectangle(265, 233, 751, 422), // Preview
                1, 6, null,
                new Rectangle(1039, 313, 209, 94), // Start
                null, // Cancel
                new Rectangle(1039, 482, 209, 94), // Stop
                new Rectangle(574, 644, 209, 127), // Done
                false, new List<string>
                {
                    "start_enable.png",
                    "stop_enable.png",
                    "next_enable.png"
                })
            {
                ImageSequences = new List<AnimatedImageSequenceEnumTrigger<RecordVideoScreenStates>>
                {
                    new AnimatedImageSequenceEnumTrigger<RecordVideoScreenStates>
                    {
                        EnumFlags = new List<RecordVideoScreenStates>
                        {
                            RecordVideoScreenStates.Recording
                        },
                        ImageSequence = new AnimatedImageSequence
                        {
                            DrawLocation = new Point(85, 337),
                            ImageGenerators = new List<IAnimatedImageSequenceGenerator>
                            {
                                new AnimatedImageSequenceGeneratorBasicFiles
                                {
                                    FilePathsLang = new Dictionary<string, IList<string>>
                                    {
                                        {"en", new List<string>() { $"{uiDir}en{Path.DirectorySeparatorChar}recording_button.png"}},
                                        {"fr", new List<string>() { $"{uiDir}fr{Path.DirectorySeparatorChar}recording_button.png"}}
                                    }
                                }
                            }
                        }
                    },
                    new AnimatedImageSequenceEnumTrigger<RecordVideoScreenStates>
                    {
                        EnumFlags = new List<RecordVideoScreenStates>
                        {
                            RecordVideoScreenStates.Transfering
                        },
                        ImageSequence = new AnimatedImageSequence
                        {
                            DrawLocation = new Point(85, 644),
                            ImageGenerators = new List<IAnimatedImageSequenceGenerator>
                            {
                                new AnimatedImageSequenceGeneratorFromText()
                                {
                                    ProceduralImageTextInfo = new ProceduralImageTextInfo
                                    {
                                        BitmapSize = new Size(438, 127),
                                        FontSize = 40,
                                        Color = Color.White
                                    },
                                    TextsLang = new Dictionary<string, IList<string>>
                                    {
                                        {"en", new List<string>()
                                        {
                                            "Transfering",
                                            "Transfering.",
                                            "Transfering..",
                                            "Transfering..."
                                        }},
                                        {"fr", new List<string>()
                                        {
                                            "Transfert",
                                            "Transfert.",
                                            "Transfert..",
                                            "Transfert..."
                                        }}
                                    }
                                }
                            }
                        }
                    }
                }
            };
            screensController.AddScreen(recordVideoScreen);

            // Écran 3 - Édition
            var previewScreen = new PreviewAndCutVideoScreen(uiDir + "3. EDITION VIDEO.jpg", uiDir + "3. EDITION VIDEO.jpg", sc,
                null, // cancel
                new Rectangle(1039, 465, 209, 127), // next
                new Rectangle(1039, 296, 209, 129), // back
                new Rectangle(38, 318, 201, 86), // play
                new Rectangle(38, 485, 201, 86), // pause
                new Rectangle(266, 672, 748, 36), // Bar Zone 
                235, new Size(750, 419), true)
            {
                CurrentTimePosition = new Point(130, 675),
                TotalTimePosition = new Point(1050, 675),
                SaveInactiveDrawPoint = new Point(1044, 468),
                DisableTimer = true,
                NextPageOptions = ScreenChangeOptions.GenerateBasicNextOptions(1)
            };
            screensController.AddScreen(previewScreen);

            var questionnaireScreen = new QuestionnaireScreen_CAR201903A(uiDir + "4. INTERNET BREAKDOWN DATA.jpg",
                uiDir + "4. INTERNET BREAKDOWN DATA.jpg", sc)
            {
            };
            screensController.AddScreen(questionnaireScreen);

            /* Ecran 4 : Merci */
            var thxScreen = new ThanksEndScreen(uiDir + "5. MERCI!.jpg", sc)
            {
                InactivityTimerInterval = 5 * 1000
            };
            screensController.AddScreen(thxScreen);
        }
    }
}