using System.Windows.Forms;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using NomadLogic.Common.Ui.NomadScreen;
using NomadLogic.Common.Util;
using NomadLogic.Common.Util.Json;

namespace NomadLogic.iBrandDesktop.Ui.ApplicationScreen.Generic.Helpers.ScreenDefinitions
{
    public class ScreenDefinitionTimerOptions : IJsonConfig, IExternalFileConfig, IInactiveTimer, IOnLoad
    {
        public string DefaultConfigFileName { get; set; } = "screen_timer.json";
        public string ConfigFileName { get; set; }

        /// <summary>
        /// Action entreprise par le système quand l'événement se déclenche. Par défaut: Cancellation du questionnaire.
        /// </summary>
        [JsonRequired]
        [JsonConverter(typeof(StringEnumConverter))]
        public TimerActionEnum TimerAction { get; set; } = TimerActionEnum.Cancel;

        /// <summary>
        /// Interval de temps, en milisecondes, avant que l'événement se déclenche. Par défaut, 5 minutes (300,000 milisecondes).
        /// </summary>
        public int InactivityTimerInterval { get; set; } = 5 * 60 * 1000;

        public bool OnInactiveTimerEvent(Control screen)
        {
            if (!(screen is AbstractScreen s)) return false;

            switch (TimerAction)
            {
                case TimerActionEnum.End:
                    s.End();
                    break;
                case TimerActionEnum.Next:
                    s.Next();
                    break;
                case TimerActionEnum.Cancel:
                    s.FirstScreen();
                    break;
                default:
                    return false;
            }
            
            return true;
        }

        public bool OnLoad(AbstractScreen screen)
        {
            screen.InactivityTimerInterval = InactivityTimerInterval;
            return true;
        }
    }

    /// <summary>
    /// Actions possible sur événement lié à l'horloge d'inactivité.
    /// </summary>
    public enum TimerActionEnum
    {
        /// <summary>
        /// Terminer le questionnaire, avec validation.
        /// </summary>
        End,
        /// <summary>
        /// Passer au prochain écran.
        /// </summary>
        Next,
        /// <summary>
        /// Annuler le présent questionnaire.
        /// </summary>
        Cancel
    }
}