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
    class OpenCLFilter : ImageFilter
    {
        private OpenCLManager OCLMan;
        private Program OCLProgram;
        private Kernel OCLKernel;
        private IntPtr[] GlobalWorkSize = new IntPtr[2];
        private string CLFileName;
        private string KernelName;
        private Dictionary<int, float> FilterParams;

        private readonly object syncLock = new object();

        public OpenCLFilter(string CLFileName, string KernelName, Dictionary<int, float> FilterParams)
        {
            this.CLFileName = CLFileName;
            this.KernelName = KernelName;
            this.FilterParams = FilterParams;
            InitializeOpenCL();
        }

        /// <summary>
        /// Create a OpenCLManager, configure it, and then create a context using all devices in platform 0,
        /// Once the context is up and running we compile our source file "OpenCLFunctions.cl"
        /// The Helper automatically compiles and creates kernels.
        /// We can then extract named kernels using the GetKernel method.
        /// 
        /// For more advanced scenarios, one might use the functions in the Platform class
        /// to query devices, create contexts etc. Platforms can be enumerated using
        /// for( int i=0; i<OpenCL.NumberofPlatforms; i++ )
        ///     Platform p = OpenCL.GetPlatform(i);
        /// </summary>
        private void InitializeOpenCL()
        {
            if (OpenCL.NumberOfPlatforms == 0)
            {
                MessageBox.Show("OpenCL not available");
                Application.Exit();
            }

            OCLMan = new OpenCLManager();
            // Attempt to save binaries after compilation, as well as load precompiled binaries
            // to avoid compilation. Usually you'll want this to be true. 
            OCLMan.AttemptUseBinaries = false;
            // Attempt to compile sources. This should probably be true for almost all projects.
            // Setting it to false means that when you attempt to compile "mysource.cl", it will
            // only scan the precompiled binary directory for a binary corresponding to a source
            // with that name. There's a further restriction that the compiled binary also has to
            // use the same Defines and BuildOptions
            OCLMan.AttemptUseSource = true;
            // Binary and source paths
            // This is where we store our sources and where compiled binaries are placed
            OCLMan.BinaryPath = @"OpenCLFilters\OpenCL\bin";
            OCLMan.SourcePath = @"OpenCLFilters\OpenCL\src";
            // If true, RequireImageSupport will filter out any devices without image support
            // In this project we don't need image support though, so we Set it to false
            OCLMan.RequireImageSupport = false;
            // The Defines string gets prepended to any and all sources that are compiled
            // and serve as a convenient way to pass configuration information to the compilation process
            OCLMan.Defines = "#define MyCompany_MyProject_Define 1";
            // The BuildOptions string is passed directly to clBuild and can be used to do debug builds etc
            OCLMan.BuildOptions = "";

            OCLMan.CreateDefaultContext(0, DeviceType.ALL);

            OCLProgram = OCLMan.CompileFile(CLFileName);

            OCLKernel = OCLProgram.CreateKernel(KernelName);
        }

        public override Bitmap Filter(Bitmap source)
        {
            Bitmap OutputBitmap = new Bitmap(source.Width, source.Height, PixelFormat.Format32bppArgb);
            OutputBitmap.SetResolution(300, 300);
            Rectangle rect = new Rectangle(0, 0, source.Width, source.Height);
            BitmapData bmData = source.LockBits(rect, ImageLockMode.ReadOnly,
                source.PixelFormat);
            BitmapData bmData_dest = OutputBitmap.LockBits(rect, ImageLockMode.WriteOnly,
                PixelFormat.Format32bppArgb);

            int stride = bmData.Stride;
            int stride_dest = bmData_dest.Stride;
            IntPtr Scan0 = bmData.Scan0;
            IntPtr Scan0_dest = bmData_dest.Scan0;
            int width = source.Width;
            int height = source.Height;

            lock (syncLock)
            {

                Mem InputBuffer0 = OCLMan.Context.CreateBuffer(MemFlags.USE_HOST_PTR, stride * bmData.Height, Scan0);
                Mem outputBuffer = OCLMan.Context.CreateBuffer(MemFlags.USE_HOST_PTR, stride_dest * bmData_dest.Height, Scan0_dest);

                int deviceIndex = 0;

                GlobalWorkSize[0] = (IntPtr)width;
                GlobalWorkSize[1] = (IntPtr)height;
                OCLKernel.SetArg(0, width);
                OCLKernel.SetArg(1, height);
                OCLKernel.SetArg(2, InputBuffer0);
                OCLKernel.SetArg(3, stride);
                OCLKernel.SetArg(4, outputBuffer);
                OCLKernel.SetArg(5, stride_dest);
                if (FilterParams != null)
                {
                    foreach (int index in FilterParams.Keys)
                    {
                        OCLKernel.SetArg(index+6, FilterParams[index]);
                    }
                }

                OCLMan.CQ[deviceIndex].EnqueueNDRangeKernel(OCLKernel, 2, null, GlobalWorkSize, null);
                OCLMan.CQ[deviceIndex].EnqueueBarrier();
                IntPtr p = OCLMan.CQ[deviceIndex].EnqueueMapBuffer(outputBuffer, true, MapFlags.READ, IntPtr.Zero, (IntPtr)(stride_dest * bmData_dest.Height));
                OCLMan.CQ[deviceIndex].EnqueueUnmapMemObject(outputBuffer, p);
                OCLMan.CQ[deviceIndex].Flush();
                OCLMan.CQ[deviceIndex].Finish();
                outputBuffer.Dispose();
                InputBuffer0.Dispose();

                // Unlock the bits.
                source.UnlockBits(bmData);
                OutputBitmap.UnlockBits(bmData_dest);
            }

            return OutputBitmap;
        }

        protected override byte[] FilterRgb(byte alpha, byte r, byte g, byte b)
        {
            return new byte[] { 0, 0, 0, 0 };
        }

        public override void ParseXml(XmlTextReader reader)
        {
            bool layerFinished = false;
            while (!layerFinished)
            {
                reader.Read();
                switch (reader.NodeType)
                {
                    case XmlNodeType.Element:
                        if (reader.Name.Equals("param_filter"))
                        {
                            string paramName = reader.GetAttribute("index");
                            int index = int.Parse(paramName);
                            string value = reader.GetAttribute("value");
                            float paramValue = float.Parse(value);
                            FilterParams.Add(index, paramValue);
                        }
                        break;
                    case XmlNodeType.EndElement:
                        if (reader.Name.Equals("image_filter"))
                        {
                            layerFinished = true;
                        }
                        break;
                }
            }
        }
    }
}
