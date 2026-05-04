namespace NomadLogic.PrintStation
{
    public class PrintCountSocialCam
    {
        public PrintCountSocialCam(long id, int eventId, string identifier)
        {
            Id = id;
            EventId = eventId;
            Identifier = identifier;
        }

        public string Identifier { get; }
        public long Id { get; }
        public int EventId { get; }
    }
}