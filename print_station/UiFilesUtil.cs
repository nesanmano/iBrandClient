using System.IO;
using JetBrains.Annotations;
using NomadLogic.Common.Services;
using NomadLogic.Common.Util;
using NomadLogic.PrintStation.Services;
// ReSharper disable MissingXmlDoc

namespace NomadLogic.PrintStation
{
    /// <summary>
    /// Classe qui fournit des méthodes de recherche pour les fichiers UI d'un utilisateur précis, ou les fichiers par défaut
    /// du PrintStation.
    /// </summary>
    [PublicAPI]
    public static class UiFilesUtil
    {
        public static readonly string Logo = "logo";
        public static readonly string NextButton = "next_button";
        public static readonly string BackButton = "back_button";
        public static readonly string SearchButton = "search_button";
        public static readonly string SearchText = "search_text";
        public static readonly string ClearSearchButton = "clear_search_button";
        public static readonly string Background = "background";
        public static readonly string CloseButton = "close";
        public static readonly string PrintButton = "print";
        
        public static readonly string PrintButtonClicked = "print_clicked";
        public static readonly string PhotoFrame = "frame";
        public static readonly string PhotoFrameFr = "frame_fr";
        public static readonly string PhotoOverlay = "overlay";
        public static readonly string PhotoOverlayWinner = "overlay_winner";
        public static readonly string PopUpWinner = "pop_up_winner";
        public static readonly string ContestParametersFile = "contest_parameters_file";
        public static readonly string FtpSourceJson = "ftp_source";

        /* Les fichiers par défauts sont déjà initialisé avec leur chemin complet. */
        private static readonly string assetsDir = DirectoryUtil.GetLocalNomadLogicAssetsDir() + "printstation_ui" + Path.DirectorySeparatorChar;

        private static readonly string DefaultLogo = assetsDir + "logo.png";
        private static readonly string DefaultNextButton = assetsDir + "next.png";
        private static readonly string DefaultBackButton = assetsDir + "previous.png";
        private static readonly string DefaultSearchButton = assetsDir + "search.png";
        private static readonly string DefaultSearchText = assetsDir + "search_txt.png";
        private static readonly string DefaultClearSearchButton = assetsDir + "clear_search.png";
        private static readonly string DefaultCloseButton = assetsDir + "close.png";
        private static readonly string DefaultPrintButton = assetsDir + "print_button.png";
        private static readonly string DefaultPrintButtonClicked = assetsDir + "print_button_click.png";
        private static readonly string DefaultPhotoFrame = assetsDir + "photo_frame.png";
        private static readonly string DefaultPhotoFrameFr = assetsDir + "photo_frame.png";
        private static readonly string DefaultPopupWinner = assetsDir + "winner_dialog_background.png";

        /// <summary>
        /// Va chercher un fichier par type selon un usager précis. Si l'usager ne possède pas ce fichier, on chercher
        /// dans le répertoire de fichiers par défaut.
        /// </summary>
        /// <param name="type">Le type du fichier à chercher. Utiliser les constantes de cette classe.</param>
        /// <param name="userId">L'identifiant du feature visé.</param>
        /// <returns>Le chemin absolu du fichier trouvé. NULL sinon.</returns>
        public static string GetFileName(string type, int userId)
        {
            var fileName = GetFileNameWhitoutDefault(type, userId);

            return fileName ?? GetDefaultFilename(type);
        }

        /// <summary>
        /// Tente de trouver un fichier dans un répertoire d'utilisateur exclusivement.
        /// </summary>
        /// <param name="type">Le type du fichier à chercher. Utiliser les constantes de cette classe.</param>
        /// <param name="userId">L'identifiant de l'usager visé.</param>
        /// <returns>Le chemin absolu du fichier trouvé. NULL sinon.</returns>
        public static string GetFileNameWhitoutDefault(string type, int userId)
        {
            var database = ServicesContainer.GetService<PrintStationDatabase>();
            var uiFile = database.GetUiFile(type, userId);

            if (uiFile == null) return null;
            var fileName =
                DirectoryUtil.GetLocalPrintstationMediaDir(userId,
                    DirectoryUtil.NomadLogicPrintstationDirectoriesEnum.ui) + StringUtil.GetFileName(uiFile.Src);
            return File.Exists(fileName) ? fileName : null;
        }

        [PublicAPI]
        public static string GetDefaultFilename(string type)
        {
            if (type.Equals(PhotoFrame))
                return DefaultPhotoFrame;
            if (type.Equals(PhotoFrameFr))
                return DefaultPhotoFrameFr;
            if (type.Equals(Logo))
                return DefaultLogo;
            if (type.Equals(NextButton))
                return DefaultNextButton;
            if (type.Equals(BackButton))
                return DefaultBackButton;
            if (type.Equals(SearchButton))
                return DefaultSearchButton;
            if (type.Equals(SearchText))
                return DefaultSearchText;
            if (type.Equals(ClearSearchButton))
                return DefaultClearSearchButton;
            if (type.Equals(CloseButton))
                return DefaultCloseButton;
            if (type.Equals(PrintButton))
                return DefaultPrintButton;
            if (type.Equals(PrintButtonClicked))
                return DefaultPrintButtonClicked;
            if (type.Equals(PopUpWinner))
                return DefaultPopupWinner;

            return null;
        }
    }
}