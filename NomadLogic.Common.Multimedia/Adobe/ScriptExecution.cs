using System;
using System.Diagnostics;
using NLog;

namespace NomadLogic.Multimedia.Adobe
{
    public class ScriptExecution
    {
        //private const String AE = "C:\\Program Files (x86)\\Adobe\\Adobe After Effects CS4\\Support Files\\AfterFX.exe";
        private static readonly Logger log = LogManager.GetCurrentClassLogger();

        public void ExecuteScript(string exectFile,  string script)
        {

            var startInfo = new ProcessStartInfo
            {
                CreateNoWindow = false,
                UseShellExecute = false,
                FileName = exectFile,
                Arguments = "-r " + script
            };
            //startInfo.WindowStyle = ProcessWindowStyle.Hidden;

            try
            {
                // Start the process with the info we specified.
                // Call WaitForExit and then the using statement will close.
                using (var exeProcess = Process.Start(startInfo))
                {
                    exeProcess?.WaitForExit();
                }
            }
            catch (Exception ex)
            {
                log.Error(ex);
            }

        }
    }
}
