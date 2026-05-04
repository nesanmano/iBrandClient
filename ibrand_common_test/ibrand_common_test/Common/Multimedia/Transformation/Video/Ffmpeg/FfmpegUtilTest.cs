using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Timers;
using NLog;
using NomadLogic.Common.Respondent;
using NomadLogic.Common.Util;
using NomadLogic.Common.Util.DateAndTime;
using NomadLogic.Common.Util.Math;
using NomadLogic.Multimedia;
using NomadLogic.Multimedia.Cli.Ffmpeg;
using NUnit.Framework;

namespace NomadLogic.Test.Common.Multimedia.Transformation.Video.Ffmpeg
{
    public class FfmpegUtilTest : TestBaseFfmpeg
    {
        // ReSharper disable once UnusedMember.Local
        private static readonly Logger log = LogManager.GetCurrentClassLogger();
        
        [Test]
        public void TestFfmpegProbeFileGeneration()
        {
            var outDir = GetFullOutputDir();

            var inVideo = new FileInfo(BaseDir + "original_green_120fps.MP4");
            var outSpecs = new FileInfo(outDir + Path.DirectorySeparatorChar + "specs.txt");
            var outSpecsUtil = new FileInfo(outDir + Path.DirectorySeparatorChar + "specs_from_util.txt");

            LaunchProcessUtil.LaunchProcess(ExecLocationFfprobe, $"-show_streams \"{inVideo}\"", outSpecs);

            Assert.That(File.Exists(outSpecs.FullName));

            var md = FfmpegUtil.GetVideoFileMetadata(ExecLocationFfprobe, inVideo, outSpecsUtil);

            Assert.That(md != null);
            Assert.That(File.Exists(outSpecsUtil.FullName));
        }

        [Test]
        public void TestFfmpegProbeMetaData()
        {
            var outDir = GetFullOutputDir();

            var inVideo = BaseDir + "original_green_120fps.MP4";
            var outSpecs = outDir + Path.DirectorySeparatorChar + "specs.txt";

            LaunchProcessUtil.LaunchProcess(ExecLocationFfprobe, $"-show_streams \"{inVideo}\"", new FileInfo(outSpecs));

            Assert.That(File.Exists(outSpecs));

            var vmd = VideoMetadata.FromTextFile(new FileInfo(outSpecs));

            Assert.IsNotNull(vmd);
            
            Assert.IsNotEmpty(vmd.Streams);

            Assert.IsNotNull(vmd.Streams[0]);

        }

        [Test]
        public void TestVideoMetaDataGeneration()
        {
            var inSpecFile = BaseDir + Path.DirectorySeparatorChar + "video_data.txt";

            var vmdInput = VideoMetadata.FromTextFile(new FileInfo(inSpecFile));
            var vmdCheck = new VideoMetadata
            {
                Streams = new List<StreamData>
                {
                    // Video
                    new StreamData
                    {
                        Index = 0,
                        Resolution = new Size(1920, 1080),
                        PixelAspectRatio = new Fraction(0, 1),
                        VideoAspectRatio = new Fraction(0, 1),
                        PixelFormat = "yuvj420p",
                        FrameRate = new Fraction(30000, 1001),
                        StartTime = 0.0,
                        DurationSeconds = 4.804800,
                        BitRate = 1524200,
                        BitRateMax = null,
                        FrameCount = 144,
                        Codec = new CodecInfo
                        {
                            CodecFullName = "H.264 / AVC / MPEG-4 AVC / MPEG-4 part 10",
                            CodecShortName = "h264",
                            CodecTag = "avc1",
                            CodecTagHex = "0x31637661",
                            CodecTimeBase = new Fraction(1001, 60000),
                            CodecType = CodecTypeEnum.video
                        },
                        Tags = new Dictionary<string, string>
                        {
                            {"language", "eng" },
                            {"handler_name", "VideoHandler" }
                        },
                        Dispositions = new Dictionary<string, string>
                        {
                            {"default", "1" },
                            {"dub", "0" }
                        }
                    },
                    // Audio
                    new StreamData
                    {
                        Index = 1,
                        FrameRate = new Fraction(0,0),
                        StartTime = -0.021333,
                        DurationSeconds = 4.826125,
                        BitRate = 125390,
                        BitRateMax = 128000,
                        FrameCount = 227,
                        Codec = new CodecInfo
                        {
                            CodecFullName = "AAC (Advanced Audio Coding)",
                            CodecShortName = "aac",
                            CodecTag = "mp4a",
                            CodecTagHex = "0x6134706d",
                            CodecTimeBase = new Fraction(1, 48000),
                            CodecType = CodecTypeEnum.audio
                        },
                        Tags = new Dictionary<string, string>
                        {
                            {"language", "eng" },
                            {"handler_name", "SoundHandler" }
                        },
                        Dispositions = new Dictionary<string, string>
                        {
                            {"default", "1" },
                            {"dub", "0" }
                        },
                        Others = new Dictionary<string, string>
                        {
                            {"sample_fmt", "fltp" },
                            {"sample_rate", "48000" },
                            {"channels", "2" },
                            {"channel_layout", "stereo"},
                            {"bits_per_sample", "0"}
                        }
                    }
                }
            };
            
            Assert.AreNotSame(vmdCheck, vmdInput);
            Assert.AreEqual(vmdCheck, vmdInput);
        }

        [Test]
        [SuppressMessage("ReSharper", "PossibleNullReferenceException")]
        public void TestVideoCutSlowMo()
        {
            var outDir = GetFullOutputDir();
            
            var inVideo = BaseDir + Path.DirectorySeparatorChar + "original_green_120fps.mp4";
            var midVideo = new FileInfo(outDir + Path.DirectorySeparatorChar + "cut.mp4");
            var specFile = new FileInfo(outDir + Path.DirectorySeparatorChar + "specs.txt");
            Assert.That(File.Exists(inVideo));
            EnsureFileNotExists(midVideo);
            EnsureFileNotExists(specFile);

            var slowVideo = outDir + Path.DirectorySeparatorChar + "slow.mp4";
            EnsureFileNotExists(slowVideo);

            const double DURATION = 2.9;
            const double START = 8.0;
            const double END = START + DURATION;

            FfmpegUtil.CutVideo(ExecLocationFfmpeg, inVideo, midVideo.FullName, START, END);
            
            Assert.That(File.Exists(midVideo.FullName));

            var metaMid = FfmpegUtil.GetVideoFileMetadata(ExecLocationFfprobe, midVideo, specFile);
            Assert.That(File.Exists(specFile.FullName));

            Assert.That(metaMid.Streams.Count >= 1);
            var midStrVid = metaMid.GetMainVideoStreamData();
            Assert.That(midStrVid.Resolution.HasValue);
            
            Assert.GreaterOrEqual(midStrVid.DurationSeconds + 0.1, DURATION); // Précis au dixième de seconde.

            const double SCALE = 0.1;
            // ReSharper disable once PossibleInvalidOperationException
            var size = midStrVid.Resolution.Value;
            var crop = size.Width - size.Height;
            Assert.NotNull(midStrVid.FrameRate);
            FfmpegUtil.SlowCropVideo(ExecLocationFfmpeg, midVideo.FullName, slowVideo, SCALE, midStrVid.FrameRate.DoubleValue * SCALE, crop);

           Assert.That(File.Exists(slowVideo));
        }

        [Test]
        [Category("Long")]
        public void TestChromakeyFfmpeg()
        {
            var outDir = GetFullOutputDir();

            for (var blend = 0.01f; blend < 0.15f; blend += 0.02f)
            {
                for (var similarity = 0.05f; similarity < 0.2f; similarity += 0.02f)
                {
                    {
                        var inImage = BaseDir + Path.DirectorySeparatorChar + "not_keyed_bluescreen.jpg";
                       Assert.That(File.Exists(inImage));

                        var outImage = outDir + Path.DirectorySeparatorChar + $"result_blue_{StringUtil.FormatFloat(2, similarity)}_{StringUtil.FormatFloat(2, blend)}.png";
                        EnsureFileNotExists(outImage);

                        var chroma = new FfmpegChromaKeyInfo
                        {
                            Red = 96,
                            Green = 184,
                            Blue = 255,
                            Blend = blend,
                            Similarity = similarity
                        };

                        FfmpegUtil.ChromakeyImage(ExecLocationFfmpeg, inImage, outImage, chroma);

                       Assert.That(File.Exists(outImage));

                        ImageUtil.DebugDisplayTransparencyOnImage(outImage);
                    }

                    {
                        var inImage = BaseDir + Path.DirectorySeparatorChar + "not_keyed_bad.jpg";
                       Assert.That(File.Exists(inImage));

                        var outImage = outDir + Path.DirectorySeparatorChar + $"result_green_{StringUtil.FormatFloat(2, similarity)}_{StringUtil.FormatFloat(2, blend)}.png";
                        EnsureFileNotExists(outImage);

                        FfmpegUtil.ChromakeyImage(ExecLocationFfmpeg, inImage, outImage, 84, 167, 104, similarity, blend);

                        Assert.That(File.Exists(outImage));

                        ImageUtil.DebugDisplayTransparencyOnImage(outImage);
                    }
                }
            }
        }

        /// <summary>
        /// Test la création d'une commande valide avec la méthode qui lit une commande FFMPEG et est capable de remplacer les token dans le contenu du fichier en
        /// réponse de question et fichiers externes.
        /// </summary>
        [Test]
        public void TestFfmpegCreateCommandFromFileRegex()
        {
            var outDir = GetFullOutputDir();

            var baseDirInput = BaseDir + Path.DirectorySeparatorChar;
            var inCommandFile = baseDirInput + "test_command_ffmpeg_regex.txt";
            var finalFile = outDir + "finalFile.mp4";

            Assert.That(File.Exists(inCommandFile));
            EnsureFileNotExists(finalFile);

            // Besoin d'une répertoire de base, d'un fichier final, et de trois réponses (vitesse en float, duration fade et fichier original)
            var dicAnswers = new List<Answer> {
                  new Answer {QuestionNo = 1, ChoiceNo = 1, Response = "0.10"}, // vitesse
                  new Answer {QuestionNo = 2, ChoiceNo = 2, Response = "0.8"}, // duration fade
                  new Answer {QuestionNo = 3, ChoiceNo = 1, Response = baseDirInput + "swing_slow_crop.mp4"} // fichier intrant
            };
            
            var tokenCommand = FileUtil.ReadFileToStringSingleLine(inCommandFile);
            var modCommand = StringUtil.CreateStringFromTokenizedPatternRespondent(
                tokenCommand, 
                dicAnswers.ToDictionary(c => c.QuestionIdentifier), 
                new QuestionIdentifier(3), null, baseDirInput, finalFile);

            LaunchProcessUtil.LaunchProcess(ExecLocationFfmpeg, modCommand);

            var fileTest = new FileInfo(finalFile);
            Assert.That(fileTest.Exists, "No output file.");
            Assert.That(fileTest.Length > 100, "Output file is way too small to be correct.");

        }

        /// <summary>
        /// Teste la création de vidéo (GIF-Like) à partir d'une séquence d'image limitée.
        /// </summary>
        [Test]
        public void TestFfmpegCreateVideoFromFiles()
        {
            var outDir = GetFullOutputDir();

            var outDirWork = Path.GetTempPath() + GenerateUniqueId.GetId() + Path.DirectorySeparatorChar;
            EnsureDirExists(outDirWork);
            Console.WriteLine(@"Tmp directory created: " + outDirWork);

            var finalVideoFile = outDir + "video.mp4";
            EnsureFileNotExists(finalVideoFile);
            Console.WriteLine(@"File deleted: " + finalVideoFile);

            var baseDirInput = BaseDir + Path.DirectorySeparatorChar;
            IList<FileInfo> inputImages = new List<FileInfo> {
                new FileInfo(baseDirInput + "001.jpg"),
                new FileInfo(baseDirInput + "002.jpg"),
                new FileInfo(baseDirInput + "003.jpg"),
                new FileInfo(baseDirInput + "004.jpg")};
            const int NBR_ITER = 4;
            
            var workDirInfo = new DirectoryInfo(outDirWork);

            var nbrDigit = FfmpegUtil.PrepareDirectoryForImageListVideoCreation(inputImages, workDirInfo, NBR_ITER, false);
            workDirInfo.Refresh();
            Assert.AreEqual(workDirInfo.EnumerateFiles().Count(), NBR_ITER * inputImages.Count);

            FfmpegUtil.CreateVideoFromPictureFiles(ExecLocationFfmpeg, workDirInfo, $"%{nbrDigit}d.jpg", finalVideoFile, 3, 15, 0);
            
            Assert.That(File.Exists(finalVideoFile));
            Assert.GreaterOrEqual(new FileInfo(finalVideoFile).Length, 100);

            FileUtil.DeleteFolderContent(outDirWork);

            Assert.That(!Directory.Exists(outDirWork));

            Console.WriteLine(@"File created: " + finalVideoFile);
        }

        [Test]
        public void TestDirectShowDetectAndList()
        {
            Assert.IsTrue(File.Exists(ExecLocationFfmpeg));

            var outDir = GetFullOutputDir();
            EnsureDirEmpty(outDir);
            
            var outFileFind = new FileInfo(outDir + "ffmpeg_ds_find_data.txt");

            var sources = ExtractDirectShowSources(outDir, outFileFind, true);

            foreach (var o in sources.VideoDevices.Aggregate(new List<FfmpegDirectShowDeviceSourceVideoOptionData>(),
                (list, vd) =>
                {
                    if (vd.Options is FfmpegDirectShowDeviceSourceVideoOptions vOpt) list.AddRange(vOpt.VideoOptions);
                    return list;
                }))
            {
                Assert.NotZero(o.FrameSize.Width);
                Assert.NotZero(o.FrameSize.Height);
                Assert.AreEqual(2, o.FpsRange.Count);
                if (!(o.VideoCodec != null || o.PixelFormat != null))
                {
                    Console.Out.WriteLine("\t- Bizarre codec/pixel format detected.");
                }

                Console.Out.WriteLine($"\t{o}");
            }
        }

        private VideoDirectShowSources ExtractDirectShowSources(string outDir, FileInfo outFileFind = null, bool createIndividualFileForSource = false, bool outputToCOnsole = true)
        {
            var sources = FfmpegUtil.FindDirectShowSources(ExecLocationFfmpeg, outFileFind);

            var vidDevices = sources?.VideoDevices;
            Assert.That(vidDevices != null);
            if (vidDevices.Count < 1)
            {
                Assert.Inconclusive("No video webcam detected. Cannot proceed further.");
            }

            if (outputToCOnsole) Console.Out.WriteLine($"{vidDevices.Count} video device detected.");
            if (outputToCOnsole) Console.Out.WriteLine($"{sources.AudioDevices.Count} audio device detected. (ignored for options)");

            foreach (var vd in vidDevices)
            {
                if (outputToCOnsole) Console.Out.WriteLine($"Device '{vd}' detected. Type: {vd.DeviceType}");

                var opt = FfmpegUtil.UpdateDirectShowSourceData(ExecLocationFfmpeg, vd, 
                    createIndividualFileForSource ? 
                        new FileInfo(outDir + "ffmpeg_ds_opt_" + vd.Name.Replace(' ', '_') + ".txt") : 
                        null);
                Assert.That(opt != null);
                Assert.That(!string.IsNullOrWhiteSpace(opt.PinName));
                Assert.That(ReferenceEquals(opt, vd.Options));
            }

            return sources;
        }
        
        [TestCase(1920, 1080, 30, null)]
        [TestCase(1280, 720, 90, "7200k")]
        [TestCase(1920, 1080, 60, "20000k")]
        [TestCase(2560, 1440, 30, "30000k")]
        [TestCase(4096, 2160, 30, "40000k")]
        public void TestDirectShowDetectHighestQualityAndCapture(int width, int height, double desiredMaxFps, string bitRate)
        {
            Assert.IsTrue(File.Exists(ExecLocationFfmpeg));

            var outDir = GetFullOutputDir();
            
            var sources = ExtractDirectShowSources(outDir, null, false, false);

            var desiredSize = new Size(width, height);

            var success = sources.GetBestVideoFeed(desiredSize, desiredMaxFps, out var source, out var canal);
            if (!success)
            {
                Assert.Inconclusive("No DirectShow video output with desired quality found.");
            }
            Assert.IsNotNull(source);
            Assert.IsNotNull(canal);
            Assert.IsTrue(canal.FrameSize.Equals(desiredSize), "Candidate found does not match desired frame size.");
            Assert.IsTrue(canal.FpsRange[1] >= desiredMaxFps, "Candidate found as incorrect FPS requirement." );

            var outPutFile = new FileInfo(outDir + $"extracted_video_{canal.SizeDesc}_{(int)desiredMaxFps}.mp4");
            EnsureFileNotExists(outPutFile);
            var outPutFileStdErr = new FileInfo(outDir + $"extracted_video_{canal.SizeDesc}_{(int)desiredMaxFps}_stdErr.txt");
            EnsureFileNotExists(outPutFileStdErr);

            // On tente maintenant de capturer un video avec cette source.
            success = FfmpegUtil.CaptureVideoFromDirectShowSource(CancellationToken.None, ExecLocationFfmpeg, source, canal, 5f, outPutFile,
                false, "500M", bitRate, outPutFileStdErr).GetAwaiter().GetResult();
            Assert.IsTrue(success, "Could not launch FFMPEG capture command.");

            outPutFile.Refresh();

            Assert.IsTrue(File.Exists(outPutFile.FullName), "Video file does not exists.");
            Assert.Greater(outPutFile.Length, 100, "Video file way too small to be correct.");

            var vidData = FfmpegUtil.GetVideoFileMetadata(ExecLocationFfprobe, outPutFile)?.GetMainVideoStreamData();
            Assert.IsNotNull(vidData);
            Assert.That(vidData.Resolution.Equals(desiredSize), "Incorrect frame size detected in video.");
            Console.Out.WriteLine("Frame size: " + canal.SizeDesc);
            if (vidData.FrameRate != null && Math.Abs(vidData.FrameRate.DoubleValue - desiredMaxFps) > 0.01)
            {
                Console.Out.WriteLine("Video frame output does not match frame sent. Real FPS => " + vidData.FrameRate.DoubleValue);
            }
            Console.Out.WriteLine($"FPS: {canal.FpsMaxDesc}");
            Console.Out.WriteLine($"File Size: {MathUtil.ByteNumberReadable(outPutFile.Length)}");
            Console.Out.WriteLine("------------ Video Meta-Data follows ---------------------");
            Console.Out.WriteLine(vidData.ToStringPublicProperties(1));
        }

        [Test]
        public async Task TestMultipleCliInstanceFfmpeg()
        {
            Assert.IsTrue(File.Exists(ExecLocationFfmpeg));
            var outDir = GetFullOutputDir();
            EnsureDirEmpty(outDir);

            var files = new []
            {
                BaseDir + "countdown_10to1_hd.mp4",
                BaseDir + "original_green_120fps.mp4",
                BaseDir + "swing_green.mp4",
                BaseDir + "swing_slow_crop.mp4",
                BaseDir + "swing_slow_crop_green.mp4",
            };

            var syncTime = new Stopwatch();
            syncTime.Start();

            var tasks = files.Select(videoFile => Task.Run(() => { LaunchVideoTransform(videoFile, outDir, "_ASYNC.mp4"); })).ToArray();

            Task.WaitAll(tasks);
            syncTime.Stop();
            await Console.Out.WriteLineAsync("Total time for async transformation: " + syncTime.Elapsed.ToMinimumReadableTime());

            await Task.Delay(1000);
            syncTime.Reset();
            syncTime.Start();

            foreach (var vi in files)
            {
                LaunchVideoTransform(vi, outDir, "_LINEAR.mp4");
            }

            syncTime.Stop();
            await Console.Out.WriteLineAsync("Total time for linear (one by one) transformation: " + syncTime.Elapsed.ToMinimumReadableTime());
        }

        private void LaunchVideoTransform(string videoFile, string outDir, string extension)
        {
            var timer = new Stopwatch();
            timer.Start();
            Console.Out.WriteLine($"Started transformation of file: {videoFile}");
            var outFile = outDir + StringUtil.RemoveExtension(StringUtil.GetFileName(videoFile)) + extension;
            Assert.IsTrue(File.Exists(videoFile));
            Assert.IsFalse(File.Exists(outFile));

            FfmpegUtil.CompressVideo(ExecLocationFfmpeg, videoFile, outFile, 24, true);
            Assert.IsTrue(File.Exists(outFile));
            timer.Stop();
            Console.Out.WriteLine($"Transformation finished: time={timer.Elapsed.ToMinimumReadableTime()}, out={StringUtil.GetFileName(outFile)}");
        }

        [TestCase(new []
        {
            @"rtsp://admin:iBrand2021@192.168.1.81:554/cam/realmonitor?channel=1&subtype=0", 
            @"rtsp://admin:iBrand2021@192.168.1.80:554/cam/realmonitor?channel=1&subtype=0"
        },new []
        {
            "ip_cam_test_W12X.mp4",
            "ip_cam_test_WAX.mp4"
        })]
        public async Task TestMultipleIpCameraReadingsFfmpeg(string[] url, string[] filename)
        {
            Assert.IsTrue(File.Exists(ExecLocationFfmpeg));
            var outDir = GetFullOutputDir();
            EnsureDirEmpty(outDir);

            var fileDir = BaseDir + @"ip_cam\";
            var scriptFile = fileDir + "ffmpeg_cam_capture.txt";
            Assert.IsTrue(url != null && filename != null && url.Length == filename.Length);
            Assert.IsTrue(File.Exists(scriptFile));

            var syncTime = new Stopwatch();
            syncTime.Start();

            var cts = new CancellationTokenSource(TimeSpan.FromSeconds(20));

            var l = url.Length;
            var tasks = new Task[l];
            
            for (var i = 0; i < l; i++)
            {
                var index = i;
                tasks[i] = Task.Run(async () =>
                {
                    await LaunchIpCamCapture(cts.Token, url, filename, index, outDir, scriptFile);
                }, cts.Token);
            }
            
            await Task.WhenAll(tasks);
            syncTime.Stop();
            await Console.Out.WriteLineAsync("Total time for async capture: " + syncTime.Elapsed.ToMinimumReadableTime());
        }

        private async Task LaunchIpCamCapture(CancellationToken token, string[] url, string[] filename, int index, string outDir, string scriptFile)
        {
            var dico = new Dictionary<QuestionIdentifier, Answer>
            {
                {new QuestionIdentifier(1), new Answer {Response = StringUtil.FormatTime(15)}},
                {new QuestionIdentifier(2), new Answer {Response = url[index]}}
            };
            var finalFile = outDir + filename[index];
            EnsureFileNotExists(finalFile);

            var tokenCommand = FileUtil.ReadFileToStringSingleLine(scriptFile);
            var modifiedCommandLine = StringUtil.CreateStringFromTokenizedPatternRespondent(tokenCommand, dico,
                null, null, null, finalFile);

            Assert.IsFalse(string.IsNullOrWhiteSpace(modifiedCommandLine));

            await LaunchProcessUtil.LaunchProcessAsync(token, ExecLocationFfmpeg, modifiedCommandLine);

            Assert.IsTrue(File.Exists(finalFile));
        }
    }
}
