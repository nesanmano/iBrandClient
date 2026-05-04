using System;
using System.Collections.Generic;
using System.Windows.Forms;

namespace NomadLogic.iBrandDesktop.Ui.ApplicationScreen.Generic.Helpers
{
    /// <summary>
    /// Définition additionnelle pour les élément visuels de type ComboBox, soit des listes déroulantes d'éléments distincts.
    /// Ces éléments sont affichés dans l'ordre établit, son indexé via zéro mais quand vient le temps de les écrire en tant que réponses,
    /// leur indice est ajusté de 1. En clair, l'élément zéro devient la réponse X-Y-1. (Dans le modèle qNo, cNo, sNo).
    /// Il faut s'assurer que pour la bonne marche de ces définitions, il faut que les propriétés relative à des indices contiennent les bons nombres d'éléments.
    /// </summary>
    public class ComboboxDefinition: ScreenUiFieldAbstract<ComboBox>
    {
        /// <summary>
        /// Liste des éléments textes à utiliser.
        /// Il s'agit d'un dictionnaire indexé par le code de langue, qui contient une liste d'éléments, et chacun de ces éléments est un couple de valeurs textes qui représentent
        /// la valeur d'affichage (élément 0) et la valeur à insérer dans la réponse (élément 1). Si l'élément 1 est nul, on utilise l'élément zéro dans les deux situations.
        /// </summary>
        public IDictionary<string, IList<Tuple<string, string>>> TextValues { get; set; } = new Dictionary<string, IList<Tuple<string, string>>>();

        /// <summary>
        /// Si non-nul, à chaque recyclage on va remettre à neuf l'élément avec cet indice au lieu de -1 (aucune sélection). Il s'agit d'un index, donc en base zéro.
        /// </summary>
        public uint? DefaultIndex { get; set; }
    }
}