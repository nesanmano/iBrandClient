using System.Drawing;
using JetBrains.Annotations;
using NomadLogic.Multimedia;

namespace NomadLogic.iBrandDesktop.Ui.ApplicationScreen.Generic.Helpers
{
    /// <summary>
    /// Interface pour un décorateur d'affichage de décompte numérique. Ce qui est affiché varie selon la nature de l'implémentation, mais la
    /// règle générale est que l'on a un décompte préalable, représenté par une valeur supérieure à zéro qui décroira, puis un deuxième
    /// décompte, le principal, suivant le même principe. Une valeur de zéro ou négative ne doit pas être prise en considération pour l'affichage,
    /// ainsi que le variable booléenne de "isRecording", qui doit être vrai pour que l'on affiche le décompte principal. La raison est que ces valeurs
    /// peuvent être différente de zéro mais que l'on bloque l'enregistrement, auquel cas il ne faut plus afficher le décompte.
    /// </summary>
    [PublicAPI]
    public interface ICountdownDecorator
    {
        /// <summary>
        /// La méthode avec laquelle on instancie les images avec texte. Habituellement la classe qui contient la référence de ce décorateur
        /// possède un méthode d'instanciation d'image, mais on peut donner toute méthode qui correspond à la signature au besoin.
        /// </summary>
        GetImageFileDelegate GetImageFileMethod { get; set; }
        
        /// <summary>
        /// La méthode avec laquelle on instancie les images sans texte. Habituellement la classe qui contient la référence de ce décorateur
        /// possède un méthode d'instanciation d'image, mais on peut donner toute méthode qui correspond à la signature au besoin.
        /// </summary>
        GetImageFileDelegate GetImageFileNoneLanguageMethod { get; set; }

        /// <summary>
        /// Méthode d'initialisation. Si on doit remplacer des images, il faut relancer cette méthode. Les instances précédentes seront détruites.
        /// Devrait lancer une exception si l'instance ne possède pas asser d'information pour s'initialiser correctement et être prête
        /// à l'emploi.
        /// </summary>
        /// <param name="bmf">Optionnel, les paramètres de modification d'images.</param>
        void Init(BitmapModificationFilter? bmf);

        /// <summary>
        /// Le point milieu-supérieur de l'emplacement où apparaissent les images de chiffre lors du décompte.
        /// </summary>
        Point? UpperCenterPointNumber { get; set; }

        /// <summary>
        /// Le point milieu-supérieur de l'emplacement où apparaissent l'image du message que l'enregistrement est en cours. Optionnel.
        /// </summary>
        Point? UpperCenterPointMessage { get; set; }

        /// <summary>
        /// Indique le temps maximal du décompte. Utilisé lors de l'initialisation pour générer les images qui seront requises au besoin.
        /// </summary>
        int MaxTimeCountdown { get; set; }

        /// <summary>
        /// SI vrai, ce décorateur n'affiche rien lors de l'enregistrement.
        /// </summary>
        bool DoNotShowWhenRecording { get; set; }

        /// <summary>
        /// Méthode qui peint les éléments visuels à partir d'une référence sur l'événement de peinture habituel.
        /// Ne fait rien si l'initialisation ne s'est pas produite, ou complétée correctement.
        /// </summary>
        /// <param name="graph">La référence vers l'objet de dessein.</param>
        /// <param name="timeBeforeRecord">La valeur du décompte préalable.</param>
        /// <param name="timeToRecord">La valeur du décompte principal.</param>
        /// <param name="isRecording">Si on est bel et bien en train d'enregistrer. Le décompte principal ne s'affichage pas quand faux, quel que soit sa valeur.</param>
        void Draw(Graphics graph, int timeBeforeRecord, int timeToRecord, bool isRecording);
    }
}