using System;
using System.Collections.Generic;
using System.IO;

namespace NomadLogic.Multimedia.Video
{
    /// <summary>
    /// Cette classe indique quelles sont les configurations de base pour une capture vidéo.
    /// Il peut y avoir plusieures sources, et chaque source peut être configurée individuellement.
    /// </summary>
    public class VideoCaptureConfiguration<T> where T : VideoCaptureSourceConfigurationBase
    {
        public List<T> SourcesConfigurations { get; set; }
        
        /// <summary>
        /// Si possible, détermine si on doit créer un vidéo de présentation rapide avec le résultat des captures.
        /// L'implémentation exacte de ce vidéo de présentation peut varier; cette propriété ne fait qu'indiquer que cette opération doit être tentée.
        /// </summary>
        public bool CreatePreviewVideo { get; set; }

        /// <summary>
        /// Si à vrai, tente de sauvegarder les fichiers originaux de la capture à un endroit externe.
        /// </summary>
        public bool DebugSaveOriginals { get; set; }

        /// <summary>
        /// Si on sauvegarde les originaux, on doit spécifier un répertoire de base pour la sauvegarde. Ce répertoire contiendra un répertoire avec une copie
        /// de chaque sources. Si possible, on tentera d'utiliser le même nom de répertoire temporaire que le répertoire de travail.
        /// </summary>
        public DirectoryInfo DebugSaveOriginalBasePath { get; set; }
    }

    public abstract class VideoCaptureSourceConfigurationBase
    {
        /// <summary>
        /// Durée, en seconde fractionnaire, de la capture.
        /// </summary>
        public double VideoLength { get; set; }

        /// <summary>
        /// Nom du fichier vidéo créé par cette opération de capture (sans chemin). Comme chaque capture est faite en isolation (dans un répertoire distinct),
        /// il est possible de conserver un nom commun par source.
        /// Selon l'implémentation, cela devrait inclure l'extension du fichier.
        /// </summary>
        public string VideoName { get; set; }

        /// <summary>
        /// Nom de code de la source, pour meilleure identification. (exemple: 'Vidéo Axis 01, glissade 02')
        /// </summary>
        public string SourceName { get; set; }

        public virtual bool Init(string ffmpegLocation)
        {
            // Rien par défaut.
            return true;
        }

        /// <summary>
        /// Délais supplémentaire pour cette instance de capture.
        /// Cela permet de décaler les captures d'un même événement, à partir du départ initial, PAS entre eux.
        /// </summary>
        public TimeSpan CaptureDelay { get; set; } = TimeSpan.Zero;
    }
}