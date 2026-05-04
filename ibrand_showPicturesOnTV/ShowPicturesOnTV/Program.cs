using System;
using System.Collections.Generic;
using System.Windows.Forms;
using NomadLogic.Common.Services;
using NomadLogic.Common.Services.Configuration;
using NomadLogic.Common.Services.LocalCapabilities;
using NomadLogic.Common.Util;

namespace NomadLogic.ShowPicturesOnTV
{
    static class Program
    {
        /// <summary>
        /// The main entry point for the application.
        /// </summary>
        [STAThread]
        static void Main()
        {
            if (SingleApplication.IsAlreadyRunning())
            {
                SingleApplication.SwitchToCurrentInstance();
                return;
            }
            
            var config = new NomadConfiguration { ApplicationName = DirectoryUtil.ApplicationNameEnum.ShowPictureOnTv };
            ServicesContainer.AddService(config);
            LogUtil.InitNLog();

            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);
            
            var verifiers = new List<ILocalCapabilityVerifier>
            {
                new LocalCapabilityVerifierVlc()
            };

            var id = new DisplayForm
            {
                CapabilityVerifiers = verifiers
            };
            
            Application.Run(id);
            
        }
    }
}
