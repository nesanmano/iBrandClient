using System.Collections.Generic;
using System.Drawing;
using System.Windows.Forms;
using JetBrains.Annotations;
using Newtonsoft.Json;
using NLog;
using NomadLogic.Common.Respondent;
using NomadLogic.Common.Ui.NomadScreen;
using NomadLogic.Common.Util;
using NomadLogic.Common.Util.Json;

namespace NomadLogic.iBrandDesktop.Ui.ApplicationScreen.Generic.Helpers.ScreenDefinitions
{
    /// <summary>
    /// Affiche du texte qui peut contenir la valeur texte des réponses du répondant courant.
    /// On peut intégrer ces textes comme de valeurs par jeton, avec un script.
    /// Attention: présentement, les réponses sont fixés au moment ou l'on active la page. Si les réponses change pendant que la page est active,
    /// les valeurs ne seront pas mises à jour. Le texte change aussi sur un changement de langue.
    /// </summary>
    [PublicAPI]
    public class ScreenDefinitionQuestionLabels : IJsonConfig, IExternalFileConfig, IPaint, IActivateScreen, IChangeLanguage, IRecyclable
    {
        private static readonly Logger log = LogManager.GetCurrentClassLogger();
        private string _activateLangCode;
        private IRespondent _respondent;
        private Dictionary<QuestionIdentifier, Answer> _responsesDico;

        public IList<ScreenDefinitionQuestionLabel> LabelDefinitions { get; set; } = new List<ScreenDefinitionQuestionLabel>();
        
        public string DefaultConfigFileName { get; } = "screen_def_question_labels.json";

        public string ConfigFileName { get; set; }

        public void Paint(Graphics g, bool debug = false)
        {
            foreach (var ld in LabelDefinitions)
            {
                // Initialisation des fontes et pinceaux, comme il est super important que ce soit le "thread" UI qui le fasse.
                if (ld.CurrentFont == null)
                {
                    ld.CurrentFont = StaticCustomFontRepository.CreateCustomFont(ld.FontDefinition);
                    ld.CurrentBrush = StaticCustomFontRepository.CreateCustomTextBrush(ld.FontDefinition);
                }

                if (!ld.Position.HasValue || ld.CurrentText == null) continue;

                g.DrawString(ld.CurrentText, ld.CurrentFont, ld.CurrentBrush, ld.Position.Value);
                if (debug) g.DrawRectangle(ScreenUtil.DebugBluePen, 
                    ld.Position.Value.X, ld.Position.Value.Y, ld.Position.Value.Width, ld.Position.Value.Height);
            }
        }

        public void Recycle(Control parent)
        {
            _respondent = null;
            _responsesDico = null;
            foreach (var ld in LabelDefinitions)
            {
                ld.CurrentText = null;
            }
        }

        public void Activate(Control parent, IRespondent respondent)
        {
            if (!(parent is AbstractScreen abs)) return;
            _respondent = respondent;
            _responsesDico = abs.GetCurrentQuestionnaire().GetAnswersDictionary();
            if (respondent.Language != null) _activateLangCode = respondent.Language;
            UpdateTextData();
        }

        private void UpdateTextData()
        {
            if (_respondent == null || _activateLangCode == null) return;
            
            foreach (var ld in LabelDefinitions)
            {
                if (!ld.TokenString.TryGetValue(_activateLangCode, out var token))
                {
                    log.Trace("No string token for current language: {0}", _activateLangCode);
                    continue;
                }
                ld.CurrentText = StringUtil.CreateStringFromTokenizedPatternRespondent(token, _responsesDico, null, _respondent);
            }
        }

        public void ChangeLanguage(AbstractScreen screen, string languageCode)
        {
            if (_activateLangCode?.Equals(languageCode) ?? false) _activateLangCode = languageCode;
            UpdateTextData();
        }

        public void Dispose()
        {
            //
        }

    }

    /// <summary>
    /// Définition individuelle de chaque texte.
    /// </summary>
    [PublicAPI]
    public class ScreenDefinitionQuestionLabel
    {
        [JsonRequired]
        public RectangleF? Position { get; set; }
        
        /// <summary>
        /// Le dictionnaire des textes formatté dans lesquels mettre les valeurs des réponses. Utilise le même format
        /// que les jetons des scripts (ex: "Valeur = {q2c1:float}") pour chercher les valeurs du questionnaire.
        /// Ce dictionnaire est indexé par langue comme clé.
        /// </summary>
        public Dictionary<string, string> TokenString { get; set; }

        public FontDefinitionInfo FontDefinition { get; set; } = new FontDefinitionInfo();

        [JsonIgnore]
        internal string CurrentText { get; set; }

        [JsonIgnore]
        internal Font CurrentFont { get; set; }
        
        [JsonIgnore]
        internal Brush CurrentBrush { get; set; }
    }
}