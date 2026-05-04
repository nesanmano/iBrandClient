using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using NLog;
using JetBrains.Annotations;
using NomadLogic.Common.Ui.NomadScreen;
using NomadLogic.Common.Util.Json;
using NomadLogic.Common.Util.Win32;
using NomadLogic.iBrandDesktop.Ui.ApplicationScreen.Generic.Helpers;
using NomadLogic.iBrandDesktop.Ui.ApplicationScreen.Generic.Helpers.ScreenDefinitions;

namespace NomadLogic.iBrandDesktop.Ui.ApplicationScreen.Generic
{
    /// <summary>
    /// Cette classe se veut une option hautement configurable pour les écrans qui sont utiliser pour saisir des réponses venant de l'usager.
    /// Spécifiquement, elle utilise un lot de propriétées, idéalement toutes "JSON-ifiable", pour définir les éléments divers suivants:
    /// - Boîtes de textes (TextBox) *DONE
    /// - Boîte de choix (ComboBox) *DONE
    /// - Coches (CheckBox) *DONE
    /// - Clavier (Et son type) * DONE
    /// - Zones de navigation (SkipScreenData) * DONE
    /// - Zones de réponses (Zone qui provoque l'enregistrement d'une réponse spécifique) * DONE
    /// - Validations de base (Questions obligatoires, formatage de texte via REGEX) * DONE
    /// - Modification de l'action et de l'intervalle du chronomètre d'inactivité. * DONE
    /// - Actions spécifiques (comme impression d'image, incrémentation du compteur)
    /// - Présentation d'images basées sur conditions (Si Q9C2, alors afficher une image spécifique, non basé sur les réponses d'un répondant) -> Ajouter à la classe ScreenDefinitionPreviews
    /// - Présentation d'image contenues dans des questions * DONE
    /// - Présentation de texte basé sur les réponses à des questions * DONE
    /// - Compatible i18n (ou du moins, englais et français).
    /// - Lancer les "ImageTransformOptions", soit en arrivant sur la page, soit en quittant la page.
    /// - Affichage de longs textes avec barre de défilement, comme des mentions légales. * DONE
    /// - Saisie d'un RFID sans requête externe * DONE
    ///
    /// À peu près chacune des fonctionalités mentionnées ci-haut sont intégrées d'une façon ou d'une autre dans un autre écran.
    /// Cette classe se veut comme une sorte de prototype où il est possible d'intégrer chacune de ces fonctionalités dans un tout cohérent,
    /// avec comme but de faire en sorte que ces fonctionalité "génériques" soient la base de tous les écrans dans le futur.
    ///
    /// Le but principal est d'éviter à l'avenir de devoir créer pour tous les projets de simples classes de questionnaire.
    /// Cette classe vise aussi à rendre obsolète les classes suivantes, en intégrant leur fonctionalité de façon compréhensive:
    /// - BackgroundSelectionScreen (Zones de réponses associées à des images)
    /// - DisclaimerScreen (Long textes avec défilement)
    /// - EmailSCreen2, MailSCreen, SmsScreen (questions textes avec vérification)
    /// - PreviewSCreen (partiel -> affichage de base fonctionnel)
    /// - SectionSkipScreen
    /// - ThanksEndScreen
    /// - RfidScannerScreen
    /// </summary>
    [PublicAPI]
    public class GenericScreen : AbstractScreen, ILanguageScreenInterface
    {
        #region Propriétés
        private static readonly Logger log = LogManager.GetCurrentClassLogger();

        /// <summary>
        /// Si mis à vrai, les items peints par cette classe vont afficher des informations supplémentaires à fin de débogage.
        /// </summary>
        public bool VisualDebugMode { get; set; }

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
        /// Pour afficher du texte basé sur le contenu des réponses.
        /// </summary>
        public ScreenDefinitionQuestionLabels DefinitionQuestionLabels { get; set; }

        /// <summary>
        /// Pour afficher des versions modifiées des images pointées par les réponses.
        /// </summary>
        public ScreenDefinitionPreviews DefinitionImagePreviews { get; set; }

        /// <summary>
        /// Définition des boîtes de textes, pour la siasie d'information.
        /// </summary>
        public ScreenDefinitionTextBoxes DefinitionTextbox { get; set; }

        /// <summary>
        /// Cette définition sert à mettre des réponses directement dans le questionnaire, dans un but de débogage.
        /// On ne devrait pas mettre aucune question de cette manière dans un project en production.
        /// </summary>
        public ScreenDefinitionDebug ScreenDefinitionDebug { get; set; }

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
        public ScreenDefinitionCheckBoxes DefinitionCheckBoxes { get; set; }
        
        /// <summary>
        /// Cette définition sert à transférer le "Event Day" d'une configuration Eshots vers le questionnaire.
        /// </summary>
        public ScreenDefinitionEshotsEventDayTransfer DefinitionEshotsEventDayTransfer { get; set; }
        
        /// <summary>
        /// Définition pour une gallery de sélection dynamique basée sur une question.
        /// </summary>
        public ScreenDefinitionGalleryFromFiles DefinitionGallery { get; set; }

        /// <summary>
        /// Indique les options d'exécution lors d'un événement d'intervalle inactif.
        /// </summary>
        public ScreenDefinitionTimerOptions DefinitionTimer { get; set; }
        
        /// <summary>
        /// Sert à afficher une large texte, internationalisable, tiré (obligatoirement) d'un fichier,
        /// dans une boîte de texte à défilement.
        /// </summary>
        public ScreenDefinitionLargeText DefinitionLargeText { get; set; }

        /// <summary>
        /// Sert quand on a à saisir un à plusieurs RFIDs.
        /// </summary>
        public ScreenDefinitionRfidMultipleScan DefinitionRfidScan { get; set; }

        /// <summary>
        /// Configure les éléments pour lancer des impressions d'images
        /// </summary>
        public ScreenDefinitionImpressions DefinitionImpressions { get; set; }

        /// <summary>
        /// Configure les boutons de copies de fichier.
        /// </summary>
        public ScreenDefinitionImageCopyList DefinitionImageCopy { get; set; }

        /// <summary>
        /// Pour faire une copie d'un répertoire configurable vers un répertoire manuellement choisi à chaque interaction.
        /// </summary>
        public ScreenDefinitionFolderCopy DefinitionFolderCopy { get; set; }

        /// <summary>
        /// Définit une boîte de recherche pour trouver un fichier dans un FTP.
        /// </summary>
        public ScreenDefinitionFtpFileFinder DefinitionFtpImageSource { get; set; }

        #endregion

        /// <summary>
        /// Constructeur principal. Utilisé pour rester compatible avec AbstractScreen. L'objet ScreenConfig ne sera pas utilisé, mais
        /// par convention on le garde.
        /// </summary>
        /// <param name="backgroundImgEnSrc">L'image de fond englaise.</param>
        /// <param name="backgroundImageFrSrc">L'image de fond française.</param>
        /// <param name="screenConf">Les configuration d'écrans globales. Non utilisé.</param>
        public GenericScreen(string backgroundImgEnSrc, string backgroundImageFrSrc, ScreenConfig screenConf) : 
            base(backgroundImgEnSrc, backgroundImageFrSrc, screenConf) { }
        
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

            this.SafeCall(() => { this.PaintAllProperties(g, VisualDebugMode); });  // TODO On devrait mettre cela dans AbstractScreen en fait...
        }

        public override bool Init()
        {
            return base.Init() && this.InitAllProperties(); // TODO On devrait mettre cela dans AbstractScreen en fait...
        }

        public override void Recycle()
        {
            base.Recycle();

            this.SafeCall(this.RecycleAllProperties);  // TODO On devrait mettre cela dans AbstractScreen en fait...
        }

        protected override void OnTimerEvent(object sender, EventArgs e)
        {
            this.SafeCall(() => this.OnIntervalTimerEvent(() => base.OnTimerEvent(sender, e)));
        }

        protected override bool OnMouseDownHandler(int x, int y)
        {
            if (!base.OnMouseDownHandler(x, y)) return false;
            
            var rez = this.EvaluateResultsAllProperties(x, y, EvaluateQuestionState);

            if (!rez.Any()) 
            {
                log.Trace("Mouse Down Event (Generic - {2}): ({0},{1}) - No Hit", x, y, Tag);
                return true; // Aucune zone touchée.
            }
            
            log.Trace("Mouse Down Event (Generic - {2}): ({0},{1}) - Hit: {3}", x, y, Tag, rez.Count);

            // À partir d'ici, tout doit retourner faux car une zone a été touchée.
            var agg = ScreenEvaluationResult.AggregateResults(rez);

            ExecuteScreenResult(agg);

            return false;
        }

        private void ExecuteScreenResult(ScreenEvaluationResult agg)
        {
            // S'il y a des messages à afficher, on le fait ici.
            ScreenUtil.CheckAndShowUserMessages(agg.UserMessages);

            if (agg.Result != EvaluationResult.Valid)
            {
                return;
            }

            if (!SaveToQuestionnaire())
            {
                // TODO i18n
                ScreenUtil.CheckAndShowUserMessages("Could not save data for current form. Please retry.");
                return;
            }

            foreach (var act in agg.ActionsToExecute)
            {
                act?.Invoke();
            }
        }

        /// <summary>
        /// Provoque une validation de l'écran et une exécution des résultats, en plus du résultat donnée en paramètre, qui peut être construit sur mesure.
        /// </summary>
        public void ProvokeScreenEvaluation(ScreenEvaluationResult ser)
        {
            var rez = EvaluateQuestionState();
            var agg = ScreenEvaluationResult.AggregateResults(rez, ser);

            ExecuteScreenResult(agg);
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
        internal ScreenEvaluationResult EvaluateQuestionState()
        {
            var rez = this.EvaluateStateAllProperties();
            
            var agg = ScreenEvaluationResult.AggregateResults(rez);

            return agg;
        }

        protected bool SaveToQuestionnaire()
        {
            return this.SaveAllProperties();
        }

        #endregion
    }
}