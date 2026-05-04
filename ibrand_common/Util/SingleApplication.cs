using System;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Threading;

namespace NomadLogic.Common.Util
{
    public class SingleApplication
    {
        private static Mutex _mutex;
        private const int SwRestore = 9;

        [DllImport("user32.dll")]
        private static extern int ShowWindow(IntPtr hWnd, int nCmdShow);

        [DllImport("user32.dll")]
        private static extern int SetForegroundWindow(IntPtr hWnd);

        [DllImport("user32.dll")]
        private static extern int IsIconic(IntPtr hWnd);

        /// <summary>
        /// GetCurrentInstanceWindowHandle
        /// </summary>
        /// <returns></returns>
        private static IntPtr GetCurrentInstanceWindowHandle()
        {
            var hWnd = IntPtr.Zero;
            var process = Process.GetCurrentProcess();
            var processes = Process.GetProcessesByName(process.ProcessName);
            foreach (var p in processes.Where(p => process.MainModule != null && (p.MainModule != null && (p.Id != process.Id && p.MainModule.FileName == process.MainModule.FileName && p.MainWindowHandle != IntPtr.Zero))))
            {
                hWnd = p.MainWindowHandle;
                break;
            }
            return hWnd;
        }
        /// <summary>
        /// SwitchToCurrentInstance
        /// </summary>
        public static void SwitchToCurrentInstance()
        {
            var hWnd = GetCurrentInstanceWindowHandle();
            if (hWnd != IntPtr.Zero)
            {
                // Restore window if minimised. Do not restore if already in
                // normal or maximised window state, since we don't want to
                // change the current state of the window.
                if (IsIconic(hWnd) != 0)
                {
                    ShowWindow(hWnd, SwRestore);
                }

                // Set foreground window.
                SetForegroundWindow(hWnd);
            }
        }

        /// <summary>
        /// for console base application
        /// </summary>
        /// <returns></returns>
        public static bool Run()
        {
            if (IsAlreadyRunning())
            {
                return false;
            }
            return true;
        }

        /// <summary>
        /// check if given exe alread running or not
        /// </summary>
        /// <returns>returns true if already running</returns>
        public static bool IsAlreadyRunning()
        {
            var strLoc = AppDomain.CurrentDomain.FriendlyName;
            FileSystemInfo fileInfo = new FileInfo(strLoc);
            var sExeName = fileInfo.Name;

            _mutex = new Mutex(true, "Nomad_" + sExeName, out var bCreatedNew);
            if (bCreatedNew)
                _mutex.ReleaseMutex();

            return !bCreatedNew;
        }
    }
}
