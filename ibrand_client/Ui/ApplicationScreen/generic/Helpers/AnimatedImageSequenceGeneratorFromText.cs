using System.Collections.Generic;
using System.Drawing;
using JetBrains.Annotations;
using NomadLogic.Common.Respondent;
using NomadLogic.Multimedia;

namespace NomadLogic.iBrandDesktop.Ui.ApplicationScreen.Generic.Helpers
{
    /// Générateur avec des chemins absolus. Les fichiers qui ne sont pas gérés par un code de langues sont ajoutés en derniers.
    [PublicAPI]
    public class AnimatedImageSequenceGeneratorFromText : IAnimatedImageSequenceGenerator
    {
        /// <inheritdoc />
        public void GenerateImages(IList<Bitmap> list, string langCode, Questionnaire quest = null, IRespondent respondent = null)
        {
            if (ProceduralImageTextInfo == null) {
                throw new System.Exception("Cannot generate image sequence without a procedural text image definition.");
            }

            if (TextsLang != null && TextsLang.ContainsKey(langCode)) {
                var textList = TextsLang[langCode];
                foreach (var text in textList) { list.Add(ImageUtil.CreateProceduralTextBitmap(ProceduralImageTextInfo, text)); }
            }

            if (Texts != null) {
                foreach (var text in Texts) { list.Add(ImageUtil.CreateProceduralTextBitmap(ProceduralImageTextInfo, text)); }
            }
        }
        
        /// Textes à rendre.
        public IList<string> Texts { get; set; }

        /// Textes groupés par code de langue.
        public Dictionary<string, IList<string>> TextsLang { get; set; }

        /// Règle de création de chaque image, si voulu.
        public ProceduralImageTextInfo ProceduralImageTextInfo { get; set; }
    }
}