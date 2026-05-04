using System;
using System.Diagnostics.CodeAnalysis;
using System.Drawing;
using System.Linq;
using NomadLogic.Common.Util;
using NomadLogic.Multimedia;

namespace NomadLogic.iBrandDesktop.Ui.ApplicationScreen.Generic.Helpers
{
    public class CondensedAnswerSelectionScreenConfig : IEquatable<CondensedAnswerSelectionScreenConfig>
    {
        /// Date de la dernière mise-à jour serveur.
        public Rectangle? DebugLastCallInfo { get; set; }
        /// Le dernier identifiant de répondant cherché sur le serveur.
        public Rectangle? DebugLastNumberInfo { get; set; }
        /// Information d'erreur, au besoin
        public Rectangle? DebugFacadeInfo { get; set; }
        /// Le nombre de répondants potential affichés à l'écran.
        public Rectangle? DebugCurrentSelectionInfo { get; set; }
        /// Si on veut que du texte apparaisse pour expliquer les erreurs ou changements d'état, il faut spécifier
        /// une instance de cette classe.
        public ProceduralImageTextInfo TextGenerator { get; set; }
        /// L'identifiant du questionnaire source de données.
        public int SourceQuestionnaireId { get; set; }
        /// Numéros de question avec lesquels in faut bâtir la ligne de présentation.
        public int[] SourceQuestionnaireQuestionIdentifiers { get; set; }
        /// Temps limite pour aller chercher des répondants dans le passé. NUll indique aucune limite.
        public TimeSpan? SourceQuestionnaireTimeSpanCutOff { get; set; }

        public override string ToString()
        {
            return ClassUtil.ToStringPublicProperties(this);
        }

        #region Equality members

        public bool Equals(CondensedAnswerSelectionScreenConfig other)
        {
            if (ReferenceEquals(null, other)) return false;
            if (ReferenceEquals(this, other)) return true;
            return Equals(DebugLastCallInfo, other.DebugLastCallInfo) &&
                Equals(DebugLastNumberInfo, other.DebugLastNumberInfo) &&
                Equals(DebugFacadeInfo, other.DebugFacadeInfo) &&
                Equals(DebugCurrentSelectionInfo, other.DebugCurrentSelectionInfo) &&
                Equals(TextGenerator, other.TextGenerator) &&
                SourceQuestionnaireId == other.SourceQuestionnaireId &&
                ((SourceQuestionnaireQuestionIdentifiers == null && other.SourceQuestionnaireQuestionIdentifiers == null) ||
                 (SourceQuestionnaireQuestionIdentifiers != null && SourceQuestionnaireQuestionIdentifiers.SequenceEqual(other.SourceQuestionnaireQuestionIdentifiers))) &&
                Equals(SourceQuestionnaireTimeSpanCutOff, other.SourceQuestionnaireTimeSpanCutOff);
        }

        public override bool Equals(object obj)
        {
            if (ReferenceEquals(null, obj)) return false;
            if (ReferenceEquals(this, obj)) return true;
            if (obj.GetType() != GetType()) return false;
            return Equals((CondensedAnswerSelectionScreenConfig) obj);
        }

        [SuppressMessage("ReSharper", "NonReadonlyMemberInGetHashCode")]
        public override int GetHashCode()
        {
            unchecked
            {
                var hashCode = DebugLastCallInfo?.GetHashCode() ?? 0;
                hashCode = (hashCode * 397) ^ DebugLastNumberInfo?.GetHashCode() ?? 0;
                hashCode = (hashCode * 397) ^ DebugFacadeInfo?.GetHashCode() ?? 0;
                hashCode = (hashCode * 397) ^ DebugCurrentSelectionInfo?.GetHashCode() ?? 0;
                hashCode = (hashCode * 397) ^ (TextGenerator != null ? TextGenerator.GetHashCode() : 0);
                hashCode = (hashCode * 397) ^ SourceQuestionnaireId;
                hashCode = (hashCode * 397) ^ (SourceQuestionnaireQuestionIdentifiers != null ? SourceQuestionnaireQuestionIdentifiers.GetHashCode() : 0);
                hashCode = (hashCode * 397) ^ SourceQuestionnaireTimeSpanCutOff?.GetHashCode() ?? 0;
                return hashCode;
            }
        }

        public static bool operator ==(CondensedAnswerSelectionScreenConfig left, CondensedAnswerSelectionScreenConfig right)
        {
            return Equals(left, right);
        }

        public static bool operator !=(CondensedAnswerSelectionScreenConfig left, CondensedAnswerSelectionScreenConfig right)
        {
            return !Equals(left, right);
        }

        #endregion
    }
}