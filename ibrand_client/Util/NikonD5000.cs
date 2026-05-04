using System.Runtime.InteropServices;

namespace NomadLogic.iBrandDesktop.Util
{
    public class NikonD5000
    {
        [DllImport("NikonD5000.dll",CallingConvention = CallingConvention.Cdecl)]
        public static extern void ConnectCameras(int nbrCamera, out int nbrCameraConnected);

        [DllImport("NikonD5000.dll", CallingConvention = CallingConvention.Cdecl)]
        public static extern void GetPictures(out int nbrPictureRetrieved);

        [DllImport("NikonD5000.dll")]
        public static extern void DisconnectCameras();

        [DllImport("NikonD5000.dll", CallingConvention = CallingConvention.Cdecl)]
        public static extern void GetNumberOfCameras(out int nbrCamera);
    }
}
