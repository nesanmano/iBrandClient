using System;
using System.Diagnostics;
using System.IO;
using NLog;
using NomadLogic.Common.Services;
using NomadLogic.Common.Services.Configuration;

namespace NomadLogic.Multimedia.Cli.Ffmpeg
{
    public class FfmpegThumbnailsGenerator
    {
        private static readonly Logger log = LogManager.GetCurrentClassLogger();

        /// <summary>
        /// Endroit personnalisé de location de FFMPEG. Peut être nul. Si c'est le cas, on utilise la location dans le service de AbstractConfiguration.
        /// </summary>
        public string CustomFFmpegLocation
        {
            get;
            set;
        }

        /// <summary>
        /// Lance FFMPEG pour générer un "thumbnail" d'un vidéo à une seconde précise. Si le fichier existe déjà, on le remplace.
        /// <br/>La taille du fichier sera la même que la taille d'une trame du vidéo source.
        /// </summary>
        /// <param name="videoFileName">Le nom du vidéo à utiliser.</param>
        /// <param name="outputFileName">Le nom du fichier de thumbnail résultant.</param>
        /// <param name="secs">La seconde du vidéo à saisir comme image.</param>
        public void GenerateThumbnail(string videoFileName, string outputFileName, int secs)
        {
            if (File.Exists(outputFileName))
            {
                File.Delete(outputFileName);
            }

            var Params = string.Format("-i \"{0}\" -vcodec mjpeg -ss {2} -vframes 1 -an -f rawvideo \"{1}\"", videoFileName, outputFileName, secs);
            log.Trace(Params);

            RunProcess(Params);
        }

        /// <summary>
        /// Lance FFMPEG pour générer un "thumbnail" d'un vidéo à une seconde précise. Si le fichier existe déjà, on le remplace.
        /// <br/>Il faut doner la taille voulue du fichier résultant.
        /// </summary>
        /// <param name="videoFileName">Le nom du vidéo à utiliser.</param>
        /// <param name="outputFileName">Le nom du fichier de thumbnail résultant.</param>
        /// <param name="secs">La seconde du vidéo à saisir comme image.</param>
        /// <param name="height">Hauteur, en pixels, du fichier résultant.</param>
        /// <param name="width">Largeur, en pixel, du fichier résultant.</param>
        public void GenerateThumbnail(string videoFileName, string outputFileName, int secs, int width, int height)
        {
            var Params = string.Format("-i \"{0}\" -vcodec mjpeg -ss {2} -vframes 1 -an -f rawvideo -s {3}x{4} \"{1}\"", videoFileName, outputFileName, secs, width, height);
            //log.Debug(Params);

            RunProcess(Params);
        }

        private void RunProcess(string parameters)
        {
            //create a process info
            ProcessStartInfo oInfo;

            if (CustomFFmpegLocation == null)
            {
                var configuration = ServicesContainer.GetService<NomadConfiguration>();
                oInfo = new ProcessStartInfo(configuration.FfmpegLocation, parameters);
            }
            else
            {
                oInfo = new ProcessStartInfo(CustomFFmpegLocation, parameters);
            }
            oInfo.UseShellExecute = false;
            oInfo.CreateNoWindow = true;
            oInfo.RedirectStandardOutput = true;
            oInfo.RedirectStandardError = true;

            //Create the output and streamreader to get the output
            StreamReader srOutput = null;

            //try the process
            try
            {
                //run the process
                var proc = Process.Start(oInfo);

                if (proc == null) return;
                proc.WaitForExit();

                //get the output
                srOutput = proc.StandardError;

                //now put it in a string
                srOutput.ReadToEnd();

                proc.Close();
            }
            catch (Exception)
            {
                // ignored
            }
            finally
            {
                //now, if we succeded, close out the streamreader
                if (srOutput != null)
                {
                    srOutput.Close();
                    srOutput.Dispose();
                }
            }
        }
    }
}
