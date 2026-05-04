using System;
using System.IO;
using System.Linq;
using System.Text;
using NLog;
using NomadLogic.Common.Serveur;
using NomadLogic.Common.Serveur.TerminalStructure;
using NomadLogic.Common.Services;
using NomadLogic.Common.Services.Configuration;
using NomadLogic.Common.Util;
using NomadLogic.Common.Util.Json;
using NomadLogic.iBrandDesktop.Services;
using NomadLogic.iBrandDesktop.Ui.Choice;
using NomadLogic.iBrandDesktop.Ui.Question;
using NomadLogic.iBrandDesktop.Util.SubmitAction.BaseClasses;

namespace NomadLogic.iBrandDesktop.Util
{
    public class JujuQuestionnaireLoader : QuestionnaireLoaderAbstract
    {
        private static readonly Logger log = LogManager.GetCurrentClassLogger();

        private readonly string _currentAssembly = "NomadLogic.iBrandDesktop";
        private readonly TerminalInfo _terminal;

        public JujuQuestionnaireLoader(TerminalInfo ti)
        {
            _terminal = ti;
        }

        public override Questionnaire Load(bool value)
        {
            var baseDir = DirectoryUtil.GetLocalIbrandTerminalDir(_terminal.ProjectId, _terminal.QuestionnaireId, _terminal.TerminalId);
            var filePath = StringUtil.PathCombineFromUnix(baseDir, _terminal.LoaderFile);
            var fileName = new FileInfo(filePath);

            if (!fileName.Exists)
            {
                throw new System.Exception("Questionnaire file does not exists (relative path):" + _terminal.LoaderFile);
            }

            var td = JsonUtil.ReadJsonFile<TerminalIbrandDefinition>(fileName);

            if (td == null)
            {
                throw new System.Exception("Could not load questionnaire from JSON file: " + fileName.FullName);
            }

            var configuration = ServicesContainer.GetService<NomadConfiguration>();

            var configLg = configuration.PreferedLanguage;

            if (td.Languages == null || !td.Languages.Any())
            {
                throw new System.Exception("Questionnaire definition does not contains any languages: " + fileName.FullName);
            }

            var possibleLanguages = td.Languages;

            // Vérifie que la langue donnée par le système est compatible avec les langues possibles du questionnaire.
            // Si ce n'est pas le cas, on assigne la première langue possible du questionnaire comme langue active.
            if (!possibleLanguages.Any(l => l.Equals(configLg, StringComparison.InvariantCultureIgnoreCase)))
            {
                configLg = possibleLanguages.First();
            }

            // Transformation des langues en format "{1}|{2}|..."
            var sb = new StringBuilder();
            foreach (var lg in td.Languages)
            {
                if (sb.Length != 0) sb.Append('|');
                sb.Append(lg);
            }

            var pathProj = DirectoryUtil.GetLocalIbrandProjectDir(_terminal.ProjectId);
            var questionnaire = new IBrandQuestionnaire(_terminal.QuestionnaireId, sb.ToString(), pathProj)
            {
                CanLoadLastRespondent = td.LoadLastRespondent,
                IsFullScreen = td.IsFullSCreen,
                LoaderClass = td.LoaderClass,
                LanguageCurrent = configLg,
                // TODO Éventuellement, il faudra que le terminal contienne le questionnaire, et non vice-versa.
                Terminal = _terminal,
                ServerSyncMode = td.SynchronizationMode,
                SyncFinisherClassName = td.SyncFinisherClassName
            };

            if (td.InitBarcodeScanner)
            {
                questionnaire.InitScanner();
            }

            if (td.XangleConfig != null)
            {
                questionnaire.XangleConfigFile = td.XangleConfig;
                questionnaire.InitXangleManager();
            }

            log.Debug("Questionnaire initial instanciation done: " + _terminal.QuestionnaireId);
            
            // Charge les questions
            foreach (var tq in td.Questions)
            {
                log.Trace("Loading question " + tq.Num);
                var clazz = StringUtil.GetClassName(tq.Clazz);
                /*
                
                var type = Type.GetType($"{_currentAssembly}.Ui.Question." + clazz);
                // ReSharper disable once AssignNullToNotNullAttribute
                var qTmp = (Question)Activator.CreateInstance(type);
                */
                var qTmp = ClassUtil.InstanciateClassFromName<Question>(clazz, $"{_currentAssembly}.Ui.Question");
                qTmp.No = tq.Num;
                qTmp.Mandatory = tq.Mandatory;
                qTmp.Enabled = tq.Enabled;
                qTmp.Recyclable = tq.Recyclable;
                qTmp.ToSave = tq.ToSave;
                qTmp.ToSync = tq.ToSync;
                qTmp.Visible = tq.Visible;
                qTmp.Tag = tq.Tag;
                qTmp.Language = configLg;
                
                // Textes
                if (tq.Texts != null)
                {
                    using (var parser = tq.Texts.GetEnumerator())
                    {
                        while (parser.MoveNext())
                        {
                            var kvp = parser.Current;
                            qTmp.SetText(kvp.Key, kvp.Value);
                        }
                    }

                    log.Trace($"Texts loaded ({tq.Texts.Count})");
                }
                else
                {
                    throw new System.Exception($"No texts for question {tq.Num} in terminal {_terminal.TerminalId}");
                }

                // Charge les choix

                
                if (tq.Choices == null || !tq.Choices.Any())
                {
                    // On créer par défaut un seul choix.
                    tq.Choices = new[]
                    {
                        new TerminalChoice
                        {
                            No = 1
                        }
                    };
                }
                
                foreach (var c in tq.Choices)
                {
                    var defaultType = qTmp.GetDefaultChoiceClassType();
                    var cType = defaultType;
                    if (!string.IsNullOrWhiteSpace(c.Clazz))
                    {
                        // Classe personalisé pour le système Kiosque
                        var cClass = StringUtil.GetClassName(c.Clazz);
                        var tmpType = Type.GetType($"{_currentAssembly}.Ui.Choice." + cClass);
                        if (!tmpType.InheritsOrImplements(defaultType))
                        {
                            throw new System.Exception($"Choice type for question {tq.Num} does not derives from following type: " + defaultType);
                        }
                    }

                    log.Trace($"Loading choice {tq.Num}-{c.No} -> {cType.Name}");

                    // Instanciation par réflexion.
                    var cTmp = (Choice) Activator.CreateInstance(cType);
                    cTmp.Enabled = true; // Pas de config
                    cTmp.No = c.No;
                    cTmp.Question = qTmp;

                    // Textes
                    if (c.Texts != null && c.Texts.Any())
                    {
                        using (var parser = c.Texts.GetEnumerator())
                        {
                            while (parser.MoveNext())
                            {
                                var kvp = parser.Current;
                                cTmp.SetText(kvp.Key, kvp.Value);
                            }
                        }

                        log.Trace($"Choice Text loaded ({tq.Texts.Count})");
                    }

                    qTmp.AddChoice(cTmp);
                }

                questionnaire.AddQuestion(qTmp);
            }

            log.Debug("Finished question and choice loading.");

            // SubmitAction
            //TODO Présentement, on ne prend que la premiere instance de SubmitAction.
            if (!string.IsNullOrWhiteSpace(td.SubmitActionClass))
            {
                var sClass = StringUtil.GetClassName(td.SubmitActionClass);
                var sType = Type.GetType($"{_currentAssembly}.Util.SubmitAction.{sClass}") ?? 
                            Type.GetType($"{_currentAssembly}.Util.SubmitAction.Generic.{sClass}");

                if (sClass == null || sType == null)
                {
                    throw new System.Exception("Invalid Submit Action class name: " + sClass);
                }
                
                log.Trace("Loading SubmitAction class: " + sClass);

                try
                {
                    // ReSharper disable once AssignNullToNotNullAttribute
                    var sAction = (AbstractSubmitAction)Activator.CreateInstance(sType);
                    questionnaire.SubmitAction = sAction;

                    // Lance le event pour vérifier les répondants en attente.
                    var serv = ServicesContainer.GetService<SynchronizationService>();
                    serv?.SignalNewRespondent();
                }
                catch (System.Exception e)
                {
                    throw new System.Exception("Could not instantiate Submit Action: " + sClass, e);
                }
            }

            // Liste d'exclusion de groupes dynamiques. Utilisé quand il faut ignorer certains groupes dynamiques (les question ont une option de ne pas sauver)
            questionnaire.DynamicHeaderSyncExclusionList = td.DynamicHeaderSyncExclusionList;
            
            log.Debug($"Project, Questionnaire and Terminal fully loaded: {_terminal.ProjectId}/{_terminal.QuestionnaireId}/{_terminal.TerminalId}");

            return questionnaire;
        }
    }
}
