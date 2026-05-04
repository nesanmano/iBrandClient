using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using Newtonsoft.Json;
using NomadLogic.Common.Util;
using NomadLogic.Common.Util.DateAndTime;

namespace NomadLogic.Common.Serveur.Structures
{
    public class RequestSyncResponse
    {
        [JsonProperty("date_inserted")]
        public string RespondentCreationDateStr
        {
            get => RespondentCreationDate.ToString(DateUtil.UtcTimezoneFormatString);

            set => RespondentCreationDate = DateTimeOffset.Parse(value).LocalDateTime;
        }

        public DateTime RespondentCreationDate { get; set; } = DateTime.MinValue;

        [JsonProperty("respondent_id")]
        public int RespondentServerId { get; set; }

        [JsonProperty("sync_session_id")]
        public string SyncSessionId { get; set; }

        [JsonProperty("files_to_upload")]
        public IList<RequestSyncFileUpload> FilesToUpload { get; set; }
    }

    [SuppressMessage("ReSharper", "NonReadonlyMemberInGetHashCode")]
    public class RequestSyncFileUpload : IEquatable<RequestSyncFileUpload>
    {
        [JsonProperty("local_file")]
        public string LocalFilePath { get; set; }

        [JsonProperty("question_identifier")]
        public string AnswerIdentifier { get; set; }

        public override string ToString()
        {
            return ClassUtil.ToStringPublicProperties(this);
        }
        
        public bool Equals(RequestSyncFileUpload other)
        {
            if (ReferenceEquals(null, other)) return false;
            if (ReferenceEquals(this, other)) return true;
            return LocalFilePath == other.LocalFilePath && AnswerIdentifier == other.AnswerIdentifier;
        }

        public override bool Equals(object obj)
        {
            if (ReferenceEquals(null, obj)) return false;
            if (ReferenceEquals(this, obj)) return true;
            if (obj.GetType() != this.GetType()) return false;
            return Equals((RequestSyncFileUpload) obj);
        }

        public override int GetHashCode()
        {
            unchecked
            {
                return ((LocalFilePath != null ? LocalFilePath.GetHashCode() : 0) * 397) ^ (AnswerIdentifier != null ? AnswerIdentifier.GetHashCode() : 0);
            }
        }
    }
}