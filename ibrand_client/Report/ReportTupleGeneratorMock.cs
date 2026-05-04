using System;
using System.Collections.Generic;
using NomadLogic.Common.Util;
using NomadLogic.iBrandDesktop.Services;

namespace NomadLogic.iBrandDesktop.Report
{
    public class ReportTupleGeneratorMock : ReportTupleGeneratorAbstract
    {
        private int _nbrTuples;
        private readonly Random _random;


        public ReportTupleGeneratorMock(IbrandDatabase db, Questionnaire questionnaire, bool metadataGeneration)
        {
            _db = db;
            _questionnaire = questionnaire;
            _metadataGeneration = metadataGeneration;

            _nbrCols = 4;
            _nbrTuples = 0;
            _random = new Random();
        }

        public override int ColumnNumber => _nbrCols;

        public override IList<string> GenerateHeaderData()
        {
            
            return new List<string> {"Header1","Header2","IntColumn","Header4"};
        }

        public override bool CreateNextTuple()
        {
            return _nbrTuples < NbrTupleToGenerate;
        }

        public int NbrTupleToGenerate { get; set; } = 100;

        public override IList<object> CurrentTuple()
        {
            _nbrTuples++;
            return new List<object> {GenerateUniqueId.GetId(),GenerateUniqueId.GetId(),_random.Next(),GenerateUniqueId.GetId()};
        }
    }
}
