using System.Collections.Generic;
using GoPro.Hero.Filtering;

namespace GoPro.Hero.Commands
{
    public abstract class CommandBoolean<TO> : CommandRequest<TO> where TO : IFilterProvider
    {
        protected const string ON = "1";
        protected const string OFF = "0";

        public virtual bool State
        {
            get { return !string.IsNullOrEmpty(Parameter) && Parameter != OFF; }
            set { Parameter = value ? ON : OFF; }
        }

        public CommandBoolean<TO> Set(bool state)
        {
            State = state;
            return this;
        }

        public CommandBoolean<TO> Enable()
        {
            State = true;
            return this;
        }

        public CommandBoolean<TO> Disable()
        {
            State = false;
            return this;
        }

        public IEnumerable<bool> ValidStates()
        {
            return Filter.GetValidStates<CommandBoolean<TO>>(GetType().Name);
        }

        public CommandBoolean<TO> ValidStates(out IEnumerable<bool> validStates)
        {
            validStates = Filter.GetValidStates<CommandBoolean<TO>>(GetType().Name);
            return this;
        }
    }
}