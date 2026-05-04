using System.Drawing;

namespace NomadLogic.Multimedia
{
    /// <summary>
    /// Contient les principales données pour créer une image procéduralement.
    /// On remplit l'espace total avec la couleur donnée en paramètre ou la première couleur totalement opaque trouvée
    /// dans l'image originale.
    /// </summary>
    public struct ProceduralExtendedImageDefinition
    {
        /// <summary>
        /// Constructeur de base, avec image déjà créée.
        /// </summary>
        /// <param name="width">Largeur finale.</param>
        /// <param name="height">Hauteur finale.</param>
        /// <param name="fillColor">Couleur de remplissage, optionel.</param>
        public ProceduralExtendedImageDefinition(int width, int height, Color? fillColor = null)
        {
            Width = width;
            Height = height;
            FillColor = fillColor;
        }
        
        /// <summary>
        /// La couleur de remplissage du fond, en dessous de l'image modifiée.
        /// </summary>
        public Color? FillColor { get; }
        
        /// <summary>
        /// La hauteur finale de la composition.
        /// </summary>
        public int Height { get; }

        /// <summary>
        /// La largeur finale de la composition.
        /// </summary>
        public int Width { get; }
    }
}