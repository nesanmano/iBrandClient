using System;
using System.Collections.Generic;
using JetBrains.Annotations;

namespace NomadLogic.iBrandDesktop.Ui.ApplicationScreen.Generic.Helpers
{
    /// <summary>
    /// Implémente un holder de séquence d'image avec une liste d'énumérations (le Type T est plutôt bizarre mais
    /// s'applique à des enums: voir "https://stackoverflow.com/questions/79126/create-generic-method-constraining-t-to-an-enum")
    /// La façon dont les énumérations sont utilisée dépend de qui les utilise.
    /// </summary>
    /// <typeparam name="T">Un format compatible avec une énumération C#</typeparam>
    [PublicAPI]
    public class AnimatedImageSequenceEnumTrigger<T> where T : struct, IConvertible, IComparable, IFormattable
    {
        /// La séquence d'image.
        public AnimatedImageSequence ImageSequence { get; set; }

        private IList<T> _list = new List<T>();
        
        /// La liste d'énumération associée. S'arrange pour ne jamais être null. Si on passe null à cette propriété, sera convertit en liste vide.
        public IList<T> EnumFlags
        {
            get => _list;
            set => _list = value ?? new List<T>();
        }
    }
}