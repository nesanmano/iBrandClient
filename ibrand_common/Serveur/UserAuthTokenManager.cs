using System.Threading;
using System.Threading.Tasks;
using JetBrains.Annotations;
using NLog;
using NomadLogic.Common.Serveur.Requests;

namespace NomadLogic.Common.Serveur
{
    /// <summary>
    /// Cette classe gère un jeton d'utilisation du serveur principal, jusqu'à ce qu'il ne fonctionne plus.
    /// Ces jetons ne devraient pas être constamment regénérés, car ils sont utilisable pour une heure.
    /// Si un jeton fourni ne fonctionne pas, il faut le signaler pour l'instance de cette classe puisse en redemander un autre.
    /// Une instance de cette classe gère un et un seul jeton.
    /// </summary>
    public class UserAuthTokenManager
    {
        private static readonly Logger log = LogManager.GetCurrentClassLogger();

        private readonly object _authLock = new object();
        private string _authToken;
        private readonly ServerConfiguration _serverConf;

        public UserAuthTokenManager([NotNull] ServerConfiguration serverConf)
        {
            _serverConf = serverConf;
        }

        public bool GetAuthToken(out string token)
        {
            lock (_authLock)
            {
                if (_authToken != null)
                {
                    token = _authToken;
                    return true;
                }

                var res = ServerRestRequestsUtil.Login(_serverConf, CancellationToken.None).GetAwaiter().GetResult();
                if (res?.Result != ServerRestRequestResultType.Success)
                {
                    log.Error("Could not fetch token from server.");
                    token = null;
                    return false;
                }

                _authToken = res.Data;
                token = _authToken;
                return true;
            }
        }

        public async Task<string> GetAuthTokenAsync(CancellationToken ct)
        {
            lock (_authLock)
            {
                if (_authToken != null)
                {
                    return _authToken;
                }
            }

            var res = await ServerRestRequestsUtil.Login(_serverConf, ct);
            if (res?.Result != ServerRestRequestResultType.Success)
            {
                log.Error("Could not fetch token from server.");
                return null;
            }

            lock (_authLock)
            {
                _authToken = res.Data;
                return _authToken;
            }
        }

        public void InvalidateToken()
        {
            lock (_authLock)
            {
                _authToken = null;
            }
        }
    }
}