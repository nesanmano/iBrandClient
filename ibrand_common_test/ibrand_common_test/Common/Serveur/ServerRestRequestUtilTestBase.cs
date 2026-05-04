using System;
using System.Threading;
using System.Threading.Tasks;
using NomadLogic.Common.Serveur;
using NomadLogic.Common.Serveur.Requests;
using NUnit.Framework;

namespace NomadLogic.Test.Common.Serveur
{
    public abstract class ServerRestRequestUtilTestBase : TestBaseNunit
    {
        /// <summary>
        /// Fait une demande de jeton et retourne le jeton, avec assertions que tout s'est bien passé.
        /// </summary>
        /// <returns>Le jeton sous forme de string.</returns>
        // ReSharper disable once NUnit.TestCaseAttributeRequiresExpectedResult
        protected async Task<string> GetAuthorizationToken(ServerConfiguration conf)
        {
            var login = await ServerRestRequestsUtil.Login(conf, CancellationToken.None);
            Assert.AreEqual(ServerRestRequestResultType.Success, login.Result);
            Assert.IsNotNull(login.Data);
            Assert.IsTrue(login.Data.Length > 10); // Test un peu arbitraire. SI on peut tester la validité d'un token on devrait le faire.
            return login.Data;
        }
    }
}