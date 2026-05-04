namespace NomadLogic.Common.Util.Networking.QuestionnaireWebRequest.ConnectAndGo.Schemas
{
    public class BaseDateAndI18N : BaseCreationDate, INameI18N
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public string NameI18N { get; set; }
    }
}