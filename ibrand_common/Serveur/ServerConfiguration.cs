using JetBrains.Annotations;
using System.Text;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using NomadLogic.Common.Util;

namespace NomadLogic.Common.Serveur
{
    public class ServerConfiguration
    {
        /// <summary>
        /// Identifiant local de l'ordinateur. Si vide, on va chercher le nom de la machine, telle que définie par Windows.
        /// </summary>
        [JsonRequired]
        // ReSharper disable once InconsistentNaming
        public string LocalPPC { get; set; }

        /// <summary>
        /// Le port de communication. Sera toujours ajouté de façon explicite.
        /// </summary>
        public int Port { get; set; } = 443;
        
        /// <summary>
        /// L'adresse (DNS ou IP) sans protocole.
        /// </summary>
        [JsonRequired]
        public string Address { get; set; }

        /// <summary>
        /// User/Pass pour l'usager du serveur capable de fournir un token d'identification.
        /// </summary>
        [JsonRequired]
        public ServerUserIdentifier Identifier { get; set; }

        /// <summary>
        /// Le protocole d'entré (http ou https)
        /// </summary>
        [JsonConverter(typeof(StringEnumConverter))]
        public ServerConfigurationProtocol Protocol { get; set; } = ServerConfigurationProtocol.Https;

        /// <summary>
        /// Créer un url vers l'adresse de cette configuration, plus un ajout de la commande REST à effectuer.
        /// </summary>
        /// <param name="restCommand">La portion à ajouter immédiatement après le nom de domaine.</param>
        /// <returns>Un url complet.</returns>
        public string GetRestUrl([NotNull] string restCommand)
        {
            var sb = new StringBuilder();

            sb.Append(Protocol.ToString().ToLowerInvariant());
            sb.Append("://");
            sb.Append(Address);
            if (Port != 80)
            {
                sb.Append(':');
                sb.Append(Port);
            }

            if (!restCommand.StartsWith("/"))
            {
                sb.Append('/');
            }

            sb.Append(restCommand);

            return sb.ToString();
        }
    }

    [JsonConverter(typeof(StringEnumConverter))]
    public enum ServerConfigurationProtocol
    {
        Http,
        Https
    }

    [JsonConverter(typeof(StringEnumConverter))]
    public enum ServerConfigurationSwitch
    {
        Prod, //0
        Preprod, //1
        Local, //2
        Dev, //3
        Other, //4
        DevAWS, //5
    }

    public class ServerUserIdentifier
    {
        [JsonRequired]
        [JsonProperty("username")]
        public string Username { get; set; }
        
        [JsonRequired]
        [JsonProperty("password")]
        public string Password { get; set; }

        public override string ToString()
        {
            return this.ToStringPublicProperties();
        }
    }
}