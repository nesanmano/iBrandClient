using System.Drawing;
using System.IO;
using NomadLogic.Common.Respondent;
using NomadLogic.Common.Util;
using NomadLogic.iBrandDesktop.Ui.ApplicationScreen;
using NomadLogic.iBrandDesktop.Ui.ApplicationScreen.Generic;

namespace NomadLogic.PrintStation.Ui
{
    /// <summary>
    /// Classe analogue à son père, mais qui surcharge les méthodes de recherche de fichier pour se conformer à la structure du PrintStation.
    /// </summary>
    public class WebBrowserScreenPrintStation : WebBrowserScreen, IAbstractPrintStationScreen
    {
        public WebBrowserScreenPrintStation(ScreenConfig sc, Size screensize) : base(sc, screensize)
        {
        }

        public WebBrowserScreenPrintStation(ScreenConfig screenConfig, Size screensize, Size keyboardsize) : base(screenConfig, screensize, keyboardsize)
        {
        }

        /// <summary>
        /// Va chercher un fichier d'image selon les chemins d'un PrintStation pour un usager spécifique dans le répertoire
        /// agnostique à la langue.
        /// <br/>Lance une erreur si on ne trouve rien.
        /// </summary>
        /// <param name="fileName">Le nom de fichier à chercher.</param>
        /// <returns>Le fichier d'image, si trouvé.</returns>
        protected override Bitmap GetNoneLanguageBitmap(string fileName)
        {
            return GetBitmapDoPrintstation(fileName, "");
        }

        private Bitmap GetBitmapDoPrintstation(string fileName, string subDir)
        {
            var userId = ((PrintStationScreenConfig)_screenConfig).PrintStationUser.Id;
            var path = DirectoryUtil.GetLocalPrintstationMediaDir(userId, DirectoryUtil.NomadLogicPrintstationDirectoriesEnum.ui)
                + subDir + fileName;
            var bitmap = FileUtil.LoadBitmap(path);

            if (bitmap == null)
            {
                path = DirectoryUtil.GetLocalNomadLogicAssetsDir() + fileName;
                bitmap = FileUtil.LoadBitmap(path);
            }

            if (_screenConfig.ForceFileCheck && bitmap == null)
            {
                throw new FileNotFoundException("Could not find image file in user and default assests: " + fileName);
            }

            return bitmap;
        }

        public PicturePostInfo GetSelectedPicture()
        {
            return MainForm.GetInstance().SelectedPicture;
        }

        public ICurrentRespondentSource CurrentRespondentSource { get; set; }
    }
}