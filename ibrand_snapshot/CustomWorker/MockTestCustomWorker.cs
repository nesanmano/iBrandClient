using System.IO;
using System.Threading;
using NomadLogic.iBrand.Common.Util;

namespace iBrand_SnapShot.CustomWorker
{
    /// <summary>
    /// Classe de "traitement" qui ne fait que détruire un fichier avec un temps de "traitement" de 120 secondes.
    /// </summary>
    internal class MockTestCustomWorker : CustomWorker<SnapShotCustomWorkerParameter, FileInfo>
    {
        public override void DoWork()
        {
            var fichierOriginal = Parameter.WorkFile.FullName;

            try
            {
                Thread.Sleep(120 * 10000);
            }
            catch (ThreadInterruptedException)
            {
                // Do nothing
            }

            File.Delete(fichierOriginal);
        }
    }
}
