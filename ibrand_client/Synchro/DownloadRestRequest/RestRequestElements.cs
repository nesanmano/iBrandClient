namespace NomadLogic.iBrandDesktop.Synchro.DownloadRestRequest
{
    public class RestRequestElements
    {

        public string ServerAddr { get; set; }

        public string ApiMethod1 { get; set; }

        public string ApiMethod2 { get; set; }

        public string ApiMethod3 { get; set; }

        public string[][] Headers { get; set; }

        public string TerminalType { get; set; }

        public string DeviceName { get; set; }

        public RestRequestElements(string srvAddr, string servPort)
        {
            this.ServerAddr = "http://" + srvAddr + ":" + servPort;
        }

        public RestRequestElements()
        {

        }
    }
}
