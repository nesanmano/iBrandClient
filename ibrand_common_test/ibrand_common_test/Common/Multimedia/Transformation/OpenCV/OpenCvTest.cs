using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Linq;
using System.Text;
using Emgu.CV;
using Emgu.CV.CvEnum;
using Emgu.CV.Structure;
using Emgu.CV.Util;
using Ionic.Zip;
using NLog;
using NomadLogic.Common.Extentions;
using NomadLogic.Common.Util;
using NomadLogic.Common.Util.Json;
using NomadLogic.Multimedia.Gpu.OpenCV;
using NomadLogic.Multimedia.Gpu.OpenCV.TransformFromJson;
using NomadLogic.Multimedia.XmlTransformation;
using NUnit.Framework;

namespace NomadLogic.Test.Common.Multimedia.Transformation.OpenCV
{
    public class OpenCvTest : TestBaseNunit
    {
        // ReSharper disable once UnusedMember.Local
        private static readonly Logger log = LogManager.GetCurrentClassLogger();

        /// <summary>
        /// Test pour faire fonctionner le code de normalisation d'image de Éric, via le code de Matthew Toews.
        /// Ce code n'est pas optimisé et contient la même logique que le code original, sans optimisation.
        /// </summary>
        [Test]
        public void NormalizeCameraRingImagesToJsonWithRawOpenCv()
        {
            #region Étape 1 - On prépare les fichiers
            var originalImageZipFile = BaseDir + Path.DirectorySeparatorChar + "cameraring_raw_images" +
                                       Path.DirectorySeparatorChar + "test3.zip";
            Assert.IsTrue(File.Exists(originalImageZipFile));
            var outputDir = GetFullOutputDir();
            var files = Directory.GetFiles(outputDir);
            if (files.Length > 0)
            {
                foreach (var file in files)
                {
                    File.Delete(file);
                }
                files = Directory.GetFiles(outputDir);
            }
            Assert.IsTrue(files.Length == 0);

            var zip = ZipFile.Read(originalImageZipFile);
            zip.ExtractAll(outputDir);
            files = Directory.GetFiles(outputDir);
            FileUtil.SortFileCollection(files);
            Assert.IsTrue(files.Length == 30); // Il doit y avoir 24 fichiers dans le ZIP.

            var finalTransformationFileName = new FileInfo(outputDir + "result.json");
            #endregion

            /* Ces valeurs sont des variables d'entrées fixes. Je ne sais pas quelles les valeurs à entrer pour l'instant */
            var inputs = new[]
            {
                new float[] { 959, 561, 20, 100 },
                new float[] { 957, 754, 20, 100 }
            };

            #region Étape 2 - Calcul des valeurs
            var vxys = new VectorOfFloat();

            // On initialise le vecteur de points
            foreach (var input in inputs)
            {
                vxys.Push(input);
            }

            var vTemplates = new VectorOfMat();
            var iTemplateCount = 0;
            var iSearchSize = 0.0f;
            var iSize = 0.0f;

            var firstImage = true;
            var iImageCount = 0;
            var vecResultX = new VectorOfInt();
            var vecResultY = new VectorOfInt();
            var iImageSizeX = 0;
            var iImageSizeY = 0;

            foreach (var imageFile in files)
            {
                var imageFromFile = new Image<Bgr, byte>(imageFile);
                var image = CvInvoke.CvArrToMat(imageFromFile);
                var result = new Mat();

                var pirgb = new VectorOfMat(3);
                CvInvoke.Split(image, pirgb);

                var imgR = new Mat();
                var imgG = new Mat();
                var diffRg = new Mat();
                const int ksize = 11;
                const float fSigma = 3.0f;
                CvInvoke.GaussianBlur(pirgb[0], imgR, new Size(ksize, ksize), fSigma, fSigma);
                CvInvoke.GaussianBlur(pirgb[1], imgG, new Size(ksize, ksize), fSigma, fSigma);
                CvInvoke.Subtract(imgR, imgG, diffRg);

                diffRg = imgG;//pirgb[1]; // TODO Pourquoi?????

                if (firstImage)
                {
                    firstImage = false;

                    // Crop templates

                    iImageSizeX = image.Cols;
                    iImageSizeY = image.Rows;

                    // Assume one template size, one search region size
                    iSize = vxys[2];
                    iSearchSize = vxys[3];

                    // Set to size of template list for multi-template
                    iTemplateCount = vxys.Size / 4;

                    for (var j = 0; j < iTemplateCount; j++)
                    {
                        vTemplates.Push(
                                 new Mat(diffRg,
                                    new Rectangle(
                                        (int)(vxys[4 * j + 0] - iSize / 2),
                                        (int)(vxys[4 * j + 1] - iSize / 2),
                                        (int)iSize,
                                        (int)iSize)));
                        //imshow("template", vTemplates[j]);

                        // Push back zero displacements for the template(s) in first image
                        //vecResultX.push_back(0);
                        //vecResultY.push_back(0);
                    }

                    // Create result array

                    var resultCols = (int)(iSearchSize - iSize + 1);
                    var resultRows = (int)(iSearchSize - iSize + 1);
                    result.Create(resultRows, resultCols, DepthType.Cv32F, 1);
                }

                // Identify templates in new image

                // Only do 1 point for now ... need two for rotation / scale
                for (var j = 0; j < iTemplateCount; j++)
                {
                    // Get subimage
                    var subimage = new Mat(diffRg,
                        new Rectangle(
                            (int)(vxys[4 * j + 0] - iSearchSize / 2),
                            (int)(vxys[4 * j + 1] - iSearchSize / 2),
                            (int)iSearchSize,
                            (int)iSearchSize
                        ));

                    // Match templates
                    //matchTemplate( subimage, vTemplates[j], result, CV_TM_CCORR_NORMED );
                    CvInvoke.MatchTemplate(subimage, vTemplates[j], result, TemplateMatchingType.Sqdiff);
                    CvInvoke.Normalize(result, result, 0, 1, NormType.MinMax);
                    //Les paramètres supplémentaires étaient "-1, Mat()" avant.

                    // Identify max
                    var minVal = 0.0;
                    var maxVal = 0.0;
                    var minLoc = new Point();
                    var maxLoc = new Point();
                    CvInvoke.MinMaxLoc(result, ref minVal, ref maxVal, ref minLoc, ref maxLoc, new Mat());
                    maxLoc = minLoc;

                    //rectangle( result, maxLoc, Point( maxLoc.x + iSize, maxLoc.y + iSize ), Scalar::all(255), 2, 8, 0 );

                    // Show resultx
                    //imshow("SubImage", result);

                    // Determine location of match in the entire image
                    var matchLocX = (int)(maxLoc.X + vxys[4 * j + 0] - iSearchSize / 2);
                    var matchlocY = (int)(maxLoc.Y + vxys[4 * j + 1] - iSearchSize / 2);
                    var matchLoc = new Rectangle(
                        matchLocX,
                        matchlocY,
                        (int)(matchLocX + iSize),
                        (int)(matchlocY + iSize));
                    CvInvoke.Rectangle(image, matchLoc, new MCvScalar(255), 2);
                    // Valeurs par défaut redondantes: ", LineType.EightConnected, 0);"

                    // Save template location in new image
                    vecResultX.Push(new[] { (int)(matchLoc.X + iSize / 2) });
                    vecResultY.Push(new[] { (int)(matchLoc.Y + iSize / 2) });
                }

                // Show region with both templates
                //int x1 = vecResultX[iTemplateCount*iImageCount+0]-iSize/2;
                //int y1 = vecResultY[iTemplateCount*iImageCount+0]-iSize/2;
                //int x2 = vecResultX[iTemplateCount*iImageCount+1]-iSize/2;
                //int y2 = vecResultY[iTemplateCount*iImageCount+1]-iSize/2;
                /*
                var x1 = vecResultX[0];
                var y1 = vecResultY[0];
                var x2 = vecResultX[1];
                var y2 = vecResultY[1];
                */

                // Show ROI
                //Mat imageROI;

                //imageROI= image(cv::Rect(
                //				     vxys[4*1+0]-vxys[4*1+3]/2,
                //				     vxys[4*1+1]-vxys[4*1+3]/2,
                //				     vxys[4*1+3],
                //				     vxys[4*1+3]
                //			 ));

                /*
                var iRoiX = (int)(Math.Min(x1, x2) - iSearchSize / 2);
                var iRoiY = (int)(Math.Min(y1, y2) - iSearchSize / 2);
                var imageRoi = new Mat(image, new Rectangle(
                             iRoiX,
                             iRoiY,
                             (int)(Math.Abs(x1 - x2) + iSearchSize),
                             (int)(Math.Abs(y1 - y2) + iSearchSize)
                             ));
                */

                #region Callback Function
                /*
                CALLBACK_DATA cbdata;
                cbdata.ptOrigin.x = iROIx;
                cbdata.ptOrigin.y = iROIy;
                cbdata.pvResultX = vecResultX;
                cbdata.pvResultY = vecResultY;
                cbdata.iTemplateCount = iTemplateCount;
                cbdata.iImageCount = iImageCount;
                cbdata.pimg = &imageROI;
                imshow("Image sequence | press ESC to close", imageROI);
                setMouseCallback("Image sequence | press ESC to close", CallBackFunc, &cbdata);
                */
                #endregion

                // Increment image count
                iImageCount++;
            }
            #endregion

            #region Étape 3 - Collection des résultats (write_json_pairs)
            var transforms = new FileBundleNormalizationTransforms
            {
                GlobalParameters = new GlobalParameters(),
                PicturesTransform = new List<PictureTransform>()
            };

            var iPointsPerImage = vecResultX.Size / iImageCount;
            // Values in first image
            var fDist0 = -1.0;
            var fAngle0 = -1000.0;
            var iX0 = -10000;
            var iY0 = -1111;

            for (var i = 0; i < vecResultX.Size; i += iPointsPerImage)
            {
                var x1 = vecResultX[i];
                var y1 = vecResultY[i];

                var x2 = i == vecResultX.Size - 1 ? x1 : vecResultX[i + 1];
                var y2 = i == vecResultX.Size - 1 ? y1 : vecResultY[i + 1];

                float dx = x2 - x1;
                float dy = y2 - y1;

                var fDist = Math.Sqrt(dx * dx + dy * dy);
                var fAngle = Math.Atan2(dy, dx);

                if (i == 0)
                {
                    // Save first/reference distance and angle.
                    fDist0 = fDist;
                    fAngle0 = fAngle;
                    iX0 = x1;
                    iY0 = y1;
                }

                var vecX = x1 - iX0;
                var vecY = y1 - iY0;
                transforms.PicturesTransform.Add(new PictureTransform
                {
                    Angle = (float)((fAngle - fAngle0) * 180 / Math.PI),
                    Scale = fDist / fDist0,
                    Offset = new Point(vecX, vecY),
                    FinalBoundingBox = new Rectangle(new Point(iImageSizeX - 2 * Math.Abs(vecX), iImageSizeY - 2 * Math.Abs(vecY)), new Size())
                });
            }
            transforms.GlobalParameters.MinCrop = new Point(transforms.PicturesTransform.Any() ? transforms.PicturesTransform.Min(min => min.FinalBoundingBox.X) : -1,
                transforms.PicturesTransform.Any() ? transforms.PicturesTransform.Min(min => min.FinalBoundingBox.Y) : -1);

            Assert.IsTrue(JsonUtil.WriteJsonFile(finalTransformationFileName, transforms));
            #endregion

            #region Étape 4 - Test de la structure résultat final
            Assert.IsTrue(transforms.GlobalParameters.MinCrop.X < int.MaxValue);
            Assert.IsTrue(transforms.GlobalParameters.MinCrop.Y < int.MaxValue);
            Assert.IsTrue(transforms.PicturesTransform.Count == 30);
            Assert.IsTrue(transforms.GlobalParameters.MinCrop.X > 0);
            Assert.IsTrue(transforms.GlobalParameters.MinCrop.Y > 0);

            // Contenue rigoureusement identique si on lit le fichier.
            var readFromFile = JsonUtil.ReadJsonFile<FileBundleNormalizationTransforms>(finalTransformationFileName);
            Assert.AreNotSame(readFromFile, transforms);
            Assert.AreEqual(readFromFile, transforms);
            #endregion
        }

        /// <summary>
        /// Test pour faire rouler OpenCV via le wrapper EMGU.
        /// Basé sur le code de ce test:
        /// http://www.emgu.com/wiki/index.php/Shape_(Triangle,_Rectangle,_Circle,_Line)_Detection_in_CSharp
        /// </summary>
        [Test]
        public void DetectShapesInImage()
        {
            var originalImageFile = BaseDir + Path.DirectorySeparatorChar + "Opencvpic3sample.png";
            Assert.IsTrue(File.Exists(originalImageFile));
            var outputDir = GetFullOutputDir();
            var triangleImageOuput = outputDir + "triangle.jpg";
            EnsureFileNotExists(triangleImageOuput);
            var circleImageOuput = outputDir + "circle.jpg";
            EnsureFileNotExists(circleImageOuput);
            var lineImageOuput = outputDir + "line.jpg";
            EnsureFileNotExists(lineImageOuput);

            var msgBuilder = new StringBuilder("Performance: ");

            //Load the image from file and resize it for display
            var img =
               new Image<Bgr, byte>(originalImageFile)
               .Resize(400, 400, Inter.Linear, true);

            //Convert the image to grayscale and filter out the noise
            var uimage = new UMat();
            CvInvoke.CvtColor(img, uimage, ColorConversion.Bgr2Gray);

            //use image pyr to remove noise
            var pyrDown = new UMat();
            CvInvoke.PyrDown(uimage, pyrDown);
            CvInvoke.PyrUp(pyrDown, uimage);

            #region circle detection
            var watch = Stopwatch.StartNew();
            const double cannyThreshold = 180.0;
            const double circleAccumulatorThreshold = 120;
            var circles = CvInvoke.HoughCircles(uimage, HoughType.Gradient, 2.0, 20.0, cannyThreshold, circleAccumulatorThreshold, 5);

            watch.Stop();
            msgBuilder.Append($"Hough circles - {watch.ElapsedMilliseconds} ms; ");
            #endregion

            #region Canny and edge detection
            watch.Reset(); watch.Start();
            const double cannyThresholdLinking = 120.0;
            var cannyEdges = new UMat();
            CvInvoke.Canny(uimage, cannyEdges, cannyThreshold, cannyThresholdLinking);

            var lines = CvInvoke.HoughLinesP(
               cannyEdges,
               1, //Distance resolution in pixel-related units
               Math.PI / 45.0, //Angle resolution measured in radians.
               20, //threshold
               30, //min Line width
               10); //gap between lines

            watch.Stop();
            msgBuilder.Append($"Canny & Hough lines - {watch.ElapsedMilliseconds} ms; ");
            #endregion

            #region Find triangles and rectangles
            watch.Reset(); watch.Start();
            var triangleList = new List<Triangle2DF>();
            var boxList = new List<RotatedRect>(); //a box is a rotated rectangle

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
                            var pts = approxContour.ToArray();
                            triangleList.Add(new Triangle2DF(
                                pts[0],
                                pts[1],
                                pts[2]
                            ));
                        }
                        else if (approxContour.Size == 4) //The contour has 4 vertices.
                        {
                            #region determine if all the angles in the contour are within [80, 100] degree

                            var pts = approxContour.ToArray();
                            var edges = PointCollection.PolyLine(pts, true);

                            var isRectangle = edges.Select((t, j) => Math.Abs(edges[(j + 1) % edges.Length].GetExteriorAngleDegree(t))).All(angle => !(angle < 80) && !(angle > 100));

                            #endregion

                            if (isRectangle) boxList.Add(CvInvoke.MinAreaRect(approxContour));
                        }
                    }
                }
            }

            watch.Stop();
            msgBuilder.Append($"Triangles & Rectangles - {watch.ElapsedMilliseconds} ms; ");
            #endregion

            Console.WriteLine(msgBuilder.ToString());

            #region draw triangles and rectangles
            var triangleRectangleImage = img.CopyBlank();
            foreach (var triangle in triangleList)
                triangleRectangleImage.Draw(triangle, new Bgr(Color.DarkBlue), 2);
            foreach (var box in boxList)
                triangleRectangleImage.Draw(box, new Bgr(Color.DarkOrange), 2);
            using (var result = triangleRectangleImage.ToBitmap())
            {
                Assert.IsTrue(result.SaveAsJpeg(90, triangleImageOuput));
            }
            #endregion

            #region draw circles
            var circleImage = img.CopyBlank();
            foreach (var circle in circles)
                circleImage.Draw(circle, new Bgr(Color.Brown), 2);
            using (var result = circleImage.ToBitmap())
            {
                Assert.IsTrue(result.SaveAsJpeg(90, circleImageOuput));
            }
            #endregion

            #region draw lines
            var lineImage = img.CopyBlank();
            foreach (var line in lines)
                lineImage.Draw(line, new Bgr(Color.Green), 2);
            using (var result = lineImage.ToBitmap())
            {
                Assert.IsTrue(result.SaveAsJpeg(90, lineImageOuput));
            }
            #endregion
        }

        [Test]
        public void TestFileSort()
        {
            var toSort = new[]
            {
                @"c:\test\2.jpg",
                @"c:\test\10.jpg",
                @"c:\test\14.jpg",
                @"c:\test\11.jpg",
                @"c:\test\12.jpg",
                @"c:\test\089.jpg",
                @"c:\test\001.jpg",
                @"c:\test\78.jpg",
                @"c:\test\34.jpg"

            };

            var sorted = new[]
            {
                @"c:\test\001.jpg",
                @"c:\test\2.jpg",
                @"c:\test\10.jpg",
                @"c:\test\11.jpg",
                @"c:\test\12.jpg",
                @"c:\test\14.jpg",
                @"c:\test\34.jpg",
                @"c:\test\78.jpg",
                @"c:\test\089.jpg"
            };

            for (var i = 0; i < sorted.Length - 1; i++)
            {
                Assert.IsTrue(FileUtil.CompareFileNames(sorted[i], sorted[i + 1]) < 0, "File name not correctly detected as lower: {0} < {1}", sorted[i], sorted[i + 1]);
            }

            FileUtil.SortFileCollection(toSort);

            Assert.IsTrue(sorted.SequenceEqual(toSort));
        }

        /// <summary>
        /// Ce test effectue un calcul de normalisation puis applique cette normalisation à un groupe d'image.
        /// </summary>
        [Test]
        [SuppressMessage("ReSharper", "RedundantArgumentDefaultValue")]
        public void CalculateAndApplyImageBundleNormalization()
        {
            #region Étape 1 - On prépare les fichiers
            const int NBR_FILES = 30;
            var originalImageZipFile = BaseDir + Path.DirectorySeparatorChar + "cameraring_raw_images" +
                                       Path.DirectorySeparatorChar + "test3.zip";
            Assert.IsTrue(File.Exists(originalImageZipFile));
            var outputDir = GetFullOutputDir();
            var filesToDestroy = Directory.GetFiles(outputDir);
            if (filesToDestroy.Length > 0)
            {
                foreach (var file in filesToDestroy)
                {
                    File.Delete(file);
                }
                filesToDestroy = Directory.GetFiles(outputDir);
            }
            Assert.IsTrue(filesToDestroy.Length == 0);
            var finalTransformationFileName = new FileInfo(outputDir + "result.json");

            var zip = ZipFile.Read(originalImageZipFile);
            zip.ExtractAll(outputDir);
            var files = Directory.GetFiles(outputDir).ToList();
            FileUtil.SortFileCollection(files);
            Assert.IsTrue(files.Count == NBR_FILES); // Il doit y avoir X fichiers dans le ZIP.
            #endregion

            /* Ces valeurs sont des variables d'entrées fixes. Je ne sais pas quelles les valeurs à entrer pour l'instant */
            var inputs = new List<CoordinatesInput>
            {
                new CoordinatesInput
                {
                    Zone = new Rectangle(1304, 697, 16, 20),
                    Radius = 100
                },
                new CoordinatesInput
                {
                    Zone = new Rectangle(1307, 997, 19, 13),
                    Radius = 100
                }
            };

            var transforms = OpenCvUtil.CalculateNormalizationTransforms(inputs, files, false);
            Assert.IsNotNull(transforms);
            Assert.IsTrue(JsonUtil.WriteJsonFile(finalTransformationFileName, transforms));
            Assert.IsTrue(transforms.GlobalParameters.Input.SequenceEqual(inputs));

            // Il ne peut y avoir de boundingBox interne plus grande que la première transformatio.
            // De plus, toutes les coordonnées de ces boundings box doivent être positifs, car ce sont tous des
            // rectangle plus petits que la taille originale. La seule exception est l'égalité et les indices 0,0
            var firstTransform = transforms.PicturesTransform[0].FinalBoundingBox;
            Assert.IsFalse(transforms.PicturesTransform.Any(t => 
                t.FinalBoundingBox.Width > firstTransform.Width || 
                t.FinalBoundingBox.Height > firstTransform.Height ||
                t.FinalBoundingBox.X < 0 ||
                t.FinalBoundingBox.Y < 0));
            
            // On traite maintenant les fichiers pour les transformer.
            var result = OpenCvUtil.NormalizeFileBundleWithTransforms(files.ToList(), transforms, new DirectoryInfo(outputDir), null, null, null, false);
            Assert.IsTrue(result);

            foreach (var file in files)
            {
                using (var img = FileUtil.LoadBitmap(file))
                {
                    Assert.IsTrue(img.Width == transforms.GlobalParameters.MinCrop.X);
                    Assert.IsTrue(img.Height == transforms.GlobalParameters.MinCrop.Y);
                }
            }
        }

        /// <summary>
        /// Ce test effectue un calcul de normalisation sur une série d'images puis applique cette normalisation à un groupe d'image dont la résolution est plus petite
        /// que les images dont est issue la normalisation.
        /// </summary>
        [Test]
        [SuppressMessage("ReSharper", "RedundantArgumentDefaultValue")]
        public void CalculateAndApplyImageBundleNormalizationToLowerResolutionFiles()
        {
            #region Étape 1 - On prépare les fichiers
            var originalImageZipFileNorm = BaseDir + Path.DirectorySeparatorChar + "cameraring_raw_images" +
                                       Path.DirectorySeparatorChar + "test3.zip";
            Assert.IsTrue(File.Exists(originalImageZipFileNorm));
            var outputDir = GetFullOutputDir();

            var outputNorm = outputDir + "normalization" + Path.DirectorySeparatorChar;
            EnsureDirExists(outputNorm);
            var filesToDestroy = Directory.GetFiles(outputNorm);
            if (filesToDestroy.Length > 0)
            {
                foreach (var file in filesToDestroy)
                {
                    File.Delete(file);
                }
                filesToDestroy = Directory.GetFiles(outputNorm);
            }
            Assert.IsTrue(filesToDestroy.Length == 0);

            var outputFinal = outputDir + "final" + Path.DirectorySeparatorChar;
            EnsureDirExists(outputFinal);
            filesToDestroy = Directory.GetFiles(outputFinal);
            if (filesToDestroy.Length > 0)
            {
                foreach (var file in filesToDestroy)
                {
                    File.Delete(file);
                }
                filesToDestroy = Directory.GetFiles(outputFinal);
            }
            Assert.IsTrue(filesToDestroy.Length == 0);
            var finalTransformationFileName = new FileInfo(outputDir + "normalisation.json");

            var zip = ZipFile.Read(originalImageZipFileNorm);
            zip.ExtractAll(outputNorm);
            var filesNorm = Directory.GetFiles(outputNorm).ToList();
            FileUtil.SortFileCollection(filesNorm);

            // On copie les images en les transformant en plus petit dans le répertoire final
            foreach (var file in filesNorm)
            {
                var bmp = FileUtil.LoadBitmap(file);
                var tmp = bmp.Resize(1080, 720);
                tmp.SaveAsJpeg(90, outputFinal + StringUtil.GetFileName(file));
                tmp.Dispose();
                bmp.Dispose();
            }
            var filesFinal = Directory.GetFiles(outputFinal).ToList();
            FileUtil.SortFileCollection(filesFinal);
            #endregion

            /* Ces valeurs sont des variables d'entrées fixes. */
            var inputs = new List<CoordinatesInput>
            {
                new CoordinatesInput
                {
                    Zone = new Rectangle(1304, 697, 16, 20),
                    Radius = 100
                },
                new CoordinatesInput
                {
                    Zone = new Rectangle(1307, 997, 19, 13),
                    Radius = 100
                }
            };

            var transforms = OpenCvUtil.CalculateNormalizationTransforms(inputs, filesNorm, false);
            Assert.IsNotNull(transforms);
            Assert.IsTrue(JsonUtil.WriteJsonFile(finalTransformationFileName, transforms));

            // On traite maintenant les fichiers pour les transformer.
            var result = OpenCvUtil.NormalizeFileBundleWithTransforms(filesFinal.ToList(), transforms, new DirectoryInfo(outputFinal), null, null, null, false);
            Assert.IsTrue(result);
        }

        /// <summary>
        /// Ce test calcule une normalisation sur un groupe d'image, puis applique cette transformation sur un groupe d'image
        /// avec la même nomenclature, mais avec des fichiers d'images manquants. Cela teste si les transformations sont appliquées correctement
        /// quand l'information de "tags" est compatible (le tag est le nom de fichier sans extension ni chemin).
        /// </summary>
        [Test]
        [SuppressMessage("ReSharper", "RedundantArgumentDefaultValue")]
        public void CalculateAndApplyImageBundleNormalizationToIncompleteFileBundle()
        {
            #region Étape 1 - On prépare les fichiers
            var originalImageZipFileNorm = BaseDir + Path.DirectorySeparatorChar + "cameraring_raw_images" +
                                       Path.DirectorySeparatorChar + "test3.zip";
            Assert.IsTrue(File.Exists(originalImageZipFileNorm));
            var outputDir = GetFullOutputDir();

            var outputToCalculateNorm = outputDir + "calculation" + Path.DirectorySeparatorChar;
            EnsureDirExists(outputToCalculateNorm);
            var filesToDestroy = Directory.GetFiles(outputToCalculateNorm);
            if (filesToDestroy.Length > 0)
            {
                foreach (var file in filesToDestroy)
                {
                    File.Delete(file);
                }
                filesToDestroy = Directory.GetFiles(outputToCalculateNorm);
            }
            Assert.IsTrue(filesToDestroy.Length == 0);
            var finalTransformationFileName = new FileInfo(outputToCalculateNorm + "normalisation.json");

            var outputFinal = outputDir + "complete" + Path.DirectorySeparatorChar;
            EnsureDirExists(outputFinal);
            filesToDestroy = Directory.GetFiles(outputFinal);
            if (filesToDestroy.Length > 0)
            {
                foreach (var file in filesToDestroy)
                {
                    File.Delete(file);
                }
                filesToDestroy = Directory.GetFiles(outputFinal);
            }
            Assert.IsTrue(filesToDestroy.Length == 0);

            var outputFinaMissing = outputDir + "missing" + Path.DirectorySeparatorChar;
            EnsureDirExists(outputFinaMissing);
            filesToDestroy = Directory.GetFiles(outputFinaMissing);
            if (filesToDestroy.Length > 0)
            {
                foreach (var file in filesToDestroy)
                {
                    File.Delete(file);
                }
                filesToDestroy = Directory.GetFiles(outputFinaMissing);
            }
            Assert.IsTrue(filesToDestroy.Length == 0);
            
            var zip = ZipFile.Read(originalImageZipFileNorm);
            zip.ExtractAll(outputToCalculateNorm);
            var filesToCalculateNorm = Directory.GetFiles(outputToCalculateNorm).ToList();
            FileUtil.SortFileCollection(filesToCalculateNorm);

            // On copie les images en les transformant en plus petit dans le répertoire final<
            var rand = new Random();
            foreach (var file in filesToCalculateNorm)
            {
                var fileName = StringUtil.GetFileName(file);
                var outputFinalFileName = outputFinal + fileName;

                var bmp = FileUtil.LoadBitmap(file);
                var tmp = bmp.Resize(1080, 720);
                tmp.SaveAsJpeg(90, outputFinalFileName);
                tmp.Dispose();
                bmp.Dispose();

                // On fait aussi une copie dans le répertoire
                var tmpR = rand.Next(0, 10);
                if (tmpR % 2 == 0)
                {
                    File.Copy(outputFinalFileName, outputFinaMissing + fileName);
                }
            }
            var filesFinal = Directory.GetFiles(outputFinal).ToList();
            FileUtil.SortFileCollection(filesFinal);
            var filesFinalMissing = Directory.GetFiles(outputFinaMissing).ToList();
            FileUtil.SortFileCollection(filesFinalMissing);
            #endregion

            /* Ces valeurs sont des variables d'entrées fixes. */
            var inputs = new List<CoordinatesInput>
            {
                new CoordinatesInput
                {
                    Zone = new Rectangle(1304, 697, 16, 20),
                    Radius = 100
                },
                new CoordinatesInput
                {
                    Zone = new Rectangle(1307, 997, 19, 13),
                    Radius = 100
                }
            };

            var transforms = OpenCvUtil.CalculateNormalizationTransforms(inputs, filesToCalculateNorm, false);
            Assert.IsNotNull(transforms);
            Assert.IsTrue(JsonUtil.WriteJsonFile(finalTransformationFileName, transforms));

            // Test sur l'intégrité des tags
            var set = new HashSet<string>();
            foreach (var trans in transforms.PicturesTransform)
            {
                set.Add(trans.Tag);
            }
            Assert.IsTrue(set.Count == transforms.PicturesTransform.Count);

            // On traite maintenant les fichiers pour les transformer.
            var result = OpenCvUtil.NormalizeFileBundleWithTransforms(filesFinal, transforms, new DirectoryInfo(outputFinal), null, null, null, false);
            Assert.IsTrue(result);

            result = OpenCvUtil.NormalizeFileBundleWithTransforms(filesFinalMissing, transforms, new DirectoryInfo(outputFinaMissing), null, null, null, false);
            Assert.IsTrue(result);

            // Vérification de l' "égalité" des fichiers générés.
            foreach (var file in filesFinalMissing)
            {
                var fileData = new FileInfo(file);
                // trouve son équivalent dans la liste complète
                var twinFile = new FileInfo(filesFinal.First(fi => StringUtil.GetFileName(fi).Equals(StringUtil.GetFileName(file))));
                Assert.IsTrue(twinFile.Exists);

                Assert.IsTrue(twinFile.Length == fileData.Length);
            }
        }

        /// <summary>
        /// Ce test effectue un calcul de normalisation sur des images qui sont prises avec une orientation autre que paysage.
        /// Cela vérifie que l'on est capable de considérer l'information EXIF des caméras.
        /// Cela peut s'appliquer à un ring de caméra en portrait.
        /// </summary>
        [Test]
        [SuppressMessage("ReSharper", "RedundantArgumentDefaultValue")]
        public void CalculateAndApplyImageBundleNormalizationToExifRotatedImages()
        {
            #region Étape 1 - On prépare les fichiers
            var originalImageZipFile = BaseDir + Path.DirectorySeparatorChar + "cameraring_raw_images" +
                                       Path.DirectorySeparatorChar + "test_exif.zip";
            Assert.IsTrue(File.Exists(originalImageZipFile));
            // Comme la vérification EXIF modifie les fichiers originaux, on sépare toutes les étapes pour vérifier le résultat.
            var outputDir = GetFullOutputDir();
            var outputDirOriginal = outputDir + "original" + Path.DirectorySeparatorChar;
            EnsureDirExists(outputDirOriginal);
            var outputDirCalculation = outputDir + "calculation" + Path.DirectorySeparatorChar;
            EnsureDirExists(outputDirCalculation);
            var outputDirFinal = outputDir + "final" + Path.DirectorySeparatorChar;
            EnsureDirExists(outputDirFinal);

            var allDirsToClean = new [] {outputDir, outputDirCalculation, outputDirOriginal, outputDirFinal};

            foreach (var dir in allDirsToClean)
            {
                var filesToDestroy = Directory.GetFiles(dir);
                if (filesToDestroy.Length > 0)
                {
                    foreach (var file in filesToDestroy)
                    {
                        File.Delete(file);
                    }
                    filesToDestroy = Directory.GetFiles(dir);
                }
                Assert.IsTrue(filesToDestroy.Length == 0);
            }

            var finalTransformationFileName = new FileInfo(outputDir + "result.json");

            ZipFile.Read(originalImageZipFile).ExtractAll(outputDirOriginal);
            ZipFile.Read(originalImageZipFile).ExtractAll(outputDirCalculation);

            var files = Directory.GetFiles(outputDirOriginal).ToList();
            FileUtil.SortFileCollection(files);
            #endregion

            /* Ces valeurs sont des variables d'entrées fixes. */
            var inputs = new List<CoordinatesInput>
            {
                new CoordinatesInput
                {
                    Zone = new Rectangle(629, 376, 18, 35),
                    Radius = 150
                },
                new CoordinatesInput
                {
                    Zone = new Rectangle(630, 779, 30, 18),
                    Radius = 150
                }
            };

            var transforms = OpenCvUtil.CalculateNormalizationTransforms(inputs, files, false);
            Assert.IsNotNull(transforms);
            Assert.IsTrue(JsonUtil.WriteJsonFile(finalTransformationFileName, transforms));
            Assert.IsTrue(transforms.GlobalParameters.Input.SequenceEqual(inputs));

            // Il ne peut y avoir de boundingBox interne plus grande que la première transformatio.
            // De plus, toutes les coordonnées de ces boundings box doivent être positifs, car ce sont tous des
            // rectangle plus petits que la taille originale. La seule exception est l'égalité et les indices 0,0
            var firstTransform = transforms.PicturesTransform[0].FinalBoundingBox;
            Assert.IsFalse(transforms.PicturesTransform.Any(t =>
                t.FinalBoundingBox.Width > firstTransform.Width ||
                t.FinalBoundingBox.Height > firstTransform.Height ||
                t.FinalBoundingBox.X < 0 ||
                t.FinalBoundingBox.Y < 0));

            // On traite maintenant les fichiers pour les transformer.
            files = Directory.GetFiles(outputDirCalculation).ToList();
            FileUtil.SortFileCollection(files);
            var result = OpenCvUtil.NormalizeFileBundleWithTransforms(files.ToList(), transforms, new DirectoryInfo(outputDirFinal), null, null, null, false);
            Assert.IsTrue(result);

            files = Directory.GetFiles(outputDirFinal).ToList();
            foreach (var file in files)
            {
                using (var img = FileUtil.LoadBitmap(file))
                {
                    Assert.IsTrue(img.Width == transforms.GlobalParameters.MinCrop.X);
                    Assert.IsTrue(img.Height == transforms.GlobalParameters.MinCrop.Y);
                }
            }
        }

        [Test]
        public void OpenCvTestMatrixManipulation()
        {
            var originalImageFile1 = new FileInfo(BaseDir + Path.DirectorySeparatorChar + "statue_model_01.png");
            Assert.IsTrue(originalImageFile1.Exists);
            var outputDir = GetFullOutputDir();
            var outputFile0 = outputDir + "result_00.png";
            var outputFile1 = outputDir + "result_01.png";
            var outputFile2 = outputDir + "result_02.png";
            EnsureFileNotExists(outputFile0, outputFile1, outputFile2);
            
            var alphaMat = new Mat(250, 250, DepthType.Cv8U, 1);

            // Image non-initialisée
            using (var image = alphaMat.ToImage<Gray, byte>())
            {
                using (var bmp = image.ToBitmap())
                {
                    Assert.That(bmp.Save(ImageFormat.Png, 9, outputFile1));
                    Assert.IsTrue(File.Exists(outputFile1));
                }
            }

            // Image initialisée avec même valeur
            using (var matrice = OpenCvUtil.GetMatrixFromOpenCvArray<byte>(alphaMat))
            {
                matrice.SetValue(128);
                
                using (var image = alphaMat.ToImage<Gray, byte>())
                {
                    using (var bmp = image.ToBitmap())
                    {
                        Assert.That(bmp.Save(ImageFormat.Png, 9, outputFile2));
                        Assert.IsTrue(File.Exists(outputFile2));
                    }
                }
            }
        }
        
        [Test]
        public void OpenCvBitmapCombination()
        {
            var originalImageFile1 = new FileInfo(BaseDir + Path.DirectorySeparatorChar + "statue_model_01.png");
            Assert.IsTrue(originalImageFile1.Exists);
            var originalImageFile2 = new FileInfo(BaseDir + Path.DirectorySeparatorChar + "statue_model_02.png");
            Assert.IsTrue(originalImageFile2.Exists);
            var outputDir = GetFullOutputDir();

            var blendTypesArr = Enum.GetValues(typeof(BlendMode));
            var ite = blendTypesArr.GetEnumerator();
            var blendTypes = new List<BlendMode>();
            while (ite.MoveNext())
            {
                if (ite.Current != null) blendTypes.Add((BlendMode)ite.Current);
            }
            var outputFiles = new List<string>();

            foreach (var blendType in blendTypes)
            {
                var outputFile0 = outputDir + $"result_{blendType}.png";
                EnsureFileNotExists(outputFile0);
                outputFiles.Add(outputFile0);
            }
            
            var img1 = OpenCvUtil.SplitImage<ChannelSplitImageRgb>(originalImageFile1);
            var img2 = OpenCvUtil.SplitImage<ChannelSplitImageRgb>(originalImageFile2);

            for (var i = 0; i < blendTypes.Count; i++)
            {
                Image<Bgra, byte> img;
                try
                {
                    img = OpenCvUtil.BlendImages(img1, img2, blendTypes[i]);
                }
                catch (NotImplementedException e)
                {
                    Console.Out.WriteLine(e.Message);
                    continue;
                }

                Assert.IsNotNull(img);
                Assert.IsTrue(img.ToBitmap().Save(ImageFormat.Png, 9, outputFiles[i]));
                Assert.IsTrue(File.Exists(outputFiles[i]));
                img.Dispose();
            }
        }
    }
}