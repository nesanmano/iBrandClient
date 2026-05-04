using System;
using System.Collections.Generic;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using Amazon;
using Amazon.Runtime;
using Amazon.S3;
using Amazon.S3.Model;
using NomadLogic.Common.Util;
using NomadLogic.Common.Util.Json;
using NomadLogic.Common.Util.Networking.QuestionnaireWebRequest.ConnectAndGo;
using NomadLogic.Common.Util.Networking.QuestionnaireWebRequest.ConnectAndGo.Schemas.v1;
using NomadLogic.Common.Util.Networking.QuestionnaireWebRequest.ConnectAndGo.Schemas.v1.payloads;
using NUnit.Framework;
using GrantTypeEnum = NomadLogic.Common.Util.Networking.QuestionnaireWebRequest.ConnectAndGo.Schemas.v1.payloads.GrantTypeEnum;

namespace NomadLogic.Test.Common.Util.QuestionnaireWebRequest.ConnectGo
{
    public class TestBaseConnectAndGo : TestBaseNunitAws
    {
        /// <summary>
        /// Juste pour tester le marshalling en utiliant le résultat d'une commande Postman
        /// </summary>
        [Test]
        public void TestMarshallingAccessLogData()
        {
            const string responseData = "{\"id\":309,\"gate_id\":13,\"uid\":\"A6BAAF10\",\"access_point_id\":1,\"zone_from\":null,\"zone_to\":null,\"kill_switch\":\"NO\",\"validation_type\":\"NONE\",\"bypass_zone_validation\":false,\"update_zone_current\":true,\"wallet_id\":306,\"wallet_type\":{\"id\":1000002,\"name\":\"Premium wristband\",\"name_i18n\":\"{}\",\"issuing\":\"MIFARE\",\"created_at\":1588714279},\"ticket\":{\"id\":327,\"ticket_type\":{\"id\":1000002,\"name\":\"Fulltime membership\",\"name_i18n\":\"{}\",\"validity\":{\"id\":1000002,\"name\":\"season admission\",\"name_i18n\":\"{}\",\"created_at\":1588714152},\"status\":\"ACTIVE\",\"issuing\":\"WALLET\",\"created_at\":1588714415},\"event\":null,\"transaction\":{\"id\":1000276,\"transaction_id\":\"DEM-238\",\"created_at\":1611144507},\"product\":{\"id\":1000016,\"name\":\"48 pouces et plus\",\"image\":\"https://everest-demo.s3.ca-central-1.amazonaws.com/demopark/products/E6KxsYSkN5foMXzNdvsGfpaCB7yWI6zzdKWPTSI2.jpeg\",\"price\":\"$299.00\",\"amount\":99.989999999999995,\"min_quantity\":null,\"max_quantity\":null,\"increment\":null,\"time_based\":false,\"event_group_id\":null,\"event_group_name\":null},\"created_at\":1611144508},\"account\":{\"id\":1000010,\"firstname\":\"Marc-Andre\",\"lastname\":\"Dube\",\"email\":\"marc-andre@connectngo.com\",\"image\":\"https://everest-demo.s3.ca-central-1.amazonaws.com/demopark/accounts/bpAmT85QpogHFtgDiL3ocIiWKZARYzvMDM9uK0CU.png\",\"created_at\":1594674800},\"validation_result\":{\"id\":1,\"name\":\"OK\",\"name_i18n\":\"{\\\"en\\\": \\\"Ok\\\", \\\"fr\\\": \\\"Ok\\\"}\",\"created_at\":1600905730},\"gate_created_at\":1623099124,\"created_at\":1623099124,\"created_by\":1000029,\"addon_cost\":[],\"addon_balances\":[]}";

            Assert.IsTrue(JsonUtil.CreateInstanceFromJson<AccessControlLog>(responseData, out var res));
            Assert.IsNotNull(res);
        }

        [TestCase("A6BAAF10")]
        [TestCase("W82-LFLT-HM-AORB")]
        public async Task TestProcessAccessScan(string uidToTest)
        {
            var wr = new WebRequestConnectAndGo
            {
                BaseUrl = "https://connectngo-demo.com"
            };

            Assert.IsTrue(await wr.InitializeExecutor());

            var logResponse = await wr.CnGLogin(CancellationToken.None, 5, "1q2w3e4r5t","frcoupal@nomadlogic.com","5gjLrwVCmDzgweDhBiyPgUV2VKQi2dIvIBolRSas");

            var token = logResponse?.Data;
            Assert.IsNotNull(token);

            var pars = new ProcessAccessScanPayload
            {
                UniqueId = uidToTest,
                AccessPointId = 1,
                Direction = "IN",
                GateName = "mad-pc",
                XtenantId = 1000001
            };

            var result = await wr.ProcessAccessScan(token, pars);

            Assert.IsNotNull(result);
        }

        [TestCase(AWS_DEFAULT_PROFILE_UNIT_TESTS, "cng-experiential-koolreplay-dev")]
        public async Task TestSendFilesToAwsS3(string profileToLoad, string bucketName, string regionS3 = null)
        {
            var fileToSend = Path.Combine(BaseDir, "swing_green.mp4");
            EnsureFileExists(fileToSend);

            var region = string.IsNullOrWhiteSpace(regionS3) ? RegionEndpoint.USEast2 : RegionEndpoint.GetBySystemName(regionS3);

            var s3Config = new AmazonS3Config
            {
                ReadWriteTimeout = TimeSpan.FromSeconds(10),
                Timeout = TimeSpan.FromSeconds(10),
                RetryMode = RequestRetryMode.Standard,
                MaxErrorRetry = 0,
                RegionEndpoint = region
            };

            var client = new AmazonS3Client(AwsCredentials, s3Config);

            var awsPutRequest = new PutObjectRequest
            {
                BucketName = bucketName,
                ContentType = "application/octet-stream",
                FilePath = fileToSend,
                ReadWriteTimeout = TimeSpan.FromSeconds(10),
                Timeout = TimeSpan.FromSeconds(10),
                Key = "swing_green.mp4",
                
            };

            awsPutRequest.Metadata.Add("x-amz-meta-title", "Unit Testing S3");
            var awsResponse = await client.PutObjectAsync(awsPutRequest);

            Assert.IsNotNull(awsResponse?.ETag);
            await Console.Out.WriteLineAsync("PutObject Metadata call:");
            await Console.Out.WriteLineAsync(awsResponse.ToStringPublicProperties());

            // TODO Dû à des problèmes d'authentification, un "Get" est présentement impossible sur ces objets.
            /*
            var awsGetMetaRequest = new GetObjectRequest
            {
                BucketName = bucketName,
                EtagToMatch = awsResponse.ETag,
                Key = "swing_green.mp4"
            };

            var getAwsResp = await client.GetObjectAsync(awsGetMetaRequest);
            Assert.IsNotNull(getAwsResp);
            await Console.Out.WriteLineAsync("GetObject Metadata call:");
            await Console.Out.WriteLineAsync(getAwsResp.ToStringPublicProperties());
            */
        }

        [Test]
        public void TestMediaCallMarshalling()
        {
            var file = new FileInfo(BaseDir + @"WebRequestFacade\ConnectAndGo\media_request_result.json");
            EnsureFileExists(file);

            //var res = JsonUtil.ReadJsonFile<MediaResult>(file);
            var res = JsonUtil.ReadJsonFile<List<MediaResultItem>>(file);
            Assert.IsNotNull(res);
        }

        [TestCase("A6BAAF10", 1, "https://cng-experiential-koolreplay-dev.s3.us-east-2.amazonaws.com/swing_green.mp4")]
        public async Task TestSendMediaToCnGo(string uidToTest, int sourceId, string s3Link)
        {
            var baseDir = GetFullOutputDir();
            var resultFileLogin = new FileInfo(baseDir + "login_result.json");
            var resultFileProcessScan = new FileInfo(baseDir + "process_result.json");
            var resultFileMedia = new FileInfo(baseDir + "media_result.json");
            EnsureFileNotExists(resultFileLogin, resultFileProcessScan, resultFileMedia);

            var wr = new WebRequestConnectAndGo
            {
                BaseUrl = "https://connectngo-demo.com"
            };

            Assert.IsTrue(await wr.InitializeExecutor());

            var logResponse = await wr.CnGLogin(CancellationToken.None, 5, "1q2w3e4r5t",
                "frcoupal@nomadlogic.com","5gjLrwVCmDzgweDhBiyPgUV2VKQi2dIvIBolRSas", 
                GrantTypeEnum.password, "*", resultFileLogin);

            var token = logResponse?.Data;
            Assert.IsNotNull(token);

            var pars = new ProcessAccessScanPayload
            {
                UniqueId = uidToTest,
                AccessPointId = 1,
                Direction = "IN",
                GateName = "mad-pc",
                XtenantId = 1000001
            };

            var proc = await wr.ProcessAccessScan(token, pars, CancellationToken.None, resultFileProcessScan);
            Assert.IsNotNull(proc?.WalletId);

            var med = new MediaPayload
            {
                AccessPointId = 1,
                XtenantId = 1000001,
                WalletId = proc.WalletId,
                SourceId = sourceId,
                Medias = new List<Media>
                {
                    new Media
                    {
                        Format = MediaFormatEnum.full_size,
                        MediaType = Media.MEDIA_TYPE_MP4,
                        MediaUrl = s3Link
                    }
                }
            };

            var result = await wr.SendMediaInfo(token, med, CancellationToken.None, resultFileMedia);

            Assert.IsNotNull(result);
        }

    }
}