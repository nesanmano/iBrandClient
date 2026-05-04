namespace NomadLogic.PrintStation
{
    public class PrintCount
    {
        public PrintCount(long id, int postId, string identifier)
        {
            Id = id;
            PostId = postId;
            Identifier = identifier;
        }

        public string Identifier { get; }
        public long Id { get; }
        public int PostId { get; }
    }
}