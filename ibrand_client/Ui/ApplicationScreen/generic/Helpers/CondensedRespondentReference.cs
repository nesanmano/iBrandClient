namespace NomadLogic.iBrandDesktop.Ui.ApplicationScreen.Generic.Helpers
{
    /// Liste des numéro de répondant potentiels locaux, via la table "condensed_answers"
    public class CondensedRespondentReference
    {
        /// Numéro unique du répondant contenant des réponses.
        public int Numero { get; set; }
        /// Texte à afficher dans le combobox.
        public string Identifier { get; set; }
        /// Index de lecture. On peut utiliser cette valeur comme index de combobox.
        public int Index { get; set; }
    }
}