using System;

namespace NomadLogic.iBrandDesktop.Util.BarcodeScanner
{
    /// <summary>
    /// Classe d'événement pour une lecture nouvelle d'un code barre.
    /// </summary>
    public class ScanEventArgs : EventArgs
    {
        public ScanEventArgs(string barcode)
        {
            BarCode = barcode;
        }

        public string BarCode { get; }
    }
}
