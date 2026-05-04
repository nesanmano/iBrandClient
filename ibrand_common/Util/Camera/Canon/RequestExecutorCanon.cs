using System;
using System.Threading;
using System.Threading.Tasks;
using Canon.Eos.Framework;
using NLog;

// ReSharper disable StaticMemberInGenericType

namespace NomadLogic.Common.Util.Camera.Canon
{
    internal abstract class RequestExecutorCanon<T> : IDisposable where T : class, ICameraRequest
    {
        private static readonly Logger log = LogManager.GetCurrentClassLogger();
        protected EosCamera[] _cams;
        protected CameraControllerCanon _ccc;
        private readonly CancellationTokenSource _cts;
        private Task _runningTask;
        protected bool _isFinished;

        /// <summary>
        /// Constructeur de base.
        /// </summary>
        /// <param name="cameraControllerCanon">Le contrôleur Canon.</param>
        /// <param name="request">La requête de prise de photo qui gère le comnportement et les callbacks de la prise de photo.</param>
        /// <param name="cancellationTokenSource">Le jeton d'annulation de toute l'opération.</param>
        protected RequestExecutorCanon(CameraControllerCanon cameraControllerCanon, T request, CancellationTokenSource cancellationTokenSource)
        {
            _ccc = cameraControllerCanon;
            _cts = cancellationTokenSource;
            Request = request;
        }

        /// <summary>
        /// La requête de prise de photo.
        /// </summary>
        public T Request { get; private set; }

        /// <summary>
        /// Démarre la tâche asynchrone de prise de photo multiple.
        /// </summary>
        public void StartRequest()
        {
            if (_ccc == null) return;
            _runningTask = Task.Factory.StartNew(() => Execute(_cts.Token), _cts.Token);
        }

        private void Execute(CancellationToken ctsToken)
        {
            try
            {
                DoExecute(ctsToken);
            }
            finally
            {
                _runningTask = null;
            }
        }

        protected abstract void DoExecute(CancellationToken token);

        /// <summary>
        /// Annule la requête asynchrone d'image.
        /// Cette méthode est synchrone et s'assure que la tâche est arrêtée.
        /// </summary>
        public void CancelRequest()
        {
            if (_ccc == null || _cts == null || _runningTask == null) return;

            _cts.Cancel();
            _runningTask?.Wait(1000);
            _runningTask = null;
            log.Trace("Picture request cancelled.");
        }

        public void Dispose()
        {
            if (_runningTask != null)
            {
                CancelRequest();
            }

            _ccc = null;
            _cams = null;
            Request = default(T);
        }
    }
}