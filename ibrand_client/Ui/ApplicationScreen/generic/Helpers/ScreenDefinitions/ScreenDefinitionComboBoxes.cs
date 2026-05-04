using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows.Forms;
using NomadLogic.Common;
using NomadLogic.Common.Questionnaire;
using NomadLogic.Common.Respondent;
using NomadLogic.Common.Ui.NomadScreen;
using NomadLogic.Common.Util;
using NomadLogic.Common.Util.Json;
using NomadLogic.Common.Util.Win32;

namespace NomadLogic.iBrandDesktop.Ui.ApplicationScreen.Generic.Helpers.ScreenDefinitions
{
    /// <summary>
    /// Définition des boîtes à liste déroulantes dans un écran.
    /// </summary>
    public class ScreenDefinitionComboBoxes : IJsonConfig, IExternalFileConfig, IRecyclable, IActivateScreen, ISaveToQuestionnaire, IEvaluateDefinitionsState, IOnLoad, IChangeLanguage
    {
        /// <summary>
        /// Textes destinés à l'usager par défaut pour une boîte de texte.
        /// </summary>
        public static readonly IReadOnlyDictionary<UserErrorEnum, IReadOnlyDictionary<string, string>> DefaultUserMessages;

        static ScreenDefinitionComboBoxes()
        {
            // Initialisation statique.
            // TODO À remplacer par un véritable système i18n

            var dico = new Dictionary<UserErrorEnum, IReadOnlyDictionary<string, string>>
            {
                {UserErrorEnum.Empty, new Dictionary<string, string>
                {
                    {Constants.EN, "The field '{0}' cannot be empty."}, {Constants.FR, "le champ '{0}' ne peut être vide."}
                }}
                ,{UserErrorEnum.Mandatory, new Dictionary<string, string>
                {
                    {Constants.EN, "You must provide a valeur for this field: {0}"}, {Constants.FR, "Vous devez fournir une valeur pour ce champ: {0}"}
                }}
                ,{UserErrorEnum.Validation, new Dictionary<string, string>
                {
                    {Constants.EN, "The value {0} is invalid."}, {Constants.FR, "La valeur {0} est invalide."}
                }}
                ,{UserErrorEnum.System, new Dictionary<string, string>
                {
                    {Constants.EN, "System configuration error. Please contact an administrator."}, {Constants.FR, "Erreur de configuration système. Contactez un administrateur."}
                }}
            };

            DefaultUserMessages = dico;
        }
        
        public string DefaultConfigFileName { get; } = "screen_combobox_definition.json";

        public string ConfigFileName { get; set; }

        /// <summary>
        /// Liste des définitions individuelles.
        /// </summary>
        public IList<ComboboxDefinition> ComboboxDefinitions { get; set; }

        /// <summary>
        /// Fonte d'affichage par défaut des boîtes.
        /// </summary>
        public FontDefinitionInfo DefaultFontDefinition { get; set; }

        public void Recycle(Control parent)
        {
            foreach (var t in ComboboxDefinitions.Select(def => def).Where(t => t.ControlInstance != null))
            {
                if (t.DefaultIndex.HasValue)
                    t.ControlInstance.SelectedIndex = (int)t.DefaultIndex.Value;
                else
                    t.ControlInstance.SelectedIndex = -1;
            }
        }

        public void Activate(Control parent, IRespondent respondent)
        {
            var tdList = ComboboxDefinitions.Where(td => td.ControlInstance != null).ToList();
            if (!tdList.Any()) return;

            foreach (var def in tdList)
            {
                if (def.DataModels is DataModelQuestionnaire dmq)
                {
                    var ans = respondent.GetAnswer(dmq.Identifier);
                    if (ans != null && ans.ChoiceNo > 0)
                    {
                        parent.SafeCall(() => { def.ControlInstance.SelectedIndex = ans.ChoiceNo - 1; });
                    }
                    else if (def.DefaultIndex.HasValue && def.DefaultIndex.Value <= def.ControlInstance.Items.Count)
                    {
                        parent.SafeCall(() => { def.ControlInstance.SelectedIndex = (int) def.DefaultIndex.Value; });
                    }
                }
                else
                {
                    throw new NotImplementedException("Implement combobox logic for dynamic headers.");
                }
            }

            // Focus sur la première boîte de la liste.
            tdList.First().ControlInstance.Focus();
        }

        public bool Save(ISaveLocalRespondentData saveTo, string activeLanguage)
        {
            var tdList = ComboboxDefinitions.Where(td =>
                td.ControlInstance != null && 
                td.ControlInstance.SelectedIndex >= 0).ToList();
            if (!tdList.Any()) return true;

            return tdList.All(td =>
            {
                var (item1, item2) = td.TextValues.GetDefaultValue(activeLanguage)[td.ControlInstance.SelectedIndex];

                if (td.DataModels is DataModelQuestionnaire dmq)
                {

                    var answer = new Answer
                    {
                        QuestionType = QuestionTypeEnum.Combo,
                        QuestionNo = dmq.Identifier.QuestionNo,
                        ChoiceNo = td.ControlInstance.SelectedIndex + 1,
                        Response = item2 ?? item1
                    };
                    return saveTo.SaveAnswerToQuestion(answer);
                }

                // else
                throw new NotImplementedException("Implement save for dynamic header data model option.");
            });
        }

        public ScreenEvaluationResult EvaluateDefinitionsState(string activeLanguage)
        {
            if (ComboboxDefinitions == null) return null;

            var data = new List<ScreenEvaluationResult>();

            foreach (var def in ComboboxDefinitions)
            {
                if (def.ControlInstance == null)
                {
                    var tmp = def.GetErrorResult(activeLanguage, UserErrorEnum.System, DefaultUserMessages);
                    tmp.Result = EvaluationResult.Incomplete;
                    data.Add(tmp);
                }
                else
                {
                    var index = def.ControlInstance.SelectedIndex;

                    if (index != -1) continue;
                    if (def.Mandatory)
                    {
                        data.Add(def.GetErrorResult(activeLanguage, UserErrorEnum.Mandatory, DefaultUserMessages));
                    }
                    else if (!def.AllowEmpty)
                    {
                        data.Add(def.GetErrorResult(activeLanguage, UserErrorEnum.Empty, DefaultUserMessages));
                    }
                }
            }

            var result = data.Any(r => r.Result == EvaluationResult.Incomplete) ? EvaluationResult.Incomplete :
                data.All(r => r.Result == EvaluationResult.Valid) ? EvaluationResult.Valid : EvaluationResult.Invalid;

            return new ScreenEvaluationResult()
            {
                Result = result,
                UserMessages = data.SelectMany(d => d.UserMessages).ToList()
            };
        }

        public bool OnLoad(AbstractScreen screen)
        {
            if (screen == null || ComboboxDefinitions == null) return false;
            
            var fontToUse = StaticCustomFontRepository.CreateCustomFont(DefaultFontDefinition);

            var tabOrder = 0;
            var aggResult = true;
            foreach (var def in ComboboxDefinitions)
            {
                var txtList = def.TextValues.GetDefaultValue(screen.ActiveLanguage).Select(t => (object)t.Item1).ToArray();
                aggResult &= screen.InitComboBox(out var box, def.Position, fontToUse, tabOrder++, txtList);
                def.ControlInstance = box;
            }

            return aggResult;
        }

        public void ChangeLanguage(AbstractScreen screen, string languageCode)
        {
            if (screen == null || ComboboxDefinitions == null || string.IsNullOrWhiteSpace(languageCode)) return;

            foreach (var def in ComboboxDefinitions)
            {
                var txtList = def.TextValues.GetDefaultValue(languageCode).Select(t => (object)t.Item1).ToArray();
                screen.UpdateComboBox(def.ControlInstance, txtList);
            }
        }
    }
}