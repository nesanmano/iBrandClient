using System;
using System.Collections;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Windows.Forms;
using JetBrains.Annotations;
using NLog;
using NomadLogic.Common;
using NomadLogic.Common.Respondent;
using NomadLogic.Common.Services;
using NomadLogic.Common.Services.Configuration;
using NomadLogic.Common.Util;
using NomadLogic.iBrandDesktop.Datasource;
using NomadLogic.iBrandDesktop.Ui.Choice;

#pragma warning disable 618

namespace NomadLogic.iBrandDesktop.Ui.Question
{
    public abstract class Question : IScreenElement
    {
        private readonly Logger log = LogManager.GetCurrentClassLogger();

        public event OnAnswerChangedDelegate OnAnswerChanged;

        private readonly Hashtable _textTable;
        
        private readonly List<Choice.Choice> _choiceList;

        protected Link _link;

        private List<DatasourceAnswer> _datasourceAnswerList;

        private List<SubChoicesFromDatasourceAnswer> _subChoicesFromDatasourceAnswerList;

        [Obsolete]
        private bool _mandatoryDefault;
        [Obsolete]
        private bool _enabledDefault = true;
        [Obsolete]
        private bool _toSaveDefault = true;
        
        private List<Question> _changeAnswerListeners;

        private readonly List<Equation> _equations = new List<Equation>();
        
        private Label _textLabel;

        protected string _activeLanguage;
        private bool _enabled;
        private readonly IDictionary<string, IDictionary<int, Answer>> _defaultAnswersDic = new Dictionary<string, IDictionary<int, Answer>>();

        /// <summary>
        /// Obtient le type associé à la classe de l'instance de question pour le système de synchronisation de Nomad Logic.
        /// </summary>
        public abstract QuestionTypeEnum QuestionType { get; }

        /// <summary>
        /// Obtient le type numérique associé à la classe de l'instance de question pour le système de synchronisation de Nomad Logic.
        /// Il suffit normalement de "caster" à un INT32. Cette propriété est plus propre et le fait pour nous.
        /// </summary>
        public int QuestionTypeInt => (int) QuestionType;

        protected Question()
        {
            _choiceList = new List<Choice.Choice>();
            _textTable = new Hashtable();
           
            _changeAnswerListeners = null;
        }

        public Panel QuestionPanel { get; private set; }

        public virtual string Language
        {
            set
            {
                _activeLanguage = value;

                var nbrChoice = _choiceList.Count;

                for (var i = 0; i < nbrChoice; ++i)
                {
                    var c = _choiceList[i];
                    c.Language = value;
                }

                if (_link != null)
                    _link.Language = value;
            }
        }

        public bool Recyclable { get; set; } = true;

        public bool ToSave { get; set; } = true;

        [Obsolete]
        public bool Visible { get; set; } = true;

        /// <summary>
        /// Retourne une enumération en lecture seule des choix de cette question.
        /// </summary>
        public IReadOnlyList<Choice.Choice> Choices => _choiceList.AsReadOnly().ToList();

        public void SetLink(Link link)
        {
            _link = link;
        }

        public bool Enabled
        {
            set
            {
                if (_textLabel != null)
                {
                    _textLabel.ForeColor = value ? Color.Black : Color.Gray;
                }

                var nbrChoice = _choiceList.Count;

                for (var i = 0; i < nbrChoice; ++i)
                {
                    var c = _choiceList[i];
                    c.Enabled = value;
                }
                _enabled = value;
            }

            get => _enabled;
        }

        public void AddChangeAnswerListener(Question question)
        {
            if (_changeAnswerListeners == null)
                _changeAnswerListeners = new List<Question>();

            _changeAnswerListeners.Add(question);
        }

        public void AddDatasourceAnswer(DatasourceAnswer datasourceAnswer)
        {
            if (_datasourceAnswerList == null)
                _datasourceAnswerList = new List<DatasourceAnswer>();

            _datasourceAnswerList.Add(datasourceAnswer);
        }

        public void AddSubChoicesFromDatasourceAnswer(SubChoicesFromDatasourceAnswer subChoicesFromDatasourceAnswer)
        {
            if (_subChoicesFromDatasourceAnswerList == null)
                _subChoicesFromDatasourceAnswerList = new List<SubChoicesFromDatasourceAnswer>();

            _subChoicesFromDatasourceAnswerList.Add(subChoicesFromDatasourceAnswer);
        }

        /// <summary>
        /// C'est un callback pour les choix.
        /// </summary>
        internal void OnAnswerChange()
        {
            if (_changeAnswerListeners != null)
            {
                var nbrAnswerListener = _changeAnswerListeners.Count;

                for (var i = 0; i < nbrAnswerListener; ++i)
                {
                    var question = _changeAnswerListeners[i];
                    question.OnAnswerChangeNotify();
                }
            }

            OnAnswerChanged?.Invoke(this);
        }

        public void AddEquation(Equation e)
        {
            if (e == null) return;
            _equations.Add(e);
        }

        /// <summary>
        /// Retourne une enumération en lecture seule des choix de cette question.
        /// </summary>
        public IReadOnlyList<Equation> Equations => _equations.AsReadOnly().ToList();

        public int No { get; set; }

        public void Reset()
        {
            Mandatory = _mandatoryDefault;

            Enabled = _enabledDefault;

            ToSave = _toSaveDefault;

            Focusable = true;

            SetChoicesFocusable(Focusable);
            
            if (Recyclable)
                ResetAnswers();
        }

        public bool Mandatory { get; set; } = true;

        public Font Font { get; } = new Font("Arial", 10F, FontStyle.Regular);

        [Obsolete]
        public bool EnabledDefault
        {
            set
            {
                _enabledDefault = value;
                Enabled = value;
            }

            get => _enabledDefault;
        }

        [Obsolete]
        public bool ToSaveDefault
        {
            set
            {
                _toSaveDefault = value;
                ToSave = value;
            }

            get => _toSaveDefault;
        }

        public bool ToSync { get; set; } = true;
        
        [Obsolete]
        public bool MandatoryDefault
        {
            set
            {
                _mandatoryDefault = value;
                Mandatory = _mandatoryDefault;
            }

            get => _mandatoryDefault;
        }

        public void SetText(string language, string text)
        {
            _textTable.Add(language, text);
        }

        public virtual void SetAnswer([NotNull] Answer answer)
        {
            var choiceNo = answer.ChoiceNo;
            var choice = GetChoice(choiceNo);
            choice.SetAnswer(answer);
        }

        public virtual void AddChoice([NotNull] Choice.Choice choice)
        {
            choice.Question = this;
            _choiceList.Add(choice);
        }

        public virtual int GetHeight(int availableWidth, Graphics g)
        {
            var textLabelWidth = availableWidth - (2 * Constants.QUESTION_MARGIN) - (2 * Constants.QUESTION_TEXT_MARGIN);

            var nbrOfLines = 0;
            var text = GetText();
            if (text.Length > 0)
                nbrOfLines = StringUtil.GetNumberOfLines(g, Font, textLabelWidth, text);

            var height = nbrOfLines * Font.GetHeight();

            var choiceY = (int)Math.Ceiling(height) + Constants.CHOICE_PAD;

            var choiceAvailableWidth = availableWidth - (2 * Constants.QUESTION_MARGIN) - (2 * Constants.QUESTION_TEXT_MARGIN);

            var nbrChoice = _choiceList.Count;
            
            for (var i = 0; i < nbrChoice; i++)
            {
                var choice = _choiceList[i];
                choiceY += choice.GetHeight(choiceAvailableWidth, g);
            }

            return choiceY + Constants.QUESTION_PAD;
        }

        public virtual int AddToUserControl(UserControl screenPanel, int availableWidth, int y, Graphics g)
        {
            QuestionPanel = new Panel {Location = new Point(Constants.QUESTION_MARGIN, y)};
            screenPanel.Controls.Add(QuestionPanel);

            var textLabelHeight = 0;

            if (GetText().Length > 0)
            {
                _textLabel = new Label
                {
                    Font = Font,
                    Text = GetText(),
                    Location = new Point(Constants.QUESTION_TEXT_MARGIN, 2),
                    Name = "Q_" + No
                };

                var textLabelWidth = availableWidth - (2 * Constants.QUESTION_MARGIN) - (2 * Constants.QUESTION_TEXT_MARGIN);

                var nbrOfLines = StringUtil.GetNumberOfLines(g, Font, textLabelWidth, GetText());

                var height = nbrOfLines * Font.GetHeight();

                //On ajoute 1 pixel par ligne > 1. Bug Fix. C'est comme si il avait un espace en chaque ligne de 1 pixel qui fait que la derniere ligne ne s'affiche pas. 
                var toAdd = nbrOfLines -1;

                _textLabel.Size = new Size(textLabelWidth, (int)Math.Ceiling(height) + toAdd );

                QuestionPanel.Controls.Add(_textLabel);

                textLabelHeight = _textLabel.Size.Height;
            }

            var nbrChoice = _choiceList.Count;

            var choiceY = textLabelHeight + Constants.CHOICE_PAD;

            var choiceAvailableWidth = availableWidth - (2 * Constants.QUESTION_MARGIN) - (2 * Constants.QUESTION_TEXT_MARGIN);
            
            for (var i = 0; i < nbrChoice; i++)
            {
                var choice = _choiceList[i];

                // On utilise ici le patron de Template Method, au cas où ce comportement varie dans une classe d'implémentation.
                choiceY += AddChoiceToUserControl(choice, QuestionPanel, Constants.QUESTION_MARGIN, choiceY, g, choiceAvailableWidth);
            }

            var linkY = choiceY;

            if (_link != null)
                linkY += _link.AddToUserControl(QuestionPanel, Constants.QUESTION_MARGIN, linkY, g, choiceAvailableWidth);

            QuestionPanel.Size = new Size(availableWidth - (2 * Constants.QUESTION_MARGIN), linkY);

            // Encore Template method
            linkY += AddAdditionalQuestionElement(QuestionPanel, Constants.QUESTION_MARGIN, linkY, g, choiceAvailableWidth);

            return linkY + Constants.QUESTION_PAD;
        }

        /// <summary>
        /// Une fois toutes les questions ajoutées, on peut ajouter un élément propre à la question à la fin des choix.
        /// </summary>
        protected virtual int AddAdditionalQuestionElement(Panel p, int margin, int choiceY, Graphics g, int choiceMaxWidth)
        {
            return 0; // Implémentation par défaut
        }

        /// <summary>
        /// Appelé pour chaque choix à ajouter au paneau de Winform qui compose cette question.
        /// </summary>
        protected virtual int AddChoiceToUserControl(Choice.Choice c, Panel p, int margin, int choiceY, Graphics g, int choiceMaxWidth)
        {
            return c.AddToUserControl(p, margin, choiceY, g, choiceMaxWidth);
        }

        private void SetChoicesFocusable(bool focusable)
        {
            var nbrChoice = _choiceList.Count;

            for (var i = 0; i < nbrChoice; ++i)
            {
                var c = _choiceList[i];
                c.SetFocusable(focusable);
            }
        }

        /// <summary>
        /// Retourne le choix de la question. Retourne null si l'index est incorrect.
        /// </summary>
        public virtual Choice.Choice GetChoice(int choiceNo)
        {
            var index = choiceNo - 1;
            return _choiceList.Count <= index ? null : _choiceList[choiceNo - 1];
        }

        public string GetText()
        {
            if (_activeLanguage == null)
            {
                var configuration = ServicesContainer.GetService<NomadConfiguration>();
                _activeLanguage = configuration.PreferedLanguage;
            }

            var text = (string)_textTable[_activeLanguage];

            if (text != null) return text;
            //Get the first supported language which we have text for.
            foreach (var lg in Constants.SUPPORTED_LANGUAGES)
            {
                text = (string)_textTable[lg];

                if (text != null)
                    return text;
            }

            return "";

        }

        // TODO Déplacer dans QuestionAnswersValidationBase
        public virtual bool ValidateAnswersPattern()
        {
            return true;
        }

        // TODO Déplacer dans QuestionAnswersValidationBase
        public virtual bool ValidateDatasourceField()
        {
            return true;
        }
        
        // TODO Déplacer dans QuestionAnswersValidationBase
        public virtual string AnswersPattern => "";

        public bool Focusable { get; set; } = true;
        public string Tag { get; set; }

        public virtual void OnAnswerChangeNotify()
        {
            if (_equations != null)
            {
                var equationMet = false;
                
                foreach (var equation in _equations)
                {
                    equationMet = equation.Evaluate();

                    if (!equationMet)
                        break;
                }

                // TODO On prend la premiere equation pour les etats. A Changer. FSF
                if (_equations[0].HasFocusableState())
                {
                    Focusable = equationMet;
                    SetChoicesFocusable(Focusable);
                    if (!Focusable && HasFocus())
                    {
                        //_screen.MoveFocus(); // TODO
                    }
                }

                if (_equations[0].HasMandatoryState())
                    Mandatory = equationMet;

                if (_equations[0].HasEnabledState())
                {
                    Enabled = equationMet;
                }

                if (!equationMet)
                    ResetAnswers();

                QuestionPanel.Invalidate();
            }

            if (_datasourceAnswerList != null)
            {
                var nbrAnswerDatasource = _datasourceAnswerList.Count;

                for (var i = 0; i < nbrAnswerDatasource; ++i)
                {
                    var da = _datasourceAnswerList[i];
                    var answer = new Answer{QuestionType = (QuestionTypeEnum)QuestionTypeInt, QuestionNo = No, ChoiceNo = da.ChoiceNo, Response = da.GetAnswer()};

                    var c = _choiceList[da.ChoiceNo - 1];

                    if (c is ChoiceCombo)
                    {
                        try
                        {
                            answer.ChoiceNo = int.Parse(answer.Response);
                        }
                        catch(System.Exception)
                        {
                            // The string can't be parsed - we'll Set the Combo's subchoice to zéro
                            answer.ChoiceNo = 0;
                        }
                    }
                    c.SetAnswer(answer);
                }
            }

            if (_subChoicesFromDatasourceAnswerList != null)
            {
                var subChoicesFromDatasource = _subChoicesFromDatasourceAnswerList.Count;

                for (var i = 0; i < subChoicesFromDatasource; ++i)
                {
                    var scfda = _subChoicesFromDatasourceAnswerList[i];
                    scfda.AddSubChoices();
                }
            }
        }

        public void SetFocus()
        {
            if (_choiceList.Count <= 0) return;
            var c = _choiceList[0];
            c.SetFocus();
        }

        public virtual bool HasFocus()
        {
            if (_choiceList.Count <= 0) return false;
            var c = _choiceList[0];
            return c.HasFocus();
        }

        public virtual bool ValidateAnswers()
        {
            if (!Equations.Any()) return !Mandatory || IsAnswered();

            var equation = Equations[0];// TODO On prend la première equation. A voir. FSF.
            var equationMet = equation.Evaluate();

            if (equation.HasMandatoryState())
                Mandatory = equationMet;

            return !Mandatory || IsAnswered();
        }

        /// <summary>
        /// Indique que la question est répondue de façon satisfaisante.
        /// </summary>
        /// <returns>True si la question est considérée répondue.</returns>
        public virtual bool IsAnswered()
        {
            return Choices.Any(c => c.IsAnswered());
        }

        public void LoadAnswerInUiElement(Answer answer)
        {
            var nbrChoice = _choiceList.Count;

            for (var i = 0; i < nbrChoice; ++i)
            {
                var choice = _choiceList[i];
                if (answer.ChoiceNo == choice.No)
                    choice.SetAnswer(answer);
            }
        }

        public abstract void GetAnswers(List<Answer> answers);

        public virtual void ResetAnswers()
        {
            foreach (var c in _choiceList)
            {
                c.ResetAnswer();
            }
        }

        /// <summary>
        /// Indique si l'un des choix est de type texte.
        /// La seule raison de surcharger cette méthode est quand aucun choix n'est possiblement texte,
        /// ou totalement sûr que oui.
        /// </summary>
        /// <returns>SI la liste de choix contient un type "ChoiceText" ou ses dérivés.</returns>
        public virtual bool HasChoiceText()
        {
            return Choices.Any(c => c is ChoiceText);
        }

        /// <summary>
        /// Détruit les ressources qui ne peuvent pas être libérée automatiquement.
        /// Par défaut, invoque la même méthode sur tous les choix.
        /// </summary>
        public virtual void Dispose()
        {
            if (_choiceList.Count <= 0) return;
            foreach(var c in _choiceList)
            {
                c.Dispose();
            }
        }

        public virtual bool HasDefaultAnswer()
        {
            return _defaultAnswersDic.Count > 0;
        }

        public virtual Answer GetDefaultAnswer(int choiceNo)
        {
            //var en = from ent in _defaultAnswersDic where ent.Key == _activeLanguage) sele ;
            if (!_defaultAnswersDic.ContainsKey(_activeLanguage??"en")) return null;
            foreach (var entry in _defaultAnswersDic)
            {
                if (!entry.Key.Equals(_activeLanguage ?? "en")) continue;
                entry.Value.TryGetValue(choiceNo, out var ans);
                return ans;
            }

            return null;
        }

        public void AddDefaultAnswer([NotNull] string lg, int choiceNo, [NotNull] string response)
        {
            if (!_defaultAnswersDic.ContainsKey(lg))
            {
                _defaultAnswersDic.Add(lg, new Dictionary<int, Answer>());
            }

            if (_defaultAnswersDic.TryGetValue(lg, out var answerDic))
            {
                answerDic.Add(choiceNo, new Answer{QuestionType = (QuestionTypeEnum)QuestionType, QuestionNo = No, ChoiceNo = choiceNo, Response = response});
            }
            else
            {
                log.Warn($"Could not add default answer in question: {lg}->(q{No}c{choiceNo}:{response})");
            }
        }

        public void ClearDefaultAnswer()
        {
            _defaultAnswersDic.Clear();
        }

        /// <summary>
        /// Prend une liste de réponses et retourne un format texte qui correspond à représentation correcte pour un rapport.
        /// Selon la nature de la question et de ses choix, cela peut représenter soit la valeur textuelle de la réponse ou
        /// le texte d'une question ou de l'un de ses choix.
        /// Quand plusieurs réponses sont disponibles, et lorsque applicable, les "réponses formattées" sont concaténées avec une virgule.
        /// </summary>
        /// <param name="answers">Le groupe de réponse à analyser.</param>
        /// <returns>Une chaîne de caractère comtenant le résultat du formatage. N'est jamais nul, mais peut être vide.</returns>
        public abstract string ReportFormat(List<Answer> answers);

        /// <summary>
        /// On fait toujours des instanciations réflexives, mais désormais le type de base d'un choix d'une implémentation de question est
        /// fixé par défaut selon l'implémentation.
        /// </summary>
        /// <returns>Le type exact de choix par défaut pour cette implémentation de Question.</returns>
        public abstract Type GetDefaultChoiceClassType();
    }
}
