using System;
using System.Drawing;
using System.Windows.Forms;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using NomadLogic.Common.Ui.NomadScreen;
using NomadLogic.Common.Util;
using static NomadLogic.iBrandDesktop.Ui.ApplicationScreen.Generic.Helpers.StaticCustomFontRepository;

namespace NomadLogic.iBrandDesktop.Ui.ApplicationScreen.Generic.Helpers
{
    /// <summary>
    /// Contient les informations et les méthodes d'aide pour afficher du contenu du questionnaire dans un "Label" de Windows From.
    /// </summary>
    [Obsolete("Utilisez la classe ScreenDefinitionQuestionLabel désormais. Cette classe n'est pas multi-langue.")]
    public class QuestionnaireAnswerLabel : IDisposable, IInitialisable
    {
        /// Définition sérialisable d'un font. Optionnel. Si null, on utilise un Font basique, ajusté selon la taille
        /// de la zone du Label.
        public FontDefinitionInfo FontInfo { get; set; }

        /// Si on veut afficher le contenu du questionnaire selon un token à l'écran.
        public Rectangle? LabelZone;

        /// Requis si on veut afficher le contenu du questionnaire. On utilise un token style "{q1}x{q2:float}"
        public string TokenString;

        /// L'alignement du texte du Label. Par défaut au centre.
        [JsonConverter(typeof(StringEnumConverter))]
        public ContentAlignment LabelAlign { get; set; } = ContentAlignment.MiddleCenter;

        /// Référence vers l'instance du Label, une fois initialisée.
        [JsonIgnore]
        public Label LabelInstance { get; private set; }

        /// Référence vers l'instance du Font, une fois initialisé.
        [JsonIgnore]
        public Font FontInstance { get; private set; }
        
        public bool Init()
        {
            if (LabelZone == null) return false;
            FontInstance = CreateCustomFont(FontInfo);
            LabelInstance = new Label
            {
                Location = LabelZone.Value.Location,
                Size = LabelZone.Value.Size,
                Font = FontInstance,
                BackColor = Color.Transparent,
                ForeColor = FontInfo.Color,
                TextAlign = LabelAlign
                
            };

            return true;
        }

        public void Dispose()
        {
            LabelInstance?.Dispose();
            FontInstance?.Dispose();
        }

        public override string ToString()
        {
            return this.ToStringPublicProperties();
        }
    }
}