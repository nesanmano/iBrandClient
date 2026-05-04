using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using JetBrains.Annotations;
using Newtonsoft.Json;
using NomadLogic.Common.Questionnaire;
using NomadLogic.Common.Respondent;
using NomadLogic.Common.Ui.NomadScreen;
using NomadLogic.Common.Util;

namespace NomadLogic.iBrandDesktop.Ui.ApplicationScreen.Generic.Helpers
{
    [PublicAPI]
    public class ScreenFieldAbstract
    {
        /// <summary>
        /// Position de base dans l'écran.
        /// </summary>
        [JsonRequired]
        public Rectangle Position { get; set; }
        
        /// <summary>
        /// Le type de sauvegarde de ce champ.
        /// </summary>
        [JsonRequired]
        public IList<DataModelBase> DataModels { get; set; }

        /// <summary>
        /// Indique si une réponse doit être obligatoirement et correctement répondue.
        /// </summary>
        public bool Mandatory { get; set; }

        /// <summary>
        /// Liste des messages spécifiques associé à ce contrôle particulier. Remplace les messages par défaut.
        /// </summary>
        public IDictionary<UserErrorEnum, IDictionary<string, string>> UserMessages { get; set; }

        /// <summary>
        /// TODO Utiliser une forme i18n
        /// </summary>
        public string FieldName { get; set; }

        public bool SaveToAllModels(ISaveLocalRespondentData saveTo, string val)
        {
            return DataModels == null || DataModels.Aggregate(true, (current, dm) => current & dm.SaveData(saveTo, val));
        }

        public bool SaveToAllModels(ISaveLocalRespondentData saveTo, bool val)
        {
            return DataModels == null || DataModels.Aggregate(true, (current, dm) => current & dm.SaveData(saveTo, val));
        }

        public bool SaveToAllModels(ISaveLocalRespondentData saveTo, IEnumerable<string> val)
        {
            return DataModels == null || DataModels.Aggregate(true, (current, dm) => current & dm.SaveData(saveTo, val));
        }

        public string ObtainUserMessage([NotNull] ScreenFieldAbstract def, UserErrorEnum messageType, string languageCode, IReadOnlyDictionary<UserErrorEnum, IReadOnlyDictionary<string, string>> defaultMessages)
        {
            IDictionary<string,string> dic = null;
            if (!(def.UserMessages?.TryGetValue(messageType,out dic) ?? false)) return defaultMessages?.GetDefaultValue(messageType)?.GetDefaultValue(languageCode);
            return dic!= null ? dic.GetDefaultValue(languageCode) : defaultMessages?.GetDefaultValue(messageType)?.GetDefaultValue(languageCode);
        }

        public string ObtainUserMessage(UserErrorEnum messageType, string languageCode, IReadOnlyDictionary<UserErrorEnum, IReadOnlyDictionary<string, string>> defaultMessages)
        {
            return ObtainUserMessage(this, messageType, languageCode, defaultMessages);
        }

        public static ScreenEvaluationResult GetErrorResult(ScreenFieldAbstract def, string activeLanguage, UserErrorEnum messageType, IReadOnlyDictionary<UserErrorEnum, IReadOnlyDictionary<string, string>> defaultMessages, string messageToken = null)
        {
            var message = def.ObtainUserMessage(messageType, activeLanguage, defaultMessages);
            if (message != null)
            {
                message = string.Format(message, messageToken);
            }
            return new ScreenEvaluationResult(false)
            {
                UserMessages = new List<string> {message ?? messageToken}
            };
        }

        public ScreenEvaluationResult GetErrorResult(string activeLanguage, UserErrorEnum messageType, IReadOnlyDictionary<UserErrorEnum, IReadOnlyDictionary<string, string>> defaultMessages, string messageToken = null)
        {
            return GetErrorResult(this, activeLanguage, messageType, defaultMessages, messageToken ?? FieldName ?? Position.ToString());
        }
    }

}