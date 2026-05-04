using System;
using System.Collections.Generic;
using System.Drawing;
using JetBrains.Annotations;

namespace NomadLogic.Common.Util
{
    /// <summary>
    /// Cette interface décrit les princinpales méthode et propriétés d'un objet qui gère la pagination
    /// d'une liste triée d'éléments, en tant que moniteur d'éléments sources.
    /// On gère la liste, mais surtout où on est présentement, comme la page présente et/ou selon un élément précis.
    /// On agit selon la prémisse que un autre objet "nourrit" ou met à jour la liste d'éléments, et que l'instance doit
    /// indiquer si on doit se mettre à jour ou non, pour limiter le nombre de requêtes des éléments à afficher.
    /// On part de l'assomption que cette mise à jour est coûteuse en terme de mémoire et de temps.
    /// On assume que le type d'objet à lister possède une méthode de comparaison intrinsèque (CompareTo), ce qui
    /// est réflété dans la hiérarchie de classe.
    /// L'index des pages est basé sur ZÉRO. En clair, les indices d'un groupe sur trois pages sont 0, 1 et 2 respectivement.
    /// </summary>
    /// <typeparam name="T">Le type d'objet à présenter.</typeparam>
    [PublicAPI]
    public interface IPaginationMonitor<T> : IElementMonitor<T> where T : IComparable<T>
    {
        /// <summary>
        /// Indique si on n'est pas sur la dernière page.
        /// </summary>
        bool HasNextPage { get; }

        /// <summary>
        /// Indique si on n'est pas sur la première page.
        /// </summary>
        bool HasPreviousPage { get; }

        /// <summary>
        /// Indique le nombre total d'éléments, dans l'ensemble des pages.
        /// </summary>
        uint TotalElements { get; }

        /// <summary>
        /// Indique le nombre de rangées et de colonnes (respectivement) qu'il faut afficher par page.
        /// Le nombre d'éléments par page devient donc W * H.
        /// Cette propriété ne devrait pas accepter des valeurs impossibles (comme zéro), mais selon l'implémentation,
        /// pourrait inférer des valeurs variables (comme (5, -1), à déterminer selon la résolution.
        /// </summary>
        Size PresentationSize { get; set; }

        /// <summary>
        /// Référence vers l'élément qui est le présent ancre de présentation (affecte le besoin de mettre à jour l'affichage).
        /// Des événements de mises à jour vont toujours tenter de garder cet élément visible. Est modifié lorsque l'on demande
        /// un changement de page.
        /// </summary>
        T CurrentPresentedElement { get; }

        /// <summary>
        /// L'indice maximal de la dernière page.
        /// </summary>
        uint MaxPage { get; }

        /// <summary>
        /// L'indice de la page actuelle.
        /// </summary>
        uint CurrentPage { get; }

        /// <summary>
        /// Indique que la liste à été modifiée. Soit des éléments ont été modifiés, soit des éléments ont été ajoutés ou retirés.
        /// Cette méthode retournera vrai si une mise à jour de liste a eu lieu mais que aucune commande de modification de page ou
        /// aucune demande d'éléments paginé n'a été faite encore.
        /// </summary>
        bool HasUpdated { get; }

        /// <summary>
        /// Une version allégée pour savoir si malgré une mise à jour, les éléments de la page actuelle sont rigouresement identiques ou non.
        /// Si une mise à jour a été faite, mais que cette variable est fausse, une demande des éléments de la page retournera exactement la même liste,
        /// ce qui est donc inutile.
        /// Se produit normalement si on n'affiche pas la dernière page, et que des éléments suivants sont ajoutés sans que le contenu de la page prédédente
        /// ne soit affectés.
        /// </summary>
        bool NeedRefresh { get; }

        /// <summary>
        /// On peut s'abonner à cet événement pour savoir si on doit se mettre à jour.
        /// Est lancé dès que la propriété NeedRefresh peut retourner TRUE.
        /// </summary>
        event EventHandler ShouldRefresh;

        /// <summary>
        /// Indique qu'il faudrait que les éléments visuels relatifs à cet objet soient re-dessinés.
        /// </summary>
        event EventHandler UiRefresh;

        /// <summary>
        /// Indique que les éléments sont inversés et que la pagination réflète l'inverse de la liste en mémoire.
        /// </summary>
        bool IsReversed { get; }

        /// <summary>
        /// Inverse l'ordre des éléments, sans toucher à la liste sous-jacente. Recalcule les éléments de pagination, en tentant de garder l'élément
        /// courant comme ancre de présentation.
        /// </summary>
        /// <returns></returns>
        bool Reverse();
        
        /// <summary>
        /// Provoque un re-calcul des informations de pagination.
        /// Retourne faux si et seulement si l'indice voulu est invalide (négatif, ou supérieur au maximum).
        /// </summary>
        /// <param name="pageIndex">L'indice de page (base zéro) à aller.</param>
        /// <returns>FALSE si l'argument est invalide.</returns>
        bool GotoPage(uint pageIndex);

        /// <summary>
        /// Tente de trouver un élément source identique, et va à la page où cet élément existe, et met cet élément comme ancre.
        /// Arrête la recherche sur le premier élément trouvé.
        /// Retourne TRUE si la page à changée (donc un élément qui n'est pas contenu dans la liste retourne faux).
        /// </summary>
        /// <param name="element">L'élément à chercher. Null provoque un no-op.</param>
        /// <param name="comparer">Optionel. L'objet de comparaison personalisé.</param>
        /// /// <returns>True si les éléments de pagination sont modifiés.</returns>
        bool GotoElement(T element, IComparer<T> comparer = null);
        
        /// <summary>
        /// Retourne une sous-liste des éléments qui apparaissent dans cette page. Les éléments sont déjà ordonnés selon
        /// l'affichage voulu (donc, prennent en compte l'inversion si présente). Ils sont également accompagnés de leur index réel dans la liste complète.
        /// </summary>
        IReadOnlyList<(ComparableFileInfo, int)> GetCurrentPageElements();

        /// <summary>
        /// Retourne la valeur d'indice de l'élément, quelque soit l'ordre d'affichage, selon la liste sous-jacente.
        /// Cette valeur est basée sur zéro.
        /// </summary>
        /// <param name="element">L'élément à identifier.</param>
        /// <returns>-1 si l'élément est null ou n'existe pas. Sinon, son indice dans la liste.</returns>
        int GetElementIndex(T element);
    }
}