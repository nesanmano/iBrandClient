using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Drawing;
using System.Linq;
using NomadLogic.Common.Util;

namespace NomadLogic.iBrandDesktop.Ui.ApplicationScreen.Generic.Helpers
{
    public class PreviewOverlayHelper : IEquatable<PreviewOverlayHelper>
    {
        public Rectangle? PreviewRectangle { get; set; }
        public IList<PreviewOverlayInfo> PreviewOverlayInfoList { get; set; } = new List<PreviewOverlayInfo>();

        public override string ToString()
        {
            return ClassUtil.ToStringPublicProperties(this);
        }

        public bool Equals(PreviewOverlayHelper other)
        {
            if (ReferenceEquals(null, other)) return false;
            if (ReferenceEquals(this, other)) return true;
            return Equals(PreviewRectangle, other.PreviewRectangle) &&
                (PreviewOverlayInfoList?.SequenceEqual(other.PreviewOverlayInfoList) ?? other.PreviewOverlayInfoList == null);
        }

        public override bool Equals(object obj)
        {
            if (ReferenceEquals(null, obj)) return false;
            if (ReferenceEquals(this, obj)) return true;
            if (obj.GetType() != this.GetType()) return false;
            return Equals((PreviewOverlayHelper) obj);
        }

        [SuppressMessage("ReSharper", "NonReadonlyMemberInGetHashCode")]
        public override int GetHashCode()
        {
            unchecked
            {
                return (PreviewRectangle?.GetHashCode() * 397 ?? 0 ) ^ 
                    (PreviewOverlayInfoList != null ? PreviewOverlayInfoList.GetHashCode() : 0);
            }
        }

    }
}
