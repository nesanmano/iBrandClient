using System.Drawing;

namespace NomadLogic.iBrandDesktop.Ui.ApplicationScreen.Generic.Helpers.ImageTransforms.Base
{
    public class ImageLayer
    {
        public Point Position { get; set; } = new Point(0,0);
        
        /// <summary>
        /// Indique que l'on utilise le centre de l'image chromatisé comme origine, et non le point en ahut à gauche.
        /// </summary>
        public bool Center { get; set; }
        
        public Size? Scale { get; set; }

        /// <summary>
        /// Si une valeur de Scale existe, Crop indique si on coupe le superflu lorsque la nouvelle taille n'a pas le meme ratio.
        /// Si faux, va creer une distorsion.
        /// </summary>
        public bool Crop { get; set; }

        public bool UseChromaKey { get; set; }

    }
}