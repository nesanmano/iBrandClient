using System;
using Newtonsoft.Json;

namespace NomadLogic.Common.Util.Json
{
    /// <summary>
    /// TimeSpans are not serialized consistently depending on what properties are present. So this 
    /// serializer will ensure the format is maintained no matter what.
    /// https://stackoverflow.com/questions/39876232/newtonsoft-json-serialize-timespan-format
    /// </summary>
    public class TimespanConverter : JsonConverter<TimeSpan>
    {
        /// <summary>
        /// Format: Days.Hours:Minutes:Seconds:Milliseconds
        /// </summary>
        public const string TimeSpanFormatString = @"d\.hh\:mm\:ss\:FFF";

        public override void WriteJson(JsonWriter writer, TimeSpan value, JsonSerializer serializer)
        {
            var timespanFormatted = $"{value.ToString(TimeSpanFormatString)}";
            writer.WriteValue(timespanFormatted);
        }

        public override TimeSpan ReadJson(JsonReader reader, Type objectType, TimeSpan existingValue, bool hasExistingValue, JsonSerializer serializer)
        {
            TimeSpan.TryParseExact((string)reader.Value, TimeSpanFormatString, null, out var parsedTimeSpan);
            return parsedTimeSpan;
        }
    }
}