using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Windows.Forms;
using JetBrains.Annotations;
using Newtonsoft.Json;
using NLog;
using NomadLogic.Common.Respondent;
using NomadLogic.Common.Ui.NomadScreen;
using NomadLogic.Common.Util;
using NomadLogic.Multimedia.Cli.ImageMagickCLI;

namespace NomadLogic.iBrandDesktop.Ui.ApplicationScreen.Generic.Helpers.ScreenDefinitions
{
    [PublicAPI]
    public class ScreenDefinitionImageCopyList : IOnLoad, IDisposable, IActivateScreen
    {
        public IList<ScreenDefinitionImageCopyBase> ImageCopyDefinitions { get; set; }
        
        public bool OnLoad(AbstractScreen screen)
        {
            return ImageCopyDefinitions.Aggregate(true, (current, defs) => current & (defs?.OnLoad(screen) ?? false));
        }

        public void Dispose()
        {
            foreach (var defs in ImageCopyDefinitions)
            {
                defs?.Dispose();
            }
        }

        public void Activate(Control parent, IRespondent respondent)
        {
            foreach (var defs in ImageCopyDefinitions)
            {
                defs?.Activate(parent, respondent);
            }
        }
    }

    public class ScreenDefinitionImageCopyBase : ScreenDefinitionButtonClicBase
    {
        private static readonly Logger log = LogManager.GetCurrentClassLogger();

        /// <summary>
        /// La taille finale voulue du fichier à copier.
        /// Si le ratio est exactement pareil, on utilisera le fichier original.
        /// Sinon, il y aura élaguage pour préserver le format des pixels (I.E. Il n'y aura pas de "stretching").
        /// Dans la situation où l'orientation est différente, on va garder l'orientation originale en inversant les dimensions.
        /// </summary>
        [JsonRequired]
        public Size DesiredSize { get; set; }
        
        /// <summary>
        /// Valeur de compression de l'encodage. Optionel.
        /// </summary>
        public int? CompressionValue { get; set; }

        /// <summary>
        /// Si on veut un format autre que PNG, spécifier l'extension ici.
        /// </summary>
        public string RequiredFileFormatExtension { get; set; }

        /// <summary>
        /// Répertoire de copie où placer les fichiers à copier.
        /// </summary>
        public DirectoryInfo CopyDirectoryPath { get; set; }
        
        protected override bool ExecuteMainLogic()
        {
            var originalPrintFile = new FileInfo(DataModelSource.GetActivationTextValue(_curResp));

            if (!originalPrintFile.Exists)
            {
                log.Warn("File does not exists: " + originalPrintFile);
                return false;
            }

            // Créer le fichier immédiatement dans le bon répertoire.
            var fileToCopy = ImageMagickCliUtil.ResizeAndCopyImageFile(originalPrintFile, DesiredSize, 
                CopyDirectoryPath, RequiredFileFormatExtension, CompressionValue)?.FullName;

            return fileToCopy != null;
        }

    }
}