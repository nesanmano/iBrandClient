using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using MCollections;
using NLog;
using NomadLogic.Common.Util;

namespace NomadLogic.iBrandDesktop.Ui.ApplicationScreen.Generic.Helpers.Gallery
{
    public class GallerySourceFixedDirectory : GallerySourceDirectoryBase
    {
        private static readonly Logger log = LogManager.GetCurrentClassLogger();

        /// <summary>
        /// Une liste des filtres supplémentaires à exécuter sur la liste de fichier à vérifier. Comme par exemple, une date de création ou une
        /// structure de noms spécifiques.
        /// Si aucun filtre n'est présent, on prend tous les fichiers. Si un ou plusieurs filtres sont présents, un fichier doit satisfaire TOUS les
        /// filtres (une série de AND en somme).
        /// </summary>
        public IList<ComparableFileInfoFilterBase> Filters { get; set; }

        private readonly IndexedSet<ComparableFileInfo> _lastListDebug = new IndexedSet<ComparableFileInfo>();

        protected override Task UpdateEventDo()
        {
            if (_disposing) return Task.CompletedTask;

            lock (_fileSet)
            {
                var files = WorkDirectory.GetFiles();

                var fils = Filters ?? new List<ComparableFileInfoFilterBase>();
                var tmpList = files.AsParallel().Select(file => new ComparableFileInfo(file)).AsParallel().
                    Where(p => { return !fils.Any() || fils.All(f => f.Filter(p)); }).AsParallel();
                
                _fileSet.Clear();
                foreach (var cfi in tmpList)
                {
                    _fileSet.Add(cfi);
                }

                if (log.IsDebugEnabled)
                {
                    if (!_fileSet.SetEquals(_lastListDebug))
                    {
                        _lastListDebug.Clear();
                        foreach (var f in _fileSet)
                        {
                            _lastListDebug.Add(f);
                            log.Trace($"File: {f.FileInfoRef.FullName}");
                        }

                        if (_lastListDebug.Count >= 2)
                        {
                            var first = _lastListDebug[0];
                            var last = _lastListDebug[_lastListDebug.Count - 1];
                            log.Debug($"GallerySourceFixedDirectory: File list changed. Total files: {_lastListDebug.Count}.\n\t\tOldest file: {first.FileInfoRef.Name}\n\t\tNewest file: {last.FileInfoRef.Name}");
                        } 
                        else if (_lastListDebug.Count == 1)
                        {
                            var first = _lastListDebug[0];
                            log.Debug($"GallerySourceFixedDirectory: File list changed. Total files: 1.\n\t\tOnly file: {first}");
                        }
                        else
                        {
                            log.Debug("GallerySourceFixedDirectory: File list changed. No files shown.");
                        }
                    }
                }

                foreach (var monitor in _monitors)
                {
                    monitor.UpdateElements(_fileSet);
                }

                _initialized = true;
            }

            return Task.CompletedTask;
        }
    }
}