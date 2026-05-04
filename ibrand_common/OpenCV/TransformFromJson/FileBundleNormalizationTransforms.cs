using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Runtime.Serialization;
using NomadLogic.iBrand.Common.Util;

namespace NomadLogic.iBrand.Transformation.OpenCV.TransformFromJson
{
    /// <summary>
    /// Cette classe englobe les informations nécessaires pour normalizer le focus d'un groupe d'image.
    /// La structure de cette classe a été faite de façon à pouvoir être facilement sérialisée dans un fichier JSON.
    /// </summary>
    [DataContract]
    public class FileBundleNormalizationTransforms
    {
        /// <summary>
        /// Liste des transformations normatives pour chaque image d'un groupe.
        /// </summary>
        [DataMember(Name = "computed_transforms")]
        public List<PictureTransform> PicturesTransform{ get; set; }

        /// <summary>
        /// Paramètre globaux de la normalisation d'un groupe d'image.
        /// </summary>
        [DataMember(Name = "global_parameters")]
        public GlobalParameters GlobalParameters { get; set; }

        /// <inheritdoc />
        public override bool Equals(object o)
        {
            var arg = o as FileBundleNormalizationTransforms;
            if (arg == null) return false;

            return GlobalParameters.Equals(arg.GlobalParameters) && 
                PicturesTransform.SequenceEqual(arg.PicturesTransform);
        }

        /// <inheritdoc />
        [SuppressMessage("ReSharper", "NonReadonlyMemberInGetHashCode")]
        public override int GetHashCode()
        {
            return ((PicturesTransform?.GetHashCode() ?? 0)*397) ^ (GlobalParameters?.GetHashCode() ?? 0);
        }

        /// <summary>
        /// Indique la localisation par défaut du fichier de normalisation. Peut ne pas exister.
        /// Les configurations doivent être actives pour que cette propriété fonctionne.
        /// </summary>
        public static string DefaultTransformsJsonFileLocation => DirectoryUtil.GetLocalApplicationDir() + "FileBundleNormalizationTransforms.json";
    }
}
