using System.Runtime.InteropServices;
using System.Text;

namespace NomadLogic.iBrandDesktop.Util
{
    public class NikonD5000OneCamera
    {
        [DllImport("NikonD5000OneCamera.dll")]
        [return:MarshalAs(UnmanagedType.I1)]
        public static extern bool TakePicture();

        [DllImport("NikonD5000OneCamera.dll")]
        [return: MarshalAs(UnmanagedType.I1)]
        public static extern bool ConnectCamera();

        [DllImport("NikonD5000OneCamera.dll", CallingConvention = CallingConvention.Cdecl)]
        public static extern void GetPictureFileName(StringBuilder fileName);

        [DllImport("NikonD5000OneCamera.dll", CallingConvention = CallingConvention.Cdecl)]
        public static extern void GetPictureFileSize(out int fileSize);

        [DllImport("NikonD5000OneCamera.dll")]
        [return: MarshalAs(UnmanagedType.I1)]
        public static extern bool DisconnectCamera();

        [DllImport("NikonD5000OneCamera.dll")]
        [return: MarshalAs(UnmanagedType.I1)]
        public static extern bool ObtainPictureFile();
    }
}
