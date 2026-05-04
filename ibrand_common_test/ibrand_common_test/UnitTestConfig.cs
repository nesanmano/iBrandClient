namespace NomadLogic.Test
{
    internal class UnitTestConfig
    {
        /// <summary>
        /// Comme il est impossible de détrerminer OÙ le répertoire GIT de l'usager local se situe, il faut manuellement
        /// Écrire dans ce fichier l'endroit exact, de la machine locale, où se trouve le répertoire qui contient les
        /// fichier de tests.
        /// </summary>
        public string TestFilesDirectory { get; set; }
    }
}