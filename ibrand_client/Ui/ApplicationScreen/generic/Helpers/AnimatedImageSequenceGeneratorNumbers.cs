using System.Collections.Generic;
using System.Drawing;
using JetBrains.Annotations;
using NomadLogic.Common.Respondent;
using NomadLogic.Multimedia;

namespace NomadLogic.iBrandDesktop.Ui.ApplicationScreen.Generic.Helpers
{
    /// Génère une liste d'images représentant des nombres entiers, partant de 1 jusqu'au maximum donné.
    /// On peut inverser l'ordre au besoin.
    [PublicAPI]
    public class AnimatedImageSequenceGeneratorNumbers : IAnimatedImageSequenceGenerator
    {
        /// <inheritdoc />
        public void GenerateImages(IList<Bitmap> list, string langCode, Questionnaire quest = null, IRespondent respondent = null)
        {
            if (ReverseOrder) {
                for (var i = MaxValue; i >= 1; i--) { list.Add(ImageUtil.CreateProceduralTextBitmap(ProceduralImageTextInfo, i.ToString())); }
            } else {
                for (var i = 1; i <= MaxValue; i++) { list.Add(ImageUtil.CreateProceduralTextBitmap(ProceduralImageTextInfo, i.ToString())); }
            }
        }

        /// Valeur maximale de la séquence.
        public int MaxValue { get; set; } = 5;

        /// Ordonne les images générée en ordre inverse (plus grand au plus bas).
        public bool ReverseOrder { get; set; }

        /// Règle de création de chaque image, si voulu.
        public ProceduralImageTextInfo ProceduralImageTextInfo { get; set; }
    }
}