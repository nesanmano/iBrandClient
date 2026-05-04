using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using JetBrains.Annotations;
using NomadLogic.Common.Ui.NomadScreen;

namespace NomadLogic.Common.Ui
{
    /// <summary>
    /// Cette interface résume les éléments visibles de l'extérieur pour gérer des images en mémoire selon une liste de définition d'image.
    /// Les implémentations de cette interface doivent tenter de 1) Garder les images en mémoire, pour ne pas avoir à relire les fichiers constamment.
    /// 2) Vérifier lors de l'initialisation que toutes les possibilités d'état (I.E. Toute l'énumération d'état) est donnée, pour éviter les probèmes.
    /// Comme il s'agit de valeurs d'image en mémoire vive, il faut s'assurer de détruire correctement ces données, d'où l'implémentation de IDisposable.
    /// </summary>
    [PublicAPI]
    public interface IThumbnailOverlaySelector: IInitialisable, IDisposable
    {
        /// <summary>
        /// Si on veut un fichier spécifique comme présentation quand il n'y a pas de "thumbnail" pré-établit, on le met ici.
        /// Cette image peut-être modifiée par les overlays de type et de sélection, au besoin.
        /// Cette propriété ne donne qu'un nom: on assume qun répertoire de base est fournit autrement aux implémentations.
        /// </summary>
        string ThumbnailDefaultFileName { get; set; }

        /// <summary>
        /// Définitions d'images à utiliser comme superpositions pour les thumbnails, pour certains états.
        /// Les informations de fichiers sont relatif au répertoire de défaut de la classe de présentation.
        /// </summary>
        IDictionary<ThumbnailOverlaySelectorState, ThumbnailOverlaySelectorImageInfo> ThumbnailOverlayImagesDefinitions { get; set; }

        /// <summary>
        /// Contient un méthode générique pour faire des recherches dans le dictionnaire efficacement, et retourner la bonne image
        /// selon l'état de l'implémentation. Une absence de la clé va provoquer une sélection de l'image par défaut via le fichier
        /// référencé par "ThumbnailDefaultFile".
        /// Si ce dernier et nul, on retourne nul. Sinon on retourne l'image en mémoire créée.
        /// </summary>
        /// <param name="key">La clé d'état que l'on veut obtenir.</param>
        /// <returns>Une référence vers l'image, ou  nul si le dictionnaire est nul ou </returns>
        Image GetThumbnailOverlayImage(ThumbnailOverlaySelectorState key);
    }
}