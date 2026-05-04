using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Text;
using System.IO;
using System.Threading;
using NLog;
using NomadLogic.Common;
using NomadLogic.Common.Respondent;
using NomadLogic.Common.Services;
using NomadLogic.Common.Util;
using NomadLogic.iBrandDesktop.Services;
using NomadLogic.iBrandDesktop.Ui.ApplicationScreen.Generic.Helpers;
using NomadLogic.iBrandDesktop.Ui.ApplicationScreen.Generic.Helpers.ImageTransforms;
using NomadLogic.iBrandDesktop.Util.SubmitAction.BaseClasses;
using NomadLogic.Multimedia;

namespace NomadLogic.iBrandDesktop.Util.SubmitAction
{
    /// <summary>
    /// Fait une transformation blue-screen pour créer une image de front, puis créer une autre image pour le dos d'une carte de hockey.
    /// On leur ajoute également du texte avec un font particulier.
    /// Une fois fini, on lance l'impression, puis on termine.
    /// </summary>
    public class CossetteLaRondeSubmitAction : AbstractSubmitAction
    {
        // ReSharper disable once UnusedMember.Local
        private static readonly Logger log = LogManager.GetCurrentClassLogger();

        protected override void InstanciateSubmitActionPools()
        {
            AddSubmitActionWorkerPool("transform", 1);
        }
        
        protected override ISubmitActionWorker CreateWorker(Respondent resp, bool usePool = true)
        {
            return new CossetteLaRondeSubmitActionWorker(this, resp, resp.Id);
        }
        
        public PrivateFontCollection CustomFontCollection { get; } = new PrivateFontCollection();

        /// <summary>
        /// Créer le GIF pour cette action.
        /// </summary>
        /// <param name="fontFile">Chemin complet vers le fichier de police à utiliser.</param>
        /// <param name="gifFile">Nom du fichier final du GIF, avec chemin complet.</param>
        /// <param name="overlayList">Liste des fichiers d'overlay, avec chemin complet.</param>
        /// <param name="workDir">Répertoire de travail pour les fichiers temporaires.</param>
        /// <param name="respondent">Le répondant qui contient les réponses d'où extraire les données.</param>
        public void MakeCossetteGif(string fontFile, string gifFile, List<string> overlayList, string workDir, IRespondent respondent)
        {
            if (!Directory.Exists(workDir))
            {
                Directory.CreateDirectory(workDir);
            }

            var tmpFiles = new List<string>();

            // 1- Création des paramètres de transformation

            // NOTE: Le fichier d'overlay sera mis dans cette structure entre chaque invocation.
            var options = new ImageListTransformOptionsFirstPicture
            {
                JpegSaveQuality = 90,
                CanvasSize = new Size(534, 800),
                CanvasBaseColor = Color.Black,
                NewPos = new Point(70, 190),
                NewSize = new Size(400, 267),
                NewRotateFlip = RotateFlipType.Rotate180FlipNone,
                TextCase = 1,
                QuestionNoToFetchText = 7,
                TextCentered = true,
                TextPos = new Point(265, 130),
                OverlayPos = new Point(0, 0),
                FontDefinition = new FontDefinitionInfo
                {
                    Color = Color.White,
                    FamilyName = "Louis",
                    Size = 15,
                    Style = FontStyle.Italic.ToString(),
                    FontFile = fontFile
                }
            };

            // 2- Création des frames du GIF
            var mainPicture = respondent.GetAnswerText(1);
            foreach (var overFile in overlayList)
            {
                options.OverlaySelector = new ImageSelectorSingle(overFile); // MAJ du fichier d'overlay

                var tmp = workDir + GenerateUniqueId.GetId() + ".jpg";
                //ImageListTransform.CreateFirstPicture(respondent, new List<string> { mainPicture }, tmp, options);
                tmpFiles.Add(tmp);
            }

            // 3- Création du GIF

            if (!ImageUtil.CreateGif(tmpFiles, gifFile, true, false, 1, 400, null, null, false, null))
            {
                throw new System.Exception("Gif file was not generated.");
            }
        }

        private class CossetteLaRondeSubmitActionWorker : ISubmitActionWorker
        {
            private static readonly Logger logWorker = LogManager.GetCurrentClassLogger();
            private readonly Respondent _respondent;
            private readonly CossetteLaRondeSubmitAction _action;
            private readonly int _respId;

            public CossetteLaRondeSubmitActionWorker(CossetteLaRondeSubmitAction action, Respondent resp, int respId)
            {
                _respondent = resp;
                _action = action;
                _respId = respId;
            }

            public void DoWork(CancellationToken? ct)
            {
                if (ct?.IsCancellationRequested ?? false) return;
                
                var db = ServicesContainer.GetService<IbrandDatabase>();

                try
                {
                    var pId = _action.ProjectId;
                    var qId = _action.QuestionnaireId;

                    var baseDir = _action.GetTerminalDirectory;

                    var langResp = int.Parse(_respondent.GetAnswerText(14)); // 1 = anglais, 2 = français
                    var langMod = langResp == 2 ? "" : "_en";

                    var gifFile = DirectoryUtil.GetLocalIbrandTerminalDataDir(_action.TerminalInfo,
                        DirectoryUtil.ProjectDataDirectoriesEnum.pictures,
                        DirectoryUtil.ProjectDataDirectoriesModEnum.gif) + GenerateUniqueId.GetId() + ".gif";

                    var overlayList = new List<string>
                    {
                        baseDir + $"GIF_1{langMod}.png",
                        baseDir + $"GIF_2{langMod}.png",
                        baseDir + $"GIF_3{langMod}.png",
                        baseDir + $"GIF_4{langMod}.png",
                        baseDir + $"GIF_5{langMod}.png",
                        baseDir + $"GIF_6{langMod}.png"
                    };

                    var fontFile = baseDir + "LOUIS-BOLDITALIC.OTF";

                    var workDir = DirectoryUtil.GetLocalApplicationWorkDir() + GenerateUniqueId.GetId() + Path.DirectorySeparatorChar;
                    
                    _action.MakeCossetteGif(fontFile, gifFile, overlayList, workDir, _respondent);

                    // Sauvegarde du fichier final dans la question.
                    var ansPrint = new Answer {QuestionType = QuestionTypeEnum.PhotoText, QuestionNo = 2, ChoiceNo = 1, Response = gifFile};
                    db.DeleteAnswer(_respId, ansPrint);
                    db.InsertAnswer(_respId, ansPrint);

                    // 5- On finalise le répondant
                    db.SetRespondentReadyToSync(_respId);
                    _action.UiSynchro?.AddUiMessage("Respondent La Ronde GIF action finished.", 4);

                }
                catch (System.Exception ex)
                {
                    logWorker.Fatal(ex, "Error while executing treatment of respondent.");
                    _action.UiSynchro?.AddUiMessage("Error while treating respondent.", 4);
                }
            }

            

            public ISubmitActionWorker NextWorkerStep { get; set; }

            public ISubmitAction GetSubmitAction()
            {
                return _action;
            }

            public int GetRespId()
            {
                return _respId;
            }
        }
    }
}