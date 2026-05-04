using System.Diagnostics.CodeAnalysis;
using System.Drawing;
using System.Runtime.Serialization;

namespace NomadLogic.iBrand.Transformation.OpenCV.TransformFromJson
{
    /// <summary>
    /// Informations sur la transformation à utiliser pour une image précise.
    /// </summary>
    [DataContract]
    public class PictureTransform
    {
        /// <summary>
        /// Déplacement en X,Y de l'image.
        /// </summary>
        [DataMember(Name = "offset")]
        public Point Offset { get; set; }

        /// <summary>
        /// Angle de rotation en degrés à effectuer sur l'image.
        /// </summary>
        [DataMember(Name = "angle")]
        public float Angle { get; set; }

        /// <summary>
        /// Le centre de rotation (et de scaling) pour les transformations matricielles.
        /// </summary>
        [DataMember(Name = "rotation_center")]
        public Point RotationCenter { get; set; }

        /// <summary>
        /// Facteur de taille pour l'image.
        /// </summary>
        [DataMember(Name = "scale")]
        public double Scale { get; set; }

        /// <summary>
        /// Largeur finale de l'image.
        /// </summary>
        [DataMember(Name = "bounding_box")]
        public Rectangle FinalBoundingBox { get; set; }

        /// <summary>
        /// Index de création.
        /// </summary>
        [DataMember(Name = "index")]
        public int Index { get; set; }

        /// <summary>
        /// Identifiant du fichier qui a servi à faire le calcul.
        /// </summary>
        [DataMember(Name = "tag")]
        public string Tag { get; set; }

        /// <inheritdoc />
        [SuppressMessage("ReSharper", "CompareOfFloatsByEqualityOperator")]
        public override bool Equals(object o)
        {
            var arg = o as PictureTransform;
            if (arg == null) return false;

            return Offset.Equals(arg.Offset) &&
                Angle == arg.Angle &&
                Scale == arg.Scale &&
                FinalBoundingBox.Equals(arg.FinalBoundingBox) &&
                RotationCenter.Equals(arg.RotationCenter) &&
                Index == arg.Index;
        }

        /// <inheritdoc />
        [SuppressMessage("ReSharper", "NonReadonlyMemberInGetHashCode")]
        public override int GetHashCode()
        {
                return Offset.GetHashCode() ^
                    RotationCenter.GetHashCode() ^
                    Angle.GetHashCode() ^
                    Scale.GetHashCode() ^
                    FinalBoundingBox.GetHashCode() ^
                    Index.GetHashCode();
        }
    }
}
