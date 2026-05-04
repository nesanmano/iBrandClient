using System.Collections.Generic;
using System.Linq;
using System.Windows.Forms;
using NomadLogic.Common;
using NomadLogic.Common.Respondent;
using NomadLogic.Common.Ui.NomadScreen;
using NomadLogic.Common.Util;
using NomadLogic.Common.Util.Json;
using NomadLogic.Common.Util.Win32;

namespace NomadLogic.iBrandDesktop.Ui.ApplicationScreen.Generic.Helpers.ScreenDefinitions
{
    public class ScreenDefinitionTextBoxes : IJsonConfig, IExternalFileConfig, IRecyclable, IActivateScreen, ISaveToQuestionnaire, IEvaluateDefinitionsState, IOnLoad
    {
        /// <summary>
        /// Textes destinés à l'usager par défaut pour une boîte de texte.
        /// </summary>
        public static readonly IReadOnlyDictionary<UserErrorEnum, IReadOnlyDictionary<string, string>> DefaultUserMessages;

        static ScreenDefinitionTextBoxes()
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

        public string DefaultConfigFileName { get; } = "screen_textbox_definition.json";

        public string ConfigFileName { get; set; }

        public IList<TextboxDefinition> TextboxDefinitions { get; set; }

        public FontDefinitionInfo DefaultFontDefinition { get; set; }
        
        /// <summary>
        /// Valide l'ensemble des définitions de boîtes de texte pour que leur contenu soit valide pour la suite.
        /// Si le retour est null, aucune opération n'a été faite car il n'y a pas d'élément à vérifier.
        /// Sinon, on retourne un aggrégat des résultats.
        /// Si un seul est incomplèt, on retourne ce résultat. Et pour être valide, il faut que chacun soit valide.
        /// S'il y a un ou plusieurs messages à afficher, ils seront groupé ensemble.
        /// </summary>
        /// <returns>Le résultat des opérations.</returns>
        public ScreenEvaluationResult EvaluateDefinitionsState(string activeLanguage)
        {
            if (TextboxDefinitions == null) return null;

            var data = new List<ScreenEvaluationResult>();

            foreach (var def in TextboxDefinitions)
            {
                if (def.ControlInstance == null)
                {
                    var tmp = def.GetErrorResult(activeLanguage, UserErrorEnum.System, DefaultUserMessages);
                    tmp.Result = EvaluationResult.Incomplete;
                    data.Add(tmp);
                }
                else
                {
                    var text = def.ControlInstance.Text;

                    if (string.IsNullOrWhiteSpace(text))
                    {

                        if (def.Mandatory)
                        {
                            data.Add(def.GetErrorResult(activeLanguage, UserErrorEnum.Mandatory, DefaultUserMessages));
                        }
                        else if (!def.AllowEmpty)
                        {
                            data.Add(def.GetErrorResult(activeLanguage, UserErrorEnum.Empty, DefaultUserMessages));
                        }
                    }
                    else
                    {
                        text = ApplyTextMofication(text, def.Modifications);

                        if (def.Validation != null && !def.Validation.Validate(text))
                        {
                            data.Add(def.GetErrorResult(activeLanguage, UserErrorEnum.Validation, DefaultUserMessages, text));
                        }
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

        /// <summary>
        /// Applique certaines légères modification à du texte.
        /// </summary>
        /// <param name="text">Le texte à modifier.</param>
        /// <param name="mods">La valeur drapeaux de modifications.</param>
        /// <returns>Nul si argument null. Sinon, une nouvelle chaîne modifiée.</returns>
        public static string ApplyTextMofication(string text, TextModificationsEnum mods)
        {
            if (text == null) return null;

            var res = text;

            // ReSharper disable once NonConstantEqualityExpressionHasConstantResult
            if ((mods & TextModificationsEnum.Trim) == TextModificationsEnum.Trim)
            {
                res = res.Trim();
            }

            // Modification exclusives.
            switch (mods)
            {
                case TextModificationsEnum.ToLower:
                    res = res.ToLower();
                    break;
                case TextModificationsEnum.ToUpper:
                    res = res.ToUpper();
                    break;
            }

            return res;
        }

        public void Activate(Control parent, IRespondent respondent)
        {
            var tdList = TextboxDefinitions.Where(td => td.ControlInstance != null).ToList();
            if (!tdList.Any()) return;

            foreach (var def in tdList)
            {
                var text = def.DataModels.First().GetActivationTextValue(respondent);
                if (!string.IsNullOrWhiteSpace(text))
                {
                    parent.SafeCall(() => { def.ControlInstance.Text = text; });
                }
            }

            // Focus sur la première boîte de la liste.
            tdList.First().ControlInstance.Focus();
        }

        public bool Save(ISaveLocalRespondentData saveTo, string activeLanguage)
        {
            var tdList = TextboxDefinitions.Where(td => td.ControlInstance != null).ToList();
            if (!tdList.Any()) return true;

            return tdList.All(td => {
                var text = ApplyTextMofication(td.ControlInstance.Text, td.Modifications);
                return td.SaveToAllModels(saveTo, text);
            });
        }

        public void Recycle(Control parent)
        {
            foreach (var t in TextboxDefinitions.Select(def => def.ControlInstance).Where(t => t != null))
            {
                t.Text = "";
            }
        }
        
        public virtual bool OnLoad(AbstractScreen screen)
        {
            if (screen == null || TextboxDefinitions == null) return false;
            
            var fontToUse = StaticCustomFontRepository.CreateCustomFont(DefaultFontDefinition);

            var tabOrder = 0;
            var aggResult = true;
            foreach (var def in TextboxDefinitions)
            {
                aggResult &= screen.InitTextBox(out var box, def.Position, fontToUse, tabOrder++); // TODO valeur par défaut devrait être revue.
                def.ControlInstance = box;
            }

            return aggResult;
        }
    }
}