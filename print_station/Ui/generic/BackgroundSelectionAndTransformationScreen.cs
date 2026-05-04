using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using NLog;
using NomadLogic.Common;
using NomadLogic.Common.Services;
using NomadLogic.Common.Ui;
using NomadLogic.Common.Util;
using NomadLogic.Common.Util.Print;
using NomadLogic.iBrandDesktop.Ui.ApplicationScreen.Generic.Helpers;
using NomadLogic.PrintStation.Services;

namespace NomadLogic.PrintStation.Ui.generic
{
    public struct TransformationSelectionOption
    {
        public TransformationSelectionOption(AnswerZone bso, string file)
        {
            BkgSelectOption = bso;
            FrameFile = file;
        }

        public AnswerZone BkgSelectOption { get; set; }
        public string FrameFile { get; set; }
    }

    /// <summary>
    /// Cette classe d'écran propose un nombre arbitraire de choix par clic. Quand on choisit un choix, la zone sauvegarde des réponses
    /// dans le questionnaire actif et effectue une tranformation sur l'image chosie. Utilisé pour choisir des Frame différents au choix de
    /// l'usager.
    /// On peut rendre l'impression inactive dans la cas où il y a des écrans passé celui-ci.
    /// </summary>
    // TODO Classe basée sur un écran similaire de photobooth.
    public class BackgroundSelectionAndTransformationScreen : AbstractPrintStationScreen
    {
        private static readonly Logger log = LogManager.GetCurrentClassLogger();

        private readonly List<TransformationSelectionOption> _zones = new List<TransformationSelectionOption>();
        private readonly Rectangle? _nextArea;
        private readonly Rectangle? _cancelArea;
        private string _chosenFrameFile;
        public bool HasAnswer { get; private set; }

        public BackgroundSelectionAndTransformationScreen(string backgroundImgEnSrc, PrintStationScreenConfig sc, 
            Rectangle? nextArea, Rectangle? cancelArea, 
            IEnumerable<TransformationSelectionOption> zones)
            : base(backgroundImgEnSrc, backgroundImgEnSrc, sc)
        {
            _zones.AddRange(zones);
            _cancelArea = cancelArea;
            _nextArea = nextArea;
        }

        /// <inheritdoc/>
        public override void Recycle()
        {
            base.Recycle();
            HasAnswer = false;
            _chosenFrameFile = null;
        }

        /// <inheritdoc/>
        protected override bool OnMouseDownHandler(int x, int y)
        {
            if (!base.OnMouseDownHandler(x, y)) return false;

            if (InArea(x, y, _nextArea))
            {
                if (!HasAnswer)
                {
                    var mBox = new MessageDialog();
                    mBox.ShowMessage(_activeLg.Equals(Constants.FR) ? "Sélectionnez une image." : "Please select an image.");
                    return false;
                }

                TransformAndPrintPicture();
                return false;
            }

            if (InArea(x, y, _cancelArea))
            {
                Close();
                return false;
            }

            foreach (var z in _zones)
            {
                if (!InArea(x, y, z.BkgSelectOption.Zone)) continue;

                var bso = z.BkgSelectOption;
                var answer = bso.AnswersUponSelection.First();
                SaveAnswerToQuestion(answer);
                _chosenFrameFile = z.FrameFile;

                if (_nextArea == null)
                {
                    TransformAndPrintPicture();
                    return false;
                }

                HasAnswer = true;
                Refresh();
                return false;
            }

            return true;
        }

        /// <summary>
        /// Action de transformation personnalisée avec la même signature que "PrintStationUtil.CreateFramedPhoto".
        /// Remplace cette même méthode si présente.
        /// Fait NEXT à la fin.
        /// </summary>
        public Action<string, PicturePostInfo, string, string> CustomPrintTranformation { get; set; }

        private void TransformAndPrintPicture()
        {
            var pictureSel = GetSelectedPicture();
            var saveFile = GetTerminalDirectoryDataFile(GenerateUniqueId.GetId() + ".jpg",
                DirectoryUtil.ProjectDataDirectoriesEnum.pictures,
                DirectoryUtil.ProjectDataDirectoriesModEnum.print);

            if (CustomPrintTranformation != null) { CustomPrintTranformation.Invoke(pictureSel.Src, pictureSel, saveFile, _chosenFrameFile); }
            else { PrintStationUtil.CreateFramedPhoto(pictureSel.Src, pictureSel, saveFile, _chosenFrameFile); }

            if (_screenConfig.QuestionPictureToPrintNo != null)
            {
                //SaveAnswerToQuestion(_screenConfig.QuestionPictureToPrintNo.Value, 1, saveFile);
            }

            if (!NoPrint)
            {
                var database = ServicesContainer.GetService<PrintStationDatabase>();
                var picture = MainForm.GetInstance().SelectedPicture;
                database.AddPrintCount(picture.ServerPostId);

                PrintUtil.Print(saveFile, null, this);

                if (_screenConfig.QuestionNumberOfPrintNo != null)
                {
                    IncrementEventInQuestionnaire(_screenConfig.QuestionNumberOfPrintNo.Value, 1);
                }

                NewPrintEvent.GetInstance().NewItemEvent.Set();
            }

            Next();
        }

        /// <summary>
        /// Indique qu'il ne faut pas imprimer le fichier après la transformation, si mis à vrai. 
        /// Faux par défaut, ce qui implique que l'impression à lieu.
        /// </summary>
        public bool NoPrint { get; set; }

        /// <inheritdoc/>
        public override void SetActivated()
        {
            base.SetActivated();
            log.Trace("SetActivated Called");
        }
    }
}
