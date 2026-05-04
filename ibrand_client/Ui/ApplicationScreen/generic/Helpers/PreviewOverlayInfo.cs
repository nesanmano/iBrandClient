using System;
using System.Drawing;
using JetBrains.Annotations;
using Newtonsoft.Json;
using NomadLogic.Common.Condition;
using NomadLogic.Common.Util;
using NomadLogic.Common.Util.Json;

// ReSharper disable NonReadonlyMemberInGetHashCode

namespace NomadLogic.iBrandDesktop.Ui.ApplicationScreen.Generic.Helpers
{
    /// <summary>
    /// Structure pour obtenir et placer une image en superposition sur l'image du live view.
    /// </summary>
    [PublicAPI]
    public class PreviewOverlayInfo : IJsonConfig, IEquatable<PreviewOverlayInfo>
    {
        /// <summary>
        /// Constructeur complet.
        /// </summary>
        /// <param name="fileName">Nom de fichier de l'image à superposer.</param>
        /// <param name="position">Le point de la position haut-gauche de l'image.</param>
        /// <param name="originalConstraint">Si non-nul, cette valeur déplace l'affichage de l'image originale à un rectangle plus petit de la surface originale.
        /// Permet, par exemple, d'utiliser l'image superposée comme cadre par rapport à l'image originale.</param>
        /// <param name="condition">La condition de questionnaire associée à cette image superposée. Utilisée si on doit choisir cette structure par rapport
        /// à l'état du questionnaire.</param>
        public PreviewOverlayInfo(string fileName, Point position, Rectangle? originalConstraint = null, ICondition condition = null)
        {
            FileName = fileName;
            Position = position;
            Condition = condition;
            OriginalConstraint = originalConstraint;
            LoadedImageFile = null;
        }

        /// <summary>
        /// Constructeur vide.
        /// </summary>
        public PreviewOverlayInfo() { }

        public string DefaultConfigFileName => "PreviewOverlayInfo.config";
        
        /// Nom de fichier de l'image à superposer.
        public string FileName { get; set; }
        /// Le point de la position haut-gauche de l'image à superposer.
        public Point Position { get; set; } = new Point(0, 0);
        /// La condition de questionnaire associée à cette image superposée. Utilisée si on doit choisir cette structure par rapport à l'état du questionnaire.
        [JsonConverter(typeof(ConditionJsonConverter))]
        public ICondition Condition { get; set; }
        /// Rectangle d'affichage pour l'image originale. À utiliser si on ne veut pas que l'image originale fasse tout le canevas.
        public Rectangle? OriginalConstraint { get; set; }      
        /// Référence vers le Bitmap chargé en mémoire à partir du nom de fichier. N'est pas créé automatiquement par la structure et n'est utilisé que pour sauver de la mémoire..
        [JsonIgnore]
        public Bitmap LoadedImageFile { get; set; }

        /// <inheritdoc />
        public override string ToString()
        {
            return ClassUtil.ToStringPublicProperties(this, 1);
        }

        public bool Equals(PreviewOverlayInfo other)
        {
            if (ReferenceEquals(null, other)) return false;
            if (ReferenceEquals(this, other)) return true;
            return string.Equals(FileName, other.FileName) && 
                Position.Equals(other.Position) && 
                Equals(Condition, other.Condition) && 
                Equals(OriginalConstraint, other.OriginalConstraint);
        }

        public override bool Equals(object obj)
        {
            if (ReferenceEquals(null, obj)) return false;
            if (ReferenceEquals(this, obj)) return true;
            return obj.GetType() == GetType() && Equals((PreviewOverlayInfo) obj);
        }

        public override int GetHashCode()
        {
            unchecked
            {
                var hashCode = (FileName != null ? FileName.GetHashCode() : 0);
                hashCode = (hashCode * 397) ^ Position.GetHashCode();
                hashCode = (hashCode * 397) ^ (Condition != null ? Condition.GetHashCode() : 0);
                hashCode = (hashCode * 397) ^ OriginalConstraint?.GetHashCode() ?? 0;
                return hashCode;
            }
        }
    }
}
