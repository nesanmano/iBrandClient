using Moq;
using NomadLogic.Common.Util;
using NomadLogic.iBrandDesktop.Synchro.DownloadRestRequest;
using NUnit.Framework;
using RestSharp;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using NomadLogic.Common.Serveur;
using NomadLogic.Common.Util.Networking;

namespace NomadLogic.Test.iBrand.ProjectDownload
{
    public class ProjectDownloadTest :TestBaseNunit
    {
        private Mock<IDatabaseProjectFromServer> _db;

        [SetUp]
        public void SetUpFixture()
        {
            var qId = 0; // Faux, à vérifier.

            _db = new Mock<IDatabaseProjectFromServer>();

            var r1 = new ResourceFileToDownload()
            {
                Src = "t1-2017-01-android-ui.xml",
                ServerModifiedTime = 1525894014107,
                FileSize = 38860
            };

            var r2 = new ResourceFileToDownload()
            {
                Src = "1-Tap to start.jpg",
                ServerModifiedTime = 1525894418851,
                FileSize = 226406
            };

            var r3 = new ResourceFileToDownload()
            {
                Src = "2-Waiver - Rules and regulations.png",
                ServerModifiedTime = 1525894422675,
                FileSize = 12
            };

            var r4 = new ResourceFileToDownload()
            {
                Src = "2-Waiver - Rules and regulations_FR.png",
                ServerModifiedTime = 1525894434121,
                FileSize = 13
            };

            var r5 = new ResourceFileToDownload()
            {
                Src = "2-Waiver_Bottom_banner.png",
                ServerModifiedTime = 1525894438813,
                FileSize = 25
            };

            var r6 = new ResourceFileToDownload()
            {
                Src = "2-Waiver_top_banner.png",
                ServerModifiedTime = 1525894442070,
                FileSize = 26
            };
        
            var r7 = new ResourceFileToDownload()
            {
                Src = "2-Waiver_top_banner_FR.png",
                ServerModifiedTime = 1525894445266,
                FileSize = 30
            };
            var r8 = new ResourceFileToDownload()
            {
                Src = "3-Terms and conditions.jpg",
                ServerModifiedTime = 1525894447886,
                FileSize = 543
            };
            var r9 = new ResourceFileToDownload()
            {
                Src = "3-Terms and conditions_FR.jpg",
                ServerModifiedTime = 1525894450721,
                FileSize = 459
            };

            var r10 = new ResourceFileToDownload()
            {
                Src = "4-Scan_Top_banner_ENG.png",
                ServerModifiedTime = 1525894453484,
                FileSize = 312
            };
            
            var r11 = new ResourceFileToDownload()
            {
                Src = "4-Scan_Top_banner_FR.png",
                ServerModifiedTime = 1525894455996,
                FileSize = 308
            };
            var r12 = new ResourceFileToDownload()
            {
                Src = "5-Thank you.jpg",
                ServerModifiedTime = 1525894458654,
                FileSize = 211
            };


            var r13 = new ResourceFileToDownload()
            {
                Src = "5-Thank you_FR.jpg",
                ServerModifiedTime = 1525894461838,
                FileSize = 198
            };

            var r14 = new ResourceFileToDownload()
            {
                Src = "Background.jpg",
                ServerModifiedTime = 1525894469142,
                FileSize = 140
            };

            var r15 = new ResourceFileToDownload()
            {
                Src = "BigSpin_Watermark.png",
                ServerModifiedTime = 1525894472375,
                FileSize = 28
            };

            var r16 = new ResourceFileToDownload()
            {
                Src = "Bouton_suivant.png",
                ServerModifiedTime = 1525894477218,
                FileSize = 6
            };

            var r17 = new ResourceFileToDownload()
            {
                Src = "Bouton_suivant_ET_précédent.png",
                ServerModifiedTime = 1525894481739,
                FileSize = 8
            };

            var r18 = new ResourceFileToDownload()
            {
                Src = "database.xml",
                ServerModifiedTime = 1525894491148,
                FileSize = 255
            };

            var r19 = new ResourceFileToDownload()
            {
                Src = "facebook.json",
                ServerModifiedTime = 1525894500458,
                FileSize = 0
            };

            var r20 = new ResourceFileToDownload()
            {
                Src = "next_AND_previous_button.png",
                ServerModifiedTime = 1525894503436,
                FileSize = 8
            };

            var r21 = new ResourceFileToDownload()
            {
                Src = "next_button.png",
                ServerModifiedTime = 1525894509063,
                FileSize = 5
            };

            var r22 = new ResourceFileToDownload()
            {
                Src = "subscription.xml",
                ServerModifiedTime = 1525894514177,
                FileSize = 0
            };

            var r23 = new ResourceFileToDownload()
            {
                Src = "t1-2017-01-android.xml",
                ServerModifiedTime = 1536864239462,
                FileSize = 5
            };

            var r24 = new ResourceFileToDownload()
            {
                Src = "email_fr_dl.xml",
                ServerModifiedTime = 1525894891164,
                FileSize = 22
            };

            var r25 = new ResourceFileToDownload()
            {
                Src = "sms_button_inactive.png",
                ServerModifiedTime = 1542147203197,
                FileSize = 26
            };

            var listRes = new List<ResourceFileToDownload> { r1, r2, r3, r4, r5, r6, r7, r8, r9, r10, r11, r12, r13, r14, r15, r16, r17, r18, r19, r20, r21, r22, r23, r24, r25 };


            _db.Setup(p => p.GetResourceFiles(It.IsAny<int>())).Returns(listRes);

            _db.Setup(p => p.UpdateResourceFileData(qId, It.IsAny<ResourceFileToDownload>())).Returns(1);


        }

        [TearDown]
        public void TearDown()
        {
            _db = null;
        }
        
        /*
        /// <summary>
        /// Cette méthode néecessite un serveur de test soit un localhost ou un en prod
        /// </summary>
        /// <returns></returns>
        [Test]
        public async Task StartDownloadTaskRestClientLegacy_versionWhenAll()
        {
            //FileInfo fileInfo = new FileInfo(BaseDir + "UnitTest\\");
            //fileInfo.Delete();

            var outputDir = GetFullOutputDir();
            EnsureDirEmpty(outputDir);

            ISet<string> directoryPathSet = new HashSet<string>();
            var token = new CancellationToken();
            var url = "http://localhost:8080/ibrand_app/ibrandLegacy/listFiles?username=mercedez&password=benz&deviceName=pc";

            var result = await HttpRestUtil.ExecuteCompleteRestRequest<List<ResourceFileToDownload>>(url, null,token);

            Console.WriteLine(@"Please wait while we retrieve some data!");

            Assert.IsNotEmpty(result.InstancedBody);

            IList<DownloadReportInfo> rapportCumilatif = new List<DownloadReportInfo>();

            var progress = new Progress<MultipleFileDownloadReport>(res =>
            {
                
            });

            var deltaFileToDownload = ComputeDelta(result.InstancedBody, result.InstancedBody[0].QuestionnaireId, result.InstancedBody[0].ProjectId);

            var doclist = new List<Tuple<Uri, FileInfo, WebHeaderCollection>>();
            // var token = new CancellationTokenSource();

            string path = "";


            foreach (var x in deltaFileToDownload)
            {
                var url2 = UrlBuilder("http://localhost:8080",x.ProjectId, x.QuestionnaireId, -1,x.Src, false);
                //path = outputDir + GenerateUniqueId.GetId() + "__" + StringUtil.GetFileName(x.Src);
                path = DirectoryUtil.CreateLocalTemporaryFile(BaseDir + "UnitTest\\iBrandLegacy\\projects\\1085\\1013\\") + x.Src;
                Directory.CreateDirectory(Path.GetDirectoryName(path));
                directoryPathSet.Add(new FileInfo(path).DirectoryName);
                doclist.Add(new Tuple<Uri, FileInfo, WebHeaderCollection>(new Uri(url2), new FileInfo(path), new WebHeaderCollection()));
            }

            await HttpUtil.DownloadMultipleFilesAsync(doclist, token, progress);
           
            string[] tmp = GetAllReceivedFiles(directoryPathSet);

            Assert.IsTrue(tmp.Length == deltaFileToDownload.Count);

            //var totalBytesExpected = deltaFileToDownload.Sum(d => d.FileSize);
            var totalBytesOnDisk = tmp.Sum(f => (new FileInfo(f).Length));

            //Assert.AreEqual(totalBytesExpected, totalBytesOnDisk, "Total size on disk is not the same as expected size from delta.");

            //Console.Out.WriteLine("Total size D/L: " + totalBytesOnDisk);
        }
        */

        private string[] GetAllReceivedFiles(ISet<string> pathSet)
        {
            List<string> stringList = new List<string>();
            
            foreach(string path in pathSet)
            {
                stringList.AddRange(Directory.GetFiles(path));
            }
            return stringList.ToArray();
        }

        /// <summary>
        /// Cette méthode néecessite un serveur de test soit un localhost ou un en prod
        /// </summary>
        /// <returns></returns>
        [Test]
        public async Task StartDownloadTaskRestClientJuju()
        {

            var login = await Login();
            var url = "http://localhost:8080/downloadProjectInfo?username=big&password=spin&terminalType=Android";
            
            var token = new CancellationToken();

            var pars = new List<RestRequestAdditionalParameters>
            {
                new RestRequestAdditionalParameters("application/json", "Content-Type", HttpParameterType.Header),
                new RestRequestAdditionalParameters(login, "Authorization", HttpParameterType.Header),
            };

            var result = await HttpRestUtil.ExecuteCompleteRestRequestAsync<TerminalDownloadInfo>(token, url, pars);


            Console.WriteLine("Please wait while we retreive some data!");

            Assert.IsNotEmpty(result.Headers);
            Assert.NotNull(result.InstancedBody);

            //calculate delta
            var deltaFileToDownload = ComputeDelta(result.InstancedBody.FilesToDownload.ToList(), result.InstancedBody.QuestionnaireId,result.InstancedBody.ProjectId);

            var progress = new Progress<SingleFileDownloadReport>(res =>
            {
                if (res.Finished)
                {
                    Console.WriteLine(@"Downloaded File Name: " + res.FileName + @"  Bytes downloaded: " + res.ReceivedBytes);
                }
            });

            foreach (var x in deltaFileToDownload)
            {
                var url2 = UrlBuilder("http://localhost:8080", result.InstancedBody.ProjectId, result.InstancedBody.QuestionnaireId, result.InstancedBody.TerminalId, x.Src);
                var path = DirectoryUtil.CreateLocalTemporaryFile(BaseDir + result.InstancedBody.ProjectId + "\\" + result.InstancedBody.QuestionnaireId + "\\") + x.Src;
                var dest = DirectoryUtil.GetLocalApplicationDir() + "\\projects\\" + result.InstancedBody.ProjectId + "\\" + result.InstancedBody.QuestionnaireId + "\\" + x.Src;
                //var token = new CancellationToken();

                //var task2 = HttpUtil.DownloadFileAsync(new Uri(url2), new FileInfo(path), token, header, progress);

                Assert.IsTrue(File.Exists(path));
                    SaveFile(path, dest, x, result.InstancedBody.QuestionnaireId);
            }
        }

        // Async Task est l'equivalent d'une methode avec void.
        public async Task TestDownloadJuju()
        {
            var url2 = UrlBuilder("http://localhost:8080", 24, 27, -1, "t1-2017-01-android-ui.xml");
            var path = DirectoryUtil.CreateLocalTemporaryFile("UnitTest\\iBrandJuJu\\projects\\24\\27\\t1-2017-01-android-ui.xml");


            var x = new ResourceFileToDownload();
            var fileInfo = new FileInfo("t1-2017-01-android-ui.xml");

            var login = Login();
            //var url = "http://localhost:8080/get-project?username=big&password=spin&terminalType=Android";

            var modHeaders = new[]
            {
                new[] {"Authorization", login.Result},
                new[] {"Content-Type", "application/json"}
            };

            var header = new WebHeaderCollection();
            header.Add("Authorization", login.Result);
            header.Add("Content-Type", "application/json");

            var progress = new Progress<SingleFileDownloadReport>(res =>
            {
                if (res.Finished)
                {
                    Console.WriteLine(@"Downloaded File Name: " + res.FileName + @"  Bytes downloaded: " + res.ReceivedBytes);
                }
            });
            var token = new CancellationTokenSource();
            await HttpUtil.DownloadFileAsync(new Uri(url2), fileInfo, token.Token, header, progress);
        }
        
        [Test]
        public async Task TestDownloadWithCancellationProgress()
        {
            var url2 = UrlBuilder("http://localhost:8080", 24, 27,-1, "t1-2017-01-android-ui.xml");
            var path = DirectoryUtil.CreateLocalTemporaryFile(BaseDir+"UnitTest\\iBrandJuJu\\projects\\24\\27\\t1-2017-01-android-ui.xml");

            var x = new ResourceFileToDownload();
            var fileInfo = new FileInfo("t1-2017-01-android-ui.xml");
            x.Src = "t1-2017-01-android-ui.xml";
            x.ServerModifiedTime = 1525894014107;
            x.FileSize = 38860;

            var login = Login();
            var url = "http://localhost:8080/downalodProjectInfo?username=big&password=spin&terminalType=Android";

            /*var modHeaders = new[]
            {
                new [] {"Authorization", login.Result},
                new [] { "Content-Type", "application/json"}
            };*/

            var header = new WebHeaderCollection();
            header.Add("Authorization", login.Result);
            header.Add("Content-Type", "application/json");

            var token = new CancellationTokenSource();

            var progress = new Progress<SingleFileDownloadReport>(res =>
            {
                if (res.Finished)
                {
                    Console.WriteLine(@"Downloaded File Name: " + res.FileName + @"  Bytes downloaded: " + res.ReceivedBytes);
                }
            });

            var task2 = HttpUtil.DownloadFileAsync(new Uri(url2), fileInfo, token.Token, header, progress);

            token.CancelAfter(10);

            await task2;
            Console.WriteLine(task2.Status);

            if (task2.IsCompleted)
            {
                Assert.IsFalse(File.Exists(path));

            }
        }
        
        public async Task<string> Login()
        {
            var url = "http://localhost:8080/login";
            var token = CancellationToken.None;

            var b = new HttpJsonBody("{\"username\":\"devTeam@nomadlogic.com\",\"password\":\"1234\"}");

            var result = await HttpRestUtil.ExecuteCompleteRestRequestAsync<RestResponse>(token, url, b, null, HttpMethod.Post);
            Assert.IsNotNull(result);
            foreach (var param in result.Headers)
            {
                if (param.Key.Equals("Authorization"))
                {
                    return param.Value.First();
                }
            }

            return null;
        }

        /// <summary>
        /// Cette methode permet de generer une liste avec les fichiers à télécharger uniquement
        /// Cette methode necessite la base de donne local de l'application
        /// </summary>
        /// <param name="arr"></param>
        /// <param name="qId"></param>
        /// <returns></returns>
        private List<ResourceFileToDownload> ComputeDelta(List<ResourceFileToDownload> arr, int qId,int pId)
        {
            var ficsToDownload = new List<ResourceFileToDownload>();//_db.Object.GetResourceFiles(qId);


            var output = DirectoryUtil.GetLocalApplicationDir() + "projects\\" + pId + "\\" + qId + "\\";

            var directoryInfo = new DirectoryInfo(output);

            if (directoryInfo.Exists)
            {
                ficsToDownload.AddRange(directoryInfo.GetFiles().Select(f => new ResourceFileToDownload {Src = f.Name}));
            }

            var temp = new List<ResourceFileToDownload>();


            foreach (var src in arr)
            {
                var d = ficsToDownload.FindIndex(e => e.Src.Equals(src.Src));

                if (d != -1)
                {
                    if (src.ServerModifiedTime != ficsToDownload[d].ServerModifiedTime)
                    {
                        temp.Add(src);

                    }
                    else
                    {
                        Console.WriteLine(@"File: " + src.Src + @" Update to date!");
                    }
                    
                    continue;
                }
                temp.Add(src);
            }

            return temp;

        }

        /// <summary>
        /// Cette methode permet d'enregister dans la base de donnee
        /// </summary>
        /// <param name="srcPath"></param>
        /// <param name="destPath"></param>
        private void SaveFile(string srcPath, string destPath, ResourceFileToDownload fic, int qId)
        {

            Directory.CreateDirectory(Path.GetDirectoryName(destPath));
            if (File.Exists(destPath))
                File.Delete(destPath);
            File.Move(srcPath, destPath);
            Assert.IsTrue(File.Exists(destPath));
        }
        
        /// <summary>
        /// Cette methode devrait aller dans une classe util
        /// </summary>
        /// <param name="server"></param>
        /// <param name="pId"></param>
        /// <param name="qId"></param>
        /// <param name="file"></param>
        /// <param name="isJuju"></param>
        /// <returns></returns>
        private string UrlBuilder(string server, int pId, int qId,int tId, string file, bool isJuju = true)
        {
            if (!isJuju)
            {
                if (qId != -1)
                {
                    return server + "/ibrand_app/projects/" + pId + "/" + qId +"/"+ file;
                }
                return server + "/ibrand_app/projects/" + pId + "/" +"/"+ file;      
            }
            return server + "/downloadProject" + "?projectID=" + pId + "&questionnaireID=" + qId + "&clientTypeID=1&fileName=" + file+"&terminalID="+tId;

        }

        public void DownloadCompleted(object sender, AsyncCompletedEventArgs e)
        {
            if (e.Error != null)
            {
                //TODO: Handle error
            }
            else if (e.Cancelled)
            {

            }
            else
            {
                //Assert.IsTrue(File.Exists(path));
                //SaveFile(path, dest, x,x.qId);
                Console.WriteLine(e.UserState);
            }


        }
        
        public void DownloadProgressChanged(object sender, DownloadProgressChangedEventArgs e)
        {
            Console.WriteLine("Total Bytes To Receive: " + e.TotalBytesToReceive);

            Console.WriteLine("Bytes Received: " + e.BytesReceived);

        }

    }
}
