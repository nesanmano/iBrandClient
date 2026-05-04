using System;
using JetBrains.Annotations;
using NomadLogic.Common.Respondent;

// ReSharper disable NonReadonlyMemberInGetHashCode

namespace NomadLogic.Common.Condition
{
    [PublicAPI, Obsolete("Les conditions seront inutilisée avec le serveur Juju.")]
    public class EventCondition : ICondition, IEquatable<EventCondition>
    {
        public int EventId { get; set; }

        public EventCondition(int eventId)
        {
            EventId = eventId;
        }

        public EventCondition() { }

        bool ICondition.Evaluate(IRespondent respondent)
        {
            return respondent.EventId.Equals(EventId);
        }

        public bool Equals(EventCondition other)
        {
            if (ReferenceEquals(null, other)) return false;
            if (ReferenceEquals(this, other)) return true;
            return EventId == other.EventId;
        }

        public int CompareTo(ICondition other)
        {
            if (!(other is EventCondition cond)) return 1;
            return EventId.CompareTo(cond.EventId);
        }

        public override bool Equals(object obj)
        {
            if (ReferenceEquals(null, obj)) return false;
            if (ReferenceEquals(this, obj)) return true;
            return obj.GetType() == GetType() && Equals((EventCondition) obj);
        }

        public override int GetHashCode()
        {
            return EventId;
        }
    }
}
