using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading;
using System.Windows.Forms;
using JetBrains.Annotations;
using Newtonsoft.Json.Linq;
using NLog;
using NomadLogic.Common;
using NomadLogic.Common.Respondent;
using NomadLogic.Common.Serveur;
using NomadLogic.Common.Util;
using NomadLogic.Common.Util.Json;
using NomadLogic.Common.Xangle;
using NomadLogic.iBrandDesktop.Datasource;
using NomadLogic.iBrandDesktop.Ui.Choice;
using NomadLogic.iBrandDesktop.Ui.Question;
using NomadLogic.iBrandDesktop.Util;
using NomadLogic.iBrandDesktop.Util.BarcodeScanner;
using NomadLogic.iBrandDesktop.Util.SubmitAction.BaseClasses;
using TerminalInfo = NomadLogic.Common.Serveur.TerminalInfo;

namespace NomadLogic.iBrandDesktop
{
    public abstract class Questionnaire : ISaveLocalRespondentData
    {
        private static readonly Logger log = LogManager.GetCurrentClassLogger();
        public event ScanEventHandler NewScan;
        public event OnNextPageRequestDelegate OnNextPageRequest;
        public event PageUpDownDelegate OnPageUpDown;

        private readonly Dictionary<int, Question> _questionMap = new Dictionary<int, Question>();

        private readonly List<DynamicHeader> _dynamicHeaderList = new List<DynamicHeader>();

        private List<IBarcodeScannerListener> _scannerListenerList;
        private List<QuestionCombo> _pageUpDownListenerList;

        private readonly Hashtable _datasourcesMap = new Hashtable();

        private Question _languageQuestion;
        private string _languages;

        private string _activeLanguage;

        private List<int> _questionPhotoTextList;

        private bool _scannerInitiated;

        private SymbolScanner _symbolScanner;
        private FullScreenImageViewer _fullScreenImageViewer;

        private string _uiFile;
        private string _logoFile;

        private string _projectPath;

        private const int WH_KEYBOARD_LL = 13;
        private const int WM_KEYDOWN = 0x0100;
        private LowLevelKeyboardProc _proc;
        private IntPtr _hookId = IntPtr.Zero;
        private XangleDetectionManager _manager;

        [DllImport("user32.dll", CharSet = CharSet.Auto, SetLastError = true)]
        private static extern IntPtr SetWindowsHookEx(int idHook, LowLevelKeyboardProc lpfn, IntPtr hMod, uint dwThreadId);

        [DllImport("user32.dll", CharSet = CharSet.Auto, SetLastError = true)]
        [return: MarshalAs(UnmanagedType.Bool)]
        private static extern bool UnhookWindowsHookEx(IntPtr hhk);

        [DllImport("user32.dll", CharSet = CharSet.Auto, SetLastError = true)]
        private static extern IntPtr CallNextHookEx(IntPtr hhk, int nCode, IntPtr wParam, IntPtr lParam);

        [DllImport("kernel32.dll", CharSet = CharSet.Auto, SetLastError = true)]
        private static extern IntPtr GetModuleHandle(string lpModuleName);

        private delegate IntPtr LowLevelKeyboardProc(int nCode, IntPtr wParam, IntPtr lParam);
        
        protected void InitQuestionnaire(int id, string languages, string projectPath)
        {
            Id = id;
            _languages = languages ?? Constants.EN;

            if (LanguagesCodeAvailiable.Length > 0)
                _activeLanguage = LanguagesCodeAvailiable[0];
          

            _projectPath = projectPath;
        }

        /// <summary>
        /// Cette méthode appelle les écoutants du questionnaire pour signifier une demande
        /// de soumission de répondant. Cette demande ne passe donc pas par un événement causé par l'UI mais
        /// bien par un événement venant d'une question ou d'un choix contenu dans le questionnaire, ou le questionnaire lui-même.
        /// </summary>
        public void NextPageRequest()
        {
            OnNextPageRequest?.Invoke();
        }
        
        [Obsolete("N'est plus requis grâce au système de réflexion de classe de synchro.")]
        public TerminalServerSyncMode ServerSyncMode { get; set; }

        public int Id { get; set; }

        [Obsolete]
        public LeaderBoard LeaderBoard { get; set; }

        [Obsolete]
        public bool HasLeaderBoard { set; get; }

        public void InitLeaderBoard()
        {
            if (Screen.AllScreens.Length <= 1) return;

            LeaderBoard = new LeaderBoard();
            var t = new Thread(LeaderBoard.LoadHtml);
            t.SetApartmentState(ApartmentState.STA);
            t.Start();
        }

        public void OnChoiceTimerChange(string value, bool reset)
        {
            LeaderBoard?.ChangeCounterValue(value, reset);
        }


        public string Dir => _projectPath + Id + Path.DirectorySeparatorChar;

        [Obsolete]
        public string UiFile
        {
            set => _uiFile = value;

            get => Id + Path.DirectorySeparatorChar + _uiFile;
        }

        [Obsolete]
        public string LogoFile
        {
            set => _logoFile = value;

            get => Id + Path.DirectorySeparatorChar + _logoFile;
        }

        public string[] LanguagesCodeAvailiable => _languages?.Split('|');

        public bool HasQuestionPhotoTextCount(int count)
        {
            return _questionPhotoTextList?.Count >= count;
        }

        [Obsolete]
        public bool UseCamera { get; private set; }

        public void Dispose()
        {
            CloseDatasource();

            _manager?.Dispose();

            if (_scannerInitiated)
            {
                CloseScanner();

                if (_scannerListenerList != null)
                {
                    var nbrListener = _scannerListenerList.Count;

                    for (var i = 0; i < nbrListener; ++i)
                    {
                        var choice = _scannerListenerList[i];
                        NewScan -= choice.Scanner_Read;
                    }
                }


            }

            if (_pageUpDownListenerList != null)
            {
                var nbrListener = _pageUpDownListenerList.Count;

                for (var i = 0; i < nbrListener; ++i)
                {
                    var qCombo = _pageUpDownListenerList[i];
                    OnPageUpDown -= qCombo.OnArrowsDown;
                    UnhookWindowsHookEx(_hookId);
                    _proc = null;
                }
            }

            LeaderBoard?.CloseForm();

            foreach (var question in _questionMap.Values)
            {
                try
                {
                    question.Dispose();
                }
                catch (System.Exception ex)
                {
                    log.Fatal(ex);
                }
            }

            _fullScreenImageViewer?.Close();

            SubmitAction?.StopProcessing();
        }

        public void InitScanner()
        {
            try
            {
                _symbolScanner = new SymbolScanner();
                _symbolScanner.Connect();
                _symbolScanner.ScanNotify += Scanner_ReadNotify;
                _scannerInitiated = true;
            }
            catch (System.Exception e)
            {
                throw new System.Exception("Cannot instanciate Scanner. Have you installed the Motorola Scanner SDK?", e);
            }
        }

        public void InitListenForPageDown(Question question, int projetId, string backgroundDir)
        {
            try
            {
                _proc = Page_Up_Down_Notify;
                _hookId = SetHook(_proc);
                _fullScreenImageViewer = new FullScreenImageViewer(projetId, Id, Terminal.TerminalId, backgroundDir);
                _fullScreenImageViewer.Show();
                question.OnAnswerChanged += _fullScreenImageViewer.OnAnswerChanged;

            }
            catch (System.Exception e)
            {
                throw new System.Exception("Problem creating fullscreen image viewer.", e);
            }
        }

        private void CloseScanner()
        {
            _symbolScanner.Disconnect();
            _symbolScanner.ScanNotify -= Scanner_ReadNotify;
            _symbolScanner = null;
        }

        private void CloseDatasource()
        {
            var datasources = _datasourcesMap.Values;

            foreach (AbstractDatasource datasource in datasources)
            {
                datasource.Close();
            }
        }

        /// <summary>
        /// Asigne une question comme étant la question associée à la langue du répondant.
        /// </summary>
        /// <param name="question"></param>
        public void SetLanguageQuestion(Question question)
        {
            _languageQuestion = question;
        }

        [Obsolete]
        public Question PhotoQuestion { set; get; }

        /// <summary>
        /// Obtient le code de langue qu'un répondant issu de l'état de ce questionnaire devrait avoir..
        /// On cherche s'il existe une question associé à la langue, si elle est répondue, et si oui, on retourne
        /// le code de langue associé à ce répondant. Sinon, on retourne le code de langue du questionnaire actif, et sinon, la
        /// labgue par défaut du questionnaire.
        /// </summary>
        /// <returns>Le code de langue du répondant, selon l'état actuel du questionnaire.</returns>
        public string GetRespondentLanguage()
        {
            var defaultLanguage = LanguagesCodeAvailiable != null ? LanguagesCodeAvailiable[0] : Constants.EN;

            // S'il n'y a pas de question de langue explicite (et c'est un truc obsolète), on prend la langue courante du questionnaire.
            // S'il n'y a pas de langue courante (probablement un bogue), on prend la langue par défaut du questionnaire.
            if (_languageQuestion == null) return _activeLanguage ?? defaultLanguage;
            var answers = new List<Answer>();

            if (_languageQuestion.QuestionPanel != null && _languageQuestion.QuestionPanel.InvokeRequired)
            {
                // we were called on a worker thread
                // marshal the call to the user interface thread
                return (string)_languageQuestion.QuestionPanel.Invoke(new GetRespondentLanguageDelegate(GetRespondentLanguage));
            }

            switch (_languageQuestion)
            {
                case QuestionRadio question:
                {
                    var radio = question;

                    if (!radio.IsAnswered()) return defaultLanguage;

                    _languageQuestion.GetAnswers(answers);
                    var answer = answers[0];
                    var choice = question.GetChoice(answer.ChoiceNo);
                    var choiceText = choice.GetText();

                    return choiceText.IndexOf("fr", StringComparison.InvariantCultureIgnoreCase) != -1 ? Constants.FR : Constants.EN;
                }
                case QuestionCombo combo:
                {
                    var questionCombo = combo;

                    if (!questionCombo.IsAnswered()) return defaultLanguage;

                    questionCombo.GetAnswers(answers);

                    var answer = answers[0];
                    var choice = questionCombo.GetChoice(answer.ChoiceNo);
                    var choiceText = choice.GetAnswerText();

                    return choiceText.IndexOf("fr", StringComparison.CurrentCultureIgnoreCase) != -1 ? Constants.FR : Constants.EN;
                }
            }

            return defaultLanguage;
        }

        public int GetPhotoTextQuestionNo(int nbr)
        {
            if (_questionPhotoTextList != null && _questionPhotoTextList.Count >= nbr)
                return _questionPhotoTextList[nbr - 1];

            return -1;
        }

        public void AddQuestion(Question question)
        {
            if (question is QuestionPhotoText)
            {
                if (_questionPhotoTextList == null)
                    _questionPhotoTextList = new List<int>();
                _questionPhotoTextList.Add(question.No);
            }
            else if (question is QuestionCameraRing || question is QuestionPhoto)
            {
                UseCamera = true;
            }

            _questionMap.Add(question.No, question);
        }

        public void AddDatasource(string key, AbstractDatasource obj)
        {
            _datasourcesMap.Add(key, obj);
        }

        public AbstractDatasource GetDatasource(string key)
        {
            return (AbstractDatasource)_datasourcesMap[key];
        }

        public virtual Question GetQuestion(int questionNo) // must override
        {
            return !_questionMap.ContainsKey(questionNo) ? null : _questionMap[questionNo];
        }


        public int GetNbrQuestion()
        {
            return _questionMap.Values.Count;
        }

        public int GetEUIQuestionNo()
        {
            foreach (var question in _questionMap.Values)
            {
                if (question.QuestionType == QuestionTypeEnum.ExternalUniqueIdentifier)
                {
                    return question.No;
                }
            }
            return -1;
        }

        public int GetLastVisibleQuestionNo()
        {
            var qNo = -1;

            foreach (var question in _questionMap.Values)
            {
                if (question.Visible && question.No > qNo)
                {
                    qNo = question.No;
                }
            }

            return qNo;
        }

        public virtual List<DynamicHeader> GetDynamicHeadersToSaveInDb()
        {
            return _dynamicHeaderList;
        }

        public virtual List<Answer> GetAnswersToSaveInDb()  // must override
        {
            var answers = new List<Answer>();

            foreach (var question in _questionMap.Values)
            {
                question.GetAnswers(answers);
            }

            return answers;
        }

        /// <summary>
        /// Assigne à chaque question un code de langue. Provoque un changement visuel.
        /// </summary>
        public void SetQuestionsLanguage()
        {
            foreach (var question in _questionMap.Values)
            {
                question.Language = LanguageCurrent;
            }
        }

        public List<Answer> GetVideoQuestionsAnswers()
        {
            var answers = new List<Answer>();

            foreach (var question in _questionMap.Values)
            {
                if (question is QuestionVideo)
                    question.GetAnswers(answers);
            }

            return answers;
        }

        public List<Answer> GetQuestionAnswers(int questionNo)
        {
            var answers = new List<Answer>();

            var question = _questionMap[questionNo];
            question.GetAnswers(answers);

            return answers;
        }

        public string LanguageCurrent
        {
            set
            {
                _activeLanguage = value;
                SetQuestionsLanguage();
            }

            get => _activeLanguage;
        }

        public AbstractSubmitAction SubmitAction { set; get; }

        /// <summary>
        /// Indique si un Questionnaire peut authoriser l'usager à remplir la forme avec les réponse du dernier répondant de ce questionnaire,
        /// s'il existe.
        /// </summary>
        public bool CanLoadLastRespondent { get; set; }

        /// <summary>
        /// Si TRUE, cela indique qu'il existe au moins un choix qui demande une écoute du scanner de code barre.
        /// À chaque invocation, fait une recherche complète dans tous les choix possibles.
        /// </summary>
        public bool HasScanChoice
        {
            get
            {
                var hasOneScanChoice = false;
                foreach (var question in _questionMap.Values)
                {
                    foreach (var scanChoice in question.Choices.Select(choice => choice as IBarcodeScannerListener).Where(scanChoice => scanChoice != null))
                    {
                        hasOneScanChoice |= scanChoice.BarCodeActivated;
                        if (hasOneScanChoice) break;
                    }

                    if (hasOneScanChoice) break;
                }

                return hasOneScanChoice;
            }
        }

        /// <summary>
        /// Si vide, on'utilise pas Xangle. Si non vide, on tente d'initialiser le manager Xangle avec ce fichier.
        /// </summary>
        public string XangleConfigFile { get; set; }

        /// <summary>
        /// Si mis à TRUE, on vide automatiquement les réponses une fois le répondant synchronisé.
        /// </summary>
        public bool AutoCleanAnswers { get; set; }

        /// <summary>
        /// Vide automatiquement les fichiers si mis à vrai après la synchronisation.
        /// </summary>
        public bool AutoCleanFiles { get; set; }

        /// <summary>
        /// Indique si le questionnaire mentionne clairement s'il est basé sur du plein écran ou non.
        /// </summary>
        public bool IsFullScreen { get; set; }

        /// <summary>
        /// Indique, si existant, la classe de chargement dynamique pour les écrans de ce questionnaire.
        /// </summary>
        public string LoaderClass { get; set; }

        /// <summary>
        /// Ajoute un écoutant sur les événements de Scan.
        /// </summary>
        /// <param name="scanListener"></param>
        public void AddScannerListener(IBarcodeScannerListener scanListener)
        {
            if (_scannerListenerList == null)
                _scannerListenerList = new List<IBarcodeScannerListener>();

            _scannerListenerList.Add(scanListener);

            NewScan += scanListener.Scanner_Read;
        }

        public void AddPageUpDownListener(QuestionCombo qc)
        {
            if (_pageUpDownListenerList == null)
            {
                _pageUpDownListenerList = new List<QuestionCombo>();
            }

            _pageUpDownListenerList.Add(qc);

            OnPageUpDown += qc.OnArrowsDown;
        }

        /// Vide les questions du questionnaire.
        public void Reset()
        {
            if (log.IsTraceEnabled)
            {
                log.Trace("Before reset: answers= {0}", StringUtil.FormatDictionary(GetAnswersDictionary()));
            }
            foreach (var question in _questionMap.Values)
            {
                question.Reset();
            }
            if (log.IsTraceEnabled)
            {
                log.Trace("After reset: answers= {0}", StringUtil.FormatDictionary(GetAnswersDictionary()));
            }

            _dynamicHeaderList.Clear();
        }

        private void Scanner_ReadNotify(string barcode)
        {
            if (barcode[0] == '{')
            {
                var parsedAnswers = JObject.Parse(barcode);
                var i = 0;
                while (parsedAnswers.SelectToken("answers[" + i + "]") != null)
                {
                    var answerJson = parsedAnswers.SelectToken("answers[" + i + "]");
                    var questionNo = (int)answerJson.SelectToken("questionNo");
                    var choiceNo = (int)answerJson.SelectToken("choiceNo");
                    var answer = new Answer {QuestionNo = questionNo, ChoiceNo = choiceNo};
                    var response = (string)answerJson.SelectToken("response");

                    if (response != null)
                    {
                        answer.Response = response;
                    }
                    var q = _questionMap[questionNo];
                    q.SetAnswer(answer);
                    i++;
                }
            }
            else
            {
                NewScan?.Invoke(this, new ScanEventArgs(barcode));
            }
        }

        private static IntPtr SetHook(LowLevelKeyboardProc proc)
        {
            using (var curProcess = Process.GetCurrentProcess())
            using (var curModule = curProcess.MainModule)
            {
                return SetWindowsHookEx(WH_KEYBOARD_LL, proc, GetModuleHandle(curModule.ModuleName), 0);
            }
        }

        public IntPtr Page_Up_Down_Notify(int nCode, IntPtr wParam, IntPtr lParam)
        {
            if (nCode < 0 || wParam != (IntPtr) WM_KEYDOWN) return CallNextHookEx(_hookId, nCode, wParam, lParam);

            var key = (Keys)Marshal.ReadInt32(lParam);
            if (key != Keys.Next && key != Keys.PageUp) return CallNextHookEx(_hookId, nCode, wParam, lParam);

            OnPageUpDown?.Invoke(key);
            return CallNextHookEx(_hookId, nCode, wParam, lParam);
        }

        public bool ValidateAnswers()
        {
            foreach (var question in _questionMap.Values)
            {
                if (question.ValidateAnswers()) continue;

                log.Debug(StringUtil.FormatDictionary(ClassUtil.ConvertPropertiesToDictionary(question)));

                var liste = new List<Answer>();
                question.GetAnswers(liste);

                var sb = new StringBuilder();
                sb.Append("Question ");
                sb.Append(question.No);
                sb.Append(" failed validation with answers being: ");
                foreach (var a in liste)
                {
                    sb.Append("\n\t");
                    sb.Append(a);
                }

                log.Warn(sb.ToString());
                return false;
            }

            return true;
        }

        public Dictionary<QuestionIdentifier, Answer> GetAnswersDictionary()
        {
            var ret = new Dictionary<QuestionIdentifier, Answer>();

            foreach (var qKey in _questionMap.Keys)
            {
                var la = new List<Answer>();
                _questionMap[qKey].GetAnswers(la);
                if (!la.Any()) continue;

                foreach (var a in la)
                {
                    ret.Add(a.QuestionIdentifier, a);
                }
            }

            return ret;
        }

        /// <summary>
        /// Tente d'initialiser le détecteur d'éléments Xangle. Lance une exception pour toute erreur.
        /// </summary>
        public void InitXangleManager()
        {
            var questDir = DirectoryUtil.GetLocalIbrandTerminalDir(Terminal);
            var filePath = new FileInfo(StringUtil.PathCombineFromUnix(questDir, XangleConfigFile));

            if (!filePath.Exists) throw new System.Exception("Xangle config file does not exists: " + filePath);

            var manager = JsonUtil.ReadJsonFile<XangleDetectionManager>(filePath);

            _manager = manager ?? throw new System.Exception("Manager could not be marshalled from JSON: " + filePath);

            if (!_manager.Init()) throw new System.Exception("Manager could not be initialized: " + filePath);
            _manager.NewXangleItem += ManagerOnNewXangleItem;

            _manager.Start();
        }

        private void ManagerOnNewXangleItem(object sender, XangleItemDetected e)
        {
            var lastItem = e.NewItems.LastOrDefault();
            if (lastItem == null) return;

            foreach (var mapper in _manager.Mapping)
            {
                if (!mapper.FindMapping(null, lastItem, out var ans, out var isCheck)) continue;
                var ok = isCheck
                    ? SaveAnswerToQuestionCheck(ans.QuestionNo, ans.ChoiceNo, true)
                    : SaveAnswerToQuestion(ans);

                if (!ok)
                {
                    log.Error("Could not save answer to questionnaire: {0}", ans);
                    return;
                }
            }

            if (_manager.AutomaticSubmit)
            {
                NextPageRequest();
            }
        }

        public virtual bool SaveAnswerToQuestion(Answer answer)
        {
            return answer != null && SaveAnswerToQuestion(answer.QuestionNo, answer.ChoiceNo, answer);
        }

        public virtual bool SaveAnswerToQuestionCheck(int questionNo, int choiceNo, bool check)
        {
            return SaveAnswerToQuestionCheck(new QuestionIdentifier(questionNo, choiceNo), check);
        }

        public bool SaveAnswerToQuestionCheck(QuestionIdentifier qi, bool check)
        {
            var questionnaire = this;
            var success = false;

            var question = questionnaire.GetQuestion(qi.QuestionNo);
            if (question == null)
            {
                log.Warn("Could not save answer. Unavailiable question no = {0}", qi.QuestionNo);
                return false;
            }

            var tmp = question.GetChoice(qi.ChoiceNo);
            if (tmp == null)
            {
                log.Warn("Could not save answer. Unavailiable choice no = {0}", qi.ChoiceNo);
                return false;
            }

            try
            {
                var choice = (ChoiceCheck)tmp;

                var answer = new Answer
                {
                    QuestionType = QuestionTypeEnum.Check, 
                    QuestionNo = question.No, 
                    ChoiceNo = check ? choice.No : -1
                };
                choice.SetAnswer(answer);

                success = true;
            }
            catch (System.Exception e)
            {
                log.Error(e, "Could not save answer. check={0}", check);
            }

            return success;
        }

        public virtual bool SaveDynamicHeader(DynamicHeader dh)
        {
            if (dh == null) return false;
            if (!dh.DataValues.Any()) return false;

            _dynamicHeaderList.Add(dh);
            return true;
        }

        public virtual bool SaveDynamicHeaderCollection(ICollection<DynamicHeader> dhc)
        {
            if (dhc == null || !dhc.Any()) return false;
            return dhc.Aggregate(true, (current, dh) => current & SaveDynamicHeader(dh));
        }

        public bool ClearDynamicHeaderCollection(params string[] dataKeyPurge)
        {
            if (!dataKeyPurge.Any()) return false;

            _dynamicHeaderList.RemoveAll(dh => dataKeyPurge.Any(dk => string.Equals(dk, dh.DataKey)));
            return true;
        }

        public bool AddToFirstMatchingKeyHeader(string dataKey, DataValue dv)
        {

            var dh = _dynamicHeaderList.FirstOrDefault(d => d.DataKey.Equals(dataKey));

            if (dh == null)
            {
                dh = new DynamicHeader
                {
                    DataKey = dataKey,
                    DateCreated = DateTime.Now
                };
                _dynamicHeaderList.Add(dh);
            }

            dh.DataValues.Add(dv);

            return true;
        }

        public bool AddToFirstMatchingKeyHeader(string dataKey, List<DataValue> dvList)
        {
            var dh = _dynamicHeaderList.FirstOrDefault(d => d.DataKey.Equals(dataKey));

            if (dh == null)
            {
                dh = new DynamicHeader
                {
                    DataKey = dataKey,
                    DateCreated = DateTime.Now
                };
                _dynamicHeaderList.Add(dh);
            }
            dh.DataValues.AddRange(dvList);
            return true;
        }

        public virtual bool SaveAnswerToQuestion(QuestionIdentifier qi, string responseValue = null)
        {
            var questionnaire = this;

            var question = questionnaire.GetQuestion(qi.QuestionNo);
            if (question == null)
            {
                log.Warn("Could not save answer. Unavailiable question no = {0}", qi.QuestionNo);
                return false;
            }

            var tmp = question.GetChoice(qi.ChoiceNo);
            if (tmp == null)
            {
                log.Warn("Could not save answer. Unavailiable choice no = {0}", qi.ChoiceNo);
                return false;
            }

            if (!(tmp is ChoiceText ct)) return false;
            var answer = new Answer
            {
                QuestionType = question.QuestionType, 
                QuestionNo = question.No, 
                ChoiceNo = ct.No,
                Response = responseValue
            };
            ct.SetAnswer(answer);

            return true;
        }

        public virtual bool SaveAnswerToQuestion(int questionNo, int choiceNo, Answer answer)
        {
            var success = false;

            var question = GetQuestion(questionNo);
            if (question == null)
            {
                log.Warn("Could not save answer. Unavailiable question no = {0}", questionNo);
                return false;
            }

            var tmp = question.GetChoice(choiceNo);
            if (tmp == null)
            {
                log.Warn("Could not save answer. Unavailiable choice no = {0}", choiceNo);
                return false;
            }

            try
            {
                tmp.SetAnswer(answer);
                success = true;
            }
            catch (System.Exception e)
            {
                log.Error(e, "Could not save answer. answer={0}", answer);
            }

            return success;
        }

        public IList<int> GetQuestionIdList()
        {
            return _questionMap.Keys.ToList();
        }

        public TerminalInfo Terminal { get; set; }

        /// <summary>
        /// Contient un liste des clé de groupe à ne pas inclure dans une synchronisation.
        /// </summary>
        public List<string> DynamicHeaderSyncExclusionList { get; set; } = new List<string>();

        /// <summary>
        /// Nom (optionel) de la classe à charger pour la finalisation de synchro.
        /// </summary>
        public string SyncFinisherClassName { get; set; }

        /// <summary>
        /// Cette méthode assaine les réponses et les valeurs dynamique présent dans un répondant selon les information dont l'instance de questionnaire dispose.
        /// Notamment, on va enlever les réponses qui ne font pas partie du questionnaire, ajuster certaines information du répondant selon le contenu,
        /// et élaguer les réponses dynamiques.
        /// ATTENTION: Comme cette méthode modifie des informations existantes, toutes les modifications sont faites sur une COPIE du répondant, et cette copie
        /// est retournée. Cela préserve les données de répondants en mémoire.
        /// Si l'argument est bull, on retourne null. Sinon, on retourne une version modifiée du répondant.
        /// </summary>
        [PublicAPI]
        public IRespondent ActualizeRespondentData(IRespondent resp)
        {
            if (resp == null) return null;

            var modResp = new Respondent(resp);

            var finalAnswerList = new List<Answer>();
            foreach (var answer in resp.Answers)
            {
                // On enlève les questions qui ne doivent pas être sauvegardées.
                var q = GetQuestion(answer.QuestionNo);
                if (q == null) continue;

                var newAsn = new Answer(answer);
                if (q.ToSync)
                {
                    finalAnswerList.Add(newAsn);
                }

                // Si la question est un type fichier mais qu'il ne faut pas envoyer vers le serveur, on force le type TEXT
                // au lieu du type original pour n'envoyer que le "path" local vers le serveur.
                if (!q.ToSave && q.QuestionType.IsFileType())
                {
                    newAsn.QuestionType = QuestionTypeEnum.Text;
                }

                // Si la réponse est de type EUI, on assigne sa valeur au répondant.
                if (newAsn.QuestionType == QuestionTypeEnum.ExternalUniqueIdentifier)
                {
                    modResp.ExternalUniqueIdentifier = newAsn.Response;

                    // Assign the provided EUI if present
                    modResp.ExternalUniqueIdentifier = newAsn.Response;

                    // If missing, generate one automatically
                    if (string.IsNullOrWhiteSpace(modResp.ExternalUniqueIdentifier))
                    {
                        modResp.ExternalUniqueIdentifier = Guid.NewGuid().ToString();
                        log.Warn($"Generated missing EUI for respondent {modResp.Id}: {modResp.ExternalUniqueIdentifier}");
                    }
                }
            }

            modResp.Answers = finalAnswerList;

            List<DynamicHeader> selectedDynamics;
            if (DynamicHeaderSyncExclusionList?.Any() ?? false)
            {
                selectedDynamics = new List<DynamicHeader>();
                var removed = 0;
                foreach (var dh in resp.DynamicHeaders)
                {
                    if (DynamicHeaderSyncExclusionList.Contains(dh.DataKey))
                    {
                        removed++;
                    }
                    else
                    {
                        selectedDynamics.Add(dh);
                    }
                }
                
                if (log.IsDebugEnabled)
                {
                    log.Debug("{0} elements removed from existing dynamic headers from exclusion list: {1}",
                        removed, StringUtil.FormatEnumerable(DynamicHeaderSyncExclusionList));
                }
            }
            else
            {
                selectedDynamics = resp.DynamicHeaders;
            }

            modResp.DynamicHeaders = new List<DynamicHeader>();
            foreach (var dyna in selectedDynamics)
            {
                modResp.DynamicHeaders.Add(new DynamicHeader(dyna));
            }
            return modResp;
        }
    }
}
