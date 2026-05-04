using System;
using System.Drawing;
using System.Linq;
using NomadLogic.Common.Ui.NomadScreen;
using NomadLogic.Common.Util;
using NomadLogic.Common.Util.Json;
using NomadLogic.Common.Util.Win32;
using NomadLogic.iBrandDesktop.Ui.ApplicationScreen.Generic.Helpers;
using NomadLogic.iBrandDesktop.Ui.ApplicationScreen.Generic.Helpers.ScreenDefinitions;

namespace NomadLogic.PrintStation.Ui.generic
{
    /// <summary>
    /// Version Printstation de la classe d'écran générique.
    /// </summary>
    public class GenericScreen : AbstractPrintStationScreen, ILanguageScreenInterface
    {
        #region Propriétés

        /// <summary>
        /// Si mise à vrai, cette propriété va faire en sorte que l'écran va écrire sur disque la sérialisation de ses propriétés de configuration
        /// plutôt que de tenter de lire des configurations sur disque via les mêmes noms de fichier.
        /// Les fichiers créés par la sérialisation tenteront de ne pas écraser des fichiers déjà existants, pour ne pas perdre de données.
        /// Peut être utilisé dans les premières mises en place des recettes pour créer les bases des fichiers de configuration, puis remis à faux pour
        /// utiliser ces fichiers de configurations par la suite.
        /// Si un nom de fichier spécifique n'existe pas, on utilise le nom par défaut de la classe de définition. Le chemin de base sera celui
        /// du répertoire local du questionnaire, le même d'où il pige ses fichiers de configuration habituellement.
        /// Cela ne concerne que les propriétés implémentant les interfaces de configuration par fichiers, soit IJsonConfig et IExternalFileConfig
        /// </summary>
        public bool DebugWritePropertiesToFiles { get; set; }

        /// <summary>
        /// Définition des boîtes de textes.
        /// </summary>
        public ScreenDefinitionTextBoxes DefinitionTextbox { get; set; }

        /// <summary>
        /// Définition des boîtes de navigation.
        /// </summary>
        public ScreenDefinitionNavigation DefinitionNavigation { get; set; }

        /// <summary>
        /// Définition de l'emplacement et configuration du clavier.
        /// </summary>
        public ScreenDefinitionKeyboard DefinitionKeyboard { get; set; }

        /// <summary>
        /// Définition des zones cliquables qui remplissent le questionnaire via des réponses spécifiques.
        /// </summary>
        public ScreenDefinitionAnswerZones DefinitionAnswerZones { get; set; }

        // TODO Vérifier si on ne pourrait pas fusionner langues et answers ensemble.
        /// <summary>
        /// Définitions des zones qui changent la langues du répondant/questionnaire/écrans.
        /// </summary>
        public ScreenDefinitionLanguageZones DefinitionLanguageZones { get; set; }
        
        /// <summary>
        /// Définition des boîtes de listes déroulantes.
        /// </summary>
        public ScreenDefinitionComboBoxes DefinitionComboxbox { get; set; }

        /// <summary>
        /// Liste des définitions de boîtes à cocher.
        /// </summary>
        public ScreenDefinitionCheckBoxes DefinitionsCheckBoxes { get; set; }

        #endregion

        /// <summary>
        /// Constructeur principal. Utilisé pour rester compatible avec AbstractScreen. L'objet ScreenConfig ne sera pas utilisé, mais
        /// par convention on le garde.
        /// </summary>
        /// <param name="backgroundImgEnSrc">L'image de fond englaise.</param>
        /// <param name="backgroundImageFrSrc">L'image de fond française.</param>
        /// <param name="screenConf">Les configuration d'écrans globales. Non utilisé.</param>
        public GenericScreen(string backgroundImgEnSrc, string backgroundImageFrSrc, PrintStationScreenConfig screenConf) : 
            base(backgroundImgEnSrc, backgroundImageFrSrc, screenConf)
        {
            
        }
        
        #region Overloads

        [Obsolete]
        public int? QuestionLanguageNo { get; set; }
        [Obsolete]
        public int QuestionLanguageChoiceNoFr { get; set; } = 2;
        [Obsolete]
        public int QuestionLanguageChoiceNoEn { get; set; } = 1;
        [Obsolete]
        public bool IsQuestionLanguageRadio { get; set; }

        public event LanguageSelectionEventHandler OnLanguageSelection;

        public void LanguageSelection(string lg)
        {
            OnLanguageSelection?.Invoke(lg);
        }

        protected override void OnLoad()
        {
            base.OnLoad();

            var baseDir = GetTerminalDirectory();
            
            if (!DebugWritePropertiesToFiles)
            {
                JsonUtil.UpdatePropertiesFromFiles(this, baseDir);
            }
            else
            {
                JsonUtil.WritePropertiesToFiles(this, baseDir, true);
            }

            this.SafeCall(() => this.OnLoadAllProperties());  // TODO On devrait mettre cela dans AbstractScreen en fait...
            
            InitBitmap();
        }


        public override bool ChangeLanguage(string language)
        {
            var change = base.ChangeLanguage(language);
            if (!change) return false;

            this.SafeCall(InitBitmap);

            this.SafeCall(() => this.ChangeLanguageAllProperties(_activeLg));  // TODO On devrait mettre cela dans AbstractScreen en fait...

            return true;
        }

        protected override void OnPaintEvent(Graphics g)
        {
            base.OnPaintEvent(g);

            this.SafeCall(() => { this.PaintAllProperties(g, false); });  // TODO On devrait mettre cela dans AbstractScreen en fait...
        }

        public override void Recycle()
        {
            base.Recycle();

            this.SafeCall(this.RecycleAllProperties);  // TODO On devrait mettre cela dans AbstractScreen en fait...
        }

        protected override void OnTimerEvent(object sender, EventArgs e)
        {
            base.OnTimerEvent(sender, e);
        }

        protected override bool OnMouseDownHandler(int x, int y)
        {
            if (!base.OnMouseDownHandler(x, y)) return false;
            
            var rez = this.EvaluateResultsAllProperties(x, y, EvaluateQuestionState);

            if (!rez.Any()) return true; // Aucune zone touchée.

            // À partir d'ici, tout doit retourner faux car une zone a été touchée.
            var agg = ScreenEvaluationResult.AggregateResults(rez);

            // S'il y a des messages à afficher, on le fait ici.
            ScreenUtil.CheckAndShowUserMessages(agg.UserMessages);

            if (agg.Result != EvaluationResult.Valid) return false; 
            
            SaveToQuestionnaire();

            foreach (var act in agg.ActionsToExecute)
            {
                act?.Invoke();
            }

            return false;
        }

        /// <inheritdoc />
        public override void SetActivated()
        {
            base.SetActivated();

            this.ActivateAllProperties(GetCurrentRespondentWithAnswers());
        }

        public override void Dispose()
        {
            base.Dispose();

            this.SafeCall(this.DisposeAllProperties);  // TODO On devrait mettre cela dans AbstractScreen en fait...
        }

        #endregion

        #region Méthodes

        /// <summary>
        /// Méthode pour instancier et/ou mettre à jour les images utilisées par l'écran.
        /// Doit être appellée quand on change de langue, pas simplement quand on change d'usager (Recycle et SetActivated font ce cas de figure).
        /// </summary>
        protected virtual void InitBitmap()
        {
            var baseDir = GetTerminalDirectory();

            this.InitializeImagesAllProperties(baseDir);
        }

        /// <summary>
        /// Cette méthode valide l'état que tous les éléments contenus dans cet écran et confirme si on peut avancer.
        /// </summary>
        /// <returns>Le résultat aggrégé de cette vérification.</returns>
        protected ScreenEvaluationResult EvaluateQuestionState()
        {
            var rez = this.EvaluateStateAllProperties();
            
            var agg = ScreenEvaluationResult.AggregateResults(rez);

            return agg;
        }

        protected void SaveToQuestionnaire()
        {
            this.SaveAllProperties();
        }

        #endregion
    }
}