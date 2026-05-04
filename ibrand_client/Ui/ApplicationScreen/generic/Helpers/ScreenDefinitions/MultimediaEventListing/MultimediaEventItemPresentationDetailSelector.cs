using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using JetBrains.Annotations;
using Newtonsoft.Json;
using NomadLogic.Common.Util.Math;

namespace NomadLogic.iBrandDesktop.Ui.ApplicationScreen.Generic.Helpers.ScreenDefinitions.MultimediaEventListing
{
    /// <summary>
    /// Version de présentation pour une forme complexe où on peut sélectionner l'état des éléments individuellement, tous en présentant les éléments
    /// de façon groupée. En gros, on présente dans l'écran les éléments d'une seule rangée, en groupe.
    /// Cette classe regroupe seulement les informations de positionnement.
    /// </summary>
    [PublicAPI]
    public class MultimediaEventItemPresentationDetailSelector : MultimediaEventItemPresentationDetailBase
    {
        [JsonIgnore]
        public List<Rectangle> ZoneList { get; private set; }
        
        [JsonRequired]
        public Size GridSize { get; set; }
        
        [JsonRequired]
        public  Rectangle GridZone { get; set; }

        public double GridPaddingRatio { get; set; } = 0.05;
        
        public double GridElementSizeRatio { get; set; } = 1;

        public override bool Init()
        {
            var res = base.Init();

            if (!res) return false;

            ZoneList = MathUtil.CalculatePresentationCoordinatesOfSubElements(
                GridSize,
                GridZone,
                GridPaddingRatio,
                GridElementSizeRatio);

            if (ZoneList != null && ZoneList.Any()) return true;
            
            ZoneList = null;
            return false;
        }
    }
}