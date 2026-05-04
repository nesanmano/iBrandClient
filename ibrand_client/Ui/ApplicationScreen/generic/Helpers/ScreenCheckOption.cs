using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using Newtonsoft.Json;
using NomadLogic.Common.Questionnaire;
using NomadLogic.Common.Respondent;
using NomadLogic.Common.Util.Math;

namespace NomadLogic.iBrandDesktop.Ui.ApplicationScreen.Generic.Helpers
{
    /// <summary>
    /// Classe d'information sur la gestion d'une question de type "CheckBox", "RadioBox" ou "ComboBox".
    /// Est représentée par une zone où il est possible de cliquer, et de visualiser comme cliquée, avec une image qui se dessine à 
    /// un point précis, qui peut ne pas être celui de sa zone d'activation.
    /// </summary>
    public class ScreenCheckOption : ScreenFieldAbstract
    {
        private Point? _checkLocation;

        [JsonIgnore]
        public bool AnswerState { get; private set; }

        /// <summary>
        /// Point d'ancrage optionel pour l'image du crochet. Si nul, on utilise le point en haut à gauche de la position.
        /// </summary>
        public Point? CheckLocation
        {
            get => _checkLocation ?? Position.Location;
            set => _checkLocation = value;
        }
        
        /// <summary>
        /// Indique que la zone ne peut pas basculer entre actif et inactif. Une fois actif, on ne peut pas désactiver cette zone avec une action de l'usager.
        /// </summary>
        public bool ToggleOnly { get; set; }

        public ScreenCheckOption() {}

        /// <summary>
        /// Constructeur complet.
        /// </summary>
        /// <param name="zone">Zone d'activation.</param>
        /// <param name="questionNo">Numéro de question associé à cette option.</param>
        /// <param name="choiceNo">Numéro de choix associé à cette option.</param>
        /// <param name="mandatory">Indique si on ne peut pas continuer sans avoir coché cette option.</param>
        /// <param name="checkLocation">Le point d'ancrage de l'image à afficher quand l'Option est à vrai. Optionel. Si null, on prend la location de la zone dde clic.</param>
        [Obsolete("Utiliser la version avec identifiant de question.")]
        public ScreenCheckOption(Rectangle zone, int questionNo, int choiceNo, bool mandatory, Point? checkLocation = null)
        {
            Position = zone;
            DataModels = new List<DataModelBase>{
                new DataModelQuestionnaire
            {
                Identifier = new QuestionIdentifier(questionNo, choiceNo)
            }};
            AnswerState = false;
            Mandatory = mandatory;
            CheckLocation = checkLocation;
        }

        /// <summary>
        /// Constructeur complet.
        /// </summary>
        /// <param name="zone">Zone d'activation.</param>
        /// <param name="qi">Identifiant de question.</param>
        /// <param name="mandatory">Indique si on ne peut pas continuer sans avoir coché cette option.</param>
        /// <param name="checkLocation">Le point d'ancrage de l'image à afficher quand l'Option est à vrai. Optionel. Si null, on prend la location de la zone dde clic.</param>
        public ScreenCheckOption(Rectangle zone, QuestionIdentifier qi, bool mandatory, Point? checkLocation = null)
        {
            Position = zone;
            DataModels = new List<DataModelBase>{
                new DataModelQuestionnaire
            {
                Identifier = qi
            }};

            AnswerState = false;
            Mandatory = mandatory;
            CheckLocation = checkLocation;
        }
        
        /// Vérifie si la coordonnée cliquée correspond à la zone donnée par cette option.
        /// Si oui, ajuste l'état du bouton et retourne vrai.
        public bool MouseDown(int eX, int eY)
        {
            if (!MathUtil.InArea(eX, eY, Position)) return false;
            
            Toggle();
            return true; // Indique qu'une zone aété touchée, pas que la valeur à changée.
        }

        public bool Toggle()
        {
            if (AnswerState && ToggleOnly) return false;

            AnswerState = !AnswerState;

            return true;
        }

        public void Recycle()
        {
            AnswerState = false;
        }

        public void AssignInitialValue(bool val)
        {
            AnswerState = val;
        }

        public void AssignInitialValue(IRespondent respondent)
        {
            var valSource = DataModels.FirstOrDefault();
            if (valSource == null)
            {
                throw new System.Exception("Cannot continue. NULL DataModel for ScreenCheckOption detected.");
            }
            
            AnswerState = valSource.GetActivationBoolValue(respondent);
        }
    }
}