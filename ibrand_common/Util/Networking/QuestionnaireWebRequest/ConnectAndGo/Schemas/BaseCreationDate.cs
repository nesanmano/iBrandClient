using System;
using Newtonsoft.Json;
using NomadLogic.Common.Util.DateAndTime;

namespace NomadLogic.Common.Util.Networking.QuestionnaireWebRequest.ConnectAndGo.Schemas
{
    /// <summary>
    /// Classe de base pour une représentation ENTIÈRE d'une date Java, et d'une propriété C# associé en DateTime.
    /// </summary>
    public abstract class BaseCreationDate : ICreationDate
    {
        public long CreationDateInt { get; set; }
        
        [JsonIgnore]
        public DateTime CreationDate
        {
            get => DateUtil.FromJavaTimeToDateTime(CreationDateInt);

            set => CreationDateInt = DateUtil.GetJavaTimeFromDateTime(value);
        }
    }
}