using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using JetBrains.Annotations;
using NLog;
using NomadLogic.Common.Util;

// ReSharper disable InconsistentlySynchronizedField

namespace NomadLogic.iBrandDesktop.Ui.Choice
{
    internal class CameraRingCanonMultipleShotTextBox : CameraRingCanonTextBox
    {
        private static readonly Logger log = LogManager.GetCurrentClassLogger();
        [NotNull]
        private readonly Dictionary<string, List<string>> _filesSaved = new Dictionary<string, List<string>>();

        /// <summary>
        /// Indique combien de caméras ont envoyé des photos.
        /// </summary>
        public override int CurrentCameraCount => _filesSaved.Count;

        /// <summary>
        /// Indique combien de séquences complètes ont été saisies par les caméras.
        /// Cette propriété à une signification différente que celle de la classe parent.
        /// </summary>
        public override int CurrentPicturesCount => _filesSaved.Values.Select(vals => vals.Count).Concat(new[] { int.MaxValue }).Min();

        protected override bool OnFileReception(string fileName)
        {
            lock (_lock)
            {
                try
                {
                    CreateFileBundleDirectory();
                    
                    var info = StringUtil.ExtractUniqueIdsFromFileName(fileName);

                    if (info.Length < 2)
                    {
                        File.Delete(fileName);
                        return false;
                    }

                    // Première fois que l'on lit un fichier de l'auteur.
                    if (!_filesSaved.ContainsKey(info[0]))
                    {
                        _filesSaved.Add(info[0], new List<string>());
                    }

                    if (!DoCheckAuthorUnicity(info, fileName)) return false;

                    var nbrFileForCam = _filesSaved[info[0]].Count;

                    var finalFile = DoCreateNewValidImageFileFromTemporaryFile(info, fileName, FormatIntToFixedSize(nbrFileForCam));
                    
                    // Si la liste des fichiers associé à la caméra contient déjà le fichier.
                    if (_filesSaved[info[0]].Contains(finalFile))
                    {
                        // Fichier déjà existant.
                        return true;
                    }

                    // On ajoute le fichier final à la caméra.
                    _filesSaved[info[0]].Add(finalFile);

                    // Calcul du plus petit nombre de fichier par caméra recensée.
                    var lowestNbrPictureCamera = CurrentPicturesCount;
                    SetNbrPicture(Convert.ToString(lowestNbrPictureCamera));
                    if (lowestNbrPictureCamera < int.MaxValue)
                    {
                        EnablePreviewButton(true);
                        SetNbrPicture(lowestNbrPictureCamera.ToString());
                    }
                    else
                    {
                        EnablePreviewButton(false);
                    }
                    
                }
                catch (System.Exception ex)
                {
                    var tmp = Resources.iBrand.UI_CameraRingChoice_ErrorDuringFileAnalisys + fileName;
                    log.Error(ex, tmp);
                    if (File.Exists(fileName)) File.Delete(fileName);
                    UpdateStatusLabel(tmp, CameraRingStatus.Error);
                    return false;
                }
            }

            return true;
        }
        
        protected override void ClearAllPictureFiles()
        {
            base.ClearAllPictureFiles();
            lock (_lock)
            {
                _filesSaved.Clear();
                SetNbrPicture("0");
            }
        }
    }
}