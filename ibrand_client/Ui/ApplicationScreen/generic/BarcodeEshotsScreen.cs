using System;
using System.Drawing;
using System.Threading.Tasks;
using System.Windows.Forms;
using NLog;
using NomadLogic.Common.Respondent;
using NomadLogic.Common.Services;
using NomadLogic.Common.Util.Networking.QuestionnaireWebRequest;
using NomadLogic.Common.Util.Networking.QuestionnaireWebRequest.Eshots;
using NomadLogic.iBrandDesktop.Services;

namespace NomadLogic.iBrandDesktop.Ui.ApplicationScreen.Generic
{
    /// <summary>
    /// Cette classe fait des actions supplémentaires pour vérifier l'état des API avec Eshots, qui requiert des actions personnalisées pour fonctionner.
    /// </summary>
    public class BarcodeEshotsScreen : BarcodeScreen
    {
        private static readonly Logger log = LogManager.GetCurrentClassLogger();

        public int? NameQuestion { get;set;} = null;

        public BarcodeEshotsScreen(string backgroundImgEnSrc, string backgroundImgFrSrc, ScreenConfig sc, Rectangle? textboxLocation = null)
            : base(backgroundImgEnSrc, backgroundImgFrSrc, sc, textboxLocation) { }

        /// <summary>
        /// Force la vérification d'un utilisateur Eshots correct pour poursuivre.
        /// </summary>
        public bool RequireCorrectCustomerId { get; set; }

        /// <summary>
        /// Si non nul, Indique qu'il faut sauvegarder l'identifiant du "event day" courant dans le questionnaire dans le numéro de question approprié.
        /// </summary>
        public int? EventDayIdQuestionNo { get; set; }

        protected override async Task<bool> InitializeFacadeCustom(WebRequestFacade facade)
        {
            if (!await facade.Init()) return false;

            if (facade.WebRequestExecutor is WebRequestEshots wre && wre.EshotsDatabase == null)
            {
                wre.EshotsDatabase = ServicesContainer.GetService<IbrandDatabase>();
            }
            else
            {
                return false;
            }

            return true;
        }

        protected override DialogResult ExecuteFacadeCustomCode(string cardUid, object res, Type returnType)
        {
            // Normalement le type reçu est "EshotsConsumerIdentificationResult"

            if (!(res is EshotsConsumerIdentificationResult resultEshots))
            {
                return DialogResult.Cancel;
            }

            // Teste custom pour le type de retour de eShots ainsi que les vérifications d'état de l'API
            if (resultEshots.ExecutionResult == EshotsComplexRequestResultEnum.Success)
            {
                // Deux choix: Contient des données, ou ne contient rien. Les deux retournent OK, mais aucune donnée implique
                // aucun consomateur Eshots associé.
                if (!RequireCorrectCustomerId || resultEshots.ConsumerData != null)
                {
                    if (!EventDayIdQuestionNo.HasValue) return DialogResult.OK;

                    // Si on doit sauvegarder le event day ID dans le questionnaire.
                    var eventDayId = ServicesContainer.GetService<EshotsApiService>()?.CurrentEventDay?.EventDayId;

                    if (!eventDayId.HasValue)
                    {
                        log.Error("There is no event day currently defined.");
                        return DialogResult.Abort;
                    }

                    SaveAnswerToQuestion(new Answer {QuestionNo = EventDayIdQuestionNo.Value, Response = eventDayId.Value.ToString()});

                    // TODO option pour combiner firstName et lastName
                    if (NameQuestion.HasValue)
                    {
                        SaveAnswerToQuestion(new Answer
                        {
                            QuestionNo = NameQuestion.Value, 
                            Response = resultEshots.ConsumerData.FirstName + " " + resultEshots.ConsumerData.LastName
                        });
                    }

                    return DialogResult.OK;
                }

                var resultNoItem = MessageBox.Show(@"There is no consumer associated with that identifier.", 
                    @"No consumer associated.", 
                    MessageBoxButtons.AbortRetryIgnore, MessageBoxIcon.Exclamation);

                return resultNoItem;
            }

            string message;
            MessageBoxButtons buttons;
            // Si ce n'est pas un succès on analyse le résultat et on pose une question à l'usager.
            switch (resultEshots.ExecutionResult)
            {
                case EshotsComplexRequestResultEnum.InternetProblem:
                    message = "Communication problem. This may be a timeout. Please try again.";
                    buttons = MessageBoxButtons.RetryCancel;
                    break;
                case EshotsComplexRequestResultEnum.InvalidDay:
                    message = "The current selected event day is incorrect. Cannot continue. Please correct this in the configurations.";
                    buttons = MessageBoxButtons.AbortRetryIgnore;
                    break;
                case EshotsComplexRequestResultEnum.RequireLogin:
                    message = "Incorrect event and/or event day selected. You must log in the Eshots server to correct this. Please correct this in the configurations.";
                    buttons = MessageBoxButtons.AbortRetryIgnore;
                    break;
                default:
                    message = $"Error detected ({resultEshots.ExecutionResult}). Please contact a representative.";
                    buttons = MessageBoxButtons.AbortRetryIgnore;
                    log.Warn($"Unusual error detected. Returned {resultEshots.ExecutionResult}");
                    break;
            }

            var result = MessageBox.Show(message, @"Could not fetch data from server.", buttons, MessageBoxIcon.Exclamation);

            return result;
        }
    }
}