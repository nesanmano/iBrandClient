using Newtonsoft.Json;

namespace NomadLogic.Common.Util.Networking.QuestionnaireWebRequest.Emisys
{
    /// <summary>
    /// Cette class est la represenation structurelle du body de la reponse retourne par les UID mock.
    /// Les methodes INIT et FINALIZE
    /// </summary>
    public class MockUidResultTestObject
    {
        [JsonProperty(PropertyName = "result")]
        public int Result { get; set; }

        [JsonProperty(PropertyName = "status")]
        public string  Status { get; set; }

    }
}
