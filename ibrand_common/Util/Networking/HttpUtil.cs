using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Xml;
using JetBrains.Annotations;
using NLog;

// ReSharper disable NonReadonlyMemberInGetHashCode

namespace NomadLogic.Common.Util.Networking
{
    /// <summary>
    /// Classe de méthode utile pour gérer les demandes et les réponses HTTP, en plus de quelques autres méthodes plus complexes
    /// pour des opérations fréquentes.
    /// </summary>
    [Obsolete, PublicAPI]
    public class HttpUtil
    {
        private static readonly Logger log = LogManager.GetCurrentClassLogger();

        /// <summary>
        /// Génère une requête HTTP avec des données POST.
        /// </summary>
        /// <param name="serverAddress">L'addresse à envoyer l'information.</param>
        /// <param name="postData">Les données POST.</param>
        /// <returns>La requête web.</returns>
        public static HttpWebRequest GenerateRequest(string serverAddress, string postData)
        {
            var request = (HttpWebRequest)WebRequest.Create(serverAddress);
            request.AllowAutoRedirect = false;
            request.Method = "POST";
            request.ContentType = "application/x-www-form-urlencoded";
            request.ProtocolVersion = HttpVersion.Version11;
            request.KeepAlive = true;
            var bytes = Encoding.ASCII.GetBytes(postData);

            request.ContentLength = bytes.Length;

            var stream = request.GetRequestStream();

            stream.Write(bytes, 0, bytes.Length);
            stream.Close();

            return request;
        }

        /// <summary>
        /// Génère une requête HTTP sans données d'envoi POST.
        /// </summary>
        /// <param name="serverAddress">L'addresse visée.</param>
        /// <returns>La requête web.</returns>
        public static HttpWebRequest GenerateRequest(string serverAddress)
        {
            return GenerateRequest(serverAddress, "");
        }
        
        /// <summary>
        /// Génère une requête Web sur notre serveur avec des données d'action, de données POST et un temps maximal pour les exécuter.
        /// </summary>
        /// <param name="server">L'addresse du serveur.</param>
        /// <param name="action">L'action (servlet) à exécuter.</param>
        /// <param name="postData">Les données POST.</param>
        /// <param name="timeoutMili">Milisecondes avant échec de connexion.</param>
        /// <returns>La requête web.</returns>
        public static HttpWebRequest GenerateRequest(Server server, string action, string postData, int timeoutMili)
        {
            var serverAdr = "http://";
            serverAdr += server.Address;
            serverAdr += ":" + server.WebPort + "/"+ server.WebApp +"/";
            var request = (HttpWebRequest)WebRequest.Create(serverAdr + action);
            request.AllowAutoRedirect = false;
            request.Method = "POST";
            request.ContentType = "application/x-www-form-urlencoded";
            request.ProtocolVersion = HttpVersion.Version11;
            request.Timeout = timeoutMili;

            var bytes = Encoding.ASCII.GetBytes(postData);

            request.ContentLength = bytes.Length;

            var stream = request.GetRequestStream();

            stream.Write(bytes, 0, bytes.Length);
            stream.Close();

            return request;
        }

        public HttpWebRequest GeneratePostJSonRequest(string url, string jsonContent, int timeoutMili)
        {
            var request = (HttpWebRequest)WebRequest.Create(url);
            request.AllowAutoRedirect = false;
            request.Method = "POST";
            request.ContentType = "application/json";
            request.Accept = "application/json";
            request.ProtocolVersion = HttpVersion.Version11;
            request.Timeout = timeoutMili;

            var encoding = new UTF8Encoding();
            var bytes = encoding.GetBytes(jsonContent);

            request.ContentLength = bytes.Length;

            using (var dataStream = request.GetRequestStream())
            {
                dataStream.Write(bytes, 0, bytes.Length);
            }

            return request;
        }

        public HttpWebRequest GenerateGetRequest(string url, int timeoutMili)
        {
            var request = (HttpWebRequest)WebRequest.Create(url);
            request.AllowAutoRedirect = false;
            request.Method = "GET";
            request.ContentType = "application/json";
            request.Accept = "application/json";
            request.ProtocolVersion = HttpVersion.Version11;
            request.Timeout = timeoutMili;

            return request;
        }

        /// <summary>
        /// Génère une requête Web sur notre serveur avec des données d'action, de données POST. Le temps maximal est de 3000 milisecondes.
        /// </summary>
        /// <param name="server">L'addresse du serveur.</param>
        /// <param name="action">L'action (servlet) à exécuter.</param>
        /// <param name="postData">Les données POST.</param>
        /// <returns>La requête web.</returns>
        public static HttpWebRequest GenerateRequest(Server server, string action, string postData)
        {
            return GenerateRequest(server, action, postData, 3000);
        }

        /// <summary>
        /// Va chercher un fichier sur un serveur avec 20 secondes de time-out.
        /// </summary>
        /// <param name="server">l'addresse du serveur complète, avant le nom de fichier.</param>
        /// <param name="file">Le nom du fichier à aller chercher.</param>
        /// <returns>La requête web.</returns>
        public static HttpWebRequest GenerateRequest(Server server, string file)
        {
            var serverAdr = "http://";
            serverAdr += server.Address;
            serverAdr += ":" + server.WebPort + "/" + server.WebApp + "/";
            var request = (HttpWebRequest)WebRequest.Create(serverAdr + file);
            request.AllowAutoRedirect = false;
            request.Timeout = 20000;
            request.AllowWriteStreamBuffering = false;

            return request;
        }

        private static bool IsServerFileExist(Server server, string serverFile)
        {
            Stream s = null;

            try
            {
                var request = GenerateRequest(server, "FileExist", "&file=" + serverFile);

                s = request.GetResponse().GetResponseStream();

                var manifest = new XmlDocument();
                if (s == null) return false;

                manifest.Load(s);
                var root = manifest.DocumentElement;
                var versionNode = root?.SelectSingleNode("exist");
                if (versionNode == null) return false;

                var response = versionNode.InnerText;
                return response.Equals("true");
            }
            catch (Exception e)
            {
                throw new ApplicationException("Could not check if file exists on server.", e);
            }
            finally
            {
                s?.Close();
            }
        }

        /// <summary>
        /// Va chercher un fichier sur un serveur et le sauvegarde dans un fichier local.
        /// Si existant, le fichier local est écrasé.
        /// </summary>
        /// <param name="server">L'instance de serveur à utiliser.</param>
        /// <param name="localFile">Le fichier local.</param>
        /// <param name="serverFile">Le nom du fichier à chercher sur le serveur.</param>
        [Obsolete]
        public static void GetServerFile(Server server, string localFile, string serverFile)
        {
            FileStream fs = null;
            Stream s = null;

            if (!IsServerFileExist(server, serverFile))
                throw new ApplicationException("File :" + serverFile + " does not exist on the server");

            try
            {
                var request = GenerateRequest(server, serverFile);

                if (File.Exists(localFile)) { File.Delete(localFile); }

                fs = new FileStream(localFile, FileMode.Create);
                s = request.GetResponse().GetResponseStream();

                var buffer = new byte[4096];

                if (s == null) return;
                var bytesRead = s.Read(buffer, 0, buffer.Length);
                while (bytesRead > 0)
                {
                    fs.Write(buffer, 0, bytesRead);
                    bytesRead = s.Read(buffer, 0, buffer.Length);
                }
            }
            catch (Exception e)
            {
                throw new ApplicationException("Could not download file " + serverFile, e);
            }
            finally
            {
                s?.Close();
                fs?.Close();
            }
        }

        /// <summary>
        /// Vide les cookies du système en invoquant une commande de Windows.
        /// </summary>
        public static void DeleteCookies()
        {
            var startInfo = new ProcessStartInfo
            {
                CreateNoWindow = true,
                UseShellExecute = false,
                FileName = "rundll32.exe",
                WindowStyle = ProcessWindowStyle.Hidden,
                Arguments = "InetCpl.cpl,ClearMyTracksByProcess 10"
            };

            try
            {
                // Start the process with the info we specified.
                // Call WaitForExit and then the using statement will close.
                using (var exeProcess = Process.Start(startInfo))
                {
                    exeProcess?.WaitForExit();
                }
            }
            catch(Exception e)
            {
                log.Error(e, "Could not delete cookies.");
            }
        }

        /// <summary>
        /// Télécharge un fichier à partir d'un URL dans un fichier local.
        /// </summary>
        /// <param name="fileUrl">Le fichier à aller chercher sur le Web.</param>
        /// <param name="localFile">Le fichier local dans lequel le sauvegarder.</param>
        public static void DownloadFileFromUrl(string fileUrl, string localFile)
        {
            FileStream fs = null;
            Stream s = null;

            try
            {
                var request = (HttpWebRequest)WebRequest.Create(fileUrl);
                request.Timeout = 40000;

                request.AllowWriteStreamBuffering = false;

                if (File.Exists(localFile)) { File.Delete(localFile); }

                fs = new FileStream(localFile, FileMode.Create);

                s = request.GetResponse().GetResponseStream();

                var buffer = new byte[4096];

                if (s == null) return;
                var bytesRead = s.Read(buffer, 0, buffer.Length);
                while (bytesRead > 0)
                {
                    fs.Write(buffer, 0, bytesRead);
                    bytesRead = s.Read(buffer, 0, buffer.Length);
                }
            }
            catch (Exception e)
            {
                throw new ApplicationException("Could not download file " + fileUrl, e);
            }
            finally
            {
                s?.Close();
                fs?.Close();
            }
        }
        
        /// <summary>
        /// Méthode pour télécharger un fichier à partir d'une requête web HTTP.
        /// Voir ici pour plus d'information: https://stackoverflow.com/questions/16514027/download-multiple-files-async-and-wait-for-all-of-them-to-finish-before-executin/16514441#16514441
        /// </summary>
        /// <param name="url">Le URL du fichier.</param>
        /// <param name="outFile">Le fichier résultat.</param>
        /// <param name="headers">Les entêtes à donner à la requête.</param>
        /// <param name="ct">Le jeton de cancellation.</param>
        /// <param name="prog">Un rapport asynchrone sur l'avancement du téléchargement.</param>
        /// <returns>TRUE si et seulement si le fichier à été correctement sauvegardé.</returns>
        public static async Task<bool> DownloadFileAsync(Uri url, FileInfo outFile, CancellationToken ct, 
            WebHeaderCollection headers = null, IProgress<SingleFileDownloadReport> prog = null)
        {
            if (outFile == null || url == null) return false;

            try
            {
                using (var webClient = new WebClient
                {
                    Headers = headers ?? new WebHeaderCollection()
                })
                {
                    ct.Register(webClient.CancelAsync);

                    var fileUri = url.AbsoluteUri;
                    var fileName = StringUtil.GetFileName(url.AbsoluteUri);
                    long total = 0;
                    long received = 0;

                    if (prog != null)
                    {
                        // TODO Refaire Rapport de progrès.
                        webClient.DownloadProgressChanged += (sender, args) =>
                        {
                            total = args.TotalBytesToReceive;
                            received = args.BytesReceived;
                            prog.Report(new SingleFileDownloadReport
                            {
                                Uri = fileUri,
                                TotalBytesToDownload = total,
                                ReceivedBytes = received,
                                FileName = fileName
                            });
                        };
                    }
                    
                    //webClient.Credentials = System.Net.CredentialCache.DefaultNetworkCredentials;
                    await webClient.DownloadFileTaskAsync(url, outFile.FullName).ConfigureAwait(false);

                    prog?.Report(new SingleFileDownloadReport
                    {
                        Uri = fileUri,
                        TotalBytesToDownload = total,
                        ReceivedBytes = received,
                        FileName = fileName,
                        Finished = true
                    });

                    outFile.Refresh();

                    return outFile.Exists;
                }
            }
            catch (Exception e)
            {
                log.Error(e);
                return false;
            }
        }

        /// <summary>
        /// Télécharge plusieurs fichiers via HTTP via une seule méthode asynchrone.
        /// Peut utiliser un rapport de progès aggrégé.
        /// Voir le post suivant pour plus de precision: https://stackoverflow.com/questions/16514027/download-multiple-files-async-and-wait-for-all-of-them-to-finish-before-executin/16514441#16514441
        /// </summary>
        /// <param name="doclist">Liste des dicuments à aller chercher.</param>
        /// <param name="ct">Jeton de cancellation.</param>
        /// <param name="report">Le rapport aggrégé des téléchargents.</param>
        /// <returns></returns>
        public static async Task DownloadMultipleFilesAsync(List<Tuple<Uri, FileInfo, WebHeaderCollection>> doclist, CancellationToken ct, IProgress<MultipleFileDownloadReport> report = null)
        {
            IProgress<SingleFileDownloadReport> prog2 = null;
            var dfiles = new ConcurrentDictionary<string, SingleFileDownloadReport>();
            
            if (report != null)
            {
                prog2 = new Progress<SingleFileDownloadReport>(res =>
                {
                    dfiles.AddOrUpdate(res.Uri, res, (k,v) => v);

                    report.Report(new MultipleFileDownloadReport
                    {
                        DownloadList = dfiles
                    });
                });
            }
            
            await Task.WhenAll(doclist.Select(doc => DownloadFileAsync(doc.Item1, doc.Item2, ct, doc.Item3, prog2))).ConfigureAwait(false);

            report?.Report(new MultipleFileDownloadReport
            {
                DownloadList = dfiles
            });

        }
    }

    public class SingleFileDownloadReport : IEquatable<SingleFileDownloadReport>
    {
        public string Uri { get; set; }
        public string FileName { get; set; }
        public long TotalBytesToDownload { get; set; }
        public long ReceivedBytes { get; set; }
        public bool Finished { get; set; }

        public bool Equals(SingleFileDownloadReport other)
        {
            if (other is null) return false;
            if (ReferenceEquals(this, other)) return true;
            return Uri == other.Uri;
        }

        public override bool Equals(object obj)
        {
            if (obj is null) return false;
            if (ReferenceEquals(this, obj)) return true;
            if (obj.GetType() != this.GetType()) return false;
            return Equals((SingleFileDownloadReport) obj);
        }

        public override int GetHashCode()
        {
            return (Uri != null ? Uri.GetHashCode() : 0);
        }
    }

    [PublicAPI]
    public class MultipleFileDownloadReport
    {
        internal IReadOnlyDictionary<string, SingleFileDownloadReport> DownloadList { get; set; }

        public long GetTotalToReceive()
        {
            return DownloadList.Values.Sum(dl => dl.TotalBytesToDownload);
        }

        public long GetTotalReceived()
        {
            return DownloadList.Values.Sum(dl => dl.ReceivedBytes);
        }

        public int GetPercentageBytesDone()
        {
            return (int)(GetTotalReceived()/(double)GetTotalToReceive()) * 100;
        }

        public int GetTotalOfFinishedDownloads()
        {
            return DownloadList.Values.Sum(dl => dl.Finished ? 1 : 0);
        }
    }
}
