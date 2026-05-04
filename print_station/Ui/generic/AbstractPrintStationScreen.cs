using System;
using System.Drawing;
using System.IO;
using NomadLogic.Common.Respondent;
using NomadLogic.Common.Util;
using NomadLogic.iBrandDesktop;
using NomadLogic.iBrandDesktop.Ui.ApplicationScreen;

namespace NomadLogic.PrintStation.Ui.generic
{
    /// <summary>
    /// Extensions de la classe de configuration de base pour y ajouter les informations critique pour un projet PrintStation.
    /// </summary>
    public abstract class AbstractPrintStationScreen : AbstractScreen, IAbstractPrintStationScreen
    {
        // ReSharper disable once UnusedMember.Local
        private static readonly NLog.Logger log = NLog.LogManager.GetCurrentClassLogger();

        /// <summary>
        /// Constructeur principal.
        /// </summary>
        /// <param name="screenConf">La configuration d'écran</param>
        /// <param name="size">La taille de l'écran.</param>
        protected AbstractPrintStationScreen(PrintStationScreenConfig screenConf, Size size) : base(size, screenConf) { }

        /// <summary>
        /// Constructeur minimal. On ne spécifie que la taille du contrôle.
        /// <br/>Il faut absolument spécifier les autres paramètres, car l'écran est inutilisable comme tel après ce constructeur.
        /// </summary>
        /// <param name="size">LA taille X,Y du contrôle, en pixels.</param>
        protected AbstractPrintStationScreen(Size size) : base(size) { }

        /// <summary>
        /// </summary>
        /// <param name="backgroundImgEnSrc">L'image de fond en anglais.</param>
        /// <param name="backgroundImgFrSrc">L'image de fond en français.</param>
        /// <param name="screenConf">Classe de configuration du projet pour écran.</param>
        protected AbstractPrintStationScreen(string backgroundImgEnSrc, string backgroundImgFrSrc, PrintStationScreenConfig screenConf)
            : base(backgroundImgEnSrc, backgroundImgFrSrc, screenConf)
        { }

        #region Image Getters

        /// <summary>
        /// Va chercher un fichier d'image selon les chemins d'un PrintStation pour un usager spécifique, avec le code de langue par défaut.
        /// <br/>Lance une erreur si on ne trouve rien.
        /// </summary>
        /// <param name="fileName">Le nom de fichier à chercher.</param>
        /// <returns>Le fichier d'image, si trouvé.</returns>
        protected new Bitmap GetBitmap(string fileName)
        {
            return GetBitmap(fileName, _activeLg);
        }

        /// <summary>
        /// Va chercher un fichier d'image selon les chemins d'un PrintStation pour un usager spécifique, avec le code de langue par défaut.
        /// <br/>Lance une erreur si on ne trouve rien.
        /// </summary>
        /// <param name="fileName">Le nom de fichier à chercher.</param>
        /// <param name="lg">Le code de langue précis.</param>
        /// <returns>Le fichier d'image, si trouvé.</returns>
        protected new Bitmap GetBitmap(string fileName, string lg)
        {
            return GetBitmapDoPrintstation(fileName, Path.DirectorySeparatorChar + lg + Path.DirectorySeparatorChar);
        }
        
        /// <summary>
        /// Va chercher un fichier d'image selon les chemins d'un PrintStation pour un usager spécifique dans le répertoire
        /// agnostique à la langue.
        /// <br/>Lance une erreur si on ne trouve rien.
        /// </summary>
        /// <param name="fileName">Le nom de fichier à chercher.</param>
        /// <returns>Le fichier d'image, si trouvé.</returns>
        protected new Bitmap GetNoneLanguageBitmap(string fileName)
        {
            return GetBitmapDoPrintstation(fileName, "");
        }

        /// <summary>
        /// Tente de chercher une image sans langue associé dans le répertoire ui d'un Photobooth (projId/questId/photobooth_ui/).
        /// Si ne trouve rien, va chercher l'image comme normal, via "GetNoneLanguageBitmap(string)".
        /// </summary>
        /// <param name="filename">Le nom de fichier à chercher.</param>
        /// <param name="projectId">L'identifiant du projet iBrand.</param>
        /// <param name="questId">L'identifiant du questionnaire iBrand.</param>
        /// <returns>Une référence evers le fichier, si trouvé.</returns>
        protected Bitmap GetNoneLanguageBitmapFromIbrand(string filename, int projectId, int questId)
        {
            var path = GetTerminalDirectory() + filename;
            var bitmap = FileUtil.LoadBitmap(path);

            return bitmap ?? GetBitmapDoPrintstation(filename, "");
        }

        private Bitmap GetBitmapDoPrintstation(string fileName, string subDir)
        {
            var featureId = ((PrintStationScreenConfig)_screenConfig).PrintStationUser.FeatureId;
            var path = DirectoryUtil.GetLocalPrintstationMediaDir(featureId, DirectoryUtil.NomadLogicPrintstationDirectoriesEnum.ui)
                + subDir + fileName;
            var bitmap = FileUtil.LoadBitmap(path);

            //On essaie avec le repertoire du projet iBrand
            // ReSharper disable once ConvertIfStatementToNullCoalescingExpression
            if (bitmap == null)
            {
                bitmap = GetBitmapDo(null, fileName);
            }

            if (bitmap == null)
            {
                path = DirectoryUtil.GetPrintstationUiDefaultAssetsDir() + fileName;
                bitmap = FileUtil.LoadBitmap(path);
            }
            
            if (_screenConfig.ForceFileCheck && bitmap == null)
            {
                throw new FileNotFoundException("Could not find image file in user and default assests: " + fileName);
            }

            return bitmap;
        }

        #endregion ImageGetters

        /// <summary>
        /// Va chercher le questionnaire avec le chemin propre à un questionnaire de PrintStation
        /// </summary>
        /// <returns>Le questionnaire actuel du PrintStation</returns>
        public override Questionnaire GetCurrentQuestionnaire()
        {
            var tmp = MainForm.GetInstance();
            if(tmp == null)
            {
                log.Fatal("MainForm instance is null");
                return null;
            }
            return tmp.Questionnaire;
        }
        
        public override IRespondent GetCurrentRespondentWithAnswers()
        {
            var resp = CurrentRespondentSource?.GetValidatedCurrentRespondent();
            if (resp == null)
            {
                throw new Exception("No respondent source has been given to this printstation screen instance. Unlike iBrand screens, you must manually specify the current respondent source.");
            }
            return resp;
        }

        /// Manual specification of the current respondent source. Usually, you use the QuestionnaireController.
        public ICurrentRespondentSource CurrentRespondentSource { get; set; }

        /// <summary>
        /// Va directement cherche l'instance d'image qui a été sélectionnée par l'usager avant de lancer le questionnaire.
        /// </summary>
        /// <returns></returns>
        public PicturePostInfo GetSelectedPicture()
        {
            return MainForm.GetInstance().SelectedPicture;
        }

        /// <summary>
        /// Quand une forme PrintStation termine son timer, on ferme cette dernière, elle ne revient pas au premier écran.
        /// </summary>
        /// <param name="sender">L'envoyeur.</param>
        /// <param name="e">Paramètre d'événement.</param>
        protected override void OnTimerEvent(object sender, EventArgs e)
        {
            Close();
        }

        /// <summary>
        /// Étends les écran de printstation pour ne pas que ces derniers utiliser le pop-up de configuration de iBrand.
        /// </summary>
        /// <param name="x"></param>
        /// <param name="y"></param>
        /// <returns></returns>
        protected override bool OnMouseDownHandler(int x, int y)
        {
            if (_flagForClosing) return false;
            if (!_activated) return false;
            return true;
        }

        public override void FirstScreen()
        {
            if (!_activated) return;
            _activated = false;
            Close();
        }
    }
}