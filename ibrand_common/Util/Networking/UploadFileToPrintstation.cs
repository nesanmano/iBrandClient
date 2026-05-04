using System;
using System.IO;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using NLog;

namespace NomadLogic.iBrand.Common.Util.Net
{
    public class UploadFileToPrintStation
    {
        private static readonly Logger log = LogManager.GetCurrentClassLogger();
        private readonly string _address;
        private readonly string _port;

        public UploadFileToPrintStation(string address, string port)
        {
            _address = address;
            _port = port;
        }

        public bool IsFileServerRunning(int timeoutMili = 1000)
        {
            TcpClient tcpClient = null;
            NetworkStream stream = null;
            var running = false;

            if (timeoutMili < 300) timeoutMili = 300;

            try
            {
                tcpClient = new TcpClient(_address, int.Parse(_port));

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
        /// Méthode d'exécution de l'envoi. Retourne FALSE si aucune erreur n'a été détectée.
        /// </summary>
        /// <param name="fileName">Envoie le fichier vers le PrintStation</param>
        /// <returns>FALSE si aucune erreur n'est survenue.</returns>
        public bool Go(string fileName)
        {
            log.Trace("Upload File Launched : {0}, Thread={1}", fileName, Thread.CurrentThread.ManagedThreadId);
            TcpClient tcpClient = null;
            NetworkStream stream = null;
            var error = true;
            try
            {
                tcpClient = new TcpClient(_address, int.Parse(_port));

                stream = tcpClient.GetStream();

                if (SendFile(stream, fileName))
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

        /*
        private void CopyFileToWorkDir(string fileName)
        {
            var tempDir = DirectoryUtil.GetLocalApplicationWorkDir() + _address;

            if (!Directory.Exists(tempDir))
            {
                Directory.CreateDirectory(tempDir);
            }

            var newFileName = tempDir + Path.DirectorySeparatorChar + StringUtil.GetFileName(fileName);

            if (!File.Exists(newFileName))
                File.Copy(fileName, newFileName);
        }

        private void SendWorkDirFiles()
        {
            var tempDir = DirectoryUtil.GetLocalApplicationWorkDir() + _address;

            if (Directory.Exists(tempDir))
            {
                var files = Directory.GetFiles(tempDir);

                foreach (var file in files)
                {
                    TcpClient tcpClient = null;
                    NetworkStream stream = null;
                    try
                    {
                        tcpClient = new TcpClient(_address, int.Parse(_port));

                        stream = tcpClient.GetStream();

                        if (SendFile(stream, file))
                            File.Delete(file);
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

                }
            }
        }
        */
        private static bool SendFile(Stream stream, string fileName)
        {
            FileStream fs = null;
            BinaryReader r = null;
            var completed = false;
            try
            {
                if (!File.Exists(fileName)) return false;

                log.Trace("Upload File Name : {0}", fileName);
                fs = new FileStream(fileName, FileMode.Open, FileAccess.Read);

                r = new BinaryReader(fs);

                WriteHeader(fs.Length, StringUtil.GetFileName(fileName), stream);

                while (true)
                {
                    var b = r.ReadBytes(1024);

                    if (b.Length != 0)
                    {
                        stream.Write(b, 0, b.Length);
                    }

                    if (b.Length < 1024)
                        break;
                }

                stream.Flush();

                // On lit maintenant la réponse.
                var temp = new byte[512];
                var read = stream.Read(temp, 0, temp.Length);
                var returnValue = "";

                while (read != -1)
                {
                    returnValue = Encoding.UTF8.GetString(temp, 0, read) + returnValue;

                    if (returnValue.Equals("ok"))
                    {
                        completed = true;
                        break;
                    }

                    if (returnValue.Equals("error"))
                    {
                        break;
                    }

                    read = stream.Read(temp, 0, temp.Length);
                }
            }
            catch (Exception e)
            {
                log.Error(e, "Error during reading/sending of file.");
            }
            finally
            {
                r?.Close();
                fs?.Close();
            }

            log.Trace("Upload File Name finished: {0}, success={1}", fileName, completed);
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
            var h = Encoding.UTF8.GetBytes("=hello;\r\n");
            stream.Write(h, 0, h.Length);
            stream.Flush();

            var temp = new byte[512];

            var read = stream.Read(temp, 0, temp.Length);

            var returnValue = "";

            while (read > -1 && !token.IsCancellationRequested)
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
