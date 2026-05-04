using NLog;
using NomadLogic.Common.Services;
using NomadLogic.Common.Ui.Dialog;
using NomadLogic.Common.Ui.Keyboard;
using NomadLogic.iBrandDesktop.Services;
using System;
using System.Drawing;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;
using NomadLogic.Common;
using NomadLogic.Common.Util;
using NomadLogic.Common.Util.Networking.QuestionnaireWebRequest;
using NomadLogic.Common.Util.Networking.QuestionnaireWebRequest.Eshots;
using NomadLogic.Common.Util.Win32;
using NomadLogic.iBrandDesktop.Ui.ApplicationScreen.Generic.Helpers;
#pragma warning disable 618

namespace NomadLogic.PrintStation.Ui.custom.Brewers
{
    class EshotsIdentiferVerificationScreen : EshotsApi
    {
        // ReSharper disable once UnusedMember.Local
        private static readonly Logger log = LogManager.GetCurrentClassLogger();
        private readonly Rectangle _nextArea;

        private TextBox _identifierTextbox;
        private bool _block;
        private bool _showError; // TODO À utiliser avec une image
        private Task _nextAction;
        private CancellationTokenSource _cts;

        public string IdentierType { get; set; }

        public int? SaveIdentiferToQuestion { get;set;}

        public int? NameQuestion { get;set;}

        public int SaveGallerySelectedImageToQuestion { get; set; }

        public Rectangle? CancelZone { get; set; }

        /// Quand on fait une validation automatique, on va accéder à cette liste pour l'opération de changement d'écran.
        /// Par défaut: Next(1)
        public ScreenChangeOptions OnAutoScreenChange { get; set; } = ScreenChangeOptions.GenerateBasicNextOptions();

        /// Plus une option de débogage, quand on ne détecte aucune option de changement valide, on annule le répondant.
        /// Cela représente un cas d'état invalide (il manque des informations dans le questionnaire). 
        public bool OnMissingScreenChangeDoCancel { get; set; }

        protected override void OnLoad()
        {
            LoadEshotsApiConfig();
        }

        private async void LoadEshotsApiConfig()
        {
            if (!await InitializeFacade())
            {
                MsgBox.Show(this, "Configuration file was not found. Contact system admin.","Printstation",
                    MessageBoxButtons.OK, MessageBoxIcon.Warning);
            }
        }

        public EshotsIdentiferVerificationScreen(string bkgEn, string bkgFr, PrintStationScreenConfig sc)
            : base(bkgEn, bkgFr, sc)
        {
            AddTextBoxs();
            AddKeyboard();

            _nextArea = new Rectangle(584, 878, 520, 106);
        }

        public override void Recycle()
        {
            base.Recycle();

           this.SafeCall(() =>
           {
               _identifierTextbox.Text = "";
               _identifierTextbox.ForeColor = Color.Black;
           });
          
        }

        private void AddKeyboard()
        {
            var kb = new KeyboardEmail(-1, 344, 1600, 400)
            {
                CustomLabelInfo = new CustomPaintLabelInfo
                {
                    Font = new Font(FontFamily.GenericSansSerif, 18, FontStyle.Bold)
                }
            };
            AddKeyboardToControls(kb);
        }

        private void AddTextBoxs()
        {
            this.InitTextBox(out _identifierTextbox, new Rectangle(374, 235, 1010, 78), new Font(FontFamily.GenericSansSerif, 20));
        }


        protected override bool OnMouseDownHandler(int x, int y)
        {
            if (InArea(x, y, _nextArea))
            {
                if ( !_block)
                {
                    this.SafeCall(() => CheckCodeEvent(true));
                }
                return false;
            }
            
            if (InArea(x, y, CancelZone))
            {
                _cts?.Cancel();
                _cts = null;
                _nextAction = null;
                Close();
                return false;
            }

            return true;
        }
        
        protected async void CheckCodeEvent(bool nextAfterCheck)
        {
            if (_identifierTextbox == null) return;

            try
            {
                var code = _identifierTextbox.Text?.Trim() ?? "";
                _block = true;
                if (!PreValidationCheck(code))
                {
                    MessageBox.Show(@"You must enter a valid email to continue.", @"iBrand");
                    return;
                }

                if (!SaveIdentiferToQuestion.HasValue)
                {
                    MessageBox.Show(@"Insufficent configuration. You must provide a question number to save the code to.", @"iBrand");
                    return;
                }
                //SaveAnswerToQuestion(SaveIdentiferToQuestion.Value, 1, code);

                var valid = await OnValidateAction(code);
                log.Trace("Code validation after action: {0}", valid);

                _identifierTextbox.ForeColor = valid ? Color.Black : Color.Red;

                if (valid)
                {
                    _showError = false;
                    if (nextAfterCheck)
                    {
                        _cts = new CancellationTokenSource();
                        _nextAction = new Task(() =>
                        {
                            try
                            {
                                Thread.Sleep(AutomaticNextDelay);
                            }
                            finally
                            {
                                // Si next action est mise à null avant qu'on arrive ici, c'est qu'il ne faut pas l'exécuter (dans le cas d'un timeout ou cancellation par exemple).
                                if (_nextAction != null)
                                {
                                    var resp = GetCurrentRespondentWithAnswers();
                                    log.Trace("Evaluating respondent for screen change: resp={0}", resp);
                                    var sco = OnAutoScreenChange?.Evaluate(resp);
                                    if (sco == null)
                                    {
                                        if (OnMissingScreenChangeDoCancel)
                                        {
                                            log.Warn(
                                                "No valid ScreenChange option detected. Check screen configuration. Cancelling respondent.");
                                            this.SafeCall(() =>
                                            {
                                                var message =
                                                    _activeLg.Equals(Constants.FR,
                                                        StringComparison.InvariantCultureIgnoreCase)
                                                        ? "Une erreur de logique a été détectée. Impossible de continuer. Ce répondant sera annulé."
                                                        : "Logic error detected. Cannot continue. This respondent will be cancelled.";
                                                MessageBox.Show(this, message, @"iBrand", MessageBoxButtons.OK,
                                                    MessageBoxIcon.Error, MessageBoxDefaultButton.Button1);
                                                FirstScreen();
                                            });
                                        }
                                        else
                                        {
                                            log.Warn(
                                                "No valid ScreenChange option detected. Check screen configuration.");
                                        }
                                    }
                                    else
                                    {
                                        Next(sco.Step);
                                    }
                                }
                            }
                        }, _cts.Token);
                        _nextAction.Start();
                    }
                    else
                    {
                        ResumeTimer();
                    }
                }
                else
                {
                    ResumeTimer();
                }
            }
            finally
            {
                _block = false;
            }

            Refresh();
        }

        /// <summary>
        /// Vérifie que ce qui est entré soit minimalement valide (non-vide, peut être une adresse courriel ou autre, etc).
        /// Peut inclure une transformation au besoin.
        /// </summary>
        /// <returns>Le code validé.</returns>
        protected bool PreValidationCheck(string text)
        {
            if (string.IsNullOrWhiteSpace(text) && EmptyCodeValid )
            {
                return true;
            }

            if (IsCodeEmail && !StringUtil.IsValidEmail(text))
            {
                return false;
            }

            return true;
        }

        /// <summary>
        /// Indique que le code est un courriel et doit donc être validé en tant que tel avant l'appel.
        /// </summary>
        public bool IsCodeEmail { get; set; }

        /// <summary>
        /// Indique qu'un code vide est valide. Habituellement, il n'y a pas d'appel vers le API et le répondant doit être optionel également.
        /// </summary>
        public bool EmptyCodeValid { get; set; }

        public TimeSpan AutomaticNextDelay { get; set; } = TimeSpan.FromMilliseconds(500);

        public override void SetActivated()
        {
            base.SetActivated();
            _identifierTextbox.ForeColor = Color.Black;
            _identifierTextbox.Focus();
        }
        
        /// <summary>
        /// Force la vérification d'un utilisateur Eshots correct pour poursuivre.
        /// </summary>
        public bool RequireCorrectCustomerId { get; set; }


        /// <summary>
        /// Force la vérification d'un utilisateur Eshots correct pour poursuivre.
        /// </summary>
        public int CustomerIdQuestionNo { get; set; }


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
                // Avec un succès, on enregistre immédiatement le event day.

                // Deux choix: Contient des données, ou ne contient rien. Les deux retournent OK, mais aucune donnée implique
                // aucun consomateur Eshots associé.

                // 1- Pas de donnée associée
                if (resultEshots.ConsumerData == null)
                {
                    if (RequireCorrectCustomerId)
                    {
                        var resultNoItem = MessageBox.Show(@"There is no consumer associated with that identifier.",
                            @"No consumer associated.",
                            MessageBoxButtons.AbortRetryIgnore, MessageBoxIcon.Exclamation);

                        return resultNoItem;
                    }
                }
                else
                {
                    var f = resultEshots.ConsumerData.FirstName;
                    var l = resultEshots.ConsumerData.LastName;

                    // 2- Avec données
                    if (NameQuestion.HasValue && (!string.IsNullOrWhiteSpace(f) || !string.IsNullOrWhiteSpace(l)))
                    {
                        //SaveAnswerToQuestion(NameQuestion.Value, 1, f + " " + l);
                    }
                }

                if (!EventDayIdQuestionNo.HasValue) return DialogResult.OK;

                // Si on doit sauvegarder le event day ID dans le questionnaire.
                var eventDayId = ServicesContainer.GetService<EshotsApiService>()?.CurrentEventDay?.EventDayId;

                if (!eventDayId.HasValue)
                {
                    log.Error("There is no event day currently defined.");
                    return DialogResult.Abort;
                }

                //SaveAnswerToQuestion(EventDayIdQuestionNo.Value, 1, eventDayId.Value.ToString());

                return DialogResult.OK;
            }

            if ((resultEshots.ExecutionResult == EshotsComplexRequestResultEnum.InternetProblem || 
                 resultEshots.ExecutionResult == EshotsComplexRequestResultEnum.ServerError) &&
                ContinueEvenWithError)
            {
                log.Warn("Internet problem or server problem detected from eshots result. Returning OK due to configuration.");
                return DialogResult.OK;
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
                case EshotsComplexRequestResultEnum.ServerError:
                    message = "Server rejected the request. Try again?";
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

            var result = MessageBox.Show(message, @"Could not complete operation: ", buttons, MessageBoxIcon.Exclamation);

            return result;
        }

        /// <summary>
        /// Si cette options est mise à vrai, une erreur de réseau ne fera
        /// </summary>
        public bool ContinueEvenWithError { get; set; }
    }
}
