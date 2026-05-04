using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Threading;
using System.Threading.Tasks;
using NLog;
using NomadLogic.Common.Respondent;
using NomadLogic.Common.Serveur;
using NomadLogic.Common.Serveur.Requests;
using NomadLogic.Common.Serveur.Structures;
using NomadLogic.Common.Services;
using NomadLogic.Common.Services.Configuration;
using NomadLogic.Common.Services.Database;
using NomadLogic.Common.Util;
using NomadLogic.Common.Util.DateAndTime;
using NomadLogic.iBrandDesktop.Services;
using Logger = NLog.Logger;

namespace NomadLogic.iBrandDesktop.Ui.ApplicationScreen.Generic.Helpers.Gallery
{
    /// <summary>
    /// Cette classe va chercher les informations d'un questionnaire précis sur le serveur principal pour meubler
    /// une liste de fichiers à montrer dans une gallerie.
    /// </summary>
    public class GallerySourceFromServerRespondantData : GallerySourceFixedDirectory
    {
        private static readonly Logger log = LogManager.GetCurrentClassLogger();

        private ICondensedAnswerDatabase _db;
        private ServerConfiguration _serverConfig;
        private CancellationTokenSource _cts;
        private Task _fetchData;
        private UserAuthTokenManager _authManager;
        private FileDownloadService _fileDownloadService;
        private readonly ISet<DownloadRequest> _currentRequests = new HashSet<DownloadRequest>();

        /// <summary>
        /// Maximum de demandes de téléchargements simultanées.
        /// </summary>
        public uint MaxDownloadRequests = 15;

        private DatetimeCalculator _dateCalculator = new DateTimeCalculatorFixed();

        /// <summary>
        /// ID du questionnaire ACTIF localement, à qui seront associées les données téléchargées par le système.
        /// </summary>
        public int LocalCurrentQuestionaireId { get; set; }
        
        /// <summary>
        /// L'identifiant du terminal précis duquel on veut extraire les données. Peut être nul. Dans ce cas, les données seront celles
        /// de tous les terminaux du questionnaire serveur.
        /// </summary>
        public int? ServerSourceTerminalId { get; set; }

        /// <summary>
        /// Le questionnaire duquel il faut extraire les informations sur le serveur.
        /// </summary>
        public int ServerSourceQuestionaireId { get; set; }

        /// <summary>
        /// Date minimale avec laquelle on va tenter de synchroniser les données sur le serveur. Comme dans beaucoup de
        /// situations, on ne s'intéresse qu'aux valeurs les plus récentes, habituellement dans la dernière journée ou semaine.
        /// Bien sûr, cette classe va quand même donner priorité au images les plus récentes en tout temps, mais il va tout de même tenter de
        /// chercher les éléments plus vieux quand les plus récents sont obtenus.
        /// Cette valeur va également cacher les images reçues, mais trop vieilles pour être considérées.
        /// </summary>
        public DatetimeCalculator ReferenceDate
        {
            get => _dateCalculator;
            set
            {
                // TODO On peut désormais brancher l'événement de changement de date au besoin plutôt que de fe fier aux changements.

                if (value == null)
                {
                    _dateCalculator = new DateTimeCalculatorFixed();
                }

                _dateCalculator = value;
            }
        } 
        
        /// <summary>
        /// Un liste d'identifiants de questions pour le serveur. Le serveur doit renvoyer la liste de réponses (si répondues) associées à cette liste.
        /// </summary>
        public IList<QuestionIdentifier> QuestionFetchIdentifiers { get; set; } = new List<QuestionIdentifier>();
        
        /// <summary>
        /// Identifie la question associée à l'image de la gallerie.
        /// </summary>
        public QuestionIdentifier QuestionIdentifierGalleryImage { get; set; }
        
        protected override bool ExtraInitialization()
        {
            _db = ServicesContainer.GetService<IbrandDatabase>();
            if (_db == null) return false;

            _serverConfig = ServicesContainer.GetService<NomadConfiguration>().CurrentServerConfiguration;
            _authManager = new UserAuthTokenManager(_serverConfig);

            _fileDownloadService = ServicesContainer.GetService<FileDownloadService>();
            if (_fileDownloadService == null)
            {
                // Il faut partir le service
                var serv = new FileDownloadService();
                if (!ServicesContainer.AddService(serv).GetAwaiter().GetResult())
                {
                    return false;
                }
                serv.StartLongRunning();
                _fileDownloadService = ServicesContainer.GetService<FileDownloadService>();
            }

            log.Debug("Gallery initialised for the following reference date: {0}", ReferenceDate.ToString());

            return base.ExtraInitialization();
        }

        protected override void ExtraDispose()
        {
            lock (_currentRequests)
            {
                _cts?.Cancel();
                _fetchData = null;
            
                _currentRequests.Clear();
            }
        }

        protected override Task UpdateEventDo()
        {
            base.UpdateEventDo().GetAwaiter().GetResult();

            // TODO On pourrait séparer l'affichage des images et la fréquence des recherches au besoin. Nécésitte un deuxième timer.

            if (_fetchData != null) return Task.CompletedTask;

            lock (_currentRequests)
            {
                _cts = new CancellationTokenSource();
                _fetchData = Task.Factory.StartNew(() => LaunchFetchData(_cts.Token), _cts.Token, TaskCreationOptions.LongRunning, TaskScheduler.Default);
            }

            return Task.CompletedTask;
        }

        private async Task LaunchFetchData(CancellationToken ct)
        {
            try
            {
                var dateMaxBd = _db.GetHighestDateFromCondensedAnswers(LocalCurrentQuestionaireId);
                var dateMinBd = _db.GetHighestDateFromCondensedAnswers(LocalCurrentQuestionaireId, true);

                // 1- on détermine s'il y a des éléments plus récents.
                if (!_authManager.GetAuthToken(out var authToken))
                {
                    return;
                }

                var qiList = QuestionFetchIdentifiers.Select(qfi => qfi.ToString()).ToList();

                var payload = new RequestQuestionnaireDataPayload
                {
                    QuestionnaireId = ServerSourceQuestionaireId,
                    TerminalId = ServerSourceTerminalId,
                    SearchParameters = new SearchParametersQuestionnaireData
                    {
                        QuestionDefinitions = qiList,
                        MinimalDate = dateMaxBd
                    }
                };

                if (!await ExecGetData(ct, authToken, payload)) return;
                
                // 2- Anciennes Réponse
                payload = new RequestQuestionnaireDataPayload
                {
                    QuestionnaireId = ServerSourceQuestionaireId,
                    TerminalId = ServerSourceTerminalId,
                    ReverseOrderByDate = true,
                    SearchParameters = new SearchParametersQuestionnaireData
                    {
                        QuestionDefinitions = qiList,
                        MinimalDate = _dateCalculator.CalculateDate(),
                        MaximumDate = dateMinBd
                    }
                };

                if (!await ExecGetData(ct, authToken, payload)) return;

                // 3- On lance les demandes de téléchargement.

                // On cherche juste les réponses d'images qui sont encore vides.
                var missingFiles = _db.GetCondensedAnswers(LocalCurrentQuestionaireId, _dateCalculator.CalculateDate(), QuestionIdentifierGalleryImage, false, false, MaxDownloadRequests);

                if (!missingFiles.Any())
                {
                    return;
                }

                var listToAdd = new List<DownloadRequest>();

                lock (_currentRequests)
                {
                    foreach (var mf in missingFiles)
                    {
                        // le format de sauvegarde est "{ticks_date}_{identifiant répondant serveur}_{nom fichier + extension}"
                        var name = StringUtil.GetFileName(mf.Response);
                        var ext = StringUtil.GetFileExtension(name);
                        
                        // Vérification un: pas de nom de répetoire. DOIT contenir une extension.
                        if (string.IsNullOrWhiteSpace(ext))
                        {
                            log.Warn("Illegal file name detected from questionnaire data. Will not try again. CondensedAnswer=" + mf);
                            if (_db.UpdateCondensedAnswerLocalFile(mf.RespondentNumber, mf.GetQuestionIdentifier(), "ILLEGAL FILE") != 1)
                            {
                                log.Error("Could not flag illegal file name in database: " + mf);
                            }
                            continue;
                        }

                        var url = _serverConfig.GetRestUrl(mf.Response);

                        var finalFilePath = new FileInfo(Path.Combine(WorkDirectory.FullName, $"{mf.LocalInsertedTime.Ticks}_{mf.RespondentNumber}_{QuestionIdentifierGalleryImage}_{name}"));
                        var dr = new DownloadRequest(finalFilePath, url, OnFileDownloadSuccess, OnFileDownloadError)
                        {
                            CustomData = mf
                        };
                        if (_currentRequests.Contains(dr)) continue; // Déjà demandée.
                        _currentRequests.Add(dr);
                        listToAdd.Add(dr);
                    }
                }

                _fileDownloadService.AddDownloadRequest(listToAdd);
                log.Trace("Added the following files for download request:\n\t" + StringUtil.FormatEnumerable(listToAdd, true, 1));
            }
            catch (System.Exception e)
            {
                log.Error(e, "Error during data fetching.");
            }
            finally
            {
                lock (_currentRequests)
                {
                    _fetchData = null;
                    _cts = null;
                }
            }
        }

        private async Task<bool> ExecGetData(CancellationToken ct, string authToken, RequestQuestionnaireDataPayload payload)
        {
            var res = await ServerRestRequestsUtil.GetQuestionnaireData(ct, _serverConfig, authToken, payload);
            if (res.Result == ServerRestRequestResultType.Unauthorized)
            {
                log.Trace("Auth token expired. Will retry.");
                _authManager.InvalidateToken();
                return false;
            }
            
            if (res.Result != ServerRestRequestResultType.Success)
            {
                log.Error("Unable to obtain questionnaire data: " + res.MessageUi);
                return false;
            }

            if (ct.IsCancellationRequested) return false;

            var data = res.Data;

            if (data.AnswersNumber < 1)
            {
                return true;
            }

            if (data.Elements == null || !data.Elements.Any())
            {
                log.Error("Invalid dataset: empty list but should have returned data.");
                return false;
            }

            log.Trace("Check success (min date:{1} | max date:{2}): Received {0} lines of data.", 
                data.Elements?.Count, payload.SearchParameters.MinimalDateStr, payload.SearchParameters.MaximumDateStr);
            foreach (var e in data.Elements)
            {
                foreach (var a in e.Answers)
                {
                    _db.InsertCondensedAnswersData(LocalCurrentQuestionaireId, e.RespondentServerId,
                        e.CreationDate, a.QuestionNo, a.ChoiceNo, a.Response);
                }
            }

            return true;
        }

        private void OnFileDownloadError(DownloadRequest obj)
        {
            if (obj.StatusCode == HttpStatusCode.NotFound && obj.CustomData is CondensedAnswer ca )
            {
                // On indique que ce fichier est à oublier.
                var nbr = _db.UpdateCondensedAnswerLocalFile(ca.RespondentNumber, ca.GetQuestionIdentifier(), "NOT FOUND");
                log.Warn("Gallery file to download does not exist on distant server. Will not retry. URL={0}", obj.GetUrl());
                if (nbr == 0)
                {
                    log.Error("No condensed answer could be found for file error callback with following values: " + ca);
                }
            }

            lock (_currentRequests)
            {
                _currentRequests.Remove(obj);
            }
        }

        private void OnFileDownloadSuccess(DownloadRequest obj)
        {
            if (obj.StatusCode == HttpStatusCode.OK && obj.CustomData is CondensedAnswer ca )
            {
                // On met la nouvelle séquence
                var nbr = _db.UpdateCondensedAnswerLocalFile(ca.RespondentNumber, ca.GetQuestionIdentifier(), obj.GetFinalFileLocation().FullName);
                if (nbr == 0)
                {
                    log.Error("No condensed answer could be found for file success callback with following values: " + ca);
                }
                else
                {
                    log.Trace("Gallery file downloaded: {0}", obj.GetFinalFileLocation().FullName);
                }
            }

            lock (_currentRequests)
            {
                _currentRequests.Remove(obj);
            }
        }
    }
}