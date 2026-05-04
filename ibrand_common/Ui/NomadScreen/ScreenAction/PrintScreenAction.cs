using System;
using JetBrains.Annotations;
using NomadLogic.Common.Util;

namespace NomadLogic.Common.Ui.NomadScreen.ScreenAction
{
    /// <summary>
    /// Indique qu'il faut tenter une impression avec les paramètres précis.
    /// </summary>
    [PublicAPI]
    public class PrintScreenAction : AbstractScreenAction, IEquatable<PrintScreenAction>
    {
        /// Question qui contient le nombre d'impression.
        public int? QuestionNumberOfPrint { get; set; }

        /// Question qui contient le chemin complet du fichier à imprimer.
        public int? QuestionPrintPicture { get; set; }

        /// Dans le cas où on imprime toujours la même chose, on pointe ici le fichier à imprimier (peut être absolu ou relatif au questionnaire actif).
        public string FixedPrintPicture { get; set; }

        public override string ToString()
        {
            return ClassUtil.ToStringPublicProperties(this);
        }

        public bool Equals(PrintScreenAction other)
        {
            if (ReferenceEquals(null, other)) return false;
            if (ReferenceEquals(this, other)) return true;
            return QuestionNumberOfPrint == other.QuestionNumberOfPrint && 
                   QuestionPrintPicture == other.QuestionPrintPicture && 
                   string.Equals(FixedPrintPicture, other.FixedPrintPicture);
        }

        public override bool Equals(object obj)
        {
            if (ReferenceEquals(null, obj)) return false;
            if (ReferenceEquals(this, obj)) return true;
            if (obj.GetType() != GetType()) return false;
            return Equals((PrintScreenAction) obj);
        }

        public override int GetHashCode()
        {
            unchecked
            {
                var hashCode = QuestionNumberOfPrint.GetHashCode();
                hashCode = (hashCode * 397) ^ QuestionPrintPicture.GetHashCode();
                hashCode = (hashCode * 397) ^ (FixedPrintPicture != null ? FixedPrintPicture.GetHashCode() : 0);
                return hashCode;
            }
        }

        public static bool operator ==(PrintScreenAction left, PrintScreenAction right)
        {
            return Equals(left, right);
        }

        public static bool operator !=(PrintScreenAction left, PrintScreenAction right)
        {
            return !Equals(left, right);
        }
    }
}