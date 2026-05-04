namespace NomadLogic.Common.Util.Networking.VimeoApi
{
    /// <summary>
    /// Classe d'information JSON sur les options d'images provenant du site web Vimeo. Cette classe représente
    /// une option possible d'une image, et le lien vers cette variation.
    /// </summary>
    public class VimeoPictureSizeInfo
    {
        /// <summary>
        /// Largeur de l'option
        /// </summary>
        public int Width { get; set; }

        /// <summary>
        /// Hauteur de l'option
        /// </summary>
        public int Height { get; set; }

        /// <summary>
        /// Le lien web direct vers cette version de l'image.
        /// </summary>
        public string Link { get; set; }
    }
}