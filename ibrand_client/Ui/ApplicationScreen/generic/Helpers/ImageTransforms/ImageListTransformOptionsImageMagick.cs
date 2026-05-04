using System;
using System.Collections.Generic;
using JetBrains.Annotations;
using NomadLogic.Common.Condition;
using NomadLogic.Common.Questionnaire;
using NomadLogic.Common.Util;

namespace NomadLogic.iBrandDesktop.Ui.ApplicationScreen.Generic.Helpers.ImageTransforms
{
    [PublicAPI]
    public class ImageListTransformOptionsImageMagick
    {
        /// <summary>
        /// Condition (optionelle) pour l'exécution de ce script.
        /// </summary>
        public ICondition Condition { get; set; }

        /// <summary>
        /// Le nom (sans le chemin) du fichier texte contenant les commandes ImageMagick (avec jetons).
        /// </summary>
        public string CommandFile { get; set; }

        /// <summary>
        /// Version minimale de ImageMagick. Remplace la version minimale visée par défaut dans le manager.
        /// </summary>
        [CanBeNull]
        public VersionInfo MinImageMagickVersion { get; set; }

        /// <summary>
        /// Temps maximal pour la transformation. Par défaut 10 secondes.
        /// </summary>
        public TimeSpan Timeout { get; set; } = new TimeSpan(0,0,0,10);

        /// <summary>
        /// Indique quel identifiant X (clé) du dictionnaire de fichiers résultants doit être sauvegardé de quelle façon (via le modèle de sauvegarde).
        /// </summary>
        public Dictionary<string, DataModelBase> ResultMapSaveModels { get; set; } = new Dictionary<string, DataModelBase>();
    }
}