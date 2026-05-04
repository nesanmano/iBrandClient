using System.Diagnostics.CodeAnalysis;
using System.Drawing;
using System.Runtime.Serialization;

namespace NomadLogic.iBrand.Transformation.OpenCV.TransformFromJson
{
    /// <summary>
    /// Une coordonnée intrante pour la méthode de normalisation est un rectangle avec un radius de recherche, pour chaque point
    /// d'intérêt. Il faut au moins deux de ces point pour faire une recherche correcte.
    /// </summary>
    [DataContract]
    public class CoordinatesInput
    {
        /// <summary>
        /// La zone d'intérêt à utiliser comme base.
        /// </summary>
        [DataMember(Name = "zone")]
        public Rectangle Zone { get; set; }

        /// <summary>
        /// Le radius en pixel où faire des recherche pour identifier la zone.
        /// </summary>
        [DataMember(Name = "radius")]
        public float Radius { get; set; }

        /// <inheritdoc />
        public override bool Equals(object o)
        {
            var arg = o as CoordinatesInput;
            if (arg == null) return false;

            // ReSharper disable once CompareOfFloatsByEqualityOperator
            return Zone.Equals(arg.Zone) && Radius == arg.Radius;
        }

        /// <inheritdoc />
        public override string ToString()
        {
            return $"[{Zone.X},{Zone.Y},{Zone.Width},{Zone.Height},{Radius}]";
        }

        /// <inheritdoc />
        [SuppressMessage("ReSharper", "NonReadonlyMemberInGetHashCode")]
        public override int GetHashCode()
        {
            return (Zone.GetHashCode() * 13) ^ Radius.GetHashCode();
        }
    }
}