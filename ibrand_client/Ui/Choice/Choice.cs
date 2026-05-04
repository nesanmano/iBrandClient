using System;
using System.Collections.Generic;
using System.Drawing;
using System.Windows.Forms;
using NomadLogic.Common.Respondent;
using NomadLogic.Common.Services;
using NomadLogic.Common.Services.Configuration;
using NomadLogic.Common.Util;

namespace NomadLogic.iBrandDesktop.Ui.Choice
{
    public abstract class Choice
    {
        protected IDictionary<string, string> _textTable;
        protected int _no;
        protected Question.Question _question;

        protected UiStyle _uiStyle;

        protected string _prefix;
        protected string _suffix;
        protected string _maxLength;
        protected bool _titleCase;

        protected string _activeLanguage;

        protected bool _enabledDefault = true;


        protected Choice()
        {
            _textTable = new Dictionary<string, string>();
            _question = null;
            _prefix = null;
            _suffix = null;
        }

        public virtual string Language
        {
            set => _activeLanguage = value;
        }

        public bool EnabledDefault
        {
            set
            {
                _enabledDefault = value;
                Enabled = value;
            }
        }

        public UiStyle UiStyle
        {
            set => _uiStyle = value;
        }

        public string Prefix
        {
            set => _prefix = value;
        }

        public bool TitleCase
        {
            set => _titleCase = value;
        }

        public string MaxLength
        {
            set => _maxLength = value;
            get => _maxLength;
        }

        public string Suffix
        {
            set => _suffix = value;
        }

        public int No
        {
            get => _no;

            set => _no = value;
        }

        protected virtual void OnResponseChanged(object sender, EventArgs e)
        {
            if (_question.ValidateAnswersPattern())
            {
                _question.OnAnswerChange();
            }
        }
        
        public virtual Question.Question Question
        {
            set => _question = value;

            get => _question;
        }

        public virtual void SetText(string language, string text)
        {
            _textTable.Add(language, text);
        }

        public virtual bool IsAnswered()
        {
            return false;
        }

        public string GetText()
        {
            if (_activeLanguage == null)
            {
                var configuration = ServicesContainer.GetService<NomadConfiguration>();
                _activeLanguage = configuration.PreferedLanguage;
            }

            var text = _textTable.GetDefaultValue(_activeLanguage);

            return text;
        }

        //To be overrided
        public virtual void SetFocusable(bool focusable)
        {

        }

        //To be overrided
        public abstract void SetAnswer(Answer answer);

        public virtual string GetAnswerText()
        {
            return "";
        }


        /// <summary>
        /// Détruit les ressources associée à ce choix.
        /// </summary>
        public virtual void Dispose()
        {

        }

        /// <summary>
        /// Ajoute les éléments visuels de ce choix à un panneau d'affichage. La position supérieure gauche du dernier élément à être placé est
        /// passé en paramètre via les variables X et Y. On doit donc calculer la position absolue de chaque contrôle à ajouter via ce point.
        /// <br/>Cette méthode retourne la hauteur totale des éléments ajoutés par ce choix, permettant de calculer les coordonnées minimales
        /// du prochain élément visuel du panneau.
        /// </summary>
        /// <param name="questionPanel">La panneau auquel on ajoute les éléments visuels.</param>
        /// <param name="x">Position en X de départ.</param>
        /// <param name="y">Position en Y de départ.</param>
        /// <param name="g">Référence vers l'outil de rendu des éléments visuel. Utilisé pour calculer la tailles des composantes visuelles.</param>
        /// <param name="availableWidth">La taille maximale en largeur (X) que les éléments peuvent prendre.</param>
        /// <returns>La hauteur utilisées par ce xhoix pour ses éléments visuels.</returns>
        public abstract int AddToUserControl(Panel questionPanel, int x, int y, Graphics g, int availableWidth);

        /// <summary>
        /// Obtient la hauteur du contrôle de ce choix. Cela inclu habituellement la hauteur de son plus grand élément (si disposition horizontale)
        /// plus un "padding".
        /// </summary>
        /// <param name="availableWidth">Le nombre de pixel disponible pour dessiner les contrôles du choix.</param>
        /// <param name="g">L'élément de dessein. Utiliser pour calculer la tailles des éléments (particulièrement textes).</param>
        /// <returns>La hauteur en pixel de l'espace vertical requis par ce choix.</returns>
        public abstract int GetHeight(int availableWidth, Graphics g);

        /// <summary>
        /// Retourne la taille idéale d'un élément contenant du texte pour afficher chaque ligne correctement. Idéalement un objet de type "Label", mais peut fonctionner
        /// avec d'autres type de contenant de texte au besoin.
        /// <br/>Chaque paramètre doit être non-nul et le texte à afficher non vide. La largeur doit aussi être supérieure à zéro. Si ces conditions ne sont pas respectées,
        /// on retourne immédiatement zéro comme réponse.
        /// </summary>
        /// <param name="g">L'outil graphique pour calculer la taille.</param>
        /// <param name="font">Le Font utilisé pour le texte.</param>
        /// <param name="maxWidth">La largeur maximale du widget graphique.</param>
        /// <param name="text">Le texte à afficher.</param>
        /// <returns>La taille en pixels du widget graphique.</returns>
        public virtual int GetTextHeight(Graphics g, Font font, int maxWidth, string text)
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

        /// <summary>
        /// Lancé quand le questionnaire se réinitialise et que l'on doit revenir à la valeur par défaut.
        /// </summary>
        public abstract void ResetAnswer();

        public abstract void SetFocus();

        public abstract bool Enabled{ set; internal get; }

        public abstract bool HasFocus();

        public abstract void SetText();

        /// Référence vers un méthode quand un veut un comportement qui remplit automatiquement un choix.
        public Action AutoPopulateAction { get; set; }

        /// <summary>
        /// Contient la verification et appel necessaire pour chercher et mettre a jour une reponse via la question.
        /// </summary>
        protected void CheckAndUpdateDefaultAnswer()
        {
            if (!Question.HasDefaultAnswer()) return;
            var defAns = Question.GetDefaultAnswer(No);
            SetAnswer(defAns);
        }
    }
}
