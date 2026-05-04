using System;
using System.IO;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using System.Xml;
using JetBrains.Annotations;
using NLog;
using NomadLogic.Common;
using NomadLogic.Common.Respondent;
using NomadLogic.Common.Services;
using NomadLogic.Common.Services.Configuration;
using NomadLogic.Common.Ui;
using NomadLogic.Common.Ui.Dialog;
using NomadLogic.Common.Util;
using NomadLogic.Common.Util.Trigger.LaserBeam;
using NomadLogic.iBrandDesktop.Datasource;
using NomadLogic.iBrandDesktop.Exception;
using NomadLogic.iBrandDesktop.Services;
using NomadLogic.iBrandDesktop.Ui;
using NomadLogic.iBrandDesktop.Ui.Choice;
using NomadLogic.iBrandDesktop.Ui.Question;
using NomadLogic.iBrandDesktop.Util.BarcodeScanner;
using NomadLogic.iBrandDesktop.Util.SubmitAction.BaseClasses;
#pragma warning disable 612

#pragma warning disable 618

namespace NomadLogic.iBrandDesktop.Util
{
    public abstract class QuestionnaireLoaderAbstract
    {
        private static readonly Logger log = LogManager.GetCurrentClassLogger();
        private QuestionnaireInfo _questionnaireInfo;
        private Project _project;
        private Questionnaire _questionnaire;  // pointe sur le type abstract
        private Question _question;
        private Choice _choice;
        private Equation _equation;
        private DatasourceAnswer _datasourceAnswer;
        private SubChoicesFromDatasourceAnswer _subChoicesFromDatasourceAnswer;
        private Link _link;
        private PopupDlg _popupDlg;
        private HtmlPopupDlg _htmlPopupDlg;
        private AbstractSubmitAction _submitAction;
        
        private string _ansValue;
        private int _defaultAnsChoiceNo;

        
        private bool _inChoice;
        private bool _inQuestion;
        private bool _inLink;
        private bool _inPopupDlg;
        private string _lg;
        private string _text;
        // ReSharper disable once NotAccessedField.Local
        private IbrandDatabase _db;

        private bool _loadChoices = true;
        private readonly string _currentAssembly = "NomadLogic.iBrandDesktop";

        /// <summary>
        /// Charge un questionnaire en mémoire
        /// </summary>
        /// <param name="loadQuestions">Ne charge pas les questions en mémoire. Utile uniquement que si l'on veut aller chercher le SubmitAction
        /// ou les méta-données d'un projet.</param>
        /// <returns>Une référence vers le questionnaire créé.</returns>
        public abstract Questionnaire Load(bool loadQuestions);

        protected void QuestionnaireLoader(QuestionnaireInfo questionnaireInfo, Project project)
        {
            _questionnaireInfo = questionnaireInfo;
            _project = project;
            _questionnaire = null;
            _question = null;
            _choice = null;
            _link = null;
            _popupDlg = null;
            _inChoice = false;
            _inQuestion = false;
            _inLink = false;
            _inPopupDlg = false;
            _lg = null;

            //Kugan 

            _db = ServicesContainer.GetService<IbrandDatabase>();
        }

        /// <summary>
        /// Si mis à FALSE (TRUE par défaut), empêche la charge des datasources et les liens vers les autres composantes d'événements dû
        /// à une interaction usager.
        /// Utile dans les situations où on sait que le questionnaire n'est
        /// utilisé qu'à fin de lecture (pas de nouveaux répondants, donc pas de nouvelles réponses et leur validations).
        /// </summary>
        [PublicAPI]
        public bool EnableDatasourceAndEvents { get; set; } = true;
        
        protected Questionnaire LoadQuestionnaire(bool withChoices)
        {
            StreamReader stream = null;

            try
            {
                _loadChoices = withChoices;

                var fileName = DirectoryUtil.GetLocalApplicationDir(DirectoryUtil.ApplicationNameEnum.iBrand) + _questionnaireInfo.FileSrc;

                if (!File.Exists(fileName))
                {
                    throw new NoFileException("Questionnaire file not found: " + fileName);
                }

                stream = new StreamReader(fileName,Encoding.UTF8);

                var settings = new XmlReaderSettings
                {
                    ConformanceLevel = ConformanceLevel.Fragment,
                    IgnoreWhitespace = true,
                    IgnoreComments = true
                };

                var xmlReader = XmlReader.Create(stream, settings);

                ParseXml(xmlReader);

                var configuration = ServicesContainer.GetService<NomadConfiguration>();
                var configLg = configuration.PreferedLanguage;
                var possibleLanguages = _questionnaire.LanguagesCodeAvailiable;

                // Vérifie que la langue donnée par le système est compatible avec les langues possibles du questionnaire.
                // SI ce n'est pas le cas, on assigne la première langue possible du questionnaire comme langue active.
                if (!possibleLanguages.Any(l => l.Equals(configLg, StringComparison.InvariantCultureIgnoreCase)))
                {
                    configLg = possibleLanguages.First();
                }

                _questionnaire.LanguageCurrent = configLg;

                return _questionnaire;
            }
            catch (System.Exception e)
            {
                throw new System.Exception("Projet parsing aborted.", e);
            }
            finally
            {
                stream?.Close();
            }
        }

        private void ParseXml(XmlReader xmlReader)
        {
            while (xmlReader.Read())
            {
                if (xmlReader.NodeType == XmlNodeType.Element)
                {
                    StartElement(xmlReader);
                    if (xmlReader.IsEmptyElement)
                    {
                        EndElement(xmlReader);
                    }
                }
                else if (xmlReader.NodeType == XmlNodeType.EndElement)
                {
                    EndElement(xmlReader);
                }
                else if (xmlReader.NodeType == XmlNodeType.Text)
                {
                    _text = xmlReader.Value;
                }
            }
        }

        private void StartElement(XmlReader xmlReader)
        {
            if (xmlReader.Name.Equals("questionnaire"))
            {
                var fullscreenStr = xmlReader.GetAttribute("fullscreen");

                // ReSharper disable once UnusedVariable
                var loaderClass = xmlReader.GetAttribute("loader_class");

                var languages = xmlReader.GetAttribute("lg");
                var canLoadLastRespondent = xmlReader.GetAttribute("load_last_respondent");
                var xangle = xmlReader.GetAttribute("xangle_config_file");

                var autoCleanAnswers = xmlReader.GetAttribute("auto_clean_answers");
                var autoCleanFiles = xmlReader.GetAttribute("auto_clean_files");

                _questionnaire = new IBrandQuestionnaire(_questionnaireInfo.Id, languages, _project.ProjectDir);

                bool.TryParse(canLoadLastRespondent, out var tmp);
                _questionnaire.CanLoadLastRespondent = tmp;

                bool.TryParse(fullscreenStr, out var fullSCreen);
                _questionnaire.IsFullScreen = fullSCreen;

                if (!string.IsNullOrWhiteSpace(xangle))
                {
                    _questionnaire.XangleConfigFile = xangle;
                }

                bool.TryParse(autoCleanAnswers, out var aca);
                _questionnaire.AutoCleanAnswers = aca;

                bool.TryParse(autoCleanFiles, out var acf);
                _questionnaire.AutoCleanFiles = acf;
            }
            else if (xmlReader.Name.Equals("datasource"))
            {
                if (!EnableDatasourceAndEvents) return;
                var datasourceName = xmlReader.GetAttribute("name");
                var fileName = xmlReader.GetAttribute("file_name");
                var type = xmlReader.GetAttribute("type");

                var fileSrc = _project.ProjectDir + _questionnaireInfo.Id + Path.DirectorySeparatorChar + fileName;

                var datasourceInfo = new DatasourceInfo(datasourceName, fileSrc, type);
                AbstractDatasource datasource = null;

                if (type == null) return;
                if (type.Equals(AbstractDatasource._inMemoryType))
                {
                    datasource = new InMemoryDatasource(datasourceInfo, _project);

                }
                else if (type.Equals(AbstractDatasource._bdType))
                {
                    datasource = new DatabaseDatasource(datasourceInfo, _project);
                }

                if (datasource == null) return;
                datasource.Load();
                _questionnaire.AddDatasource(datasourceName, datasource);
            }
            else if (xmlReader.Name.Equals("question"))
            {
                var fullClassName = xmlReader.GetAttribute("class");
                var no = xmlReader.GetAttribute("no");
                var language = xmlReader.GetAttribute("language");
                var mandatory = xmlReader.GetAttribute("mandatory");
                var enabled = xmlReader.GetAttribute("enabled");
                var recyclable = xmlReader.GetAttribute("recyclable");
                var toSave = xmlReader.GetAttribute("to_save");
                var visible = xmlReader.GetAttribute("visible");
                var toSync = xmlReader.GetAttribute("to_sync");
                
                try
                {
                    var clazz = StringUtil.GetClassName(fullClassName);
                    var type = Type.GetType($"{_currentAssembly}.Ui.Question." + clazz);
                    // ReSharper disable once AssignNullToNotNullAttribute
                    _question = (Question)Activator.CreateInstance(type);
                    _question.No = Convert.ToInt32(no);
                    _questionnaire.AddQuestion(_question);
                    _inQuestion = true;
                }
                catch (System.Exception ex)
                {
                    throw new System.Exception($"({_project.Id}/{_questionnaire.Id}) Could not instantiate question from type: {fullClassName}.", ex);
                }

                if (mandatory != null)
                {
                    if (mandatory.Equals("false"))
                        _question.MandatoryDefault = false;
                }

                if (enabled != null)
                {
                    if (enabled.Equals("false"))
                        _question.EnabledDefault = false;
                }

                if (visible != null)
                {
                    if (visible.Equals("false"))
                        _question.Visible = false;
                }

                if (recyclable != null)
                {
                    if (recyclable.Equals("false"))
                        _question.Recyclable = false;
                }

                if (toSave != null)
                {
                    if (toSave.Equals("false"))
                        _question.ToSaveDefault = false;
                }

                if (toSync != null)
                {
                    if (toSync.Equals("false"))
                        _question.ToSync = false;
                }

                if (language != null)
                {
                    _questionnaire.SetLanguageQuestion(_question);
                }

                if (_question is QuestionPhoto)
                {
                    _questionnaire.PhotoQuestion = _question;
                }
            }
            else if (xmlReader.Name.Equals("default_answer"))
            {
                
                _lg = xmlReader.GetAttribute("lg");
                _defaultAnsChoiceNo = Int32.Parse(xmlReader.GetAttribute("choice_no")??"1");
                //_defaultAnsSubChoiceNo = Int32.Parse(xmlReader.GetAttribute("sub_choice_no")??"-1");
                xmlReader.Read();

                _ansValue = xmlReader.Value;
                //_answer = new Answer(_question.QuestionType,_question.No,_choice.No,xmlReader.ReadString());
            }
            else if (xmlReader.Name.Equals("answer_pattern"))
            {
                if (!(_question is QuestionAnswersValidationBase)) return;

                var value = xmlReader.GetAttribute("value");

                if (value == null) return;
                try
                {
                    if (value.Equals("email"))
                    {
                        ((QuestionAnswersValidationBase)_question).SetAnswersPattern(PatternMatcher.EmailAdr);
                    }
                    else if (value.Equals("canada_postal_code"))
                    {
                        ((QuestionAnswersValidationBase)_question).SetAnswersPattern(PatternMatcher.CanadaPostalCode);
                    }
                    else if (value.Equals("postal_code"))
                    {
                        ((QuestionAnswersValidationBase)_question).SetAnswersPattern(PatternMatcher.PostalCode);
                    }
                    else if (value.Equals("canon"))
                    {
                        ((QuestionAnswersValidationBase)_question).SetAnswersPattern(PatternMatcher.Canon);
                    }
                    else if (value.Equals("nikon"))
                    {
                        ((QuestionAnswersValidationBase)_question).SetAnswersPattern(PatternMatcher.Nikon);
                    }
                    else if (value.Equals("us_postal_code"))
                    {
                        ((QuestionAnswersValidationBase)_question).SetAnswersPattern(PatternMatcher.UsPostalCode);
                    }
                    else if (value.Equals("phone_number"))
                    {
                        ((QuestionAnswersValidationBase)_question).SetAnswersPattern(PatternMatcher.PhoneNumber);
                    }
                    else if (value.Equals("integer"))
                    {
                        ((QuestionAnswersValidationBase)_question).SetAnswersPattern(PatternMatcher.Integer);
                    }
                    else if (value.Equals("minute_time"))
                    {
                        ((QuestionAnswersValidationBase)_question).SetAnswersPattern(PatternMatcher.MinuteTime);
                    }
                }
                catch (UnsuportedPatternException)
                {
                    throw new System.Exception("Unsuported pattern");
                }
            }
            else if (xmlReader.Name.Equals("answer_datasource_validation"))
            {
                if (!(_question is QuestionAnswersValidationBase) || !EnableDatasourceAndEvents) return;
                var datasourceName = xmlReader.GetAttribute("datasource");
                var fieldName = xmlReader.GetAttribute("field");

                var datasource = _questionnaire.GetDatasource(datasourceName);

                if (datasource != null)
                    ((QuestionAnswersValidationBase)_question).SetDatasourceValidation(datasource, fieldName);
                //TBD throw datasource not found exception
            }
            else if (xmlReader.Name.Equals("submit_action"))
            {
                var clazz = StringUtil.GetClassName(xmlReader.GetAttribute("class"));
                var type = Type.GetType($"{_currentAssembly}.Util.SubmitAction.{clazz}") ?? 
                    Type.GetType($"{_currentAssembly}.Util.SubmitAction.Generic.{clazz}");

                if (clazz == null || type == null)
                {
                    throw new System.Exception("Invalid Submit Action class name: " + clazz);
                }
                
                try
                {
                    // ReSharper disable once AssignNullToNotNullAttribute
                    _submitAction = (AbstractSubmitAction)Activator.CreateInstance(type);
                }
                catch (System.Exception e)
                {
                    throw new System.Exception("Could not instantiate Submit Action: " + clazz, e);
                }
            }
            else if (xmlReader.Name.Equals("ui"))
            {
                //var deviceName = xmlReader.GetAttribute("device_name");
                var fileName = xmlReader.GetAttribute("file_name");

                /*var configuration = ServicesContainer.GetService<NomadConfiguration>();
                if (configuration.DeviceName.Equals(deviceName))
                {*/
                    _questionnaire.UiFile = fileName;
                //}
            }
            else if (xmlReader.Name.Equals("logo"))
            {
                var deviceName = xmlReader.GetAttribute("device_name");
                var fileName = xmlReader.GetAttribute("file_name");

                var configuration = ServicesContainer.GetService<NomadConfiguration>();
                if (configuration.DeviceName.Equals(deviceName))
                    _questionnaire.LogoFile = fileName;

            }
            else if (xmlReader.Name.Equals("choice"))
            {
                if (_loadChoices)
                {
                    var fullClassName = xmlReader.GetAttribute("class");
                    var no = xmlReader.GetAttribute("no");
                    var prefix = xmlReader.GetAttribute("prefix");
                    var suffix = xmlReader.GetAttribute("suffix");
                    var maxLength = xmlReader.GetAttribute("max_length");
                    var titleCase = xmlReader.GetAttribute("title_case");
                    var appendBarCode = xmlReader.GetAttribute("append_bar_code");
                    var fromBarCode = xmlReader.GetAttribute("from_bar_code");
                    // ReSharper disable once UnusedVariable
                    var arrowsEnable = xmlReader.GetAttribute("arrows_enable");
                    var enabled = xmlReader.GetAttribute("enabled");
                    var autosubmit = xmlReader.GetAttribute("autosubmit");
                    var countDown = xmlReader.GetAttribute("count_down");
                    var maxTime = xmlReader.GetAttribute("max_time");
                    var print = xmlReader.GetAttribute("print");
                    var livePreview = xmlReader.GetAttribute("live_preview");
                    var sendEventToLeaderBoard = xmlReader.GetAttribute("send_event_to_leader_board");
                    var showStartStopButton = xmlReader.GetAttribute("show_start_stop_button");
                    var minCameras = xmlReader.GetAttribute("min_cameras");
                    var minPictures = xmlReader.GetAttribute("min_pictures");
                    var linkToBasePicture = xmlReader.GetAttribute("link_base_picture");
                    var automatePageChange = xmlReader.GetAttribute("automate_page_change");
                    try
                    {
                        var clazz = StringUtil.GetClassName(fullClassName);
                        var type = Type.GetType($"{_currentAssembly}.Ui.Choice.{clazz}");

                        _choice = (Choice)Activator.CreateInstance(type);

                    }
                    catch (System.Exception e)
                    {
                        throw new System.Exception("Could not instantiate choice type: " + fullClassName, e);
                    }

                    _choice.No = Convert.ToInt32(no);

                    if (prefix != null)
                        _choice.Prefix = prefix;

                    if (suffix != null)
                        _choice.Suffix = suffix;

                    if (maxLength != null)
                        _choice.MaxLength = maxLength;

                    if (titleCase != null)
                    {
                        _choice.TitleCase = bool.Parse(titleCase);
                    }

                    // Si on lit un choix avec un lien vers la fonction de BasePicture, on donne une méthode
                    // d'auto-remplissage (pas tous les sous-types de choix utilise l'auto-remplissage)
                    // pour assigner la valeur générale système qui pointe vers ce fichier d'image.
                    if (linkToBasePicture != null)
                    {
                        if (bool.Parse(linkToBasePicture))
                        {
                            var tmpChoice = _choice;
                            tmpChoice.AutoPopulateAction = () =>
                            {
                                var file = DirectoryUtil.GetLocalNomadLogicDir() + "BasePictureFileInfo.txt";
                                if (!File.Exists(file)) return;
                                var baseFileLocation = File.ReadAllText(file);
                                if (!File.Exists(baseFileLocation))
                                {
                                    log.Warn("No valid base file detected: does not exists: " + baseFileLocation);
                                    return;
                                }
                                tmpChoice.SetAnswer(new Answer
                                {
                                    QuestionType = tmpChoice.Question.QuestionType,
                                    QuestionNo = tmpChoice.Question.No,
                                    ChoiceNo = tmpChoice.No,
                                    Response = baseFileLocation

                                });
                            };
                        }
                    }

                    if (enabled != null)
                    {
                        if (enabled.Equals("false"))
                            _choice.EnabledDefault = false;
                    }

                    if (fromBarCode != null)
                    {
                        if (fromBarCode.Equals("true"))
                        {
                            if (_choice is IBarcodeScannerListener tmpChoice && EnableDatasourceAndEvents)
                            {
                                tmpChoice.BarCodeActivated = true;
                                _questionnaire.AddScannerListener(tmpChoice);
                            }
                        }
                    }

                    /*
                    if (arrowsEnable != null)
                    {
                        if (arrowsEnable.Equals("true"))
                        {
                            _questionnaire.AddPageUpDownListener((ChoiceCombo)_choice);

                            try
                            {
                                var dir = xmlReader.GetAttribute("bkg_dir") ?? "backgrounds";
                                _questionnaire.InitListenForPageDown(_question, _project.Id, dir);
                            }
                            catch (System.Exception e)
                            {
                                throw new System.Exception("Problem creating fullscreen image viewer.", e);
                            }
                        }
                    }
                    */

                    if (appendBarCode != null)
                    {
                        if (appendBarCode.Equals("true"))
                        {
                            ((ChoiceText)_choice).AppendBarCode = true;
                        }
                    }

                    if (autosubmit != null && autosubmit.Equals("true"))
                    {
                        var canon = _choice as ChoiceVideoCaptureCanon;
                        canon?.SetActionOnVideoFileReady(_questionnaire.NextPageRequest);
                    }

                    if (countDown != null)
                    {
                        switch (_choice)
                        {
                            case ChoiceVideoCaptureCanon canon:
                                canon.SetCountDown(countDown);
                                break;
                            case ChoiceTimer timer:
                                var ct = timer;
                                ct?.SetCountDown(countDown);
                                break;
                        }
                    }

                    if (maxTime != null)
                    {
                        var timer = _choice as ChoiceTimer;
                        timer?.SetMaxTime(maxTime);
                        
                    }

                    if (print != null)
                    {
                        if (_choice is ChoicePhotoCaptureCanon canon && print.Equals("true"))
                        {
                            canon.Print = true;
                        }
                    }

                    if (livePreview != null)
                    {
                        if (_choice is ChoicePhotoCaptureCanon canon && livePreview.Equals("true"))
                        {
                            canon.LivePreview = true;
                        }
                    }

                    if (sendEventToLeaderBoard != null)
                    {
                        if (_choice is ChoiceTimer timer && _questionnaire.HasLeaderBoard)
                        {
                            timer._on_time_change += _questionnaire.OnChoiceTimerChange;
                        }
                    }

                    if (showStartStopButton != null)
                    {
                        var canon = _choice as ChoiceVideoCaptureCanon;
                        canon?.ShowStartStopButtons(bool.Parse(showStartStopButton));
                    }

                    
                    if (automatePageChange != null && bool.Parse(automatePageChange))
                    {
                        if (_choice is ChoiceCameraRing canon)
                            canon.SetActionOnPictureFileReady(_questionnaire.NextPageRequest);
                    }

                    if (minPictures != null)
                    {
                        if (_choice is ChoiceCameraRing canon)
                            canon.MinPictures = int.Parse(minPictures);
                    }

                    if (minCameras != null)
                    {
                        if (_choice is ChoiceCameraRing canon)
                            canon.MinCameras = int.Parse(minCameras);
                    }
                }

                _inChoice = true;
            }
            else if (xmlReader.Name.Equals("link"))
            {
                _link = new Link();
                _inLink = true;
            }
            else if (xmlReader.Name.Equals("popup_dialog"))
            {
                if (_loadChoices)
                    _popupDlg = new PopupDlg();

                _inPopupDlg = true;
            }
            else if (xmlReader.Name.Equals("html_popup_dialog") && _loadChoices)
            {
                _htmlPopupDlg = new HtmlPopupDlg();

                var file = xmlReader.GetAttribute("file");

                if (file != null)
                {
                    _htmlPopupDlg.HtmlFile = _project.ProjectDir + file;
                }
            }
            else if (xmlReader.Name.Equals("sub_choice"))
            {
                /*
                if (_loadChoices)
                {
                    var no = xmlReader.GetAttribute("no");
                    _subChoice = new SubChoice(Convert.ToInt32(no));
                }
                _inSubChoice = true;
                */
            }
            else if (xmlReader.Name.Equals("sub_choices_from_datasource") && _loadChoices)
            {
                if (!EnableDatasourceAndEvents) return;

                var datasourceName = xmlReader.GetAttribute("datasource");
                var field = xmlReader.GetAttribute("field");
                var lg = xmlReader.GetAttribute("lg");

                var datasource = _questionnaire.GetDatasource(datasourceName);

                if (datasource != null)
                    ((ChoiceCombo)_choice).AddSubChoicesFromDatasource(datasource, field, lg);
            }
            else if (xmlReader.Name.Equals("text"))
            {
                var lg = xmlReader.GetAttribute("lg");
                _lg = lg != null && lg.Equals("fr") ? Constants.FR : Constants.EN;
            }
            else if (xmlReader.Name.Equals("equation"))
            {
                var operatorr = xmlReader.GetAttribute("operator");

                var state = xmlReader.GetAttribute("state");

                _equation = new Equation(state, operatorr, _questionnaire);
                _question.AddEquation(_equation);
            }
            else if (xmlReader.Name.Equals("operand"))
            {
                var questionNo = xmlReader.GetAttribute("question_no");
                var choiceNo = xmlReader.GetAttribute("choice_no");
                //var subChoiceNo = xmlReader.GetAttribute("sub_choice_no");

                _equation.AddOperand(new Operand(questionNo, choiceNo));

                var q = _questionnaire.GetQuestion(Convert.ToInt32(questionNo));
                q.AddChangeAnswerListener(_question);
            }
            else if (xmlReader.Name.Equals("datasource_answer"))
            {
                if (!EnableDatasourceAndEvents) return;

                var choiceNo = xmlReader.GetAttribute("choice_no");
                var datasourceName = xmlReader.GetAttribute("datasource");
                var field = xmlReader.GetAttribute("field");

                var datasource = _questionnaire.GetDatasource(datasourceName);

                if (datasource == null) return;
                _datasourceAnswer = new DatasourceAnswer(_questionnaire, datasource, field, Convert.ToInt32(choiceNo));
                _question.AddDatasourceAnswer(_datasourceAnswer);
            }
            else if (xmlReader.Name.Equals("sub_choices_from_datasource_answer") && _loadChoices)
            {
                if (!EnableDatasourceAndEvents) return;

                var datasourceName = xmlReader.GetAttribute("datasource");
                var field = xmlReader.GetAttribute("field");

                var datasource = _questionnaire.GetDatasource(datasourceName);

                if (datasource == null) return;
                if (!(_choice is ChoiceCombo)) return;
                _subChoicesFromDatasourceAnswer = new SubChoicesFromDatasourceAnswer(_questionnaire, datasource, field, (ChoiceCombo)_choice);
                _question.AddSubChoicesFromDatasourceAnswer(_subChoicesFromDatasourceAnswer);
            }
            else if (xmlReader.Name.Equals("depends_on"))
            {
                var choiceNo = xmlReader.GetAttribute("choice_no");
                var questionNo = xmlReader.GetAttribute("question_no");
                var field = xmlReader.GetAttribute("field");

                if (_datasourceAnswer != null)
                {
                    _datasourceAnswer.SetDependsOn(Convert.ToInt32(questionNo), Convert.ToInt32(choiceNo), field);
                }
                else
                {
                    _subChoicesFromDatasourceAnswer?.SetDependsOn(Convert.ToInt32(questionNo), Convert.ToInt32(choiceNo), field);
                }

                var q = _questionnaire.GetQuestion(Convert.ToInt32(questionNo));
                q.AddChangeAnswerListener(_question);
            }
            else if (xmlReader.Name.Equals("laserbeam"))
            {
                // Permet d'associer un gestionnaire de faisceau à un choix de type ChoiceVideoCaptureCanon
                if (!(_choice is ChoiceVideoCaptureCanon) || !EnableDatasourceAndEvents) return;

                var time = xmlReader.GetAttribute("record_time");

                var cvcc = (ChoiceVideoCaptureCanon)_choice;

                // AbstractConfiguration de base pour le laser de Daniel Bleau
                var lbm = new LaserBeamManager("F310", 4);
                float timeFloat = 30;

                if (time != null)
                {
                    try
                    {
                        timeFloat = float.Parse(time);
                    }
                    catch (FormatException e)
                    {
                        log.Error(e, "Could not parse time of video. Using 30 secs instead.");
                    }
                }

                cvcc.SetLaserBeamManager(lbm);
                cvcc.SetRecordTime(timeFloat);
            }
            else if (xmlReader.Name.Equals("leader_board"))
            {
                _questionnaire.HasLeaderBoard = true;
            }
        }

        private void EndElement(XmlReader xmlReader)
        {
            if (xmlReader.Name.Equals("questionnaire"))
            {
                if (_questionnaire.HasScanChoice && EnableDatasourceAndEvents)
                {
                    try
                    {
                        _questionnaire.InitScanner();
                    }
                    catch (System.Exception ex)
                    {
                        MsgBox.Show(WorkingArea.Container, ex.Message, "iBrand(tm)", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    }
                }

                if (_questionnaire.XangleConfigFile != null)
                {
                    try
                    {
                        _questionnaire.InitXangleManager();
                    }
                    catch (System.Exception ex)
                    {
                        MsgBox.Show(WorkingArea.Container, ex.Message, "iBrand(tm)", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    }
                }
            }
            else if (xmlReader.Name.Equals("default_answer"))
            {

                    _question.AddDefaultAnswer(_lg??"en",_defaultAnsChoiceNo,_ansValue);
               
                
                //_answer = new Answer(_question.QuestionType,_question.No,_choice.No,xmlReader.Value);
            }
            else if (xmlReader.Name.Equals("question"))
            {
                _question = null;
                _inQuestion = false;
            }
            else if (xmlReader.Name.Equals("choice"))
            {
                if (_loadChoices)
                {
                    /*if (_answer != null)
                    {
                        _choice.SetAnswer(_answer);
                    }*/
                    
                    _question.AddChoice(_choice);
                    _choice.SetText();
                }
                _choice = null;
                _inChoice = false;
            }
            else if (xmlReader.Name.Equals("link"))
            {
                _question.SetLink(_link);
                _link = null;
                _inLink = false;
            }
            else if (xmlReader.Name.Equals("submit_action"))
            {
                _questionnaire.SubmitAction = _submitAction;
                _submitAction = null;
            }
            else if (xmlReader.Name.Equals("popup_dialog"))
            {
                if (_loadChoices)
                    _link.SetPopUpDlg(_popupDlg);
                _popupDlg = null;
                _inPopupDlg = false;
            }
            else if (xmlReader.Name.Equals("html_popup_dialog") && _loadChoices)
            {
                _link.SetPopUpDlg(_htmlPopupDlg);
                _htmlPopupDlg = null;
            }
            else if (xmlReader.Name.Equals("sub_choice"))
            {
                /*
                if (_loadChoices)
                    ((ChoiceCombo)_choice).AddSubChoice(_subChoice);
                _subChoice = null;
                _inSubChoice = false;
                */
            }
            else if (xmlReader.Name.Equals("text"))
            {
                /*
                if (_inSubChoice)
                {
                    if (_loadChoices)
                        _subChoice.SetText(_lg, _text);
                }
                else 
                */
                if (_inChoice)
                {
                    if (_loadChoices)
                        _choice.SetText(_lg, _text);
                }
                else if (_inPopupDlg)
                    _popupDlg.SetText(_lg, _text);
                else if (_inLink)
                {
                    if (_loadChoices)
                        _link.SetText(_lg, _text);
                }
                else if (_inQuestion)
                {
                    _question.SetText(_lg, _text);
                }

                _text = null;
                _lg = null;
            }
            else if (xmlReader.Name.Equals("equation"))
            {
                _equation = null;
            }
            else if (xmlReader.Name.Equals("datasource_answer"))
            {
                _datasourceAnswer = null;
            }
            else if (xmlReader.Name.Equals("sub_choices_from_datasource_answer") && _loadChoices)
            {
                _subChoicesFromDatasourceAnswer = null;
            }
        }

    }
}
