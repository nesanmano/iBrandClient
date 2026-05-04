using JetBrains.Annotations;
using Newtonsoft.Json;

namespace NomadLogic.Common.Ui.NomadScreen.ScreenAction
{
    /// <summary>
    /// Interface de base pour une action standard dans un écran.
    /// </summary>
    [PublicAPI]
    [JsonConverter(typeof(ScreenActionConverter))]
    public interface IScreenAction
    {
        /// Indique que si l'action ne se complète pas correctement, il faut stopper le reste des actions et afficher une erreur (si possible).
        bool IsCritical { get; set; }

        /// Indique que si on affiche un message d'erreur UI quand une erreur se produit.
        bool ShowErrorMessage { get; set; }

        /// Message à afficher en cas d'erreur pour cette action. Français
        string ErrorMessageFr { get; set; }

        /// Message à afficher en cas d'erreur pour cette action. Anglais
        string ErrorMessageEn { get; set; }
    }
}