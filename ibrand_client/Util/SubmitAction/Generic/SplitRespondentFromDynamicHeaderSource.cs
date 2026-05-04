using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using NLog;
using NomadLogic.Common;
using NomadLogic.Common.Respondent;
using NomadLogic.iBrandDesktop.Services;
using NomadLogic.iBrandDesktop.Util.SubmitAction.BaseClasses;

namespace NomadLogic.iBrandDesktop.Util.SubmitAction.Generic
{
    public class SplitRespondentFromDynamicHeaderSource : AbstractSubmitAction
    {
        private const string SPLIT_DATA_KEY = "split_datakey";
        private const string QUESTIONS_TO_TRANSFERT = "questions_to_transfert";
        private const string DYNAMIC_HEADERS_TO_TRANSFERT = "dynamic_headers_to_transfert";
        private const string SPLIT_VALUE_TO_QUESTION = "split_value_to_question";
        private const string SPLIT_VALUE_TO_QUESTION_TYPE = "split_value_to_question_type";
        private const string DISALLOW_ORIGINAL_SYNC = "disallow_original_sync";

        protected override void InstanciateSubmitActionPools()
        {
            AddSubmitActionWorkerPool("SplitRespondentsFromDynamicHeaderSource", 1);
        }

        protected override ISubmitActionWorker CreateWorker(Respondent resp, bool usePool = true)
        {
            return new SplitRespondentFromDynamicHeaderSourceWorker(this, resp, resp.Id);
        }
        
        #region Properties data config

        protected override void ValidateConfigData()
        {
            if (!_configData.ContainsKey(SPLIT_DATA_KEY))
            {
                throw new System.Exception("Missing critical configuration in config file: " + SPLIT_DATA_KEY);
            }
            
            SplitDataKey = _configData[SPLIT_DATA_KEY];
            if (string.IsNullOrWhiteSpace(SplitDataKey))
            {
                throw new System.Exception("Datakey for dynamic header of split data required.");
            }

            // section Optionelle

            if (_configData.ContainsKey(QUESTIONS_TO_TRANSFERT))
            {
                var toSplit = _configData[QUESTIONS_TO_TRANSFERT].Split(',');
                foreach (var qiStr in toSplit)
                {
                    var qi = QuestionIdentifier.FromTextDefinition(qiStr);
                    QuestionsToTransfert.Add(qi);
                }
            }

            if (_configData.ContainsKey(DYNAMIC_HEADERS_TO_TRANSFERT))
            {
                var toSplit = _configData[DYNAMIC_HEADERS_TO_TRANSFERT].Split(',');
                foreach (var dhSplit in toSplit)
                {
                    DynamicHeadersToTransfert.Add(dhSplit);
                }
            }

            if (_configData.ContainsKey(SPLIT_VALUE_TO_QUESTION))
            {
                var qiStr = _configData[SPLIT_VALUE_TO_QUESTION];
                SplitValueToNewAnswer = QuestionIdentifier.FromTextDefinition(qiStr);
                if (SplitValueToNewAnswer == null)
                {
                    throw new System.Exception("Invalid question identifier value to parse: " + qiStr);
                }
            }

            if (_configData.ContainsKey(DISALLOW_ORIGINAL_SYNC))
            {
                var disallow = bool.Parse(_configData[DISALLOW_ORIGINAL_SYNC]);
                DisallowOriginalToSync = disallow;
            }

            if (_configData.ContainsKey(SPLIT_VALUE_TO_QUESTION_TYPE))
            {
                var val = _configData[SPLIT_VALUE_TO_QUESTION_TYPE];
                if (!Enum.TryParse<QuestionTypeEnum>(val, out var dhType))
                {
                    throw new System.Exception("Invalid enum value for split question value type: " + val);
                }
                SplitValueToNewAnswerType = dhType;
            }
        }

        public string SplitDataKey { get; private set; }

        public List<QuestionIdentifier> QuestionsToTransfert { get; } = new List<QuestionIdentifier>();

        public List<string> DynamicHeadersToTransfert { get; } = new List<string>();

        public QuestionIdentifier SplitValueToNewAnswer { get; private set; }
        
        public bool DisallowOriginalToSync { get; private set; }

        public QuestionTypeEnum? SplitValueToNewAnswerType { get; set; }

        #endregion
    }

    public class SplitRespondentFromDynamicHeaderSourceWorker : SubmitActionWorkerAbstract<SplitRespondentFromDynamicHeaderSource>
    {
        private static readonly Logger logWorker = LogManager.GetCurrentClassLogger();

        public SplitRespondentFromDynamicHeaderSourceWorker(SplitRespondentFromDynamicHeaderSource action, Respondent resp, int respId, 
            SubmitActionWorkerNextStepInfo stepInfo = null) : base(action, resp, respId, stepInfo) { }

        protected override ISubmitActionWorker NextWorkerInstance(SubmitActionWorkerNextStepInfo nextSteps)
        {
            // Pas de chaînage de worker ici.
            return null;
        }

        protected override void DoWorkExecute(IbrandDatabase db, CancellationToken? ct)
        {
            if (ct?.IsCancellationRequested ?? false) return;

            try
            {
                if (_action._stopped) return;
                
                // La liste des répondants à ajouter.
                var nouveauRepondants = new List<Respondent>();

                // Construit une seule liste de valeurs à partir de tous les groupes avec le même Datakey.
                var splitDynamicSource = _respondent.DynamicHeaders.
                    Where(dh => dh.DataKey.Equals(_action.SplitDataKey)).
                    SelectMany(dh => dh.DataValues).ToList();

                // S'applique si uniquement cette liste contient des éléments. Les "répondants fils" du split ne contiennent pas ces informations.
                if (splitDynamicSource.Any())
                {
                    foreach (var sourceValue in splitDynamicSource)
                    {
                        // 0- Création du nouveau répondant pour cet identifiant, basé (mais pas totalement) sur l'original.
                        var newResp = new Respondent(_respondent, true)
                        {
                            DateLocal = DateTime.Now, Id = -1, IsSynchronized = false
                        };

                        // 1- Obtient les questions à transférer
                        foreach (var qi in _action.QuestionsToTransfert)
                        {
                            var ans = _respondent.GetAnswer(qi);
                            newResp.Answers.Add(ans);
                        }

                        // 2- Création de nouvelle réponses

                        if (_action.SplitValueToNewAnswer != null)
                        {
                            var ans = new Answer
                            {
                                QuestionIdentifier = _action.SplitValueToNewAnswer,
                                QuestionType = _action.SplitValueToNewAnswerType ?? QuestionTypeEnum.Text,
                                Response = sourceValue.ToString()
                            };
                            newResp.Answers.Add(ans);
                        }

                        // 3- Transférer des valeurs dynamiques.
                        foreach (var dht in _action.DynamicHeadersToTransfert)
                        {
                            var dhTrans = _respondent.DynamicHeaders.Where(dh => string.Equals(dh.DataKey, dht));
                            newResp.DynamicHeaders.AddRange(dhTrans);
                        }

                        // 4- TODO Créer des valeurs dynamiques.

                        nouveauRepondants.Add(newResp);
                    }
                    
                }

                // Une fois les nouveaux répondants créés, on les enregistre dans la DB et on les flag comme étant prêts eux-aussi
                // On le fait après la boucle de création au cas où une erreur se produit et que l'on a ajouté des répondants dans la bd pour rien.
                // Cette liste PEUT être vide, car seulement le répondant original doit être traité comme un split potentiel.
                foreach (var resp in nouveauRepondants)
                {
                    var newId = db.InsertRespondent(resp, true);
                    logWorker.Debug($"Added splitted respondent {newId} to database.");
                }
                
                // 5- Finalisation du répondant "source": On peut choisir de ne pas l'envoyer sur le serveur.
                if (_action.DisallowOriginalToSync)
                {
                    db.SetRespondentSynchronized(_respId, -1);
                    logWorker.Trace("Disabled original respondent from being sent to server: {0}", _respId);
                }

                _action.UiSynchro?.AddUiMessage("Respondent split from dynamic source action finished.", 4);
            }
            catch (System.Exception ex)
            {
                logWorker.Fatal(ex, "Error while executing treatment of respondent.");

                _action.UiSynchro?.AddUiMessage("Error while treating respondent.", 4);
            }
        }
    }
}