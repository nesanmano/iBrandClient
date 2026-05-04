using JetBrains.Annotations;
using NLog;
using NomadLogic.Common.Util;
using NomadLogic.PrintStation.Ui.generic;
using System;
using System.IO;
using System.Threading.Tasks;
using System.Windows.Forms;
using NomadLogic.Common.Ui.Dialog;
using NomadLogic.Common.Util.Json;
using NomadLogic.Common.Util.Networking.QuestionnaireWebRequest;

namespace NomadLogic.PrintStation.Ui.custom.Brewers
{
    [PublicAPI]
    public abstract class EshotsApi : AbstractPrintStationScreen
    {
        private static readonly Logger log = LogManager.GetCurrentClassLogger();

        protected EshotsApi(string bkgEn, string bkgFr, PrintStationScreenConfig sc)
         : base(bkgEn, bkgFr, sc)
        {

        }

        protected override async void OnLoad()
        {
            base.OnLoad();

            if (!await InitializeFacade())
            {
                MsgBox.Show(this, "Configuration file was not found. Contact system admin.", "iBrand",
                    MessageBoxButtons.OK, MessageBoxIcon.Warning);
            }
        }
        
        private WebRequestFacade _facadeInstance;
        
        /// <summary>
        /// Si n'étend pas le code pour le résultat web, on peut dire ici qu'un code problématique provoque un problème.
        /// </summary>
        public bool FailOnNullResult { get; set; }

        /// <summary>
        /// Nom du fichier de configuration de facade. Doit etre dans le repertoire de base du questionnaire.
        /// </summary>
        public string WebFacadeConfigFile { get; set; } = "webfacade_config.json";
        
        /// <summary>
        /// Définition de la méthode à invoquer dans la facade web instanciée par réflexion lors de la lecture d'un code RFID.
        /// On ne peut appeller qu'une seule méthode.
        /// </summary>
        public WebRequestFacadeAction CardReadAction { get; set; }

        /// <summary>
        /// Cette méthode appelle la facade et les fonctions connexes avec le ID de la carte lue.
        /// </summary>
        /// <param name="sequenceRead">Le code saisi par l'écran.</param>
        protected async Task<bool> ExecuteFacade(string sequenceRead)
        {
            if (_facadeInstance == null || string.IsNullOrWhiteSpace(sequenceRead)) return false;
            
            var resp = GetCurrentRespondentWithAnswers();
            var dic = GetCurrentQuestionnaire().GetAnswersDictionary();
            DialogResult diagResult;

            do
            {
                log.Trace("Launching facade method.");
                var res = await _facadeInstance.ConstructAndInvokeMethod(resp, dic);
                
                // Sauvegarde le résultat de l'appel via les configurations de "mapping" de réponse à questionnaire contenues dans la configuration de la facade.
                log.Trace("Saving data from request.");
                _facadeInstance.SaveDataToQuestionnaire(this, res);

                diagResult = ExecuteFacadeCustomCode(sequenceRead, res, _facadeInstance.WebRequestAction.ReturnType);
                log.Trace("Result from custom code: {0}", diagResult);

            } while (diagResult == DialogResult.Retry); // Si l'usager veut recommencer, on le fait immédiatement.

            return diagResult == DialogResult.OK;
        }

        /// <summary>
        /// Méthode pour crochet de code personnalisé à la fin d'une exécution de requête qui ne peut être gérée totalement par la façade.
        /// </summary>
        /// <param name="cardUid"></param>
        /// <param name="res">l'instance de l'objet résultat venant de la requête.</param>
        /// <param name="returnType">Le type de cet objet.</param>
        /// <returns>OK si tout s'est bien passé. Sinon, un retour de dialogue indiquant ce qu'il faut faire.</returns>
        protected abstract DialogResult ExecuteFacadeCustomCode(string cardUid, object res, Type returnType);
        
        /// <summary>
        /// Cette méthode vérifie si une configuration est disponible pour la façade, et si oui tente d'instancier et valider l'instance de facade.
        /// Si oui, l'objet de facade sera non-nul comme effet de bord.
        /// </summary>
        /// <returns>TRUE si une configuration valide et correctement utilisable existe.</returns>
        protected async Task<bool> InitializeFacade()
        {
            var baseDir = DirectoryUtil.GetLocalIbrandTerminalDir(_screenConfig.ProjectId, _screenConfig.QuestionnaireId, _screenConfig.TerminalId);
            var configFile = baseDir + WebFacadeConfigFile;
            var facadeConfigFilePath = new FileInfo(configFile);
            log.Trace("Trying to locate configuration file: {0}", configFile);

            if (!facadeConfigFilePath.Exists) return false;
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

            try
            {
                log.Trace("Initializing facade instance: {0}", _facadeInstance.WebExecutorClassName);
                if (!await InitializeFacadeCustom(_facadeInstance))
                {
                    log.Error("Facade initialisation requested but failed.");
                    return false;
                }
            }
            catch (System.Exception e)
            {
                log.Error(e, "Error while initializing facade instance.");
                return false;
            }

            return true;
        }

        /// <summary>
        /// Dans le cas où il faut une initialisation personnalisée, on peut invoquer cette méthode.
        /// Basé sur le concept de Template method.
        /// </summary>
        /// <param name="facade"></param>
        /// <returns></returns>
        protected virtual async Task<bool> InitializeFacadeCustom([NotNull] WebRequestFacade facade)
        {
            return await facade.Init();
        }

        
        /// Permet au code d'exécuter du code supplémentaire quand on a fini la validation de l'identifiant lu.
        /// Incluant la vérification via exécution de façade web.
        /// <param name="sequenceRead">Le texte lu à valider.</param>
        protected virtual async Task<bool> OnValidateAction(string sequenceRead)
        {
            if (_facadeInstance != null) return await ExecuteFacade(sequenceRead);
            return true;
        }

        protected WebRequestFacade GetFacade()
        {
            return _facadeInstance;
        }

        public override void Dispose()
        {
            base.Dispose();

            _facadeInstance?.Dispose();
        }
    }
}
