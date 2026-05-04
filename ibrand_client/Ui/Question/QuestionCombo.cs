using System;
using System.Collections;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using JetBrains.Annotations;
using NLog;
using NomadLogic.Common;
using NomadLogic.Common.Respondent;
using NomadLogic.Common.Util;
using NomadLogic.iBrandDesktop.Ui.Choice;

namespace NomadLogic.iBrandDesktop.Ui.Question
{
    /// <summary>
    /// Classe interne pour stocker les valeurs affichables du ComboBox
    /// </summary>
    public class QuestionComboItem
    {
        private static readonly Logger log = LogManager.GetCurrentClassLogger();
        private readonly Dictionary<string, string> _textsI18N = new Dictionary<string, string>();

        public void SetText(string langCode, string displayText)
        {
            if (_textsI18N.ContainsKey(langCode))
            {
                _textsI18N.Remove(langCode);
            }

            _textsI18N.Add(langCode, displayText);
            log.Trace("Added text to ComboItem: '{0}' - '{1}'", langCode, displayText);
        }

        public string LanguageCode { get; set; }

        public int? AssignedChoiceNo { get; set; }

        public override string ToString()
        {
            if (LanguageCode is null || !_textsI18N.ContainsKey(LanguageCode)) return "NULL";
            return _textsI18N[LanguageCode];
        }
    }

    [PublicAPI]
    public class QuestionCombo : Question
    {
        private static readonly Logger log = LogManager.GetCurrentClassLogger();
        private const int LABELED_CHOICE_FIELD_WIDTH = 80;
        private readonly ComboBox _comboControl;
        private readonly ArrayList _choiceItems;
        private Label _textLabel;

        public QuestionCombo()
        {
            _comboControl = new ComboBox {DropDownStyle = ComboBoxStyle.DropDownList};
            _textLabel = new Label();
            _choiceItems = new ArrayList();

            // Valeur initiale de tout les combo box. Cette valeur est invalide pour une question, mais est
            // cruciale pour indiquer à l'usager qu'il n'y a pas de réponse valide de sélectionnée.
            var sc = new QuestionComboItem();
            sc.SetText(Constants.EN, "Select...");
            sc.SetText(Constants.FR, "Sélection...");
            sc.LanguageCode = Constants.EN;

            _choiceItems.Add(sc);
            log.Trace("Added Selection text to combo list.");

            PopulateComboBox();
            
            _comboControl.KeyDown += (sender, e) =>
            {
                if (e.KeyCode == Keys.Next || e.KeyCode == Keys.PageUp)
                {
                    // Ignorer l'input.
                    e.SuppressKeyPress = true;
                }
            };
        }
        
        private void OnResponseChanged(object sender, EventArgs e)
        {
            if (ValidateAnswersPattern())
            {
                OnAnswerChange();
            }
        }
        
        public override Type GetDefaultChoiceClassType()
        {
            return typeof(ChoiceCombo);
        }

        public override void AddChoice(Choice.Choice choice)
        {
            if (!(choice is ChoiceCombo cc))
            {
                throw new System.Exception("Due to unique constraints, only ChoiceCombo instance are permitted in a QuestionCombo.");
            }

            base.AddChoice(cc); // Comportement de base

            // On associe un objet qui va dans le combo box avec ce choix.
            var tmp = new QuestionComboItem
            {
                LanguageCode = _activeLanguage
            };
            cc.AssignComboItem(tmp);
            
            _choiceItems.Add(tmp);
            log.Trace("New comboItem added to combo list: q{0}c{1}", No, cc.No);

            PopulateComboBox();
        }
        
        public override QuestionTypeEnum QuestionType => QuestionTypeEnum.Combo;
        
        protected override int AddChoiceToUserControl(Choice.Choice c, Panel p, int margin, int choiceY, Graphics g, int choiceMaxWidth)
        {
            // Les choix d'un combo ne sont pas ajoutés visuellement, car ils n'existe pas en terme d'élément visuel.
            // ils font partie de la boîte de sélection de la question.
            return 0; 
        }

        // Basé sur l'algorithme qui était jadis dans le ChoiceCombo.
        protected override int AddAdditionalQuestionElement(Panel p, int margin, int currentY, Graphics g, int availableWidth)
        {
            // Comme le ComboBox est géré par la question plutôt que le Choix, on l'ajoute en tant qu'élément visuel à la fin du processus.
            int choiceFieldWidth;
            // If text in textChoice, then put a label before.
            if (GetText() != null && GetText().Length > 0)
            {
                _textLabel = new Label
                {
                    Font = Font,
                    Text = GetText(),
                    Location = new Point(Constants.CHOICE_MARGIN, currentY),
                    Name = "TC_" + No
                };

                var textLabelWidth = availableWidth - LABELED_CHOICE_FIELD_WIDTH - (4 * Constants.CHOICE_MARGIN);

                _textLabel.Size = new Size(textLabelWidth, GetTextHeight(g, _textLabel.Font, textLabelWidth, GetText()));

                p.Controls.Add(_textLabel);

                choiceFieldWidth = LABELED_CHOICE_FIELD_WIDTH;

                margin = availableWidth - Constants.CHOICE_MARGIN - choiceFieldWidth;
            }
            else
            {
                choiceFieldWidth = availableWidth - (2 * Constants.CHOICE_MARGIN);
            }
            
            _comboControl.Name = "C_" + No;

            if (choiceFieldWidth > Constants.MAX_WIDTH)
                choiceFieldWidth = Constants.MAX_WIDTH;

            _comboControl.Size = new Size(choiceFieldWidth, _comboControl.Size.Height);
            
            if (_textLabel != null && _textLabel.Size.Height > _comboControl.Size.Height)
                currentY = currentY + ((_textLabel.Size.Height - _comboControl.Size.Height) / 2);

            _comboControl.Location = new Point(margin + Constants.CHOICE_MARGIN, currentY);

            p.Controls.Add(_comboControl);

            if (_textLabel != null)
                return Math.Max(_comboControl.Size.Height, _textLabel.Size.Height) + Constants.CHOICE_PAD;
            return _comboControl.Size.Height + Constants.CHOICE_PAD;
        }

        // Tiré du calcul de base de Choice.
        private int GetTextHeight(Graphics g, Font font, int maxWidth, string text)
        {
            if (g == null || font == null || maxWidth < 1 || text == null || text.Length == 0)
            {
                return 0;
            }

            var nbrOfLines = StringUtil.GetNumberOfLines(g, font, maxWidth, GetText());

            var height = nbrOfLines * font.GetHeight();

            //On ajoute 5 pixel par ligne > 1. Bug Fix. C'est comme si il avait un espace en chaque ligne de 1 pixel qui fait que la derniere ligne ne s'affiche pas. 
            var toAdd = (nbrOfLines - 1) * 5;

            var resultat = (int)Math.Ceiling(height) + toAdd;

            return resultat;
        }

        public override bool IsAnswered()
        {
            return _comboControl.SelectedIndex > 0;
        }

        public override void GetAnswers(List<Answer> answers)
        {
            if (!IsAnswered()) return;

            // Une seule réponse possible sur les choix possibles.
            var cNo = _comboControl.SelectedIndex;
            var item = (QuestionComboItem) _comboControl.SelectedItem;
            var a = new Answer
            {
                QuestionType = this.QuestionType,
                QuestionNo = this.No,
                ChoiceNo = cNo,
                Response = item.ToString()
            };
            answers.Add(a);
            log.Trace("QuestionCombo returned answer value: {0}", a);
        }

        public override string ReportFormat(List<Answer> answers)
        {
            var sb = new StringBuilder();
            var first = true;
            foreach (var str in answers.Select(a => a.Response))
            {
                if (first)
                {
                    first = false;
                }
                else
                {
                    sb.Append(',');
                }

                sb.Append(str);
            }

            return sb.ToString();
        }

        /// <summary>
        /// Callback pour indiquer qu'il faut modifier l'état actif du ComboBox UI
        /// </summary>
        internal void SetEnabledStatus(bool value)
        {
            _comboControl.Enabled = value;
            _textLabel.Enabled = value;
        }

        /// <summary>
        /// Retourne l'état UI du combo box.
        /// </summary>
        internal bool GetEnabledStatus()
        {
            return _comboControl.Enabled;
        }

        /// <summary>
        /// Callback d'un choix pour dire que son language à changé.
        /// </summary>
        internal void SetLanguage(string codeLangue, QuestionComboItem qti)
        {
            if (qti == null || _choiceItems.Count == 0 || codeLangue == null) return;
            
            log.Trace("Changing comboItem (cNo={2}) langage code from {0} to {1}", qti.LanguageCode, codeLangue, qti.AssignedChoiceNo);
            qti.LanguageCode = codeLangue;

            // Habituellement, un changement de langue va se produire sur chaque choix.
            // On attend que le dernier choix soit modifié avant de lancer un changement UI sur le combobox.
            // En gros, seul un des calls aux choix va provoquer un changement visuel.

            var lastItem = _choiceItems[_choiceItems.Count - 1] as QuestionComboItem;
            if (lastItem != qti) // Est-ce le dernier objet?
            {
                return;
            }

            // On provoque maintenant une mise à jour du contenu texte.
            PopulateComboBox();
        }

        internal void PopulateComboBox()
        {
            StringBuilder sb = null;
            if (log.IsTraceEnabled)
            {
                sb = new StringBuilder();
                sb.AppendLine("Update for the combo box items:");
            }

            // On débranche les événements
            _comboControl.SelectedIndexChanged -= OnResponseChanged;

            var currentIndex = _comboControl.SelectedIndex;

            _comboControl.Items.Clear(); 

            foreach (var sc in _choiceItems)
            {
                if (sb != null)
                {
                    var qci = (QuestionComboItem) sc;
                    sb.AppendLine($"\t\tAdding item to combobox. cNo={qci.AssignedChoiceNo}, lang={qci.LanguageCode}, Visible value={qci}");
                }

                _comboControl.Items.Add(sc); // Va utiliser le ToString() pour générer le texte
            }

            // Regarde si l'index est devenu invalide
            if (_comboControl.Items.Count <= currentIndex)
            {
                log.Debug("Current combo box index invalidated because of removal of items.");
                currentIndex = 0;
            }

            // On remet la sélection au dernier endroit valide
            _comboControl.SelectedIndex = currentIndex;

            // On débranche les événements
            _comboControl.SelectedIndexChanged += OnResponseChanged;

            if (sb != null)
            {
                log.Trace("PopulateComboBox trace events: " + sb.ToString());
            }
        }

        internal int GetCurrentSelectionIndex()
        {
            return _comboControl.SelectedIndex;
        }

        public override bool HasFocus()
        {
            return _comboControl.Focused;
        }

        public void OnArrowsDown(Keys key)
        {
            if (key != Keys.PageUp && key != Keys.Next) return;

            var currentIndex = _comboControl.SelectedIndex;
            switch (key)
            {
                case Keys.PageUp:
                    currentIndex--;
                    break;
                case Keys.Next:
                    currentIndex++;
                    break;
            }

            // Provoque l'événement de changement de réponse volontairement.
            if (currentIndex < 0)
            {
                return;
            }

            if (_comboControl.Items.Count-1 < currentIndex) // Plus haut que le nbr d'items (sans l'objet par déaut)
            {
                return;
            }
            _comboControl.SelectedIndex = currentIndex;
        }

        /// <summary>
        /// Met à jour la sélection sans provoquer d'événements de validation.
        /// </summary>
        /// <param name="selectedIndex"></param>
        public void SetSelectedIndex(int selectedIndex)
        {
            _comboControl.SelectedIndexChanged -= OnResponseChanged;
            _comboControl.SelectedIndex = selectedIndex;
            _comboControl.SelectedIndexChanged += OnResponseChanged;
        }

        public override void SetAnswer([CanBeNull] Answer answer)
        {
            if (answer != null && answer.QuestionNo != No) return;
            var newIndex = answer?.ChoiceNo ?? 0;
            if (_comboControl.Items.Count-1 < newIndex) // Faut considérer l'item par défaut (Sélection).
            {
                newIndex = 0;
            }
            log.Trace("Chanching answer to combo box to index {0}. Input answer= {1}", newIndex, answer);
            _comboControl.SelectedIndex = newIndex;
        }

        public override void ResetAnswers()
        {
            if (_comboControl.SelectedIndex == 0) return;
            _comboControl.SelectedIndexChanged -= OnResponseChanged;
            _comboControl.SelectedIndex = 0;
            _comboControl.SelectedIndexChanged += OnResponseChanged;
            log.Trace("Question combo q{0} reseted", No);
            
            if (!HasDefaultAnswer()) return;
            var defAns = GetDefaultAnswer(No);
            SetAnswer(defAns);
            log.Trace("Default answer set q{0}: {1}", No, defAns);
        }
    }

}
