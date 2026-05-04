using System;
using JetBrains.Annotations;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;

namespace NomadLogic.Common.Ui
{
    [PublicAPI]
    [JsonConverter(typeof(StringEnumConverter))]
    [Flags]
    public enum ThumbnailOverlaySelectorState
    {
        Selected,
        CannotBeSelected,
        NoAvailiable
    }
}