using System;
using System.Collections.Generic;
using GoPro.Hero.Filtering;

namespace GoPro.Hero.Commands
{
    public abstract class CommandMultiChoice<T, TO> : CommandRequest<TO> where TO : IFilterProvider
    {
        public T Selection
        {
            get
            {
                if (string.IsNullOrEmpty(Parameter))
                    return default(T);

                var num = int.Parse(Parameter.Substring(1));
                return (T) Enum.ToObject(typeof (T), num);
            }

            set 
            { //base.Parameter = "%" + Convert.ToInt32(value).ToString("x2");
                Parameter = Convert.ToInt32(value).ToString();
            }
        }

        public CommandMultiChoice<T, TO> Select(T mode)
        {
            Selection = mode;
            return this;
        }

        public IEnumerable<T> ValidStates()
        {
            return Filter.GetValidStates<T, CommandMultiChoice<T, TO>>(GetType().Name);
        }

        public CommandMultiChoice<T, TO> ValidStates(out IEnumerable<T> validStates)
        {
            validStates = Filter.GetValidStates<T, CommandMultiChoice<T, TO>>(GetType().Name);
            return this;
        }
    }
}