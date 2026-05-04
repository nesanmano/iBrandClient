using System.Collections.Generic;
using System.Drawing;
using JetBrains.Annotations;
using NomadLogic.Common.Respondent;
using NomadLogic.Common.Util;
using NomadLogic.Multimedia;

namespace NomadLogic.iBrandDesktop.Ui.ApplicationScreen.Generic.Helpers
{
    /// Générateur avec des chemins absolus. Les fichiers qui ne sont pas gérés par un code de langues sont ajoutés en derniers.
    [PublicAPI]
    public class AnimatedImageSequenceGeneratorBasicFiles : IAnimatedImageSequenceGenerator
    {
        /// <inheritdoc />
        public void GenerateImages(IList<Bitmap> list, string langCode, Questionnaire quest = null, IRespondent respondent = null)
        {
            if (FilePathsLang != null && FilePathsLang.ContainsKey(langCode)) {
                var pathList = FilePathsLang[langCode];
                foreach (var path in pathList) { CreateAndAddBitmap(list, path); }
            }

            if (FilePaths != null) {
                foreach (var path in FilePaths) { CreateAndAddBitmap(list, path); }
            }
        }

        private void CreateAndAddBitmap(ICollection<Bitmap> list, string path)
        {
            var bmp = FileUtil.LoadBitmap(path);
            if (ImageExtensionDefinition.HasValue)
            {
                var tmp = ImageUtil.CreateProceduralExtendedBitmap(bmp, ImageExtensionDefinition.Value);
                bmp.Dispose();
                bmp = tmp;
            }
            list.Add(bmp);
        }

        /// Chemins de fichiers non-affectés par le code de langue.
        public IList<string> FilePaths { get; set; }

        /// Chemins des fichiers à ajouter, groupés par code de langue.
        public Dictionary<string, IList<string>> FilePathsLang { get; set; }

        /// Règle de modification de chaque image, si voulu.
        public ProceduralExtendedImageDefinition? ImageExtensionDefinition { get; set; }
    }
}