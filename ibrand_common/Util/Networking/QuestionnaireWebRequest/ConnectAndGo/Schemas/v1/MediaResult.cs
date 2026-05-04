using System;
using System.Collections.Generic;
using System.Globalization;
using JetBrains.Annotations;
using Newtonsoft.Json;
using NomadLogic.Common.Util.DateAndTime;
using NomadLogic.Common.Util.Json;

namespace NomadLogic.Common.Util.Networking.QuestionnaireWebRequest.ConnectAndGo.Schemas.v1
{
    [PublicAPI]
    [JsonConverter(typeof(JsonGenericListConverter))]
    public class MediaResult
    {
        public List<MediaResultItem> ResultItems { get; set; }
    }

    [PublicAPI]
    public class MediaResultItem
    {
        [JsonRequired]
        [JsonProperty("id")]
        public int Id { get; set; }
        
        [JsonRequired]
        [JsonProperty("tenant_id")]
        public int TenantId { get; set; }
        
        [JsonRequired]
        [JsonProperty("wallet_id")]
        public int WalletId { get; set; }
        
        [JsonRequired]
        [JsonProperty("status")]
        // Possible Enum?
        public string Status { get; set; }
        
        [JsonRequired]
        [JsonProperty("url")]
        public string Url { get; set; }
        
        [JsonRequired]
        [JsonProperty("format")]
        public MediaFormatEnum Format { get; set; }
        
        [JsonRequired]
        [JsonProperty("media_source_id")]
        public int MediaSourceId { get; set; }
        
        [JsonRequired]
        [JsonProperty("media_source_name")]
        public string MediaSourceName { get; set; }
        
        [JsonRequired]
        [JsonProperty("media_type")]
        public string MediaType { get; set; }

        [JsonIgnore]
        public DateTime DateCreated
        {
            get => DateTime.ParseExact(DateCreatedStr, DateUtil.Iso8601FormatString, CultureInfo.InvariantCulture);
            set => DateCreatedStr = value.ToString(DateUtil.Iso8601FormatString);
        }
        [JsonRequired]
        [JsonProperty("created_at")]
        public string DateCreatedStr { get; set; }

        [JsonIgnore]
        public DateTime DateUpdated
        {
            get => DateTime.ParseExact(DateUpdatedStr, DateUtil.Iso8601FormatString, CultureInfo.InvariantCulture);
            set => DateUpdatedStr = value.ToString(DateUtil.Iso8601FormatString);
        }
        
        [JsonRequired]
        [JsonProperty("updated_at")]
        public string DateUpdatedStr { get; set; }
    }
}