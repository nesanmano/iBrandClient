#region License and copyright notice
// Original code by Guillaume Leparmentier, licensed under The Code Project Open License (CPOL)
#endregion

using System.Diagnostics.CodeAnalysis;
// ReSharper disable CompareOfFloatsByEqualityOperator

#pragma warning disable 1591
namespace Kaliko.ImageLibrary.ColorSpace {

    /// <summary>
    /// Structure to define CIE L*a*b*.
    /// </summary>
    public struct CieLab {
        /// <summary>
        /// Gets an empty CIELab structure.
        /// </summary>
        public static readonly CieLab Empty = new CieLab();

        #region Fields

        #endregion

        #region Operators
        public static bool operator ==(CieLab item1, CieLab item2) {
            return (
                       item1.L == item2.L
                       && item1.A == item2.A
                       && item1.B == item2.B
                   );
        }

        public static bool operator !=(CieLab item1, CieLab item2) {
            return (
                       item1.L != item2.L
                       || item1.A != item2.A
                       || item1.B != item2.B
                   );
        }

        #endregion

        #region Accessors
        /// <summary>
        /// Gets or sets L component.
        /// </summary>
        public double L { get; set; }

        /// <summary>
        /// Gets or sets a component.
        /// </summary>
        public double A { get; set; }

        /// <summary>
        /// Gets or sets a component.
        /// </summary>
        public double B { get; set; }

        #endregion

        public CieLab(double l, double a, double b) {
            L = l;
            A = a;
            B = b;
        }

        #region Methods
        public override bool Equals(object obj) {
            if (obj == null || GetType() != obj.GetType()) return false;

            return (this == (CieLab)obj);
        }

        [SuppressMessage("ReSharper", "NonReadonlyMemberInGetHashCode")]
        public override int GetHashCode() {
            return L.GetHashCode() ^ A.GetHashCode() ^ B.GetHashCode();
        }

        #endregion
    }
}