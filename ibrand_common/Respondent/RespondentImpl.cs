using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows.Forms;
using NomadLogic.Common.Util;
#pragma warning disable CS0612

namespace NomadLogic.Common.Respondent
{
    /// <summary>
    /// Implémentation d'un répondant.
    /// Cette classe tente de découpler le concept de répondant de la version très fortement couplée du module "ibrand_client".
    /// </summary>
    public class RespondentImpl : IRespondent
    {
        private readonly Dictionary<QuestionIdentifier, Answer> _responses;

        /// <summary>
        /// Constructeur de base.
        /// </summary>
        /// <param name="lg">Le code de langue.</param>
        /// <param name="questionnaireDir">Le répertoire de base du questionnaire.</param>
        /// <param name="eventId">L'identifiant numérique de l'événement.</param>
        /// <param name="id">L'identifiant unique, venant de la BD (si à été écrit au moins une fois dans la BD).</param>
        public RespondentImpl(string lg, string questionnaireDir, int eventId, int id = -1)
        {
            Language = lg;
            QuestionnaireDirectory = questionnaireDir;
            EventId = eventId;
            _responses = new Dictionary<QuestionIdentifier, Answer>();
            Id = id;
        }

        /// <summary>
        /// Ajoute (ou met à jour si existant) une réponse, via référence.
        /// </summary>
        /// <param name="questionNo">Le numéro de question</param>
        /// <param name="choiceNo">Le numéro de choix.</param>
        /// <param name="response">La réponse texte.</param>
        public void AddAnswer(int questionNo, int choiceNo, string response)
        {
            var qi = new QuestionIdentifier(questionNo, choiceNo);
            if (!_responses.ContainsKey(qi))
            {
                _responses.Add(qi, new Answer
                {
                    QuestionIdentifier = qi,
                    Response = response
                });
            }
        }

        /// <summary>
        /// Ajoute (ou met à jour si existant) une réponse, via référence.
        /// </summary>
        /// <param name="answer">La référence de réponse à ajouter.</param>
        public void AddAnswer(Answer answer) {
            if (answer == null) return;
            AddAnswer(answer.QuestionNo, answer.ChoiceNo, answer.Response);
        }

        /// <inheritdoc/>
        public Answer GetAnswer(int questionNo, int choiceNo = 1) {
            return GetAnswerDo(questionNo, choiceNo);
        }

        /// <inheritdoc/>
        public Answer GetAnswer(QuestionIdentifier qi)
        {
            return qi == null ? null : GetAnswerDo(qi.QuestionNo, qi.ChoiceNo);
        }

        /// <inheritdoc/>
        public string GetAnswerText(int questionNo, int choiceNo = 1) {
            return GetAnswerDo(questionNo, choiceNo)?.Response;
        }

        /// <inheritdoc/>
        public string GetAnswerText(QuestionIdentifier qi) {
            return qi == null ? null : GetAnswerDo(qi.QuestionNo, qi.ChoiceNo)?.Response;
        }

        /// <inheritdoc/>
        public string Language { get; set; }
        
        private Answer GetAnswerDo(int questionNo, int choiceNo = 1) {

            var qi = new QuestionIdentifier(questionNo, choiceNo);
            return !_responses.ContainsKey(qi) ? null : _responses[qi];
        }
        
        [Obsolete]
        public string QuestionnaireDirectory { get; }

        [Obsolete]
        /// <inheritdoc/>
        public int EventId { get; set; }

        public string TerminalDirectory => DirectoryUtil.GetLocalIbrandTerminalDir(ProjectId, QuestionnaireId, TerminalId);

        /// <inheritdoc/>
        public string GetContextDirectory()
        {
            return Application.StartupPath;
        }

        /// <inheritdoc/>
        public string Date { get; set; }

        /// <inheritdoc/>
        public int ProjectId { get; set; }
        
        /// <inheritdoc/>
        public int QuestionnaireId { get; set; }

        /// <inheritdoc/>
        public int TerminalId { get; set; }

        /// <inheritdoc/>
        public int Id { get; set; }

        /// <inheritdoc/>
        public string TimeZone { get; set; }

        /// <inheritdoc/>
        public bool IsSynchronized { get; set; }

        /// <inheritdoc/>
        public List<DynamicHeader> DynamicHeaders { get; set; } = new List<DynamicHeader>();

        /// <inheritdoc/>
        public List<Answer> Answers
        {
            get => _responses.Values.ToList();

            set
            {
                _responses.Clear();
                if (value == null)
                {
                    return;
                }

                foreach (var ans in value)
                {
                    AddAnswer(ans);
                }
            }
        }

        /// <inheritdoc/>
        public string ExternalUniqueIdentifier { get; set; }
    }
}
