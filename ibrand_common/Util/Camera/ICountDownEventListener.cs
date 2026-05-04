namespace NomadLogic.Common.Util.Camera
{
    /// <summary>
    /// Interface pour écouter les événements durant la prise de photo.
    /// Aide pour synchroniser d'autres événements ou une interface visuelle.
    /// </summary>
    public interface ICountDownEventListener
    {
        /// <summary>
        /// Méthode appellée quand une seconde d'attente s'est écoulée.
        /// Ne se déclenche pas sur la zéro-ième seconde.
        /// </summary>
        /// <param name="countDownValue">Le nombre de secondes qui reste au décompte.</param>
        void CountDownTimerEvent(int countDownValue);

        /// <summary>
        /// Indique que la ou les caméras sont présentement en train de s'activer.
        /// </summary>
        void CamerasActivateEvent();
    }
}