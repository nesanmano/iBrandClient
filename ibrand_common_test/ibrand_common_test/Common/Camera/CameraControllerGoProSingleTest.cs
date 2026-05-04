using System;
using System.Diagnostics;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using GoPro.Hero;
using NLog;
using NomadLogic.Common.Util;
using NomadLogic.Common.Util.Camera;
using NomadLogic.Common.Util.Camera.GoProCam;
using NomadLogic.Common.Util.Json;
using NUnit.Framework;
using Assert = NUnit.Framework.Assert;
using FileNotFoundException = System.IO.FileNotFoundException;

namespace NomadLogic.Test.Common.Camera
{
    /// <summary>
    /// Classe de tests pour le contrôleur de caméra GoPro, version caméra unique.
    /// </summary>
    [Category("LongRunning")]
    public class CameraControllerGoProSingleTest : TestBaseNunit
    {
        // ReSharper disable once UnusedMember.Local
        private static readonly Logger log = LogManager.GetCurrentClassLogger();

        [Test]
        public void TestAccountInfoGet()
        {
            var outputDir = GetFullOutputDir();

            var configFile = outputDir + "camera_config.json";
            EnsureFileNotExists(configFile);

            var ccgp = CameraControllerGoProSingle.GetInstance();
            ccgp.PictureSaveDirectory = new DirectoryInfo(outputDir);

            Assert.That(ccgp.Init());
            Assert.That(ccgp.NbrCameraReady() == 1);

            var config = ccgp.GetCameraConfiguration(0);
            JsonUtil.WriteJsonFile(new FileInfo(configFile), config);
            Assert.That(File.Exists(configFile));
            
        }

        [Test]
        public void TestTakeSinglePictureGoPro()
        {
            var outputDir = GetFullOutputDir();
            
            var ccgp = CameraControllerGoProSingle.GetInstance();
            ccgp.PictureSaveDirectory = new DirectoryInfo(outputDir);

            Assert.That(ccgp.Init());
            Assert.That(ccgp.NbrCameraReady() == 1);
            
            var listener = new MockPictureListener();

            var request = new PictureRequestSingle
            {
                SaveDirectory = ccgp.PictureSaveDirectory,
                PictureListener = listener
            };
            ccgp.TakePictures(request);

            while (!request.Cancelled && !listener.Finished)
            {
                Thread.Sleep(100);
            }

            Assert.That(listener.PictureFile?.Exists ?? false);
        }

        [Test]
        public void TestTakeVideoGoPro()
        {
            var outputDir = GetFullOutputDir();

            var ccgp = CameraControllerGoProSingle.GetInstance();
            ccgp.PictureSaveDirectory = new DirectoryInfo(outputDir);

            Assert.That(ccgp.Init());
            Assert.That(ccgp.NbrCameraReady() == 1);
            
            var request = new VideoRequestSingle
            {
                CountDown = 0,
                Duration = 20,
                SaveDirectory = ccgp.PictureSaveDirectory
            };

            var take = new VideoTakeGoPro(ccgp.GetCamera(), request);

            var cts = new CancellationTokenSource();
            var task = Task.Run(() => take.StartCapture(cts.Token), cts.Token);

            Task.WaitAny(new Task[] {task}, 60 * 1000);
            
            Assert.That(take.VideoFile?.Exists ?? false);
        }

        /// <summary>
        /// Ce test vise à vérifier quelle est la durée maximale que la GoPro peut prendre des vidéos à intervalles réguliers avant soit:
        /// 1- De manquer de batteries
        /// 2- Un problème de connexion.
        /// 3- Une cessation de connexion.
        /// Comme la version 3 est problématique, cela vérifie si les contre-mesures de "keep-alive" sont efficaces.
        /// </summary>
        [Test]
        [Category("Long")]
        public void TestTakeVideoGoProLongPeriod()
        {
            var outputDir = GetFullOutputDir();

            var ccgp = CameraControllerGoProSingle.GetInstance();
            ccgp.PictureSaveDirectory = new DirectoryInfo(outputDir);

            Assert.That(ccgp.Init());
            Assert.That(ccgp.NbrCameraReady() == 1);

            var request = new VideoRequestSingle
            {
                CountDown = 0,
                Duration = 20,
                SaveDirectory = ccgp.PictureSaveDirectory
            };

            var time = new Stopwatch();
            var timeInfo = 0;
            time.Start();
            Exception err;

            while (true)
            {
                try
                {
                    var cam = ccgp.GetCamera();
                    var take = new VideoTakeGoPro(cam, request);

                    var cts = new CancellationTokenSource();
                    var task = Task.Run(() => take.StartCapture(cts.Token), cts.Token);

                    Task.WaitAny(new Task[] {task}, 60 * 1000);
                
                    if (take.VideoFile != null && take.VideoFile.Exists)
                    {
                        File.Delete(take.VideoFile.FullName);
                    }
                    else
                    {
                        err = new FileNotFoundException("The video file is not found or valid");
                        break;
                    }

                    Thread.Sleep(30 *1000);
                    // ReSharper disable once UnusedVariable
                    Mode test;
                    cam.Mode(out test);
                    Thread.Sleep(15 * 1000);

                    var elapsedMin = time.Elapsed.TotalMinutes;
                    var tmp = (int)Math.Floor(elapsedMin / 5.0);
                    if (tmp > timeInfo) continue;
                    timeInfo = tmp;
                    var tmpStr = $"Elapsed time: {elapsedMin} minutes. Camera mode: {test}";
                    log.Info(tmpStr);
                    Debug.WriteLine(tmpStr);
                }
                catch (Exception e)
                {
                    err = e;
                    break;
                }
            }

            time.Stop();
            var testResults = $"Test duration: {time.Elapsed.TotalMinutes} minutes.";
            log.Info(testResults);
            log.Info(err, "Reason for stop");
            Debug.WriteLine(testResults);
        }

        /// <summary>
        /// Ce test vise à vérifier combien de temps la caméra peut survivre connecté en "stand-by" avec une commande "keep alive".
        /// </summary>
        [Test]
        [Category("Long")]
        public void TestGoProLongPeriodStandby()
        {
            var outputDir = GetFullOutputDir();

            var ccgp = CameraControllerGoProSingle.GetInstance();
            ccgp.PictureSaveDirectory = new DirectoryInfo(outputDir);

            Assert.That(ccgp.Init());
            Assert.That(ccgp.NbrCameraReady() == 1);
            
            var time = new Stopwatch();
            time.Start();
            Exception err;

            var timeInfo = 0;

            log.Info("Starting endurance test for standby with GoPro Camera");
            Debug.WriteLine("Starting endurance test for standby with GoPro Camera");

            while (true)
            {
                try
                {
                    var cam = ccgp.GetCamera();
                    
                    Thread.Sleep(30 * 1000);
                    // ReSharper disable once UnusedVariable
                    Mode test;
                    cam.Mode(out test);

                    var elapsedMin = time.Elapsed.TotalMinutes;
                    var tmp = (int)Math.Floor(elapsedMin / 5.0);
                    if (tmp > timeInfo) continue;
                    timeInfo = tmp;
                    var tmpStr = $"Elapsed time: {elapsedMin} minutes. Camera mode: {test}";
                    log.Info(tmpStr);
                    Debug.WriteLine(tmpStr);
                }
                catch (Exception e)
                {
                    err = e;
                    break;
                }
            }

            time.Stop();
            var testResults = $"Test duration: {time.Elapsed.TotalMinutes} minutes.";
            log.Info(testResults);
            log.Info(err, "Reason for stop");
            Debug.WriteLine(testResults);
        }
    }
}