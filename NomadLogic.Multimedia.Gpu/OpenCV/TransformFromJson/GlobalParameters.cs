using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Runtime.Serialization;

namespace NomadLogic.Multimedia.Gpu.OpenCV.TransformFromJson
{
    /// <summary>
    /// Information critique globale pour la normalisation d'un groupe d'image.
    /// </summary>
    [DataContract]
    public class GlobalParameters
    {
        /// <summary>
        /// La plus petite valeur de largeur détectée dans les images.
        /// </summary>
        [DataMember(Name = "min_crop")]
        public Point MinCrop { get; set; }

        /// <summary>
        /// Les intrants qui ont été utilisés pour faire la normalisation. Ne sert qu'à fin de validation.
        /// </summary>
        [DataMember(Name = "coordinates_input")]
        public List<CoordinatesInput> Input { get; set; }

        /// <summary>
        /// Indique si on faisait juste le calcul sur les translations, en ignorant les rotations et le redimensionnement.
        /// </summary>
        [DataMember(Name = "only_translation")]
        public bool OnlyTranslation { get; set; }

        /// <inheritdoc />
        public override bool Equals(object o)
        {
            var arg = o as GlobalParameters;
            if (arg == null) return false;

            return MinCrop.Equals(arg.MinCrop) && 
                   // ReSharper disable once AssignNullToNotNullAttribute
                ((Input == null && arg.Input == null) || Input.SequenceEqual(arg.Input)) && 
                OnlyTranslation == arg.OnlyTranslation;
        }

        /// <inheritdoc />
        public override int GetHashCode()
        {
            unchecked
            {
                return (MinCrop.GetHashCode() * 397) ^ Input.GetHashCode();
            }
        }
    }
}
