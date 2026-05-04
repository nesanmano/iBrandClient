using System;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Linq;
using NLog;

namespace sony_camera_api
{
    public class SsdpDiscovery
    {
        private static readonly Logger log = LogManager.GetCurrentClassLogger();

        private const string MULTICAST_ADDRESS = "239.255.255.250";

        private const int SSDP_PORT = 1900;
        private const int RESULT_BUFFER = 8192;

        public const string ST_ALL = "ssdp:all";

        public uint Mx { set; get; } = 1;

        private readonly TimeSpan _defaultTimeout = new TimeSpan(0, 0, 5);

        public delegate void SonyCameraDeviceHandler(object sender, SonyCameraDeviceEventArgs e);

        public event SonyCameraDeviceHandler SonyCameraDeviceDiscovered;

        protected void OnDiscovered(SonyCameraDeviceEventArgs e)
        {
            SonyCameraDeviceDiscovered?.Invoke(this, e);
        }

        public delegate void DeviceDescriptionHandler(object sender, DeviceDescriptionEventArgs e);

        public event DeviceDescriptionHandler DescriptionObtained;

        protected void OnDiscovered(DeviceDescriptionEventArgs e)
        {
            DescriptionObtained?.Invoke(this, e);
        }

        public event EventHandler Timeout;

        protected void OnTimeout(EventArgs e)
        {
            Timeout?.Invoke(this, e);
        }

        private async void Search(string st, TimeSpan? timeout = null)
        {
            log.Trace("Search");

            var ssdpData = new StringBuilder()
                .Append("M-SEARCH * HTTP/1.1").Append("\r\n")
                .Append("HOST: ").Append(MULTICAST_ADDRESS).Append(":").Append(SSDP_PORT.ToString()).Append("\r\n")
                .Append("MAN: ").Append("\"ssdp:discover\"").Append("\r\n")
                .Append("MX: ").Append(Mx.ToString()).Append("\r\n")
                .Append("ST: ").Append(st).Append("\r\n")
                .Append("\r\n")
                .ToString();
            var dataByte = Encoding.UTF8.GetBytes(ssdpData);

            var timeoutCalled = false;


            var ddHandler = new AsyncCallback(ar =>
            {
                // ReSharper disable once AccessToModifiedClosure
                if (timeoutCalled)
                {
                    return;
                }

                var req = ar.AsyncState as HttpWebRequest;

                try
                {
                    var res = req?.EndGetResponse(ar) as HttpWebResponse;
                    if (res == null) return;
                    // ReSharper disable once AssignNullToNotNullAttribute
                    using (var reader = new StreamReader(res.GetResponseStream(), Encoding.UTF8))
                    {
                        try
                        {
                            var response = reader.ReadToEnd();
                            OnDiscovered(new DeviceDescriptionEventArgs(response));

                            var camera = AnalyzeDescription(response);
                            if (camera != null)
                            {
                                OnDiscovered(new SonyCameraDeviceEventArgs(camera, req.RequestUri));
                            }
                        }
                        catch (Exception)
                        {
                            log.Error("Invalid XML");
                        }
                    }
                }
                catch (WebException)
                {
                    //Invalid DD location or network error.
                }
            });
            var socket = new Socket(AddressFamily.InterNetwork, SocketType.Dgram, ProtocolType.Udp)
            {
                SendBufferSize = dataByte.Length
            };

            var sndEventArgs = new SocketAsyncEventArgs
            {
                RemoteEndPoint = new IPEndPoint(IPAddress.Parse(MULTICAST_ADDRESS), SSDP_PORT)
            };
            sndEventArgs.SetBuffer(dataByte, 0, dataByte.Length);

            var rcvEventArgs = new SocketAsyncEventArgs();
            rcvEventArgs.SetBuffer(new byte[RESULT_BUFFER], 0, RESULT_BUFFER);

            var sndHandler = new EventHandler<SocketAsyncEventArgs>((sender, e) =>
            {
                if (e.SocketError != SocketError.Success || e.LastOperation != SocketAsyncOperation.SendTo) return;
                try
                {
                    socket.ReceiveBufferSize = RESULT_BUFFER;
                    socket.ReceiveAsync(rcvEventArgs);
                }
                catch (ObjectDisposedException)
                {
                    log.Error("Socket is already disposed.");
                }
            });
            sndEventArgs.Completed += sndHandler;

            var rcvHandler = new EventHandler<SocketAsyncEventArgs>((sender, e) =>
            {
                if (e.SocketError != SocketError.Success || e.LastOperation != SocketAsyncOperation.Receive) return;
                var result = Encoding.UTF8.GetString(e.Buffer, 0, e.BytesTransferred);

                GetDeviceDescriptionAsync(ddHandler, result);

                try
                {
                    socket.ReceiveAsync(e);
                }
                catch (ObjectDisposedException)
                {
                    log.Error("Socket is already disposed.");
                }
            });
            rcvEventArgs.Completed += rcvHandler;
            socket.SendToAsync(sndEventArgs);

            await Task.Delay(timeout ?? _defaultTimeout).ConfigureAwait(false);

            log.Info("Search Timeout");
            timeoutCalled = true;
            sndEventArgs.Completed -= sndHandler;
            rcvEventArgs.Completed -= rcvHandler;
            socket.Close();

            OnTimeout(new EventArgs());
        }
        
        /// <summary>
        /// Search sony camera devices and retrieve the endpoint URLs.
        /// </summary>
        /// <param name="timeout">Timeout to end up search.</param>
        public void SearchSonyCameraDevices(TimeSpan? timeout = null)
        {
            Search("urn:schemas-sony-com:service:ScalarWebAPI:1", timeout);
        }

        /// <summary>
        /// Search UPnP devices and retrieve the device description.
        /// </summary>
        /// <param name="st">Search Target parameter for SSDP.</param>
        /// <param name="timeout">Timeout to end up search.</param>
        public void SearchUpnpDevices(string st = ST_ALL, TimeSpan? timeout = null)
        {
            if (string.IsNullOrEmpty(st))
            {
                st = ST_ALL;
            }
            Search(st, timeout);
        }


        private static void GetDeviceDescriptionAsync(AsyncCallback ac, string data)
        {
            var ddLocation = ParseLocation(data);
            if (ddLocation == null) return;
            try
            {
                var req = WebRequest.Create(new Uri(ddLocation)) as HttpWebRequest;
                if (req == null) return;
                req.Method = "GET";
                req.BeginGetResponse(ac, req);
            }
            catch (Exception)
            {
                //Invalid DD location.
            }
        }


        private static string ParseLocation(string response)
        {
            var reader = new StringReader(response);
            var line = reader.ReadLine();
            if (line != "HTTP/1.1 200 OK")
            {
                return null;
            }

            while (true)
            {
                line = reader.ReadLine();
                if (line == null)
                    break;
                if (line == "")
                    continue;

                var divider = line.IndexOf(':');
                if (divider < 1)
                    continue;

                var name = line.Substring(0, divider).Trim();
                if (name == "LOCATION" || name == "location")
                {
                    return line.Substring(divider + 1).Trim();
                }
            }

            return null;
        }

        private const string UPNP_NS = "{urn:schemas-upnp-org:device-1-0}";
        private const string SONY_NS = "{urn:schemas-sony-com:av}";

        public static SonyCameraDeviceInfo AnalyzeDescription(string response)
        {
            //Log(response);
            var endpoints = new Dictionary<string, string>();

            var xml = XDocument.Parse(response);
            if (xml.Root == null) return null;
            var device = xml.Root.Element(UPNP_NS + "device");
            var xElement = device?.Element(UPNP_NS + "friendlyName");
            if (xElement == null) return null;
            var fName = xElement.Value;
            var element = device.Element(UPNP_NS + "modelName");
            if (element == null) return null;
            var mName = element.Value;
            var xElement1 = device.Element(UPNP_NS + "UDN");
            if (xElement1 == null) return null;
            var udn = xElement1.Value;
            var info = device.Element(SONY_NS + "X_ScalarWebAPI_DeviceInfo");
            if (info == null)
            {
                return null;
            }
            var list = info.Element(SONY_NS + "X_ScalarWebAPI_ServiceList");

            if (list == null) return endpoints.Count == 0 ? null : new SonyCameraDeviceInfo(udn, mName, fName, endpoints);

            foreach (var service in list.Elements())
            {
                var element1 = service.Element(SONY_NS + "X_ScalarWebAPI_ServiceType");
                if (element1 == null) continue;
                var name = element1.Value;
                var xElement2 = service.Element(SONY_NS + "X_ScalarWebAPI_ActionList_URL");
                if (xElement2 == null) continue;
                var url = xElement2.Value;

                string endpoint;
                if (url.EndsWith("/"))
                    endpoint = url + name;
                else
                    endpoint = url + "/" + name;

                endpoints.Add(name, endpoint);
            }
            return endpoints.Count == 0 ? null : new SonyCameraDeviceInfo(udn, mName, fName, endpoints);
        }
        
    }

    public class DeviceDescriptionEventArgs : EventArgs
    {
        public string Description { private set; get; }

        public DeviceDescriptionEventArgs(string description)
        {
            Description = description;
        }

    }

    public class SonyCameraDeviceEventArgs : EventArgs
    {
        public SonyCameraDeviceInfo SonyCameraDevice { private set; get; }

        // ReSharper disable once UnusedParameter.Local
        public SonyCameraDeviceEventArgs(SonyCameraDeviceInfo info, Uri location)
        {
            SonyCameraDevice = info;
        }

    }
}
