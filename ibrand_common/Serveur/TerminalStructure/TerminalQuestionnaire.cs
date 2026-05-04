using Newtonsoft.Json;

namespace NomadLogic.Common.Serveur.TerminalStructure
{
    public class TerminalQuestionnaire 
    {

        [JsonProperty("id")]
        public int Id { get; set; }

        [JsonProperty("projectId")]
        public int ProjectId { get; set; }

        [JsonProperty("title")]
        public string Title { get; set; }

        [JsonProperty("src")]
        public string Src { get; set; }

        [JsonProperty("languages")]
        public string[] Languages { get; set; }

        [JsonProperty("terminals")]
        public TerminalStructureData[] Terminals { get; set; }

        [JsonProperty("submitAction")]
        public string SubmitAction { get; set; }

        [JsonProperty("cloneSource")]
        public bool CloneSource { get; set; }

        [JsonProperty("mergeQuestionNo")]
        public int MergeQuestionNo { get; set; }

        [JsonProperty("cloneQuestionNo")]
        public int CloneQuestionNo { get; set; }

        [JsonProperty("loadLastRespondent")]
        public bool LoadLastRespondent { get; set; }

        [JsonProperty("uiFile")]
        public string UiFile { get; set; }

        [JsonProperty("questions")]
        public TerminalQuestion[] Questions { get; set; }

        [JsonProperty("email")]
        public bool Email { get; set; }

        [JsonProperty("sms")]
        public bool Sms { get; set; }

        [JsonProperty("postSyncActions")]
        public object[] PostSyncActions { get; set; }

        // mis pour legacy
        [JsonProperty("fileList")]
        public ResourceFileToDownload[] FilesToDownload { get; set; }


    }

}
