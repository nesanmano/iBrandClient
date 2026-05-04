using System;
using System.Net;
using System.Runtime.Serialization.Json;
using NLog;
using NomadLogic.Common.Services;
using NomadLogic.Common.Services.Configuration;
using NomadLogic.Common.Util;
using NomadLogic.Common.Util.HumanEquation.Json;
using NomadLogic.Common.Util.Networking;
using NUnit.Framework;

namespace NomadLogic.Test.Common.Util
{
    public class HumanEquation : TestBaseNunit
    {
        private static readonly Logger Log = LogManager.GetCurrentClassLogger();
        
        [Test]
        public void RequestToken()
        {
            try {
                String json = "{\"api_key\":\"Fp96CB4QEEoDtJwihefquVr0pEBbDWl3\"}";

                var request = new HttpUtil().GeneratePostJSonRequest("http://bell-mdia-api.humanequation.co/request-token",
                        json, 3000);
                var response = (HttpWebResponse)request.GetResponse();

                RequestTokenResponse token_response = null;

                if (response.StatusCode == HttpStatusCode.OK)
                {
                    var jsonSerializer = new DataContractJsonSerializer(typeof(RequestTokenResponse));
                    if (response.GetResponseStream() != null)
                    {
                        var objResponse = jsonSerializer.ReadObject(response.GetResponseStream());
                        token_response = objResponse as RequestTokenResponse;
                    }
                }

                if (token_response != null)
                    Console.Out.Write(token_response.ReturnData.Token);
            }catch(System.Exception ex)
            {
                Log.Fatal(ex.StackTrace);
            }
        }

        public static void InitConfigurationAndNlog()
        {
            var config = new NomadConfiguration {ApplicationName = DirectoryUtil.ApplicationNameEnum.Test};
            ServicesContainer.AddService(config);

            LogUtil.InitNLog();
        }

        [Test]
        public void EventScan()
        {
            try
            {
                var request = new HttpUtil().GenerateGetRequest("http://bell-mdia-api.humanequation.co/event/scan?token=g1rpuh61ut86cb8g4rvm2r77ulaose7y", 3000);
                var response = (HttpWebResponse)request.GetResponse();

                EventScanResponse event_scan_response = null;

                if (response.StatusCode == HttpStatusCode.OK)
                {
                    var jsonSerializer = new DataContractJsonSerializer(typeof(EventScanResponse));
                    if (response.GetResponseStream() != null)
                    {
                        var objResponse = jsonSerializer.ReadObject(response.GetResponseStream());
                        event_scan_response = objResponse as EventScanResponse;
                    }
                }

                if (event_scan_response != null)
                    Console.Out.Write(event_scan_response.ReturnData.EventData.User.Email);
            }
            catch (System.Exception ex)
            {
                Log.Fatal(ex.StackTrace);
            }
        }

        
    }
}
