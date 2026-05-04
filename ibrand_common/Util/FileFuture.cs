using System;
using System.IO;
using System.Threading;
using JetBrains.Annotations;
using NLog;

namespace NomadLogic.Common.Util
{
    /// <summary>
    /// Le concept de cette classe est le suivant: on référence un fichier dans le système de fichier qui n'existe peut-être pas encore. Dès que le fichier devient disponible,
    /// un événement se lève. Si le fichier est disponible dès le départ, dans ce cas tout va bien. Dans l'intérim si le fichier n'existe pas, il peut y avoir un fichier "place-holder",
    /// temporaire, qui peut être référencé.
    /// Utilisé principalement dans une situation où on a une gallerie qui présente des fichiers, mais que ces fichiers ne sont peut-être pas encore sur le disque,
    /// et que l'on doit quand même afficher quelque chose entre-temps. Implique que l'on sait avec certitude que ce fichier VA exister,
    /// et que ce n'est qu'une question de temps avant qu'il ne le soit.
    /// Assume que le fichier ne sera pas déplacé ou modifié une fois en place.
    /// Il existe un "flag" pour spécifier que la référence principale ne sera JAMAIS un fichier valide (pour une multitude raison), mais qu'il faut quand utiliser le concept de fichier
    /// place-holder. Cela peut être pour masquer des valeurs impossibles, ou référencer des entitées qui doivent être visuellement disponibles dans une gallerie, mais qui
    /// ne représentent pas des fichiers sur disque.
    /// </summary>
    [PublicAPI]
    public class FileFuture
    {
        private static readonly Logger log = LogManager.GetCurrentClassLogger();
        private Timer _timer;

        /// <summary>
        /// Cet événement est lancé quand le fichier final devient disponible. Ne sera jamais lancé si le fichier est disponible dès le départ.
        /// </summary>
        public event EventHandler FinalFileAvailiable;

        public FileFuture(FileInfo placeholder)
        {
            PlaceholderFileInfo = placeholder;
            IsValidFile = true;

            IsAvailiable = false;
            _timer = new Timer(CheckForFile, null, TimeSpan.Zero, TimeSpan.FromMilliseconds(500));
        }

        public void WaitForFileCallback(object sender, FileInfo e)
        {
            FinalFileInfo = e.FullName;
            CheckForFile(this);
        }

        public FileFuture([NotNull] string finalFileReference, FileInfo placeholder = null, bool nonFileReference = false)
        {
            FinalFileInfo = finalFileReference;
            PlaceholderFileInfo = placeholder;
            IsValidFile = !nonFileReference;
            IsAvailiable = true;

            if (!IsValidFile) return;
            if (File.Exists(FinalFileInfo)) return;

            IsAvailiable = false;
            _timer = new Timer(CheckForFile, null, TimeSpan.Zero, TimeSpan.FromMilliseconds(500));
        }
        
        private void CheckForFile(object state)
        {
            log.Trace("Timer event raised!");
            if (!IsValidFile || !File.Exists(FinalFileInfo)) return;

            IsAvailiable = true;
            _timer?.Change(TimeSpan.Zero, TimeSpan.FromMilliseconds(-1)); // Déactive le timer.
            _timer = null;
            log.Trace("File check finished: file is now readable on disk: {0}", FinalFileInfo);
            FinalFileAvailiable?.Invoke(this, new EventArgs());
        }

        /// <summary>
        /// Indique que l'instance à été créée avec un avertissement que la valeur finale ne sera jamais un fichier valide.
        /// </summary>
        public bool IsValidFile { get; private set; }

        /// <summary>
        /// Indique que la référence de fichier principale est désormais finale et que l'on peut l'utiliser.
        /// </summary>
        public bool IsAvailiable { get; private set; }

        /// <summary>
        /// Référence vers le fichier final. Il est possible que l'on en connaisse pas encore ce chemin, et que même si le chemin est connu, que le fichier ne soit pas encore sur disque.
        /// </summary>
        public string FinalFileInfo { get; private set; }

        /// <summary>
        /// Valeur transitive que l'on peut utiliser en lieu et place de la valeur de fichier finale. Cette valeur est toujours considérée valide si elle existe.
        /// Il est possible qu'en cas de fichier invalide, cette valeur sera toujours celle à utiliser (si existante).
        /// </summary>
        public FileInfo PlaceholderFileInfo { get; }
    }
}