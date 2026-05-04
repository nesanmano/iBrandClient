using System;
using System.Windows.Forms;
using iBrand_SnapShot.UI;

namespace iBrand_SnapShot
{
    static class Program
    {
        /// <summary>
        /// The main entry point for the application.
        /// </summary>
        [MTAThread]
        private static void Main()
        {
            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);
            Application.Run(new MainFormUi());
        }
    }
}
