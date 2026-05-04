using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using JetBrains.Annotations;
using NLog;

namespace NomadLogic.Common.Util
{
    [PublicAPI]
    public static class LaunchProcessUtil
    {
        private static readonly Logger log = LogManager.GetCurrentClassLogger();

        /*
        /// <summary>
        /// Code "boiler-plate" pour lancer un exécutable.
        /// On peut utiliser cette méthode pour demander une commande personnalisée, qui n'est pas créée à partir des multiples méthodes
        /// de cette classe.
        /// Cette méthode peut être est bloquante: Si lancé de façon bloquante (par défaut), le contrôle d'exécution est suspendu tant que
        /// le processus appellé n'est pas tué. Dans certaines circonstances, cela peut être dangeureux.
        /// </summary>
        /// <param name="execLocation">La location absolue (ou relative si chemin incomplet) de l'exécutable voulu.</param>
        /// <param name="args">La chaîne d'argument à passer à l'exécutable.</param>
        /// <param name="outputFile">Si on veut rediriger le output de l'exécution vers un fichier.</param>
        /// <param name="waitForExit">Vrai par défaut. On attend que le processus lancé soit terminé.</param>
        /// <param name="token">Jeton de cancellation optionnel.</param>
        public static async Task LaunchProcessAsync([NotNull] string execLocation, [NotNull] string args,
            FileInfo outputFile = null, bool waitForExit = true, CancellationToken? token = null)
        {
            if (token.HasValue)
            {
                await Task.Run(() =>
                {
                    LaunchProcess(execLocation, args, outputFile);
                }, token.Value);
            }
            else
            {
                await Task.Run(() =>
                {
                    LaunchProcess(execLocation, args, outputFile);
                });
            }
        }
        */

        /// <summary>
        /// OBSOLÈTE: Utilisez la version asynchrone pour éviter un "deadlock" avec des processus qui ne stoppent pas par eux-même.
        /// Code "boiler-plate" pour lancer un exécutable.
        /// On peut utiliser cette méthode pour demander une commande personnalisée, qui n'est pas créée à partir des multiples méthodes
        /// de cette classe.
        /// Cette méthode peut être est bloquante: Si lancé de façon bloquante (par défaut), le contrôle d'exécution est suspendu tant que
        /// le processus appellé n'est pas tué. Dans certaines circonstances, cela peut être dangeureux.
        /// Voir aussi: https://stackoverflow.com/questions/1145969/processinfo-and-redirectstandardoutput
        /// </summary>
        /// <param name="execLocation">La location absolue (ou relative si chemin incomplet) de l'exécutable voulu.</param>
        /// <param name="args">La chaîne d'argument à passer à l'exécutable.</param>
        /// <param name="outputFileStdOut">Si on veut rediriger le output STANDARD de l'exécution vers un fichier.</param>
        /// <param name="outputFileStdErr">Si on veut rediriger le output ERROR de l'exécution vers un fichier.</param>
        /// <param name="waitForExit">Vrai par défaut. On attend que le processus lancé soit terminé. Si à faux, on ne peut pas rediriger les outputs
        /// du processus, car la gestion de ceux-ci sont lié au contexte de cette méthode.</param>
        [Obsolete("Il faudrait utiliser une version asynchrone pour pouvoir stopper un processus bloqué après X secondes.")]
        public static void LaunchProcess([NotNull] string execLocation, [NotNull] string args, 
            FileInfo outputFileStdOut = null, FileInfo outputFileStdErr = null, bool waitForExit = true)
        {
            if (waitForExit)
            {
                var t = LaunchProcessAsync(CancellationToken.None, execLocation, args, outputFileStdOut, outputFileStdErr);
                t.GetAwaiter().GetResult();
            }
            else
            {
                var t = LaunchProcessAsync(CancellationToken.None, execLocation, args);
                t.GetAwaiter().GetResult();
            }
        }

        /// <summary>
        /// Code "boiler-plate" pour lancer un exécutable.
        /// On peut utiliser cette méthode pour demander une commande personnalisée, qui n'est pas créée à partir des multiples méthodes
        /// de cette classe.
        /// Cette méthode peut être est bloquante: Si lancé de façon bloquante (par défaut), le contrôle d'exécution est suspendu tant que
        /// le processus appellé n'est pas tué. Dans certaines circonstances, cela peut être dangeureux.
        /// Voir aussi: https://stackoverflow.com/questions/1145969/processinfo-and-redirectstandardoutput
        /// </summary>
        /// <param name="maxWait">Période temporelle maximale, au dela de laquelle on stopppe l'exécution.</param>
        /// <param name="execLocation">La location absolue (ou relative si chemin incomplet) de l'exécutable voulu.</param>
        /// <param name="args">La chaîne d'argument à passer à l'exécutable.</param>
        /// <param name="outputFileStdOut">Si on veut rediriger le output STANDARD de l'exécution vers un fichier.</param>
        /// <param name="outputFileStdErr">Si on veut rediriger le output ERROR de l'exécution vers un fichier.</param>
        /// <param name="waitForExit">Vrai par défaut. On attend que le processus lancé soit terminé. Si à faux, on ne peut pas rediriger les outputs
        /// du processus, car la gestion de ceux-ci sont lié au contexte de cette méthode.</param>
        public static async Task LaunchProcessAsync(TimeSpan maxWait, [NotNull] string execLocation, [NotNull] string args,
            FileInfo outputFileStdOut = null, FileInfo outputFileStdErr = null, bool waitForExit = true)
        {
            var cts = new CancellationTokenSource(maxWait);
            if (waitForExit)
            {
                await LaunchProcessAsync(cts.Token, execLocation, args, outputFileStdOut, outputFileStdErr);
            }
            else
            {
                await LaunchProcessAsync(cts.Token, execLocation, args, false);
            }
        }

        /// <summary>
        /// Code "boiler-plate" pour lancer un exécutable.
        /// On peut utiliser cette méthode pour demander une commande personnalisée, qui n'est pas créée à partir des multiples méthodes
        /// de cette classe.
        /// On ne peut analyser les extrants de STDOUT et STDERR avec cette méthode.
        /// </summary>
        /// <param name="token">La jeton d'Arrêt.</param>
        /// <param name="execLocation">La location absolue (ou relative si chemin incomplet) de l'exécutable voulu.</param>
        /// <param name="args">La chaîne d'argument à passer à l'exécutable.</param>
        /// <param name="waitForExit">Donne l'option de ne pas attendre la fin du processus. Si cette option est à vrai</param>
        public static Task LaunchProcessAsync(CancellationToken token, [NotNull] string execLocation, [NotNull] string args, bool waitForExit = true)
        {
            var t = Task.Run(async () => {
                try
                {
                    log.Debug("Command to be executed: \n\"" + execLocation + "\" " + args);
                    
                    using (var exeProcess = new Process())
                    {
                        var startInfo = new ProcessStartInfo
                        {
                            CreateNoWindow = true,
                            UseShellExecute = false,
                            FileName = execLocation,
                            WindowStyle = ProcessWindowStyle.Hidden,
                            Arguments = args
                        };
                        
                        exeProcess.StartInfo = startInfo;

                        // On attend de façon simili-synchrone la fin de l'exécution du processus.
                        exeProcess.Start();
                        
                        while (!exeProcess.HasExited)
                        {
                            if (token.IsCancellationRequested)
                            {
                                exeProcess.Kill();
                                exeProcess.WaitForExit();
                                return;
                            }

                            await Task.Delay(20, token);
                        }
                    }
                }
                catch (Exception ex)
                {
                    log.Error(ex);
                }
            }, token);

            if (!waitForExit) return t;
            t.Wait(token);
            return Task.CompletedTask;

        }

        /// <summary>
        /// Code "boiler-plate" pour lancer un exécutable.
        /// On peut utiliser cette méthode pour demander une commande personnalisée, qui n'est pas créée à partir des multiples méthodes
        /// de cette classe.
        /// Cette version donne une façon d'intercepter les extrant STDERR et STDOUT. Dû à la façon d'intercepter les informations,
        /// cette méthode est "bloquante" par défaut.
        /// </summary>
        /// <param name="token">La jeton d'Arrêt.</param>
        /// <param name="execLocation">La location absolue (ou relative si chemin incomplet) de l'exécutable voulu.</param>
        /// <param name="args">La chaîne d'argument à passer à l'exécutable.</param>
        /// <param name="outputFileStdOut">Si on veut rediriger le output STANDARD de l'exécution vers un fichier.</param>
        /// <param name="outputFileStdErr">Si on veut rediriger le output ERROR de l'exécution vers un fichier.</param>
        public static async Task LaunchProcessAsync(CancellationToken token, [NotNull] string execLocation, [NotNull] string args, 
            FileInfo outputFileStdOut, FileInfo outputFileStdErr)
        {
            StreamWriter outFile = null;
            StreamWriter errFile = null;
            FileInfo tmpOut = null;
            FileInfo tmpErr = null;

            if (!File.Exists(execLocation))
            {
                log.Error("The executable file does not exists:\n\t" + execLocation);
                return;
            }

            try
            {
                log.Debug("Command to be executed: \n\"" + execLocation + "\" " + args);

                // Start the process with the info we specified.
                // Call WaitForExit and then the using statement will close.
                using (var exeProcess = new Process())
                {
                    var startInfo = new ProcessStartInfo
                    {
                        CreateNoWindow = true,
                        UseShellExecute = false,
                        FileName = execLocation,
                        WindowStyle = ProcessWindowStyle.Hidden,
                        Arguments = args
                    };

                    if (outputFileStdOut != null)
                    {
                        log.Trace("Setup for OUT redirection to file: {0}", outputFileStdOut.FullName);
                        startInfo.RedirectStandardOutput = true;
                        tmpOut = new FileInfo(DirectoryUtil.CreateLocalTemporaryFile());
                        outFile = new StreamWriter(tmpOut.FullName, false);
                        exeProcess.OutputDataReceived += (sender, eventArgs) =>
                        {
                            try
                            {
                                outFile.WriteLine(eventArgs.Data);
                            }
                            catch (Exception)
                            {
                                // ignored
                            }
                        };
                        exeProcess.EnableRaisingEvents = true;
                    }

                    if (outputFileStdErr != null)
                    {
                        log.Trace("Setup for ERR redirection to file: {0}", outputFileStdErr.FullName);
                        startInfo.RedirectStandardError = true;
                        tmpErr = new FileInfo(DirectoryUtil.CreateLocalTemporaryFile());
                        errFile = new StreamWriter(tmpErr.FullName, false);
                        exeProcess.ErrorDataReceived += (sender, eventArgs) =>
                        {
                            try
                            {
                                errFile.WriteLine(eventArgs.Data);
                            }
                            catch (Exception)
                            {
                                // ignored
                            }
                        };
                        exeProcess.EnableRaisingEvents = true;
                    }
                    
                    exeProcess.StartInfo = startInfo;

                    // On attend de façon simili-synchrone la fin de l'exécution du processus.
                    exeProcess.Start();

                    if (startInfo.RedirectStandardOutput) exeProcess.BeginOutputReadLine();
                    if (startInfo.RedirectStandardError) exeProcess.BeginErrorReadLine();

                    while (!exeProcess.HasExited)
                    {
                        if (token.IsCancellationRequested)
                        {
                            exeProcess.Kill();
                            exeProcess.WaitForExit();
                            return;
                        }

                        await Task.Delay(20, token);
                    }
                }
            }
            catch (Exception ex)
            {
                log.Error(ex);
            }
            finally
            {
                try
                {
                    if (tmpOut != null && outFile != null)
                    {
                        outFile.Close();
                        tmpOut.Refresh();
                        File.Move(tmpOut.FullName, outputFileStdOut.FullName);
                    }
                } catch (Exception) { /* */ }

                try
                {
                    if (tmpErr != null && errFile != null)
                    {
                        errFile.Close();
                        tmpErr.Refresh();
                        File.Move(tmpErr.FullName, outputFileStdErr.FullName);
                    }
                } catch (Exception) { /* */ }
            }
        }


        /// <summary>
        /// Invoke une commande en ligne avec le but de transformer le résultat texte de la requête dans un objet (via une méthode de conversion).
        /// On peut donner également l'option de sauvegarde l'ensemble du "out" dans un fichier texte à part.
        /// </summary>
        /// <param name="token">Le jeton de cancellation.</param>
        /// <param name="execLocation">L'exécutable à </param>
        /// <param name="commandLine">La ligne de commande à utiliser.</param>
        /// <param name="textOutputOptions">Les paramètres de conversion texte des extrant de la commande.</param>
        /// <returns>TRUE si toutes les coversions des extrant de la requêtes sont effectués sans erreurs.</returns>
        public static async Task<bool> InvokeCliforTextResult(CancellationToken token, [NotNull] string execLocation, [NotNull] string commandLine, 
            [NotNull] IDictionary<ProcessOutputEnum, ProcessOutputConfig> textOutputOptions)
        {
            try
            {
                FileInfo tmpOut = null;
                FileInfo tmpErr = null;

                if (textOutputOptions.TryGetValue(ProcessOutputEnum.StandardOut, out var pocOut))
                {
                    tmpOut = new FileInfo(DirectoryUtil.CreateLocalTemporaryFile());
                    pocOut.TempFile = tmpOut;
                }

                if (textOutputOptions.TryGetValue(ProcessOutputEnum.StandardError, out var pocErr))
                {
                    tmpErr = new FileInfo(DirectoryUtil.CreateLocalTemporaryFile());
                    pocErr.TempFile = tmpErr;
                }

                await LaunchProcessAsync(token, execLocation, commandLine, tmpOut, tmpErr);
                
                var allGood = true;

                if (pocOut != null)
                {
                    var obj = pocOut.ConversionMethod.Invoke(pocOut.TempFile);
                    if (pocOut.ConversionType.IsInstanceOfType(obj))
                    {
                        pocOut.ConvertedInstance = obj;
                    }
                    else
                    {
                        log.Warn($"Conversion expected type was not the type returned: was '{obj.GetType().FullName}', and expecting '{pocOut.ConversionType.FullName}'");
                        allGood = false;
                    }
                }

                if (pocErr != null)
                {
                    var obj = pocErr.ConversionMethod.Invoke(pocErr.TempFile);
                    if (pocErr.ConversionType.IsInstanceOfType(obj))
                    {
                        pocErr.ConvertedInstance = obj;
                    }
                    else
                    {
                        log.Warn($"Conversion expected type was not the type returned: was '{obj.GetType().FullName}', and expecting '{pocErr.ConversionType.FullName}'");
                        allGood = false;
                    }
                }

                return allGood;

            }
            catch (Exception e)
            {
                log.Error(e);
                return default;
            }
            finally
            {
                foreach (var tmp in textOutputOptions.Values)
                {
                    try
                    {
                        if (tmp.TempFile != null && tmp.OutputFile != null)
                        {
                            File.Move(tmp.TempFile.FullName, tmp.OutputFile.FullName);
                        }
                        else
                        {
                            tmp.TempFile?.Delete();
                        }
                    }
                    catch (Exception e)
                    {
                        log.Error(e, "Could not write result to out file, or destroy temp result file.");
                    }
                }
            }
        }
    }

    public class ProcessOutputConfig
    {
        public Type ConversionType { get; set; }

        public Func<FileInfo, object> ConversionMethod { get; set; }

        public FileInfo OutputFile { get; set; }

        public object ConvertedInstance { get; set; }
        
        internal FileInfo TempFile { get; set; }
    }

    public enum ProcessOutputEnum
    {
        StandardOut,
        StandardError
    }
}