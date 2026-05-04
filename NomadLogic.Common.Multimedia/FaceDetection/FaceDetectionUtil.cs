using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using JetBrains.Annotations;
using NomadLogic.Common.Extentions;
using NomadLogic.Multimedia.FaceDetection.Harr;
using NomadLogic.Multimedia.FaceDetection.Harr.Hid;

// ReSharper disable InconsistentNaming

// ReSharper disable CompareOfFloatsByEqualityOperator

namespace NomadLogic.Multimedia.FaceDetection
{
    [PublicAPI]
    internal class FaceDetectionUtil
    {
        private const int MinimumNeighbors = 0;

        public static List<AvgComp> DetectFaces(Bitmap image, HaarClassifierCascade haarClassifierCascade, HarrSize minimumWindowSize = null, double scaleFactor = 1.1f) {
            var cascade = haarClassifierCascade;

            if (cascade == null)
            {
                throw new NullReferenceException("Haar Cascade object inexistent. Cannot process face detection.");
            }

            var minimumSize = minimumWindowSize ?? new HarrSize(30,30);
            Bitmap resizedImage;
            var changeRatio = 1.0f;
            
            if(image.Width > 640 && image.Height > 480) {
        	    var ratioWidth = 640.0f / image.Width;
        	    var ratioHeight = 480.0f / image.Height;
        	    changeRatio = Math.Max(ratioWidth, ratioHeight);
            	
        	    resizedImage = image.Scale((int)(image.Width * changeRatio), (int)(image.Height * changeRatio));
            } else {
        	    resizedImage = image;
            }
            var img = Picture.FromImageGrayScale(resizedImage);

            //Picture où chaque pixel contient l'addition de toutes les valeurs à sa gauche
            // Picture où chaque pixel contient l'addition des carré de toutes les valeurs à sa gauche
            int splitStage = 2, nPass = 2;

            var temp = new Picture(img.Width, img.Height);
            var sum = new Picture(img.Width + 1, img.Height + 1);
            var squaredSum = new PictureDouble(img.Width + 1, img.Height + 1);

            if (null == cascade.HidCascade) {
                try {
                    cascade.HidCascade = HidHaarClassifierCascade.Compute(cascade);
                } catch (Exception) {
                    //e.printStackTrace();
                }
            }

            var seq = new List<Rectangle>();
            //AvgComp[] comps;

            {
                GetIntegralImages(img, sum, squaredSum);

                if (cascade.HidCascade != null && (splitStage >= cascade.Count ||
                                                   cascade.HidCascade.IsTree)) {
                    splitStage = cascade.Count;
                    nPass = 1;
                }

                double factor;
                for (factor = 1;
                              factor * cascade.OriginalWindowSize.Width <
                              img.Width - 10 &&
                              factor * cascade.OriginalWindowSize.Height <
                              img.Height - 10;
                              factor *= scaleFactor) {
                    var yStep = Math.Max(2, factor);
                    var windowSize = new HarrSize(
                        Math.Round(cascade.OriginalWindowSize.Width * factor),
                        Math.Round(cascade.OriginalWindowSize.Height * factor));
                    //Rectangle equ_rect = new Rectangle(0, 0, 0, 0);
                    int pass, stageOffset = 0;
                    var stopHeight = (int) Math.Round(((img.Height -
                            windowSize.Height) / yStep));

                    if (windowSize.Width < minimumSize.Width ||
                        windowSize.Height < minimumSize.Height)
                        continue;

                    try {
                        cascade.SetImages(sum, squaredSum, factor);
                    } catch (Exception) {
                        //e.printStackTrace();
                    }

                    cascade.HidCascade.Count = splitStage;
                    for (pass = 0; pass < nPass; pass++) {
                        for (var _iy = 0; _iy < stopHeight; _iy++) {
                            var iy = (int) Math.Round(_iy * yStep);
                            int _ix, _xstep = 1;
                            var stop_width = (int) Math.Round((img.Width -
                                    windowSize.Width) / yStep);

                            for (_ix = 0; _ix < stop_width; _ix += _xstep) {
                                var ix = (int) Math.Round(_ix * yStep); // it really should be ystep

                                // première passe
                                if (pass == 0) {
                                    int result;
                                    _xstep = 2;

                                    try {
                                        result = cascade.Run(new HaarPoint(ix, iy),
                                                0);
                                    } catch (Exception) {
                                        result = 0;
                                        //e.printStackTrace();
                                    }
                                    if (result > 0) {
                                        // si on est pas la dernière passe, on sauvegarde la position
                                        if (pass < nPass - 1)
                                            temp.Set(iy, ix, 1);
                                        // si c'est la dernière passe, on fait un rectangle
                                        else {
                                            var rect = new Rectangle(ix, iy,
                                                    windowSize.Width,
                                                    windowSize.Height);
                                            seq.Add(rect);
                                        }
                                    }

                                    if (result < 0)
                                        _xstep = 1;
                                }
                                // si on a pas déjà trouvé de carré là
                                else if (temp.Get(iy, ix) != 0) {
                                    int result;
                                    try {
                                        result = cascade.Run(new HaarPoint(ix, iy),
                                                stageOffset);
                                    } catch (Exception) {
                                        result = 0;
                                        //e.printStackTrace();
                                    }
                                    if (result > 0) {
                                        if (pass == nPass - 1) {
                                            var rect = new Rectangle(ix, iy,
                                                    windowSize.Width,
                                                    windowSize.Height);
                                            seq.Add(rect);
                                        }
                                    } else
                                        temp.Set(iy, ix, 0);
                                }
                            }
                        }
                        stageOffset = cascade.HidCascade.Count;
                        cascade.HidCascade.Count = cascade.Count;
                    }
                }
            }

            /*
            if (minimumNeighbors != 0) {
                // group retrieved rectangles in order to filter out noise
                int[] indexSequence = PartitionSequence(seq);
                int nComp = indexSequence.Length; //cvSeqPartition(seq, 0, &idx_seq, is_equal, 0);
                comps = new AvgComp[nComp + 1];

                for (int i = 0; i < nComp + 1; i++) {
                    comps[i] = new AvgComp();
                }

                // count number of neighbors
                for (int i = 0; i < seq.Count; i++) {
                    Rectangle r1 = seq[i];
                    int index = indexSequence[i];
                    if (index < nComp) {

                        comps[index].Neighbors = (comps[index].Neighbors + 1);
                        Rectangle tmpRec = comps[index].Rectangle;
                        tmpRec.X += r1.X;
                        tmpRec.Y += r1.Y;
                        tmpRec.Width += r1.Width;
                        tmpRec.Height += r1.Height;
                        comps[index].Rectangle = tmpRec;
                    }
                }

                // calculate average bounding box
                for (int i = 0; i < nComp; i++) {
                    int n = comps[i].Neighbors;
                    if (n >= minimumNeighbors) {
                        AvgComp comp = new AvgComp();

                        Rectangle r = comp.Rectangle;
                        r.X = (comps[i].Rectangle.X * 2 + n) / (2 * n);
                        r.Y = (comps[i].Rectangle.Y * 2 + n) / (2 * n);
                        r.Width = (comps[i].Rectangle.Width * 2 + n) / (2 * n);
                        r.Height = (comps[i].Rectangle. Height * 2 + n) / (2 * n);
                        comp.Rectangle = r;
                        comp.Neighbors = (comps[i].Neighbors);

                        seq2.Add(comp);
                    }
                }

                // filter out small face rectangles inside large face rectangles
                for (int i = 0; i < seq2.Count; i++) {
                    AvgComp r1 = seq2[i];
                    int flag = 1;

                    for (int j = 0; j < seq2.Count; j++) {
                        AvgComp r2 = seq2[j];
                        int distance = (int) Math.Round(r2.Rectangle.Width *
                                0.2);

                        if (i != j &&
                            r1.Rectangle.X >= r2.Rectangle.X - distance &&
                            r1.Rectangle.Y >= r2.Rectangle.Y - distance &&
                            r1.Rectangle.X + r1.Rectangle.Width <=
                            r2.Rectangle.X + r2.Rectangle.Width +
                            distance &&
                            r1.Rectangle.Y + r1.Rectangle.Height <=
                            r2.Rectangle.Y + r2.Rectangle.Height +
                            distance &&
                            (r2.Neighbors > Math.Max(3, r1.Neighbors) ||
                             r1.Neighbors < 3)) {
                            flag = 0;
                            break;
                        }
                    }

                    if (flag != 0) {
                        resultSequence.Add(r1);
                    }
                }
            } else {
            */
            var resultSequence = seq.Select(t => new AvgComp(t, 0)).ToList();

            // On divise par le ratio pour retourner à la grandeur de l'image originale
            if (changeRatio == 1.0f) return resultSequence;
            foreach(var avgComp in resultSequence) {
                var r = avgComp.Rectangle;
                r.X = (int)(r.X / changeRatio);
                r.Y = (int) (r.Y / changeRatio);
                r.Width = (int)(r.Width / changeRatio);
                r.Height = (int)(r.Height / changeRatio);
                avgComp.Rectangle = r;
            }

            return resultSequence;
        }

        static void GetIntegralImages(Picture source, Picture sum,
                                      PictureDouble squaredSum) {
            int y;

            for (y = 1; y < source.Height; y++)
            {
                double sq;
                int s;
                int x;
                for (x = 0, s = 0, sq = 0; x < source.Width; x++) {
                    var it = source.Get(y - 1, x);
                    var t = it;
                    double tq = icv8x32fSqrTab[it + 128];

                    s += t;
                    sq += tq;

                    sum.Set(y, x, sum.Get(y - 1, x) + s);
                    squaredSum.Set(y, x, squaredSum.Get(y - 1, x) + sq);
                }
            }
        }

        static int[] PartitionSequence(List<Rectangle> sequence) {
            var part = new int[sequence.Count];
            for (var i = 0; i < sequence.Count; i++) {
                var dist = Double.MaxValue;
                var _i = sequence[i];

                for (var j = 0; j < sequence.Count; j++) {
                    if (i == j)
                        continue;

                    var _j = sequence[j];
                    var curDist = Math.Sqrt(Math.Pow(_i.X - _j.X, 2) +
                                               Math.Pow(_i.Y - _j.Y, 2));
                    if (curDist < dist) {
                        part[i] = j;
                        dist = curDist;
                    }
                }
            }

            return part;
        }

        private static float[] icv8x32fSqrTab = new float[] {
                                                      16384.0f, 16129.0f, 15876.0f,
                                                      15625.0f, 15376.0f, 15129.0f,
                                                      14884.0f, 14641.0f,
                                                      14400.0f, 14161.0f, 13924.0f,
                                                      13689.0f, 13456.0f, 13225.0f,
                                                      12996.0f, 12769.0f,
                                                      12544.0f, 12321.0f, 12100.0f,
                                                      11881.0f, 11664.0f, 11449.0f,
                                                      11236.0f, 11025.0f,
                                                      10816.0f, 10609.0f, 10404.0f,
                                                      10201.0f, 10000.0f, 9801.0f,
                                                      9604.0f, 9409.0f,
                                                      9216.0f, 9025.0f, 8836.0f,
                                                      8649.0f, 8464.0f, 8281.0f,
                                                      8100.0f, 7921.0f,
                                                      7744.0f, 7569.0f, 7396.0f,
                                                      7225.0f, 7056.0f, 6889.0f,
                                                      6724.0f, 6561.0f,
                                                      6400.0f, 6241.0f, 6084.0f,
                                                      5929.0f, 5776.0f, 5625.0f,
                                                      5476.0f, 5329.0f,
                                                      5184.0f, 5041.0f, 4900.0f,
                                                      4761.0f, 4624.0f, 4489.0f,
                                                      4356.0f, 4225.0f,
                                                      4096.0f, 3969.0f, 3844.0f,
                                                      3721.0f, 3600.0f, 3481.0f,
                                                      3364.0f, 3249.0f,
                                                      3136.0f, 3025.0f, 2916.0f,
                                                      2809.0f, 2704.0f, 2601.0f,
                                                      2500.0f, 2401.0f,
                                                      2304.0f, 2209.0f, 2116.0f,
                                                      2025.0f, 1936.0f, 1849.0f,
                                                      1764.0f, 1681.0f,
                                                      1600.0f, 1521.0f, 1444.0f,
                                                      1369.0f, 1296.0f, 1225.0f,
                                                      1156.0f, 1089.0f,
                                                      1024.0f, 961.0f, 900.0f,
                                                      841.0f, 784.0f, 729.0f,
                                                      676.0f, 625.0f,
                                                      576.0f, 529.0f, 484.0f,
                                                      441.0f, 400.0f, 361.0f,
                                                      324.0f, 289.0f,
                                                      256.0f, 225.0f, 196.0f,
                                                      169.0f, 144.0f, 121.0f,
                                                      100.0f, 81.0f,
                                                      64.0f, 49.0f, 36.0f, 25.0f,
                                                      16.0f, 9.0f, 4.0f, 1.0f,
                                                      0.0f, 1.0f, 4.0f, 9.0f, 16.0f,
                                                      25.0f, 36.0f, 49.0f,
                                                      64.0f, 81.0f, 100.0f, 121.0f,
                                                      144.0f, 169.0f, 196.0f,
                                                      225.0f,
                                                      256.0f, 289.0f, 324.0f,
                                                      361.0f, 400.0f, 441.0f,
                                                      484.0f, 529.0f,
                                                      576.0f, 625.0f, 676.0f,
                                                      729.0f, 784.0f, 841.0f,
                                                      900.0f, 961.0f,
                                                      1024.0f, 1089.0f, 1156.0f,
                                                      1225.0f, 1296.0f, 1369.0f,
                                                      1444.0f, 1521.0f,
                                                      1600.0f, 1681.0f, 1764.0f,
                                                      1849.0f, 1936.0f, 2025.0f,
                                                      2116.0f, 2209.0f,
                                                      2304.0f, 2401.0f, 2500.0f,
                                                      2601.0f, 2704.0f, 2809.0f,
                                                      2916.0f, 3025.0f,
                                                      3136.0f, 3249.0f, 3364.0f,
                                                      3481.0f, 3600.0f, 3721.0f,
                                                      3844.0f, 3969.0f,
                                                      4096.0f, 4225.0f, 4356.0f,
                                                      4489.0f, 4624.0f, 4761.0f,
                                                      4900.0f, 5041.0f,
                                                      5184.0f, 5329.0f, 5476.0f,
                                                      5625.0f, 5776.0f, 5929.0f,
                                                      6084.0f, 6241.0f,
                                                      6400.0f, 6561.0f, 6724.0f,
                                                      6889.0f, 7056.0f, 7225.0f,
                                                      7396.0f, 7569.0f,
                                                      7744.0f, 7921.0f, 8100.0f,
                                                      8281.0f, 8464.0f, 8649.0f,
                                                      8836.0f, 9025.0f,
                                                      9216.0f, 9409.0f, 9604.0f,
                                                      9801.0f, 10000.0f, 10201.0f,
                                                      10404.0f, 10609.0f,
                                                      10816.0f, 11025.0f, 11236.0f,
                                                      11449.0f, 11664.0f, 11881.0f,
                                                      12100.0f, 12321.0f,
                                                      12544.0f, 12769.0f, 12996.0f,
                                                      13225.0f, 13456.0f, 13689.0f,
                                                      13924.0f, 14161.0f,
                                                      14400.0f, 14641.0f, 14884.0f,
                                                      15129.0f, 15376.0f, 15625.0f,
                                                      15876.0f, 16129.0f,
                                                      16384.0f, 16641.0f, 16900.0f,
                                                      17161.0f, 17424.0f, 17689.0f,
                                                      17956.0f, 18225.0f,
                                                      18496.0f, 18769.0f, 19044.0f,
                                                      19321.0f, 19600.0f, 19881.0f,
                                                      20164.0f, 20449.0f,
                                                      20736.0f, 21025.0f, 21316.0f,
                                                      21609.0f, 21904.0f, 22201.0f,
                                                      22500.0f, 22801.0f,
                                                      23104.0f, 23409.0f, 23716.0f,
                                                      24025.0f, 24336.0f, 24649.0f,
                                                      24964.0f, 25281.0f,
                                                      25600.0f, 25921.0f, 26244.0f,
                                                      26569.0f, 26896.0f, 27225.0f,
                                                      27556.0f, 27889.0f,
                                                      28224.0f, 28561.0f, 28900.0f,
                                                      29241.0f, 29584.0f, 29929.0f,
                                                      30276.0f, 30625.0f,
                                                      30976.0f, 31329.0f, 31684.0f,
                                                      32041.0f, 32400.0f, 32761.0f,
                                                      33124.0f, 33489.0f,
                                                      33856.0f, 34225.0f, 34596.0f,
                                                      34969.0f, 35344.0f, 35721.0f,
                                                      36100.0f, 36481.0f,
                                                      36864.0f, 37249.0f, 37636.0f,
                                                      38025.0f, 38416.0f, 38809.0f,
                                                      39204.0f, 39601.0f,
                                                      40000.0f, 40401.0f, 40804.0f,
                                                      41209.0f, 41616.0f, 42025.0f,
                                                      42436.0f, 42849.0f,
                                                      43264.0f, 43681.0f, 44100.0f,
                                                      44521.0f, 44944.0f, 45369.0f,
                                                      45796.0f, 46225.0f,
                                                      46656.0f, 47089.0f, 47524.0f,
                                                      47961.0f, 48400.0f, 48841.0f,
                                                      49284.0f, 49729.0f,
                                                      50176.0f, 50625.0f, 51076.0f,
                                                      51529.0f, 51984.0f, 52441.0f,
                                                      52900.0f, 53361.0f,
                                                      53824.0f, 54289.0f, 54756.0f,
                                                      55225.0f, 55696.0f, 56169.0f,
                                                      56644.0f, 57121.0f,
                                                      57600.0f, 58081.0f, 58564.0f,
                                                      59049.0f, 59536.0f, 60025.0f,
                                                      60516.0f, 61009.0f,
                                                      61504.0f, 62001.0f, 62500.0f,
                                                      63001.0f, 63504.0f, 64009.0f,
                                                      64516.0f, 65025.0f
        };

    }
}
