using System.Windows.Forms;
using Newtonsoft.Json;
using NLog;
using NomadLogic.Common.Respondent;
using NomadLogic.Common.Services;
using NomadLogic.Common.Ui.NomadScreen;
using NomadLogic.Common.Util;
using NomadLogic.Common.Util.Json;
using NomadLogic.Common.Util.Networking.QuestionnaireWebRequest.Eshots;
using NomadLogic.Common.Util.Win32;
using NomadLogic.iBrandDesktop.Services;

namespace NomadLogic.iBrandDesktop.Ui.ApplicationScreen.Generic.Helpers.ScreenDefinitions
{
    /// <summary>
    /// Option d'écran pour valider que les configurations de Event Day de Eshots (un de nos partenaire) sont correctements configurées au moment de
    /// la présentation de l'écran.
    /// </summary>
    public class ScreenDefinitionEshotsEventDayTransfer : IOnLoad, IActivateScreen, IJsonConfig, IExternalFileConfig
    {
        private static readonly Logger log = LogManager.GetCurrentClassLogger();

        [JsonRequired]
        public int EventDayIdQuestionNo { get; set; }

        /// <summary>
        /// Indique que si le jour d'événement est manquant dans la configuration, il faut notifier l'usager pour qu'un ambassadeur corrige la situation.
        /// On en peut pas continuer si ce dialogue apparaît.
        /// </summary>
        public bool ShowErrorDialog { get; set; }

        /// <summary>
        /// Si faux (par défaut), on bloque l'écran quand la configuration eShots est manquante. Cela empêche des répondants qui n'ont pas de Event Day de eShots
        /// de continuer dans le processus.
        /// Mettre à TRUE si un Event Day n'est pas requis (principalement pour des tests).
        /// </summary>
        public bool AllowRespondentToContinue { get; set; }

        public string DefaultConfigFileName { get; set; } = "screen_EventDayTransfer_definition.json";

        public string ConfigFileName { get; set; }

        public void Activate(Control parent, IRespondent respondent)
        {
            if (!(parent is AbstractScreen screen))
            {
                log.Warn("Control parent is not of type Abstract Screen");
                return;
            }
            
            // API pas configure?
            // Si on doit sauvegarder le event day ID dans le questionnaire.
            var db = ServicesContainer.GetService<IbrandDatabase>();
            var serviceApi = ServicesContainer.GetService<EshotsApiService>();
            serviceApi = EshotsApiService.InitialInitCheck(serviceApi, db);

            var currentEventDay = serviceApi?.CurrentEventDay;
            var eventDayId = currentEventDay?.EventDayId;

            if (!eventDayId.HasValue)
            {
                log.Error("There is no event day currently defined.");

                if (!ShowErrorDialog) return;

                var btn = AllowRespondentToContinue ? MessageBoxButtons.OKCancel : MessageBoxButtons.OK;
                const string msg1 = "Invalid question number for event day.";
                const string msg2 = "Invalid question number for event day. Do you want to continue?";
                var diagResult = DialogResult.None;
                if (AllowRespondentToContinue)
                {
                    diagResult = MessageBox.Show(screen, msg2, @"iBrand", btn);
                }
                if (!AllowRespondentToContinue)
                {
                    diagResult = MessageBox.Show(screen, msg1, @"iBrand", btn);
                }
                if (diagResult == DialogResult.OK)
                    screen.SafeCall(() => screen.FirstScreen());

                return;
            }

            screen.SaveAnswerToQuestion(new Answer{QuestionNo = EventDayIdQuestionNo, Response = eventDayId.Value.ToString()});

            log.Debug($"Event day ({eventDayId.Value}) saved to question no ({EventDayIdQuestionNo})");
        }

        public bool OnLoad(AbstractScreen screen)
        {
            return EventDayIdQuestionNo > 0;
        }
    }
}
