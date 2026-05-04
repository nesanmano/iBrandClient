using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Windows.Forms;
using NLog;
using NomadLogic.Common.Respondent;
using NomadLogic.Common.Util;
using NomadLogic.Common.Util.Json;
using NomadLogic.iBrandDesktop.Ui.ApplicationScreen.Generic;
using NomadLogic.iBrandDesktop.Ui.Choice;

namespace NomadLogic.iBrandDesktop.Ui.ApplicationScreen.custom._360
{
    public class RfidScanAlphaGene : RfidScannerScreen
    {
        private static readonly Logger log = LogManager.GetCurrentClassLogger();

        private SweepstakesDefinitions _fullDefs;
        private IList<SweepdIdTuple> _tuples;
        private FileInfo _resultFileName;
        private string _baseDir;

        public RfidScanAlphaGene(string backgroundImgEnSrc, string backgroundImgFrSrc,
            ScreenConfig sc, Rectangle? textboxLocation) :
            base(backgroundImgEnSrc, backgroundImgFrSrc, sc, textboxLocation)
        {
            _baseDir = GetTerminalDirectory();
            _resultFileName = new FileInfo(_baseDir + "results.json");
        }

        public override string WebFacadeConfigFile => null;
        public string SweepstakesIdsFile { get; set; }
        public string SweepstakesDefinitionFile { get; set; }
        public int SweepstakesBlockId { get; set; }

        public IList<SweepStakesConditions> SweepstakeCategoryConditions { get; set; }

        protected override void CustomLoad()
        {
            base.CustomLoad();

            // Charger définitions

            _fullDefs = JsonUtil.ReadJsonFile<SweepstakesDefinitions>(new FileInfo(SweepstakesDefinitionFile));

            // Si null, vide ou pas rapport avec le type donné, exception. Pas le temps de niaiser.
            if (_fullDefs?.BlockDefinitions == null 
                || !_fullDefs.BlockDefinitions.ContainsKey(SweepstakesBlockId) 
                || !_fullDefs.BlockDefinitions[SweepstakesBlockId].Any())
            {
                throw new System.Exception($"No valid sweepstakes definition data for type {SweepstakesBlockId} found in file {SweepstakesDefinitionFile}");
            }

            if (log.IsDebugEnabled)
            {
                var today = DateTime.Today.Date;
                var todayConfig = _fullDefs.BlockDefinitions[SweepstakesBlockId].Where(
                    b => b.MinDate.Date <= today && b.MaxDate.Date >= today).ToList();
                log.Debug("{0} block definitions are detected for today.", todayConfig.Count, today.ToShortDateString());
                log.Debug(StringUtil.FormatEnumerable(todayConfig, true, 2, "\t", true, o => ClassUtil.ToStringPublicProperties(o)));
            }

            // Charger IDs

            _tuples = JsonUtil.ReadJsonFile<IList<SweepdIdTuple>>(new FileInfo(SweepstakesIdsFile));

            if (_tuples == null || !_tuples.Any())
            {
                throw new System.Exception($"No valid RFID list found in file {SweepstakesIdsFile}");
            }

            if (log.IsDebugEnabled)
            {
                log.Debug("{0} RFIDs loaded into memory.", _tuples.Count);
            }

            // Ajout des identifiants comme valeurs valides.
            CustomExtraValidationSetData(from t in _tuples select t.Rfid);
        }

        protected override bool HasCustomSuccessAction => true;

        protected override void ExecuteCustomSuccessAction(string sequenceRead)
        {
            // Charger le fichier de résultat
            _resultFileName.Refresh();
            
            if (!_resultFileName.Exists)
            {
                JsonUtil.WriteJsonFile(_resultFileName, new SweepstakesResults());
                _resultFileName.Refresh();
            }

            var results = JsonUtil.ReadJsonFile<SweepstakesResults>(_resultFileName);
            var now = DateTime.Now;
            var resp = GetCurrentRespondentWithAnswers();
            
            var quest = GetCurrentQuestionnaire();

            var choiceType = quest.GetQuestion(5).Choices[0] as ChoiceCombo;
            var choiceSecteur = quest.GetQuestion(6).Choices[0] as ChoiceCombo;

            var textType = choiceType?.GetAnswerText();
            var textSecteur = choiceSecteur?.GetAnswerText();

            log.Info("Nouveau Réspondant: Type={0}, Secteur={1}", textType, textSecteur);

            var result = results.DetermineCategory(sequenceRead, resp, _fullDefs, _tuples, SweepstakeCategoryConditions, now);
            
            // Dialogue
            var gifFile = _baseDir + "WS2k.gif";
            var popup = new SweepstakesWinner
            {
                LeftImagePath = gifFile,
                RightImagePath = gifFile,
                MainImageLocation = _baseDir + result.ImagePrix,
                UserMessage = result.Message,
                Rfid = sequenceRead,
                Code = WinnerDialogCode,
                // Comme la taille peut varier énormément (environnement de DEV surtout), on place l'écran manuellement.
                StartPosition = FormStartPosition.Manual,
                Location = new Point(331, 129),
                SoundLocation = _baseDir + "fanfare_low.wav"
            };

            var dialogresult = popup.ShowDialog();
            switch (dialogresult)
            {
                case DialogResult.OK:
                    results.PreviouSweepstakeResults.Add(result);
                    JsonUtil.WriteJsonFile(_resultFileName, results);

                    SaveAnswerToQuestion(new Answer {QuestionNo = 10, Response = result.CodePrix.ToString()});
                    break;
                case DialogResult.Cancel:
                    // Si un problème
                    break;
            }
            popup.Dispose();
        }

        public string WinnerDialogCode { get; set; } = "911";
    }
}

