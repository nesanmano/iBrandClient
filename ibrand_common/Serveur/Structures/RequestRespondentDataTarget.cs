using System;
using System.Diagnostics.CodeAnalysis;
using Newtonsoft.Json;
using NomadLogic.Common.Util;

namespace NomadLogic.Common.Serveur.Structures
{
    /// <summary>
    /// Est utilisé comme dans les requêtes de "get-respondent-data" et "send-respondent-data"
    /// </summary>
    [SuppressMessage("ReSharper", "NonReadonlyMemberInGetHashCode")]
    public class RequestRespondentDataTarget : IEquatable<RequestRespondentDataTarget>
    {
        /*
            "token": <string>,
            "identifier": <string>,
            "resp_id": <int>,
            "project_id": <int>,
            "questionnaire_id": <int>,
            "terminal_id": <int>
         */

        /// <summary>
        /// Le jeton qui identifie un répondant de façon unique à travers tout le système.
        /// </summary>
        [JsonProperty("token")]
        public string ServerToken { get; set; }

        /// <summary>
        /// Un identifiant externe qui identifie de façon unique un répondant à l'intérieur d'un questionnaire sur le serevur.
        /// </summary>
        [JsonProperty("identifier")]
        public string UniqueExternalIdentifier { get; set; }

        [JsonProperty("resp_id")] public int ServerIdRespondent { get; set; } = -1;
        
        [JsonProperty("project_id")] public int ServerIdProject { get; set; } = -1;

        [JsonProperty("questionnaire_id")] public int ServerIdQuestionnaire { get; set; } = -1;

        [JsonProperty("terminal_id")] public int ServerIdTerminal { get; set; } = -1;

        public override string ToString()
        {
            return this.ToStringPublicProperties();
        }

        #region Equality comparers

        public bool Equals(RequestRespondentDataTarget other)
        {
            if (ReferenceEquals(null, other)) return false;
            if (ReferenceEquals(this, other)) return true;
            return ServerToken == 
                other.ServerToken 
                && UniqueExternalIdentifier == other.UniqueExternalIdentifier 
                && ServerIdRespondent == other.ServerIdRespondent 
                && ServerIdProject == other.ServerIdProject 
                && ServerIdQuestionnaire == other.ServerIdQuestionnaire 
                && ServerIdTerminal == other.ServerIdTerminal;
        }

        public override bool Equals(object obj)
        {
            if (ReferenceEquals(null, obj)) return false;
            if (ReferenceEquals(this, obj)) return true;
            if (obj.GetType() != this.GetType()) return false;
            return Equals((RequestRespondentDataTarget) obj);
        }

        public override int GetHashCode()
        {
            unchecked
            {
                var hashCode = (ServerToken != null ? ServerToken.GetHashCode() : 0);
                hashCode = (hashCode * 397) ^ (UniqueExternalIdentifier != null ? UniqueExternalIdentifier.GetHashCode() : 0);
                hashCode = (hashCode * 397) ^ ServerIdRespondent;
                hashCode = (hashCode * 397) ^ ServerIdProject;
                hashCode = (hashCode * 397) ^ ServerIdQuestionnaire;
                hashCode = (hashCode * 397) ^ ServerIdTerminal;
                return hashCode;
            }
        }

        public static bool operator ==(RequestRespondentDataTarget left, RequestRespondentDataTarget right)
        {
            return Equals(left, right);
        }

        public static bool operator !=(RequestRespondentDataTarget left, RequestRespondentDataTarget right)
        {
            return !Equals(left, right);
        }

        #endregion
    }
}