using System;
using System.Diagnostics;
using System.Drawing;
using System.IO;
using Emgu.CV;
using Emgu.CV.CvEnum;
using Emgu.CV.Structure;
using NLog;
using NomadLogic.Multimedia.Gpu.OpenCV;
using NUnit.Framework;

namespace NomadLogic.Test.Common.Multimedia.Transformation.OpenCV
{
    public class OpenCvFilterTest : TestBaseNunit
    {
        // ReSharper disable once UnusedMember.Local
        private static readonly Logger log = LogManager.GetCurrentClassLogger();
        
        [Test]
        public void OpenCvMatFilterGallery()
        {
            var originalColorImageFile = BaseDir + Path.DirectorySeparatorChar + "statue_model_02.png";
            Assert.IsTrue(File.Exists(originalColorImageFile));
            var outputDir = GetFullOutputDir();

            var splitColor = OpenCvUtil.SplitImage<ChannelSplitImageRgb>(originalColorImageFile);
            var splitAlpha = splitColor.GetOriginalAlphaImage();
            Assert.IsNotNull(splitAlpha);
            
            // BilinearFilter
            ExecuteOpenCvFilter(outputDir, splitAlpha.Mat, splitColor, "Bilateral", CvInvoke.BilateralFilter, 10, 30, 30);

            // Blur
            ExecuteOpenCvFilter(outputDir, splitAlpha.Mat, splitColor, "Blur", CvInvoke.Blur, new Size(5,5), new Point(-1,-1));

            // Box filter
            ExecuteOpenCvFilter(outputDir, splitAlpha.Mat, splitColor, "BoxFilter", CvInvoke.BoxFilter, DepthType.Cv8U, new Size(5,5), new Point(-1,-1));

            // Dilate
            ExecuteOpenCvFilter(outputDir, splitAlpha.Mat, splitColor, "Dilate", CvInvoke.Dilate, ElementShape.Rectangle, new Size(5,5), new Point(-1,-1), 1);

            // Erode
            ExecuteOpenCvFilter(outputDir, splitAlpha.Mat, splitColor, "Erode", CvInvoke.Erode, ElementShape.Rectangle, new Size(5,5), new Point(-1,-1), 1);

            // Filter 2D
            //ExecuteOpenCvFilter(outputDir, splitAlpha, splitColor, "Filter2D", CvInvoke.Filter2D, ElementShape.Rectangle, new Size(5,5), new Point(-1,-1), 0);

            // Gaussian blur
            ExecuteOpenCvFilter(outputDir, splitAlpha.Mat, splitColor, "GaussianBlur", CvInvoke.GaussianBlur, new Size(5,5), 0);

            // Median blur
            ExecuteOpenCvFilter(outputDir, splitAlpha.Mat, splitColor, "MedianBlur", CvInvoke.MedianBlur, 5);

            // Pyr Down
            ExecuteOpenCvFilter(outputDir, splitAlpha.Mat, splitColor, "PyrDown", CvInvoke.PyrDown);

            // Pyr Up
            ExecuteOpenCvFilter(outputDir, splitAlpha.Mat, splitColor, "PyrUp", CvInvoke.PyrUp);

            // Pyr Mean Shift
            ExecuteOpenCvFilter(outputDir, splitColor, "PyrMeanShift", CvInvoke.PyrMeanShiftFiltering, 5, 5, 2, 3);
        }

        private void ExecuteOpenCvFilter(string outputDir, ChannelSplitImageRgb splitColor, string filterName, 
            Action<IInputArray, IOutputArray, double, double, int, MCvTermCriteria> openCvMethod, double spatialWindow, double spatialColor, int maxLevel, int eps)
        {
            BeforeExecution(outputDir, filterName, out var matAlpha, out var matColor, out var watch, out var fileAlphaPath, out var fileColorPath);
            
            openCvMethod(splitColor.BaseImageBgr, matColor, spatialWindow, spatialColor, maxLevel, new MCvTermCriteria(eps));

            AfterFilterExecution(filterName, matAlpha, matColor, watch, fileAlphaPath, fileColorPath);
        }

        private void ExecuteOpenCvFilter(string outputDir, Mat splitAlpha, ChannelSplitImageRgb splitColor, string filterName, 
            Action<IInputArray, IOutputArray, BorderType> openCvMethod)
        {
            BeforeExecution(outputDir, filterName, out var matAlpha, out var matColor, out var watch, out var fileAlphaPath, out var fileColorPath);
            
            openCvMethod(splitAlpha, matAlpha, BorderType.Reflect101);
            openCvMethod(splitColor.BaseMat, matColor, BorderType.Reflect101);

            AfterFilterExecution(filterName, matAlpha, matColor, watch, fileAlphaPath, fileColorPath);
        }

        private void ExecuteOpenCvFilter(string outputDir, Mat splitAlpha, ChannelSplitImageRgb splitColor, string filterName, 
            Action<IInputArray, IOutputArray, int> openCvMethod, int size)
        {
            BeforeExecution(outputDir, filterName, out var matAlpha, out var matColor, out var watch, out var fileAlphaPath, out var fileColorPath);
            
            openCvMethod(splitAlpha, matAlpha, size);
            openCvMethod(splitColor.BaseMat, matColor, size);

            AfterFilterExecution(filterName, matAlpha, matColor, watch, fileAlphaPath, fileColorPath);
        }

        private void ExecuteOpenCvFilter(string outputDir, Mat splitAlpha, ChannelSplitImageRgb splitColor, string filterName, 
            Action<IInputArray, IOutputArray, Size, double, double, BorderType> openCvMethod, Size size, int sigma)
        {
            BeforeExecution(outputDir, filterName, out var matAlpha, out var matColor, out var watch, out var fileAlphaPath, out var fileColorPath);
            
            openCvMethod(splitAlpha, matAlpha, size, sigma, 0d, BorderType.Reflect101);
            openCvMethod(splitColor.BaseMat, matColor, size, sigma, 0d, BorderType.Reflect101);

            AfterFilterExecution(filterName, matAlpha, matColor, watch, fileAlphaPath, fileColorPath);
        }

        private void ExecuteOpenCvFilter(string outputDir, Mat splitAlpha, ChannelSplitImageRgb splitColor, string filterName, 
            Action<IInputArray, IOutputArray, IInputArray, Point, double, BorderType> openCvMethod, ElementShape shape, Size size, Point point, double delta)
        {
            BeforeExecution(outputDir, filterName, out var matAlpha, out var matColor, out var watch, out var fileAlphaPath, out var fileColorPath);
            
            matAlpha = new Mat(splitAlpha.Width, splitAlpha.Height, splitAlpha.Depth, splitAlpha.NumberOfChannels);
            matColor = new Mat(splitColor.BaseMat.Width, splitColor.BaseMat.Height, splitColor.BaseMat.Depth, splitColor.BaseMat.NumberOfChannels);

            var elem = CvInvoke.GetStructuringElement(shape, size, point);
            openCvMethod(splitAlpha, matAlpha, elem, point, delta, BorderType.Reflect101);
            openCvMethod(splitColor.BaseMat, matColor, elem, point, delta, BorderType.Reflect101);

            AfterFilterExecution(filterName, matAlpha, matColor, watch, fileAlphaPath, fileColorPath);
        }

        private void ExecuteOpenCvFilter(string outputDir, Mat splitAlpha, ChannelSplitImageRgb splitColor, string filterName, 
            Action<IInputArray, IOutputArray, IInputArray, Point, int, BorderType, MCvScalar> openCvMethod, ElementShape shape, Size size, Point point, int iteration)
        {
            BeforeExecution(outputDir, filterName, out var matAlpha, out var matColor, out var watch, out var fileAlphaPath, out var fileColorPath);

            var elem = CvInvoke.GetStructuringElement(shape, size, point);
            openCvMethod(splitAlpha, matAlpha, elem, point, iteration, BorderType.Reflect101, new MCvScalar(0,0,0,0));
            openCvMethod(splitColor.BaseMat, matColor, elem, point, iteration, BorderType.Reflect101, new MCvScalar(0,0,0,0));

            AfterFilterExecution(filterName, matAlpha, matColor, watch, fileAlphaPath, fileColorPath);
        }

        private void ExecuteOpenCvFilter(string outputDir, Mat splitAlpha, ChannelSplitImageRgb splitColor, string filterName, 
            Action<IInputArray, IOutputArray, DepthType, Size, Point, bool, BorderType> openCvMethod, DepthType depth, Size size, Point point)
        {
            BeforeExecution(outputDir, filterName, out var matAlpha, out var matColor, out var watch, out var fileAlphaPath, out var fileColorPath);

            openCvMethod(splitAlpha, matAlpha, depth, size, point, true, BorderType.Reflect101);
            openCvMethod(splitColor.BaseMat, matColor, depth, size, point, true, BorderType.Reflect101);

            AfterFilterExecution(filterName, matAlpha, matColor, watch, fileAlphaPath, fileColorPath);
        }
        
        private void ExecuteOpenCvFilter(string outputDir, Mat splitAlpha, ChannelSplitImageRgb splitColor, string filterName, 
            Action<IInputArray, IOutputArray, Size, Point, BorderType> openCvMethod, Size arg1, Point arg2)
        {
            BeforeExecution(outputDir, filterName, out var matAlpha, out var matColor, out var watch, out var fileAlphaPath, out var fileColorPath);

            openCvMethod(splitAlpha, matAlpha, arg1, arg2, BorderType.Reflect101);
            openCvMethod(splitColor.BaseMat, matColor, arg1, arg2, BorderType.Reflect101);

            AfterFilterExecution(filterName, matAlpha, matColor, watch, fileAlphaPath, fileColorPath);
        }

        private void ExecuteOpenCvFilter(string outputDir, Mat splitAlpha, ChannelSplitImageRgb splitColor, string filterName, 
            Action<IInputArray, IOutputArray, int, double, double, BorderType> openCvMethod, int arg1, double arg2, double arg3)
        {
            BeforeExecution(outputDir, filterName, out var matAlpha, out var matColor, out var watch, out var fileAlphaPath, out var fileColorPath);

            openCvMethod(splitAlpha, matAlpha, arg1, arg2, arg3, BorderType.Reflect101);
            openCvMethod(splitColor.BaseImageBgr, matColor, arg1, arg2, arg3, BorderType.Reflect101);

            AfterFilterExecution(filterName, matAlpha, matColor, watch, fileAlphaPath, fileColorPath);
        }

        private void BeforeExecution(string outputDir, string filterName, out Mat matAlpha, out Mat matColor, out Stopwatch watch, out string fileAlphaPath, out string fileColorPath)
        {
            matAlpha = new Mat();
            matColor = new Mat();
            watch = new Stopwatch();
            fileAlphaPath = outputDir + filterName + "_alpha.jpg";
            fileColorPath = outputDir + filterName + "_color.png";
            EnsureFileNotExists(fileAlphaPath);
            EnsureFileNotExists(fileColorPath);
            watch.Restart();
        }

        private static void AfterFilterExecution(string filterName, Mat matAlpha, Mat matColor, Stopwatch watch, string fileAlphaPath, string fileColorPath)
        {
            Assert.IsTrue(OpenCvUtil.SaveMatToImageFile(matAlpha, fileAlphaPath));
            Assert.IsTrue(OpenCvUtil.SaveMatToImageFile(matColor, fileColorPath));
            watch.Stop();
            Console.WriteLine(@"{1} Filter: {0} ms", watch.ElapsedMilliseconds, filterName);
            matAlpha.Dispose();
            matColor.Dispose();
        }
        
    }
}