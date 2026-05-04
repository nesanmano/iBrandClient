using System;
using System.Windows.Forms;
using NLog;
using NomadLogic.Common.Services;
using NomadLogic.Common.Services.Configuration;
using NomadLogic.Common.Util;

namespace NomadLogic.iBrand.CameraRingClient
{
    internal static class Program
    {
        private static readonly Logger log = LogManager.GetCurrentClassLogger();

        /// <summary>
        /// The main entry point for the application.
        /// </summary>
        [MTAThread] //Obligatoire quand on utilise la librairie Canon FSF
        private static void Main()
        {
            try
            {
                var config = new NomadConfiguration {ApplicationName = DirectoryUtil.ApplicationNameEnum.CameraRingClient};
                ServicesContainer.AddService(config);
                
                LogUtil.InitNLog();

                Application.EnableVisualStyles();
                Application.SetCompatibleTextRenderingDefault(false);
                Application.Run(new CameraRingClientForm());

                ServicesContainer.Dispose();
            }
            catch (Exception ex)
            {
                log.Fatal(ex);
                throw;
            }
        }
    }
}
