using System;

namespace NomadLogic.iBrandDesktop
{
    [Obsolete]
    public class IbrandEventInfo
    {
        public IbrandEventInfo(int id, string name, int questionnaireId)
        {
            Id = id;
            Name = name;
            QuestionnaireId = questionnaireId;
        }

        public override string ToString()
        {
            return Name;
        }

        public string Name { get; }

        public int QuestionnaireId { get; }

        public int Id { get; }
    }
}
