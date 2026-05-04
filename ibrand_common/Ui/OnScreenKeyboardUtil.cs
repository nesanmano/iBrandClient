using System;
using System.Diagnostics;
using System.Runtime.InteropServices;

namespace NomadLogic.Common.Ui
{
    /// <summary>
    /// Méthode pour gérer le mieux possible le clavier "On Screen" de Windows 10.
    /// Idéalement, on lance la méthode d'affichage via l'événement "Onload" d'une forme, et on appelle la fermeture du clavier dans son
    /// événement de fermeture "OnClosing".
    /// C'est un sujet épineux et il y a plusieurs avis sur cet outil dans la communauté.
    /// Voir ces liens:
    /// https://stackoverflow.com/questions/16601424/after-killing-the-process-for-tabletkeyboardtabtip-exe-application-doesnt-bri
    /// </summary>
    public static class OnScreenKeyboardUtil
    {
        private static Process _myProcess;

        [return: MarshalAs(UnmanagedType.Bool)]
        [DllImport("user32.dll", SetLastError = true)]
        public static extern bool PostMessage(int hWnd, uint Msg, int wParam, int lParam);

        [DllImport("user32.dll")]
        public static extern IntPtr FindWindow(String sClassName, String sAppName);

        /// <summary>
        /// Tente de lancer l'exécutable pour afficher le clavier.
        /// </summary>
        public static void OpenKeyboard()
        {
            var startInfo =
                new ProcessStartInfo(@"C:\Program Files\Common Files\Microsoft Shared\ink\TabTip.exe")
                {
                    WindowStyle = ProcessWindowStyle.Hidden
                };
            _myProcess = Process.Start(startInfo);
        }

        /// <summary>
        /// Ferme l'exécutable du clavier, de façon douce.
        /// </summary>
        public static void CloseKeyboard()
        {
            const uint WM_SYSCOMMAND = 274;
            const uint SC_CLOSE = 61536;
            var keyboardWnd = GetKeyboardWindowHandle();
            PostMessage(keyboardWnd.ToInt32(), WM_SYSCOMMAND, (int)SC_CLOSE, 0);
            _myProcess = null;
        }

        /// <summary>
        /// Gets the window handler for the virtual keyboard.
        /// </summary>
        /// <returns>The handle.</returns>
        public static IntPtr GetKeyboardWindowHandle()
        {
            return FindWindow("IPTip_Main_Window", null);
        }
    }
}