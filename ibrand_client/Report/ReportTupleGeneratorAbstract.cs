using System.Collections.Generic;
using NomadLogic.iBrandDesktop.Services;

namespace NomadLogic.iBrandDesktop.Report
{
    public abstract class ReportTupleGeneratorAbstract : IReportTupleGenerator
    {
        protected int _nbrCols;
        protected IbrandDatabase _db;
        protected Questionnaire _questionnaire;
        protected bool _metadataGeneration;
        public abstract int ColumnNumber { get; }
        public abstract IList<string> GenerateHeaderData();
        public abstract bool CreateNextTuple();
        public abstract IList<object> CurrentTuple();
    }
}