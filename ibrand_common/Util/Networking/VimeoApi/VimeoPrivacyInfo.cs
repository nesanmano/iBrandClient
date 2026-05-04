namespace NomadLogic.Common.Util.Networking.VimeoApi
{
    /// <summary>
    /// Classe de définition JSON d'un niveau de confidentialité pour Vimeo.
    /// </summary>
    public class VimeoPrivacyInfo
    {
        /// <summary>
        /// Type de confidentialité pour le visionnement.
        /// </summary>
        public string View { get; set; }

        /// <inheritdoc/>
        public override bool Equals(object obj)
        {
            var other = obj as VimeoPrivacyInfo;
            return Equals(other);
        }

        protected bool Equals(VimeoPrivacyInfo other)
        {
            if (other == null) return false;
            return string.Equals(View, other.View);
        }

        /// <inheritdoc/>
        public override int GetHashCode()
        {
            // ReSharper disable once NonReadonlyMemberInGetHashCode
            return View.GetHashCode();
        }
    }
}