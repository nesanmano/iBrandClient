using System;
using System.Collections.Generic;

namespace NomadLogic.Common.Util.Networking.QuestionnaireWebRequest.Eshots
{
    public class EshotsDataConfiguration
    {
        public DateTime? ConfigurationDate { get; set; }
        public string LastUserName { get; set; }
        public string AccessToken { get; set; }
        public string RefreshToken { get; set; }
        public IList<EshotsEventDay> EventDaysCached { get; set; }
        public bool DontForceRefresh { get; set; }
        public bool AllowAllDays { get; set; }
        public bool AllowPastData { get; set; }
    }
}