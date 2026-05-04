using System.Collections.Generic;

namespace NomadLogic.Common.Xangle
{
    public struct XangleReportEvent
    {
        public string Message { get; set; }
        public IList<XangleItem> NewItems { get; set; }
    }
}