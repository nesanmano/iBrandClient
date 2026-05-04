using System;
using System.Collections.Generic;
using System.IO;
using System.Threading;
using JetBrains.Annotations;
using NomadLogic.Common.Util.ThreadPool;
using WinSCP;

namespace NomadLogic.Common.Util.Networking.Ftp
{
    /// <summary>
    /// Classe qui gère une queue d'envoi FTP via des instances de travail. Ces instances ne sont pas disponibles en tant que telles, mais sont
    /// automatiquement générées via la méthode AddFtpTransfert().
    /// <p/>Il est très important qu'une fois l'instance créée, on appelle la méthode statique StartPoolInstance() pour lancer son exécution
    /// sur un fil séparé. À moins que l'on ne veuille utiliser l'instance de façon synchrone, auquel cas il faut appeller ses méthodes
    /// séparéments (par exemple: UploadFile(string) directement).
    /// </summary>
    [PublicAPI]
    [Obsolete]
    public class FtpPool : CustomWorkerPool<FtpUploadParameter, string[]>
    {
        private static readonly NLog.Logger log = NLog.LogManager.GetCurrentClassLogger();
        internal static string _protocol = "ftp://";

        /// <summary>
        /// Constructeur de base pour ce bassin de travaux.
        /// </summary>
        public FtpPool(string address, int port, string userName, string password)
        {
            ConnectionOptions = new SessionOptions()
            {
                Protocol = Protocol.Ftp,
                HostName = address,
                UserName = userName,
                Password = password,
                PortNumber = port
            };
        }

        public SessionOptions ConnectionOptions { get; }

        /// <summary>
        /// Vérifie que l'arborescence de répertoire sur le serveur est correcte. Sinon, tente de créer
        /// les répertoires de toute l'arborescence d'un coup. Le chemin doit comporter le nom du fichier à la fin, même si cette
        /// méthode ne vérifie que les répertoires.
        /// <param name="path">Le chemin URI ftp complet.</param>
        /// <returns>TRUE si et seulement si on confirme que l'arborescence de répertoire existe à la fin du traitement. FALSE sinon.</returns>
        /// </summary>
        public bool CheckFtpDirectoryPath(string path)
        {
            /* On analyse la structure, on la divise entre tous les éléments à créer, et un teste chaque étage un à un. */

            // Test de nullité et vide.
            if (string.IsNullOrEmpty(path))
            {
                return false;
            }

            // Split de la chaîne.
            var protocolSplit = path.Split(@"//".ToCharArray()); // donne "ftp:", "", "adressr", "dir1", "dir2", ..., "fichier.ext"

            if (protocolSplit.Length < 3)
            {
                // String mal formée.
                log.Error("Invalid FTP URI path. Must be in the form 'ftp://IP_ADDRESS/dir1/dir2/file.name'. Invalid path: {0}", path);
                return false;
            }

            var uri = protocolSplit[0] + "//" + protocolSplit[2] + "/"; // Répertoire racine
            var uriLast = uri;
            var checkForDirectory = true;

            try
            {
                using (var session = new Session())
                {

                    for (var i = 3; i < protocolSplit.Length - 1; i++)
                    {
                        // On teste chaque étage au-dessus de la racine.
                        uriLast += protocolSplit[i] + "/";

                        if (checkForDirectory)
                        {
                            var needToCreate = !FtpWebUtil.CheckIfExists(session, uriLast);

                            if (!needToCreate) continue;
                            checkForDirectory = false; // Une fois faux, on sais que tous les répertoires subséquentes n'existent pas;
                            FtpWebUtil.CreateDirectory(session, uriLast);
                        }
                        else
                        {
                            FtpWebUtil.CreateDirectory(session, uriLast);
                        }
                    }
                }
            }
            catch (Exception e)
            {
                log.Error(e, "Could not verify/create the following path:{0} ", path);
                return false;
            }

            return true;
        }
        
        /// <summary>
        /// Ajoute une demande d'envoi Ftp à la queue.
        /// La demande sera encapsulée dans un travailleur et mise en file pour être traitée.
        /// Comme on envoit plusieurs fichiers, ils sont traiter séquentiellement. Si on veut en envoyer
        /// en parallèle, il faut invoquer plusieures fois cette méthode.
        /// <p/>Ne fait rien si les deux premiers paramètres ne sont pas non-nuls et de taille égale.
        /// <p/>Ne fait rien si l'instance ne roule pas, vérifiable avec IsRunning
        /// </summary>
        /// <param name="dirs">Répertoires relatif où placer les fichiers sur le site FTP.</param>
        /// <param name="filesToUpload">Un tableau de chemins absolus des fichiers à envoyer.</param>
        /// <param name="workerId">L'identifiant de l'instance.</param>
        /// <param name="archiveDir">L'endroit où déplacer le fichier après téléchargement.</param>
        public void AddFtpTransfer(string[] dirs, string[] filesToUpload, int workerId, string archiveDir)
        {
            if (dirs != null && filesToUpload != null && dirs.Length == filesToUpload.Length)
            {
                var p = new FtpUploadParameter
                {
                    ArchiveDirectory = archiveDir,
                    FilesToUpload = filesToUpload,
                    OutputDirectories = dirs
                };
                AddCustomWorker(workerId, Type.GetType("FtpWorker"), p, null);
            }
        }

        /// <summary>
        /// Effectue un test de connexion avec les paramètres données. 
        /// </summary>
        /// <returns>Si tout est beau, la méthode retourne une valeur nulle. Sinon, elle retourne un message expliquant ce qui s'est passé.</returns>
        public string TestConnection()
        {
            string retour = null;

            // On vérifie les informations de base.
            if (string.IsNullOrEmpty(ConnectionOptions.HostName))
            {
                return "Missing Ftp address.";
            }

            FtpWorker testWorker = null;
            var testParameter = new FtpUploadParameter();

            try
            {
                using (var session = new Session())
                {
                    /* Test sans PATH */

                    // On créer un fichier temporaire.
                    var fileName = Path.GetTempPath() + Guid.NewGuid() + ".csv";

                    // On teste l'identification
                    var uri = GetUploadPath(null, fileName);
                    if (!FtpWebUtil.CheckIfExists(session, uri)) return "Not Exists";
                    
                    // On ajoute quelques données dedans
                    string[] lines = {"First line", "Second line", "Third line"};
                    File.WriteAllLines(fileName, lines);

                    // On créer un travailleur pour ce fichier.
                    string[] files = {fileName};
                    string[] dirs = {null};
                    testParameter.OutputDirectories = dirs;
                    testParameter.FilesToUpload = files;

                    testWorker = new FtpWorker
                    {
                        Id = -1,
                        Pool = this,
                        Parameter = testParameter
                    };

                    // On lance le travailleur.
                    testWorker.DoWork(CancellationToken.None);
                    File.Delete(fileName);
                    if (!testWorker.Success)
                    {
                        retour = "Ftp test error: " + testWorker.Error;
                    }
                    else
                    {
                        try
                        {
                            FtpWebUtil.DeleteFileOnServer(session, uri);
                        }
                        catch (Exception)
                        {
                            retour = "Could not delete file after Ftp test transfert.";
                        }
                    }

                    if (retour == null) // Pas encore d'erreur
                    {
                        /* Test AVEC PATH */

                        // On créer un fichier temporaire.
                        fileName = Path.GetTempPath() + Guid.NewGuid() + ".csv";
                        File.WriteAllLines(fileName, lines);

                        // On teste l'identification
                        uri = GetUploadPath("test/test/test", fileName);
                        FtpWebUtil.CheckIfExists(session, uri);
                        
                        // On créer un travailleur pour ce fichier.
                        files[0] = fileName;
                        dirs[0] = "test/test/test";
                        testWorker = new FtpWorker
                        {
                            Id = -1,
                            Pool = this,
                            Parameter = testParameter
                        };

                        testWorker.DoWork(CancellationToken.None);
                        File.Delete(fileName);
                        if (!testWorker.Success)
                        {
                            retour = "Ftp test (path) error: " + testWorker.Error;
                        }
                        else
                        {
                            try
                            {
                                FtpWebUtil.DeleteFileOnServer(session, uri);
                            }
                            catch (Exception)
                            {
                                retour = "Could not delete file after Ftp test (path) transfert.";
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                log.Debug(ex, "Exception occured during Ftp test run.");
                if (retour != null) return retour;
                if (testWorker == null)
                {
                    retour = "Ftp test exception occured. Worker could not be created. Check log for details.";
                }
                else
                {
                    retour = "Ftp test failed. " + testWorker.Error;
                }
            }

            return retour;
        }

        /// <summary>
        /// Effectue un test de connexion avec les paramètres données. 
        /// </summary>
        /// <returns>Si tout est beau, la méthode retourne une valeur nulle. Sinon, elle retourne un message expliquant ce qui s'est passé.</returns>
        public string SendFalseFile(string[] dirs, string[] filenames)
        {
            string retour = null;

            // On vérifie les informations de base.
            if (string.IsNullOrEmpty(ConnectionOptions?.HostName))
            {
                return "Missing Ftp address.";
            }

            FtpWorker testWorker = null;
            var testParam = new FtpUploadParameter();

            try
            {
                /* Test sans PATH */

                // On créer un fichier temporaire.
                var files = new List<string>();

                foreach (var filename in filenames)
                {
                    var filePath = Path.GetTempPath() + filename;

                    // On ajoute quelques données dedans
                    string[] lines = { DateTime.Now.ToString("G") };
                    File.WriteAllLines(filePath, lines);

                    files.Add(filePath);
                }

                // On créer un travailleur pour ce fichier.
                testParam.OutputDirectories = dirs;
                testParam.FilesToUpload = files.ToArray();
                testWorker = new FtpWorker
                {
                    Id = -1,
                    Pool = this,
                    Parameter = testParam
                };

                testWorker.DoWork(CancellationToken.None);
                foreach (var file in files)
                {
                    File.Delete(file);
                }
                if (!testWorker.Success)
                {
                    retour = "Ftp test error: " + testWorker.Error;
                }

            }
            catch (Exception ex)
            {
                log.Debug(ex, "Exception occured during Ftp test run.");
                if (retour != null) return retour;
                if (testWorker == null)
                {
                    retour = "Ftp test exception occured. Worker could not be created. Check log for details.";
                }
                else
                {
                    retour = "Ftp test failed. " + testWorker.Error;
                }
            }

            return retour;
        }

        /// <summary>
        /// Calcule le chemin (URI) externe pour envoyer un fichier FTP via les configurations de l'instance FtpPool.
        /// </summary>
        /// <param name="outputDir">Le répertoire destination sur le serveur Ftp.</param>
        /// <param name="localFilePath">Le fichier local à envoyer sur le serveur Ftp.</param>
        /// <returns>Le URI ainsi créé.</returns>
        public string GetUploadPath(string outputDir, string localFilePath)
        {
            string retour;
            var fileName = StringUtil.GetFileName(localFilePath);

            if (outputDir != null)
            {
                retour = "/" + outputDir + fileName;
            }
            else
            {
                retour = fileName;
            }

            return retour;
        }


    }
}

