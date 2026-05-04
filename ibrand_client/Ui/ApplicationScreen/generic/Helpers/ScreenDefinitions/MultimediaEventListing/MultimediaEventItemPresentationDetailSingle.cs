using System.Drawing;
using JetBrains.Annotations;
using Newtonsoft.Json;

namespace NomadLogic.iBrandDesktop.Ui.ApplicationScreen.Generic.Helpers.ScreenDefinitions.MultimediaEventListing
{
    /// <summary>
    /// Cette présentation détaillée affiche un seul élément d'un groupe à la fois. Permet le plus d'espace possible pour visualiser l'élément.
    /// Est également la seule façon de visualiser un vidéo.
    /// </summary>
    [PublicAPI]
    public class MultimediaEventItemPresentationDetailSingle : MultimediaEventItemPresentationDetailBase
    {
        [JsonRequired]
        public Rectangle PresentationZone { get; set; }
    }
}