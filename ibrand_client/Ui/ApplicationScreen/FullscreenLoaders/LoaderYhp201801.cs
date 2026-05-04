using System.Drawing;
using JetBrains.Annotations;
using NomadLogic.Common.Util;
using NomadLogic.iBrandDesktop.Ui.ApplicationScreen.custom.Maizex;
using NomadLogic.iBrandDesktop.Ui.ApplicationScreen.Generic;
using NomadLogic.iBrandDesktop.Ui.ApplicationScreen.Generic.Helpers;

namespace NomadLogic.iBrandDesktop.Ui.ApplicationScreen.FullscreenLoaders
{
    [PublicAPI]
    public class LoaderYhp201801 : IFullscreenLoader
    {
        public void CreateScreens(TouchScreenController screensController, ScreenConfig sc)
        {
            var uiDir = DirectoryUtil.GetLocalIbrandTerminalDataDir(sc.ProjectId, sc.QuestionnaireId, sc.TerminalId,
                DirectoryUtil.ProjectDataDirectoriesEnum.photobooth_ui, DirectoryUtil.ProjectDataDirectoriesModEnum.none);
            var baseDir = DirectoryUtil.GetLocalIbrandTerminalDir(sc.ProjectId, sc.QuestionnaireId, sc.TerminalId);

            var home = new HomeScreen(uiDir + "1. APPUYEZ POUR COMMENCER.jpg", sc,
                new Rectangle(472, 400, 338, 124))
            {
            };
            screensController.AddScreen(home);

            // Écran 2 - Prise vidéo
            var recordVideoScreen = new RecordVideoSonyScreen(uiDir + "2. PRISE VIDEO.jpg", uiDir + "2. PRISE VIDEO.jpg", sc,
                new Rectangle(1042, 314, 205, 89), // Start
                new Rectangle(1042, 484, 203, 89), // Stop
                null, // Cancel


                new Size(749, 424), 233)
            {
                // DEBUG SEULEMENT
                //DebugMovieFile = @"C:\Users\Nomad Logic Inc\AppData\local\Nomad Logic\original_green_120fps.MP4"
            };
            screensController.AddScreen(recordVideoScreen);

            // Écran 3 - Édition
            var previewScreen = new PreviewAndCutVideoScreen(uiDir + "3. ÉDITION VIDEO.jpg", uiDir + "3. ÉDITION VIDEO.jpg", sc,
                null, // cancel
                new Rectangle(1044, 469, 203, 117), // next
                new Rectangle(1044, 302, 204, 119), // back
                new Rectangle(37, 318, 205, 89), // play
                new Rectangle(38, 486, 203, 85), // pause
                new Rectangle(266, 672, 748, 34), // Bar Zone 
                235, new Size(750, 419), true)
            {
                CurrentTimePosition = new Point(130, 675),
                TotalTimePosition = new Point(1050, 675),
                SaveInactiveDrawPoint = new Point(1044, 468),
                DisableTimer = true,
                //DebugVideoProbeFile = baseDir + "debug_video_metadata.txt",
                NextPageOptions = ScreenChangeOptions.GenerateBasicNextOptions(1)


            };
            screensController.AddScreen(previewScreen);

            var questionnaireScreen = new MaizexQuestionnaireScreen(uiDir + "4. INTERNET BREAKDOWN DATA.jpg",
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