using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Drawing.Imaging;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading;
using Emgu.CV;
using Emgu.CV.CvEnum;
using Emgu.CV.Structure;
using Emgu.CV.Util;
using JetBrains.Annotations;
using NLog;
using NomadLogic.Common.Extentions;
using NomadLogic.Common.Util;
using NomadLogic.Common.Util.Math;
using NomadLogic.Multimedia.Gpu.OpenCV.TransformFromJson;
using NomadLogic.Multimedia.XmlTransformation;
using FileBundleNormalizationTransforms = NomadLogic.Multimedia.Gpu.OpenCV.TransformFromJson.FileBundleNormalizationTransforms;
using GlobalParameters = NomadLogic.Multimedia.Gpu.OpenCV.TransformFromJson.GlobalParameters;
using PictureTransform = NomadLogic.Multimedia.Gpu.OpenCV.TransformFromJson.PictureTransform;

namespace NomadLogic.Multimedia.Gpu.OpenCV
{
    /// <summary>
    /// Cette classe stocke les résultats des formes détectées dans une image.
    /// </summary>
    [PublicAPI]
    public class DetectedShapes
    {
        /// <summary>
        /// Les cercles détectés.
        /// </summary>
        [CanBeNull]
        public List<CircleF> Circles { get; set; }
        /// <summary>
        /// Les triangles détectés.
        /// </summary>
        [CanBeNull]
        public List<Triangle2DF> TriangleList { get; set; }
        /// <summary>
        /// Les rectangles détectés.
        /// </summary>
        [CanBeNull]
        public List<RotatedRect> RectangleList { get; set; }
    }

    /// <summary>
    /// Cette classe contient des méthode pratiques pour effectuer des opérations simple ou complexes en utilisant la librairie de traitement graphique OpenCV.
    /// </summary>
    [PublicAPI]
    public static class OpenCvUtil
    {
        private static readonly Logger log = LogManager.GetCurrentClassLogger();
        private const double TOLERANCE = 0.001;

        /// Ratio mathématique pour convertir une valeur entière (byte) de [0..255] à une valeur float normalisée de [0..1].
        /// En clair, 1 / 255
        public const double BYTE_TO_FLOAT_RATIO = 0.0039215686;

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
        public static FileBundleNormalizationTransforms CalculateNormalizationTransforms([NotNull] List<CoordinatesInput> inputs, [NotNull] List<string> files, bool disableRotationScaling = false,
            CancellationToken? token = null, Action<int> progressAction = null)
        {
            // Prend deux coordonnées, ni plus ni moins. Représente aussi le nombre de templates.
            const int INPUTS_NBR = 2;

            if (inputs.Count != INPUTS_NBR || files.Count < INPUTS_NBR) return null;

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

                BitmapExtentions.MatchExifData(file);

                vecTags.Add(StringUtil.RemoveExtension(StringUtil.GetFileName(file)));
                var imageFromFile = new Image<Bgr, byte>(file);
                var image = CvInvoke.CvArrToMat(imageFromFile);
                if (image == null) return null;

                var imageChannels = new VectorOfMat(3);
                CvInvoke.Split(image, imageChannels);

                var imgR = new Mat();
                var imgG = new Mat();
                var diffRg = new Mat();
                const int KSIZE = 11;
                const float F_SIGMA = 3.0f;
                CvInvoke.GaussianBlur(imageChannels[0], imgR, new Size(KSIZE, KSIZE), F_SIGMA, F_SIGMA);
                CvInvoke.GaussianBlur(imageChannels[1], imgG, new Size(KSIZE, KSIZE), F_SIGMA, F_SIGMA);
                CvInvoke.Subtract(imgR, imgG, diffRg);

                diffRg = imgG; // Assigne le header de imgG à diffRg, sans changer le contenu de la matrice résultante.

                // Si première image, initialisation des Matrices de comparaisons, basées sur le contenu de la première image.
                if (firstImage)
                {
                    firstImage = false; // Pour ne plus entrer ici.
                    firstImageSize = new Size(image.Cols, image.Rows);

                    for (var j = 0; j < INPUTS_NBR; j++)
                    {
                        // ReSharper disable once PossibleNullReferenceException
                        try
                        {
                            vTemplates.Push(new Mat(diffRg, inputs[j].Zone));
                        }
                        catch (System.Exception)
                        {
                            // Tente de déterminer pour quoi il y a eu exception en analysant le hearder des matrices.
                            var sb = new StringBuilder();
                            sb.Append("Could not create OpenCv Mat for first file normalization. Headers:\n");
                            sb.AppendLine($"Original Mat header: {GetMatHeader(diffRg)}");
                            sb.AppendLine($"Proposed Zone (from CoordinateInput): {inputs[j].Zone}");
                            log.Error(sb.ToString());
                            throw;
                        }
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
                    var tRi = (int)tR; // float to int

                    Mat subimage;
                    var candidateX = Math.Max(0, tX - tR2);
                    var candidateW = Math.Min(diffRg.Cols, tRi);
                    var candidateY = Math.Max(0, tY - tR2);
                    var candidateH = Math.Min(diffRg.Rows, tRi);
                    var roi = new Rectangle(
                        candidateX, 
                        candidateY, 
                        candidateW, 
                        candidateH);
                    // Créer une sous-image qui représente la zone de recherche.
                    try
                    {
                        subimage = new Mat(diffRg, roi);
                    }
                    catch (System.Exception)
                    {
                        // Tente de déterminer pour quoi il y a eu exception en analysant le hearder des matrices.
                        var sb = new StringBuilder();
                        sb.Append($"Could not create OpenCv Mat for file {nbr} normalization. Headers:\n");
                        sb.AppendLine($"Original Mat header: {GetMatHeader(diffRg)}");
                        sb.AppendLine($"Original CoordinateInput [{j}]: {inputs[j]}");
                        sb.AppendLine($"Proposed (possibly Cropped) ROI from zone (tX - tR2, tY - tR2, tRi, tRi): {roi}");
                        log.Error(sb.ToString());
                        throw;
                    }

                    var result = new Mat(tRi - tW, tRi - tH, DepthType.Cv32F, 1);
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
            var transforms = new FileBundleNormalizationTransforms();

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
                        angle = (float)MathUtil.AngleBetween2DVectors(currentVec.X, currentVec.Y, originalVec.X, originalVec.Y, true);
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

                var ok = MathUtil.CalculateRotationInternalBoundingBox(firstImageSize.Width, firstImageSize.Height, angle, out var boundingBox);
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
                // ReSharper disable once PossibleNullReferenceException
                transforms.PicturesTransform.Any() ? transforms.PicturesTransform.Min(min => min.FinalBoundingBox.Width) : -1,
                // ReSharper disable once PossibleNullReferenceException
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
        // ReSharper disable once ParameterTypeCanBeEnumerable.Global
        public static bool NormalizeFileBundleWithTransforms([NotNull] List<FileInfo> pictures,
            [NotNull] FileBundleNormalizationTransforms transformInfo, [NotNull] DirectoryInfo dirToSave, Size? cropTo = null,
            CancellationToken? token = null, Action<int> progressAction = null)
        {
            return NormalizeFileBundleWithTransforms(pictures.Select(f => f?.FullName).ToList(), transformInfo, dirToSave, cropTo, token, progressAction);
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
        public static bool NormalizeFileBundleWithTransforms([NotNull] List<string> pictures, [NotNull] FileBundleNormalizationTransforms transformInfo, [NotNull] DirectoryInfo dirToSave,
            Size? cropTo = null, CancellationToken? token = null, Action<int> progressAction = null, bool debug = false)
        {
            if (!pictures.Any()) return false;
            if (!Directory.Exists(dirToSave.FullName)) return false;

            var globalParameters = transformInfo.GlobalParameters;
            var finalCropX = globalParameters.MinCrop.X;
            var finalCropY = globalParameters.MinCrop.Y;
            var sizeConstraintX = 0;
            var sizeConstraintY = 0;
            // On prend le plus petit indice de tableau comme indice maximum.
            var maxIndex = pictures.Count <= transformInfo.PicturesTransform.Count ? pictures.Count : transformInfo.PicturesTransform.Count;
            var globalScale = 1.0;

            var indexByTag = transformInfo.PicturesTransform.Where(trans => !string.IsNullOrWhiteSpace(trans?.Tag)).ToDictionary(trans => trans.Tag);

            for (var i = 0; i < maxIndex; i++)
            {
                if (token?.IsCancellationRequested ?? false)
                {
                    return false;
                }

                BitmapExtentions.MatchExifData(pictures[i]);

                var tag = StringUtil.RemoveExtension(StringUtil.GetFileName(pictures[i]));
                var pictureTranform = !indexByTag.ContainsKey(tag) ? transformInfo.PicturesTransform[i] : indexByTag[tag];

                var path = dirToSave.FullName + Path.DirectorySeparatorChar;
                var name = StringUtil.RemoveExtension(StringUtil.GetFileName(pictures[i]));
                if (debug) File.Copy(pictures[i], path + name + "_0o.jpg");

                var final = FileUtil.LoadBitmap(pictures[i], 72f);
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
                            var tmp = final.Resize(sizeConstraintX, sizeConstraintY);
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
                            var tmp2 = final.Rotate(pictureTranform.Angle,
                                modificationCenterX,
                                modificationCenterY);
                            final.Dispose();
                            final = tmp2;
                            if (debug) final.Save(90, path + name + "_1r.jpg");
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

                            if (debug) final.Save(90, path + name + "_2t.jpg");
                        }

                        if (Math.Abs(pictureTranform.Scale) - 1.0 > TOLERANCE) // Si on n'est pas trop près de zéro
                        {
                            var tmp2 = final.Scale(pictureTranform.Scale,
                                modificationCenterX,
                                modificationCenterY);
                            final.Dispose();
                            final = tmp2;
                            if (debug) final.Save(90, path + name + "_3s.jpg");
                        }

                        {
                            var tmp2 = final.Crop(finalCropX, finalCropY);
                            final.Dispose();
                            final = tmp2;
                            if (debug) final.Save(90, path + name + "_4c.jpg");
                        }

                        if (cropTo != null)
                        {
                            var tmp = final.Frame(cropTo.Value.Width, cropTo.Value.Height);
                            final.Dispose();
                            final = tmp;
                        }

                        var outputName = dirToSave.FullName + Path.DirectorySeparatorChar + StringUtil.GetFileName(pictures[i]);
                        final.Save(ImageFormat.Jpeg, 90, outputName);
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
        /// Effectue une opération de clé chromatique sur un fichier en entrée, et met le résultat dans un fichier en sortie.
        /// </summary>
        /// <param name="originalImageFile">Fichier original à traiter.</param>
        /// <param name="keyedImageFile">Le futur fichier résultant.</param>
        /// <param name="keyInfo">L'information de clé chromatique.</param>
        /// <param name="alphaFilters">Liste de filtres à appliquer sur l'alpha de l'image.
        /// Si vide, une série de filtre par défaut sera appliqué. Pour absolument zéro filtrage, utilise la valeur "OpenCvFilterFactory.NoFilter()</param>
        public static void ChromaKeyOpenCv([NotNull] string originalImageFile, [NotNull] string keyedImageFile, 
            [NotNull] ChromaKeyInfoOpenCvCieLuv keyInfo, params OpenCvFilter[] alphaFilters)
        {
            ChromaKeyOpenCv(new FileInfo(originalImageFile), new FileInfo(keyedImageFile), keyInfo, alphaFilters);
        }

        /// <summary>
        /// Effectue une opération de clé chromatique sur un fichier en entrée, et met le résultat dans un fichier en sortie.
        /// </summary>
        /// <param name="originalImageFile">Fichier original à traiter.</param>
        /// <param name="keyedImageFile">Le futur fichier résultant.</param>
        /// <param name="keyInfo">L'information de clé chromatique.</param>
        /// <param name="alphaFilters">Liste de filtres à appliquer sur l'alpha de l'image.
        /// Si vide, une série de filtre par défaut sera appliqué. Pour absolument zéro filtrage, utilise la valeur "OpenCvFilterFactory.NoFilter()</param>
        [PublicAPI]
        public static void ChromaKeyOpenCv([NotNull] FileInfo originalImageFile, [NotNull] FileInfo keyedImageFile, 
            [NotNull] ChromaKeyInfoOpenCvCieLuv keyInfo, params OpenCvFilter[] alphaFilters)
        {
            if (!File.Exists(originalImageFile.FullName) || !keyInfo.KeyingData.Any()) return;

            using (var split = SplitImage<ChannelSplitImageLuv>(originalImageFile))
            {
                ChromaKeyOpenCv(split, keyInfo, false, keyedImageFile, null, alphaFilters);
            }
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
            var img = CvInvoke.Imread(inputFile.FullName, ImreadModes.Grayscale);

            //use image pyr to remove noise
            var pyrDown = new UMat();
            CvInvoke.PyrDown(img, pyrDown);
            CvInvoke.PyrUp(pyrDown, img);

            const double CANNY_THRESHOLD = 180.0;

            #region circle detection
            if (detectCircles)
            {
                const double CIRCLE_ACCUMULATOR_THRESHOLD = 120;
                result.Circles = CvInvoke.HoughCircles(img, HoughType.Gradient, 2.0, 20.0, CANNY_THRESHOLD, CIRCLE_ACCUMULATOR_THRESHOLD, 5).ToList();
            }
            #endregion

            if (!detectRectangles && !detectTriangles) return result;
            #region Find triangles and rectangles

            const double CANNY_THRESHOLD_LINKING = 120.0;
            var cannyEdges = new UMat();
            CvInvoke.Canny(img, cannyEdges, CANNY_THRESHOLD, CANNY_THRESHOLD_LINKING);

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
        /// On requiert au moins 3 canaux pour un format PNG, mais on peut sauvegarder un nombre moindre de canaux
        /// en JPG. ÈA titre d'exemple, un seul canal correspond èa une image "greyscale".
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
                    bmp.Save(ImageFormat.Png, 9, outputImage);
                    return true;
                }
                bmp.Save(ImageFormat.Jpeg, 90, outputImage);
                return true;
            }
        }

        /// <summary>
        /// Prépare une image pour des opérations répétée via les fonctions de OpenCv.
        /// </summary>
        /// <param name="file">Le fichier de l'image à préparer.</param>
        /// <param name="newSize">La nouvelle taille de l'image.</param>
        /// <returns>Une référence vers l'objet qui englobe les valeurs OpenCv pré-calculées.</returns>
        public static T SplitImage<T>(string file, Size? newSize = null) where T : ChannelSplitImageRgb, new ()
        {
            return SplitImage<T>(new FileInfo(file), newSize);
        }

        /// <summary>
        /// Prépare une image pour des opérations répétée via les fonctions de OpenCv.
        /// </summary>
        /// <param name="file">Le fichier de l'image à préparer.</param>
        /// <param name="newSize">La taille du contenant en pixel maximal. Le redimensionnement va 
        /// calculer la taille optimale pour être dansle contenant sans changer le ratio et perdre de l'information.</param>
        /// <returns>Une référence vers l'objet qui englobe les valeurs OpenCv pré-calculées.</returns>
        [PublicAPI]
        public static T SplitImage<T>(FileInfo file, Size? newSize = null) where T : ChannelSplitImageRgb, new ()
        {
            if (file == null || !File.Exists(file.FullName)) return null;

            var baseImage = CvInvoke.Imread(file.FullName, ImreadModes.Unchanged);

            var result = new T(); 
                
            result.Init(baseImage, newSize);

            return result;
        }

        /// <summary>
        /// Prépare une image pour des opérations répétée via les fonctions de OpenCv.
        /// </summary>
        /// <param name="bmp">L'image à préparer.</param>
        /// <param name="newSize">La taille du contenant en pixel maximal. Le redimensionnement va 
        /// calculer la taille optimale pour être dansle contenant sans changer le ratio et perdre de l'information.</param>
        /// <returns>Une référence vers l'objet qui englobe les valeurs OpenCv pré-calculées.</returns>
        [PublicAPI]
        public static T SplitImage<T>(Bitmap bmp, Size? newSize = null) where T : ChannelSplitImageRgb, new ()
        {
            if (bmp == null || bmp.Width == 0 || bmp.Height == 0) return null;

            if (bmp.PixelFormat != PixelFormat.Format24bppRgb || bmp.PixelFormat != PixelFormat.Format32bppArgb)
                return null;

            var result = new T();
            
            if (bmp.PixelFormat == PixelFormat.Format32bppArgb)
            {
                result.Init(new Image<Bgra, byte>(bmp), newSize);
            }
            else
            {
                result.Init(new Image<Bgr, byte>(bmp), newSize);
            }

            return result;
        }

        /// <summary>
        /// Effectue le calcul de clé chromatique sur une image pré-calculée. Il y a trois paramètres de sortie, tous indépendant l'un de l'autre.
        /// Si le paramètre "calculateBitmap" est vrai, on va mettre le résultat dans un Bitmap GDI et le retourner via le retour. Si on créé un fichier via
        /// l'argument "saveToFile", on retourne aussi une image car il faut créer un Bitmap de toute façon pour se faire.
        /// Si on passe un chemin valide de fichier, on va écrire un fichier d'image approprié à cet endroit, en écrasant le fichier s'il existe déjà.
        /// On fait une vérification de l'extension du fichier, et si l'extension est invalide (autre que ".PNG"), on ne fera pas la sauvegarde, on on va
        /// journaliser un avertissement.
        /// Si on passe une référence vers un ImageBox valide, on va faire une mise à jour des valeurs de l'image qu'il contient automatiquement par copie
        /// de pointeurs.
        /// Si aucun paramètre de sortie n'est spécifié, on ne fera rien. Également, si les intrants d'image et de clé chromatique sont invalides, on ne
        /// fera rien non plus.
        /// </summary>
        /// <param name="splitImage">L'image pré-calculée par OpenCv</param>
        /// <param name="chromaKeys">L'information de clé chromatique.</param>
        /// <param name="calculateBitmap">Indique si on génère un Bitmap comme sortie.</param>
        /// <param name="saveToFile">Indique si on sauvegarde le résultat dans un fichier PNG.</param>
        /// <param name="imageData">Indique si on met à jour une référence de tableau de byte avec le résultat. Cette méthode existe pour
        /// faire référence à une image OpenCv externe, pour une mise à jour hyper-rapide des données alpha.</param>
        /// <param name="alphaFilters">Liste de filtres à appliquer sur l'alpha de l'image.
        /// Si vide, une série de filtre par défaut sera appliqué. Pour absolument zéro filtrage, utilise la valeur "OpenCvFilterFactory.NoFilter()".
        /// Les filtres ne pouvant pas s'exécuter sur une seul canal (on ne modifie que l'alpha ici) seront ignorés.</param>
        /// <returns>Si on a créé un Bitmap (au moins un des deux premier paramètre de sortie est valide), on retourne une référence vers ce Bitmap.</returns>
        public static Bitmap ChromaKeyOpenCv(ChannelSplitImageLuv splitImage, ChromaKeyInfoOpenCvCieLuv chromaKeys,
            bool calculateBitmap = true, FileInfo saveToFile = null, byte[,,] imageData = null, params OpenCvFilter[] alphaFilters)
        {
            if (splitImage == null || chromaKeys?.KeyingData == null || chromaKeys.KeyingData.Count == 0) return null;
            
            if (calculateBitmap == false && saveToFile == null && imageData == null)
                return null;

            if (imageData != null && (imageData.Rank != 3 || imageData.GetLength(2) != 4))
                return null;

            var rows = splitImage.BaseMat.Rows;
            var cols = splitImage.BaseMat.Cols;

            var ptrDataWork = splitImage.WorkingLuvData.Data;
            if (splitImage.TmpAlphaData == null)
            {
                splitImage.TmpAlphaData = new byte[rows*cols];
            }
            var resultAlpha = splitImage.TmpAlphaData;
            var workingAlpha = splitImage.WorkingAlpha;

            var keyingData = chromaKeys.KeyingData;

            var tmpWorkDistanceArray = new double[3];
            
            for (var i = 0; i < rows; i++)
            {
                for (var j = 0; j < cols; j++)
                {
                    if (workingAlpha != null && workingAlpha[(i * cols) + j] == 0)
                    {
                        resultAlpha[(i * cols) + j] = 0;
                        continue;
                    }

                    var l = ptrDataWork[i, j, 0];
                    var u = ptrDataWork[i, j, 1];
                    var v = ptrDataWork[i, j, 2];

                    var cAlpha = byte.MaxValue;

                    foreach (var key in keyingData)
                    {
                        if (cAlpha == 0) break;

                        var calcDist = MathUtil.DistanceBetweenVectors(
                            l, u, v,
                            key.L, key.U, key.V,
                            tmpWorkDistanceArray);

                        var tPerc = key.Tolerance;
                        var tSmoo = key.Smoothing;

                        var mAlpha = byte.MaxValue;
                        if (calcDist < tPerc - tSmoo)
                        {
                            mAlpha = 0;
                        }
                        else if (calcDist < tPerc)
                        {
                            mAlpha = (byte)((calcDist - tPerc + tSmoo) / tSmoo * 255);
                        }

                        if (mAlpha < cAlpha)
                            cAlpha = mAlpha;
                    }

                    if (workingAlpha != null)
                    {
                        resultAlpha[(i * cols) + j] = Math.Min(workingAlpha[(i * cols) + j], cAlpha);
                    }
                    else
                    {
                        resultAlpha[(i * cols) + j] = cAlpha;
                    }

                    if (imageData != null)
                    {
                        imageData[i, j, 3] = cAlpha;
                    }
                }
            }

            // On applique les filtres sur l'apha
            if (alphaFilters.Length == 0) { alphaFilters = OpenCvFilterFactory.DEFAULT_FILTERS; }

            if (alphaFilters.Length > 0 && !(alphaFilters.Length == 1 && alphaFilters[0] == OpenCvFilterFactory.NoOp)) {
                var tmpMat1 = new Mat(rows, cols, DepthType.Cv8U, 1);
                var tmpMat2 = new Mat(rows, cols, DepthType.Cv8U, 1);

                Marshal.Copy(resultAlpha, 0, tmpMat1.DataPointer, resultAlpha.Length);

                var inMat = tmpMat1;
                var outMat = tmpMat2;
                var changed = false;
                foreach (var filter in alphaFilters) {
                    if (!filter.CanExecuteOnSingleChannel) continue;
                    if (!filter.Execute(inMat, outMat)) continue;
                    changed = true;
                    var tmp = inMat;
                    inMat = outMat;
                    outMat = tmp;
                }

                if (changed) {
                    Marshal.Copy(inMat.DataPointer, resultAlpha, 0, resultAlpha.Length);

                    // On met à jour la matrice de imageData, car elle n'aura pas été modifiée.
                    if (imageData != null) {
                        for (var i = 0; i < rows; i++) {
                            for (var j = 0; j < cols; j++) { imageData[i, j, 3] = resultAlpha[(i * cols) + j]; }
                        }
                    }
                }
            }

            if (!calculateBitmap && saveToFile == null) return null;
            
            /* Reconstruction de l'image en BGRA */
            var final = splitImage.AssembleImageDataFromSplit(resultAlpha);
                
            var result = final.ToBitmap();
            if (saveToFile != null)
            {
                result.Save(ImageFormat.Png, 9, saveToFile.FullName);
            }

            if (calculateBitmap) return result;
            result.Dispose();

            return null;
        }

        /// <summary>
        /// Effectue le calcul de clé chromatique sur une image lue d'un fichier. Il y a trois paramètres de sortie, tous indépendant l'un de l'autre.
        /// Si le paramètre "calculateBitmap" est vrai, on va mettre le résultat dans un Bitmap GDI et le retourner via le retour. Si on créé un fichier via
        /// l'argument "saveToFile", on retourne aussi une image car il faut créer un Bitmap de toute façon pour se faire.
        /// Si on passe un chemin valide de fichier, on va écrire un fichier d'image approprié à cet endroit, en écrasant le fichier s'il existe déjà.
        /// On fait une vérification de l'extension du fichier, et si l'extension est invalide (autre que ".PNG"), on ne fera pas la sauvegarde, on on va
        /// journaliser un avertissement.
        /// Si on passe une référence vers un ImageBox valide, on va faire une mise à jour des valeurs de l'image qu'il contient automatiquement par copie
        /// de pointeurs.
        /// Si aucun paramètre de sortie n'est spécifié, on ne fera rien. Également, si les intrants d'image et de clé chromatique sont invalides, on ne
        /// fera rien non plus.
        /// </summary>
        /// <param name="originalImageFile">Le fichier d'image.</param>
        /// <param name="chromaKeys">L'information de clé chromatique.</param>
        /// <param name="calculateBitmap">Indique si on génère un Bitmap comme sortie.</param>
        /// <param name="saveToFile">Indique si on sauvegarde le résultat dans un fichier PNG.</param>
        /// <param name="imageData">Indique si on met à jour une référence de tableau de byte avec le résultat. Cette méthode existe pour
        /// faire référence à une image OpenCv externe, pour une mise à jour hyper-rapide des données alpha.</param>
        /// <param name="alphaFilters">Liste de filtres à appliquer sur l'alpha de l'image.
        /// Si vide, une série de filtre par défaut sera appliqué. Pour absolument zéro filtrage, utilise la valeur "OpenCvFilterFactory.NoFilter()</param>
        /// <returns>Si on a créé un Bitmap (au moins un des deux premier paramètre de sortie est valide), on retourne une référence vers ce Bitmap.</returns>
        public static Bitmap ChromaKeyOpenCv(string originalImageFile, ChromaKeyInfoOpenCvCieLuv chromaKeys,
            bool calculateBitmap = true, FileInfo saveToFile = null, byte[,,] imageData = null, params OpenCvFilter[] alphaFilters)
        {
            if (string.IsNullOrWhiteSpace(originalImageFile) || !File.Exists(originalImageFile)) return null;

            var split = SplitImage<ChannelSplitImageLuv>(originalImageFile);

            return ChromaKeyOpenCv(split, chromaKeys, calculateBitmap, saveToFile, imageData, alphaFilters);
        }

        /// <summary>
        /// Créée une image via un effet de "double-exposition". Qui est en réalité une combinaison de plusieures étapes de manipulation d'image. À savoir:
        /// 1- La conversion de chaque image en en effet de colorisation unique (une unique couleur d'intensité variable), de façon individuelle.
        /// 2- La combinaison de ces deux images via un effet d'opacité sur la deuxième et d'un algorithme donnée de combinaison.
        /// 3- Le tout pour créer une image résultante en mémoire.
        /// Les deux images doivent avoir la même taille.
        /// </summary>
        /// <param name="originalImageFile1">L'image de base.</param>
        /// <param name="originalImageFile2">L'image à superposer.</param>
        /// <param name="file1Hue">Le colorit unique de l'image de base.</param>
        /// <param name="file2Hue">Le colorit unique de l'image de superposition.</param>
        /// <param name="blendFactor">Le facteur d'opacité. Par défaut, 0.5. Est ajouté en plus de l'opacité déjà existante.</param>
        /// <param name="blendFunction">L'algorithme de combinaison de pixels. Par défaut, OVERLAY, qui accentue le contraste entre teintes foncées et claires.</param>
        /// <returns>Une référence valide vers une image RGBA en mémoire. Null si l'opération est impossible avec les paramètres (comme fichiers inexistants).</returns>
        public static Bitmap DoubleExpositionEffect(FileInfo originalImageFile1, FileInfo originalImageFile2, Color file1Hue, Color file2Hue, 
            double blendFactor = 0.5, BlendMode blendFunction = BlendMode.Overlay)
        {
            if (originalImageFile1 == null || !originalImageFile1.Exists) return null;
            if (originalImageFile2 == null || !originalImageFile2.Exists) return null;

            using (var image1 = SplitImage<ChannelSplitImageHsl>(originalImageFile1.FullName))
            {
                using (var image2 = SplitImage<ChannelSplitImageHsl>(originalImageFile2.FullName))
                {

                    return DoubleExpositionEffect(image1, image2, file1Hue, file2Hue, blendFactor, blendFunction);
                }
            }
        }
        
        /// <summary>
        /// Créée une image via un effet de "double-exposition". Qui est en réalité une combinaison de plusieures étapes de manipulation d'image. À savoir:
        /// 1- La conversion de chaque image en en effet de colorisation unique (une unique couleur d'intensité variable), de façon individuelle.
        /// 2- La combinaison de ces deux images via un effet d'opacité sur la deuxième et d'un algorithme donnée de combinaison.
        /// 3- Le tout pour créer une image résultante en mémoire.
        /// Les deux images doivent avoir la même taille.
        /// Les images en paramètres seront modifiées. Elles ne seront pas détruites automatiquement une fois l'appel à cette méthode terminé.
        /// Elles peuvent avoir été modifiée même si on retourne null.
        /// </summary>
        /// <param name="image1">L'image de base.</param>
        /// <param name="image2">L'image à superposer.</param>
        /// <param name="file1Hue">Le colorit unique de l'image de base.</param>
        /// <param name="file2Hue">Le colorit unique de l'image de superposition.</param>
        /// <param name="blendFactor">Le facteur d'opacité. Par défaut, 0.5. Est ajouté en plus de l'opacité déjà existante.</param>
        /// <param name="blendFunction">L'algorithme de combinaison de pixels. Par défaut, OVERLAY, qui accentue le contraste entre teintes foncées et claires.</param>
        /// <returns>Une référence valide vers une image RGBA en mémoire. Null si l'opération est impossible avec les paramètres.</returns>
        public static Bitmap DoubleExpositionEffect(ChannelSplitImageHsl image1, ChannelSplitImageHsl image2, Color file1Hue, Color file2Hue, 
            double blendFactor = 0.5, BlendMode blendFunction = BlendMode.Overlay)
        {
            if (image1?.BaseMat == null || image2?.BaseMat == null) return null;
            if (!image1.BaseMat.Size.Equals(image2.BaseMat.Size)) return null;

            // 1- Colorisation des images.
           
            ConvertImageToHuedEffect(image1, file1Hue);
            ConvertImageToHuedEffect(image2, file2Hue, blendFactor);

            // 2- Composition des images.

            return null;
        }

        /// <summary>
        /// Modifie une instance d'image divisée par canal, en appliquant les opérations suivantes:
        /// 1- On a totalement remplacé les valeurs de couleur par une seule et même couleur, fournie en paramètre.
        /// 2- On force la création d'un alpha avec une valeur précise, ou l'on modifie l'alpha existant via le même facteur.
        /// L'image résultante possède donc une seule information de couleur, et un alpha.
        /// </summary>
        /// <param name="image">L'image originale sur laquelle faire l'opération. Elle ne sera pas modifiée.</param>
        /// <param name="newHue">La seule couleur à assigner.</param>
        /// <param name="scaleAlpha">La nouvelle valeur d'alpha, ou le facteur avec lequel modifier l'alpha.</param>
        /// <returns>Une nouvelle instance de l'image modifiée.</returns>
        public static void ConvertImageToHuedEffect(ChannelSplitImageHsl image, Color newHue, double scaleAlpha = 1.0)
        {
            var hlsVal = new float[3];
            MathUtil.RgbToHls(new[] {newHue.R, newHue.G, newHue.B}, hlsVal, true);

            var rows = image.WorkingHlsData.Mat.Rows;
            var cols = image.WorkingHlsData.Mat.Cols;

            var hMat = new Mat(rows, cols, DepthType.Cv32F, 1);
            CvInvoke.ExtractChannel(image.WorkingHlsData.Mat, hMat, 0);

            using (var hueMat = GetMatrixFromOpenCvArray<float>(hMat))
            {
                hueMat.SetValue(hlsVal[0]);
            }
            
            CvInvoke.MixChannels(hMat, image.WorkingHlsData.Mat, new [] {0 , 0});

            if (scaleAlpha != 1.0 && image.WorkingAlpha != null)
            {
                var handle = GCHandle.Alloc(image.WorkingAlpha, GCHandleType.Pinned);
                using (var alphaMat = new Matrix<byte>(rows, cols, handle.AddrOfPinnedObject()))
                {
                    //alphaMat.Mul(scaleAlpha);
                    alphaMat._Mul(scaleAlpha);
                }
                handle.Free();
            }
            
            //Reconstruction des information RGB
            image.UpdateRgbValuesFromWorkingHls();
        }

        /// <summary>
        /// Un objet de type "Matrix" est un peu délicat à manipuler, à cause des pointeurs. À cause de ceci, il est préférable
        /// d'utiliser cette méthode pour créer la matrice et ne pas faire d'erreur d'assignation de pointeurs. À cause que l'objet en
        /// paramètre et la matrice résultantes partagent la même plage mémoire, il faut se débarrasser de l'objet Matrix le plus vite possible.
        /// Mais elle offre de belle méthode de manipulation de données.
        /// Le genre d'erreur qui cause des "Segmentation faults" par exemple...
        /// </summary>
        /// <param name="ori">Matrice originale.</param>
        /// <typeparam name="T">Le type d'éléments contenus par la matrice.</typeparam>
        /// <returns>Une référence vers la nouvelle matrice.</returns>
        public static Matrix<T> GetMatrixFromOpenCvArray<T>([NotNull] Mat ori) where T : new()
        {
            return new Matrix<T>(ori.Width, ori.Height, ori.DataPointer);
        }

        /// <summary>
        /// Effectue une opération de mélange pixel par pixel sur les deux images en paramètre, avec un mode
        /// précis.
        /// </summary>
        /// <param name="img1">Image A</param>
        /// <param name="img2">Image B</param>
        /// <param name="output">Met le résultat dans cette instance. Ne doit pas être null.</param>
        /// <param name="mode">Le mode de mélange de pixels. NORMAL par défaut.</param>
        public static void BlendImages(ChannelSplitImageRgb img1, ChannelSplitImageRgb img2,
            Image<Bgra, byte> output, BlendMode mode = BlendMode.Normal)
        {
            // Tests de validité des arguments.
            if (img1?.BaseImageBgr == null || img2?.BaseImageBgr == null || output?.Mat == null) return;
            if (img1.BaseImageBgr.Size.Equals(img2.BaseImageBgr.Size)) return;
            if (output.NumberOfChannels != 4 || !img1.BaseImageBgr.Size.Equals(output.Size)) return;

            var workingMat = DoBlendImages(img1, img2, mode);
            workingMat?.CopyTo(output.Mat);
        }
        
        /// <summary>
        /// Effectue une opération de mélange pixel par pixel sur les deux images en paramètre, avec un mode précis.
        /// Les images doivent avoir exactement les même dimensions, sinon on retourne null dès le départ.
        /// </summary>
        /// <param name="img1">Image A</param>
        /// <param name="img2">Image B</param>
        /// <param name="mode">Le mode de mélange de pixels. NORMAL par défaut.</param>
        /// <returns>Une nouvelle instance d'image BGRA avec le résultat.</returns>
        public static Image<Bgra, byte> BlendImages (ChannelSplitImageRgb img1, ChannelSplitImageRgb img2, BlendMode mode = BlendMode.Normal)
        {
            // Tests de validité des arguments.
            if (img1?.BaseImageBgr == null || img2?.BaseImageBgr == null) return null;
            if (!img1.BaseImageBgr.SizeOfDimemsion.SequenceEqual(img2.BaseImageBgr.SizeOfDimemsion)) return null;

            var workingMat = DoBlendImages(img1, img2, mode);
            return workingMat?.ToImage<Bgra, byte>();
        }

        private static Mat DoBlendImages(ChannelSplitImageRgb img1, ChannelSplitImageRgb img2, BlendMode mode = BlendMode.Normal)
        {
            // Préparation des variables.
            var rows = img1.BaseImageBgr.Rows;
            var cols = img1.BaseImageBgr.Cols;
            
            using (
                Mat rgbA = new Mat(),
                alphaA = new Mat(),
                rgbB = new Mat(),
                alphaB = new Mat(),
                tmp1 = new Mat(rows, cols, DepthType.Cv8U, 1))
            {
                img1.BaseImageBgr.ConvertTo(rgbA, DepthType.Cv32F, BYTE_TO_FLOAT_RATIO);
                tmp1.SetTo(img1.WorkingAlpha);
                tmp1.ConvertTo(alphaA, DepthType.Cv32F, BYTE_TO_FLOAT_RATIO);

                img2.BaseImageBgr.ConvertTo(rgbB, DepthType.Cv32F, BYTE_TO_FLOAT_RATIO);
                tmp1.SetTo(img2.WorkingAlpha);
                tmp1.ConvertTo(alphaB, DepthType.Cv32F, BYTE_TO_FLOAT_RATIO);

                Mat resultBgra = null;
                switch (mode)
                {
                    case BlendMode.Normal:
                        OpenCvBlend.Normal(rgbA, alphaA, rgbB, alphaB, out resultBgra);
                        break;
                    case BlendMode.Addition:
                        break;
                    case BlendMode.Substract:
                        break;
                    case BlendMode.Difference:
                        break;
                    case BlendMode.Multiply:
                        OpenCvBlend.Multiply(rgbA, alphaA, rgbB, alphaB, out resultBgra);
                        break;
                    case BlendMode.Screen:
                        break;
                    case BlendMode.Overlay:
                        OpenCvBlend.Overlay(rgbA, alphaA, rgbB, alphaB, out resultBgra);
                        break;
                    case BlendMode.Ligthen:
                        break;
                    case BlendMode.Darken:
                        break;
                    case BlendMode.Average:
                        break;
                    case BlendMode.Negation:
                        break;
                    case BlendMode.Exclusion:
                        break;
                    case BlendMode.SoftLight:
                        break;
                    case BlendMode.ColorDodge:
                        break;
                    case BlendMode.ColorBurn:
                        break;
                    case BlendMode.Reflect:
                        break;
                    case BlendMode.Hue:
                        break;
                    case BlendMode.Saturation:
                        break;
                    case BlendMode.Color:
                        break;
                    case BlendMode.Luminosity:
                        break;
                    default:
                        throw new ArgumentOutOfRangeException(nameof(mode), mode, null);
                }

                if (resultBgra == null) throw new NotImplementedException("Blend mode is not implemented: " + mode);
                
                var final = new Mat();
                resultBgra.ConvertTo(final, DepthType.Cv8U, byte.MaxValue);

                return final;
            }

            
            /*
             var resultImage = new Image<Bgra, float>(cols, rows);
            var imgA = img1.BaseImageBgr.ToImage<Bgr, float>(true);
            var imgB = img2.BaseImageBgr.ToImage<Bgr, float>(true);
            
            var aColorData = imgA.Data;
            var aAlphaData = img1.WorkingAlpha;
            var bColorData = imgB.Data;
            var bAlphaData = img2.WorkingAlpha;
            
            var rData = resultImage.Data;
            
            // variables temporaires de stockages
            var bottom = new float[3];
            var top = new float[3];
            var blended = new float[4];
            var temp = new float[4];

            // Boucle itérative.
            
            for (var i = 0; i < rows; i++)
            {
                for (var j = 0; j < cols; j++)
                {
                    var sglDimIndex = (i * cols) + j;
                    
                    // Transparence complète.
                    if (aAlphaData[sglDimIndex] == 0 && bAlphaData[sglDimIndex] == 0)
                    {
                        try
                        {
                            rData[i, j, 3] = 0;
                        }
                        catch (IndexOutOfRangeException e)
                        {
                            Console.WriteLine($@"{i}, {j} => dims: [{rData.GetUpperBound(0) + 1},{rData.GetUpperBound(1) + 1},{rData.GetUpperBound(2) + 1}]");
                        }
                        continue;
                    }

                    for (var d = 0; d < 3; d++)
                    {
                        bottom[d] = aColorData[i, j, d];
                        top[d] = bColorData[i, j, d];
                    }

                    if (mode.Blend(bottom, aAlphaData[sglDimIndex], top, bAlphaData[sglDimIndex], blended, temp))
                    {
                        for (var d = 0; d < 4; d++)
                        {
                            rData[i, j, d] = blended[d];
                        }
                    }
                    else
                    {
                        // Error occured.
                        break;
                    }
                }
            }
            
            
            // Préparation du résultat

            var tmp = new Mat();
            resultImage.Mat.ConvertTo(tmp, DepthType.Cv8U); 

            return tmp;
            */
        }

        /// Génère un ToString d'un objet de type Mat.
        public static string GetMatHeader(Mat mat)
        {
            return mat == null ? null : $"[Rows={mat.Rows}, Cols={mat.Cols}, Step={mat.Step}, Depth={mat.Depth}, Channels={mat.NumberOfChannels}, Dimensions={StringUtil.FormatEnumerable(mat.SizeOfDimemsion.GetEnumerator(), false, 0, ",")}]";
        }
    }
}