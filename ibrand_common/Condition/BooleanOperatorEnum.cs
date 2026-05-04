using System.Diagnostics.CodeAnalysis;
using JetBrains.Annotations;

namespace NomadLogic.Common.Condition
{
    [PublicAPI]
    [SuppressMessage("ReSharper", "InconsistentNaming")]
    public enum BooleanOperatorEnum
    {
        AND,
        OR,
        XOR
    }
}