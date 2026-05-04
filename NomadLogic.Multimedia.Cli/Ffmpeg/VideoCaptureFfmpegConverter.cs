using System;
using System.Linq;
using JetBrains.Annotations;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace NomadLogic.Multimedia.Cli.Ffmpeg
{
    [PublicAPI]
    public class VideoCaptureFfmpegConverter : JsonConverter<VideoCaptureFfmpeg>
    {
        public override void WriteJson(JsonWriter writer, VideoCaptureFfmpeg value, JsonSerializer serializer)
        {
            throw new NotImplementedException("Class cannot write to JSON.");
        }
        
        public override VideoCaptureFfmpeg ReadJson(JsonReader reader, Type objectType, VideoCaptureFfmpeg existingValue, bool hasExistingValue, JsonSerializer serializer)
        {
            var jsonObject = JObject.Load(reader);

            // On tente d'identifier chaque classe d'implémentation par une caractéristique unique.
            VideoCaptureFfmpeg tmb;

            /* ParameterDefinitionObject */
            if (jsonObject.Properties().Any(p => p.Name.Equals("DesiredVideoSize")))
            {
                tmb = new VideoCaptureFfmpegDirectShow();
            }
            else
            {
                tmb = new VideoCaptureFfmpeg();
            }

            serializer.Populate(jsonObject.CreateReader(), tmb);
            return tmb;
        }

        public override bool CanWrite => false;
    }
}
