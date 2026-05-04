using System;
using NomadLogic.Common.Respondent;

namespace NomadLogic.iBrandDesktop.Ui.ApplicationScreen.Generic.Helpers {

    /// <summary>
    /// Implémentation d'un sélecteur pour une image unique. Quel que soit le répondant, on retourne la même valeur.
    /// Il n'y a pas de "sélection" à proprement parler.
    /// </summary>
    public class ImageSelectorSingle : IImageSelector, IEquatable<ImageSelectorSingle> {
        
        /// Le fichier d'image unique de ce sélecteur.
        public string ImageFile { get; set; }

        /// Constructeur principal.
        public ImageSelectorSingle(string imageFile) {
            ImageFile = imageFile;
        }

        /// Constructeur vide.
        public ImageSelectorSingle() { }

        #region Implementation of IImageSelector

        /// <inheritdoc />
        public string GetImageFrame(IRespondent respondent, int? index = null) {
            return ImageFile;
        }

        #endregion

        public bool Equals(ImageSelectorSingle other)
        {
            if (ReferenceEquals(null, other)) return false;
            return ReferenceEquals(this, other) || string.Equals(ImageFile, other.ImageFile);
        }

        public override bool Equals(object obj)
        {
            if (ReferenceEquals(null, obj)) return false;
            if (ReferenceEquals(this, obj)) return true;
            return obj.GetType() == this.GetType() && Equals((ImageSelectorSingle) obj);
        }

        public override int GetHashCode()
        {
            return (ImageFile != null ? ImageFile.GetHashCode() : 0);
        }
    }
}