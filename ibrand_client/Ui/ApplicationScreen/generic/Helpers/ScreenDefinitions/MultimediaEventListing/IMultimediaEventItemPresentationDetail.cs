using System;
using System.Drawing;
using JetBrains.Annotations;
using Newtonsoft.Json;
using NomadLogic.Common.Ui.NomadScreen;

namespace NomadLogic.iBrandDesktop.Ui.ApplicationScreen.Generic.Helpers.ScreenDefinitions.MultimediaEventListing
{
    /// <summary>
    /// Cette interface regroupe les information pour présenter des éléments sur un écran pour la sélection d'items de présentation.
    /// Regroupe aussi les informations sur les images à utiliser comme overlay de thumbnail, ainsi que la position des éléments de navigation de
    /// base pour sortir de cet écran.
    /// </summary>
    [PublicAPI]
    [JsonConverter(typeof(MultimediaEventItemPresentationDetailConverter))]
    public interface IMultimediaEventItemPresentationDetail : IDisposable, IInitialisable
    {
        [JsonRequired]
        string BackgroundImageFileName { get; set; }
        
        /// <summary>
        /// Donné automatiquement par le chargement, où doit être donné manuellement dans d'autres circonstance avant utilisation.
        /// </summary>
        [JsonIgnore]
        string AssetsDirectory { get; set; }

        [JsonRequired]
        Rectangle QuitButtonPosition { get; set; }
        
        [JsonRequired]
        string QuitButtonImageFileName { get; set; }

        Color QuitButtonBackColor { get; set; }
    }
}