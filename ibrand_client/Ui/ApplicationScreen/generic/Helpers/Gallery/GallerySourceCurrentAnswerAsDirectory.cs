using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Windows.Forms;
using MCollections;
using NomadLogic.Common.Questionnaire;
using NomadLogic.Common.Respondent;
using NomadLogic.Common.Ui.NomadScreen;
using NomadLogic.Common.Util;

namespace NomadLogic.iBrandDesktop.Ui.ApplicationScreen.Generic.Helpers.Gallery
{
    /// <summary>
    /// Cette classe source analyse le répondant lors d'une activation d'écran, et retourne la liste de fichiers contenu dans un répertoire
    /// contenu comme réponse dans une question du répondant.
    /// Même si la classe contient les classe d'instanciation et de départ/arrêt, elles sont vides car l'instance n'agit que sur un nouveau répondant.
    /// </summary>
    public class GallerySourceCurrentAnswerAsDirectory : IGallerySource<ComparableFileInfo>, IActivateScreen
    {
        private readonly List<IElementMonitor<ComparableFileInfo>> _monitors = new List<IElementMonitor<ComparableFileInfo>>();
        
        public ISaveToQuestionnaireFormatter<ComparableFileInfo> SaveFormatter { get; set; }
        
        public int QuestionNo { get; internal set; }

        public void Dispose()
        {
            // Rien
        }

        public bool Init()
        {
            return true;
        }

        public bool StartUpdates()
        {
            return true;
        }

        public bool StopUpdates()
        {
            return true;
        }

        public bool AddElementMonitor(IElementMonitor<ComparableFileInfo> monitor)
        {
            lock (_monitors)
            {
                if (_monitors.Contains(monitor)) return false;
                _monitors.Add(monitor);
            }
            return true;
        }

        public bool RemoveElementMonitor(IElementMonitor<ComparableFileInfo> monitor)
        {
            lock (_monitors)
            {
                return _monitors.Remove(monitor);
            }
        }

        public void ClearAllElementMonitors()
        {
            lock (_monitors)
            {
                _monitors.Clear();
            }
        }
        
        public void Activate(Control parent, IRespondent respondent)
        {
            if (respondent == null) return;

            var response = respondent.GetAnswerText(QuestionNo);

            if (string.IsNullOrWhiteSpace(response)) return;

            var dir = new DirectoryInfo(response);

            if (!dir.Exists) return;

            var files = dir.GetFiles().Select(f => new ComparableFileInfo(f));

            var fileSet = new IndexedSet<ComparableFileInfo>();
            foreach (var f in files)
            {
                fileSet.Add(f);
            }

            if (!fileSet.Any()) return;
            
            lock (_monitors)
            {
                foreach (var monitor in _monitors)
                {
                    monitor.UpdateElements(fileSet);
                }
            }
        }
    }
}