using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using JetBrains.Annotations;
using NLog;
using NomadLogic.iBrandDesktop.Services;
using NomadLogic.iBrandDesktop.Util;
using NomadLogic.iBrandDesktop.Util.SubmitAction.BaseClasses;

namespace NomadLogic.iBrandDesktop.Synchro
{
    public class UploadDataLive : IUiSynchro
    {
        private static readonly Logger log = LogManager.GetCurrentClassLogger();

        private delegate void AddUiMessageDelegate(string message, int priority);
        private delegate void AddProgressStepDelegate(int value);
        private delegate void SetProgressMinMaxDelegate(int min, int max);
        private delegate void RemoveProgressStepDelegate();

        private readonly UploadData _uploadData;
        private readonly SyncEvents _syncEvents;
        private List<UiMessageInfo> _uiMessages;
        private Task _waitTask;
        private DateTime _nextSubmitResend = DateTime.Now;
        private readonly IbrandDatabase _db;

        public event OnUploadLiveStoppedDelegate UploadLiveStopped;

        public UploadDataLive(string username, SyncEvents syncEvents, IbrandDatabase db)
        {
            if (db == null || !db.Init())
            {
                throw new System.Exception("Invalid database object given.");
            }

            _db = db;

            _uploadData = new UploadData
            {
                User = username,
                DeleteSynchroWhenFinished = false
            };

            _uploadData.UiMessageEvent += AddUiMessage;
            _uploadData.RespondentUploaded += OnRespondentUploaded;
            _uploadData.ProgressBarMaxMin += SetProgressBarMinMax;
            _uploadData.RespondentProgressBarStep += AddProgressStep;
            
            _syncEvents = syncEvents;
			log.Trace("UploadDataLive instance created.");
        }

        private void OnRespondentUploaded(int repId)
        {
            if (WorkingArea.StatusStrip == null)
                return;

            if (WorkingArea.StatusStrip.InvokeRequired)
            {
                // we were called on a worker thread
                // marshal the call to the user interface thread
                WorkingArea.StatusStrip.Invoke(new RespondentUploaded(OnRespondentUploaded), repId);
                return;
            }
            
            var totalRespondent = _db.GetTotalRespondentsToSync(repId);

            WorkingArea.StatusStrip.RightMessage = Resources.iBrand.TotalRespondentMenu + totalRespondent;
			log.Trace("UploadDataLive OnRespondentUploaded invoked for status strip.");
        }

        private bool HasRespondentsToUpload()
        {
            var projects = _db.GetProjects();

            return projects.Any(project => _db.HasRespondentsToSync(project.Id));
        }

        private bool CheckForUnfinishedSubmitActions()
        {
            if (CurrrentSubmitAction == null || CurrrentSubmitAction.IsStopped()) return false;
            
            var projects = _db.GetProjects();

            return projects.Any(project => _db.GetRespondentsIdNotReadyToSync(project.Id.ToString()).Count > 0);
        }

        /// <summary>
        /// Boucle principale d'exécution, appelé sur l'événement de synchronisation passé lors de la construction de l'instance.
        /// Elle attend qu'un événement se passe, puis l'exécute. Si une erreur survient, on journalise l'exception, et on passe
        /// au prochain.
        /// ATTENTION: Cette méthode ne peut pas s'arrêter d'elle-même si on n'utilise pas un CancellationToken valide.
        /// </summary>
        public void UploadData(CancellationToken ct)
        {
			log.Trace("UploadDataLive upload cycle started.");
            IsWaitingForNewUpload = false;

            while (!ct.IsCancellationRequested)
            {
                try
                {
                    if (HasRespondentsToUpload())
					{
						log.Trace("UploadDataLive - upload cyle: Respondent to upload detected. Starting send off.");
                        _uploadData.Go(ct);
					}
                }
                catch (System.Exception ex)
                {
                    log.Fatal(ex, "Error while uploading data: " + ex.Message);
                    AddUiMessage("Error while uploading data.", 4);
                    _waitTask = new Task(() =>
                    {
                        var targetTime = DateTime.Now;
                        var waitTime = new TimeSpan(0, 0, 2, 0);
                        do
                        {
                            try
                            {
                                Thread.Sleep(500);
                            }
                            catch (System.Exception)
                            {
                                // nope
                            }
                        } while(!ct.IsCancellationRequested && DateTime.Now > targetTime + waitTime);

                        // Une fois le temps passé, on appelle l'événement de nouveau répondant.
                        if (!ct.IsCancellationRequested)
                        {
                            _syncEvents.NewItemEvent.Set();
                            log.Trace("Retry event launched.");
                        }

                        _waitTask = null;
                    });
                    _waitTask.Start();
                    log.Trace("Retry timer launched. Next event in two minutes.");
                }

                RemoveProgressStep();

                //Wait For a new respondent
                IsWaitingForNewUpload = true;
				
				if (!ct.IsCancellationRequested)
				{
					log.Trace("UploadDataLive - upload cyle: Upload finished. Now waiting for new respondent event.");
				}
				
                while (!ct.IsCancellationRequested && IsWaitingForNewUpload)
                {
                    try
                    {
                        if (CheckForUnfinishedSubmitActions())
                        {
                            log.Trace("Unfinished submit actions detected. Launching transformations.");
                            AddSubmitWorkers();
                        }
                    }
                    catch (System.Exception e)
                    {
                        log.Error(e, "Error while re-sending unfinished respondents to current Submit Action");
                    }

                    IsWaitingForNewUpload = !_syncEvents.NewItemEvent.WaitOne(500); // Empêche une attente infinie durant la fermeture de l'application.
                }
				
				if (!ct.IsCancellationRequested && !IsWaitingForNewUpload)
				{
					log.Trace("UploadDataLive - upload cyle: New event for new respondent detected.");
				}
            }

            // On arrête la tâche de ré-essaie, si existante
            if (_waitTask != null) {
                try
                {
                    Task.WaitAll(_waitTask); // Ne prend que 500 milisec pour arrêter.
                } catch (System.Exception) {
                    // Die nice
                }
            }

            _uploadData.DeleteSynchro();
            UploadLiveStopped?.Invoke();

            log.Debug("UploadDataLive upload cycle finished.");
        }

        private void AddSubmitWorkers()
        {
            var now = DateTime.Now;

            if (now < _nextSubmitResend)
            {
                return;
            }

            var tmpCurrentSubmit = CurrrentSubmitAction;
            if (tmpCurrentSubmit == null) return;
            
            var ids = _db.GetRespondentsIdNotReadyToSync(CurrrentSubmitAction.ProjectId.ToString());

            foreach (var id in ids)
            {
                tmpCurrentSubmit.ReSubmitRespondent(id);
            }

            log.Debug("Submit workers re-added. Next try in two minutes.");
            _nextSubmitResend = now.AddMinutes(2);
        }

        /// <summary>
        /// Indique si l'instance n'est pas présentement en train d'attendre qu'un nouveau répondant fasse son apparition.
        /// </summary>
        [PublicAPI]
        public bool IsWaitingForNewUpload
        {
            get; private set;
        }

        [PublicAPI]
        public AbstractSubmitAction CurrrentSubmitAction { get; set; }

        /// <summary>
        /// Affiche un message dans l'espace gauche de la barre de travail de la fenêtre principale. Il ne peut y avoir qu'un message à la fois.
        /// </summary>
        /// <param name="message">Le message a envoyer.</param>
        /// <param name="priority">La priorité du message.</param>
        public void AddUiMessage(string message, int priority)
        {
            if (WorkingArea.StatusStrip == null)
                return;

            if (WorkingArea.StatusStrip.InvokeRequired)
            {
                // we were called on a worker thread
                // marshal the call to the user interface thread
                WorkingArea.StatusStrip.Invoke(new AddUiMessageDelegate(AddUiMessage), message, priority);
                return;
            }

            if (_uiMessages == null)
            {
                _uiMessages = new List<UiMessageInfo>();
                WorkingArea.StatusStrip.Button.Click += NextMessage;
            }

            var uiMessage = new UiMessageInfo(message, priority);

            var index = _uiMessages.FindIndex(x => x.Message == uiMessage.Message);

            if (index > -1)
            {
                // Increase the count on that message, but don't add a new entry in the List.
                _uiMessages[index].Count++;
            }
            else
            {
                index = _uiMessages.Count(uim => uim.Priority > uiMessage.Priority);

                // Find where to insert the message.
                // A higher priority means the message will be shown first.
                // If priorities are equal, the new one will have a higher priority.

                if (index > _uiMessages.Count - 1)
                {
                    _uiMessages.Add(uiMessage);
                }
                else
                {
                    _uiMessages.Insert(index, uiMessage);

                    // Messages with a priority of 0 are too frequent and inconsequential.
                    // They are supposed to represent the current state of the program when everything is going fine.
                    // We don't want too many in our List, so we remove them when another one takes its place.
                    if (_uiMessages[index].Priority == 0)
                    {
                        if (index + 1 >= _uiMessages.Count - 1)
                        {
                            index++;
                            _uiMessages.RemoveAt(index);
                        }
                    }
                }
            }

            ShowUiMessage();

        }

        private void ShowUiMessage()
        {
            if(_uiMessages == null || _uiMessages.Count < 1)
            {
                // No message to show.
                WorkingArea.StatusStrip.LeftMessage = "Ready to upload.";
                WorkingArea.StatusStrip.Button.Visible = false;
                return;
            }

            WorkingArea.StatusStrip.Button.Visible = true;

            WorkingArea.StatusStrip.LeftMessage = _uiMessages[0].Message + (_uiMessages[0].Count > 0 ? "(" + (_uiMessages[0].Count+1) + ")" : "");

        }

        private void NextMessage(object sender, EventArgs e)
        {
            if (_uiMessages.Count > 0)
            {
                _uiMessages.RemoveAt(0);
            }
            ShowUiMessage();
        }

        /// <summary>
        /// Met à jour une nouvelle visualisation de la barre de progression. On assigne les paramètres minimum et maximum, puis
        /// on met la valeur actuelle de la barre au minimum, et finalement on affiche la barre de progression.
        /// </summary>
        /// <param name="min">Valeur entière minimale de la barre.</param>
        /// <param name="max">Valeur entière maximale de la barre.</param>
        public void SetProgressBarMinMax(int min, int max)
        {
            if (WorkingArea.StatusStrip == null)
                return;

            if (WorkingArea.StatusStrip.InvokeRequired)
            {
                // we were called on a worker thread
                // marshal the call to the user interface thread
                WorkingArea.StatusStrip.Invoke(new SetProgressMinMaxDelegate(SetProgressBarMinMax), min, max);
                return;
            }

            WorkingArea.StatusStrip.ProgressBar.Minimum = min;
            WorkingArea.StatusStrip.ProgressBar.Maximum = max;
            WorkingArea.StatusStrip.ProgressBar.Value = min;
            WorkingArea.StatusStrip.ProgressBar.Visible = true;
        }

        /// <summary>
        /// Ajoute une valeur X à la barre de progression. Si on a atteint son maximum, on l'enlève automatiquement du UI.
        /// </summary>
        /// <param name="value">La valeur à ajouter.</param>
        public void AddProgressStep(int value)
        {
            if (WorkingArea.StatusStrip == null)
                return;

            if (WorkingArea.StatusStrip.InvokeRequired)
            {
                // we were called on a worker thread
                // marshal the call to the user interface thread
                WorkingArea.StatusStrip.Invoke(new AddProgressStepDelegate(AddProgressStep), value);
                return;
            }

            WorkingArea.StatusStrip.ProgressBar.Value = WorkingArea.StatusStrip.ProgressBar.Value + value;

            if (WorkingArea.StatusStrip.ProgressBar.Value >= WorkingArea.StatusStrip.ProgressBar.Maximum)
            {
                WorkingArea.StatusStrip.ProgressBar.Visible = false;
            }
        }

        /// <summary>
        /// Enlève la visibilité de la barre de progression, quelque soit la valeur affichée.
        /// </summary>
        private void RemoveProgressStep()
        {
            if (WorkingArea.StatusStrip == null)
                return;

            if (WorkingArea.StatusStrip.InvokeRequired)
            {
                // we were called on a worker thread
                // marshal the call to the user interface thread
                WorkingArea.StatusStrip.Invoke(new RemoveProgressStepDelegate(RemoveProgressStep));
                return;
            }

            WorkingArea.StatusStrip.ProgressBar.Visible = false;
        }
    }
}
