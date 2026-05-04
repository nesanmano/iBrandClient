using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.Drawing.Imaging;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using Emgu.CV;
using Emgu.CV.CvEnum;
using Emgu.CV.Structure;
using Emgu.CV.Util;
using NLog;
using NomadLogic.Common.Extentions;
using NomadLogic.Common.Util;
using NomadLogic.Common.Util.Json;
using NomadLogic.Common.Util.Math;
using NomadLogic.Multimedia.Gpu.OpenCV;
using NUnit.Framework;

namespace NomadLogic.Test.Common.Multimedia.Transformation.OpenCV
{
    public class OpenCvChromaKeyTest : TestBaseNunit
    {
        // ReSharper disable once UnusedMember.Local
        private static readonly Logger log = LogManager.GetCurrentClassLogger();

        /*
         * https://www.reddit.com/r/Python/comments/m68js/trying_to_write_a_very_simple_green_screen_code/
         * http://docs.opencv.org/3.0-beta/doc/py_tutorials/py_imgproc/py_colorspaces/py_colorspaces.html
         * http://stackoverflow.com/questions/20229577/opencv-cvcvtcolor-drops-alpha-channel-how-to-keep-alpha-data
         * http://stackoverflow.com/questions/35113979/calculate-distance-between-colors-in-hsv-space
         */

        /// <summary>
        /// Application d'un test de clé chromatique sur une seule image.
        /// </summary>
        [Test]
        public void OpenCvCkSingleImage()
        {
            var originalImageFile = BaseDir + Path.DirectorySeparatorChar + "not_keyed_bad.jpg";
            //var originalImageFile = BaseDir + Path.DirectorySeparatorChar + "keyed.png";
            Assert.IsTrue(File.Exists(originalImageFile));
            var outputDir = GetFullOutputDir();
            var luvFile = outputDir + "01_luv.jpg";
            EnsureFileNotExists(luvFile);
            var oriName = StringUtil.GetFileName(originalImageFile);
            if (!File.Exists(outputDir + "00_" + oriName))
                File.Copy(originalImageFile, outputDir + "00_" + oriName);

            var baseImage = CvInvoke.Imread(originalImageFile, ImreadModes.Unchanged);
            var rows = baseImage.Rows;
            var cols = baseImage.Cols;
            var depth = baseImage.Depth;
            var channels = baseImage.NumberOfChannels;
            var baseImageChannels = baseImage.Split();
            var baseImageBgr = new Mat(rows, cols, depth, 3);
            var transformedBaseImageBgr = new Mat(rows, cols, DepthType.Cv32F, 3);

            Mat baseAlpha = null;

            if (channels == 4)
            {
                baseAlpha = baseImageChannels[3];
            }

            if (channels >= 3)
            {
                var colorChannelsOriginal = new VectorOfMat();
                colorChannelsOriginal.Push(baseImageChannels[0]);
                colorChannelsOriginal.Push(baseImageChannels[1]);
                colorChannelsOriginal.Push(baseImageChannels[2]);

                CvInvoke.Merge(colorChannelsOriginal, baseImageBgr);
                baseImageBgr.ConvertTo(transformedBaseImageBgr, DepthType.Cv32F, 0.0039215686);
                    // Conversion de [0..255] à [0..1] sur 32 bit plutôt que 8 bits.

                Console.WriteLine($@"{baseImageBgr.GetValueRange().Min} - {baseImageBgr.GetValueRange().Max}");
                Console.WriteLine(
                    $@"{transformedBaseImageBgr.GetValueRange().Min} - {transformedBaseImageBgr.GetValueRange().Max}");
                Assert.IsTrue(transformedBaseImageBgr.GetValueRange().Min >= 0);
                Assert.IsTrue(transformedBaseImageBgr.GetValueRange().Max <= 1);
            }
            else
            {
                Assert.Fail("Not enough channels to do a chroma key analisys.");
            }

            /* Transformation vers l'espace de couleur LUV */
            var workingImage = new Mat(rows, cols, DepthType.Cv32F, 3);
            CvInvoke.CvtColor(transformedBaseImageBgr, workingImage, ColorConversion.Bgr2Luv);
            OpenCvUtil.SaveMatToImageFile(workingImage, luvFile);
            Console.WriteLine($@"{workingImage.GetValueRange().Min} - {workingImage.GetValueRange().Max}");
            Assert.IsTrue(workingImage.GetValueRange().Min >= -140);
            Assert.IsTrue(workingImage.GetValueRange().Max <= 220);

            var color = new[]
            {
                new[] {63.876187f, -23.441281f, 26.922020f, 15f, 3f}, // 119, 166, 124
                //new []{ 25.684846f, -18.790298f, 24.290229f, 5f, 0f }, // 024, 070, 024
                //new []{ 14.712139f, -3.3514500f, 10.336456f, 5f, 0f }  // 031, 040, 019
            };
            var alpha = new Mat(rows, cols, DepthType.Cv8U, 1);

            byte[] alphaOriginalData = null;
            if (baseAlpha != null)
            {
                alphaOriginalData = new byte[rows*cols];
                Marshal.Copy(baseAlpha.DataPointer, alphaOriginalData, 0, alphaOriginalData.Length);
            }

            // On traite maintenant les pixels, et on stocke le résultat dans l'alpha
            for (var perc = 0f; perc < 75.0f; perc += 15.0f)
            {
                for (var smoo = 0f; smoo < perc; smoo += perc*0.25f)
                {
                    var imgFromWork = workingImage.ToImage<Luv, float>();
                    var ptrDataWork = imgFromWork.Data;
                    var alphaData = new byte[rows*cols];
                    Marshal.Copy(alpha.DataPointer, alphaData, 0, alphaData.Length);

                    int i;
                    for (i = 0; i < rows; i++) {
                        int j;
                        for (j = 0; j < cols; j++)
                        {
                            if (alphaOriginalData != null && alphaOriginalData[(i*cols) + j] == 0)
                            {
                                alphaData[(i*cols) + j] = 0;
                                continue;
                            }

                            var l = ptrDataWork[i, j, 0];
                            var u = ptrDataWork[i, j, 1];
                            var v = ptrDataWork[i, j, 2];

                            /* TRÈS TRÈS haut impact de performance
                            // Vérifie les ranges valide de valeur LUV
                            Assert.IsTrue(0 <= l && l <= 100, $"({i},{j}) L [0..100] => {l}");
                            Assert.IsTrue(-134 <= u && u <= 220, $"({i},{j}) U [-134..220] => {l}");
                            Assert.IsTrue(-140 <= v && v <= 122, $"({i},{j}) V [-140..122] => {l}");
                            */

                            var cAlpha = byte.MaxValue;
                            for (var c = 0; c < color.Length; c++)
                            {
                                if (cAlpha == 0) break;

                                var calcDist = MathUtil.DistanceBetweenVectors(
                                    l, u, v,
                                    color[c][0], color[c][1], color[c][2]);

                                var tPerc = perc;
                                var tSmoo = smoo;
                                if (c != 0)
                                {
                                    tPerc = color[c][3];
                                    tSmoo = color[c][4];
                                }

                                var smooMod = Math.Min(tSmoo, tPerc);

                                var mAlpha = byte.MaxValue;
                                if (calcDist < tPerc - smooMod)
                                {
                                    mAlpha = 0;
                                }
                                else if (calcDist < tPerc)
                                {
                                    mAlpha = (byte) ((calcDist - tPerc + smooMod)/smooMod*255);
                                }

                                if (mAlpha < cAlpha)
                                    cAlpha = mAlpha;
                            }

                            if (alphaOriginalData != null)
                            {
                                alphaData[(i*cols) + j] = Math.Min(alphaOriginalData[(i*cols) + j], cAlpha);
                            }
                            else
                            {
                                alphaData[(i*cols) + j] = cAlpha;
                            }
                        }
                    }

                    Marshal.Copy(alphaData, 0, alpha.DataPointer, alphaData.Length);

                    OpenCvUtil.SaveMatToImageFile(alpha, outputDir + $"04_alpha_{perc:0}_{smoo:0.0}.jpg");

                    /* Reconstruction de l'image en BGRA */
                    var colorChannels = new VectorOfMat();
                    colorChannels.Push(baseImageChannels[0]);
                    colorChannels.Push(baseImageChannels[1]);
                    colorChannels.Push(baseImageChannels[2]);
                    colorChannels.Push(alpha);

                    var finalChannels = new Mat(rows, cols, depth, 4);
                    CvInvoke.Merge(colorChannels, finalChannels);

                    OpenCvUtil.SaveMatToImageFile(finalChannels, outputDir + $"06_result_{perc:0}_{smoo:0.0}.png");
                }
            }
        }

        [Test]
        public void HsvDistanceTest()
        {
            var colorKey = new[] {(byte) (121*255/360), (byte) (27*255/100), (byte) (66*255/100)};
            var wantedDistance = 0.7;
            var smoothing = 0.2;

            var colors = new[]
            {
                (byte) (131*255/360), (byte) (37*255/100), (byte) (61*255/100)
                , (byte) (99*255/360), (byte) (72*255/100), (byte) (28*255/100)
                , (byte) (104*255/360), (byte) (71*255/100), (byte) (36*255/100)
                , (byte) (20*255/360), (byte) (44*255/100), (byte) (79*255/100)
            };

            for (var i = 0; i < colors.Length; i += 3)
            {
                var dist = MathUtil.DistanceBetweenHsvColors(colorKey[0], colorKey[1], colorKey[2], colors[0 + i],
                    colors[1 + i], colors[2 + i]);

                byte val = 255;
                if (dist < wantedDistance - smoothing)
                {
                    val = 0;
                }
                else if (dist < wantedDistance)
                {
                    val = (byte) ((dist - smoothing)/smoothing*255);
                }

                Console.WriteLine(
                    $@"Distance between [{colorKey[0]},{colorKey[1]},{colorKey[2]}] and [{colors[0 + i]},{colors[1 + i]},{colors
                        [2 + i]}] = {dist}, opacity = {val/255f*100f:0.00}%");
                Assert.IsTrue(dist >= 0 && dist <= 1);
            }

        }

        [Test]
        public void LuvDistanceTest()
        {
            var colorKey = new[] {59.6365f, -24.221746f, 26.049253f};
            var wantedDistance = 35;
            var smoothing = 3;

            // RGB = 
            // 124,175,127 (vert proche)
            // 130,168,125 (vert proche avec un peu plus d'ombre)
            // 23,84,27 (vert très sombre et légèrement plus ombragé, valeur limite)
            // 143,90,52 (couleur pas verte du tout (brun))
            var colors = new[]
            {
                colorKey[0], colorKey[1], colorKey[2]
                , 66.883044f, -25.391550f, 30.829614f
                , 65.146113f, -19.118391f, 28.203010f
                , 30.882971f, -24.647094f, 30.632529f
                , 43.259722f, 40.362917f, 29.476692f
            };

            for (var i = 0; i < colors.Length; i += 3)
            {
                var dist = MathUtil.DistanceBetweenVectors(colorKey[0], colorKey[1], colorKey[2], colors[0 + i],
                    colors[1 + i], colors[2 + i]);

                byte val = 255;
                if (dist < wantedDistance - smoothing)
                {
                    val = 0;
                }
                else if (dist < wantedDistance)
                {
                    val = (byte) ((dist - smoothing)/smoothing*255);
                }

                Console.WriteLine(
                    $@"Distance between [{colorKey[0]:0.00},{colorKey[1]:0.00},{colorKey[2]:0.00}] and [{colors[0 + i]:0.00},{colors
                        [1 + i]:0.00},{colors[2 + i]:0.00}] = {dist:0.00}, opacity = {val/255f*100f:0.00}%");
            }

        }

        [Test]
        public void TestOpenCvRgbToLuv()
        {
            var rgb = new byte[] {170, 230, 90};
            var luv = new float[3];
            var luvTarget = new[]
            {
                84.9340780384747f,
                -29.9462419969877f,
                80.9233621500983f
            };

            MathUtil.RgbToLuv(rgb, luv);

            Assert.IsTrue(
                (0 <= luv[0] && luv[0] <= 100) && (-134 <= luv[1] && luv[1] <= 220) && (-140 <= luv[2] && luv[2] <= 122),
                // ReSharper disable once UseStringInterpolation
                string.Format("L [0..100], U [-134..220], V [-140..122] => {0}, {1}, {2} (RGB=[{3},{4},{5}])",
                    luv[0].ToString("0.00000000", CultureInfo.InvariantCulture),
                    luv[1].ToString("0.00000000", CultureInfo.InvariantCulture),
                    luv[2].ToString("0.00000000", CultureInfo.InvariantCulture),
                    rgb[0], rgb[1], rgb[2]));

            for (var i = 0; i < luv.Length; i++)
            {
                Assert.IsTrue(MathUtil.IsEqualWithTolerance(luv[i], luvTarget[i]),
                    $"{luv[i].ToString("000.00000000000000", CultureInfo.InvariantCulture)} != {luvTarget[i].ToString("000.00000000000000", CultureInfo.InvariantCulture)}");
            }
        }

        [Test]
        public void TestOpenCvChromaKeyInfoDiskSave()
        {
            var outputDir = GetFullOutputDir();
            var keyInfoFile = new FileInfo(outputDir + "key.info");
            EnsureFileNotExists(keyInfoFile.FullName);

            var rand = new Random(100);
            var keyInfo = new ChromaKeyInfoOpenCvCieLuv();
            for (var i = 0; i < 10; i++)
            {
                var data = new ChromaKeyInfoOpenCvCieLuvData
                {
                    Tolerance = rand.NextDouble()*15,
                    Smoothing = rand.NextDouble()*15,
                    R = (byte) rand.Next(0, 255),
                    G = (byte) rand.Next(0, 255),
                    B = (byte) rand.Next(0, 255)
                };

                Assert.IsTrue(
                    (0 <= data.L && data.L <= 100) && (-134 <= data.U && data.U <= 220) &&
                    (-140 <= data.V && data.V <= 122),
                    // ReSharper disable once UseStringInterpolation
                    string.Format("L [0..100], U [-134..220], V [-140..122] => {0}, {1}, {2} (RGB=[{3},{4},{5}])",
                        data.L.ToString("0.00000000", CultureInfo.InvariantCulture),
                        data.U.ToString("0.00000000", CultureInfo.InvariantCulture),
                        data.V.ToString("0.00000000", CultureInfo.InvariantCulture),
                        data.R, data.G, data.B));

                keyInfo.KeyingData.Add(data);
            }

            JsonUtil.WriteJsonFile(keyInfoFile, keyInfo);

            var fromFile = JsonUtil.ReadJsonFile<ChromaKeyInfoOpenCvCieLuv>(keyInfoFile);
            Assert.AreEqual(keyInfo, fromFile, "File read is not equal to generated file.");
        }

        [Test]
        public void TestOpenCvChromaKey()
        {
            var originalImageFile = BaseDir + Path.DirectorySeparatorChar + "not_keyed_bad.jpg";
            //var originalImageFile = BaseDir + Path.DirectorySeparatorChar + "keyed.png";
            Assert.IsTrue(File.Exists(originalImageFile));
            Assert.IsTrue(File.Exists(new FileInfo(originalImageFile).FullName));
            var outputDir = GetFullOutputDir();
            var oriName = StringUtil.RemoveExtension(StringUtil.GetFileName(originalImageFile));
            var keyedImageFile = outputDir + "keyed_" + oriName + ".png";
            EnsureFileNotExists(keyedImageFile);

            #region KeyInfo

            var keyInfo = new ChromaKeyInfoOpenCvCieLuv();
            keyInfo.KeyingData.Add(new ChromaKeyInfoOpenCvCieLuvData //119, 166, 124
            {
                Tolerance = 15,
                Smoothing = 10,
                R = 119,
                G = 166,
                B = 124
            });
            keyInfo.KeyingData.Add(new ChromaKeyInfoOpenCvCieLuvData //024, 070, 024
            {
                Tolerance = 15,
                Smoothing = 10,
                R = 24,
                G = 70,
                B = 24
            });
            keyInfo.KeyingData.Add(new ChromaKeyInfoOpenCvCieLuvData //031, 040, 019
            {
                Tolerance = 15,
                Smoothing = 10,
                R = 31,
                G = 40,
                B = 19
            });

            #endregion

            OpenCvUtil.ChromaKeyOpenCv(originalImageFile, keyedImageFile, keyInfo);

            Assert.IsTrue(File.Exists(keyedImageFile));
        }

        [Test]
        public void TestOpenCvChromaKeyMultipleToBitmap()
        {
            var originalImageFile = BaseDir + Path.DirectorySeparatorChar + "not_keyed_bad.jpg";
            //var originalImageFile = BaseDir + Path.DirectorySeparatorChar + "keyed.png";
            Assert.IsTrue(File.Exists(originalImageFile));
            Assert.IsTrue(File.Exists(new FileInfo(originalImageFile).FullName));
            var outputDir = GetFullOutputDir();
            var textFileResult = outputDir + "stats.txt";
            
            #region KeyInfo

            var keyInfo = new ChromaKeyInfoOpenCvCieLuv();
            keyInfo.KeyingData.Add(new ChromaKeyInfoOpenCvCieLuvData //119, 166, 124
            {
                Tolerance = 15,
                Smoothing = 10,
                R = 119,
                G = 166,
                B = 124
            });
            keyInfo.KeyingData.Add(new ChromaKeyInfoOpenCvCieLuvData //024, 070, 024
            {
                Tolerance = 15,
                Smoothing = 10,
                R = 24,
                G = 70,
                B = 24
            });
            keyInfo.KeyingData.Add(new ChromaKeyInfoOpenCvCieLuvData //031, 040, 019
            {
                Tolerance = 15,
                Smoothing = 10,
                R = 31,
                G = 40,
                B = 19
            });

            #endregion

            using (var split = OpenCvUtil.SplitImage<ChannelSplitImageLuv>(originalImageFile))
            {

                var timer = new Stopwatch();
                var timeList = new List<long>();

                for (var i = 0; i < 5; i++)
                {
                    timer.Restart();

                    var bmp = OpenCvUtil.ChromaKeyOpenCv(split, keyInfo);

                    timer.Stop();
                    timeList.Add(timer.ElapsedMilliseconds);

                    Assert.IsTrue(bmp != null, "Created bitmap was null");
                    Assert.IsTrue(bmp.Width == 1728, "Incorrect size");
                    Assert.IsTrue(bmp.Height == 2592, "Incorrect size");
                    Assert.IsTrue(bmp.PixelFormat == PixelFormat.Format32bppArgb, "Incorrect size");

                    bmp.Dispose();
                }

                var count = $@"Iterations   : {timeList.Count}";
                var min = $@"Min time     : {timeList.Min()}";
                var max = $@"Max time     : {timeList.Max()}";
                var median = $@"Median time  : {timeList.Median()}";
                var average = $@"Average time : {timeList.Average()}";

                Console.WriteLine(count);
                Console.WriteLine(max);
                Console.WriteLine(min);
                Console.WriteLine(median);
                Console.WriteLine(average);

                using (var file = new StreamWriter(textFileResult, true))
                {
                    file.WriteLine(DateTime.Now.ToLocalTime());
                    file.WriteLine(count);
                    file.WriteLine(max);
                    file.WriteLine(min);
                    file.WriteLine(median);
                    file.WriteLine(average);
                    file.WriteLine();
                }
            }
        }

        [Test]
        public void TestOpenCvChromaKeyMultipleToImageData()
        {
            var originalImageFile = BaseDir + Path.DirectorySeparatorChar + "not_keyed_bad.jpg";
            //var originalImageFile = BaseDir + Path.DirectorySeparatorChar + "keyed.png";
            Assert.IsTrue(File.Exists(originalImageFile));
            Assert.IsTrue(File.Exists(new FileInfo(originalImageFile).FullName));
            var outputDir = GetFullOutputDir();
            var textFileResult = outputDir + "stats.txt";
            var outputFile = outputDir + "result.png";
            EnsureFileNotExists(outputFile);

            #region KeyInfo

            var keyInfo = new ChromaKeyInfoOpenCvCieLuv();
            keyInfo.KeyingData.Add(new ChromaKeyInfoOpenCvCieLuvData //119, 166, 124
            {
                Tolerance = 15,
                Smoothing = 10,
                R = 119,
                G = 166,
                B = 124
            });
            keyInfo.KeyingData.Add(new ChromaKeyInfoOpenCvCieLuvData //024, 070, 024
            {
                Tolerance = 15,
                Smoothing = 10,
                R = 24,
                G = 70,
                B = 24
            });
            keyInfo.KeyingData.Add(new ChromaKeyInfoOpenCvCieLuvData //031, 040, 019
            {
                Tolerance = 15,
                Smoothing = 10,
                R = 31,
                G = 40,
                B = 19
            });

            #endregion

            var split = OpenCvUtil.SplitImage<ChannelSplitImageLuv>(originalImageFile);

            var timer = new Stopwatch();
            var timeList = new List<long>();

            var image = split.GetReferencedImageDataFromSplit();

            for (var i = 0; i < 5; i++)
            {
                timer.Restart();

                OpenCvUtil.ChromaKeyOpenCv(split, keyInfo, false, null, image.Data);

                timer.Stop();
                timeList.Add(timer.ElapsedMilliseconds);
                
            }

            image.Bitmap.Save(ImageFormat.Png, 9, outputFile);

            var count = $@"Iterations   : {timeList.Count}";
            var min = $@"Min time     : {timeList.Min()}";
            var max = $@"Max time     : {timeList.Max()}";
            var median = $@"Median time  : {timeList.Median()}";
            var average = $@"Average time : {timeList.Average()}";

            Console.WriteLine(count);
            Console.WriteLine(max);
            Console.WriteLine(min);
            Console.WriteLine(median);
            Console.WriteLine(average);

            using (var file = new StreamWriter(textFileResult, true))
            {
                file.WriteLine(DateTime.Now.ToLocalTime());
                file.WriteLine(count);
                file.WriteLine(max);
                file.WriteLine(min);
                file.WriteLine(median);
                file.WriteLine(average);
                file.WriteLine();
            }
        }

        [Test]
        public void TestOpenCvChromaKeyMultipleToImageDataResized()
        {
            var originalImageFile = BaseDir + Path.DirectorySeparatorChar + "not_keyed_bad.jpg";
            //var originalImageFile = BaseDir + Path.DirectorySeparatorChar + "keyed.png";
            Assert.IsTrue(File.Exists(originalImageFile));
            Assert.IsTrue(File.Exists(new FileInfo(originalImageFile).FullName));
            var outputDir = GetFullOutputDir();
            var textFileResult = outputDir + "stats.txt";
            var outputFile = outputDir + "result.png";
            EnsureFileNotExists(outputFile);

            #region KeyInfo

            var keyInfo = new ChromaKeyInfoOpenCvCieLuv();
            keyInfo.KeyingData.Add(new ChromaKeyInfoOpenCvCieLuvData //119, 166, 124
            {
                Tolerance = 15,
                Smoothing = 10,
                R = 119,
                G = 166,
                B = 124
            });
            keyInfo.KeyingData.Add(new ChromaKeyInfoOpenCvCieLuvData //024, 070, 024
            {
                Tolerance = 15,
                Smoothing = 10,
                R = 24,
                G = 70,
                B = 24
            });
            keyInfo.KeyingData.Add(new ChromaKeyInfoOpenCvCieLuvData //031, 040, 019
            {
                Tolerance = 15,
                Smoothing = 10,
                R = 31,
                G = 40,
                B = 19
            });

            #endregion

            var split = OpenCvUtil.SplitImage<ChannelSplitImageLuv>(originalImageFile, new Size(173, 259));

            var timer = new Stopwatch();
            var timeList = new List<long>();

            var image = split.GetReferencedImageDataFromSplit();

            for (var i = 0; i < 5; i++)
            {
                timer.Restart();

                var d = image.Data;
                
                OpenCvUtil.ChromaKeyOpenCv(split, keyInfo, false, null, d);

                timer.Stop();
                timeList.Add(timer.ElapsedMilliseconds);

            }

            image.Bitmap.Save(ImageFormat.Png, 9, outputFile);

            var count = $@"Iterations   : {timeList.Count}";
            var min = $@"Min time     : {timeList.Min()}";
            var max = $@"Max time     : {timeList.Max()}";
            var median = $@"Median time  : {timeList.Median()}";
            var average = $@"Average time : {timeList.Average()}";

            Console.WriteLine(count);
            Console.WriteLine(max);
            Console.WriteLine(min);
            Console.WriteLine(median);
            Console.WriteLine(average);

            using (var file = new StreamWriter(textFileResult, true))
            {
                file.WriteLine(DateTime.Now.ToLocalTime());
                file.WriteLine(count);
                file.WriteLine(max);
                file.WriteLine(min);
                file.WriteLine(median);
                file.WriteLine(average);
                file.WriteLine();
            }
        }

        [Test]
        public void TestOpenCvChromaKeyWithAlphaCorrection()
        {
            var originalImageFile = BaseDir + Path.DirectorySeparatorChar + "not_keyed_bad.jpg";
            Assert.IsTrue(File.Exists(originalImageFile));
            var outputDir = GetFullOutputDir();
            var oriName = StringUtil.RemoveExtension(StringUtil.GetFileName(originalImageFile));
            var keyedImageFile = outputDir + "keyed_" + oriName + ".png";
            EnsureFileNotExists(keyedImageFile);
            var keyedFilteredImageFile = outputDir + "keyedFiltered_" + oriName + ".png";
            EnsureFileNotExists(keyedFilteredImageFile);
            
            var keyInfo = ChromaKeyInfoOpenCvCieLuv.NewInstanceChromaKeyGreen();

            var filterList = new [] {
                                 OpenCvFilterFactory.MedianFilter(5),
                                 OpenCvFilterFactory.Erode(new Size(3,3), 1),
                                 OpenCvFilterFactory.BoxFilter(new Size(3,3), true)
                             };

            var watch = new Stopwatch();

            watch.Restart();
            OpenCvUtil.ChromaKeyOpenCv(originalImageFile, keyedImageFile, keyInfo, OpenCvFilterFactory.NoFilter());
            Assert.IsTrue(File.Exists(keyedImageFile));
            watch.Stop();
            Console.WriteLine($@"Elapsed (No filters): {watch.ElapsedMilliseconds} ms");

            watch.Restart();
            OpenCvUtil.ChromaKeyOpenCv(originalImageFile, keyedFilteredImageFile, keyInfo, filterList);
            Assert.IsTrue(File.Exists(keyedFilteredImageFile));
            watch.Stop();
            Console.WriteLine($@"Elapsed (Filtered  ): {watch.ElapsedMilliseconds} ms");
        }
    }
}