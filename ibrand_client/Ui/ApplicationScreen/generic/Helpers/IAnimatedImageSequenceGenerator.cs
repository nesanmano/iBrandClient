using System.Collections.Generic;
using System.Drawing;
using NomadLogic.Common.Respondent;

namespace NomadLogic.iBrandDesktop.Ui.ApplicationScreen.Generic.Helpers
{
    /// Interface d'un générateur de séquences d'images.
    public interface IAnimatedImageSequenceGenerator
    {
        /// Popule la liste avec les images générée par la classe. Prend en compte le code de langue dans lequel générer les images.
        void GenerateImages(IList<Bitmap> list, string langCode = null, Questionnaire ques = null, IRespondent respondent = null);
    }
}