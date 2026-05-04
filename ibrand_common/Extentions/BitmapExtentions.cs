using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Drawing.Imaging;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using JetBrains.Annotations;
using NLog;
using NomadLogic.Common.Util;
using NomadLogic.Common.Util.Math;
using Encoder = System.Drawing.Imaging.Encoder;
using PixelFormat = System.Drawing.Imaging.PixelFormat;

namespace NomadLogic.Common.Extentions
{
    /// Orientation d'image EXIF connues, plus une de notre cru (Aucune).
    [PublicAPI]
    public enum ExifOrientations
    {
        /// Ne fait pas partit du standard. Implique le fichier ne contient pas d'information EXIF de rotation.
        /// Ou ne peut pas en avoir (comme un PNG)
        None = -1,
        Unknown = 0,
        /// Orientation normale, aucune rotation
        TopLeft = 1,
        TopRight = 2,
        /// Représente une rotation de 180
        BottomRight = 3,
        BottomLeft = 4,
        LeftTop = 5,
        /// Rotation de 90
        RightTop = 6,
        RightBottom = 7,
        /// Rotation de 270
        LeftBottom = 8
    }

    /// <summary>
    /// Classe d'extension pour toutes les méthodes de manipulation d'images sur les Bitmap de GDI+. Toute ces méthodes prennent au moins un
    /// argument étant un Bitmap (celui sur lequel se baser ou celui qui reçoit les changements).
    /// Contient aussi quelques méthodes générales propres aux Bitmap de GDI+.
    /// </summary>
    [PublicAPI]
    public static class BitmapExtentions
    {
        private static readonly Logger log = LogManager.GetCurrentClassLogger();

        /// Structure pour décrire les extensions valides d'un codec, et d'un fonction pour obtenir ce codec.
        [PublicAPI]
        public struct ImageSaveCodecInfo
        {
            public IReadOnlyCollection<string> ValidExtensions { get; set; }
            public Func<ImageCodecInfo> GetEncoderFunc { get; set; }
        }

        /// Dictionnaire des formats de sauvegarde simnple utilisé pour les méthode d'extension de sauvegarde.
        [PublicAPI]
        public static readonly IReadOnlyDictionary<ImageFormat, ImageSaveCodecInfo> CodecInfos = new Dictionary<ImageFormat, ImageSaveCodecInfo>
        {
            {ImageFormat.Png, new ImageSaveCodecInfo
            {
                ValidExtensions = new [] { "png" },
                GetEncoderFunc = () => GetEncoder(ImageFormat.Png)
            }},
            {ImageFormat.Jpeg, new ImageSaveCodecInfo
            {
                ValidExtensions = new [] { "jpg", "jpeg" },
                GetEncoderFunc = () => GetEncoder(ImageFormat.Jpeg)
            }},
            
            {ImageFormat.Bmp, new ImageSaveCodecInfo
            {
                ValidExtensions = new [] { "bmp" },
                GetEncoderFunc = () => GetEncoder(ImageFormat.Bmp)
            }}
            // Les autres formats devraient être utilisés dans d'autres librairies, plus spécialisées.
        };
        
        /// Regarde la validité des références d'objets Bitmaps en paramètre.
        public static bool CheckIfValidBitmaps(Image arg1, Image arg2)
        {
            if (arg1?.RawFormat != null && arg2?.RawFormat != null) return true;
            log.Error("Attempt to use a null or disposed bitmap.");
            return false;
        }
        
        /// Regarde la validité des références d'objets Bitmaps en paramètre.
        public static bool CheckIfValidBitmaps(Image arg1)
        {
            if (arg1?.RawFormat != null) return true;
            log.Error("Attempt to use a null or disposed bitmap.");
            return false;
        }
        
        /// Regarde la validité des références d'objets Bitmaps en paramètre.
        public static bool CheckIfValidBitmaps(params Image[] args)
        {
            if (args.Any(a => a?.RawFormat == null)) return true;
            log.Error("Attempt to use a null or disposed bitmap.");
            return false;
        }
        
        /// <summary>
        /// Vérifier si tous les pixels d'une ligne ne sont pas transparent à 100%
        /// </summary>
        /// <param name="sx">coordonnée en X du point de départ</param>
        /// <param name="sy">coordonnée en Y du point de départ</param>
        /// <param name="dx">coordonnée en x du point d'arrivé</param>
        /// <param name="dy">coordonnée en Y du point d'arrivé</param>
        /// <param name="image">l'image sur laquelle la droite est</param>
        /// <returns>vrai si pour tous les pixels de la ligne a != 0</returns>
        public static bool IsLineFull(this Bitmap image, int sx, int sy, int dx, int dy)
        {
            if (!CheckIfValidBitmaps(image)) return false;

            // on s'arrange pour aller de gauche à droite (ou de bas en haut si x bouge pas)
            if (sx > dx || (sx == dx && sy > dy))
            {
                var temp = sx;
                sx = dx;
                dx = temp;
                temp = sy;
                sy = dy;
                dy = temp;
            }

            // vive la géométrie et les fonctions linéaires!
            double deltaX = (dx - sx);
            double deltaY = (dy - sy);
            double a;
            if (deltaY != 0)
                a = deltaX / deltaY;
            else
                a = 0;

            var b = (int) (sy - (a * sx));

            // si la pente est plus petite que 1
            if (deltaX >= deltaY)
            {
                for (var i = sx; i < dx; i++)
                {
                    var y = (int) (a * i + b);
                    if (image.GetPixel(i, y).A == 0)
                    {
                        return false;
                    }
                }

                // si la pente est plus grande que 1, on varie le y au lieu du x
                // pour tester le plus de valeurs possibles
            }
            else
            {
                for (var i = sy; i < dy; i++)
                {
                    int x;
                    if (a != 0)
                        x = (int) ((i - b) / a);
                    else
                        x = sx;
                    if (image.GetPixel(x, i).A == 0)
                    {
                        return false;
                    }
                }
            }

            return true;
        }

        /// <summary>
        /// Trouve la colonne qui contient le pixel le plus à gauche qui n'est pas transparent.
        /// </summary>
        /// <param name="image">L'image à analyser.</param>
        /// <returns>l'indice de la colonne qui contient le premier pixel pas complètement transparent.</returns>
        public static int FirstLeftOpaquePixelIndex(this Bitmap image)
        {
            if (!CheckIfValidBitmaps(image)) return -1;
            
            var gaucheTrouve = false;
            var gauche = -1;

            // trouver le pixel le plus à gauche de l'image qui n'est pas transparent
            for (var i = 0; i < image.Width && !gaucheTrouve; i++)
            {
                for (var j = 0; j < image.Height && !gaucheTrouve; j++)
                {
                    var pixel = image.GetPixel(i, j);
                    if (pixel.A == 0 || !IsLineFull(image, i, j, i + 10, j)) continue;

                    gaucheTrouve = true;
                    gauche = i;
                }
            }

            return gauche;
        }

        /// <summary>
        /// Trouve la colonne qui contient le pixel le plus à droite qui n'est pas transparent.
        /// </summary>
        /// <param name="image">L'image à analyser.</param>
        /// <returns>L'indice de la colonne qui contient le premier pixel pas complètement transparent. -1 si aucun</returns>
        public static int FirstRightPixelIndex(this Bitmap image)
        {
            if (!CheckIfValidBitmaps(image)) return -1;
            
            var droiteTrouve = false;
            var droite = -1;

            // trouver le pixel le plus à droite de l'image qui n'est pas transparent
            for (var i = image.Width - 1; i > 0 && !droiteTrouve; i--)
            {
                for (var j = 0; j < image.Height && !droiteTrouve; j++)
                {
                    var pixel = image.GetPixel(i, j);
                    if (pixel.A == 0 || !IsLineFull(image, i - 10, j, i, j)) continue;

                    droiteTrouve = true;
                    droite = i;
                }
            }

            return droite;
        }

        /// <summary>
        /// Obtient l'information CODEC d'un format d'image spécifique.
        /// </summary>
        /// <param name="format">Le format à utiliser. Utiliser l'énumération ImageFormat pour choisir une valeur.</param>
        /// <returns>L'object ImageCodecInfo du format choisi. Null si inexistant.</returns>
        public static ImageCodecInfo GetEncoder(this ImageFormat format)
        {
            if (format == null)
            {
                return null;
            }

            var codecs = ImageCodecInfo.GetImageDecoders();

            return codecs.FirstOrDefault(codec => codec.FormatID == format.Guid);
        }

        /// <summary>
        /// Sauvegarde d'un fichier image en JPEG avec compression.
        /// <br/>Voir ici pour plus de détails: http://msdn.microsoft.com/en-us/library/bb882583%28v=vs.110%29.aspx
        /// </summary>
        /// <param name="image">L'image à sauvegarder.</param>
        /// <param name="compressionLevel">Le niveau de compression, entre 1 et 100</param>
        /// <param name="path">Le chemin où sauvegarder le fichier.</param>
        /// <returns>true si la sauvegarde s'est effectuée. Faux sinon.</returns>
        [Obsolete("On peut utiliser la version qui tente de détecter le format via le nom de fichier, ou spécifier un format.")]
        public static bool SaveAsJpeg(this Image image, long compressionLevel, string path)
        {
            return CheckIfValidBitmaps(image) && Save(image, GetEncoder(ImageFormat.Jpeg), compressionLevel, path);
        }

        /// <summary>
        /// Sauvegarde d'un fichier image. On tente de déterminer le codec via l'extension du fichier de sauvegarde.
        /// </summary>
        /// <param name="image">L'image à sauvegarder.</param>
        /// <param name="compressionLevel">Le niveau de compression. Selon le codec utilisé, cette valeur peut être interprétée de façon différente.</param>
        /// <param name="path">Le chemin où sauvegarder le fichier.</param>
        /// <returns>true si la sauvegarde s'est effectuée. Faux sinon.</returns>
        // TODO modifier pour sécuriser la qualité de sauvegarde
        public static bool Save(this Image image, long compressionLevel, string path)
        {
            if (!CheckIfValidBitmaps(image)) return false;

            var codec = GetEncoderCandidateFromFileName(path);

            return codec != null && Save(image, codec, compressionLevel, path);
        }

        /// Tente de déterminer quel est le type d'encodage approprié en examinant un nom de fichier simple.
        public static ImageCodecInfo GetEncoderCandidateFromFileName(string file)
        {
            if (string.IsNullOrWhiteSpace(file)) return null;

            ImageCodecInfo codec = null;

            using (var ite = CodecInfos.GetEnumerator())
            {
                while (ite.MoveNext())
                {
                    var ci = ite.Current.Value;
                    if (!ci.ValidExtensions.Any(ext => file.EndsWith(ext, StringComparison.InvariantCultureIgnoreCase))) continue;
                    codec = ci.GetEncoderFunc.Invoke();
                    break;
                }
            }

            if (codec != null) return codec;
            
            // No Candidate found
            log.Warn($"No simple, single frame file encoding candidate found: {file}");
            return null;
        }
        
        /// <summary>
        /// Sauvegarde une image dans un fichier avec un format d'image spécifique.
        /// <br/>Vous pouvez utiliser la méthode "ImageUtil.GetEncoder" pour sélectionner un codec précis.
        /// </summary>
        /// <param name="image">L'image à sauvegarder.</param>
        /// <param name="format">Le format d'image</param>
        /// <param name="compressionLevel">Le paramètre numérique du type de compression. Les valeurs changent selon le type de format d'image utilisé.
        /// Valeur entre 1 et 100 inclusivement.</param>
        /// <param name="path">Le chemin absolu du fichier à écrire.</param>
        /// <returns>TRUE si tout à bien fonctionné.</returns>
        public static bool Save(this Image image, ImageFormat format, long compressionLevel, string path)
        {
            return CheckIfValidBitmaps(image) && Save(image, format.GetEncoder(), compressionLevel, path);
        }

        /// <summary>
        /// Sauvegarde une image dans un fichier avec un codec d'image spécifique.
        /// <br/>Vous pouvez utiliser la méthode "ImageUtil.GetEncoder" pour sélectionner un codec précis.
        /// </summary>
        /// <param name="image">L'image à sauvegarder.</param>
        /// <param name="codec">Le codec du type d'image à utiliser.</param>
        /// <param name="compressionLevel">Le paramètre numérique du type de compression. Les valeurs changent selon le type de codec utilisé.
        /// Valeur entre 1 et 100 inclusivement.</param>
        /// <param name="path">Le chemin absolu du fichier à écrire.</param>
        /// <returns>TRUE si tout à bien fonctionné.</returns>
        public static bool Save(this Image image, ImageCodecInfo codec, long compressionLevel, string path)
        {
            if (!CheckIfValidBitmaps(image)) return false;
            
            // Vérification des arguments.
            if (codec == null || path == null || path.Length < 5 || compressionLevel < 1 ||
                compressionLevel > 100)
            {
                return false;
            }

            try
            {
                // Create an Encoder object based on the GUID 
                // for the Quality parameter category.
                var myEncoder = Encoder.Quality;

                // Create an EncoderParameters object. 
                // An EncoderParameters object has an array of EncoderParameter 
                // objects. In this case, there is only one 
                // EncoderParameter object in the array.
                var myEncoderParameters = new EncoderParameters(1);

                var myEncoderParameter = new EncoderParameter(myEncoder, compressionLevel);
                myEncoderParameters.Param[0] = myEncoderParameter;
                image.Save(path, codec, myEncoderParameters);
            }
            catch (Exception e)
            {
                log.Error(e, "Could not save Image file with options: path={0}, codec={1}, compression={2}", path,
                    codec.CodecName, compressionLevel);
                return false;
            }

            return true;
        }

        /// <summary>
        /// Rogne l'image pour en faire une nouvelle image carrée. Enlève les côtés de l'image à partir de sa plus grande dimension, sans affecter le ratio du contenu.
        /// <br/>En gros, une image rectangulaire horizontale sera rognée sur les côtés, et une image rectangulaire verticale sera rognée sur le haut et le bas
        /// de l'image.
        /// <br/>Une image déjà parfaitement carrée sera simplement copiée.
        /// </summary>
        /// <param name="image">L'image à traiter.</param>
        /// <returns>Une nouvelle instance carrée de l'image originale.</returns>
        public static Bitmap ResizeToSquare(this Bitmap image)
        {
            if (!CheckIfValidBitmaps(image)) return null;
            
            var width = image.Width;
            var height = image.Height;

            var x = 0;
            var y = 0;

            Bitmap result;

            if (width < height)
            {
                result = new Bitmap(width, width);
                y = (height - width) / 2;
            }
            else if (width > height)
            {
                result = new Bitmap(height, height);
                x = (width - height) / 2;
            }
            else
            {
                // Déjà carré
                result = new Bitmap(width, height);
            }

            result.SetResolution(image.HorizontalResolution, image.VerticalResolution);

            using (var graphics = Graphics.FromImage(result))
            {
                //set the resize quality modes to high quality
                graphics.CompositingQuality = CompositingQuality.HighQuality;
                graphics.InterpolationMode = InterpolationMode.HighQualityBicubic;
                graphics.SmoothingMode = SmoothingMode.HighQuality;
                //draw the image into the target bitmap
                graphics.DrawImage(image, 0, 0, new Rectangle(x, y, result.Width, result.Height), GraphicsUnit.Pixel);
            }

            return result;
        }

        /// <summary>
        /// Met une image sur une autre, avec un offset particulier.
        /// Assume que l'image à superposer est plus petite que l'image réceptrice.
        /// </summary>
        /// <param name="picture">L'image à poser.</param>
        /// <param name="x">Position en X de l'image à poser.</param>
        /// <param name="y">Position en Y de l'image à poser.</param>
        /// <param name="frame">L'image réceptrice.</param>
        /// <param name="frameOverPicture">Indique qu'il faut superposer le frame par dessus l'image, au lieu de la facon de faire habituelle.</param>
        /// <param name="bkgColor">Color de fond à mettre en premier sur le canevas.</param>
        /// <param name="subImageRotation">Indique la rotation/flip à effectuer sur l'image à dessiner.</param>
        /// <returns>Une nouvelle instance de la composition voulue.</returns>
        public static Bitmap CreateFramedPicture(Bitmap frame, Bitmap picture, int x, int y, bool frameOverPicture = false,
            Color? bkgColor = null, RotateFlipType? subImageRotation = null)
        {
            if (!CheckIfValidBitmaps(picture, frame)) return null;
            
            var dest = new Bitmap(frame.Width, frame.Height, PixelFormat.Format24bppRgb);

            using (var printPicture = new Bitmap(picture))
            {
                if (subImageRotation != null) printPicture.RotateFlip(subImageRotation.Value);

                using (var g = Graphics.FromImage(dest))
                {
                    if (bkgColor.HasValue)
                        g.FillRectangle(new SolidBrush(bkgColor.Value), 0, 0, dest.Width, dest.Height);

                    if (!frameOverPicture)
                    {
                        g.DrawImage(frame, 0, 0, frame.Width, frame.Height);
                        g.DrawImage(printPicture, x, y, printPicture.Width, printPicture.Height);
                    }
                    else
                    {
                        g.DrawImage(printPicture, x, y, printPicture.Width, printPicture.Height);
                        g.DrawImage(frame, 0, 0, frame.Width, frame.Height);
                    }
                }
            }

            return dest;
        }

        /// <summary>
        /// Coupe l'image vers une nouvelle taille, de façon centrée. La coupe se fait autant sur la hauteur que la largeur.
        /// L'image n'est pas redimensionnée, juste "cadrée". Si l'image est plus petite, rien ne sera perdu, et l'image résultante sera
        /// centrée dans le bitmap résultant. Le bitmap sera alors de la taille demandée, mais avec du vide autour de l'image (cadre transparent).
        /// </summary>
        /// <param name="image">L'image originale sur laquelle se baser.</param>
        /// <param name="width">La largeur du cadre.</param>
        /// <param name="height">La hauteur du cadre.</param>
        /// <returns>Un cadre avec le résultat de l'élaguage, s'il a eu lieu.</returns>
        public static Bitmap Crop(this Image image, int width, int height)
        {
            if (!CheckIfValidBitmaps(image)) return null;
            if (width < 1 || height < 1) return null;
            
            var result = new Bitmap(width, height, PixelFormat.Format32bppArgb);
            result.SetResolution(image.HorizontalResolution, image.VerticalResolution);

            DrawCroppedBitmap(image, width, height, result);

            return result;
        }

        private static void DrawCroppedBitmap(Image image, int width, int height, Bitmap result)
        {
            if (!CheckIfValidBitmaps(image, result)) return;
            
            // set the resolutions the same to avoid cropping due to resolution differences
            result.SetResolution(image.HorizontalResolution, image.VerticalResolution);

            using (var graphics = Graphics.FromImage(result))
            {
                //set the resize quality modes to high quality
                graphics.CompositingQuality = CompositingQuality.HighQuality;
                graphics.InterpolationMode = InterpolationMode.HighQualityBicubic;
                graphics.SmoothingMode = SmoothingMode.HighQuality;
                //draw the image into the target bitmap
                var x = (image.Width - width) / 2;
                var y = (image.Height - height) / 2;
                graphics.DrawImage(image, 0, 0, new Rectangle(x, y, result.Width, result.Height), GraphicsUnit.Pixel);
                graphics.Dispose();
            }
        }

        /// <summary>
        /// Créer un nouveau Bitmap en prenant uniquement une portion de l'original, à partir de coordonées fixes.
        /// Cela s'appelle un élaguage (Crop) en anglais. Si les coordonées sortent de la taille originale, seuls les pixels
        /// existant seront transférés. 
        /// </summary>
        /// <param name="image">L'original</param>
        /// <param name="startPoint">Le point de départ</param>
        /// <param name="width">La largeur voulu</param>
        /// <param name="height"></param>
        /// <returns>Une nouvelle instance de Bitmap.</returns>
        public static Bitmap Crop(this Image image, Point startPoint, int width, int height)
        {
            return !CheckIfValidBitmaps(image) ? null : Crop(image, new Rectangle(startPoint.X, startPoint.Y, width, height));
        }

        /// <summary>
        /// Créer un nouveau Bitmap en prenant uniquement une portion de l'original, à partir de coordonées fixes.
        /// Cela s'appelle un élaguage (Crop) en anglais. Si les coordonées sortent de la taille originale, seuls les pixels
        /// existant seront transférés. 
        /// </summary>
        /// <param name="image">L'original</param>
        /// <param name="cropZone">La zone à découper</param>
        /// <returns>Une nouvelle instance de Bitmap.</returns>
        public static Bitmap Crop(this Image image, Rectangle cropZone)
        {
            if (!CheckIfValidBitmaps(image)) return null;
            
            var result = new Bitmap(cropZone.Width, cropZone.Height, PixelFormat.Format32bppArgb);
            result.SetResolution(image.HorizontalResolution, image.VerticalResolution);

            using (var graphics = Graphics.FromImage(result))
            {
                //set the resize quality modes to high quality
                graphics.CompositingQuality = CompositingQuality.HighQuality;
                graphics.InterpolationMode = InterpolationMode.HighQualityBicubic;
                graphics.SmoothingMode = SmoothingMode.HighQuality;
                //draw the image into the target bitmap
                graphics.DrawImage(image, 0, 0, cropZone, GraphicsUnit.Pixel);
                graphics.Dispose();
            }

            return result;
        }

        ///
        /// Enlever les pixels transparent sur les côtés (Haut, Bas, Gauche, Droite) de l'image pour cadrer au maximum
        /// la partie centrale qui contient des pixels autres que transparent.
        /// <param name="original">L'image à tailler</param>
        /// <returns>la photo taillée pour enlever le trop-plein transparent.</returns>
        ///
        public static Bitmap CropToOpaque(this Bitmap original)
        {
            if (!CheckIfValidBitmaps(original)) return null;
            
            int haut = 0, bas = original.Height;
            bool hautTrouve = false, basTrouve = false;
            Color pixel;

            var gauche = original.FirstLeftOpaquePixelIndex();

            var droite = original.FirstRightPixelIndex();

            // trouver le plus haut pixel de l'image qui n'est pas transparent
            for (var i = 0; i < original.Height && !hautTrouve; i++)
            {
                for (var j = gauche; j < droite && !hautTrouve; j++)
                {
                    pixel = original.GetPixel(j, i);
                    if (pixel.A == 0 || !IsLineFull(original, j, i, j, i + 10)) continue;
                    hautTrouve = true;
                    haut = i;
                }
            }

            // trouver le plus bas pixel de l'image qui n'est pas transparent
            for (var i = original.Height - 1; i > haut && !basTrouve; i--)
            {
                for (var j = gauche; j < droite && !basTrouve; j++)
                {
                    pixel = original.GetPixel(j, i);
                    if (pixel.A == 0 || !IsLineFull(original, j, i - 10, j, i)) continue;
                    basTrouve = true;
                    bas = i;
                }
            }

            // si l'image n'est que transparente, on retourne null
            if (gauche == droite || haut == bas) return null;

            var finale = new Bitmap(droite - gauche, bas - haut);
            finale.SetResolution(original.HorizontalResolution, original.VerticalResolution);
            var g = Graphics.FromImage(finale);
            g.DrawImageUnscaled(original, -gauche, -droite);
            g.Flush();

            return finale;
        }

        /// <summary>
        /// Effecture une rotation sur le fichier d'image, et non seulement sur ses pixels. 
        /// La rotation s'effectue sur l'ensemble des propriétés de l'image, incluant sa taille.
        /// Par exemple, si on fait une rotation de 90 degrés, la taille passera de "AxB" à "BxA".
        /// Si on veut uniquement faire une rotation sur les pixels, sans toucher au canevas de l'image, il faut utiliser les
        /// méthodes "RotateImage".
        /// <br/>Dû aux propriétés d'une rotation sur un carré, la taille de l'image résultante peut être
        /// différente de l'image originale, semblable à une opération combinée d'élaguage (crop) et de translation.
        /// Seules les rotations de multiples de 180 degrés conservent la taille d'image originale.
        /// <br/>La rotation se fait également sur un angle de -180 à 180 degrés. Si l'angle en paramètre
        /// ne corresponde pas, il sera transformé dans cette fenêtre de valeur.
        /// Les angles qui sont un multiple de 90 sont très efficaces car on ne fait que manipuler l'ordre des colonnes/rangée, plutôt que
        /// de recalculer l'image.
        /// Le point central de la rotation (en terme de points cartésiens) est toujours le milieu.
        /// </summary>
        /// <param name="image">L'image à transformer.</param>
        /// <param name="rotationDegree">La rotation en degrés. Peut être positif ou négatif.</param>
        /// <returns>Une nouvelle instance qui contient le résultat de la rotation de fichier.</returns>
        public static Bitmap RotateToNewSize(this Image image, double rotationDegree)
        {
            if (!CheckIfValidBitmaps(image)) return null;
            
            var rotation = rotationDegree;
            if (rotation > 180)
            {
                rotation = (rotation % 360) - 360;
            }
            else if (rotation < -180)
            {
                rotation = (rotation % 360) + 360;
            }

            Bitmap rotatedImage;
            // calcul de la translation pour mettre l'image dans le centre

            // Pour enlever l'incertitude liée à la divison (valeur zéro)
            if (rotation == 180 || rotation == -180)
            {
                rotatedImage = new Bitmap(image.Width, image.Height, Graphics.FromImage(image));
                rotatedImage.SetResolution(image.HorizontalResolution, image.VerticalResolution);
                var g2D = Graphics.FromImage(rotatedImage);
                g2D.DrawImage(image, new Point(0, 0));

                rotatedImage.RotateFlip(RotateFlipType.Rotate180FlipNone);
            }
            else if (rotation == 0)
            {
                // C'est la même image!!! On ne fait qu'une copie donc.
                rotatedImage = new Bitmap(image.Width, image.Height, image.PixelFormat);
                rotatedImage.SetResolution(image.HorizontalResolution, image.VerticalResolution);
                var g2D = Graphics.FromImage(rotatedImage);
                g2D.DrawImage(image, new Point(0, 0));
            }
            else if (rotation == 90)
            {
                rotatedImage = new Bitmap(image.Width, image.Height, image.PixelFormat);
                rotatedImage.SetResolution(image.HorizontalResolution, image.VerticalResolution);
                var g2D = Graphics.FromImage(rotatedImage);
                g2D.DrawImage(image, new Point(0, 0));

                rotatedImage.RotateFlip(RotateFlipType.Rotate90FlipNone);
            }
            else if (rotation == -90)
            {
                rotatedImage = new Bitmap(image.Width, image.Height, image.PixelFormat);
                rotatedImage.SetResolution(image.HorizontalResolution, image.VerticalResolution);
                var g2D = Graphics.FromImage(rotatedImage);
                g2D.DrawImage(image, new Point(0, 0));

                rotatedImage.RotateFlip(RotateFlipType.Rotate270FlipNone);
            }
            else
            {
                var boundingRectangle = MathUtil.CalculateRotationBoundingBox(image.Width, image.Height, rotation);

                rotatedImage = new Bitmap(boundingRectangle.Width, boundingRectangle.Height, image.PixelFormat);
                rotatedImage.SetResolution(image.HorizontalResolution, image.VerticalResolution);
                var g2D = Graphics.FromImage(rotatedImage);
                g2D.RotateTransform((float) rotation);
                g2D.TranslateTransform(boundingRectangle.X, boundingRectangle.Y);
                g2D.DrawImage(image, new Point(0, 0));
            }

            return rotatedImage;
        }

        /// <summary>
        /// Effectue une rotation sur une image à partir du centre.
        /// Ne modifie que les pixels de l'image, le résultat aura les même dimensions, résolution et format de pixels.
        /// </summary>
        /// <param name="image">L'image à tourner.</param>
        /// <param name="rotationAngle">L'angle en degrés (positif => sens horaire)</param>
        /// <returns>Une nouvelle référence vers le résultat.</returns>
        public static Bitmap Rotate(this Bitmap image, float rotationAngle)
        {
            return !CheckIfValidBitmaps(image) ? null : Rotate(image, rotationAngle, image.Width / 2f, image.Height / 2f);
        }

        /// <summary>
        /// Effectue une rotation sur une image à partir d'un point précis dans l'image.
        /// Ne modifie que les pixels de l'image, le résultat aura les même dimensions, résolution et format de pixels.
        /// </summary>
        /// <param name="image">L'image à tourner.</param>
        /// <param name="rotationAngle">L'angle en degrés (positif => sens horaire)</param>
        /// <param name="x">Le point en X du centre de la rotation.</param>
        /// <param name="y">Le point en Y du centre de la rotation.</param>
        /// <returns>Une nouvelle référence vers le résultat.</returns>
        public static Bitmap Rotate(this Bitmap image, float rotationAngle, float x, float y)
        {
            if (!CheckIfValidBitmaps(image)) return null;
            
            //create an empty Bitmap image
            var bmp = new Bitmap(image.Width, image.Height);
            bmp.SetResolution(image.HorizontalResolution, image.VerticalResolution);

            //turn the Bitmap into a Graphics object
            using (var gfx = Graphics.FromImage(bmp))
            {
                //now we set the rotation point to the center of our image
                gfx.TranslateTransform(x, y);

                //now rotate the image
                gfx.RotateTransform(rotationAngle);

                gfx.TranslateTransform(-x, -y);

                //set the InterpolationMode to HighQualityBicubic so to ensure a high
                //quality image once it is transformed to the specified size
                gfx.InterpolationMode = InterpolationMode.HighQualityBicubic;

                //now draw our new image onto the graphics object
                gfx.DrawImage(image, new Point(0, 0));
            }

            //return the image
            return bmp;
        }

        /// <summary>
        /// Copie l'image en paramètre dans l'instance, en ajoutant en plus une valeur alpha de base à tous les pixels.
        /// En gros, ajoute une image potentiellement transparente par-dessus celle existante comme un calque.
        /// </summary>
        /// <param name="image">L'image originale</param>
        /// <param name="overlay">L'image qui doit être copiée (calque).</param>
        /// <param name="x">Position en X du calque.</param>
        /// <param name="y">Position en Y du calque.</param>
        /// <param name="alpha">Le degré d'alpha à mettre dans l'image originale, normalisé. Doit être entre 0 et 1. 1 par défaut.</param>
        public static void AddOverlay(this Image image, Bitmap overlay, float x = 0, float y = 0, float alpha = 1.0f)
        {
            if (!CheckIfValidBitmaps(image, overlay)) return;
            
            var a = MathUtil.Clamp(0.0f, 1.0f, alpha);

            using (var g = Graphics.FromImage(image))
            {
                if (a == 1.0f)
                {
                    overlay.SetResolution(image.HorizontalResolution, image.VerticalResolution);
                    g.InterpolationMode = InterpolationMode.HighQualityBicubic;
                    g.DrawImage(overlay, x, y);
                }
                else
                {
                    // Initialize the color matrix.
                    float[][] matrixItems =
                    {
                        new[] {1f, 0, 0, 0, 0},
                        new[] {0, 1f, 0, 0, 0},
                        new[] {0, 0, 1f, 0, 0},
                        new[] {0, 0, 0, a, 0},
                        new[] {0, 0, 0, 0, 1f}
                    };
                    var colorMatrix = new ColorMatrix(matrixItems);

                    // Create an ImageAttributes object and set its color matrix.
                    var imageAtt = new ImageAttributes();
                    imageAtt.SetColorMatrix(colorMatrix, ColorMatrixFlag.Default, ColorAdjustType.Bitmap);

                    // Now draw the semitransparent bitmap image.
                    var w = overlay.Width;
                    var h = overlay.Height;
                    g.DrawImage(overlay,
                        new Rectangle(0, 0, w, h), // destination rectangle
                        0, 0, w, h, // Source coordinates
                        GraphicsUnit.Pixel,
                        imageAtt);
                }
            }
        }

        /// <summary>
        /// Ajoute un dégradé d'une couleur par-dessus une image.
        /// <br/>La raison pour laquelle on demande une valeur réelle entre 0 et 1 plutôt qu'un entier entre 0 et 255
        /// pour la valeur alpha est qu'il est plus facile (habituellement) de générer une valeur réelle par divison
        /// lors d'une animation.
        /// </summary>
        /// <param name="source">L'image à modifier.</param>
        /// <param name="a">Valeur en alpha fractionnaire (0 à 1). Sera convertit en entier de 0 à 255.</param>
        /// <param name="r">Valeur rouge, entre 0 et 255.</param>
        /// <param name="g">Valeur verte, entre 0 et 255.</param>
        /// <param name="b">Valeur bleue, entre 0 et 255.</param>
        public static void AddOverlayColor(this Image source, int r, int g, int b, float a = 1.0f)
        {
            if (!CheckIfValidBitmaps(source)) return;

            a = MathUtil.Clamp(0.0f, 1.0f, a);
            r = MathUtil.Clamp(0, 255, r);
            g = MathUtil.Clamp(0, 255, g);
            b = MathUtil.Clamp(0, 255, b);
            var overlay = new Bitmap(source.Width, source.Height, PixelFormat.Format32bppArgb);
            using (var gra = Graphics.FromImage(overlay))
            {
                gra.Clear(Color.FromArgb((int) (a * 255), r, g, b));
                gra.Flush();
            }

            using (var gra = Graphics.FromImage(source))
            {
                gra.DrawImage(overlay, 0, 0);
                gra.Flush();
            }
        }
        
        /// <summary>
        /// Cette méthode normalise une image de façon à la centrer sur un point (ou pixel) précis à partir de deux autres points, et de quelques autres paramètres qui définissent les
        /// propriétés de l'image et des points fournis. Au final, il est possible que l'image subisse une transformation incluant translation, rotation et redimensionnement.
        /// <br/>La translation se fait en calculent un point central entre les deux points donnés en paramètre, et en effectuant une translation pour que ce point soit égal au
        /// point central donné en paramètre.
        /// <br/>La rotation se fait en calculant l'inverse de l'angle entre l'axe des X et la ligne tracée entre les deux points. Si cet angle (en absolu) est inférieur à l'angle
        /// minimal donné en paramètre, aucune rotation ne sera faite, mais on normalisera les points (avec une moyenne) en modifiant leur valeur en Y pour les rendre égaux.
        /// <br/>Le dimensionnement sera donné en calculant la distance entre les deux points (distance en pixel), et en comparant la valeur relative (donné en paramètre) qui 
        /// correspond à cette distance versus la valeur absolue de distance par pixel, également donnée en paramètre. Le ratio du redimensionnement sera égal à celui requis pour
        /// que la distance relative par pixel soit le même que la distance absolue donnée.
        /// <br/>La taille finale de l'image modifiée ne sera pas égale à celle de l'image originale, à cause des transformations. Qui plus est, si rotation il y a, il est fort
        /// possible que des zones transparentes existent autour des limites de l'image pour respecter le fait qu'une image est toujours un rectangle. En gros, l'image devient un
        /// losange qui sera contenu dans une zone rectangulaire orthonormée. Les zones contenues dans le rectangle mais à l'extérieur du losange seront transparentes.
        /// </summary>
        /// <param name="pointP1">Le point 1</param>
        /// <param name="pointP2">Le point 2</param>
        /// <param name="pointCenter">Optionnel. Un point "médian" précis. Normalement, le point médian est situé à la moitié de la distance entre les points 1 et 2, mais il est possible
        /// de vouloir un point médian de translation qui n'est pas calculé automatiquement. Si null, on calcule le point médian à partir du point 1 et 2.
        /// Le point médian ne sert qu'à la translation. La rotation et le dimensionnement sont toujours calculés à partir des point 1 et 2, même avec un point "médian" en paramètre.</param>
        /// <param name="relativeDistance">La distance relative (subjective) entre les deux points.</param>
        /// <param name="center">Le point central pour effectuer la translation. Si nul, on utilise le centre de l'image.</param>
        /// <param name="absoluteDistancePerPixel">La distance absolu par pixel que doit avoir l'image finale.</param>
        /// <param name="originalImage">L'image originale.</param>
        /// <param name="minimalAngle">L'angle minimal absolu pour effectuer la rotation. Peut être nul, ce qui signifit qu'il faut toujours faire la rotation.</param>
        /// <returns>L'image modifiée. Si un paramètre rend impossible le calcul, le retour sera nul.</returns>
        public static Bitmap NormalizeToStaticCoordinate(this Bitmap originalImage, Point pointP1, Point pointP2, Point? pointCenter,
                                                              double relativeDistance, Point? center,
                                                              double absoluteDistancePerPixel, double? minimalAngle)
        {
            if (!CheckIfValidBitmaps(originalImage)) return null;
            
            /* Étape 1: Angle et normalisation des points */
            //DebugNormalizeImageToStaticCoordinate(originalImage, @"C:\Users\prog\Documents\projets\2015\IRI-2015-01 Iris Pilote\tests\result\debug_01_original.png",
            //    new PointF(pointP1.X, pointP1.Y), new PointF(pointP2.X, pointP2.Y), center.HasValue ? new PointF(center.Value.X, center.Value.Y) : new PointF(0, 0),
            //      pointCenter.HasValue ? new PointF(pointCenter.Value.X, pointCenter.Value.Y) : new PointF(0, 0));

            // On calcule avant tout si le point 1 est à gauche du point 2, sinon on les inverse.
            PointF p1; // Variables que l'on peut modifier.
            PointF p2;
            PointF pc;
            double angleDegree;

            {
                if (pointP1.X > pointP2.X)
                {
                    p2 = new PointF(pointP1.X, pointP1.Y);
                    p1 = new PointF(pointP2.X, pointP2.Y);
                }
                else
                {
                    p1 = new PointF(pointP1.X, pointP1.Y);
                    p2 = new PointF(pointP2.X, pointP2.Y);
                }

                pc = pointCenter.HasValue
                         ? new PointF(pointCenter.Value.X, pointCenter.Value.Y)
                         : new PointF((p1.X + p2.X) / 2.0f, (p1.Y + p2.Y) / 2.0f);

                double v2X = pointP2.X - pointP1.X;
                double v2Y = pointP2.Y - pointP1.Y;
                // avec un angle comparé avec l'axe des X, l'équation de l'angle devient
                // tan(a) = |m|, où m est la pente de la ligne
                // On calcule m de cette façon
                var m = v2Y / v2X;
                // Donc on calcule l'angle a = atan(|m|)
                angleDegree = Math.Atan(Math.Abs(m));
                angleDegree = angleDegree * 180 / Math.PI; // Conversion en degrés en même temps.

                // On détermine maintenant si l'angle est négatif, car on utilise une pente absolue dans le dernier calcul
                // Si le point gauche est plus haut (donc, numériquement plus proche de zéro), on utilise un angle négatif.
                if (p1.Y < p2.Y) { angleDegree *= -1; }

                if (minimalAngle.HasValue)
                {
                    if (Math.Abs(angleDegree) < Math.Abs(minimalAngle.Value))
                    {
                        // L'angle est trop petite pour être considéré. On annule l'angle de rotation et on ajuste les points.
                        angleDegree = 0.0;
                        var newY = (int)((p1.Y + p2.Y) / 2);
                        p1.Y = newY;
                        p2.Y = newY;
                    }
                }
            }

            using (var rotated = originalImage.RotateToNewSize(angleDegree))
            {
                var newXdiff = (rotated.Width - originalImage.Width) / 2;
                var newYdiff = (rotated.Height - originalImage.Height) / 2;

                // Puis on applique une formule mathématique pour transformer ces points.
                var oldCenterOfImage = new PointF(originalImage.Width / 2.0f, originalImage.Height / 2.0f);
                if (angleDegree != 0.0)
                {
                    p1 = MathUtil.RotatePointWithArbitraryCenter(p1, oldCenterOfImage, angleDegree);
                    p2 = MathUtil.RotatePointWithArbitraryCenter(p2, oldCenterOfImage, angleDegree);
                    pc = MathUtil.RotatePointWithArbitraryCenter(pc, oldCenterOfImage, angleDegree);
                }

                //DebugNormalizeImageToStaticCoordinate(originalImage, @"C:\Users\prog\Documents\projets\2015\IRI-2015-01 Iris Pilote\tests\result\debug_02a_rotation.png",
                //_p1, _p2, _pc, _oldP1, _oldP2, _oldPC, oldCenterOfImage);

                // À partir de maintenant, comme la taille de l'image à changée, il faut prendre en considération les différences de taille.
                // Il ne s'agit que d'un simple déplacement vers le nouveau centre.
                var newCenterOfImage = new PointF(rotated.Width / 2.0f, rotated.Height / 2.0f);
                p1.X += newXdiff;
                p1.Y += newYdiff;
                p2.X += newXdiff;
                p2.Y += newYdiff;
                pc.X += newXdiff;
                pc.Y += newYdiff;

                //DebugNormalizeImageToStaticCoordinate(rotated, @"C:\Users\prog\Documents\projets\2015\IRI-2015-01 Iris Pilote\tests\result\debug_02b_rotation.png",
                //_p1, _p2, _pc, newCenterOfImage);

                /* Étape 2: dimensionnement. */

                double scale;
                {
                    // On évalue la distance entre le point 1 et le point 2, qui correspond à des "pixels".
                    // d = sqrt((pointP1.X-pointP2.X)*(pointP1.X-pointP2.X) + (pointP1.Y-pointP2.Y)*(pointP1.Y-pointP2.Y))
                    var d = Math.Sqrt((p1.X - p2.X) * (p1.X - p2.X) + (p1.Y - p2.Y) * (p1.Y - p2.Y));

                    // On prend ensuite la valeur de distance subjective en paramètre, avec la valeur de distance absolue par pixel.
                    // On fait une règle de trois pour savoir qu'elle est le ratio de dimension (scale) pour ramener la distance entre
                    // les deux points au même ratio de distance par pixel.
                    scale = relativeDistance / (d * absoluteDistancePerPixel);
                }

                /* Étape 3: calcul de la Translation */

                // IL faut penser ici que le point central ne bouge pas même avec les informations de dimensionnement, mais les deux points de
                // calcul si.
                PointF translation;
                {
                    // On évalue le point central
                    var centerFloat = !center.HasValue
                                          ? newCenterOfImage
                                          : new PointF(center.Value.X + newXdiff, center.Value.Y + newYdiff);

                    // On modifie le point médian
                    pc.X = (float)(pc.X * scale);
                    pc.Y = (float)(pc.Y * scale);

                    // On calcule la différence (translation) entre les points.
                    translation = new PointF(centerFloat.X - pc.X, centerFloat.Y - pc.Y);

                    //DebugNormalizeImageToStaticCoordinate(rotated, @"C:\Users\prog\Documents\projets\2015\IRI-2015-01 Iris Pilote\tests\result\debug_03_translation.png",
                    //    _pc, new PointF(_pc.X + translation.X, _pc.Y + translation.Y));
                }

                /* Étape 4: translation et redimensionnement */

                var pixelFormatDefault = PixelFormat.Format64bppArgb;
                if (originalImage.PixelFormat == PixelFormat.Format64bppArgb) { pixelFormatDefault = PixelFormat.Format64bppArgb; }

                var resultat = new Bitmap(rotated.Width, rotated.Height, pixelFormatDefault);
                resultat.SetResolution(originalImage.HorizontalResolution, originalImage.VerticalResolution);
                using (var g = Graphics.FromImage(resultat))
                {
                    var rect = new RectangleF(translation.X, translation.Y, (float)(rotated.Width * scale), (float)(rotated.Height * scale));
                    g.DrawImage(rotated, rect);
                }

                return resultat;
            }
        }

        /// <summary>
        /// Méthode de débogage pour la très complexe méthode de normalisation à coordonnées.
        /// </summary>
        /// <param name="originalImage">L'image à analyser.</param>
        /// <param name="file">Le fichier de sortie.</param>
        /// <param name="points">Les points à afficher sur l'image.</param>
        // ReSharper disable once UnusedMember.Local
#pragma warning disable IDE0051 // Remove unused private members
        private static void DebugNormalizeImageToStaticCoordinate(Bitmap originalImage, string file, params PointF[] points)
#pragma warning restore IDE0051 // Remove unused private members
        {
            if (!CheckIfValidBitmaps(originalImage)) return;
            
            using (var copy = new Bitmap(originalImage))
            {
                using (var g = Graphics.FromImage(copy))
                {
                    Color[] cols = {
                        Color.Red, Color.Salmon, Color.Blue, Color.Azure, Color.BlanchedAlmond,
                        Color.FloralWhite, Color.Brown, Color.RosyBrown
                    };
                    var i = 0;
                    foreach (var p in points)
                    {
                        var pen = new Pen(cols[i], 10);
                        g.DrawEllipse(pen, new RectangleF(p.X, p.Y, 1, 1));
                        i++;
                        if (i >= cols.Length) { i = 0; }
                    }
                }

                Save(copy, ImageFormat.Png, 9, file);
            }
        }
        
        /// <summary>
        /// Cette méthode redimensionne et élague une image pour maximiser l'espace pris dans une nouvelle dimension. En gros, cette méthode élimine le
        /// besoin d'une "bande noire" pour entrer une image en ajustant l'image à sa plus petite dimension puis en élagant les côtés de sa plus grande dimension, selon
        /// son orientation et en préservant son ratio.
        /// <br/>On a également l'option de spécifier un rectangle d'affichge pour forcer l'affichage de l'image originale dans un plus petit rectangle, a fins de composition d'image.
        /// Si spécifié, on créé un canevas de la taille désirée puis on constraint l'image à ce rectangle, ce qui peut provoquer une distortion.
        /// <br/>On ajoute par la suite, si non nulle, une image en superposition. Avec un autre paramètre optionnel, on peut dessiner cette image ailleurs qu'au point (0,0)
        /// </summary>
        /// <param name="original">L'image originale</param>
        /// <param name="desiredWidth">La largeur souhaitée</param>
        /// <param name="desiredHeight">La hauteur souhaitée.</param>
        /// <param name="overlay">Optionnel. L'image de superposition.</param>
        /// <param name="overlayPosition">Optionnel. La position de la superposition.</param>
        /// <param name="originalRectangle">Rectangle de contrainte pour l'image originale. En gros, où et de quelle taille on veut mettre l'image originale dans le canevas.</param>
        /// <returns>Une image centrée, redimensionnée, et élaguée.</returns>
        public static Image ResizeForView(this Image original, int desiredWidth, int desiredHeight, Bitmap overlay = null, Point? overlayPosition = null, 
            Rectangle? originalRectangle = null)
        {
            if (!CheckIfValidBitmaps(original)) return null;

            Bitmap resultBmp;

            if (originalRectangle == null)
            {
                // Cas de base. Les dimensions sont exactement celles recherchée et donc pas de transformation n'est requise.
                if (original.Height != desiredHeight || original.Width != desiredWidth)
                {
                    resultBmp = Frame(original, desiredWidth, desiredHeight);
                }
                else
                {
                    resultBmp = new Bitmap(original);
                }
            }
            else
            {
                if (originalRectangle.Value.Width < 2 || originalRectangle.Value.Height < 2) return null;

                resultBmp = new Bitmap(desiredWidth, desiredHeight, PixelFormat.Format24bppRgb);
                resultBmp.SetResolution(original.HorizontalResolution, original.VerticalResolution);

                using (var g = Graphics.FromImage(resultBmp))
                {
                    using (var resizedImage = Frame(original, originalRectangle.Value.Width, originalRectangle.Value.Height))
                    {
                       g.DrawImage(resizedImage, originalRectangle.Value ,0 ,0 ,resizedImage.Width,resizedImage.Height,GraphicsUnit.Pixel);
                    }
                }
            }

            if (overlay == null) return resultBmp;

            // On fait la superposition ici.
            var x = overlayPosition?.X ?? 0;
            var y = overlayPosition?.Y ?? 0;
            AddOverlay(resultBmp, overlay, x, y);

            return resultBmp;
        }
        
        /// <summary>
        /// Ajoute du texte, avec une rotation possible.
        /// On passe un font complet pour cette étape.
        /// </summary>
        /// <param name="image">L'image réceptrice de la ligne de texte.</param>
        /// <param name="textToAdd">Le texte à ajouter.</param>
        /// <param name="x">La position de base X</param>
        /// <param name="y">La position de base Y</param>
        /// <param name="center">Indique si on centre le texte. Si oui, le point de rotation sera le centre de l'image créée. Sinon, le point en haut à gauche.</param>
        /// <param name="brush">Le pinceau à utiliser pour dessiner les caractères.</param>
        /// <param name="font">Le font complet à écrire.</param>
        /// <param name="rotation">Optionel. Le nombre de degré en rotation.</param>
        /// <param name="maxLenght">Optionnel. La largeur maximale avant de rendre le texte. S'il dépasse, il sera divisé par ligne, entre les mots.</param>
        /// <param name="multiplineOffsetY">Optionel. La distance entre chaque ligne de texte, si séparé. 0 sinon (ne donne pas de bons résultats).</param>
        public static void AddTextToBitmap(this Bitmap image, string textToAdd, float x, float y, bool center,
            Brush brush, Font font, float? rotation = null, int? maxLenght = null, float multiplineOffsetY = 0)
        {
            // Ici, il faut créer l'image de texte à part pour faire une rotation par la suite.

            using (var g = Graphics.FromImage(image))
            {
                if (rotation != null) { g.RotateTransform(rotation.Value); }

                if (maxLenght != null)
                {
                    var lignes = SplitStringForLineRendering(maxLenght.Value, textToAdd, g, font);
                    
                    var lineSizes = new SizeF[lignes.Count];

                    // Calcul de la taille des lignes.
                    for(var i = 0; i < lignes.Count; i++)
                    {
                        lineSizes[i] = g.MeasureString(lignes[i], font);
                    }

                    var maxHeight = lineSizes.Sum(l => l.Height) + ((lignes.Count - 1) * multiplineOffsetY);
                    var yPos = center ? y - (maxHeight / 2) : 0;

                    for (var i = 0; i < lignes.Count; i++)
                    {
                        var size = lineSizes[i];
                        g.DrawString(lignes[i], font, brush, x - (size.Width / 2.0f), yPos);
                        yPos += size.Height + multiplineOffsetY;
                    }
                }
                else
                {
                    var size = new SizeF(0, 0);
                    if (center) { size = g.MeasureString(textToAdd, font); }
                    g.DrawString(textToAdd, font, brush, x - (size.Width / 2.0f), y - (size.Height / 2.0f));
                }

                g.ResetTransform();
            }
        }
        
        /// <summary>
        /// Divise une String par ligne selon une taille maximale en pixel. Utilisée pour afficher correctement une longue ligne sur une image.
        /// <br/>La division se fait par mot (et donc divisé par espaces). Ne tient pas compte des situations extrêmes (aucun espace, trop d'espace).
        /// </summary>
        /// <param name="maxWidth">Taille maximale, en pixels, que doit avoir une ligne.</param>
        /// <param name="originale">Le string original à diviser.</param>
        /// <param name="graphics">L'outil de graphisme GDI+ qui doit faire le rendu final des chaînes.</param>
        /// <param name="police">La police qui sera utilisé pour le rendu.</param>
        /// <returns>Une liste de string représentant les lignes. Peut être vide si String originale est invalide. S'il n'y a pas eu de division
        /// la liste ne contiendra qu'un élément, qui sera similaire (mais peut-être non-égale) à la chaîne originale.</returns>
        public static List<string> SplitStringForLineRendering(int maxWidth, string originale,[NotNull] Graphics graphics, Font police)
        {
            var newLine = new StringBuilder();
            var reponse = new List<string>();

            if (string.IsNullOrWhiteSpace(originale)) return reponse;

            var widthTotal = (int)graphics.MeasureString(originale, police).Width;
            if (widthTotal > maxWidth)
            {
                var words = originale.Split(' ');

                var width = 0;
                var i = 0;
                while (i < words.Length)
                {
                    while (i < words.Length && width < maxWidth)
                    {
                        newLine.Append(words[i]);
                        // si on est pas le dernier mot
                        if (i < words.Length - 1)
                        {
                            newLine.Append(' ');
                            width = (int)graphics.MeasureString(newLine + words[i + 1], police).Width;
                        }
                        i++;
                    }
                    reponse.Add(newLine.ToString());
                    // si on est pas le dernier mot
                    // OU qu'on est le dernier mot, mais qu'il n'a pas été ajouté
                    // par manque d'espace.
                    if (i < words.Length - 1 || width > maxWidth) { newLine.Clear(); }
                    width = 0;
                }
            }
            else { reponse.Add(originale); }
            return reponse;
        }
        
        /// <summary>
        /// Trouve l'encodeur pour un type de fichier précis. Retourne NULL si le type est inconnu.
        /// </summary>
        /// <param name="mimeType">Le type de fichier.</param>
        /// <returns>Un encodeur, s'il existe sur la machine présente.</returns>
        public static ImageCodecInfo GetEncoder(string mimeType)
        {
            int j;
            var encoders = ImageCodecInfo.GetImageEncoders();
            for (j = 0; j < encoders.Length; ++j)
            {
                if (encoders[j].MimeType == mimeType)
                    return encoders[j];
            }
            return null;
        }

        /// <summary>
        /// Arrange une image via ses métadonnée EXIF. Ouvre, et si les EXIFS impliquent une rotation, on va écraser
        /// le fichier original avec une nouvelle version qui contient les données de pixels dans la bonne orientation.
        /// Référence: http://www.codeproject.com/tips/64116/Using-GDIplus-code-in-a-WPF-application-for-lossle.aspx
        /// voir aussi: http://csharphelper.com/blog/2016/07/read-an-image-files-exif-orientation-data-in-c/
        /// </summary>
        /// <param name="filename">Le fichier à transformer via son EXIF.</param>
        public static void MatchExifData(string filename)
        {
            var info = new FileInfo(filename);
            if (!info.Exists || info.Length < 10 || !(info.Name.EndsWith(".jpg", StringComparison.InvariantCultureIgnoreCase) || info.Name.EndsWith(".jpeg", StringComparison.InvariantCultureIgnoreCase)))
            {
                return;
            }

            var enc = Encoder.Transformation;
            var encParms = new EncoderParameters(1);
            var codecInfo = GetEncoder("image/jpeg");

            // We cannot store into the same image, so use a temporary image instead.
            var filenameTemp = DirectoryUtil.CreateLocalTemporaryFile();

            // Load the image to rotate.
            try
            {
                using (var image = new Bitmap(filename))
                {
                    EncoderValue encoderValue;
                    
                    var exirRot = image.GetExifRotationValue();
                    switch (exirRot)
                    {
                        // 3 -> 180
                        case ExifOrientations.BottomRight:
                            encoderValue = EncoderValue.TransformRotate180;
                            break;
                        // 6 -> 90
                        case ExifOrientations.RightTop:
                            encoderValue = EncoderValue.TransformRotate90;
                            break;
                        // 8 -> 270
                        case ExifOrientations.LeftBottom:
                            encoderValue = EncoderValue.TransformRotate270;
                            break;
                        default: return;
                    }

                    // We're correcting the orientation, so set it to 1.
                    var propertyItems = image.PropertyItems;
                    propertyItems[0].Id = 0x0112; // 0x0112 as specified in EXIF standard
                    propertyItems[0].Type = 3;
                    propertyItems[0].Len = 2;
                    var orientation = new byte[2];
                    orientation[0] = 1; // Little-endian
                    orientation[1] = 0;
                    propertyItems[0].Value = orientation; // Valeur TopLeft, c'est à dire aucune rotation.
                    image.SetPropertyItem(propertyItems[0]);

                    // For lossless rewriting, we must rotate the image by 90 degree increments.
                    var encParm = new EncoderParameter(enc, (long) encoderValue);
                    encParms.Param[0] = encParm;

                    // Now write the rotated image with Orientation metadata set to 1.
                    image.Save(filenameTemp, codecInfo, encParms);
                }
            }
            catch (Exception e)
            {
                log.Error(e, "Error encountered when matching EXIF data for file. No modification was made. File: " + filename);
                return;
            }

            // Release memory now, since we could be doing a bunch of these.
            GC.Collect();

            // Delete the original file, which will be replaced below.
            File.Delete(filename);

            // Replace the original file with the rotated file.
            File.Move(filenameTemp, filename);

            // Delete the temporary image.
            File.Delete(filenameTemp);
        }

        /// Lit et retourne la rotation EXIF lue par le système, s'il en existe une.
        /// Si le fichier n'a aucune métadonnée EXIF pour la rotation, on retourne un code de notre cru, soit None.
        public static ExifOrientations GetExifRotationValue(this Image img)
        {
            var propertyItems = img.PropertyItems;

            // Find the metadata (Ajout François)
            var rotData = propertyItems.FirstOrDefault(pi => pi.Id == 0x0112);

            if (rotData == null)
            {
                return ExifOrientations.None; // Does not have EXIF rotation metadata
            }

            var by = rotData.Value;
            return (ExifOrientations)by[0];
        }
        
        /// <summary>
        /// Dessine un groupe d'image sur une seule image réceptrice.
        /// On utilise des valeurs en pourcentage pour la séparation des images, pour égaliser tous les espaces, à partir du haut et de la gauche.
        /// </summary>
        /// <param name="pictures">La liste de fichiers d'images à insérer.</param>
        /// <param name="percentageOffset">Le pourcentage de base. Est Optionnel. Valeur par défaut: 0.05</param>
        /// <param name="nbrColumn">Nombre de colonnes pour séparer les images.</param>
        /// <param name="extraTopOffset">En pixels, un déplacement additionnel pour le groupe d'images à partir du haut.</param>
        /// <param name="extraSideOffset">En pixels, un déplacement additionnel pour le groupe d'images à partir de la gauche.</param>
        /// <param name="frame">L'image de base sur laquelle coller toutes les images résultantes.</param>
        /// <param name="frameOverPicture">Si vrai, indique qu'il faut dessiner le cadre par-dessus les images. (Inverse le "Z-Order", en parlance 3D).</param>
        /// <param name="backgroundColor">Si on dessine le cadre par-dessus, on peut désigner une couleur pour remplir le canevas avant d'y dessiner les images.</param>
        /// <param name="subImageRotation">Effectue une rotation et/ou inversion d'axe sur les images à placer. Nul par défaut.</param>
        public static Bitmap PutMultiplePictureOnFrame(this Bitmap frame, IList<string> pictures, float? percentageOffset, int nbrColumn,
            int extraTopOffset, int extraSideOffset, bool frameOverPicture = false, Color? backgroundColor = null, RotateFlipType? subImageRotation = null)
        {
            if (!CheckIfValidBitmaps(frame)) return null;
            
            var shortestDimension = Math.Min(frame.Width, frame.Height);

            var offset = (int)(shortestDimension * (percentageOffset ?? 0.05F));
            var midOffset = (int)(offset / (double)2);
            var imageSize = shortestDimension - (2 * offset);

            var dest = new Bitmap(frame.Width, frame.Height, PixelFormat.Format24bppRgb);
            dest.SetResolution(300, 300);
            using (var g = Graphics.FromImage(dest))
            {
                if (!frameOverPicture)
                {
                    g.DrawImage(frame, new Rectangle(0, 0, frame.Width, frame.Height), 0, 0, frame.Width, frame.Height, GraphicsUnit.Pixel); // Cadre en-dessous
                }
                else if (backgroundColor.HasValue)
                {
                    g.FillRectangle(new SolidBrush(backgroundColor.Value), 0, 0, frame.Width, frame.Height); //  Couleur optionnelle
                }

                var count = 0;
                var row = 0;
                var col = 0;

                var sizeX = (imageSize - (midOffset * (nbrColumn - 1))) / nbrColumn;

                foreach (var fileName in pictures)
                {
                    using (var image = FileUtil.LoadBitmap(fileName))
                    {
                        using (var b = image.Frame(sizeX, sizeX))
                        {
                            if (subImageRotation != null) b.RotateFlip(subImageRotation.Value);

                            g.DrawImage(b, new PointF(
                                offset + extraSideOffset + ((col * b.Width) + (col * midOffset)),
                                offset + extraTopOffset + ((row * b.Height) + (row * midOffset))));
                        }
                    }

                    count++;
                    col = (count % nbrColumn);
                    if (col == 0) { row++; }
                }

                if (frameOverPicture)
                {
                    g.DrawImage(frame, new Rectangle(0, 0, frame.Width, frame.Height), 0, 0, frame.Width, frame.Height, GraphicsUnit.Pixel); // Frame par-dessus
                }
            }

            return dest;
        }
        
        /// <summary>
        /// Effectue une opération matricielle de redimensionnement sur l'image, à partir du centre.
        /// On stocke le résultat dans une nouvelle instance, qui à les mêmes informations d'image (taille, résolution, format).
        /// </summary>
        /// <param name="image">L'image originale.</param>
        /// <param name="scaleFactorX">Le facteur de dimentionnement en X</param>
        /// <param name="scaleFactorY">Le facteur de dimensionnement en Y</param>
        /// <returns>Une nouvelle instance contenant le résultat.</returns>
        public static Bitmap Scale(this Image image, double scaleFactorX, double scaleFactorY)
        {
            return !CheckIfValidBitmaps(image) ? null : Scale(image, scaleFactorX, scaleFactorY, image.Width / 2f, image.Height / 2f);
        }

        /// <summary>
        /// Effectue une opération matricielle de redimensionnement sur l'image, à partir du centre.
        /// On stocke le résultat dans une nouvelle instance, qui à les mêmes informations d'image (taille, résolution, format).
        /// </summary>
        /// <param name="image">L'image originale.</param>
        /// <param name="scaleFactor">Le facteur de dimentionnement général</param>
        /// <returns>Une nouvelle instance contenant le résultat.</returns>
        public static Bitmap Scale(this Image image, double scaleFactor)
        {
            return !CheckIfValidBitmaps(image) ? null : Scale(image, scaleFactor, scaleFactor, image.Width / 2f, image.Height / 2f);
        }

        /// <summary>
        /// Effectue une opération matricielle de redimensionnement sur l'image, à partir d'un point arbitraire.
        /// On stocke le résultat dans une nouvelle instance, qui à les mêmes informations d'image (taille, résolution, format).
        /// </summary>
        /// <param name="image">L'image originale.</param>
        /// <param name="scaleFactor">Le facteur de dimentionnement général</param>
        /// <param name="x">Le point central X</param>
        /// <param name="y">Le point central Y</param>
        /// <returns>Une nouvelle instance contenant le résultat.</returns>
        public static Bitmap Scale(this Image image, double scaleFactor, float x, float y)
        {
            return !CheckIfValidBitmaps(image) ? null :  Scale(image, scaleFactor, scaleFactor, x, y);
        }

        /// <summary>
        /// Effectue une opération matricielle de redimensionnement sur l'image, à partir d'un point arbitraire.
        /// On stocke le résultat dans une nouvelle instance, qui à les mêmes informations d'image (taille, résolution, format).
        /// </summary>
        /// <param name="image">L'image originale.</param>
        /// <param name="scaleFactorX">Le facteur de dimentionnement en X</param>
        /// <param name="scaleFactorY">Le facteur de dimensionnement en Y</param>
        /// <param name="x">Le point central X</param>
        /// <param name="y">Le point central Y</param>
        /// <returns>Une nouvelle instance contenant le résultat.</returns>
        public static Bitmap Scale(this Image image, double scaleFactorX, double scaleFactorY, float x, float y)
        {
            if (!CheckIfValidBitmaps(image)) return null;
            
            //create an empty Bitmap image
            var bmp = new Bitmap(image.Width, image.Height);
            bmp.SetResolution(image.HorizontalResolution, image.VerticalResolution);

            //turn the Bitmap into a Graphics object
            var gfx = Graphics.FromImage(bmp);

            //now we set the central point to the center of our image
            gfx.TranslateTransform(x, y);

            //now scale the image
            gfx.ScaleTransform((float)scaleFactorX, (float)scaleFactorY);

            gfx.TranslateTransform(-x, -y);

            //set the InterpolationMode to HighQualityBicubic so to ensure a high
            //quality image once it is transformed to the specified size
            gfx.InterpolationMode = InterpolationMode.HighQualityBicubic;

            //now draw our new image onto the graphics object
            gfx.DrawImage(image, new Point(0, 0));

            //dispose of our Graphics object
            gfx.Dispose();

            //return the image
            return bmp;
        }
        
        /// <summary>
        /// Modifie une image source pour élaguer un minimum tout en remplissant le
        /// résultat final au maximum, sans espace vide, en gardant le ratio de pixel de la source. Selon la différence entre les tailles,
        /// cela peut provoquer un agrandissement ou un rappetissement de l'image source.
        /// Si le ratio largeur/hauteur entre les deux n'est pas le même, il y aura automatiquement un élaguage d'une partie.
        /// </summary>
        /// <param name="image">L'image à modifier.</param>
        /// <param name="width">La largeur du canevas.</param>
        /// <param name="height">La hauteur du canevas.</param>
        /// <returns>L'image modifié résultante.</returns>
        public static Bitmap Frame(this Image image, int width, int height)
        {
            if (!CheckIfValidBitmaps(image)) return null;

            var result = new Bitmap(width, height, PixelFormat.Format32bppArgb);
            result.SetResolution(image.HorizontalResolution, image.VerticalResolution);
            if (image.Width == width && image.Height == height)
            {
                DrawCroppedBitmap(image, width, height, result);
                return result;
            }

            var factor = Math.Max(width / (float) image.Width, height / (float) image.Height);

            var newWidth = (int) (image.Width * factor);
            var newHeight = (int) (image.Height * factor);

            var newBitmap = Resize(image, newWidth, newHeight);
            // Ne pas faire Dispose() sur l'argument "image"!
            image = newBitmap;

            DrawCroppedBitmap(image, width, height, result);

            return result;
        }
        
        /// <summary>
        /// Redimensionne une image dans un cadre d'une dimension fixe. Si le ratio du nouveau cadre est différent, l'image est redimensionnée dans ce cadre, puis centrée
        /// de façon à ne rien perdre de l'image orignale. Cela va provoquer l'ajout de bandes transparentes (si le format de pixel permet la transparence) ou blanches.
        /// Le fichier d'image original n'est pas touché, alors on fournit en retour une référence vers la nouvelle image.
        /// </summary>
        /// <param name="original">L'image originale</param>
        /// <param name="width">La nouvelle largeur voulue.</param>
        /// <param name="height">La nouvelle hauteur voulue.</param>
        /// <param name="format">Force un format d'image spécifique sur l'instance de retour. Si null, utilise le format du fichier original.</param>
        /// <param name="action">Action supplémentaire à exécuter avec l'objet de dessin à la fin de l'opération.</param>
        /// <returns>Une référence vers une nouvelle image, avec la taille spécifiée.</returns>
        public static Bitmap Resize(this Image original, int width, int height, PixelFormat? format = null, Action<Graphics> action = null)
        {
            if (!CheckIfValidBitmaps(original)) return null;
            
            if (width == 0 || height == 0)
            {
                return null;
            }

            if (original.Width == width && original.Height == height &&
                (format?.Equals(original.PixelFormat) ?? true))
            {
                var result = new Bitmap(original);
                result.SetResolution(original.HorizontalResolution, original.VerticalResolution);
                return result;
            }

            var sourceWidth = original.Width;
            var sourceHeight = original.Height;

            var nPercentW = (width / (float) sourceWidth);
            var nPercentH = (height / (float) sourceHeight);

            var nPercent = nPercentH < nPercentW
                ? nPercentH
                : nPercentW;

            var destWidth = (int) Math.Ceiling(sourceWidth * nPercent);
            var destHeight = (int) Math.Ceiling((sourceHeight * nPercent));
            var destX = (width - destWidth) / 2;
            var destY = (height - destHeight) / 2;

            var b = new Bitmap(width, height, format ?? original.PixelFormat);
            b.SetResolution(original.HorizontalResolution, original.VerticalResolution);

            using (var g = Graphics.FromImage(b))
            {
                g.CompositingQuality = CompositingQuality.HighQuality;
                g.InterpolationMode = InterpolationMode.HighQualityBicubic;
                g.SmoothingMode = SmoothingMode.HighQuality;

                g.DrawImage(original, destX, destY, destWidth, destHeight);

                action?.Invoke(g);
            }

            return b;
        }
        
        /// <summary>
        /// Ajoute une image par dessus une autre, en prenant le alpha de l'image originale comme masque.
        /// En gros, chaque pixel de l'image résultante contiendra l'alpha de l'image original, mais le mélange RGB
        /// sera fait en composition des valeurs complètes (ARGB) de chaque pixel de chaque image.
        /// Retourne null si les images ne sont pas exactement de la même taille, ou ne sont pas pas toute deux du format
        /// 32ARGB.
        /// </summary>
        /// <param name="original">L'image originale.</param>
        /// <param name="overlay">L'image en superposition.</param>
        /// <returns>Une référence vers l'image résultante, null si il est impossible de faire la composition.</returns>
        public static Bitmap AddOverlayWithOriginalMask(this Bitmap original, Bitmap overlay)
        {
            if (!CheckIfValidBitmaps(original, overlay)) return null;
            
            var fmt1 = original.PixelFormat;
            var fmt2 = overlay.PixelFormat;

            if (fmt1 != PixelFormat.Format32bppArgb) return null;
            if (fmt2 != PixelFormat.Format32bppArgb) return null;

            var s1 = original.Size;
            var s2 = overlay.Size;
            if (s1 != s2) return null;

            var bmp3 = new Bitmap(s1.Width, s1.Height, PixelFormat.Format32bppArgb);

            var rect = new Rectangle(0, 0, s1.Width, s1.Height);

            var bmp1Data = original.LockBits(rect, ImageLockMode.ReadOnly, fmt1);
            var bmp2Data = overlay.LockBits(rect, ImageLockMode.ReadOnly, fmt2);
            var bmp3Data = bmp3.LockBits(rect, ImageLockMode.ReadWrite, PixelFormat.Format32bppArgb);

            var size1 = bmp1Data.Stride * bmp1Data.Height;
            var size2 = bmp2Data.Stride * bmp2Data.Height;
            var size3 = bmp3Data.Stride * bmp3Data.Height;
            var data1 = new byte[size1];
            var data2 = new byte[size2];
            var data3 = new byte[size3];
            Marshal.Copy(bmp1Data.Scan0, data1, 0, size1);
            Marshal.Copy(bmp2Data.Scan0, data2, 0, size2);
            Marshal.Copy(bmp3Data.Scan0, data3, 0, size3);

            for (var y = 0; y < s1.Height; y++)
            {
                for (var x = 0; x < s1.Width; x++)
                {
                    var index1 = y * bmp1Data.Stride + x * 4;
                    var index2 = y * bmp2Data.Stride + x * 4;
                    var index3 = y * bmp3Data.Stride + x * 4;

                    /* http://stackoverflow.com/questions/9014729/manually-alpha-blending-an-rgba-pixel-with-an-rgb-pixel */

                    //var fa = (data1[index1 + 3] / byte.MaxValue) * (data2[index2 + 3] / byte.MaxValue); // Foreground Alpha
                    var fa = data2[index2 + 3] / (double)byte.MaxValue;
                    var rfa = 1.0 - fa; // Reverse Foreground Alpha
                    data3[index3 + 0] = (byte)((data2[index2 + 0] * fa) + (data1[index1 + 0] * rfa));
                    data3[index3 + 1] = (byte)((data2[index2 + 1] * fa) + (data1[index1 + 1] * rfa));
                    data3[index3 + 2] = (byte)((data2[index2 + 2] * fa) + (data1[index1 + 2] * rfa));
                    data3[index3 + 3] = data1[index1 + 3];
                }
            }

            Marshal.Copy(data3, 0, bmp3Data.Scan0, data3.Length);
            original.UnlockBits(bmp1Data);
            overlay.UnlockBits(bmp2Data);
            bmp3.UnlockBits(bmp3Data);
            return bmp3;
        }
        
        /// <summary>
        /// Créée une copie d'image et dessine un gros 'X' en rouge sur ses diagonales. Le résultat n'a pas de transparence.
        /// </summary>
        /// <param name="bmp">L'image source.</param>
        /// <returns>Une référence vers le résultat.</returns>
        public static Bitmap DebugDisplayCrossOnImage(this Bitmap bmp)
        {
            if (CheckIfValidBitmaps(bmp)) return null;

            var res = new Bitmap(bmp.Width, bmp.Height, PixelFormat.Format24bppRgb);

            using (var g = Graphics.FromImage(res))
            {
                g.DrawImageUnscaled(bmp, 0, 0, bmp.Width, bmp.Height);
                g.DrawLines(new Pen(Color.Red), new[]
                {
                    new PointF(0, 0),
                    new PointF(bmp.Width, bmp.Height),
                    new PointF(0, bmp.Height),
                    new PointF(bmp.Width, 0),
                });
            }

            return res;
        }
        
        
    }

    
}