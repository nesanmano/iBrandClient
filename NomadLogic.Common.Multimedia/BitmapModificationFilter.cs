using System.Collections.Generic;
using System.Linq;

namespace NomadLogic.Multimedia
{
    public struct BitmapModificationFilter
    {
        public BitmapModificationFilter(IEnumerable<ColorSubstitutionFilter> filters, float? sizeScale)
        {
            var list = new List<ColorSubstitutionFilter>();
            if (filters != null)
            {
                list.AddRange(filters);
            }
            ColorFilters = list;
            SizeScale = sizeScale;
        }
        
        public float? SizeScale { get; private set; }

        public IList<ColorSubstitutionFilter> ColorFilters { get; }

        /// <summary>
        /// Vérifie que au moins une transformation de couleur s'applique au groupe auquel appartient l'image.
        /// </summary>
        /// <param name="group"></param>
        /// <returns>True, si au moins une transformation concerne l'image donnée.</returns>
        public bool IsApplicable(ColorSubstitutionFilterGroup group)
        {
            return group == ColorSubstitutionFilterGroup.All || ColorFilters.Any(filter => filter.Group == group || filter.Group == ColorSubstitutionFilterGroup.All);
        }
    }
}