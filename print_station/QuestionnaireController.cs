using System;
using System.IO;
using System.Text.RegularExpressions;
using System.Windows.Forms;
using NLog;
using NomadLogic.Common;
using NomadLogic.Common.Respondent;
using NomadLogic.Common.Services;
using NomadLogic.Common.Ui.NomadScreen;
using NomadLogic.Common.Util;
using NomadLogic.Common.Util.Print;
using NomadLogic.iBrandDesktop;
using NomadLogic.iBrandDesktop.Ui;
using NomadLogic.iBrandDesktop.Ui.ApplicationScreen;
using NomadLogic.iBrandDesktop.Ui.Choice;
using NomadLogic.PrintStation.Services;

namespace NomadLogic.PrintStation
{
    /// <summary>
    /// Classe pour gérer le questionnaire dans un projet PrintStation.
    /// </summary>
    [Obsolete]
    public class QuestionnaireController : TouchScreenController
    {
        private static readonly Logger log = LogManager.GetCurrentClassLogger();
        private const string ANWSER_PATTERN = @"_Q[0-9]+C[0-9]";
        private const string POSITIVE_INTEGER_PATTERN = @"[0-9]+";
        private string _fileToPrint;
        /// <summary>
        /// Événement quand on ferme le contrôlleur.
        /// </summary>
        public EventHandler OnCloseControl;

        /// <summary>
        /// Constructeur principal.
        /// </summary>
        /// <param name="project">L'instance de projet relié au questionnaire.</param>
        /// <param name="eventt">l'événement relié au questionnaire.</param>
        /// <param name="userName">Le nom de l'usager PrintStation lié au questionnaire.</param>
        /// <param name="questionnaire">L'instance du questionnaire à gérer.</param>
        /// <param name="screenPanel">La forme parent.</param>
        public QuestionnaireController(Project project, IbrandEventInfo eventt, string userName, Questionnaire questionnaire, Panel screenPanel) :
            base(project, eventt, userName, questionnaire, screenPanel)
        {
        }
        /*
        private void SetPictureAnswer(string pictureName, int questionNo)
        {
            var question = Questionnaire.GetQuestion(Questionnaire.GetPhotoTextQuestionNo(questionNo));
            var choiceText = (ChoiceText)question.GetChoice(1);

            if (choiceText.IsAnswered()) return;

            // On répond à cette question si elle n'est pas déjà répondue.
            // TODO cela semble être du code pour un projet précis. Il faudrait refactoriser ça avec des configurations.

            File.Copy(MainForm.GetInstance().SelectedPicture.Src,
                DirectoryUtil.GetLocalIbrandQuestionnaireDataDir(Project.Id, Questionnaire.Id,
                    DirectoryUtil.ProjectDataDirectoriesEnum.pictures,
                    DirectoryUtil.ProjectDataDirectoriesModEnum.none) + pictureName);

            var answer = new Answer{QuestionType = QuestionTypeEnum.Photo, QuestionNo = question.No, ChoiceNo = choiceText.No, Response = pictureName};
            choiceText.SetAnswer(answer);


            question = Questionnaire.GetQuestion(Questionnaire.GetPhotoTextQuestionNo(1));
            if (question == null) return;

            var choice = question.GetChoice(1);
            if (choice.IsAnswered()) return;

            File.Copy(MainForm.GetInstance().SelectedPicture.Src,
                DirectoryUtil.GetLocalIbrandQuestionnaireDataDir(Project.Id, Questionnaire.Id,
                    DirectoryUtil.ProjectDataDirectoriesEnum.pictures,
                    DirectoryUtil.ProjectDataDirectoriesModEnum.none) + pictureName);

            answer = new Answer{QuestionType = QuestionTypeEnum.Photo, QuestionNo = question.No, ChoiceNo = choiceText.No, Response = pictureName};
            choice.SetAnswer(answer);
        }
        */

        /// <summary>
        ///     Cette méthode identifie les métadonnée de question Combo contenues dans le nom de l'image choisie par le
        ///     PrintStation
        ///     Si un nom de fichier contient l'expression régulière suivante: "_Q[0-9]+C[0-9]" en un ou plusieurs
        ///     instance,
        ///     chaque instance sera convertie en réponse pour le questionnaire.
        /// </summary>
        /// <param name="pictureName">Le chemin du fichier à analyser.</param>
        private void SetAnswerFromFileName(string pictureName)
        {
            try
            {
                foreach (Match matchGroup in Regex.Matches(StringUtil.GetFileName(pictureName), ANWSER_PATTERN, RegexOptions.None) )
                {
                    var i = 0;
                    int[] numbers = { -1, -1 };
                    foreach (Match matchNumber in Regex.Matches(matchGroup.Value, POSITIVE_INTEGER_PATTERN, RegexOptions.None))
                    {
                        numbers[i] = int.Parse(matchNumber.Value);
                        i++;
                    }

                    var question = Questionnaire.GetQuestion(numbers[0]);
                    if (question == null) continue;
                    /*
                    var choice = (ChoiceCombo)question.GetChoice(numbers[1]);
                    var answer = new Answer {QuestionType = QuestionTypeEnum.Combo, QuestionNo = question.No, ChoiceNo = numbers[2], Response = pictureName};
                    choice.SetAnswer(answer);
                    */
                }
            }
            catch (Exception e)
            {
                log.Error(e, "Error during setting answer from question regex from following picture file name: " + pictureName);
            }
        }

        /// <summary>
        /// Transforme l'image choisie par l'usager en la mettant sur un cadre donnée par les configurations. On va placer l'image composite
        /// dans le sous-répertoire d'image d'impression du projet actif.
        /// </summary>
        /// <param name="pictureName">Le nom du fichier à créer. Ne doit pas contenir de chemin, juste le nom du fichier.</param>
        /// <param name="automaticSave">Indique que l'on sauvegarder les fichiers originaux et modifiés dans, respectivement, la première
        /// question de type photoTexte, et la deuxième (le numéro des question n'est pas obligatoirement 1 et 2).
        /// Remplace le contenu de ces réponses et donc ne devrait pas être activé si on
        /// utilise un questionnaire qui remplit déjà ces questions.</param>
        private void TransformPictureToPrint(string pictureName, bool automaticSave)
        {
            var pict = MainForm.GetInstance().SelectedPicture;
            if (pict == null || !File.Exists(pict.Src))
            {
                log.Warn("Invalid file to transform: " + pict?.Src);
                return;
            }

            try
            {
                _fileToPrint = DirectoryUtil.GetLocalIbrandQuestionnaireDataDir(Project.Id, Questionnaire.Id,
                    DirectoryUtil.ProjectDataDirectoriesEnum.pictures,
                    DirectoryUtil.ProjectDataDirectoriesModEnum.print) + pictureName;

                using (PrintStationUtil.CreateFramedPhoto(pict.Src, pict, _fileToPrint))
                {
                    // On ne garde pas le bitmap créé. Il a déjà été sauvegardé et ça nous suffit...
                }

                if (!automaticSave) return;

                var question = Questionnaire.GetQuestion(Questionnaire.GetPhotoTextQuestionNo(2));
                Answer answer;

                if (!Questionnaire.HasQuestionPhotoTextCount(1))
                {
                    var choiceText = (ChoiceText) question.GetChoice(1);
                    answer = new Answer{QuestionType = QuestionTypeEnum.Photo, QuestionNo = question.No, ChoiceNo = choiceText.No,Response = _fileToPrint};
                    choiceText.SetAnswer(answer);
                }

                question = Questionnaire.GetQuestion(Questionnaire.GetPhotoTextQuestionNo(1));
                var choice = question.GetChoice(1);
                answer = new Answer{QuestionType = QuestionTypeEnum.Photo, QuestionNo = question.No, ChoiceNo = choice.No,Response = _fileToPrint};
                choice.SetAnswer(answer);
            }
            catch (Exception ex)
            {
                log.Error(ex, "Could not transform picture to print.");
            }
        }
        
        /// <summary>
        /// Gestion de l'événement d'aller à une autre page, via un nombre précis de page à changer.
        /// <br/>Quand on passe la dernière page, on applique la sauvegarde du questionnaire.
        /// </summary>
        /// <param name="nbrPage">Le nombre de page à sauter, incluant l'actuelle. La valeur de 1 est donc la prochaine page.</param>
        protected override void OnNextScreen(int nbrPage)
        {
            if (_activeScreen == null)
                return;

            if (log.IsTraceEnabled)
            {
                log.Trace("On change. Answers={0}", StringUtil.FormatDictionary(Questionnaire.GetAnswersDictionary()));
            }

            var currentIndex = _screenList.IndexOf(_activeScreen);
            var nextIndex = currentIndex + nbrPage;
            var maxIndex = _screenList.Count - 1;

            if (nextIndex > maxIndex)
            {
                Cursor.Current = Cursors.WaitCursor;

                try
                {
                    var hasPrint = RunningFeatureConfig.GetInstance().GetConfiguration<bool>(FeatureConfigEnum.HasPrint);
                    var hasFrame = RunningFeatureConfig.GetInstance().GetConfiguration<bool>(FeatureConfigEnum.HasFrame);
                    var hasQuestionnaire = RunningFeatureConfig.GetInstance().GetConfiguration<bool>(FeatureConfigEnum.HasQuestionnaire);

                    if (hasPrint)
                    {
                        if (hasFrame)
                        {
                            var filename = GenerateUniqueId.GetId() + ".jpg";
                            TransformPictureToPrint(filename, !hasQuestionnaire);
                        }
                        else
                        {
                            _fileToPrint = MainForm.GetInstance().SelectedPicture.Src;
                        }
                    }
                    var x = MainForm.GetInstance().SelectedPicture.Src;
                    SetAnswerFromFileName(MainForm.GetInstance().SelectedPicture.Src);
                }
                catch (Exception ex)
                {
                    log.Fatal(ex.StackTrace);
                }

                SaveQuestionnaire(false);
                
                if (_fileToPrint != null)
                {
                    var database = ServicesContainer.GetService<PrintStationDatabase>();

                    if (MainForm.GetInstance().SelectedPicture.ServerPostId != -1)
                        database.AddPrintCount(MainForm.GetInstance().SelectedPicture.ServerPostId);
                    else
                        database.AddPrintCountSocialCam(database.GetActiveUser().EventId);

                    PrintUtil.Print(_fileToPrint, null, MainForm.GetInstance());
                    NewPrintEvent.GetInstance().NewItemEvent.Set();
                }
                Cursor.Current = Cursors.Default;

                OnCloseScreen(this, EventArgs.Empty);

                SetNewRespondentEvent();

                return;
            }

            SetNextScreen(nextIndex);
        }

        /// <summary>
        /// Ajoute un écran de questionnaire au gestionnaire.
        /// </summary>
        /// <param name="screen">l'écran à ajouter.</param>
        public override void AddScreen(IAbstractScreen screen)
        {
            _screenList.Add(screen);

            screen.Visible = true;

            screen.ParentForm = _screenPanel;

            screen.OnClose += OnCloseScreen;
            screen.OnNext += OnNextScreen;
            screen.OnBack += OnBackScreen;
            screen.OnChangeToPseudoModal += ChangeToPseudoModalScreen;

            if (screen is ILanguageScreenInterface lsi)
            {
                lsi.OnLanguageSelection += OnLanguageSelection;
            }
        }
        
        private void OnCloseScreen(object sender, EventArgs e)
        {
            Questionnaire.Reset();

            MainForm.GetInstance().SelectedPicture = null;

            _fileToPrint = null;

            RecycleScreens();

            OnCloseControl?.Invoke(this, EventArgs.Empty);
        }
    }
}