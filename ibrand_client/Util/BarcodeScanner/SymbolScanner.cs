using System;
using System.Threading;
using System.Windows.Forms;
using System.Xml;
using CoreScanner;

namespace NomadLogic.iBrandDesktop.Util.BarcodeScanner
{
    public class SymbolScanner
    {
        private const int APP_HANDLE = 0;
        private const short ScannerTypesAll = 1;

        private const int StatusSuccess = 0;
        private const int StatusFalse = 1;
        // ReSharper disable once UnusedMember.Local
        private const int StatusLocked = 10;

        private const int NumScannerEvents = 6;

        private const int SubscribeBarcode = 1;
        private const int SubscribeImage = 2;
        private const int SubscribeVideo = 4;
        private const int SubscribeRmd = 8;
        private const int SubscribePnp = 16;
        private const int SubscribeOther = 32;

        private const int REGISTER_FOR_EVENTS = 1001;
        
        private readonly CCoreScannerClass _mPCoreScanner;
        private bool _mBSuccessOpen;//Is open success

        public event ScanEvent ScanNotify;

        public SymbolScanner()
        {
            for (var i = 0; i < 3; i++)
            {
                try
                {
                    _mPCoreScanner = new CCoreScannerClass();
                }
                catch (System.Exception)
                {
                    if (i < 2)
                    {
                        Thread.Sleep(1000);
                    } 
                    
                }
            }

            if (_mPCoreScanner != null)
            {
                _mPCoreScanner.BarcodeEvent += OnBarcodeEvent;
            }
            else
            {
               throw new System.Exception("Initianting the scanner failed after 3 tries.");
            }
        }

        private void OnBarcodeEvent(short eventType, ref string pscanData)
        {
            try
            {
                var xdoc = new XmlDocument();
                xdoc.LoadXml(pscanData);

                var s = "";
                var xmlNode = xdoc.DocumentElement?.GetElementsByTagName("datalabel").Item(0);
                if (xmlNode != null)
                {
                    var barcode = xmlNode.InnerText;
                    var numbers = barcode.Split(' ');
                    foreach (var number in numbers)
                    {
                        if (string.IsNullOrEmpty(number))
                            break;
                        s += ((char)Convert.ToInt32(number, 16)).ToString();
                    }
                }

                ScanNotify?.Invoke(s);
            }
            catch (System.Exception exp)
            {
                MessageBox.Show(exp.StackTrace);
            }
        }

        public void Connect()
        {
            ConnectToScanner();
            RegisterForEvents();
        }

        private void RegisterForEvents()
        {
            if (!_mBSuccessOpen) return;
            int nEvents;
            var strEvtIDs = GetRegUnregIDs(out nEvents);
            var inXml = "<inArgs>" +
                        "<cmdArgs>" +
                        "<arg-int>" + nEvents + "</arg-int>" +
                        "<arg-int>" + strEvtIDs + "</arg-int>" +
                        "</cmdArgs>" +
                        "</inArgs>";

            var opCode = REGISTER_FOR_EVENTS;
            string outXml;
            int status;
            ExecCmd(opCode, ref inXml, out outXml, out status);
        }

        private void ConnectToScanner()
        {
            if (_mBSuccessOpen)
            {
                return;
            }
            
            var scannerTypes = new short[1];
            scannerTypes[0] = ScannerTypesAll;
            try
            {
                int status;
                _mPCoreScanner.Open(APP_HANDLE, scannerTypes, 1, out status);
                if (StatusSuccess == status)
                {
                    _mBSuccessOpen = true;
                }
            }
            catch (System.Exception exp)
            {
                MessageBox.Show(@"Error OPEN - " +exp.StackTrace);
                //MessageBox.Show("Error OPEN - " + exp.Message, APP_TITLE, MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        public void Disconnect()
        {
            if (!_mBSuccessOpen) return;
            const int appHandle = 0;
            try
            {
                int status;
                _mPCoreScanner.Close(appHandle, out status);
                if (StatusSuccess == status)
                {
                    _mBSuccessOpen = false;
                }
            }
            catch (System.Exception exp)
            {
                MessageBox.Show(@"Error Close - " + exp.StackTrace);
                //MessageBox.Show("CLOSE Error - " + exp.Message, APP_TITLE, MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private static string GetRegUnregIDs(out int nEvents)
        {
            nEvents = NumScannerEvents;
            var strIDs = SubscribeBarcode.ToString();
            strIDs += "," + SubscribeImage;
            strIDs += "," + SubscribeVideo;
            strIDs += "," + SubscribeRmd;
            strIDs += "," + SubscribePnp;
            strIDs += "," + SubscribeOther;
            return strIDs;
        }

        private void ExecCmd(int opCode, ref string inXml, out string outXml, out int status)
        {
            outXml = "";
            status = StatusFalse;
            if (_mBSuccessOpen)
            {
                try
                {
                    _mPCoreScanner.ExecCommand(opCode, ref inXml, out outXml, out status);
                }
                catch (System.Exception ex)
                {
                    MessageBox.Show(@"Error ExecCmd - " + ex.StackTrace);
                }
            }
        }

    }
}
