using System.Collections.Generic;
using JetBrains.Annotations;

namespace NomadLogic.Common.Ui.NomadScreen
{
    /// <summary>
    /// Indique qu'un élément contenant du texte internationalisé est basé sur un dictionnaire fixe.
    /// </summary>
    [PublicAPI]
    // ReSharper disable once InconsistentNaming
    public interface IHasI18nTextDictionary : IHasI18nText
    {
        IDictionary<string, string> TextDictionary { get; set; }
    }
}