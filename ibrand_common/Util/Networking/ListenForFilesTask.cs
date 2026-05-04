using System;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using NLog;

namespace NomadLogic.Common.Util.Networking
{
    /// <summary>
    ///     Cette tâche longue écoute sur un port en particulier et lit des demandes d'envoi de fichiers.
    ///     Cette même classe écoute aussi les demandes d'identifications d'autres système, signalant qu'en effet, il
    ///     existe un écoutant pour envoi direct de fichiers sur cette machine.
    ///     <br />Chaque demande de Socket, lorsqu'interceptée, et gérée sur un thread séparé.
    ///     <br />Quand on doit cesser d'écouter, on ferme toutes les connexions en cours et on libère le Socket, pour ne pas
    ///     bloquer cette ressource indéfiniment.
    /// </summary>
    public class ListenForFilesTask
    {
        private static readonly Logger Log = LogManager.GetCurrentClassLogger();
        private const int MAX_LINE_LENGTH = 2048;
        private readonly Encoding _enc;
        private readonly TcpListenerWrapper _tcpListener;

        public ListenForFilesTask(int port)
        {
            _enc = Encoding.UTF8;
            _tcpListener = new TcpListenerWrapper(IPAddress.Any, port);
            Port = port;
        }

        public int Port { get; }

        /// <summary>
        ///     Exécute l'écoute sur le Socket en question.
        ///     <br />La méthode commence par ouvrir le port. Si une erreur se produit ou si la connection est fermée, la méthode va
        ///     continuellement tenter de se connecter, mais avec un intervalle de temps entre les tentatives.
        ///     <br />
        ///     Par la suite, dès qu'une connexion est faite, elle sera traitée dans une tâche séparée. Ces tâches sont complétement indépendantes
        ///     et ne devraient pas affecter l'exécution de la tâche mère et des autres (à moins que le socket en dessous ne ferme,
        ///     auquel cas toutes seront affectés, bien entendu).
        ///     <br />Lorsque le signal de fermer l'écoute du port se fait, on ferme l'écoutant d'abord, puis on s'assure que les
        ///     tâches enfants se ferment par la suite, avec un token de fermeture séparé.
        /// </summary>
        /// <param name="cancel">Jeton de cancellation.</param>
        public void DoWork(CancellationToken cancel)
        {
            Log.Trace("Started looking for file task.");
            var listeConnexionActive = new List<Task>();
            CancellationTokenSource cts = null; // utilisé pour les connexions enfants.

            while (!cancel.IsCancellationRequested)
            {
                try
                {
                    cts = new CancellationTokenSource();

                    // Ouverture du Socket, s'il n'est pas déjà ouvert.
                    if (!_tcpListener.Active)
                    {
                        _tcpListener.Start();
                    }

                    if (_tcpListener.Pending())
                    {
                        var client = _tcpListener.AcceptTcpClient();
                        var cts1 = cts;
                        listeConnexionActive.Add(Task.Run(() => { SaveFile(client, cts1.Token); }, cts1.Token));
                    }
                    else
                    {
                        // On attend une nouvelle connexion.
                        for (var i = 0; i < 10; i++)
                        {
                            if (cancel.IsCancellationRequested || _tcpListener.Pending())
                            {
                                i = 10;
                            }
                            else
                            {
                                Thread.Sleep(50);
                            }
                        }
                    }

                    // Avant une nouvelle boucle, on vide les tâche complétées.
                    listeConnexionActive.RemoveAll(task => task.IsCompleted);
                }
                catch (SocketException e)
                {
                    Log.Info(e, "Connection refused.");
                }
                catch (Exception e)
                {
                    listeConnexionActive.RemoveAll(task => task.IsCompleted);
                    Log.Debug(e, "Retrying to connect to socket in 5 seconds.");
                    for (var i = 0; i < 50; i++)
                    {
                        if (cancel.IsCancellationRequested)
                        {
                            i = int.MaxValue;
                        }
                        else
                        {
                            Thread.Sleep(100);
                        }
                    }
                }
            }

            //Demande d'arrêt reçue. On stoppe toutes les requêtes en cours et ferme le tout.
            StopListener(listeConnexionActive, cts);

            Log.Trace("Picture lookout task has finished.");
        }

        /// <summary>
        /// Indique si le service est en train de rouler.
        /// </summary>
        public bool IsRunning => _tcpListener?.Active ?? false;

        private void StopListener(List<Task> listeConnexionActive, CancellationTokenSource cts)
        {
            if (!_tcpListener.Active) return;
            _tcpListener.Stop();

            if (cts == null) return;
            cts.Cancel();

            listeConnexionActive.RemoveAll(task => task.IsCompleted);

            if (listeConnexionActive.Count <= 0) return;
            Log.Trace("Stopping {0} currently active TCP connection(s).", listeConnexionActive.Count);
            Task.WaitAll(listeConnexionActive.ToArray());
            Log.Trace("Active TCP connection(s) closed.");

            listeConnexionActive.Clear();
        }

        private void SaveFile(TcpClient client, CancellationToken cancel)
        {
            Log.Trace("Downloading file from socket.");

            try
            {
                using (var clientStream = client.GetStream())
                {
                    // On s'assure que l'on attend pas à l'infini...
                    clientStream.ReadTimeout = 3000;

                    string fileName = null;
                    string fileSize = null;

                    var header = ReadLine(clientStream);

                    var headerValues = header.Split(';');

                    if (headerValues[0].Equals("=hello"))
                    {
                        var buffer = _enc.GetBytes("hello");

                        clientStream.Write(buffer, 0, buffer.Length);
                        clientStream.Flush();
                    }
                    else if (headerValues[0].Equals("=fbegin"))
                    {
                        for (var i = 1; i < headerValues.Length; i++)
                        {
                            var property = GetProperty(headerValues[i]);

                            if (property == null) continue;
                            if (property.Name.Equals("size"))
                                fileSize = property.Value;
                            else if (property.Name.Equals("file_name"))
                                fileName = property.Value;
                        }

                        if (fileName == null || fileSize == null) return;
                        var fileLength = int.Parse(fileSize);

                        var tmpFile = DirectoryUtil.CreateLocalTemporaryFile(GenerateUniqueId.GetId() + ".tmp");
                        var finalFile = Path.Combine(ReceptionDirectory.FullName, fileName);

                        var interrupted = false;

                        using (var fs = new FileStream(tmpFile, FileMode.CreateNew))
                        {
                            using (var w = new BinaryWriter(fs))
                            {
                                var temp = new byte[512];
                                var totalRead = 0;

                                try
                                {
                                    var read = clientStream.Read(temp, 0, temp.Length);

                                    while (read > 0)
                                    {
                                        if (cancel.IsCancellationRequested)
                                        {
                                            interrupted = true;
                                            break;
                                        }

                                        w.Write(temp, 0, read);
                                        totalRead += read;

                                        if (totalRead == fileLength)
                                            break;

                                        read = clientStream.Read(temp, 0, temp.Length);
                                    }
                                }
                                catch (Exception e)
                                {
                                    Log.Error(e, "Could not save this file: {0}", finalFile);
                                    interrupted = true;
                                }
                                finally
                                {
                                    try
                                    {
                                        byte[] buffer;

                                        if (totalRead != fileLength)
                                        {
                                            buffer = _enc.GetBytes("error");
                                            interrupted = true;
                                        }
                                        else
                                            buffer = _enc.GetBytes("ok");

                                        clientStream.Write(buffer, 0, buffer.Length);
                                        clientStream.Flush();
                                    }
                                    catch (Exception e)
                                    {
                                        Log.Error(e, "Could not send final message to sender.");
                                        interrupted = true;
                                    }
                                }
                            }
                        }

                        if (!File.Exists(tmpFile)) return;
                        // Si interrompu, le fichier est invalide et on va le détruire.
                        if (interrupted)
                        {
                            File.Delete(tmpFile);
                        }
                        else
                        {
                            if (File.Exists(finalFile))
                            // On écrase le nouveau fichier, si on nom similaire existe déjà.
                            {
                                File.Delete(finalFile);
                            }

                            File.Move(tmpFile, finalFile);

                            // Tout est beau, on invoque le call-back de réception, si existant.
                            OnFileReceptionCallback?.Invoke(finalFile);
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                Log.Error(ex, "Could not save file from socket.");
            }
            finally
            {
                client?.Close();
            }
        }

        /// <summary>
        /// Indique quel doit être le répertoire qui recevra les fichiers reçus par socket.
        /// </summary>
        public DirectoryInfo ReceptionDirectory { get; set; } = new DirectoryInfo(DirectoryUtil.GetLocalMediaDir(
            DirectoryUtil.NomadLogicMediaDirectoriesEnum.listen_from_ibrand));

        /// <summary>
        /// Cette action sera invoquée à chaque fois que l'on reçoit un nouveau fichier. Il est recommandé de ne pas faire de traitement trop intense
        /// dans cette action pour ne pas bloquer l'exécution du fil de réception de fichier.
        /// </summary>
        public Action<string> OnFileReceptionCallback { get; set; }

        protected string ReadLine(NetworkStream stream)
        {
            int c;
            var max = 0;
            var line = new byte[MAX_LINE_LENGTH];
            while ((c = stream.ReadByte()) != -1)
            {
                if (c == '\r')
                {
                    continue;
                }
                if (c == '\n')
                {
                    break;
                }

                if (max == MAX_LINE_LENGTH)
                {
                    throw new Exception("line too long");
                }

                line[max++] = (byte)c;
            }

            if (c == -1)
            {
                return null;
            }

            return _enc.GetString(line, 0, max);
        }

        protected HeaderProperty GetProperty(string s)
        {
            var pos = s.IndexOf("=", StringComparison.Ordinal);
            if (pos == -1)
            {
                return null;
            }

            var property = new HeaderProperty
            {
                Value = s.Substring(pos + 1),
                Name = s.Substring(0, pos)
            };
            return property;
        }

        /// <summary>
        ///     Classe interne
        /// </summary>
        protected class HeaderProperty
        {
            public string Name;
            public string Value;
        }

        /// <summary>
        ///     Wrapper autour de TcpListner pour avoir accès à la propriété Active.
        /// </summary>
        protected class TcpListenerWrapper : TcpListener
        {
            public TcpListenerWrapper(IPAddress localaddr, int port)
                : base(localaddr, port)
            {
            }

            public new bool Active => base.Active;
        }
    }
}