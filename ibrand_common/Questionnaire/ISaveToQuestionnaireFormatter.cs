using System.Collections.Generic;
using NomadLogic.Common.Respondent;

namespace NomadLogic.Common.Questionnaire
{
    public interface ISaveToQuestionnaireFormatter<in T>
    {
        /// <summary>
        /// Méthode à invoquer pour effectuer la sauvegarde.
        /// Il est possible que l'invocation de cette méthode sauvegarde plus que une réponse, selon la nature de l'implémentation.
        /// </summary>
        /// <param name="saveTo">L'instance qui peut mettre sur disque les réponses.</param>
        /// <param name="index">L'index de l'élément choici dans la présentation.</param>
        /// <param name="elem">Le référence vers l'objet qui est associé à l'item de la gallerie. </param>
        /// <returns>TRUE si la sauvegarde s'est bien effectuée.</returns>
        bool Save(ISaveLocalRespondentData saveTo, int index, T elem);

        /// <summary>
        /// Variante pour générer les réponses à sauvegarder si l'item est choisi dans la gallerie.
        /// </summary>
        /// <param name="index">L'index de la sélection de la gallerie. Est variable selon le positionnement dans les éléments de la gallerie et doit donc être passé ici.</param>
        /// <param name="elem">La référence de l'objet associé à l'item de la gallerie.</param>
        /// <returns></returns>
        IList<Answer> CreateAnswers(int index, T elem);
    }
}