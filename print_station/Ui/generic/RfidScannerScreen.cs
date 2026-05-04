using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Forms;
using NLog;
using NomadLogic.Common;
using NomadLogic.Common.Serveur;
using NomadLogic.Common.Ui.Dialog;
using NomadLogic.Common.Ui.NomadScreen.ScreenAction;
using NomadLogic.Common.Util;
using NomadLogic.Common.Util.Json;
using NomadLogic.Common.Util.Networking.QuestionnaireWebRequest;
using NomadLogic.Common.Util.Print;
using NomadLogic.Common.Util.RfidReader;
using NomadLogic.Common.Util.Win32;
using NomadLogic.iBrandDesktop.Ui.ApplicationScreen.Generic.Helpers;

namespace NomadLogic.PrintStation.Ui.generic
{
    /// <summary>
    /// Cet écran est utilisé pour saisir un code barre, soit manuellement, soit automatiquement via les événements de lecteur optique.
    /// </summary>
    public class RfidScannerScreen : ScanScreenPrintstationAbstract
    {
        private static readonly Logger log = LogManager.GetCurrentClassLogger();
        private IRfidReader _rfidReader;
        private WebRequestFacade _facadeInstance;
        
        /// Liste des zones configurées pour afficher du contenu du questionnaire, sous forme de Labels.
        public IList<QuestionnaireAnswerLabel> QuestionnaireLabels { get; set; }

        /// Liste des zones configurées pourr afficher une image selon le contenu du questionnaire.
        public IList<QuestionnaireAnswerImage> QuestionnaireImages { get; set; }

        /// Nom du fichier de configuration de facade. Doit etre dans le repertoire de base du questionnaire.
        public string WebFacadeConfigFile { get; set; } = "webfacade_config.json";

        /// Nom du fichier de configuration du lecteur RFID.
        public string RfidReaderConfigFile { get; set; } = "rfid_config.json";

        /// <summary>
        /// Définition de la méthode à invoquer dans la facade web instanciée par réflexion lors de la lecture d'un code RFID.
        /// On ne peut appeller qu'une seule méthode.
        /// </summary>
        public WebRequestFacadeAction CardReadAction { get; set; }

        public RfidScannerScreen(string backgroundImgEnSrc, string backgroundImgFrSrc, PrintStationScreenConfig sc, Rectangle? textboxLocation = null) :
            base(backgroundImgEnSrc, backgroundImgFrSrc, sc, textboxLocation)
        { }

        protected override void OnLoad()
        {
            base.OnLoad();

            ScanTriggerOnCharacterNumber = _rfidReader?.ScanTriggerOnCharacterNumber ?? 0;
        }
        
        /// Dès que la zone de texte reçoit ce nombre de caractère (ou plus), cela provoque une vérification du code. Cette valeur est tirée de l'instance du lecteur RFID
        /// et est en lecture seule.
        public new int ScanTriggerOnCharacterNumber
        {
            get => _rfidReader?.ScanTriggerOnCharacterNumber ?? 0;
            private set
            {
                _scanTriggerOnCharacterNumber = value;
                UpdateScanCharacterTriggerEvent();
            }
        }
        
        protected override async void CustomLoad()
        {
            // Charger le lecteur RFID. S'il n'y a pas de fichier de configuration, on assume que c'est un lecteur "léger" (qui ne requiert pas de configuration complexe).
            var configFilePath = new FileInfo(DirectoryUtil.GetLocalIbrandTerminalDir(ProjectId, QuestionnaireId, TerminalId) + RfidReaderConfigFile);
            _rfidReader = configFilePath.Exists ? JsonUtil.ReadJsonFile<IRfidReader>(configFilePath) : new Simple_RfidReader();

            if (_rfidReader.ConnectReader())
            {
                // Vérification de spécialisation d'instance.
                if (_rfidReader is IRfidEvents rfidEvent)
                {
                    rfidEvent.CardRead += ScanManagerOnCardRead;
                    rfidEvent.HardWareDisconnected += ScanManagerOnHardWareDisconnected;
                    rfidEvent.InvalidCardRead += ScanManagerOnInvalidCardRead;
                }
            }
            else
            {
                if (_errorBitmap == null)
                {
                    MsgBox.Show(this, 
                        _activeLg.Equals(Constants.FR, StringComparison.InvariantCultureIgnoreCase) ? 
                            "Lecteur RFID impossible à connecter." : "Cannot connect to RFID reader.", 
                        "iBrand",
                        MessageBoxButtons.OK, MessageBoxIcon.Warning);
                }
                else
                {
                    _showError = true;
                }
                Refresh();
            }

            if (QuestionnaireLabels != null && QuestionnaireLabels.Any())
            {
                foreach (var qal in QuestionnaireLabels)
                {
                    if (!qal.Init() || qal.LabelInstance == null)
                    {
                        throw new System.Exception("Could not initialize Questionnaire Label: " + qal);
                    }
                    qal.LabelInstance.Parent = PictureBox;
                    qal.LabelInstance.BringToFront();
                }
            }

            InitBitmap();

            if (!await InitializeFacade())
            {
                MsgBox.Show(this, "Configuration file was not found. Contact system admin.", "iBrand",
                    MessageBoxButtons.OK, MessageBoxIcon.Warning);
            }
        }

        //Partie pour afficher un label
        protected void InitBitmap()
        {
            if (QuestionnaireImages != null && QuestionnaireImages.Any())
            {
                foreach (var qai in QuestionnaireImages)
                {
                    if (!qai.InitImage(null, _activeLg) || qai.ImageInstance == null)
                    {
                        throw new System.Exception("Could not intiialize Questionnaire Image: " + qai);
                    }
                }
            }
        }

        private void ScanManagerOnInvalidCardRead(object sender, RfidReadEventArgs args)
        {
            if (!_activated) return;
            log.Debug("Invalid card read.");
            FormUtil.SafeCall(this, () =>
            {
                if (_errorBitmap == null)
                {
                    var message = _activeLg.Equals(Constants.FR, StringComparison.InvariantCultureIgnoreCase) ?
                        "La carte lue est invalide." :
                        "Last card read is invalid.";
                    MsgBox.Show(this, message, "iBrand",
                        MessageBoxButtons.OK, MessageBoxIcon.Warning);
                }
                else
                {
                    _showError = true;
                }
                Refresh();
            });
        }

        private void ScanManagerOnHardWareDisconnected(object sender, EventArgs eventArgs)
        {
            log.Warn("Hardware disconnected event.");

            FormUtil.SafeCall(this, () =>
            {
                if (_errorBitmap == null)
                {
                    var message = _activeLg.Equals(Constants.FR, StringComparison.InvariantCultureIgnoreCase) ?
                        "La périphérique de lecture RFID à été déconnecté." :
                        "Reading peripheral has been diconnected.";
                    MsgBox.Show(this, message, "iBrand",
                        MessageBoxButtons.OK, MessageBoxIcon.Warning);
                }
                else
                {
                    _showError = true;
                }
                Refresh();
            });
        }

        private void ScanManagerOnCardRead(object sender, RfidReadEventArgs args)
        {
            if (!_activated) return;
            log.Debug("Card read event : " + args?.CardUid);
            UpdateWithScanValue(args?.CardUid);
        }

        protected override async Task<bool> OnValidateAction(string text)
        {
            return await ExecuteFacade(text);
        }

        /// <summary>
        /// Cette méthode appelle la facade et les fonctions connexes avec le ID de la carte lue.
        /// </summary>
        /// <param name="sequenceRead">Le code lu par le lecteur RFID.</param>
        private async Task<bool> ExecuteFacade(string sequenceRead)
        {
            if (_facadeInstance == null || string.IsNullOrWhiteSpace(sequenceRead) || _rfidReader == null) return false;
            var cardUid = _rfidReader.ExecuteSequenceAnalisys(sequenceRead);

            if (ScanQuestionNo > 0)
            {
                //SaveAnswerToQuestion(ScanQuestionNo.Value, 1, cardUid, 4);
            }
            else
            {
                log.Warn("No question assigned to scan event. Web requests will probably fail. Correct this in executable.");
            }

            var resp = GetCurrentRespondentWithAnswers();
            var dic = GetCurrentQuestionnaire().GetAnswersDictionary();

            var res = await _facadeInstance.ConstructAndInvokeMethod(resp, dic);

            // Sauvegarde le résultat de l'appel via les configurations de "mapping" de réponse à questionnaire contenues dans la configuration de la facade.
            _facadeInstance.SaveDataToQuestionnaire(this, res);

            // TODO Avoir un système de vérification supplémentaire (genre tel type de retour provoque une rreur, comme dans le cas de TD PayWall (DIA-2018-02)

            var ok = ExecuteFacadeCustomCode(cardUid.FinalCode, res, _facadeInstance.WebRequestAction.ReturnType);

            if (ok)
            {
                ok &= ExecuteScreenActions(_facadeInstance.ScreenActions);
            }

            return ok;
        }

        protected virtual bool ExecuteScreenActions(IList<IScreenAction> actions)
        {
            if (actions == null || !actions.Any()) return true;

            foreach (var act in actions)
            {
                var loopBreak = false;
                switch (act)
                {
                    case PrintScreenAction print:
                        var path = print.FixedPrintPicture;
                        if (path == null && print.QuestionPrintPicture.HasValue)
                        {
                            path = GetCurrentRespondentWithAnswers().GetAnswerText(print.QuestionPrintPicture.Value);
                        }

                        if (!File.Exists(path))
                        {
                            path = DirectoryUtil.GetLocalIbrandTerminalDir(ProjectId, QuestionnaireId, TerminalId) + path;
                        }

                        if (File.Exists(path))
                        {
                            if (!PrintUtil.Print(path))
                            {
                                if (print.ShowErrorMessage)
                                {
                                    this.SafeCall(() =>
                                    {
                                        var message = _activeLg.Equals(Constants.FR)
                                            ? print.ErrorMessageFr
                                            : print.ErrorMessageEn;
                                        MsgBox.Show(this, message, @"iBrand", MessageBoxButtons.OK, MessageBoxIcon.Error, MessageBoxDefaultButton.Button1);
                                    });
                                }

                                if (print.IsCritical)
                                {
                                    log.Error("Print attempt has failed. Check printer and/or configuration.");
                                    loopBreak = true;
                                }
                            }
                            else if (print.QuestionNumberOfPrint.HasValue)
                            {
                                IncrementEventInQuestionnaire(print.QuestionNumberOfPrint.Value);
                            }
                        }
                        break;
                    // TODO AJouter les autres classes ET/OU mettre cette logique dans une méthode polymorphique.
                }

                if (loopBreak) break; // À cause du "switch"
            }

            return true;
        }

        /// Si n'étend pas le code pour le résultat web, on peut dire ici qu'un code problématique provoque un problème.
        public bool FailOnNullResult { get; set; }

        /// <summary>
        /// Méthode pour crochet de code personnalisé à la fin d'une exécution de requête qui ne peut être gérée totalement par la façade.
        /// </summary>
        /// <param name="cardUid"></param>
        /// <param name="res">l'instance de l'objet résultat venant de la requête.</param>
        /// <param name="returnType">Le type de cet objet.</param>
        /// <returns>TRUE si tout s'est bien passé. False sinon.</returns>
        protected virtual bool ExecuteFacadeCustomCode(string cardUid, object res, Type returnType)
        {
            return res != null || !FailOnNullResult;
        }

        public override void Dispose()
        {
            base.Dispose();

            // Décharger le lecteur RFID
            if (_rfidReader is IRfidEvents iRfidEvents)
            {
                iRfidEvents.CardRead -= ScanManagerOnCardRead;
                iRfidEvents.HardWareDisconnected -= ScanManagerOnHardWareDisconnected;
                iRfidEvents.InvalidCardRead -= ScanManagerOnInvalidCardRead;
                iRfidEvents.Dispose();
            }

            _facadeInstance?.Dispose();
        }

        public override void SetActivated()
        {
            base.SetActivated();

            if (QuestionnaireLabels != null && QuestionnaireLabels.Any())
            {
                var dico = GetCurrentQuestionnaire().GetAnswersDictionary();
                foreach (var qal in QuestionnaireLabels)
                {
                    var finalString = StringUtil.CreateStringFromTokenizedPatternRespondent(qal.TokenString, dico);
                    qal.LabelInstance.Text = finalString;
                }
            }

            // Vérifie les valeurs d'affichage selon le répondant actuel.
            if (QuestionnaireImages != null && QuestionnaireImages.Any())
            {
                var resp = GetCurrentRespondentWithAnswers();
                foreach (var qai in QuestionnaireImages)
                {
                    qai.Update(resp);
                }
            }
        }
        
        /// <summary>
        /// Cette méthode vérifie si une configuration est disponible pour la façade, et si oui tente d'instancier et valider l'instance de facade.
        /// Si oui, l'objet de facade sera non-nul comme effet de bord.
        /// </summary>
        /// <returns>TRUE si une configuration valide et correctement utilisable existe.</returns>
        private async Task<bool> InitializeFacade()
        {
            if (ScanQuestionNo == null)
            {
                log.Warn("No question assignec to scan ID. Web requests will probably not work.");
            }

            var baseDir = DirectoryUtil.GetLocalIbrandTerminalDir(_screenConfig.ProjectId, _screenConfig.QuestionnaireId, _screenConfig.TerminalId);
            var configFile = baseDir + WebFacadeConfigFile;
            var facadeConfigFilePath = new FileInfo(configFile);
            log.Trace("Trying to locate configuration file: {0}", configFile);

            if (facadeConfigFilePath.Exists)
            {
                try
                {
                    log.Trace("Converting configuration file to facade instance.");
                    _facadeInstance = JsonUtil.ReadJsonFile<WebRequestFacade>(facadeConfigFilePath);
                }
                catch (System.Exception e)
                {
                    log.Error(e, "Could not instanciate class from facade config file.");
                    return false;
                }

                if (_facadeInstance == null)
                {
                    log.Error("Impossible to initialize facade with config file. Check JSON config.");
                    return false;
                }
            }

            try
            {
                log.Trace("Initializing facade instance: {0}", _facadeInstance.WebExecutorClassName);
                if (await _facadeInstance.Init()) return true;
            }
            catch (System.Exception e)
            {
                log.Error(e, "Error while initializing facade instance.");
            }

            _facadeInstance = null;
            log.Warn("Configuration for web request facade was invalid. Please check project files.");
            return false;
        }
    }
}