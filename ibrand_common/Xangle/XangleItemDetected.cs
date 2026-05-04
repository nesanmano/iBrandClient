using System;
using System.Collections.Generic;

namespace NomadLogic.Common.Xangle
{
    public class XangleItemDetected : EventArgs
    {
        /// <summary>
        /// Nouveau éléments détectés depuis la dernière vérification.
        /// </summary>
        public IReadOnlyCollection<XangleItem> NewItems { get; internal set; }
    }
}