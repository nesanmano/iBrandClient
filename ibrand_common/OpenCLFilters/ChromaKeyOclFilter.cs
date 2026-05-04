using OpenCLNet;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Imaging;
using System.Windows.Forms;
using System.Xml;
using NomadLogic.iBrand.Transformation.Filters;

namespace NomadLogic.iBrand.Transformation.OpenCLFilters
{
    public class ChromaKeyOclFilter : AbstractChromaKeyFilter
    {
        private float _lowerHue = 75; // 75
        private float _higherHue = 180; //180
        private float _lowerSaturation = 0.3f; //0.3
        private float _higherSaturation = 1; // 1
        private float _lowerLuminance = 0.15f; //0.15
        private float _higherLuminance = 0.93f; //0.97
        private float _semiLowerHue = 60; //60
        private float _semiHigherHue = 190; //190
        private float _semiLowerSaturation = 0.15f; // 0.15
        private float _semiHigherSaturation = 1; // 1
        private float _semiLowerLuminance = 0.12f; //0.15
        private float _semiHigherLuminance = 0.97f; //0.97
        private OpenCLManager _oclMan;
        private Program _oclProgram;
        private Kernel _greenScreenKernel;
        readonly IntPtr[] _greenScreenGlobalWorkSize = new IntPtr[2];

        private readonly object _syncLock = new object();

        public ChromaKeyOclFilter ()
        {
            InitializeOpenCl();
        }

        /// <summary>
        /// Create a OpenCLManager, configure it, and then create a context using all devices in platform 0,
        /// Once the context is up and running we compile our source file "OpenCLFunctions.cl"
        /// The Helper automatically compiles and creates kernels.
        /// We can then extract named kernels using the GetKernel method.
        /// 
        /// For more advanced scenarios, one might use the functions in the Platform class
        /// to query devices, create contexts etc. Platforms can be enumerated using
        /// for( int i=0; i smaller OpenCL.NumberofPlatforms; i++ )
        ///     Platform p = OpenCL.GetPlatform(i);
        /// </summary>
        private void InitializeOpenCl()
        {
            if (OpenCL.NumberOfPlatforms == 0)
            {
                MessageBox.Show("OpenCL not available");
                Application.Exit();
            }

            _oclMan = new OpenCLManager();
            // Attempt to save binaries after compilation, as well as load precompiled binaries
            // to avoid compilation. Usually you'll want this to be true. 
            _oclMan.AttemptUseBinaries = false;
            // Attempt to compile sources. This should probably be true for almost all projects.
            // Setting it to false means that when you attempt to compile "mysource.cl", it will
            // only scan the precompiled binary directory for a binary corresponding to a source
            // with that name. There's a further restriction that the compiled binary also has to
            // use the same Defines and BuildOptions
            _oclMan.AttemptUseSource = true;
            // Binary and source paths
            // This is where we store our sources and where compiled binaries are placed
            _oclMan.BinaryPath = @"OpenCLFilters\OpenCL\bin";
            _oclMan.SourcePath = @"OpenCLFilters\OpenCL\src";
            // If true, RequireImageSupport will filter out any devices without image support
            // In this project we don't need image support though, so we Set it to false
            _oclMan.RequireImageSupport = false;
            // The Defines string gets prepended to any and all sources that are compiled
            // and serve as a convenient way to pass configuration information to the compilation process
            _oclMan.Defines = "#define MyCompany_MyProject_Define 1";
            // The BuildOptions string is passed directly to clBuild and can be used to do debug builds etc
            _oclMan.BuildOptions = "";

            _oclMan.CreateDefaultContext(0, DeviceType.ALL);

            _oclProgram = _oclMan.CompileFile("GreenScreen.cl");

            _greenScreenKernel = _oclProgram.CreateKernel("GreenScreen");
        }

        public override Bitmap Filter(Bitmap source)
        {
            var outputBitmap = new Bitmap(source.Width, source.Height, PixelFormat.Format32bppArgb);
            outputBitmap.SetResolution(300, 300);
            var rect = new Rectangle(0, 0, source.Width, source.Height);
            var bmData = source.LockBits(rect, ImageLockMode.ReadOnly,
                source.PixelFormat);
            var bmDataDest = outputBitmap.LockBits(rect, ImageLockMode.WriteOnly,
                PixelFormat.Format32bppArgb);

            var stride = bmData.Stride;
            var strideDest = bmDataDest.Stride;
            var scan0 = bmData.Scan0;
            var scan0Dest = bmDataDest.Scan0;
            var width = source.Width;
            var height = source.Height;

            lock (_syncLock)
            {

                var inputBuffer0 = _oclMan.Context.CreateBuffer(MemFlags.USE_HOST_PTR, stride * bmData.Height, scan0);
                var outputBuffer = _oclMan.Context.CreateBuffer(MemFlags.USE_HOST_PTR, strideDest * bmDataDest.Height, scan0Dest);

                var deviceIndex = 0;

                _greenScreenGlobalWorkSize[0] = (IntPtr)width;
                _greenScreenGlobalWorkSize[1] = (IntPtr)height;
                _greenScreenKernel.SetArg(0, width);
                _greenScreenKernel.SetArg(1, height);
                _greenScreenKernel.SetArg(2, inputBuffer0);
                _greenScreenKernel.SetArg(3, stride);
                _greenScreenKernel.SetArg(4, outputBuffer);
                _greenScreenKernel.SetArg(5, strideDest);
                _greenScreenKernel.SetArg(6, _lowerHue);
                _greenScreenKernel.SetArg(7, _higherHue);
                _greenScreenKernel.SetArg(8, _lowerSaturation);
                _greenScreenKernel.SetArg(9, _higherSaturation);
                _greenScreenKernel.SetArg(10, _lowerLuminance);
                _greenScreenKernel.SetArg(11, _higherLuminance);
                _greenScreenKernel.SetArg(12, _semiLowerHue);
                _greenScreenKernel.SetArg(13, _semiHigherHue);
                _greenScreenKernel.SetArg(14, _semiLowerSaturation);
                _greenScreenKernel.SetArg(15, _semiHigherSaturation);
                _greenScreenKernel.SetArg(16, _semiLowerLuminance);
                _greenScreenKernel.SetArg(17, _semiHigherLuminance);

                _oclMan.CQ[deviceIndex].EnqueueNDRangeKernel(_greenScreenKernel, 2, null, _greenScreenGlobalWorkSize, null);
                _oclMan.CQ[deviceIndex].EnqueueBarrier();
                var p = _oclMan.CQ[deviceIndex].EnqueueMapBuffer(outputBuffer, true, MapFlags.READ, IntPtr.Zero, (IntPtr)(strideDest * bmDataDest.Height));
                _oclMan.CQ[deviceIndex].EnqueueUnmapMemObject(outputBuffer, p);
                _oclMan.CQ[deviceIndex].Flush();
                _oclMan.CQ[deviceIndex].Finish();
                outputBuffer.Dispose();
                inputBuffer0.Dispose();

                // Unlock the bits.
                source.UnlockBits(bmData);
                outputBitmap.UnlockBits(bmDataDest);
            }

            return outputBitmap;
        }

        /*public void filter(System.IntPtr Scan0, int stride, int width, int height)
        {

            Mem InputBuffer0 = OCLMan.Context.CreateBuffer(MemFlags.USE_HOST_PTR, stride * height, Scan0);

            int deviceIndex = 0;

            GreenScreenGlobalWorkSize[0] = (IntPtr)width;
            GreenScreenGlobalWorkSize[1] = (IntPtr)height;
            GreenScreenRealTimeKernel.SetArg(0, width);
            GreenScreenRealTimeKernel.SetArg(1, height);
            GreenScreenRealTimeKernel.SetArg(2, InputBuffer0);
            GreenScreenRealTimeKernel.SetArg(3, stride);
            GreenScreenRealTimeKernel.SetArg(4, LOWER_HUE);
            GreenScreenRealTimeKernel.SetArg(5, HIGHER_HUE);
            GreenScreenRealTimeKernel.SetArg(6, LOWER_SATURATION);
            GreenScreenRealTimeKernel.SetArg(7, HIGHER_SATURATION);
            GreenScreenRealTimeKernel.SetArg(8, LOWER_LUMINANCE);
            GreenScreenRealTimeKernel.SetArg(9, HIGHER_LUMINANCE);
            GreenScreenRealTimeKernel.SetArg(10, SEMI_LOWER_HUE);
            GreenScreenRealTimeKernel.SetArg(11, SEMI_HIGHER_HUE);
            GreenScreenRealTimeKernel.SetArg(12, SEMI_LOWER_SATURATION);
            GreenScreenRealTimeKernel.SetArg(13, SEMI_HIGHER_SATURATION);
            GreenScreenRealTimeKernel.SetArg(14, SEMI_LOWER_LUMINANCE);
            GreenScreenRealTimeKernel.SetArg(15, SEMI_HIGHER_LUMINANCE);

            OCLMan.CQ[deviceIndex].EnqueueNDRangeKernel(GreenScreenKernel, 2, null, GreenScreenGlobalWorkSize, null);
            OCLMan.CQ[deviceIndex].EnqueueBarrier();
            IntPtr p = OCLMan.CQ[deviceIndex].EnqueueMapBuffer(InputBuffer0, true, MapFlags.READ_WRITE, IntPtr.Zero, (IntPtr)(stride * height));
            OCLMan.CQ[deviceIndex].EnqueueUnmapMemObject(InputBuffer0, p);
            OCLMan.CQ[deviceIndex].Flush();
            OCLMan.CQ[deviceIndex].Finish();
            InputBuffer0.Dispose();

            return;
        }*/

        protected override byte[] FilterRgb(byte alpha, byte r, byte g, byte b)
        {
            return new byte[] { 0, 0, 0, 0 };
        }
        public override void ParseXml(XmlTextReader reader)
        {

        }

        public override void SetParameters(Dictionary<string, float> parameters)
        {
//            float value;
            if (parameters.ContainsKey("LOWER_HUE"))
            {
                _lowerHue = Convert.ToInt32(parameters["LOWER_HUE"]);
            }

            if (parameters.ContainsKey("HIGHER_HUE"))
            {
                _higherHue = Convert.ToInt32(parameters["HIGHER_HUE"]);
            }

            if (parameters.ContainsKey("LOWER_SATURATION"))
            {
                _lowerSaturation = parameters["LOWER_SATURATION"];
            }

            if (parameters.ContainsKey("HIGHER_SATURATION"))
            {
                _higherSaturation = parameters["HIGHER_SATURATION"];
            }

            if (parameters.ContainsKey("LOWER_LUMINANCE"))
            {
                _lowerLuminance = parameters["LOWER_LUMINANCE"];
            }

            if (parameters.ContainsKey("HIGHER_LUMINANCE"))
            {
                _higherLuminance = parameters["HIGHER_LUMINANCE"];
            }

            if (parameters.ContainsKey("SEMI_LOWER_HUE"))
            {
                _semiLowerHue = Convert.ToInt32(parameters["SEMI_LOWER_HUE"]);
            }

            if (parameters.ContainsKey("SEMI_HIGHER_HUE"))
            {
                _semiHigherHue = Convert.ToInt32(parameters["SEMI_HIGHER_HUE"]);
            }

            if (parameters.ContainsKey("SEMI_LOWER_SATURATION"))
            {
                _semiLowerSaturation = parameters["SEMI_LOWER_SATURATION"];
            }

            if (parameters.ContainsKey("SEMI_HIGHER_SATURATION"))
            {
                _semiHigherSaturation = parameters["SEMI_HIGHER_SATURATION"];
            }

            if (parameters.ContainsKey("SEMI_LOWER_LUMINANCE"))
            {
                _semiLowerLuminance = parameters["SEMI_LOWER_LUMINANCE"];
            }

            if (parameters.ContainsKey("SEMI_HIGHER_LUMINANCE"))
            {
                _semiHigherLuminance = parameters["SEMI_HIGHER_LUMINANCE"];
            }
        }
    }
}
