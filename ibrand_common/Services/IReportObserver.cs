using System;
using JetBrains.Annotations;

namespace NomadLogic.Common.Services
{
    [PublicAPI]
    public interface IReportObserver<T> where T : IProgressDeepCopy<T>
    {
        void RemoveObserver(IProgress<T> observer);

        void AddObserver(IProgress<T> observer);

        void TriggerReport(T report, bool dontCopy = false);

        void SendLastReport();
    }
}