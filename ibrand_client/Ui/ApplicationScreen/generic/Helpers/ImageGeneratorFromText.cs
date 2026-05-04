using System.Collections.Generic;
using System.Drawing;
using Newtonsoft.Json;
using NomadLogic.Multimedia;

namespace NomadLogic.iBrandDesktop.Ui.ApplicationScreen.Generic.Helpers
{
    /// Générateur avec des chemins absolus. Les fichiers qui ne sont pas gérés par un code de langues sont ajoutés en derniers.
    public class ImageGeneratorFromText : IImageGenerator
    {
        /// <inheritdoc />
        public Bitmap GenerateImage(string langCode)
        {
            if (Texts == null || !Texts.ContainsKey(langCode)) return null;
            var text = Texts[langCode];

            if (CustomStrings != null)
            {
                // ReSharper disable once CoVariantArrayConversion
                text = string.Format(text, CustomStrings);
            }
            
            return ImageUtil.CreateProceduralTextBitmap(ProceduralImageTextInfo, text, Rotation, MaxLength, MultipleLineOffset);
        }
        
        /// Textes groupés par code de langue.
        public Dictionary<string, string> Texts { get; set; }

        /// Règle de création de chaque image, si voulu.
        public ProceduralImageTextInfo ProceduralImageTextInfo { get; set; }

        /// <summary>
        /// Angle en degré de rotation. Aucune rotation si null ou 0. Null par défaut.
        /// </summary>
        public float? Rotation { get; set; }

        /// <summary>
        /// Longueur maximale du texte sur une ligne. Si on dépasse cette valeur (selon le texte, son Font et la taille donné),
        /// on tente de séparer le texte en plusieurs lignes, avec les espaces.
        /// Si null, on ne fait aucune tentative de subdivision du texte. Null par défaut.
        /// </summary>
        public int? MaxLength { get; set; }

        /// <summary>
        /// Dans le cas où un divise en plusieures lignes, on ajoute un extra d'espace entre les lignes avec cette valeur.
        /// </summary>
        public int MultipleLineOffset { get; set; }

        /// <summary>
        /// Si cette valeur est non-nulle lors de la génération de l'image, le texte sélectionnée sera modifié (comme une chaîne à la C++) avec le texte supplémentaire
        /// donné ici.
        /// </summary>
        [JsonIgnore]
        public string[] CustomStrings { get; set; }
    }
}