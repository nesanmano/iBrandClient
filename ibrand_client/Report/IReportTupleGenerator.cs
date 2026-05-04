using System.Collections.Generic;

namespace NomadLogic.iBrandDesktop.Report
{
    public interface IReportTupleGenerator
    {
        int ColumnNumber { get; }
        IList<string> GenerateHeaderData();
        IList<object> CurrentTuple();
    }
}