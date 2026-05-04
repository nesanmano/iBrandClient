using System;
using System.IO;
using System.Linq;
using JetBrains.Annotations;
using NomadLogic.Common.Util;

namespace NomadLogic.Common.Xangle
{
    /// <summary>
    /// Structure pour conserver des éléments détectés de la plate-forme Xangle.
    /// Pour tous les éléments de comparaison et d'égalité, on assume que la Date de chaque transformation est unique (pour garder le tout simple).
    /// </summary>
    [PublicAPI]
    public class XangleItem : IDisposable, IEquatable<XangleItem>, IComparable<XangleItem>
    {
        private FileInfo[] _aligned;
        private FileInfo[] _gif;
        private FileInfo[] _video;
        private FileInfo[] _thumbs;
        private FileInfo _smallThumb;

        /// <summary>
        /// Date de création Xangle, déduite du nom du répertoire.
        /// </summary>
        public DateTime Date { get; internal set; } = DateTime.MinValue;
        /// <summary>
        /// Identifiant unique. Est strictement identique au nom du répertoire.
        /// </summary>
        public string Id { get; internal set; }
        /// <summary>
        /// Chemin complet du répertoire.
        /// </summary>
        public DirectoryInfo FullPath { get; internal set; }
        /// <summary>
        /// Le nombre d'images générées par Xangle.
        /// </summary>
        public int FrameNumber { get; internal set; }

        /// <summary>
        /// Obtient le fichier du vidéo créé par Xangle. Si null, alors il n'y a pas de vidéo dans cet élément.
        /// </summary>
        public FileInfo GetVideoFile()
        {
            return _video?.FirstOrDefault();
        }
        /// <summary>
        /// Obtient les fichiers d'images normalisées.
        /// </summary>
        public FileInfo[] GetNormalizedImageFiles()
        {
            return _aligned;
        }
        
        /// <summary>
        /// Les fichiers générés par Xangle pour représenter le thumbnail du vidéo. Peut être nul malgré la présence d'un vidéo.
        /// Habituellement, un des fichiers contient la mention "small" dans son nom de fichier.
        /// Si vide, aucun fichier de thumbnail n'existe.
        /// </summary>
        /// <returns>Un tableau jamais null, mais qui peut être de taille zéro à N, où N est le nombre de Thumbnail généré.</returns>
        public FileInfo[] GetThumbnails()
        {
            return _thumbs;
        }

        /// <summary>
        /// Retourne le plus petit fichier sur disque des thumbnails générés par Xangle pour cet élément.
        /// Si null, aucun fichier de thumbnail n'existe.
        /// </summary>
        public FileInfo GetSmallThumbnail()
        {
            return _smallThumb;
        }

        /// <summary>
        /// Si existant, retourne le fichier GIF créé par Xangle.
        /// </summary>
        public FileInfo GetGifFile()
        {
            return _gif?.FirstOrDefault();
        }

        internal void SetFilesArray(FileInfo[] alignedFiles, FileInfo[] gifFiles, FileInfo[] videoFiles, FileInfo[] thumbsFiles)
        {
            _aligned = alignedFiles;
            _gif = gifFiles;
            _video = videoFiles;
            _thumbs = thumbsFiles ?? new FileInfo[0];
            _smallThumb = _thumbs?.FirstOrDefault(t => t.Name.Contains("small"));
        }

        public void Dispose()
        {
            _aligned = null;
            _thumbs = null;
            _gif = null;
            _smallThumb = null;
            _video = null;
        }

        #region Comparers and overrides

        public override string ToString()
        {
            return ClassUtil.ToStringPublicProperties(this);
        }

        public override bool Equals(object obj)
        {
            var other = obj as XangleItem;
            return Equals(other);
        }

        public override int GetHashCode()
        {
            return Date.GetHashCode();
        }

        public static bool operator ==(XangleItem left, XangleItem right)
        {
            return Equals(left, right);
        }

        public static bool operator !=(XangleItem left, XangleItem right)
        {
            return !Equals(left, right);
        }

        public static bool operator <(XangleItem left, XangleItem right)
        {
            return (left?.Date ?? DateTime.MinValue) < (right?.Date ?? DateTime.MinValue);
        }

        public static bool operator >(XangleItem left, XangleItem right)
        {
            return (left?.Date ?? DateTime.MinValue) > (right?.Date ?? DateTime.MinValue);
        }

        public static bool operator <=(XangleItem left, XangleItem right)
        {
            return (left?.Date ?? DateTime.MinValue) <= (right?.Date ?? DateTime.MinValue);
        }

        public static bool operator >=(XangleItem left, XangleItem right)
        {
            return (left?.Date ?? DateTime.MinValue) >= (right?.Date ?? DateTime.MinValue);
        }

        public bool Equals(XangleItem other)
        {
            if (other == null) return false;
            return Date.Equals(other.Date);
        }

        public int CompareTo(XangleItem other)
        {
            if (other == null) return 1;
            return Date.CompareTo(other.Date);
        }

        #endregion

        /// <summary>
        /// Retourne le répertoire où se trouvent les fichiers normalisés.
        /// Null si invalide ou aucun fichiers n'existe.
        /// </summary>
        public DirectoryInfo GetNormalizedImagesDirectory()
        {
            return _aligned?.FirstOrDefault()?.Directory;
        }
    }
}