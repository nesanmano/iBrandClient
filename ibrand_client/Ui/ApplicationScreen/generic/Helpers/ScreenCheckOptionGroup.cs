using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using NomadLogic.Common.Questionnaire;
using NomadLogic.Common.Util.Math;

namespace NomadLogic.iBrandDesktop.Ui.ApplicationScreen.Generic.Helpers
{
    /// <summary>
    /// Cette classe enveloppe un groupe d'instances de ScreenCheckOption, pour les gérer en groupe.
    /// <br/>Si le groupe doit se comporter comme une question radio, les options sont mutuellement exclusives, et on peut n'avoir aucune réponse.
    /// <br/>Si le groupe se comporte comme un groupe radio, seulement une option peut être sélectionnée, et on ne considère pas les informations
    /// de sous-choix dans les options (si elles existent par erreur). Si aucune option n'existe, la question est considérée comme non-répondue.
    /// <br/>Si le groupe se comporte comme un groupe combo, seule une option peut être sélectionnée, et le sous-choix est considéré dans la réponse.
    ///  Si aucune option n'existe, la question est considérée comme non-répondue.
    /// <br/>Si le groupe se comporte comme une question Check, chaque option représente un numéro de choix différent, qui peut être ou non répondu.
    /// L'information de sous-choix est ignorée, et ce genre de groupement peut générer plusieures réponses (un par choix possible).
    /// La méthode "Verify()" valide que les options référencées sont valides en tant que groupe, c'est à dire qu'elles:
    /// <br/>1- Référencent la même question.
    /// <br/>2- Selon le type de comportement, soit référencent le même choix (combo) ou tous des choix différents (tous les autres).
    /// <br/>3- Si comportement combo, aient toutes des numéros de sous-choix différents.
    /// Les informations individuelle "Mandatory" de chaque option est ignorée en faveur de celle du groupe, sauf dans le cas d'un comportement
    /// Check, qui authorise certains choix à être obligatoires et d'autres non en même temps (rarement utilisé en fait).
    /// </summary>
    [Obsolete]
    public class ScreenCheckOptionGroup
    {
        public enum ScreenCheckOptionGroupType
        {
            /// Nécessite les sous-choix, les options sont mutuellement exclusives. Seul le groupe est considéré comme obligatoire. 2 options minimum,
            Combo,
            /// Les options sont mutuellement exclusives. Seul le groupe est considéré comme obligatoire. 2 options minimum.
            Radio,
            /// Les options peuvent être répondues de façon indépendantes. Chaque option peut être, ou non, obligatoire. 1 option minimum.
            Check
        }

        private readonly List<ScreenCheckOption> _currentSelectedOptions = new List<ScreenCheckOption>();

        /// La liste d'option.
        public IList<ScreenCheckOption> OptionList { get; set; }

        /// Le type de comportement. Check par défaut.
        public ScreenCheckOptionGroupType GroupType { get; set; } = ScreenCheckOptionGroupType.Check;

        /// Indique si le groupe est obligatoire. Peut être ignoré en faveur de la configuration individuelle des options, selon le type de comportement choisi.
        public bool Mandatory { get; set; }

        /// Indique si on a une réponse (minimum) dans les choix possibles.
        public bool IsAnswered => _currentSelectedOptions.Any();

        /// Vérifie si la configuration actuelle du groupe est valide.
        public bool Verify()
        {
            if (OptionList == null || !OptionList.Any()) return false;

            var verifResult = true;

            var questOptionList =  OptionList.Where(o => o.DataModels is DataModelQuestionnaire).ToList();

            if (questOptionList.Any())
            {
                // Check de la même question (semblable pour tous les types).
                var countQid = questOptionList.Select(o => ((DataModelQuestionnaire) o.DataModels).Identifier.QuestionNo).Distinct().Count();
                if (countQid != 1) return false;
                var listCount = questOptionList.Count;
                var countCid = questOptionList.Select(o => ((DataModelQuestionnaire) o.DataModels).Identifier.ChoiceNo).Distinct().Count();

                switch (GroupType)
                {
                    case ScreenCheckOptionGroupType.Combo:
                        verifResult &= listCount < 2;
                        verifResult &= countCid == 1;
                        break;
                    case ScreenCheckOptionGroupType.Radio:
                        verifResult &= listCount < 2;
                        verifResult &= countCid == OptionList.Count;
                        break;
                    case ScreenCheckOptionGroupType.Check:
                        verifResult &= listCount < 1;
                        verifResult &= countCid == OptionList.Count;
                        break;
                    default:
                        verifResult =  false;
                        break;
                }
            }

            
            var dhOptionList =  OptionList.Where(o => o.DataModels is DataModelDynamicHeader).ToList();
            if (dhOptionList.Any())
            {
                // TODO vérifier ce ce serait la logique avec des options de type groupe dynamique ici....
                throw new NotImplementedException();
            }

            return verifResult;
        }

        /// Remet les options à zéro, pour un nouveau répondant. Remet l'état comme si aucune réponse n'existe.
        public void Reset()
        {
            if (OptionList == null) return;

            for (var i = 0; i < OptionList.Count; i++)
            {
                var opt = OptionList[i];
                opt.Recycle();
            }

            _currentSelectedOptions.Clear();
        }

        /// Dessine une image selon l'état du groupe. Peut dessiner l'image plusieures fois selon le type de groupe.
        public void Paint(Graphics eGraphics, Bitmap bmp)
        {
            if (eGraphics == null || bmp == null) return;

            var select = OptionList.Where(o => o.AnswerState).ToList();
            if (!select.Any()) return;

            switch (GroupType)
            {
                // Un seul
                case ScreenCheckOptionGroupType.Combo:
                case ScreenCheckOptionGroupType.Radio:
                    ScreenUtil.DrawImage(eGraphics, bmp, select[0].CheckLocation);
                    break;
                // Multiples possibles.
                case ScreenCheckOptionGroupType.Check:
                    foreach (var opt in select)
                        ScreenUtil.DrawImage(eGraphics, bmp, opt.CheckLocation);
                    break;
                default:
                    return;
            }
        }

        public bool MouseDown(int eX, int eY)
        {
            ScreenCheckOption hitOption = null;
            foreach (var opt in OptionList)
            {
                if (!MathUtil.InArea(eX, eY, opt.Position)) continue;
                hitOption = opt;
                break;
            }

            if (hitOption == null) return false;
            var current = hitOption;
            
            // Si une zone est touchée...
            switch (GroupType)
            {
                case ScreenCheckOptionGroupType.Combo:
                case ScreenCheckOptionGroupType.Radio:
                    if (_currentSelectedOptions.Contains(hitOption)){ return false;}
                    else
                    {
                        Reset();
                        current.AssignInitialValue(true);
                        _currentSelectedOptions.Add(current);
                    }
                    break;
                case ScreenCheckOptionGroupType.Check:
                    if (_currentSelectedOptions.Contains(hitOption))
                    {
                        current.AssignInitialValue(false);
                        _currentSelectedOptions.Remove(current);
                    }
                    else
                    {
                        current.AssignInitialValue(true);
                        _currentSelectedOptions.Add(current);
                    }
                    break;
            }

            return true;
        }
    }
}
