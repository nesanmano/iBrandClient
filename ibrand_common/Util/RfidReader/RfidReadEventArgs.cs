namespace NomadLogic.Common.Util.RfidReader
{
    public class RfidReadEventArgs
    {
        public RfidReadEventArgs(string cardUid)
        {
            CardUid = cardUid;
        }

        /// Le UID de la carte qui vient d'être lue.
        public string CardUid { get; }
    }
}