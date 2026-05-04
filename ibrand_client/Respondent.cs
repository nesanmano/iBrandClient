using System;
using System.Collections.Generic;
using System.Linq;
using JetBrains.Annotations;
using Newtonsoft.Json;
using NomadLogic.Common;
using NomadLogic.Common.Respondent;
using NomadLogic.Common.Util;
using NomadLogic.Common.Util.DateAndTime;

namespace NomadLogic.iBrandDesktop
{
    [PublicAPI]
    public class Respondent : IRespondent
    {
        // ReSharper disable once UnusedMember.Local
        private string _language;
        
        public Respondent() { }

        /// <summary>
        /// Constructeur par copie. Il s'agit d'une copie profonde, selon les informations contenues dans le répondant original.
        /// </summary>
        /// <param name="original">La référence du répondant original. Ne peut pas être nul.</param>
        /// <param name="onlyMetadata">Si true, ne copie pas les réponses et les groupes dynamiques.</param>
        public Respondent([NotNull] Respondent original, bool onlyMetadata = false)
        {
            IdLong = original.IdLong;
            EventId = original.EventId;
            ProjectId = original.ProjectId;
            QuestionnaireId = original.QuestionnaireId;
            TerminalId = original.TerminalId;
            Representative = original.Representative;
            Language = original.Language;
            DateLocal = original.DateLocal;
            TimeZone = original.TimeZone;
            SynchroId = original.SynchroId;
            IsSynchronized = original.IsSynchronized;
            DeviceTypeId = original.DeviceTypeId;
            DeviceName = original.DeviceName;
            Answers = new List<Answer>();
            DynamicHeaders = new List<DynamicHeader>();
            
            if (onlyMetadata) return;
            foreach (var a in original.Answers)
            {
                Answers.Add(new Answer(a));
            }
            foreach (var dh in original.DynamicHeaders)
            {
                DynamicHeaders.Add(new DynamicHeader(dh));
            }
        }

        /// <summary>
        /// Constructeur par copie d'in interface.
        /// Moins complet que la même classe (dûe aux différences d'implémentation.
        /// </summary>
        public Respondent([NotNull] IRespondent original, bool onlyMetadata = false)
        {
            Id = original.Id;
            EventId = original.EventId;
            ProjectId = original.ProjectId;
            QuestionnaireId = original.QuestionnaireId;
            TerminalId = original.TerminalId;
            Language = original.Language;
            DateLocal = DateUtil.FromJavaTimeToDateTime(original.Date);
            TimeZone = original.TimeZone;
            IsSynchronized = original.IsSynchronized;
            Answers = new List<Answer>();
            DynamicHeaders = new List<DynamicHeader>();
            
            if (onlyMetadata) return;
            foreach (var a in original.Answers)
            {
                Answers.Add(new Answer(a));
            }
            foreach (var dh in original.DynamicHeaders)
            {
                DynamicHeaders.Add(new DynamicHeader(dh));
            }
        }

        [JsonProperty("responses")]
        public List<Answer> Answers { set; get; } = new List<Answer>();

        [JsonProperty("dynamic_headers")] 
        public List<DynamicHeader> DynamicHeaders { get; set; } = new List<DynamicHeader>();

        [Obsolete]
        public string SynchroId { set; get; }

        [Obsolete]
        //Kugan
        public int DeviceTypeId { get; set; } = 1;

        //Kugan
        public int TerminalId { get; set; }

        public string DeviceName { get; set; }

        public int ProjectId { get; set; }

        public int QuestionnaireId { get; set; }

        /// <inheritdoc />
        [Obsolete]
        public int EventId { get; set; }

        public int Id
        {
            get => (int)IdLong;

            set => IdLong = Convert.ToInt64(value);
        }
        
        [Obsolete]
        public long IdLong { get; private set; }
        
        [Obsolete]
        public int IdInt => Convert.ToInt32(IdLong);

        /// <summary>
        /// Utilise le format JAVA (miliseconds since EPOCH) pour symboliser une date.
        /// </summary>
        [Obsolete]
        public string Date => DateUtil.GetStringJavaTimeFromDateTime(DateLocal);

        /// <summary>
        /// Retourne le format local de la date.
        /// </summary>
        public DateTime DateLocal { get; set; } = DateTime.Now;

        public string TimeZone { get; set; }

        public bool IsSynchronized { get; set; }

        public bool IsReadyToSync { get; set; }

        /// <inheritdoc />
        public string Language { get => _language;
            set => _language = value?.Trim();
        }

        [Obsolete]
        public string Representative { get; set; }
        
        public Answer GetAnswer(QuestionIdentifier qi)
        {
            return qi == null ? null : GetAnswer(qi.QuestionNo, qi.ChoiceNo);
        }
        
        public Answer GetAnswer(int questionNo, int choiceNo = 1) {
            return Answers?.FirstOrDefault(a => a.QuestionNo == questionNo && a.ChoiceNo == choiceNo);
        }

        public string GetAnswerText(int questionNo, int choiceNo = 1)
        {
            return (from answer in Answers where 
                    answer.QuestionNo == questionNo && answer.ChoiceNo == choiceNo
                    select answer.Response).FirstOrDefault();
        }

        public string GetAnswerText(QuestionIdentifier qi)
        {
            return qi == null ? null : GetAnswerText(qi.QuestionNo, qi.ChoiceNo);
        }

        public string TerminalDirectory => DirectoryUtil.GetLocalIbrandTerminalDir(ProjectId, QuestionnaireId, TerminalId);
        
        /// <summary>
        /// Dans la situation où un répondant doit utiliser un identifiant, c'est ici qu'on le met.
        /// Cette valeur est habituellement dérivée du questionnaire, et donc n'est pas sauvegardée comme champ.
        /// </summary>
        public string ExternalUniqueIdentifier { get; set; }
        
        public string GetContextDirectory()
        {
            return Constants.APP_DIR;
        }

        public override string ToString()
        {
            return this.ToStringPublicProperties(1);
        }
    }
}
