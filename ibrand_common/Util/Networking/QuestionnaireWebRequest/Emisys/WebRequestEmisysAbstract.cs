using System.Collections.Generic;
using System.Threading.Tasks;
using NLog;

namespace NomadLogic.Common.Util.Networking.QuestionnaireWebRequest.Emisys
{
    public abstract class WebRequestEmisysAbstract : IWebRequestExecutor
    {
        // ReSharper disable once UnusedMember.Local
        private static readonly Logger log = LogManager.GetCurrentClassLogger();
        protected bool _init;

        /// <summary>
        /// Créé une liste de headers de base avec les info d'authentification de Emisys.
        /// </summary>
        protected IList<RestRequestAdditionalParameters> CommonHeaders => new List<RestRequestAdditionalParameters>
        {
            new RestRequestAdditionalParameters("no-cache", "cache-control", HttpParameterType.Header),
            new RestRequestAdditionalParameters(LoginInfo?.Apikey, "apikey", HttpParameterType.Header)
        };

        /// <summary>
        /// Information d'authentification pour le serveur Emisys
        /// </summary>
        public EmisysApiLoginInfo LoginInfo { get; set; }

        /// Purement pour fin de débogage, indique qu'il ne faut pas tester le service lors de l'initialisation (pour laisser
        /// l'initialisation terminer dans un contexte sans internet.
        public bool DoNotTestEndPointInit { get; set; }

        /// Teste la disponibilité du serveur Emisys avec l'instance.
        public abstract Task<bool> TestRestEndPoint();

        public virtual void Dispose()
        {
            // Rien, toute les méthode sont "stateless" par défaut. Cela peut changer pour les classes d'implémentation.
        }

        public virtual async Task<bool> InitializeExecutor()
        {
            log.Trace("Initializing abstract Emisys executor");
            if (LoginInfo?.Apikey == null) return false;
            if (LoginInfo?.RootApiUrl == null) return false;
            if (LoginInfo?.UserEmail == null) return false;

            // On test le end-point du côté d'Emisys
            _init = DoNotTestEndPointInit || await TestRestEndPoint();

            return _init;
        }

        public int Timeout { get; set; } = 5000;
    }
}