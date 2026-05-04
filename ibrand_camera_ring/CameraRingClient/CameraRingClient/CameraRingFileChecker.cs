using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading;
using Ionic.Zip;
using Ionic.Zlib;
using NLog;
using NomadLogic.Common.Util;

namespace NomadLogic.iBrand.CameraRingClient
{
    /// <summary>
    /// Cette classe, une fois lancée, va stocker des images durant une période déterminée.
    /// Une fois le temps écoulé ou si toutes les images attendues sont obtenues, on cesse l'écoute et on fait un
    /// fichier ZIP avec toutes les images récoltées (si complet).
    /// On notifie ensuite la référence qui doit écouter le résultat.
    /// </summary>
    internal class CameraRingFileChecker
    {
        private static readonly Logger log = LogManager.GetCurrentClassLogger();

        private readonly Dictionary<string, byte> _serialListLeft; // byte, car c'est la plus petite unité de mémoire de C#
        private readonly Dictionary<string, FileInfo> _serialListDone;
        private readonly TimeSpan _waitTime;
        private volatile bool _finished;
        private readonly Action<string, bool, ICollection<string>> _callback;
        private readonly int _missingTolerance;

        public CameraRingFileChecker(int missingTolerance, IEnumerable<string> cameraSerials, TimeSpan waitTime, Action<string, bool, ICollection<string>> callback)
        {
            _missingTolerance = missingTolerance;
            _waitTime = waitTime;
            _callback = callback;
            _serialListLeft = new Dictionary<string, byte>();
            foreach (var serial in cameraSerials)
            {
                _serialListLeft.Add(serial, byte.MinValue);
            }
            _serialListDone = new Dictionary<string, FileInfo>();

        }

        internal void CheckForNewImageFiles(CancellationToken token)
        {
            var stopTime = DateTime.Now.Add(_waitTime);

            while (!token.IsCancellationRequested && !_finished)
            {
                if (DateTime.Now < stopTime)
                {
                    try
                    {
                        Thread.Sleep(100);
                    }
                    catch (Exception)
                    {
                        // Die nice
                    }
                }
                else
                {
                    _finished = true;
                }
            }

            // Vérification des fichiers.
            string resultZip = null;
            if (_serialListLeft.Count <= _missingTolerance)
            {
                var zip = new ZipFile { CompressionLevel = CompressionLevel.BestCompression };
                zip.AddFiles(_serialListDone.Values.Select(file => file.FullName), "");
                resultZip = DirectoryUtil.GetLocalMediaDir(DirectoryUtil.NomadLogicMediaDirectoriesEnum.camera_ring) +
                            GenerateUniqueId.GetId() + ".zip";
                zip.Save(resultZip);
            }
        
            // Une fois tout terminé, on invoke le callback donnée en paramètre.
            _callback?.Invoke(resultZip, token.IsCancellationRequested, _serialListLeft.Keys);
        }

        internal void NewFileDetected(string filePath, string serial)
        {
            if (_finished)
            {
                log.Debug("File discarted because file check was finished: Serial={0}, File={1}", serial, filePath);
                return;
            }

            if (!File.Exists(filePath))
            {
                log.Warn("File path does not exists for serial={1}: {0}", filePath, serial);
                return;
            }

            if (string.IsNullOrWhiteSpace(serial))
            {
                log.Warn("Picture taken with no valid serial. File will be deleted: {0}", filePath);
                try
                {
                    File.Delete(filePath);
                }
                catch (Exception e)
                {
                    log.Error(e, "Could not delete unneeded picture file: {0}", filePath);
                }
                return;
            }

            // Cas de duplication de fichier venant de la même caméra.
            if (_serialListDone.ContainsKey(serial))
            {
                log.Warn("Picture already taken with same camera serial detected. Will be discarted. Serial={0}, File={1}", serial, filePath);
                try
                {
                    File.Delete(filePath);
                }
                catch (Exception e)
                {
                    log.Error(e, "Could not delete unneeded picture file: {0}", filePath);
                }
                return;
            }

            lock (_serialListLeft)
            {
                if (_serialListLeft.ContainsKey(serial))
                {
                    _serialListLeft.Remove(serial);
                    _serialListDone.Add(serial, new FileInfo(filePath));

                    // Si on vient de capturer la dernière image, on termine.
                    if (!_serialListLeft.Any())
                    {
                        _finished = true;
                    }
                }
                else
                {
                    // Cas d'erreur
                    log.Warn("Picture taken with no waiting serial detected. Will discard. Serial={0}, File={1}", serial,
                        filePath);
                }
            }
        }
    }
}