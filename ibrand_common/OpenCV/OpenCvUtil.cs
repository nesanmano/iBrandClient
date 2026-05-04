using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Drawing.Imaging;
using System.IO;
using System.Linq;
using System.Threading;
using Emgu.CV;
using Emgu.CV.CvEnum;
using Emgu.CV.Structure;
using Emgu.CV.Util;
using NLog;
using NomadLogic.iBrand.Common.Util;
using NomadLogic.iBrand.Common.Util.Math;
using NomadLogic.iBrand.Transformation.OpenCV.TransformFromJson;
using FileBundleNormalizationTransforms = NomadLogic.iBrand.Transformation.OpenCV.TransformFromJson.FileBundleNormalizationTransforms;
using GlobalParameters = NomadLogic.iBrand.Transformation.OpenCV.TransformFromJson.GlobalParameters;
using PictureTransform = NomadLogic.iBrand.Transformation.OpenCV.TransformFromJson.PictureTransform;

namespace NomadLogic.iBrand.Transformation.OpenCV
{
    /// <summary>
    /// Cette classe contient des méthode pratiques pour effectuer des opérations simple ou complexes en utilisant la librairie de traitement graphique OpenCV.
    /// </summary>
    public abstract class OpenCvUtil
    {
        private static readonly Logger log = LogManager.GetCurrentClassLogger();
        private const double TOLERANCE = 0.001;

        /// <summary>
        /// Calcule les transformations nécessaires pour normaliser une série d'images à partir de deux points d'intérêt.
        /// Si retourne null, c'est que les paramètres d'entrés sont invalides.
        /// </summary>
        /// <param name="inputs">La liste des points d'intérêt à aligner. Doit absoluement contenir ni plus ni moins que deux points.</param>
        /// <param name="files">Les fichiers sur lesquels effectuer la transformations. Doit contenir au moins deux fichiers.</param>
        /// <param name="disableRotationScaling">Si vrai, ne calcule que les informations de translation et de taille optimale.</param>
        /// <param name="token">Jeton de cancellation pour les invocation asynchrone.</param>
        /// <param name="progressAction">Méthode de callback pour informer un UI du progrès.</param>
        /// <returns>Si non-nul, les transformations nécessaire pour modifier les fichiers, dans l'ordre donnés en paramètres.</returns>
        public static FileBundleNormalizationTransforms CalculateNormalizationTransforms(List<CoordinatesInput> inputs, List<string> files, bool disableRotationScaling = false,
            CancellationToken? token = null, Action<int> progressAction = null)
        {
            // Prend deux coordonnées, ni plus ni moins. Représente aussi le nombre de templates.
            const int INPUTS_NBR = 2;

            if (inputs == null || inputs.Count != INPUTS_NBR || files == null || files.Count < INPUTS_NBR) return null;

            var vTemplates = new VectorOfMat();
            var firstImage = true;
            var vecResultX = new VectorOfInt();
            var vecResultY = new VectorOfInt();
            var vecTags = new List<string>();
            var firstImageSize = new Size();

            var nbr = 0;
            foreach (var file in files)
            {
                if (token?.IsCancellationRequested ?? false) break;

                ImageUtil.RotateJpegToMatchExifData(file);

                vecTags.Add(StringUtil.RemoveExtension(StringUtil.GetFileName(file)));
                var imageFromFile = new Image<Bgr, byte>(file);
                var image = CvInvoke.CvArrToMat(imageFromFile);

                var imageChannels = new VectorOfMat(3);
                CvInvoke.Split(image, imageChannels);

                var imgR = new Mat();
                var imgG = new Mat();
                var diffRg = new Mat();
                const int ksize = 11;
                const float fSigma = 3.0f;
                CvInvoke.GaussianBlur(imageChannels[0], imgR, new Size(ksize, ksize), fSigma, fSigma);
                CvInvoke.GaussianBlur(imageChannels[1], imgG, new Size(ksize, ksize), fSigma, fSigma);
                CvInvoke.Subtract(imgR, imgG, diffRg);

                diffRg = imgG; // Assigne le header de imgG à diffRg, sans changer le contenu de la matrice résultante.

                // Si première image, initialisation des Matrices de comparaisons, basées sur le contenu de la première image.
                if (firstImage)
                {
                    firstImage = false; // Pour ne plus entrer ici.
                    firstImageSize = new Size(image.Cols, image.Rows);

                    for (var j = 0; j < INPUTS_NBR; j++)
                    {
                        vTemplates.Push(new Mat(diffRg, inputs[j].Zone));
                    }
                }
                else
                {
                    if (image.Cols != firstImageSize.Width || image.Rows != firstImageSize.Height)
                    {
                        log.Warn($"Calculation of normalization file aborted because not all files are the same size. First offender: {file}");
                        return null;
                    }
                }

                for (var j = 0; j < INPUTS_NBR; j++)
                {
                    // Pré-calculs
                    var tX = inputs[j].Zone.X;
                    var tY = inputs[j].Zone.Y;
                    var tW = inputs[j].Zone.Width;
                    var tH = inputs[j].Zone.Height;
                    var tR = inputs[j].Radius;
                    var tR2 = (int)tR / 2;
                    var tRi = (int)tR;

                    // Créer une sous-image qui représente la zone de recherche.
                    var subimage = new Mat(diffRg, new Rectangle(tX - tR2, tY - tR2, tRi, tRi));

                    var result = new Mat(tRi - inputs[j].Zone.Width, tRi - inputs[j].Zone.Height, DepthType.Cv32F, 1);
                    CvInvoke.MatchTemplate(subimage, vTemplates[j], result, TemplateMatchingType.Sqdiff);
                    CvInvoke.Normalize(result, result, 0, 1, NormType.MinMax);

                    var minVal = 0.0;
                    var maxVal = 0.0;
                    var minLoc = new Point();
                    var maxLoc = new Point();
                    CvInvoke.MinMaxLoc(result, ref minVal, ref maxVal, ref minLoc, ref maxLoc, new Mat());
                    maxLoc = minLoc;

                    result.Dispose();
                    subimage.Dispose();

                    var matchLocX = maxLoc.X + tX - tR2;
                    var matchlocY = maxLoc.Y + tY - tR2;
                    var matchLoc = new Rectangle(matchLocX, matchlocY, matchLocX + tW, matchlocY + tH);

                    vecResultX.Push(new[] { matchLoc.X + tW / 2 });
                    vecResultY.Push(new[] { matchLoc.Y + tH / 2 });
                }

                diffRg.Dispose();
                image.Dispose();
                imageFromFile.Dispose();

                nbr++;
                progressAction?.Invoke(nbr);
            }

            if (token?.IsCancellationRequested ?? false) return null;

            // Création de la classe de résultat.
            var transforms = new FileBundleNormalizationTransforms
            {
                GlobalParameters = new GlobalParameters(),
                PicturesTransform = new List<PictureTransform>()
            };

            var originalVec = new Point();
            double originalVecLength = 1;
            var originalMidPoint = new Point();

            for (var i = 0; i < vecResultX.Size; i += INPUTS_NBR)
            {
                var currentVec = new Point(vecResultX[i + 1] - vecResultX[i], vecResultY[i + 1] - vecResultY[i]);
                var currentVecLength = Math.Sqrt(currentVec.X * currentVec.X + currentVec.Y * currentVec.Y);
                var currentMidPoint = new Point(vecResultX[i] + (int)(currentVec.X / 2f), (int)(vecResultY[i] + currentVec.Y / 2f));

                var angle = 0.0F;
                var scale = 1.0;
                var translation = new Point(0, 0);
                if (i > 0)
                {
                    if (!disableRotationScaling)
                    {
                        // Calcul d'angle
                        angle = (float)MathUtil.AngleBetween2DVectors(currentVec.X, currentVec.Y, originalVec.X, originalVec.Y, true,
                                currentVecLength, originalVecLength);
                        // Calcul de dimension (correctif à appliquer sur 'current' pour obtenir 'last', soit "current*scale = last")
                        scale = originalVecLength / currentVecLength;
                    }
                    // Calcul de translation (correctif pour que 'current' devienne 'last', soit "currentMid+translation = lastMid")
                    translation = new Point(originalMidPoint.X - currentMidPoint.X, originalMidPoint.Y - currentMidPoint.Y);
                }
                else
                {
                    // Valeurs de la première image.
                    originalVec = currentVec;
                    originalVecLength = currentVecLength;
                    originalMidPoint = currentMidPoint;
                }

                Rectangle boundingBox;
                var ok = MathUtil.CalculateRotationInternalBoundingBox(firstImageSize.Width, firstImageSize.Height, angle, out boundingBox);
                if (!ok)
                {
                    log.Error("Error during calculation of inner bounding box for file index {0}", i);
                    return new FileBundleNormalizationTransforms
                    {
                        GlobalParameters = new GlobalParameters(),
                        PicturesTransform = new List<PictureTransform>()
                    };
                }

                var rotationBoundingOffest = new Rectangle(
                    boundingBox.X + translation.X,
                    boundingBox.Y + translation.Y,
                    boundingBox.Width,
                    boundingBox.Height);
                boundingBox.Intersect(rotationBoundingOffest);

                // Ajustement du boundingBox pour le facteur de grossisement.
                {
                    var newWidth = boundingBox.Width * scale;
                    var newHeight = boundingBox.Height * scale;
                    var scaledBounding = new Rectangle(
                        (int)-((newWidth - boundingBox.Width) / 2.0),
                        (int)-((newHeight - boundingBox.Height) / 2.0),
                        (int)newWidth,
                        (int)newHeight);
                    // Si scaledBounding est plus grand (scale > 1), alors le résultat sera égal à boundingBox.
                    // Sinon, le résultat sera égal à scaledBounding
                    boundingBox.Intersect(scaledBounding);
                }

                var index = i / INPUTS_NBR;
                transforms.PicturesTransform.Add(new PictureTransform
                {
                    Angle = angle,
                    RotationCenter = currentMidPoint,
                    Scale = scale,
                    Offset = translation,
                    FinalBoundingBox = boundingBox,
                    Index = index,
                    Tag = vecTags[index]
                });
            }
            transforms.GlobalParameters.MinCrop = new Point(
                transforms.PicturesTransform.Any() ? transforms.PicturesTransform.Min(min => min.FinalBoundingBox.Width) : -1,
                transforms.PicturesTransform.Any() ? transforms.PicturesTransform.Min(min => min.FinalBoundingBox.Height) : -1);
            transforms.GlobalParameters.Input = inputs;
            transforms.GlobalParameters.OnlyTranslation = disableRotationScaling;

            return transforms;
        }

        /// <summary>
        /// Effectue une normalisation sur un groupe d'images selon des informations pré-calculées.
        /// Les transformations sont effectuées dans l'ordre d'indexage de la liste de fichier et des paramètres de transformation.
        /// </summary>
        /// <param name="pictures">La liste d'images à transformer.</param>
        /// <param name="transformInfo">Les informations de transformation.</param>
        /// <param name="dirToSave">Le répertoire où sauvegarder les fichiers modifiés.</param>
        /// <param name="cropTo">Effectue un "Crop" centré sur les images si présent.</param>
        /// <param name="token">Jeton de cancellation si on lance cette méthode de façon asynchrone.</param>
        /// <param name="progressAction">Méthode à invoquer sur chaque marque de progès, à des fins de UI.</param>
        /// <returns>TRUE si tout s'est bien déroulé.</returns>
        public static bool NormalizeFileBundleWithTransforms(List<FileInfo> pictures,
            FileBundleNormalizationTransforms transformInfo, DirectoryInfo dirToSave, Size? cropTo = null,
            CancellationToken? token = null, Action<int> progressAction = null)
        {
            return NormalizeFileBundleWithTransforms(pictures.Select(f => f.FullName).ToList(), transformInfo, dirToSave, cropTo, token, progressAction);
        }

        /// <summary>
        /// Effectue une normalisation sur un groupe d'images selon des informations pré-calculées.
        /// Les transformations sont effectuées dans l'ordre d'indexage de la liste de fichier et des paramètres de transformation.
        /// </summary>
        /// <param name="pictures">La liste d'images à transformer.</param>
        /// <param name="transformInfo">Les informations de transformation.</param>
        /// <param name="dirToSave">Le répertoire où sauvegarder les fichiers modifiés.</param>
        /// <param name="cropTo">Effectue un "Crop" centré sur les images si présent.</param>
        /// <param name="token">Jeton de cancellation si on lance cette méthode de façon asynchrone.</param>
        /// <param name="progressAction">Méthode à invoquer sur chaque marque de progès, à des fins de UI.</param>
        /// <param name="debug">Si on écrit des fichiers de débogage au même endroit que les fichiers finaux.</param>
        /// <returns>TRUE si tout s'est bien déroulé.</returns>
        public static bool NormalizeFileBundleWithTransforms(List<string> pictures, FileBundleNormalizationTransforms transformInfo, DirectoryInfo dirToSave,
            Size? cropTo = null, CancellationToken? token = null, Action<int> progressAction = null, bool debug = false)
        {
            if (transformInfo == null) return false;
            if (pictures == null || !pictures.Any()) return false;
            if (!Directory.Exists(dirToSave.FullName)) return false;

            var globalParameters = transformInfo.GlobalParameters;
            var finalCropX = globalParameters.MinCrop.X;
            var finalCropY = globalParameters.MinCrop.Y;
            var sizeConstraintX = 0;
            var sizeConstraintY = 0;
            // On prend le plus petit indice de tableau comme indice maximum.
            var maxIndex = pictures.Count <= transformInfo.PicturesTransform.Count ? pictures.Count : transformInfo.PicturesTransform.Count;
            var globalScale = 1.0;

            var indexByTag = transformInfo.PicturesTransform.Where(trans => !string.IsNullOrWhiteSpace(trans.Tag)).ToDictionary(trans => trans.Tag);

            for (var i = 0; i < maxIndex; i++)
            {
                if (token?.IsCancellationRequested ?? false)
                {
                    return false;
                }

                var tag = StringUtil.RemoveExtension(StringUtil.GetFileName(pictures[i]));
                var pictureTranform = !indexByTag.ContainsKey(tag) ? transformInfo.PicturesTransform[i] : indexByTag[tag];

                var path = dirToSave.FullName + Path.DirectorySeparatorChar;
                var name = StringUtil.RemoveExtension(StringUtil.GetFileName(pictures[i]));
                if (debug) File.Copy(pictures[i], path + name + "_0o.jpg");

                ImageUtil.RotateJpegToMatchExifData(pictures[i]);

                var final = ImageUtil.LoadBitmapFromFile(pictures[i], 72f);
                {
                    // Calcul du scaling global, si approprié. Uniquement différent de 1 si la taille de la première
                    // information de transformation (qui représente l'image originale de la séquence) et la première
                    // image lue diffère.
                    if (i == 0)
                    {
                        sizeConstraintX = final.Width;
                        sizeConstraintY = final.Height;

                        if (final.Width != pictureTranform.FinalBoundingBox.Width)
                        {
                            globalScale = final.Width / (double)pictureTranform.FinalBoundingBox.Width;
                            finalCropX = (int)(globalParameters.MinCrop.X * globalScale);
                            finalCropY = (int)(globalParameters.MinCrop.Y * globalScale);
                        }
                    }
                    else
                    {
                        if (final.Width != sizeConstraintX || final.Height != sizeConstraintY)
                        {
                            var tmp = ImageUtil.ResizeImage(final, sizeConstraintX, sizeConstraintY);
                            final.Dispose();
                            // ReSharper disable once RedundantAssignment
                            final = tmp;

                            log.Warn($"Image file did not have the same size as it's peers: {pictures[i]}");
                        }
                    }

                    try
                    {
                        var modificationCenterX = (int)(pictureTranform.RotationCenter.X * globalScale);
                        var modificationCenterY = (int)(pictureTranform.RotationCenter.Y * globalScale);

                        if (Math.Abs(pictureTranform.Angle) > TOLERANCE) // Si on n'est pas trop près de zéro
                        {
                            var tmp2 = ImageUtil.RotateImage(final, pictureTranform.Angle,
                                modificationCenterX,
                                modificationCenterY);
                            final.Dispose();
                            final = tmp2;
                            if (debug) ImageUtil.SaveToJpegWithOptions(final, 90, path + name + "_1r.jpg");
                        }

                        {
                            var tmp = new Bitmap(final.Width, final.Height, final.PixelFormat);
                            tmp.SetResolution(final.HorizontalResolution, final.VerticalResolution);
                            using (var graphics = Graphics.FromImage(tmp))
                            {
                                //Set the resize quality modes to high quality
                                graphics.CompositingQuality = CompositingQuality.HighQuality;
                                graphics.InterpolationMode = InterpolationMode.HighQualityBicubic;
                                graphics.SmoothingMode = SmoothingMode.HighQuality;

                                graphics.DrawImage(final,
                                    (int)(pictureTranform.Offset.X * globalScale),
                                    (int)(pictureTranform.Offset.Y * globalScale));
                            }
                            final.Dispose();
                            final = tmp;

                            if (debug) ImageUtil.SaveToJpegWithOptions(final, 90, path + name + "_2t.jpg");
                        }

                        if (Math.Abs(pictureTranform.Scale) - 1.0 > TOLERANCE) // Si on n'est pas trop près de zéro
                        {
                            var tmp2 = ImageUtil.ScaleImage(final, pictureTranform.Scale,
                                modificationCenterX,
                                modificationCenterY);
                            final.Dispose();
                            final = tmp2;
                            if (debug) ImageUtil.SaveToJpegWithOptions(final, 90, path + name + "_3s.jpg");
                        }

                        {
                            var tmp2 = ImageUtil.CropImageToSize(final, finalCropX, finalCropY);
                            final.Dispose();
                            final = tmp2;
                            if (debug) ImageUtil.SaveToJpegWithOptions(final, 90, path + name + "_4c.jpg");
                        }

                        if (cropTo != null)
                        {
                            var tmp = ImageUtil.CropImageToSizeMaximized(final, cropTo.Value.Width, cropTo.Value.Height);
                            final.Dispose();
                            final = tmp;
                        }

                        var outputName = dirToSave.FullName + Path.DirectorySeparatorChar +
                                         StringUtil.GetFileName(pictures[i]);
                        ImageUtil.SaveToImageFileWithOptions(final, ImageFormat.Jpeg, 90, outputName);
                    }
                    catch (System.Exception e)
                    {
                        log.Error(e, "Could not normalize files: " + e.Message);
                        return false;
                    }
                    finally
                    {
                        final?.Dispose();
                    }
                }

                progressAction?.Invoke(i + 1);
            }

            return true;
        }

        /// <summary>
        /// Tente de trouver dans une image des formes spécifiques. On peut choisir entre trois types: cercles, triangles, rectangles.
        /// Ils sont tous tentés d'être détectés par défaut, mais on peut choisir de détecter chacun de façon indépendante.
        /// Si on n'en choisit aucun, ou si le fichier d'image n'existe pas, on retourne null.
        /// </summary>
        /// <param name="inputFile">Le fichier à traiter.</param>
        /// <param name="detectCircles">Si on veut détecter des cercles.</param>
        /// <param name="detectTriangles">Si on veut détecter des triangles.</param>
        /// <param name="detectRectangles">Si on veut détecter des rectangles.</param>
        /// <returns>Une classe qui contient la recherche de chaque type. Si un type n'est pas choisi, on retourne null.
        /// Sinon, le résultat de cette recherche sera non-nul, mais peut-être vide si aucune forme n'est détectée.</returns>
        public static DetectedShapes DetectShapesInImage(FileInfo inputFile, bool detectCircles = true, bool detectTriangles = true, bool detectRectangles = true)
        {
            if (inputFile == null || !inputFile.Exists) return null;
            if (!detectCircles && !detectRectangles && !detectTriangles) return null;
            var result = new DetectedShapes();
            var img = CvInvoke.Imread(inputFile.FullName, LoadImageType.Grayscale);

            /*
            //Convert the image to grayscale and filter out the noise
            var uimage = new UMat();
            CvInvoke.CvtColor(img, uimage, img.);
            */

            //use image pyr to remove noise
            var pyrDown = new UMat();
            CvInvoke.PyrDown(img, pyrDown);
            CvInvoke.PyrUp(pyrDown, img);

            const double cannyThreshold = 180.0;

            #region circle detection
            if (detectCircles)
            {
                const double circleAccumulatorThreshold = 120;
                result.Circles = CvInvoke.HoughCircles(img, HoughType.Gradient, 2.0, 20.0, cannyThreshold,
                    circleAccumulatorThreshold, 5).ToList();
            }
            #endregion

            if (!detectRectangles && !detectTriangles) return result;
            #region Find triangles and rectangles

            const double cannyThresholdLinking = 120.0;
            var cannyEdges = new UMat();
            CvInvoke.Canny(img, cannyEdges, cannyThreshold, cannyThresholdLinking);

            result.TriangleList = new List<Triangle2DF>();
            result.RectangleList = new List<RotatedRect>(); //a box is a rotated rectangle

            using (var contours = new VectorOfVectorOfPoint())
            {
                CvInvoke.FindContours(cannyEdges, contours, null, RetrType.List, ChainApproxMethod.ChainApproxSimple);
                var count = contours.Size;
                for (var i = 0; i < count; i++)
                {
                    using (var contour = contours[i])
                    using (var approxContour = new VectorOfPoint())
                    {
                        CvInvoke.ApproxPolyDP(contour, approxContour, CvInvoke.ArcLength(contour, true) * 0.05, true);
                        if (!(CvInvoke.ContourArea(approxContour) > 250)) continue;
                        if (approxContour.Size == 3) //The contour has 3 vertices, it is a triangle
                        {
                            if (!detectTriangles) continue;
                            var pts = approxContour.ToArray();
                            result.TriangleList.Add(new Triangle2DF(
                                pts[0],
                                pts[1],
                                pts[2]
                            ));
                        }
                        else if (approxContour.Size == 4) //The contour has 4 vertices.
                        {
                            if (!detectRectangles) continue;
                            var pts = approxContour.ToArray();
                            var edges = PointCollection.PolyLine(pts, true);
                            var isRectangle =
                                edges.Select(
                                        (t, j) => Math.Abs(edges[(j + 1) % edges.Length].GetExteriorAngleDegree(t)))
                                    .All(angle => !(angle < 80) && !(angle > 100));
                            if (isRectangle) result.RectangleList.Add(CvInvoke.MinAreaRect(approxContour));
                        }
                    }
                }
            }

            #endregion

            return result;
        }

        /// <summary>
        /// Sauvegarde une matrice à un fichier d'image. Il y a un overhead significatif à cette opération.
        /// On fait également une vérification que le format donné soit compatible avec l'extension de l'image.
        /// Par exemple, il faut au moins 3 canaux pour un jpg et 4 pour un PNG.
        /// </summary>
        /// <param name="finalChannels">Matrices à convertir</param>
        /// <param name="outputImage">Fichier extrant.</param>
        /// <returns>TRUE si l'écriture du fichier s'est effectuée.</returns>
        public static bool SaveMatToImageFile(Mat finalChannels, string outputImage)
        {
            if (outputImage == null || finalChannels == null || finalChannels.NumberOfChannels < 1) return false;
            var isPng = StringUtil.GetFileExtension(outputImage).Equals("png", StringComparison.InvariantCultureIgnoreCase);

            using (var bmp = finalChannels.Bitmap)
            {
                if (isPng && (finalChannels.NumberOfChannels == 3 || finalChannels.NumberOfChannels == 4))
                {
                    ImageUtil.SaveToImageFileWithOptions(bmp, ImageFormat.Png, 9, outputImage);
                    return true;
                }
                ImageUtil.SaveToImageFileWithOptions(bmp, ImageFormat.Jpeg, 90, outputImage);
                return true;
            }
        }
    }

    /// <summary>
    /// Cette classe stocke les résultats des formes détectées dans une image.
    /// </summary>
    public class DetectedShapes
    {
        /// <summary>
        /// Les cercles détectés.
        /// </summary>
        public List<CircleF> Circles { get; set; }
        /// <summary>
        /// Les triangles détectés.
        /// </summary>
        public List<Triangle2DF> TriangleList { get; set; }
        /// <summary>
        /// Les rectangles détectés.
        /// </summary>
        public List<RotatedRect> RectangleList { get; set; }
    }
}