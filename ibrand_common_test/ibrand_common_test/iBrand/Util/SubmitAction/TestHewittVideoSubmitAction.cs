using System;
using System.Collections.Generic;
using System.Configuration;
using System.IO;
using NLog;
using NomadLogic.Common.Util;
using NomadLogic.iBrandDesktop.Util.SubmitAction;
using NUnit.Framework;

namespace NomadLogic.Test.iBrand.Util.SubmitAction
{
    public class TestHewittVideoSubmitAction : TestBaseNunit
    {
        private static readonly Logger log = LogManager.GetCurrentClassLogger();


        /* Pour le test de la transformation du texte, voir ImageUtilTest.PutRotatedCustomFontTextOnImage() */

        [Test]
        public void CreateHewittVideo()
        {
            const string baseDir = @"C:\Users\François Coupal\Documents\projets\2016\HEW-2016-01 Video\test_video\";

            const string videoIntro = baseDir + "introHewitt_720x406.mp4";
            Assert.IsTrue(File.Exists(videoIntro));
            const string videoOutro = baseDir + "outroHewitt_720x406.mp4";
            Assert.IsTrue(File.Exists(videoOutro));
            const string soundtrack = baseDir + "sound.aac";
            Assert.IsTrue(File.Exists(soundtrack));
            const string videoTransition = baseDir + "transitionHewitt_720x406.mov";
            Assert.IsTrue(File.Exists(videoTransition));
            const string logoOverlay = baseDir + "overlay_720x406.png";
            Assert.IsTrue(File.Exists(logoOverlay));
            const string videoRespondant = baseDir + "GOPR0835.MP4";
            Assert.IsTrue(File.Exists(videoRespondant));

            const string output = @"C:\Users\François Coupal\Documents\projets\2016\HEW-2016-01 Video\test_video\result\" + "render_test.mp4";
            EnsureFileNotExists(output);

            HewittVideoSubmitAction.CreateHewittVideo(output, videoIntro, videoOutro, videoTransition,
                logoOverlay, videoRespondant, 18.34f,
                8, 15, 11,
                1, 2, "720x406",
                soundtrack, "");

            Assert.IsTrue(File.Exists(output));

        }

        private class RespondentRencodeStatus
        {
            public int RespondentId;
            public int SynchroNo;
            public string OriginalFile;
            public string ReEncodedFile;
            public int VimeoId;
        }

        [Test]
        public void RedoRespondentsVideo()
        {
            const string baseDir = @"C:\Users\François Coupal\Documents\projets\2016\HEW-2016-01 Video\reencode\";

            const string movDir = baseDir + @"originals\";
            EnsureDirExists(movDir);
            const string mp4Dir = baseDir + @"reencoded\";
            EnsureDirExists(mp4Dir);

            // Fichiers video
            const string baseDirVideo = @"C:\Users\François Coupal\Documents\projets\2016\HEW-2016-01 Video\test_video\";
            const string videoIntro = baseDirVideo + "introHewitt_720x406.mp4";
            Assert.IsTrue(File.Exists(videoIntro));
            const string videoOutro = baseDirVideo + "outroHewitt_720x406.mp4";
            Assert.IsTrue(File.Exists(videoOutro));
            const string soundtrack = baseDirVideo + "sound.aac";
            Assert.IsTrue(File.Exists(soundtrack));
            const string videoTransition = baseDirVideo + "transitionHewitt_720x406.mov";
            Assert.IsTrue(File.Exists(videoTransition));
            const string logoOverlay = baseDirVideo + "overlay_720x406.png";
            Assert.IsTrue(File.Exists(logoOverlay));

            // Step 1: Lire les informations des répondants via le fichier EXCEL d'extraction de la BD
            const string fichierExcelRespondent = baseDir + @"project_data_824.csv";
            Assert.IsTrue(File.Exists(fichierExcelRespondent));
            const string fichierExcelRespondentOutput = baseDir + @"recoded_824.csv";
            Assert.IsTrue(File.Exists(fichierExcelRespondent));

            // Si le fichier final n'existe pas, on le créé pour la première fois.
            if (!File.Exists(fichierExcelRespondentOutput))
            {
                /*
                {
                    using (var writer = new CsvWriter(new StreamWriter(fichierExcelRespondentOutput), CultureInfo.InvariantCulture))
                    {
                        while (parser.Read())
                        {
                            writer.WriteField(parser.GetField<int>(2));
                            writer.WriteField(parser.GetField<int>(3));
                            var fileName = StringUtil.RemoveExtension(StringUtil.GetFileName(parser.GetField<string>(18).Split('_')[1])) + ".mp4";
                            writer.WriteField(fileName);
                            writer.WriteField(fileName);
                            writer.WriteField(0);

                            writer.NextRecord();
                        }
                    }
                }
                */
            }
            /*
            var statusList = new List<RespondentRencodeStatus>();
            // On lit maintenant le fichier de modifications
            using (var parserOutput = new CsvReader(new StreamReader(fichierExcelRespondentOutput), new CsvConfiguration(CultureInfo.InvariantCulture)
            {
                HasHeaderRecord = false
            }))
            {
                while (parserOutput.Read())
                {
                    var respStatus = new RespondentRencodeStatus
                    {
                        RespondentId = parserOutput.GetField<int>(0),
                        SynchroNo = parserOutput.GetField<int>(1),
                        OriginalFile = movDir + parserOutput.GetField<string>(2),
                        ReEncodedFile = mp4Dir + parserOutput.GetField<string>(3),
                        VimeoId = parserOutput.GetField<int>(4)
                    };

                    statusList.Add(respStatus);
                }
            }
            
            // On vérifie maintenant que chaque répondant a un fichier transformé correspondant
            // ReSharper disable once ForCanBeConvertedToForeach
            for (var i = 0; i < statusList.Count; i++)
            {
                if (statusList[i].VimeoId != 0) continue;
                if (!File.Exists(statusList[i].OriginalFile)) continue;

                var recodedFile = statusList[i].ReEncodedFile;
                if (File.Exists(recodedFile)) continue;

                // On tente de créer le fichier vidéo s'il n'existe pas.
                HewittVideoSubmitAction.CreateHewittVideo(recodedFile, videoIntro, videoOutro,
                    videoTransition, logoOverlay,
                    statusList[i].OriginalFile, 0.0, 8, 15, 11, 1, 2, "720x406", soundtrack, "");
                
                if (!File.Exists(recodedFile))
                {
                    log.Error($"Could not generate video for respondent {statusList[i].RespondentId}-{statusList[i].SynchroNo}");
                    statusList[i].VimeoId = -2;
                }
            }

            // On écrit maintenant le résultat dans le fichier à nouveau.
            ReplaceRespondentFile(fichierExcelRespondentOutput, statusList);

            // ReSharper disable once ForCanBeConvertedToForeach
            /*
            try
            {
                
                for (var i = 0; i < statusList.Count; i++)
                {
                    if (statusList[i].VimeoId != 0) continue;
                    if (!File.Exists(statusList[i].ReEncodedFile)) continue;

                    statusList[i].VimeoId = VimeoUtil.UploadToVimeo(new FileInfo(statusList[i].ReEncodedFile));
                }
            }
            catch (Exception e)
            {
                log.Error("Error during VIMEO send loop. {0}", e.Message);
                Assert.Fail(e.Message);
            }
            finally
            {
                // On écrit maintenant le résultat dans le fichier à nouveau.
                ReplaceRespondentFile(fichierExcelRespondentOutput, statusList);
            }
            */
        }

        private static void ReplaceRespondentFile(string fichierExcelRespondentOutput, IEnumerable<RespondentRencodeStatus> statusList)
        {
            /*
            var tmpFile = Path.GetTempFileName();
            using (var writer = new CsvWriter(new StreamWriter(tmpFile), new CsvConfiguration(CultureInfo.InvariantCulture)
            {
                HasHeaderRecord = false
            }))
            {
                foreach (var statuse in statusList)
                {
                    writer.WriteField(statuse.RespondentId);
                    writer.WriteField(statuse.SynchroNo);
                    writer.WriteField(StringUtil.GetFileName(statuse.OriginalFile));
                    writer.WriteField(StringUtil.GetFileName(statuse.ReEncodedFile));
                    writer.WriteField(statuse.VimeoId);

                    writer.NextRecord();
                }
            }

            File.Delete(fichierExcelRespondentOutput);
            File.Move(tmpFile, fichierExcelRespondentOutput);
            */
        }
    }
}