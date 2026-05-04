using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using JetBrains.Annotations;
using NLog;
using NomadLogic.Common;
using NomadLogic.Common.Respondent;
using NomadLogic.Common.Services;
using NomadLogic.iBrandDesktop.Services;
using NomadLogic.iBrandDesktop.Util.SubmitAction.BaseClasses;

namespace NomadLogic.iBrandDesktop.Util.SubmitAction
{
    /// <summary>
    /// TODO Ce serait bien de pouvoir ajouter cette classe comme générique. Un split de répondant basé sur des emails multiples serait très pratique. Jusqu'au moment où le serveur serait capable de gérer cette situation.
    /// </summary>
    [UsedImplicitly]
    public class PalmBay2018SubmitAction : AbstractSubmitAction
    {
        private static readonly Logger log = LogManager.GetCurrentClassLogger();
        
        protected override void InstanciateSubmitActionPools()
        {
            AddSubmitActionWorkerPool("split_palmbay", 1);
        }

        public override void TransformNotReadyToSyncRespondents(CancellationToken? ct)
        {
            var db = ServicesContainer.GetService<IbrandDatabase>();
            var respondentList = db.GetRespondentsBySyncStatus(Convert.ToString(ProjectId));

            UiSynchro?.SetProgressBarMinMax(0, respondentList.Count);

            foreach (var id in respondentList)
            {
                if (ct != null && ct.Value.IsCancellationRequested) break;

                var resp = db.GetRespondent(id);
                resp.Answers = db.GetAnswers(id);

                var worker = new PalmBay2018SubmitActionWorker(this, resp, id);

                worker.DoWork(ct);
                
                UiSynchro?.AddProgressStep(1);
            }
        }

        protected override ISubmitActionWorker CreateWorker(Respondent resp, bool usePool = true)
        {
            return new PalmBay2018SubmitActionWorker(this, resp,resp.Id);
        }
        
        private class PalmBay2018SubmitActionWorker : ISubmitActionWorker
        {
            private readonly PalmBay2018SubmitAction _action;
            private readonly Respondent _respondent;
            private readonly int _respId;

            public PalmBay2018SubmitActionWorker(PalmBay2018SubmitAction action,
                Respondent resp, int respId)
            {
                _action = action;
                _respondent = resp;
                _respId = respId;
            }

            public void DoWork(CancellationToken? ct)
            {
                if (ct?.IsCancellationRequested ?? false) return;

                try
                {
                    if (_action._stopped) return;

                    var db = ServicesContainer.GetService<IbrandDatabase>();

                    var nouveauRepondants = new List<Respondent>();

                    var qEmail = _respondent.Answers.FirstOrDefault(a => a.QuestionNo == 12);
                    var qSms = _respondent.Answers.FirstOrDefault(a => a.QuestionNo == 13);
                    var qOptIn = _respondent.Answers.FirstOrDefault(a => a.QuestionNo == 14);

                    if (qEmail != null || qSms != null)
                    {
                        // On créé le nouveau répondant.
                        var newResp = new Respondent(_respondent) {Answers = new List<Answer>()};

                        // On assigne les réponses dont le numéro est plus bas que celui de la question de courriel principale.
                        var tempList = new List<Answer>();
                        tempList.AddRange(_respondent.Answers.FindAll(a => a.QuestionNo < 9));
                        foreach (var answer in tempList)
                        {
                            newResp.Answers.Add(new Answer(answer));
                        }

                        // On transfert les réponses 12-13-14 aux question 9-10-11
                        if (qEmail != null) newResp.Answers.Add(new Answer {QuestionType = QuestionTypeEnum.Email, QuestionNo = 9, Response = qEmail.Response});
                        if (qSms != null) newResp.Answers.Add(new Answer {QuestionType = QuestionTypeEnum.Text, QuestionNo = 10, Response = qSms.Response});
                        if (qOptIn != null) newResp.Answers.Add(new Answer{QuestionType = QuestionTypeEnum.Check, QuestionNo = 11, Response = string.Empty});

                        nouveauRepondants.Add(newResp);
                    }

                    // Une fois les nouveaux répondants créés, on les enregistre dans la DB et on les flag comme étant prêts eux-aussi
                    // On le fait après la boucle de création au cas où une erreur se produit et que l'on a ajouté des répondants dans la bd pour rien.
                    foreach (var resp in nouveauRepondants)
                    {
                        db.InsertRespondent(resp, true);
                    }

                    db.SetRespondentReadyToSync(_respId);

                    _action.UiSynchro?.AddUiMessage("Respondent split PalmBay action finished.", 4);
                }
                catch (System.Exception ex)
                {
                    log.Fatal("Error while PalmBay2018SubmitActionWorker#DoWork():\r\n" + ex.Message + "\r\n" + ex.StackTrace);
                    _action.UiSynchro?.AddUiMessage("Error while PalmBay2018SubmitActionWorker#DoWork()", 1);
                }
            }

            public ISubmitActionWorker NextWorkerStep { get; set; }

            public int GetRespId()
            {
                return _respId;
            }

            public ISubmitAction GetSubmitAction()
            {
                return _action;
            }
        }
    }
}
