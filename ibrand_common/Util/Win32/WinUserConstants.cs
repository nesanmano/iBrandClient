using System;
using JetBrains.Annotations;
// ReSharper disable InconsistentNaming

namespace NomadLogic.Common.Util.Win32
{
    /// <summary>
    /// Cette classe de constantes contient l'ensemble des valeurs entière pour l'utilisation des fonctions de la classe WinUser.
    /// Elle représente la librarie "user32.dll" de la librarie pour Windows de base.
    /// Lorsque utile, les valeurs entières sont aussi représentée sous forme de classe énumérative "flag" (et qui peuvent donc être combinées) ou sous forme d'énumération
    /// normale.
    /// </summary>
    [PublicAPI]
    public static class WinUserConstants
    {
        /* Constantes pour les messages système WndProc */
        /// windows message id for hotkey
        public const int WM_HOTKEY_MSG_ID = 0x0312;

        #region Inner enums

        /// <summary>
        /// Valeurs de HWND constantes.
        /// </summary>
        [PublicAPI]
        public enum HWND_CONSTANTS
        {
            HWND_BROADCAST = 0xffff,
            HWND_TOP = 0,
            HWND_BOTTOM = 1,
            HWND_TOPMOST = -1,
            HWND_NOTOPMOST = -2,
            HWND_MESSAGE = -3
        }

        /// <summary>
        /// SetWindowPos function
        /// Valeurs de positionnements de fenêtres.
        /// https://docs.microsoft.com/en-us/windows/win32/api/winuser/nf-winuser-setwindowpos
        /// </summary>
        [Flags]
        [PublicAPI]
        public enum SWP_PARAMS_UFLAGS
        {
            /// If the calling thread and the thread that owns the window are attached to different input queues, the system posts the request to the thread that owns the window. This prevents the calling thread from blocking its execution while other threads process the request.
            SWP_ASYNCWINDOWPOS = 0x4000,
            /// Prevents generation of the WM_SYNCPAINT message.
            SWP_DEFERERASE = 0x2000,
            /// Draws a frame (defined in the window's class description) around the window.
            SWP_DRAWFRAME = 0x0020,
            /// Applies new frame styles set using the SetWindowLong function. Sends a WM_NCCALCSIZE message to the window, even if the window's size is not being changed. If this flag is not specified, WM_NCCALCSIZE is sent only when the window's size is being changed.
            SWP_FRAMECHANGED = 0x0020,
            /// Hides the window.
            SWP_HIDEWINDOW = 0x0080,
            /// Does not activate the window. If this flag is not set, the window is activated and moved to the top of either the topmost or non-topmost group (depending on the setting of the hWndInsertAfter parameter).
            SWP_NOACTIVATE = 0x0010,
            /// Discards the entire contents of the client area. If this flag is not specified, the valid contents of the client area are saved and copied back into the client area after the window is sized or repositioned.
            SWP_NOCOPYBITS = 0x0100,
            /// Retains the current position (ignores X and Y parameters).
            SWP_NOMOVE = 0x0002,
            /// Does not change the owner window's position in the Z order.
            SWP_NOOWNERZORDER = 0x0200,
            /// Does not redraw changes. If this flag is set, no repainting of any kind occurs. This applies to the client area, the nonclient area (including the title bar and scroll bars), and any part of the parent window uncovered as a result of the window being moved. When this flag is set, the application must explicitly invalidate or redraw any parts of the window and parent window that need redrawing.
            SWP_NOREDRAW = 0x0008,
            /// Same as the SWP_NOOWNERZORDER flag.
            SWP_NOREPOSITION = 0x0200,
            /// Prevents the window from receiving the WM_WINDOWPOSCHANGING message.
            SWP_NOSENDCHANGING = 0x0400,
            /*Retains the current size (ignores the cx and cy parameters). */
            SWP_NOSIZE = 0x0001,
            /// Ignore le paramètre de zOrder
            SWP_NOZORDER = 0x0004,
            /// Affiche un fenêtre 
            SWP_SHOWWINDOW = 0x0040
        }

        /// <summary>
        /// Paramètres de Global Hot Key (Observateur de clavier Windows)
        /// RegisterHotKey
        /// https://www.dreamincode.net/forums/topic/180436-global-hotkeys/
        /// </summary>
        [Flags]
        [PublicAPI]
        public enum RH_PARAMS : uint
        {
            NOMOD = 0x0000,
            ALT = 0x0001,
            CTRL = 0x0002,
            SHIFT = 0x0004,
            WIN = 0x0008
        }

        /// <summary>
        /// Paramètres de ShowWindow
        /// https://docs.microsoft.com/en-us/windows/win32/api/winuser/nf-winuser-showwindow
        /// </summary>
        [PublicAPI]
        public enum SW_PARAMS
        {
            SW_FORCEMINIMIZE = 11,
            SW_HIDE = 0,
            SW_MAXIMIZE = 3,
            SW_MINIMIZE = 6,
            SW_RESTORE = 9,
            SW_SHOW = 5,
            SW_SHOWDEFAULT = 10,
            SW_SHOWMAXIMIZED = 3,
            SW_SHOWMINIMIZED = 2,
            SW_SHOWMINNOACTIVE = 7,
            SW_SHOWNA = 8,
            SW_SHOWNOACTIVATE = 4,
            SW_SHOWNORMAL = 1
        }

        /// <summary>
        /// Paramètres de GetSystemMetrics
        /// https://docs.microsoft.com/en-us/windows/win32/api/winuser/nf-winuser-getsystemmetrics
        /// </summary>
        public enum SM_INDEX
        {
            /// <summary>
            /// The width of the screen of the primary display monitor, in pixels. This is the same value obtained by calling GetDeviceCaps as follows: GetDeviceCaps( hdcPrimaryMonitor, HORZRES). 
            /// </summary>
            SM_CXSCREEN = 0,
            /// <summary>
            /// The height of the screen of the primary display monitor, in pixels. This is the same value obtained by calling GetDeviceCaps as follows: GetDeviceCaps( hdcPrimaryMonitor, VERTRES). 
            /// </summary>
            SM_CYSCREEN = 1
        }

        #endregion
    }
}