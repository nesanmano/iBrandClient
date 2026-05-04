using System;
using System.Collections.Generic;

namespace NomadLogic.Common.Respondent
{
    /// <summary>
    /// Interface de base pour un répondant. Permet de chercher des réponses et certaines informations de base sur le répondant.
    /// </summary>
    public interface IRespondent
    {
        /// <summary>
        /// Retourne une référence vers une réponse complète selon les identifiants numérique d'une question.
        /// </summary>
        /// <param name="questionNo">Le numéro de question.</param>
        /// <param name="choiceNo">Le numéro de choix. 1 par défaut.</param>
        /// <returns>Une référence vers la réponse. Nul si inexistant.</returns>
        Answer GetAnswer(int questionNo, int choiceNo = 1);

        /// <summary>
        /// Retourne une référence vers une réponse complète selon la définition unique d'une question/choix.
        /// </summary>
        /// <param name="qDef">La définition de la question et du choix visé.</param>
        /// <returns>Une réponse complète, si existante dans les réponses d'un répondant.</returns>
        Answer GetAnswer(QuestionIdentifier qDef);

        /// <summary>
        /// Retourne la valeur texte d'une réponse complète selon les identifiants numérique d'une question.
        /// </summary>
        /// <param name="questionNo">Le numéro de question.</param>
        /// <param name="choiceNo">Le numéro de choix. 1 par défaut.</param>
        /// <returns>Une référence vers le texte de la réponse. Nul si inexistant.</returns>
        string GetAnswerText(int questionNo, int choiceNo = 1);
        
        /// <summary>
        /// Retourne une référence vers la composante texte d'un réponse selon la définition unique d'une question/choix.
        /// </summary>
        /// <param name="qDef">La définition de la question et du choix visé.</param>
        /// <returns>Si existant, retourne la valeur texte de cette réponse.</returns>
        string GetAnswerText(QuestionIdentifier qDef);

        /// <summary>
        /// Retourne le code de langue du répondant. Si nul, on assume que l'on doit utiliser celui du questionnaire.
        /// </summary>
        string Language { get; set; }
        
        /// <summary>
        /// Retourne le répertoire de base du questionnaire associé à ce répondant. Si nul, il est impossible de l'évaluer.
        /// </summary>
        string TerminalDirectory { get; }

        [Obsolete]
        /// <summary>
        /// Le contexte d'exécution du logiciel. EN gros, le répertoire d'où on a démarré l'exécution.
        /// // TODO: Qu'est-ce que ça fout ici????
        /// </summary>
        /// <returns>Le répertoire d'exécution.</returns>
        string GetContextDirectory();

        [Obsolete]
        /// <summary>
        /// Identifiant numérique de l'événement iBrand associé.
        /// </summary>
        int EventId { get; set;  }

        [Obsolete]
        /// <summary>
        /// Obtient en format texte la date en milisecondes à partir de EPOCH (même définition que Java).
        /// Voir la classe: NomadLogic.iBrand.Common.Util.DateUtil
        /// </summary>
        string Date { get; }

        /// Obtient l'identifiant de projet d'un répondant.
        int ProjectId { get; set; }
        
        /// Obtient l'identifiant de questionnaire d'un répondant.
        int QuestionnaireId { get; set; }

        /// Obtient l'identifiant de questionnaire d'un répondant.
        int TerminalId { get; set; }

        /// <summary>
        /// L'identifiant du répondant dans la BD. Peut être -1 si n'existe pas encore dans la base de données.
        /// </summary>
        int Id { get; set; }

        [Obsolete]
        /// <summary>
        /// La définition du fuseau horaire du répondant.
        /// TODO: Utiliser une date locale désormais.
        /// </summary>
        string TimeZone { get; set; }

        /// <summary>
        /// Indique si un répondant à été synchronisé.
        /// </summary>
        bool IsSynchronized { get; set; }

        /// <summary>
        /// La liste des valeurs dynamiques du répondant.
        /// </summary>
        List<DynamicHeader> DynamicHeaders { get; set; }

        /// <summary>
        /// Liste des réponses du répondant.
        /// </summary>
        List<Answer> Answers { set; get; }

        /// <summary>
        /// Si existant, ceci est l'identifiant externe unique associé à ce répondant.
        /// </summary>
        string ExternalUniqueIdentifier { get; set; }
    }
}
