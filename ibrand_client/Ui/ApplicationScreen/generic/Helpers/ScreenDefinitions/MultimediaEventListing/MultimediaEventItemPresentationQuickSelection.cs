using System.Drawing;
using Newtonsoft.Json;

namespace NomadLogic.iBrandDesktop.Ui.ApplicationScreen.Generic.Helpers.ScreenDefinitions.MultimediaEventListing
{
    /// <summary>
    /// Cette classe indique qu'il faut afficher une image qui, lorsque cliqué dessus, sélectionne tous les éléments de la
    /// rangée d'un coup, sans avoir à cliquer de façon individuelle sur chaque élément.
    /// Est normalement affiché sur la gauche de la rangée de présentation (avant les images).
    /// </summary>
    public class MultimediaEventItemPresentationQuickSelection
    {
        [JsonRequired]
        public Size Size { get; set; }
        [JsonRequired]
        public string SelectionActivatedImageFileName { get; set; }
        [JsonRequired]
        public string SelectionUnselectedImageFileName { get; set; }
        [JsonIgnore]
        public string AssetsDirectory { get; set; }
    }
}