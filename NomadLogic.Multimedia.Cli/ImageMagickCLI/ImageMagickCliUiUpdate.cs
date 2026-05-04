using System;
using NomadLogic.Common.Util;
using NomadLogic.Common.Util.DateAndTime;

namespace NomadLogic.Multimedia.Cli.ImageMagickCLI
{
    /// <summary>
    /// Référence vers l'état d'une exécution de multiple lignes de commandes pour ImageMagick.
    /// À utiliser avec le paradigme de la classe "Progess" et "IProgress" de C#
    /// </summary>
    public class ImageMagickCliUiUpdate
    {
        public string CurrentStepDescription { get; set; }
        /// <summary>
        /// Représente un double qui contient une valeur entre [0-100].
        /// </summary>
        public double PercentCompletion { get; set; }
        public int StepTotal { get; set; }
        public int CurrentStep { get; set; }
        public TimeSpan RunningTime { get; set; }
        public TimeSpan StepTime { get; set; }

        /// <summary>
        /// Exporte une ligne texte simple de ce rapport.
        /// </summary>
        /// <returns>Une ligne simple descriptive de ce rapport.</returns>
        public string BasicLineReport()
        {
            return $"Time:({StepTime.ToStringHourMinSec(true, true)})/({RunningTime.ToStringHourMinSec(true, true)}), Steps: {CurrentStep}/{StepTotal} - {StringUtil.FormatFloat(2, PercentCompletion)}%, Description={CurrentStepDescription}";
        }
    }
}