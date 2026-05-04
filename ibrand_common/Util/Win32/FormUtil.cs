using System;
using System.Drawing;
using System.Runtime.InteropServices;
using System.Windows.Forms;
using JetBrains.Annotations;
using NLog;

namespace NomadLogic.Common.Util.Win32
{
    [PublicAPI]
    public static class FormUtil
    {
        /* Cette section est là pour contrôler avec finisse l'écran en mode "FullSCreen" sans avoir à gérer
         * la barre de tâches.
         * https://www.vesic.org/english/blog-eng/net/full-screen-maximize/
         */

        [DllImport("user32.dll", EntryPoint = "GetSystemMetrics")]
        public static extern int GetSystemMetrics(int which);

        [DllImport("user32.dll")]
        public static extern void SetWindowPos(IntPtr hwnd, IntPtr hwndInsertAfter, int x, int y, int width, int height, uint flags);

        [DllImport("user32.dll")]
        public static extern void ShowWindow(IntPtr hwnd, int nCmdShow);

        [DllImport("user32.dll")]
        private static extern bool RegisterHotKey(IntPtr hWnd, int id, int fsModifiers, int vk);

        [DllImport("user32.dll")]
        private static extern bool UnregisterHotKey(IntPtr hWnd, int id);
        
        public static int RegisterGlobalHotKey([NotNull] Control f, Keys key, WinUserConstants.RH_PARAMS modifierParams)
        {
            var intKey = (int)key;
            var hwnd = f.Handle;
            var id = (int)modifierParams ^ intKey ^ hwnd.ToInt32();

            return RegisterHotKey(hwnd, id, (int)modifierParams, intKey) ? id : 0;
        }

        public static bool UnregisterGlobalHotKey([NotNull] Control f, int id)
        {
            return UnregisterHotKey(f.Handle, id);
        }

        public static bool GetKeysAndModifiersFromHotkeyMessage(Message m, out Keys key, out WinUserConstants.RH_PARAMS modifiers)
        {
            if ( m.Msg != WinUserConstants.WM_HOTKEY_MSG_ID)
            {
                key = Keys.None;
                modifiers = WinUserConstants.RH_PARAMS.NOMOD;
                return false;
            }

            // get the keys.
            key = (Keys)(((int)m.LParam >> 16) & 0xFFFF);
            modifiers = (WinUserConstants.RH_PARAMS)((int)m.LParam & 0xFFFF);

            return true;
        }

        public static void SetWinHidden(IntPtr hwnd, bool hide = true)
        {
            ShowWindow(hwnd, hide ? (int)WinUserConstants.SW_PARAMS.SW_HIDE : (int)WinUserConstants.SW_PARAMS.SW_SHOWMAXIMIZED);
        }

        public static int ScreenX => GetSystemMetrics((int)WinUserConstants.SM_INDEX.SM_CXSCREEN);

        public static int ScreenY => GetSystemMetrics((int)WinUserConstants.SM_INDEX.SM_CYSCREEN);

        /// <summary>
        /// Obsolete: utiliser la version complète "SetWindowsPos"
        /// </summary>
        /// <param name="hwnd">Le Handle de la forme à maximiser.</param>
        [Obsolete]
        public static void SetWinFullScreen(IntPtr hwnd)
        {
            SetWindowPos(hwnd, 
                new IntPtr((int)WinUserConstants.HWND_CONSTANTS.HWND_TOP),
                0, 0, ScreenX, ScreenY, 
                (int)WinUserConstants.SWP_PARAMS_UFLAGS.SWP_SHOWWINDOW);
        }

        /// <summary>
        /// Appel (quasi)complet vers la méthode de positionement de fenêtres. Utilise des constantes pour le positionnement.
        /// Si on veut mettre une fenêtre comme parent, utilisez la version avec des pointeurs Handle.
        /// </summary>
        /// <param name="form">La forme à modifier.</param>
        /// <param name="hWndInsertAfter">Le mode d'affichage de la fenêtre.</param>
        /// <param name="x">Position en X</param>
        /// <param name="y">Position en Y</param>
        /// <param name="cx">Largeur de la fenêtre</param>
        /// <param name="cy">Hauteur de la fenêtre</param>
        /// <param name="uFlags">Paramètres d'affichage</param>
        public static void SetWindowPos([NotNull] Control form, WinUserConstants.HWND_CONSTANTS hWndInsertAfter, int x, int y, int cx, int cy, WinUserConstants.SWP_PARAMS_UFLAGS uFlags)
        {
            SetWindowPos(form.Handle, new IntPtr((int)hWndInsertAfter), x, y, cx, cy, (uint)uFlags);
        }

        /// <summary>
        /// Overload pour appeller SetWindowPos avec la fonctionalité des flags C#.
        /// </summary>
        /// <param name="hwnd">Le "handle" de la forme a modifier.</param>
        /// <param name="hWndInsertAfter">Le "handle" de la forme parent.</param>
        /// <param name="x">Position en X</param>
        /// <param name="y">Position en Y</param>
        /// <param name="cx">Largeur de la fenêtre</param>
        /// <param name="cy">Hauteur de la fenêtre</param>
        /// <param name="uFlags">Paramètres d'affichage</param>
        public static void SetWindowPos(IntPtr hwnd, IntPtr hWndInsertAfter, int x, int y, int cx, int cy, WinUserConstants.SWP_PARAMS_UFLAGS uFlags)
        {
            SetWindowPos(hwnd, hWndInsertAfter, x, y, cx, cy, (uint)uFlags);
        }

        private static readonly Logger log = LogManager.GetCurrentClassLogger();

        /// <summary>
        /// Comme normalement on n'a qu'une seule instance d'une classe Form par exécution, cette
        /// méthode cherche une instance active de cette Form et l'affiche. S'il n'y en n'existe pas,
        /// on la créer et on l'affiche.
        /// </summary>
        /// <param name="formType"></param>
        /// <param name="oldForm"></param>
        public static void ShowForm(Type formType, Form oldForm)
        {
            if (!formType.IsAssignableFrom(typeof(Form)))
            {
                return;
            }

            oldForm.Hide();

            foreach (Form frm in Application.OpenForms)
            {
                if (frm.GetType() == formType)
                {
                    frm.Show();
                    return;
                }
            }

            var f2 = (Form)Activator.CreateInstance(formType);
            f2.Show();
        }

        /// <summary>
        /// Vérifie si une coordonée se situe dans un rectangle.
        /// </summary>
        /// <param name="x">La coordonnée en X.</param>
        /// <param name="y">La coordonnée en Y.</param>
        /// <param name="area">La zone à tester.</param>
        /// <returns>TRUE si et seulement si les coordonnées sont inclusivement à l'intérieur du rectangle.</returns>
        public static bool InArea(int x, int y, Rectangle area)
        {
            return x >= area.Location.X && x < area.Location.X + area.Width && y < area.Location.Y + area.Height && y >= area.Location.Y;
        }

        /// <summary>
        /// Vérifie si une coordonée se situe dans un rectangle.
        /// </summary>
        /// <param name="x">La coordonnée en X.</param>
        /// <param name="y">La coordonnée en Y.</param>
        /// <param name="area">La zone à tester.</param>
        /// <returns>TRUE si et seulement si les coordonnées sont inclusivement à l'intérieur du rectangle.</returns>
        public static bool InArea(this Rectangle area, int x, int y)
        {
            return x >= area.Location.X && x < area.Location.X + area.Width && y < area.Location.Y + area.Height && y >= area.Location.Y;
        }

        /// <summary>
        /// Utilisé pour appeller une méthode sur le fil d'exécution UI.
        /// Ne fait rien si les deux premiers paramètres ne sont pas une référence valide.
        /// La méthode à invoquer en cas de problèmes est optionnelle.
        /// </summary>
        /// <param name="invoker">Le contrôle invocateur.</param>
        /// <param name="action">L'action à exécuter.</param>
        /// <param name="exceptionHandler">L'action à exécuter en cas de problème.</param>
        public static void SafeCall([NotNull] this Control invoker,[NotNull] Action action, Action<Exception> exceptionHandler = null)
        {
            if (invoker?.IsDisposed ?? true) return;

            if (!invoker.InvokeRequired)
            {
                try
                {
                    action.Invoke();
                }
                catch (Exception ex)
                {
                    if (exceptionHandler == null)
                    {
                        log.Error(ex);
                        return;
                    }

                    exceptionHandler.Invoke(ex);
                }
                return;
            }

            invoker.Invoke(new Action(() =>
            {
                try
                {
                    action.Invoke();
                }
                catch (Exception ex)
                {
                    if (exceptionHandler == null)
                    {
                        log.Error(ex);
                        return;
                    }

                    exceptionHandler.Invoke(ex);
                }
            }));
        }
        
    }
}
