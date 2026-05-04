using JetBrains.Annotations;

namespace NomadLogic.iBrandDesktop.Ui.ApplicationScreen.Generic.Helpers
{
    /// <summary>
    /// Structure pour décrire un pallier d'affichage d'un décompte.
    /// </summary>
    public struct ImageStepInfo
    {

        /// <summary>
        /// Constructeur principal.
        /// </summary>
        /// <param name="data">Champ à interpréter. Si vide, désigne un pallier qui n'affiche rien.</param>
        /// <param name="timeSec">Temps du pallier.</param>
        /// <param name="isLanguageDependent">Si recherche se fait avec la langue actuelle. Faux par défaut.</param>
        [PublicAPI]
        public ImageStepInfo(string data, uint timeSec, bool isLanguageDependent = false)
        {
            Data = data;
            TimeSec = timeSec;
            IsLanguageDependent = isLanguageDependent;
        }

        /// <summary>
        /// Information à afficher. Peut être un simple texte, ou un chemin vers un fichier. C'est l'option "ImageGenerationInfo" du décorateur qui
        /// détermine si on interprète cette valeur comme un fichier ou du simple texte.
        /// Le nom du fichier. Si ce chemin n'est pas relatif (il est absolu et contient le caractère ':' pour désigner le lecteur HD), on
        /// va chercher le fichier selon les règles normales de recherche de fichiers propre à un questionnaire.
        /// </summary>
        public string Data { get; }

        /// <summary>
        /// Le temps en secondes propre à ce pallier.
        /// </summary>
        public uint TimeSec { get; }

        /// <summary>
        /// Indique que le fichier, s'il est relatif, doit être cherché en tenant compte de la langue actuelle de l'écran.
        /// </summary>
        public bool IsLanguageDependent { get; }
    }
}