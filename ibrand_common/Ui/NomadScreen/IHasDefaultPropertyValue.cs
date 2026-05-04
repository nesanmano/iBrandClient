using System;

namespace NomadLogic.Common.Ui.NomadScreen
{
    public interface IHasDefaultPropertyValue<T>
    {
        bool GetDefaultPropertyValue(out T toInitialize);
    }
}