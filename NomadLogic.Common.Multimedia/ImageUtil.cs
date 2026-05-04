using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Imaging;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using JetBrains.Annotations;
using NLog;
using NomadLogic.Common.Extentions;
using NomadLogic.Common.Util;
using NomadLogic.Common.Util.Math;
using NomadLogic.Multimedia.Gif;

// ReSharper disable InconsistentNaming

// ReSharper disable PossibleLossOfFraction
// ReSharper disable CompareOfFloatsByEqualityOperator

namespace NomadLogic.Multimedia
{
    /// <summary>
    /// Classe qui regroupe diverses méthodes pour effectuer des opérations sur des images ou fichiers d'images.
    /// </summary>
    [PublicAPI]
    public static class ImageUtil
    {
        private static readonly Logger log = LogManager.GetCurrentClassLogger();
        
        /// <summary>
        /// Brush par défaut. Voir la page suivante pour plus d'exemples: http://msdn.microsoft.com/en-us/library/aa983677%28v=vs.71%29.aspx
        /// </summary>
        public static readonly Brush DefaultBrush = new SolidBrush(Color.Black);

        /// <summary>
        /// Détermine si le fichier est une image valide ou on en tentant d'en faire un Bitmap à l'interne.
        /// teste plus de format de fichiers (avec des libraries externes) que la version de FileUtil.
        /// </summary>
        /// <param name="file">Le fichier à analyser.</param>
        /// <returns>TRUE si et seulement si on est capable de construire un Bitmap avec le fichier à analyser.</returns>
        [Obsolete]
        public static bool IsImageFile(string file)
        {
            var isImage = false;

            if (!File.Exists(file)) { return false; }

            try
            {
                using (FileUtil.LoadBitmap(file)) { } // Ferme automatiquement le fichier
                isImage = true;
            }
            catch (Exception)
            {
                /* Die Nice */
            }

            return isImage;
        }
        
        /// <summary>
        /// Créer un GIF avec une série de fichiers. On peut donner ici les paramètres de GIF que l'on veut.
        /// </summary>
        /// <param name="basePictures">La séquence d'image avec laquelle créer le GIF.</param>
        /// <param name="size">La taille du GIF (il s'agit d'une carré, donc, une seule dimension suffit.)</param>
        /// <param name="repeat">Indique si on veut que le GIF se répète.</param>
        /// <param name="quality">Sa qualité. 1 est le minimum et indique une qualité supérieure.</param>
        /// <param name="delayMili">Le délai entre chaque frame, en milisecondes.</param>
        /// <param name="outputFileName">Le fichier dans lequel on veut écrire. S'il existe, il sera écrasé.</param>
        /// <param name="overlay">Un fichier de overlay, optionnel, par dessus chaque trame.</param>
        /// <returns>TRUE si tout s'est bien déroulé.</returns>
        [Obsolete]
        public static bool CreateGif(List<string> basePictures, int size, bool repeat, int quality, int delayMili, string outputFileName, string overlay)
        {
            Bitmap overlayBitmap = null;
            try
            {
                if (File.Exists(outputFileName)) { File.Delete(outputFileName); }

                if (overlay != null) { overlayBitmap = FileUtil.LoadBitmap(overlay); }

                using (var a = new AnimatedGifEncoder())
                {
                    a.Start(outputFileName);
                    a.SetDelay(delayMili);
                    a.SetRepeat(repeat
                                    ? 0
                                    : -1);
                    a.SetQuality(quality < 1
                                     ? 1
                                     : quality);

                    foreach (var fileName in basePictures)
                    {
                        using (var image = FileUtil.LoadBitmap(fileName))
                        {
                            using (var b = image.Frame(size, size))
                            {
                                if (overlayBitmap != null)
                                    b.AddOverlay(overlayBitmap);

                                a.AddFrame(b);
                            }
                        }
                    }
                    a.Finish();
                }

                return true;
            }
            catch (Exception e)
            {
                log.Error(e, "Error during creation of the GIF file.");
                if (File.Exists(outputFileName)) { File.Delete(outputFileName); }
                return false;
            }
            finally { overlayBitmap?.Dispose(); }
        }
        
        [Obsolete]
        public static bool CreateGif(List<FileInfo> basePictures, Size size, bool repeat, int quality, int delayMili, string outputFileName, string overlay)
        {
            Bitmap overlayBitmap = null;
            try
            {
                if (File.Exists(outputFileName)) { File.Delete(outputFileName); }

                if (overlay != null) { overlayBitmap = FileUtil.LoadBitmap(overlay); }

                using (var a = new AnimatedGifEncoder())
                {
                    a.Start(outputFileName);
                    a.SetDelay(delayMili);
                    a.SetRepeat(repeat
                                    ? 0
                                    : -1);
                    a.SetQuality(quality < 1
                                     ? 1
                                     : quality);

                    foreach (var fileName in basePictures)
                    {
                        using (var image = FileUtil.LoadBitmap(fileName.FullName))
                        {
                            using (var b = image.Resize(size.Width, size.Height))
                            {
                                if (overlayBitmap != null)
                                    b.AddOverlay(overlayBitmap);
                                a.AddFrame(b);
                            }
                        }
                    }
                    a.Finish();
                }

                return true;
            }
            catch (Exception e)
            {
                log.Error(e, "Error during creation of the GIF file.");
                if (File.Exists(outputFileName)) { File.Delete(outputFileName); }
                return false;
            }
            finally { overlayBitmap?.Dispose(); }
        }
        
        [Obsolete]
        public static bool CreateGif(List<FileInfo> basePictures, bool repeat, int quality, int delayMili, string outputFileName, string overlay)
        {
            Bitmap overlayBitmap = null;
            try
            {
                if (File.Exists(outputFileName)) { File.Delete(outputFileName); }

                if (overlay != null) { overlayBitmap = FileUtil.LoadBitmap(overlay); }

                using (var a = new AnimatedGifEncoder())
                {
                    a.Start(outputFileName);
                    a.SetDelay(delayMili);
                    a.SetRepeat(repeat
                                    ? 0
                                    : -1);
                    a.SetQuality(quality < 1
                                     ? 1
                                     : quality);

                    foreach (var fileName in basePictures)
                    {
                        using (var image = FileUtil.LoadBitmap(fileName.FullName))
                        {

                            if (overlayBitmap != null)
                                image.AddOverlay(overlayBitmap);
                            a.AddFrame(image);

                        }
                    }
                    a.Finish();
                }

                return true;
            }
            catch (Exception e)
            {
                log.Error(e, "Error during creation of the GIF file.");
                if (File.Exists(outputFileName)) { File.Delete(outputFileName); }
                return false;
            }
            finally { overlayBitmap?.Dispose(); }
        }
        
        [Obsolete]
        public static bool CreateCroppedGif(List<FileInfo> basePictures, int size, bool repeat, int quality, int delayMili, string outputFileName, string overlay)
        {
            Bitmap overlayBitmap = null;
            try
            {
                if (File.Exists(outputFileName)) { File.Delete(outputFileName); }

                if (overlay != null) { overlayBitmap = FileUtil.LoadBitmap(overlay); }

                using (var a = new AnimatedGifEncoder())
                {
                    a.Start(outputFileName);
                    a.SetDelay(delayMili);
                    a.SetRepeat(repeat
                                    ? 0
                                    : -1);
                    a.SetQuality(quality < 1
                                     ? 1
                                     : quality);

                    foreach (var fileName in basePictures)
                    {
                        using (var image = FileUtil.LoadBitmap(fileName.FullName))
                        {
                            using (var b = image.Frame(size, size))
                            {
                                if (overlayBitmap != null)
                                    b.AddOverlay(overlayBitmap);
                                a.AddFrame(b);
                            }
                        }
                    }
                    a.Finish();
                }

                return true;
            }
            catch (Exception e)
            {
                log.Error(e, "Error during creation of the GIF file.");
                if (File.Exists(outputFileName)) { File.Delete(outputFileName); }
                return false;
            }
            finally { overlayBitmap?.Dispose(); }
        }

        /// <summary>
        /// Créer un GIF avec une série de fichiers. On peut donner ici les paramètres de GIF que l'on veut.
        /// </summary>
        /// <param name="basePictures">La séquence d'image avec laquelle créer le GIF.</param>
        /// <param name="size">La taille du GIF . Les dimensions peuvent être différentes.</param>
        /// <param name="repeat">Indique si on veut que le GIF se répète.</param>
        /// <param name="quality">Sa qualité. 1 est le minimum et indique une qualité supérieure.</param>
        /// <param name="delayMili">Le délai entre chaque frame, en milisecondes.</param>
        /// <param name="outputFileName">Le fichier dans lequel on veut écrire. S'il existe, il sera écrasé.</param>
        /// <returns>TRUE si tout s'est bien déroulé.</returns>
        [Obsolete]
        public static bool CreateGif(string[] basePictures, Size size, bool repeat, int quality, int delayMili, string outputFileName)
        {
            try
            {
                if (File.Exists(outputFileName)) { File.Delete(outputFileName); }

                using (var a = new AnimatedGifEncoder())
                {
                    a.Start(outputFileName);
                    a.SetDelay(delayMili);
                    a.SetRepeat(repeat
                                    ? 0
                                    : -1);
                    a.SetQuality(quality < 1
                                     ? 1
                                     : quality);

                    foreach (var fileName in basePictures)
                    {
                        using (var image = FileUtil.LoadBitmap(fileName))
                        {
                            using (var b = image.Resize(size.Width, size.Height)) { a.AddFrame(b); }
                        }
                    }
                    a.Finish();
                }

                return true;
            }
            catch (Exception e)
            {
                log.Error(e, "Error during creation of the GIF file.");
                if (File.Exists(outputFileName)) { File.Delete(outputFileName); }
                return false;
            }
        }

        /// <summary>
        /// Créer un fichier Gif. Beaucoups d'options sont disponibles selon les paramètres.
        /// </summary>
        /// <param name="basePictures">La liste des images à insérer dans le Gif.</param>
        /// <param name="size">La taille finale du Gif. Les images sont redimensionnées, puis insérées. Si null, on utilise la taille de la première image.
        /// Si l'option de "crop" est fausse, et que le taille spécifiée ici n'est pas le même ratio que l'original, il y aura un effet de distortion.</param>
        /// <param name="repeat">Indique si le GIF doit se répéter à l'infini. Vrai par défaut.</param>
        /// <param name="rewind">Si à vrai (faux par défaut), on affiche les images en ordre inverse vers le début avant de terminer l'ajout de trames. Par exemple,
        /// avec 5 trames, les trames seront insérées dans cet ordre: 1-2-3-4-5-4-3-2-1. Si l'option de répétition est active, on n'ajoute pas en dernier la première trame
        /// pour faire une boucle parfaite d'affichage d'une trame sans répétition directe. Par exemple, avec trois trames et répétition ainsi que rembobinage à vrai,
        /// cela donne [1-2-3-2]-[1-2-3-2]-[1-2-3-2] à l'infini. (Les exemples ne prennent pas compte du fait que les indices sont basé sur zéro).</param>
        /// <param name="quality">La qualité voulue du Gif. 1 est la meilleure qualité, et la valeur par défaut.</param>
        /// <param name="delayMili">Le délai en milisecondes entre chaque image. Par défaut, 300.</param>
        /// <param name="outputFileName">Le fichier sortant. Sera écrasé si déjà existant.</param>
        /// <param name="overlay">Si on doit utiliser une image de superposition, cela indique le fichier à utiliser.</param>
        /// <param name="crop">Indique si on doit élaguer les côtés de l'image lors d'un resize.</param>
        /// <returns>TRUE si le fichier GIF est créé sans encombres.</returns>
        public static bool CreateGif(IList<FileInfo> basePictures, string outputFileName,
            bool repeat = true, bool rewind = false, int quality = 1, int delayMili = 300, string overlay = null, 
            Size? size = null, bool crop = true)
        {
            var stringList = basePictures.Select(f => f.FullName).ToList();
            return CreateGif(stringList, outputFileName, repeat, rewind, quality, delayMili, overlay, size);
        }

        /// <summary>
        /// Créer un fichier Gif. Beaucoups d'options sont disponibles selon les paramètres.
        /// </summary>
        /// <param name="basePictures">La liste des images à insérer dans le Gif.</param>
        /// <param name="size">La taille finale du Gif. Les images sont redimensionnées, puis insérées. Si null, on utilise la taille de la première image.
        /// Si l'option de "crop" est fausse, et que le taille spécifiée ici n'est pas le même ratio que l'original, il y aura un effet de distortion.</param>
        /// <param name="repeat">Indique si le GIF doit se répéter à l'infini. Vrai par défaut.</param>
        /// <param name="rewind">Si à vrai (faux par défaut), on affiche les images en ordre inverse vers le début avant de terminer l'ajout de trames. Par exemple,
        /// avec 5 trames, les trames seront insérées dans cet ordre: 1-2-3-4-5-4-3-2-1. Si l'option de répétition est active, on n'ajoute pas en dernier la première trame
        /// pour faire une boucle parfaite d'affichage d'une trame sans répétition directe. Par exemple, avec trois trames et répétition ainsi que rembobinage à vrai,
        /// cela donne [1-2-3-2]-[1-2-3-2]-[1-2-3-2] à l'infini. (Les exemples ne prennent pas compte du fait que les indices sont basé sur zéro).</param>
        /// <param name="quality">La qualité voulue du Gif. 1 est la meilleure qualité, et la valeur par défaut.</param>
        /// <param name="delayMili">Le délai en milisecondes entre chaque image. Par défaut, 300.</param>
        /// <param name="outputFileName">Le fichier sortant. Sera écrasé si déjà existant.</param>
        /// <param name="overlay">Si on doit utiliser une image de superposition, cela indique le fichier à utiliser.</param>
        /// <param name="crop">Indique si on doit élaguer les côtés de l'image lors d'un resize.</param>
        /// <returns>TRUE si le fichier GIF est créé sans encombres.</returns>
        public static bool CreateGif(IList<string> basePictures, string outputFileName,
            bool repeat = true, bool rewind = false, int quality = 1, int delayMili = 300, string overlay = null, 
            Size? size = null, bool crop = true)
        {
            if (basePictures == null || basePictures.Count < 1) return false;

            Bitmap overlayBitmap = null;
            try
            {
                if (File.Exists(outputFileName)) { File.Delete(outputFileName); }

                if (overlay != null) { overlayBitmap = FileUtil.LoadBitmap(overlay); }

                // Calcul de la liste d'indices à traiter, et de leur ordre, selon les paramètres.
                var listeIndice = Enumerable.Range(0, basePictures.Count).ToList();
                if (rewind)
                {
                    var minVal = repeat
                                     ? 1
                                     : 0;
                    for (var i = basePictures.Count - 1; i >= minVal; i--) { listeIndice.Add(i); }
                }

                var w = size?.Width ?? -1;
                var h = size?.Height ?? -1;

                using (var a = new AnimatedGifEncoder())
                {
                    a.Start(outputFileName);
                    a.SetDelay(delayMili);
                    a.SetRepeat(repeat
                                    ? 0
                                    : -1);
                    a.SetQuality(quality < 1
                                     ? 1
                                     : quality);

                    foreach (var i in listeIndice)
                    {
                        using (var image = FileUtil.LoadBitmap(basePictures[i]))
                        {
                            if (w < 0)
                            {
                                w = image.Width;
                                h = image.Height;
                            }

                            using (var b = !crop
                                               ? image.Resize(w, h)
                                               : image.Frame(w, h))
                            {
                                if (overlayBitmap != null)
                                    b.AddOverlay(overlayBitmap);
                                a.AddFrame(b);
                            }
                        }
                    }
                    a.Finish();
                }

                return File.Exists(outputFileName);
            }
            catch (Exception e)
            {
                log.Error(e, "Error during creation of the GIF file.");
                if (File.Exists(outputFileName)) { File.Delete(outputFileName); }
                return false;
            }
            finally { overlayBitmap?.Dispose(); }
        }

        /// <summary>
        /// Créer un fichier GIF avec le maximum de fonctionalités possibles.
        /// On défini un canevas, puis on dessine trois choses dans ce canevas pour chaque frame, dans cet ordre: une couleur de fond, l'image de base sélectionnée, puis un overlay.
        /// À moins d'avis contraire, on dessine les images et overlay sur toute la surface, mais on peut les faire commencer à un point précis.
        /// </summary>
        /// <param name="basePictures">Doit contenir au moins deux images (sinon faire un gif est ridicule). Ce sont les images de base avec lesquelles travailler.</param>
        /// <param name="outputFileName">Nom de fichier final du fichier GIF. Si existant, le fichier sera écrasé.</param>
        /// <param name="repeat">Indique qu'il faut que le GIF roule de façon infini. VRAI par défaut.</param>
        /// <param name="rewind">Si à vrai (faux par défaut), on affiche les images en ordre inverse vers le début avant de terminer l'ajout de trames. Par exemple,
        /// avec 5 trames, les trames seront insérées dans cet ordre: 1-2-3-4-5-4-3-2-1. Si l'option de répétition est active, on n'ajoute pas en dernier la première trame
        /// pour faire une boucle parfaite d'affichage d'une trame sans répétition directe. Par exemple, avec trois trames et répétition ainsi que rembobinage à vrai,
        /// cela donne [1-2-3-2]-[1-2-3-2]-[1-2-3-2] à l'infini. (Les exemples ne prennent pas compte du fait que les indices sont basé sur zéro).</param>
        /// <param name="quality">La qualité voulue du Gif. 1 est la meilleure qualité, et la valeur par défaut.</param>
        /// <param name="delayMili">Le délai en milisecondes entre chaque image. Par défaut, 300.</param>
        /// <param name="overlays">Liste de fichiers d'overlay. La façon que les overlay sont utilisé est la suivante: pour chaque frame, si la liste d'overlay contient un indice associé
        /// (ces indices commencent à zéro), on va chercher ce fichier et on l'utilise comme overlay. Sinon, on prend le dernier indice valide. En gros, si la liste d'overlay est plus petite que
        /// le nombre de trame utilisé (voir l'option Repeat et la liste d'image de base), on répète la dernière image de la liste d'overlay. Par exemple, si un seul fichier d'overlay, on
        /// utilise la même image pour toutes les trames. Si le nombre de trame est 6, et que l'on veut mettre un overlay juste sur les trois premières, on met quatre fichiers, et le dernier sera une
        /// image totalement transparente. </param>
        /// <param name="size">La taille finale des images de base dans la composition du Gif. Les images sont redimensionnées, puis insérées. Si null, on utilise la taille de la première image.
        /// Si l'option de "crop" est fausse, et que le taille spécifiée ici n'est pas le même ratio que l'original, il y aura un effet de distortion.</param>
        /// <param name="crop">Indique si on doit élaguer les côtés de l'image de base lors d'un resize.</param>
        /// <param name="canvasSize">Taille finale du GIF, et de chaque canevas de trame. Si null, on prend la taille de la première image.</param>
        /// <param name="imagePos">Position de dessein de l'image pour chaque trame. Si null, prend la position (0,0), qui est en haut à gauche.</param>
        /// <param name="overlayPos">Position de dessein de l'overlay pour chaque trame. Si null, prend la position (0,0), qui est en haut à gauche.</param>
        /// <returns>TRUE si le fichier a été créé correctement.</returns>
        public static bool CreateGif(IList<string> basePictures, string outputFileName,
            bool repeat = true, bool rewind = false, int quality = 1, int delayMili = 300, 
            IList<string> overlays = null, Size? size = null, bool crop = true, Size? canvasSize = null,
            Point? imagePos = null, Point? overlayPos = null)
        {
            if (basePictures == null || basePictures.Count < 1) return false;

            try
            {
                if (File.Exists(outputFileName)) { File.Delete(outputFileName); }
                
                // Calcul de la liste d'indices à traiter, et de leur ordre, selon les paramètres.
                var listeIndice = Enumerable.Range(0, basePictures.Count).ToList();
                if (rewind)
                {
                    var minVal = repeat
                        ? 1
                        : 0;
                    for (var i = basePictures.Count - 1; i >= minVal; i--) { listeIndice.Add(i); }
                }

                // Calcule le nombre de trames à traiter.
                var nbrFrames = Math.Max(listeIndice.Count, overlays?.Count ?? 0);

                var w = size?.Width ?? -1;
                var h = size?.Height ?? -1;

                var canvasX = canvasSize?.Width ?? w;
                var canvasY = canvasSize?.Height ?? h;

                using (var a = new AnimatedGifEncoder())
                {
                    a.Start(outputFileName);
                    a.SetDelay(delayMili);
                    a.SetRepeat(repeat
                        ? 0
                        : -1);
                    a.SetQuality(quality < 1
                        ? 1
                        : quality);

                    for(var i = 0; i < nbrFrames; i++)
                    {
                        // Étape 1 - Dessein de l'image
                        using (var image = FileUtil.LoadBitmap(basePictures[listeIndice[i]]))
                        {
                            if (w < 0)
                            {
                                w = image.Width;
                                h = image.Height;
                            }

                            if (canvasX < 0)
                            {
                                canvasX = image.Width;
                                canvasY = image.Height;
                            }

                            using (var canvas = new Bitmap(canvasX, canvasY, PixelFormat.Format24bppRgb))
                            {
                                using (var b = !crop
                                    ? image.Resize(w, h)
                                    : image.Frame(w, h))
                                {
                                    var posXimage = imagePos?.X ?? 0;
                                    var posYimage = imagePos?.Y ?? 0;

                                    canvas.AddOverlay(b, posXimage, posYimage);
                                }

                                // Étape 2 - Ajout de l'overlay
                                if (overlays != null)
                                {
                                    var overlayIndex = Math.Min(i, overlays.Count - 1);
                                    if (overlayIndex >= 0)
                                    {
                                        using (var overlayBitmap = FileUtil.LoadBitmap(overlays[overlayIndex]))
                                        {
                                            var posXoverlay = overlayPos?.X ?? 0;
                                            var posYoverlay = overlayPos?.Y ?? 0;

                                            canvas.AddOverlay(overlayBitmap, posXoverlay, posYoverlay);
                                        }
                                    }
                                }
                                a.AddFrame(canvas);
                            }
                        }
                    }
                    a.Finish();
                }

                return File.Exists(outputFileName);
            }
            catch (Exception e)
            {
                log.Error(e, "Error during creation of the GIF file.");
                if (File.Exists(outputFileName)) { File.Delete(outputFileName); }
                return false;
            }
        }

        /// <summary>
        /// Remplace les couleurs d'une image, comme une clé chromatique, pour une autre.
        /// <br/>On peut donner de multiples cas de figure pour remplacer, mais un seul sera pris en compte.
        /// <br/>Ne prend pas en compte le facteur de grossissemt du paramètre de filtre. On peut utiliser la méthode ResizeImage pour cela.
        /// </summary>
        /// <param name="sourceBitmap">L'image source</param>
        /// <param name="group">Le groupe auquel appartient l'image à modifier.</param>
        /// <param name="filterData">Les données de filtre.</param>
        /// <returns>L'image modifiée.</returns>
        public static Bitmap ReplaceColors(Bitmap sourceBitmap, ColorSubstitutionFilterGroup group,
                                           BitmapModificationFilter filterData)
        {
            var resultBitmap = new Bitmap(sourceBitmap.Width, sourceBitmap.Height, PixelFormat.Format32bppArgb);

            var sourceData = sourceBitmap.LockBits(new Rectangle(0, 0, sourceBitmap.Width, sourceBitmap.Height),
                                                   ImageLockMode.ReadOnly, PixelFormat.Format32bppArgb);
            var resultData = resultBitmap.LockBits(new Rectangle(0, 0, resultBitmap.Width, resultBitmap.Height),
                                                   ImageLockMode.WriteOnly, PixelFormat.Format32bppArgb);

            var resultBuffer = new byte[resultData.Stride * resultData.Height];
            Marshal.Copy(sourceData.Scan0, resultBuffer, 0, resultBuffer.Length);

            sourceBitmap.UnlockBits(sourceData);

            const int minValue = Byte.MinValue;
            const int maxValue = Byte.MaxValue;

            for (var k = 0; k < resultBuffer.Length; k += 4)
            {
                var sourceAlpha = resultBuffer[k + 3];

                if (sourceAlpha == 0)
                    continue;
                // Totalement transparent, on ne modifie pas la valeur RGB du pixel, même s'il en possède une.

                var sourceBlue = resultBuffer[k];
                var sourceGreen = resultBuffer[k + 1];
                var sourceRed = resultBuffer[k + 2];

                int resultBlue = sourceBlue;
                int resultGreen = sourceGreen;
                int resultRed = sourceRed;

                foreach (var filter in filterData.ColorFilters)
                {
                    if (filter.Group != ColorSubstitutionFilterGroup.All && filter.Group != group)
                    {
                        continue;
                    }

                    var newRedValue = filter.NewColor.R;
                    var newGreenValue = filter.NewColor.G;
                    var newBlueValue = filter.NewColor.B;

                    var redFilter = filter.SourceColor.R;
                    var greenFilter = filter.SourceColor.G;
                    var blueFilter = filter.SourceColor.B;

                    if ((sourceBlue >= blueFilter + filter.ThresholdValue ||
                         sourceBlue <= blueFilter - filter.ThresholdValue) ||
                        (sourceGreen >= greenFilter + filter.ThresholdValue ||
                         sourceGreen <= greenFilter - filter.ThresholdValue) ||
                        (sourceRed >= redFilter + filter.ThresholdValue ||
                         sourceRed <= redFilter - filter.ThresholdValue)) continue;

                    resultBlue = MathUtil.Clamp(minValue, maxValue, blueFilter - sourceBlue + newBlueValue);
                    resultGreen = MathUtil.Clamp(minValue, maxValue, greenFilter - sourceGreen + newGreenValue);
                    resultRed = MathUtil.Clamp(minValue, maxValue, redFilter - sourceRed + newRedValue);

                    break; // Un effet de filtre maximum par pixel
                }

                resultBuffer[k] = (byte)resultBlue;
                resultBuffer[k + 1] = (byte)resultGreen;
                resultBuffer[k + 2] = (byte)resultRed;
                resultBuffer[k + 3] = sourceAlpha;
            }


            Marshal.Copy(resultBuffer, 0, resultData.Scan0, resultBuffer.Length);
            resultBitmap.UnlockBits(resultData);

            return resultBitmap;
        }

        /// <summary>
        /// Cette méthode va faire un overlay de l'image en paramètre sur un fond carrelé, afin de pouvoir visualiser le niveau
        /// de transparence de l'image.
        /// ATTENTION: Le fichier d'image sera écrasé par la modification!
        /// </summary>
        /// <param name="outImage">Le fichier à modifier pour en visualiser la transparence.</param>
        public static void DebugDisplayTransparencyOnImage(string outImage)
        {
            var bmp = FileUtil.LoadBitmap(outImage);

            using (var image = new Bitmap(bmp.Width, bmp.Height, PixelFormat.Format24bppRgb))
            {

                using (var g = Graphics.FromImage(image))
                {
                    var brushWhite = new SolidBrush(Color.FromArgb(255, 255, 255, 255));
                    var brushRect = new SolidBrush(Color.FromArgb(255, 210, 210, 210));
                    const int jump = 10;

                    g.FillRectangle(brushWhite, 0, 0, bmp.Width, bmp.Height);

                    for (var i = 0; i < bmp.Width; i += jump * 2)
                    {
                        for (var j = 0; j < bmp.Height; j += jump * 2)
                        {
                            g.FillRectangle(brushRect, i, j, jump, jump);
                        }
                    }

                    g.DrawImageUnscaled(bmp, 0, 0);
                }

                bmp.Dispose();

                if (StringUtil.GetFileExtension(outImage).EndsWith("png", true, CultureInfo.InvariantCulture))
                {
                    image.Save(ImageFormat.Png, 9, outImage);
                }
                else
                {
                    image.Save(ImageFormat.Jpeg, 90, outImage);
                }
            }
        }

        /// <summary>
        /// Modifie les pixels d'une images pour les transformer en une autre couleur, selon une table de substitution donnée
        /// en paramètre. On retourne une nouvelle référence vers l'image transformée. l'originale n'est pas touchée.
        /// Si cette classe de substitution est nulle, ou si l'image en paramètre est nulle, cette méthode ne fait rien.
        /// </summary>
        /// <param name="bmp">L'image originale. SI aucun changement, on retourne cette référence.</param>
        /// <param name="group">Le groupe auquel appartient cette image.</param>
        /// <param name="bmf">Les paramètres de filtrage. Peut être nul, ce qui est un NO-OP.</param>
        /// <returns>Référence vers le fichier original (no-op) ou un nouveau fichier bitmap modifié.</returns>
        public static Bitmap ModifyPicture(Bitmap bmp, ColorSubstitutionFilterGroup group, BitmapModificationFilter? bmf)
        {
            if (bmp == null || bmf == null) return bmp;

            var result = bmf.Value.IsApplicable(group) ? ReplaceColors(bmp, group, bmf.Value) : bmp;

            // ReSharper disable once CompareOfFloatsByEqualityOperator
            if (bmf.Value.SizeScale == null || bmf.Value.SizeScale.Value == 1.0f) return result;

            var rez = result.Resize((int)(result.Width * bmf.Value.SizeScale.Value), (int)(result.Height * bmf.Value.SizeScale.Value));
            result.Dispose();

            return rez;
        }

        public static readonly int CARD_PRINT_DEFAULT_LEFT_COLUMN = 200;
        public static readonly int CARD_PRINT_DEFAULT_RIGHT_COLUMN = 1290;
        public static readonly int CARD_PRINT_DEFAULT_TOP_ROW = 255;
        public static readonly int CARD_PRINT_DEFAULT_BOTTOM_ROW = 1560;
        public static readonly Size CARD_PRINT_FULL_PAGE_SIZE = new Size(2550, 3300);
        public static readonly Size CARD_PRINT_DEFAULT_INTERNAL_IMAGE_SIZE = new Size(830, 1120);

        public static Bitmap CreateCardPrintBitmap(Bitmap front, Bitmap back)
        {
            return CreateCardPrintBitmap(front, back,
                CARD_PRINT_DEFAULT_LEFT_COLUMN, CARD_PRINT_DEFAULT_TOP_ROW,
                CARD_PRINT_DEFAULT_RIGHT_COLUMN, CARD_PRINT_DEFAULT_BOTTOM_ROW);
        }

        public static Bitmap CreateCardPrintBitmap(Bitmap front, Bitmap back, int leftColumn, int topRow,
            int rightColumn, int bottomRow)
        {
            var baseBmp = new Bitmap(CARD_PRINT_FULL_PAGE_SIZE.Width, CARD_PRINT_FULL_PAGE_SIZE.Height,
                PixelFormat.Format32bppArgb);

            baseBmp.SetResolution(300, 300);
            front.SetResolution(300, 300);
            back.SetResolution(300, 300);

            using (var g = Graphics.FromImage(baseBmp))
            {
                // Fond blanc
                g.FillRectangle(new SolidBrush(Color.White), 0, 0, baseBmp.Width, baseBmp.Height);

                g.DrawImageUnscaled(front, leftColumn, topRow);
                g.DrawImageUnscaled(front, rightColumn, topRow);

                //back.RotateFlip(RotateFlipType.Rotate180FlipNone); // Il faut l'imprimer à l'envers

                g.DrawImageUnscaled(back, leftColumn, bottomRow);
                g.DrawImageUnscaled(back, rightColumn, bottomRow);
            }

            return baseBmp;
        }

        /// <summary>
        /// Créer une image à partir du contenu d'une autre image, mais contraint à un rectangle particulier de l'image d'origine.
        /// L'image finale à la taille du rectangle, la même résolution et le même format de pixel que l'image originale.
        /// </summary>
        /// <param name="bmp">L'image originale.</param>
        /// <param name="value">Le rectangle de contrainte.</param>
        /// <returns>Une nouvelle instance de Bitmap, résultat du cropping.</returns>
        public static Bitmap CropImageToRectangle(Bitmap bmp, Rectangle value)
        {
            var result = new Bitmap(value.Width, value.Height, bmp.PixelFormat);
            result.SetResolution(bmp.HorizontalResolution, bmp.VerticalResolution);

            using (var g = Graphics.FromImage(result))
            {
                g.DrawImageUnscaled(bmp, -value.X, -value.Y, value.Width, value.Height);
            }

            return result;
        }

        /// <summary>
        /// Créer une image avec du texte centré X/Y.
        /// </summary>
        /// <param name="text">Le texte à écrire.</param>
        /// <param name="bkg">La couleur de fond d'écran.</param>
        /// <param name="textColor">La couleur du texte.</param>
        /// <param name="font">Le font d'écriture.</param>
        /// <param name="size">La taille de l'image finale.</param>
        /// <returns>Une nouvelle image qui contient le texte.</returns>
        public static Bitmap CreateSimpleTextImage(string text, Color bkg, Color textColor, Font font, Size size)
        {
            var piti = new ProceduralImageTextInfo
            {
                BitmapSize = size,
                Color = textColor,
                BackgroundColor = bkg,
                FontFamilly = font.FontFamily.ToString(),
                FontSize = (int)font.Size,
                FontStyle = font.Style,
                FontGraphicUnit = font.Unit

            };

            return CreateProceduralTextBitmap(piti, text, null, size.Width);
        }

        /// <summary>
        /// Créer une image avec texte procéduralement, pour ne pas avoir à refaire l'image constamment selon les différentes dimensions.
        /// Contient une couleur de fond et une couleur de texte.
        /// </summary>
        /// <param name="piti">La structure de définition pour la construction de l'image.</param>
        /// <param name="text">La texte à rendre en image.</param>
        /// <param name="rotation">Optionel. Le nombre de degré en rotation.</param>
        /// <param name="maxLenght">Optionnel. La largeur maximale avant de rendre le texte. S'il dépasse, il sera divisé par ligne, entre les mots.</param>
        /// <param name="multiplineOffsetY">Optionel. La distance entre chaque ligne de texte, si séparé. 0 sinon (ne donne pas de bons résultats).</param>
        /// <returns>Un Bitmap créé sur mesure.</returns>
        public static Bitmap CreateProceduralTextBitmap(ProceduralImageTextInfo piti, string text,
            float? rotation = null, int? maxLenght = null, float multiplineOffsetY = 0)
        {
            var result = new Bitmap(piti.BitmapSize.Width, piti.BitmapSize.Height, PixelFormat.Format32bppArgb);

            var bkgBrush = new SolidBrush(piti.BackgroundColor);
            var textBrush = new SolidBrush(piti.Color);
            var font = new Font(piti.FontFamilly, piti.FontSize, piti.FontStyle, piti.FontGraphicUnit);

            using (var g = Graphics.FromImage(result))
            {
                g.FillRectangle(bkgBrush, 0, 0, piti.BitmapSize.Width, piti.BitmapSize.Height);
            }

            var x = piti.Center ? piti.BitmapSize.Width / 2f : 0;
            var y = piti.Center ? piti.BitmapSize.Height / 2f : 0;
            
            result.AddTextToBitmap(text, x, y, piti.Center, textBrush, font, rotation, maxLenght, multiplineOffsetY);

            return result;
        }

        /// <summary>
        /// Créer une image procéduralement en mettant une image déjà existante centrée dans un cadre, pour ne pas avoir
        /// à gérer des images placeholder de différentes tailles tout le temps.
        /// On donne une dimension finale, une image à centrer, puis optionnellement une couleur de fond.
        /// </summary>
        /// <param name="original">L'image originale.</param>
        /// <param name="peid">La structure de définition pour la construction de l'image.</param>
        /// <returns>Un Bitmap créé sur mesure.</returns>
        public static Bitmap CreateProceduralExtendedBitmap([NotNull]Bitmap original, ProceduralExtendedImageDefinition peid)
        {
            var result = new Bitmap(peid.Width, peid.Height, PixelFormat.Format24bppRgb);
            var color = Color.White;

            if (peid.FillColor != null)
            {
                color = peid.FillColor.Value;
            }
            else
            {
                var pixFmt = original.PixelFormat;

                if (pixFmt != PixelFormat.Format32bppArgb)
                {
                    // Tous les pixels ne sont pas opaques, on prend le premier
                    color = original.GetPixel(0, 0);
                }
                else
                {
                    // On itère pour trouver le premier pixel non opaque.
                    var bmp1Data = original.LockBits(new Rectangle(0, 0, original.Width, original.Height), ImageLockMode.ReadOnly, pixFmt);
                    try
                    {
                        var size1 = bmp1Data.Stride * bmp1Data.Height;
                        var data1 = new byte[size1];
                        Marshal.Copy(bmp1Data.Scan0, data1, 0, size1);

                        var stopAll = false;
                        for (var y = 0; y < original.Height; y++)
                        {

                            for (var x = 0; x < original.Width; x++)
                            {
                                var index1 = y * bmp1Data.Stride + x * 4;
                                // format bgra
                                if (data1[index1 + 3] != 255) continue;
                                color = Color.FromArgb(data1[index1 + 2], data1[index1 + 1], data1[index1 + 0]);
                                stopAll = true;
                                break;
                            }
                            if (stopAll) break;
                        }
                    }
                    finally
                    {
                        original.UnlockBits(bmp1Data);
                    }
                }
            }

            // On créé l'image
            using (var g = Graphics.FromImage(result))
            {
                g.FillRectangle(new SolidBrush(color), 0, 0, peid.Width, peid.Height);
                g.DrawImageUnscaled(original,
                    (peid.Width / 2) - (original.Width / 2),
                    (peid.Height / 2) - (original.Height / 2));
            }

            return result;
        }
    }
}
