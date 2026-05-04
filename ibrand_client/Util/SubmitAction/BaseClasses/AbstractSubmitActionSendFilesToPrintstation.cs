using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using NLog;

namespace NomadLogic.iBrandDesktop.Util.SubmitAction.BaseClasses
{
    /// <summary>
    /// Classe abstraite pour les informations de base pour l'envoi vers un Printstation à partir d'une SubmitAction.
    /// <br/>Ce type d'envoi automatiquement créer un fichier ZIP qui contient les éléments suivants:
    /// 1- Un fichier JSON qui contient les réponses des questions spécifiées dans la propriété "answer_questions". Si ces questions oitent vers un fichier, elle sont copiées intégralements.
    /// 2- Tous les fichier référencés par les questions spécifiées dans la propriété "file_questions". Le nom des fichiers est EXACTEMENT le même que celui dans la réponse d'où il est issu, sans son chemin complet.
    /// C'est la tâche de celui qui reçoit ce paquet de l'extraire correctement et de transformer les références de fichier dans les question en chemin valide pour la nouvelle machine.
    /// </summary>
    public abstract class AbstractSubmitActionSendFilesToPrintstation : AbstractSubmitAction, ISubmitActionSendFilesToPrintstation
    {
        private static readonly Logger log = LogManager.GetCurrentClassLogger();

        private const string NBR_MIN_PRINTSTATION = "nbr_min_printstation";
        private const string FILE_QUESTIONS = "file_questions";
        private const string ANSWER_QUESTIONS = "answer_questions";
        private const string FILE_TO_SHOW_GALLERY = "file_to_show_gallery";
        private int[] _fileQuestions;
        private int[] _answerQuestions;

        /// <inheritdoc />
        protected override void InstanciateSubmitActionPools()
        {
            AddSubmitActionWorkerPool("SendFilesToPrintstationSubmitAction", 1);
        }

        /// <inheritdoc />
        protected override void ValidateConfigData()
        {
            if (!_configData.ContainsKey(NBR_MIN_PRINTSTATION))
            {
                throw new System.Exception("Missing critical configuration in config file: " + NBR_MIN_PRINTSTATION);
            }
            if (!_configData.ContainsKey(FILE_QUESTIONS))
            {
                throw new System.Exception("Missing critical configuration in config file: " + FILE_QUESTIONS);
            }
            if (!_configData.ContainsKey(FILE_TO_SHOW_GALLERY))
            {
                throw new System.Exception("Missing critical configuration in config file: " + FILE_TO_SHOW_GALLERY);
            }

            MinPrintstationToSend = int.Parse(_configData[NBR_MIN_PRINTSTATION]);
            if (MinPrintstationToSend < 0)
            {
                throw new System.Exception("Cannot send files to a negative number of Printstations.");
            }

            FileToShowGallery = int.Parse(_configData[FILE_TO_SHOW_GALLERY]);
            
            var tmpListeQuestion = new List<int>();
            foreach (var t in _configData[FILE_QUESTIONS].Split(';').Where(t => !string.IsNullOrEmpty(t)))
            {
                try
                {
                    tmpListeQuestion.Add(int.Parse(t));
                }
                catch (System.Exception e)
                {
                    log.Error(e, "Error parsing questions numbers. Must be in the format 'X;Y;Z', where only integers are used. Following string is inadequate: {0}", _configData[FILE_QUESTIONS]);
                    throw;
                }
            }

            _fileQuestions = tmpListeQuestion.ToArray();
            if (_fileQuestions.Length < 1)
            {
                throw new System.Exception("At least one question is necessary to send to Printstations.");
            }

            if (_configData.ContainsKey(ANSWER_QUESTIONS))
            {
                var tmpAnswerQuestions = new List<int>();
                foreach (var t in _configData[ANSWER_QUESTIONS].Split(';').Where(t => !string.IsNullOrEmpty(t)))
                {
                    try
                    {
                        tmpAnswerQuestions.Add(int.Parse(t));
                    }
                    catch (System.Exception e)
                    {
                        log.Error(e, "Error parsing questions numbers. Must be in the format 'X;Y;Z', where only intergers are used. Following string is inadequate: {0}", _configData[ANSWER_QUESTIONS]);
                        throw;
                    }
                }

                _answerQuestions = tmpAnswerQuestions.ToArray();
            }
            else
            {
                _answerQuestions = new int[0];
            }
        }

        public bool SendToPrintstationActivated => true;

        /// <inheritdoc />
        public int MinPrintstationToSend { get; private set; }

        /// <inheritdoc />
        public int FileToShowGallery { get; private set; }

        /// <inheritdoc />
        public ReadOnlyCollection<int> FileQuestions => Array.AsReadOnly(_fileQuestions);
        
        /// <inheritdoc />
        public ReadOnlyCollection<int> AnswerQuestions => Array.AsReadOnly(_answerQuestions);

    }
}
