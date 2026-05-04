using System;
using SlimDX.DirectInput;

namespace NomadLogic.Common.Util.Trigger.LaserBeam
{
    /// <summary>
    /// Représente une manette de jeu très simple, d'ont on ne peut que vérifier si un bouton en particulier est
    /// appuyé une fois initialisé.
    /// <br/>Il faut utiliser la méthode acquire() et poll(), dans cet ordre, pour obtenir une valeur utilisable.
    /// <p/>Ref: http://stackoverflow.com/questions/3929764/taking-input-from-a-joystick-with-c-sharp-net
    /// </summary>
    internal class DetectorGamePad
    {
        // TODO Utiliser SharpDX si possible car on va utiliser plus a fond la librairie de SharpDX à l'avenir.
        private Joystick _pad;
        private JoystickState _state = new JoystickState();
        public Guid Guid { get; set;}
        public string Name { get; set; }

        /// <summary>
        /// Associe cette instance avec un identifiant système de joystick.
        /// <br/>On doit au préalable mettre à jour la propriété "Guid".
        /// </summary>
        public void Acquire(DirectInput dinput)
        {
            Dispose();

            _pad = new Joystick(dinput, Guid);
            _pad.Acquire();
        }

        /// <summary>
        /// Libère toutes les ressources système associées à cette manette.
        /// Ne fait rien si aucune ressource système n'est utilisée.
        /// </summary>
        public void Dispose()
        {
            if (_pad == null) return;
            _pad.Dispose();
            _pad = null;
        }

        /// <summary>
        /// Va chercher les informations actuelles de la manette
        /// </summary>
        /// <returns>Vrai si une mise à jour s'est produite.</returns>
        public bool Poll()
        {
            if (_pad == null) return false;
            // Fait la lecture et vérifie que l'opération s'est bien passée.
            if (_pad.Poll().IsFailure)
            {
                return false;
            }

            // Fait la mise à jour de l'état
            return !_pad.GetCurrentState(ref _state).IsFailure;
        }

        /// <summary>
        /// Retourne les boutons appuyés.
        /// </summary>
        /// <param name="buttonIndex">L'indice du bouton (base zéro)</param>
        /// <returns>Un ensemble des indices des boutons présentement appuyés.</returns>
        public bool IdButtonPressed(int buttonIndex)
        {

            if (_pad == null) {
                return false;
            }

            var but = _state.GetButtons();

            return buttonIndex < but.Length && but[buttonIndex];
        }
    }
}
