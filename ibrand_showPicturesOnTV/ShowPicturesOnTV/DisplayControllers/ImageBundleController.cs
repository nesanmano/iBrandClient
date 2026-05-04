using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using NLog;
using NomadLogic.Common.Util;

namespace NomadLogic.ShowPicturesOnTV.DisplayControllers
{
    /// <summary>
    /// Contrôleur pour afficher des groupes d'images.
    /// Comme le contrôleur de base est axé sur un seul fichier à la fois, il faut surcharger la méthode
    /// de recherche pour non seulement trouver un répertoire à afficher, mais aussi quel fichier à l'intérieur il faut
    /// afficher. Cela implique aussi plusieures autre méthodes de recherches axées sur les répertoires.
    /// Il est également possible d'afficher les groupes d'images un nombre fixe de boucles, et aussi l'option d'avoir ces boucles faire
    /// un aller-retour dans leur index (start : min -> max -> min : end).
    /// </summary>
    class ImageBundleController : ImageController
    {
        private static readonly Logger log = LogManager.GetCurrentClassLogger();
        private int _nbLoops;
        private int _indexAt;
        private List<DirectoryInfo> _directoriesToShow;
        private bool _rewinding;
        
        protected override DisplayItemInfo GetItemToDisplay()
        {
            DisplayItemInfo itemToDisplay = null;
            if (_filesToShow.Any())
            {
                if (_rewinding)
                {
                    _indexAt--;
                    if (_indexAt < 1)
                    {
                        _indexAt = 0;
                        _nbLoops++;
                        _rewinding = false;
                    }
                }
                else
                {
                    _indexAt++;
                    if (_indexAt >= _filesToShow.Count)
                    {
                        if (DisplayConfiguration.RewindLoop)
                        {
                            //on reviens à ce qu'on avait avant et on enlève un
                            _indexAt -= 2;
                            _rewinding = true;
                        }
                        else
                        {
                            _indexAt = 0;
                            _nbLoops++;
                        }
                    }
                }


                if (_indexAt < 0)
                {
                    _indexAt = 0;
                }

                if (_nbLoops < DisplayConfiguration.BundleLoop)
                {
                    var file = _filesToShow.ElementAt(_indexAt).FullName;
                    log.Trace("Displaying following image: {0}", file);
                    try
                    {
                        itemToDisplay = new DisplayItemInfo
                        {
                            DisplayType = DisplayItemTypeEnum.Image,
                            Path = file,
                            Size = _elementSize,
                            Location = _elementLocation
                        };
                    }
                    catch (Exception e)
                    {
                        RemoveDirectory(file, e);
                        itemToDisplay = null;
                    }
                }
            }
            
            if (itemToDisplay != null) return itemToDisplay;

            // Si null, on a complété le bundle et on cherche un autre élément.
            
            // On teste avant tout l'élément d'intermission.
            if (DisplayConfiguration.IntermissionFile != null)
            {
                try
                {
                    if (DisplayConfiguration.IntermissionNumber < 1)
                    {
                        log.Warn("Intermission number was invalid. Changed to 5 from {0}", DisplayConfiguration.IntermissionNumber);
                        DisplayConfiguration.IntermissionNumber = 5;
                    }

                    if (_intermissionTimer >= DisplayConfiguration.IntermissionNumber)
                    {
                        itemToDisplay = CreateDisplayItemInfo(DisplayConfiguration.IntermissionFile, _intermissionSize, _intermissionLocation, true);
                        _intermissionTimer = 0;
                        return itemToDisplay;
                    }
                }
                catch (Exception e)
                {
                    log.Error(e, "Could not instanciate intermission file: {0}", DisplayConfiguration.IntermissionFile);
                    DisplayConfiguration.IntermissionFile = null;
                    return null;
                }
            }

            // il faut loader un nouveau ImageBundle
            log.Debug("Preparing to load bundle of images.");
            _filesToShow.Clear();
            var dir = new DirectoryInfo(MainFolder);

            var imageBundles = GetDirectoriesOrderByDate(dir, DisplayConfiguration.Delay);

            var directoryInfos = imageBundles as DirectoryInfo[] ?? imageBundles.ToArray();
            if (directoryInfos.Length >= 2)
            {
                
                var imageBundle = directoryInfos.First();
                if (imageBundle.FullName.Equals(ShownFolder))
                {
                    imageBundle = directoryInfos.ElementAt(1);
                }

                log.Debug("New bundle detected.");
                LoadPictureBundle(imageBundle);

                if (_filesToShow.Any())
                {
                    var file = _filesToShow.ElementAt(_indexAt).FullName;
                    try
                    {
                        itemToDisplay = new DisplayItemInfo
                        {
                            Path = file,
                            DisplayType = DisplayItemTypeEnum.Image,
                            Size = _elementSize,
                            Location = _elementLocation
                        };
                    }
                    catch (Exception e)
                    {
                        RemoveDirectory(file, e);
                        return null;
                    }
                }

                if (DisplayConfiguration.StandbyMethod != ConfigurationShowPictureOnTvStandbyEnum.Randomize ||
                    DisplayConfiguration.ResetList != ConfigurationShowPictureOnTvResetEnum.NewAdded) return itemToDisplay;

                if (_directoriesToShow == null)
                {
                    _directoriesToShow = new List<DirectoryInfo>();
                }

                _directoriesToShow.Add(imageBundle);
            }
            // On a pas de nouveau bundle
            else
            {
                DirectoryInfo dirToDisplay = null;
                switch (DisplayConfiguration.StandbyMethod)
                {
                    case ConfigurationShowPictureOnTvStandbyEnum.Orderedlist:
                        dirToDisplay = GetNextOrderedDir();
                        break;
                    case ConfigurationShowPictureOnTvStandbyEnum.Randomize:
                        dirToDisplay = GetNextRandomDir();
                        break;
                }

                if (dirToDisplay == null) return null;

                LoadPictureBundle(dirToDisplay);

                if (!_filesToShow.Any()) return null;

                var file = _filesToShow.ElementAt(_indexAt).FullName;
                
                try
                {
                    itemToDisplay = new DisplayItemInfo
                    {
                        Path = file,
                        DisplayType = DisplayItemTypeEnum.Image,
                        Size = _elementSize,
                        Location = _elementLocation
                    };
                    log.Trace("Displaying following image: {0}", file);
                }
                catch (Exception e)
                {
                    RemoveDirectory(file, e);
                    itemToDisplay = null;
                }
            }

            return itemToDisplay;
        }

        private void RemoveDirectory(string file, Exception err)
        {
            log.Error(err, "Could not load image named '{0}'. Removing whole directory named {1}", StringUtil.GetFileName(file), StringUtil.GetDirName(file));
            _filesToShow.Clear();
            var fi = new FileInfo(file);
            if (fi.DirectoryName == null) return;

            var lastDir = new DirectoryInfo(fi.DirectoryName);
            var errDir = DirectoryUtil.GetLocalApplicationDir() + "error" + Path.DirectorySeparatorChar + lastDir.Name + Path.DirectorySeparatorChar;
            if (Directory.Exists(errDir)) Directory.Delete(errDir); 
            lastDir.MoveTo(errDir);
            
        }

        private DirectoryInfo GetNextOrderedDir()
        {
            var showDir = new DirectoryInfo(ShownFolder);
            IEnumerable<DirectoryInfo> files = null;
            switch (DisplayConfiguration.OrderParam)
            {
                case ConfigurationShowPictureOnTvOrderParamEnum.Date:
                    switch (DisplayConfiguration.DateRestriction)
                    {
                        case ConfigurationShowPictureOnTvDateRestrictionEnum.All:
                            files = GetDirectoriesOrderByDate(showDir, 0);
                            break;
                        case ConfigurationShowPictureOnTvDateRestrictionEnum.Today:
                            files = GetDirectoriesTodayOrderByDate(showDir);
                            break;
                    }
                    break;
                case ConfigurationShowPictureOnTvOrderParamEnum.Name:
                    switch (DisplayConfiguration.DateRestriction)
                    {
                        case ConfigurationShowPictureOnTvDateRestrictionEnum.All:
                            files = GetDirectoriesOrderByName(showDir, 0);
                            break;
                        case ConfigurationShowPictureOnTvDateRestrictionEnum.Today:
                            files = GetDirectoriesTodayOrderByName(showDir);
                            break;
                    }
                    break;
            }

            if (files == null) return null;

            var directoryInfos = files as DirectoryInfo[] ?? files.ToArray();
            if (_itemShownIndex >= directoryInfos.Length)
            {
                _itemShownIndex = 0;
            }
            
            var folderToDisplay = new DirectoryInfo(ShownFolder + Path.DirectorySeparatorChar + directoryInfos.ElementAt(_itemShownIndex));
            _itemShownIndex++;

            log.Debug("Showing directory: {0}", folderToDisplay);
            return folderToDisplay;
        }

        private DirectoryInfo GetNextRandomDir()
        {
            var showDir = new DirectoryInfo(ShownFolder);

            if (_directoriesToShow == null || _directoriesToShow.Count == 0 ||
                (DisplayConfiguration.ResetList == ConfigurationShowPictureOnTvResetEnum.AllShown && _directoriesToShow.Count <= _nbFilesToShowWhenFull / 2))
            {
                switch (DisplayConfiguration.DateRestriction)
                {
                    case ConfigurationShowPictureOnTvDateRestrictionEnum.All:
                        _directoriesToShow = new List<DirectoryInfo>(GetDirectoriesOrderByDate(showDir, 0));
                        break;
                    case ConfigurationShowPictureOnTvDateRestrictionEnum.Today:
                        _directoriesToShow = new List<DirectoryInfo>(GetDirectoriesTodayOrderByDate(showDir));
                        break;
                }

                if (_directoriesToShow != null)
                    _nbFilesToShowWhenFull = _directoriesToShow.Count;
            }

            if (_directoriesToShow == null || _directoriesToShow.Count <= 0) return null;

            var index = _random.Next(_directoriesToShow.Count);
            var fileToDisplay = new DirectoryInfo(ShownFolder + Path.DirectorySeparatorChar + _directoriesToShow.ElementAt(index));
            _directoriesToShow.RemoveAt(index);

            log.Debug("Showing directory: {0}", fileToDisplay);
            return fileToDisplay;
        }

        private void LoadPictureBundle(DirectoryInfo dir)
        {
            var shownImageBundle = new DirectoryInfo(ShownFolder + Path.DirectorySeparatorChar + dir.Name);

            if (dir.Parent != null && !dir.Parent.FullName.Equals(ShownFolder))
            {
                if (!shownImageBundle.Exists)
                {
                    Directory.CreateDirectory(shownImageBundle.FullName);
                }

                var files = dir.GetFiles();

                foreach (var t in files)
                {
                    if (File.Exists(shownImageBundle.FullName + Path.DirectorySeparatorChar + t.Name))
                    {
                        File.Delete(shownImageBundle.FullName + Path.DirectorySeparatorChar + t.Name);
                    }
                    File.Move(t.FullName, shownImageBundle.FullName + Path.DirectorySeparatorChar + t.Name);
                }

                FileUtil.DeleteFolderContent(dir.FullName);

                log.Debug("Finished moving new directory to shown: {0}", shownImageBundle.Name);
            }

            _filesToShow.Clear();
            _filesToShow.AddRange(FileUtil.GetFilesByExtensionsOrderByName(shownImageBundle, GetFileExtensions()));
            _indexAt = 0;
            _nbLoops = 0;
            log.Debug("Loaded {0} files to show in directory '{1}'", _filesToShow.Count, shownImageBundle.Name);
        }

        private static IEnumerable<DirectoryInfo> GetDirectoriesOrderByDate(DirectoryInfo dir, int delay)
        {
            var directories = dir.EnumerateDirectories();
            return directories.Where(f => DateTime.Now.Subtract(f.CreationTime).TotalMilliseconds > delay).OrderBy(f => f.CreationTime);
        }

        private static IEnumerable<DirectoryInfo> GetDirectoriesOrderByName(DirectoryInfo dir, int delay)
        {
            var directories = dir.EnumerateDirectories();
            return directories.Where(f => DateTime.Now.Subtract(f.CreationTime).TotalMilliseconds > delay).OrderBy(f => f.Name);
        }

        private static IEnumerable<DirectoryInfo> GetDirectoriesTodayOrderByDate(DirectoryInfo dir)
        {
            var directories = dir.EnumerateDirectories();
            return directories.Where(f => DateTime.Now.DayOfYear == f.CreationTime.DayOfYear).OrderBy(f => f.CreationTime);
        }

        private static IEnumerable<DirectoryInfo> GetDirectoriesTodayOrderByName(DirectoryInfo dir)
        {
            var directories = dir.EnumerateDirectories();
            return directories.Where(f => DateTime.Now.DayOfYear == f.CreationTime.DayOfYear).OrderBy(f => f.Name);
        }
    }
}
