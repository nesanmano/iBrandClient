using System;
using System.IO;
using System.Linq;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using NLog;

namespace NomadLogic.Common.Util.Networking
{
    public class UploadFileToTcpListener
    {
        private const int BYTES_TO_READ = 1024;
        private static readonly Logger log = LogManager.GetCurrentClassLogger();

        public UploadFileToTcpListener(string address, int port)
        {
            IpAddress = address;
            Port = port;
        }

        public string IpAddress { get; }

        public int Port { get; }

        public bool IsFileServerRunning(int timeoutMili = 1000)
        {
            TcpClient tcpClient = null;
            NetworkStream stream = null;
            var running = false;

            if (timeoutMili < 300) timeoutMili = 300;

            try
            {
                tcpClient = new TcpClient(IpAddress, Port);

                stream = tcpClient.GetStream();

                var cts = new CancellationTokenSource();
                var t = new Task<bool>(() => IsRunning(stream, cts.Token), cts.Token);

                var max = DateTime.Now + TimeSpan.FromMilliseconds(timeoutMili);
                t.Start();
                while (DateTime.Now < max && !t.IsCompleted)
                {
                    try
                    {
                        Thread.Sleep(100);
                    }
                    catch (Exception)
                    {
                        // die nice
                    }
                }
                cts.Cancel();

                Task.WaitAll(t);

                running = t.Result;
            }
            catch (IOException ex)
            {
                log.Error(ex, "Connection Closed");
            }
            catch (Exception ex)
            {
                log.Error(ex, "Error Upload File");
            }
            finally
            {
                stream?.Close();
                tcpClient?.Close();
            }

            return running;
        }

        /// <summary>
        /// Indique le nombre de millisecondes avant que la connexion ne fasse un "time out" sur une opération d'écriture du flux.
        /// 3000 par défaut.
        /// </summary>
        public int ReadTimeout { get; set; } = 3000;
        /// <summary>
        /// Indique le nombre de millisecondes avant que la connexion ne fasse un "time out" sur une opération de lecture du flux.
        /// 3000 par défaut.
        /// </summary>
        public int SendTimeout { get; set; } = 3000;
        /// <summary>
        /// Indique le nombre de millisecondes avant que la connexion ne fasse un "time out" sur la connexion intiale.
        /// 500 par défaut.
        /// </summary>
        public int ConnectTimeout { get; set; } = 500;

        /// <summary>
        /// Méthode d'exécution de l'envoi. Retourne FALSE si aucune erreur n'a été détectée. Fonctionne de façon synchrone avec des timeout de connexion.
        /// Il est possible de réutiliser la même instance pour chaque appel de cette méthode.
        /// </summary>
        /// <param name="fileName">Envoie le fichier vers le PrintStation</param>
        /// <param name="token">Jeton de cancellation.</param>
        /// <returns>FALSE si aucune erreur n'est survenue.</returns>
        public bool Go(string fileName, CancellationToken? token)
        {
            log.Trace("Upload File Launched : {0}, Thread={1}", fileName, Thread.CurrentThread.ManagedThreadId);
            TcpClient tcpClient = null;
            NetworkStream stream = null;
            var error = true;
            try
            {
                tcpClient = new TcpClient(IpAddress, Port);

                stream = tcpClient.GetStream();

                if (SendFile(stream, fileName, token))
                {
                    error = false;
                }
            }
            catch (Exception ex)
            {
                error = true;
                log.Error(ex, "Error Upload File [{1}], Thread=[{0}]", Thread.CurrentThread.ManagedThreadId, fileName);
            }
            finally
            {
                stream?.Close();
                tcpClient?.Close();
            }

            return error;
        }

        /// <summary>
        /// Cette méthode est appellée avant le premier envoi de bytes pour indiquer quel est le nombre total de bytes à envoyer.
        /// N'est utile que pour des méthode UI en feedback.
        /// </summary>
        public Action<long> OnInitBeforeSend { get; set; }

        /// <summary>
        /// À chaque mise à jour on envoie le nombre de bytes envoyés au total.
        /// N'est utile que pour des méthode UI en feedback.
        /// </summary>
        public Action<long> OnSendProgress { get; set; }

        private bool SendFile(Stream stream, string fileName, CancellationToken? token)
        {
            FileStream fs = null;
            BinaryReader r = null;
            bool completed;
            try
            {
                var sendAsynchTask = Task.Run(() =>
                {
                    var localComplete = false;
                    if (!File.Exists(fileName)) return false;
                    if (token?.IsCancellationRequested ?? false) return false;

                    OnInitBeforeSend?.Invoke(new FileInfo(fileName).Length);

                    log.Trace("Upload File Name : {0}", fileName);
                    fs = new FileStream(fileName, FileMode.Open, FileAccess.Read);

                    r = new BinaryReader(fs);

                    WriteHeader(fs.Length, StringUtil.GetFileName(fileName), stream);
                    var bytesSend = 0;
                    while (!(token?.IsCancellationRequested ?? false))
                    {
                        var b = r.ReadBytes(BYTES_TO_READ);

                        if (b.Length != 0)
                        {
                            stream.Write(b, 0, b.Length);
                            bytesSend += b.Length;
                            OnSendProgress?.Invoke(bytesSend);
                        }

                        if (b.Length < BYTES_TO_READ)
                            break;
                    }

                    stream.Flush();

                    if (token?.IsCancellationRequested ?? false) return false;

                    // On lit maintenant la réponse.
                    var temp = new byte[512];
                    var read = stream.Read(temp, 0, temp.Length);
                    var returnValue = "";

                    while (read != -1 && !(token?.IsCancellationRequested ?? false))
                    {
                        returnValue = Encoding.UTF8.GetString(temp, 0, read) + returnValue;

                        if (returnValue.Equals("ok"))
                        {
                            localComplete = true;
                            break;
                        }

                        if (returnValue.Equals("error"))
                        {
                            break;
                        }

                        read = stream.Read(temp, 0, temp.Length);
                    }

                    return localComplete && !(token?.IsCancellationRequested ?? false);
                });

                var result = Task.WhenAll(sendAsynchTask).Result;

                completed = result?.All(b => b) ?? false;
            }
            catch (IOException e)
            {
                log.Error(e, "Error during reading/sending of file.");
                completed = false;
            }
            catch (Exception e)
            {
                log.Error(e, "Generic error during send.");
                completed = false;
            }
            finally
            {
                r?.Close();
                fs?.Close();
            }

            log.Trace("Upload File Name finished: file={0}, success={1}, cancelled={2}", fileName, completed, token?.IsCancellationRequested ?? false);
            return completed;
        }

        private static void WriteHeader(long fileSize, string fileName, Stream stream)
        {
            var header = "=fbegin;size=";
            header += fileSize;
            header += ";file_name=" + fileName;
            header += "\r\n";

            var h = Encoding.UTF8.GetBytes(header);
            stream.Write(h, 0, h.Length);
        }

        private static bool IsRunning(Stream stream, CancellationToken token)
        {
            if (token.IsCancellationRequested) return false;

            var h = Encoding.UTF8.GetBytes("=hello;\r\n");
            stream.Write(h, 0, h.Length);
            stream.Flush();

            var temp = new byte[512];

            var read = stream.Read(temp, 0, temp.Length);

            var returnValue = "";

            while (read != -1 && !token.IsCancellationRequested)
            {
                returnValue = Encoding.UTF8.GetString(temp, 0, read) + returnValue;

                if (returnValue.Equals("hello"))
                    return true;

                read = stream.Read(temp, 0, temp.Length);
            }

            return false;
        }
    }
}
