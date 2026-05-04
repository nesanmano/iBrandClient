using JetBrains.Annotations;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;

namespace NomadLogic.Common.Util.Networking.QuestionnaireWebRequest
{
    /// <summary>
    /// Classe de strucure qui indique comment sauvegarder certaines propriétés lues d'une requête web REST, via un résultat de classe POCO dé-marshalisé
    /// à des questions d'un questionnaire.
    /// </summary>
    [PublicAPI]
    public class WebExecutorMapper
    {
        /// Le nom de la propriété qui contient la valeur à extraire. Ignore la case. Peut être une séquence de propriétés.
        /// Voir "ClassUtil.FindPriopertyValue()".
        public string PropertyName { get; set; }

        /// Le nom complet, avec namespace, de la classe qui contient l'information. Ignore la case.
        public string TargetClass { get; set; }
        
        /// Indique comment considérer la valeur lue, et comment la sauvegarder dans le questionnaire.
        [JsonConverter(typeof(StringEnumConverter))]
        public WebExecutorMapperSaveMethod SaveMethod { get; set; } = WebExecutorMapperSaveMethod.Text;

        public int QuestionNo { get; set; }
        public int ChoiceNo { get; set; } = 1;

        public override string ToString()
        {
            return ClassUtil.ToStringPublicProperties(this);
        }
    }

    /// <summary>
    /// Énumération des types de sauvegardes possible, selon le type de question qui reçoit l'information.
    /// </summary>
    [PublicAPI]
    public enum WebExecutorMapperSaveMethod
    {
        /// Texte pur, sans sous-choix.
        Text,
        /// Copie le texte, mais au préalable modifie le texte pour être un code téléphone nord-américain valide.
        Phone,
        /// Texte avec événement de changement de langue. Le résultat est soit "fr" ou "en"
        Language,
        /// Si la valeur est non-nulle ou vide, on marque comme étant VRAI
        CheckBoxIfExists,
        /// Tente d'interpréter la valeur comme un booléen, et met le résultat le questionnaire.
        CheckBoxParseValue,
        /// Assigne directement le sous-choix, avec comme valeur texte optionnelle la valeur de la propriété (si non-vide, non-nulle).
        ComboBox,
        /// Tente de lire la valeur comme un entier, et d'utiliser cette valeur entière directement comme indice de choix.
        ComboBoxParseAsIndex,
        /// Combo avec sous-choix numérique, avec événement de changement de langue.
        LanguageComboBoxParseAsIndex,
        /// Choix de langue avec indice pour une question de type Radio
        LanguageRadioParseAsIndex,
        /// Tente de lire la valeur comme un entier, et d'utiliser cette valeur entière comme indice de choix, en incrémentant 1 (comme nos indice commencent à 1).
        ComboBoxParseAsIndexZeroBased
    }
}
