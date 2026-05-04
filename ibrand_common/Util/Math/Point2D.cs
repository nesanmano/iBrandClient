using System;
using System.Diagnostics.CodeAnalysis;

namespace NomadLogic.Common.Util.Math
{
    /// <summary>
    /// Structure représentant un point dans un espace à deux dimensions.
    /// </summary>
    public struct Point2D
    {
        /// <summary>
        /// Construteur principal
        /// </summary>
        /// <param name="x">Valeux X</param>
        /// <param name="y">Valeur Y</param>
        public Point2D(double x, double y)
        {
            X = x;
            Y = y;
        }

        /// <summary>
        /// X
        /// </summary>
        public double X { get; set; }
        /// <summary>
        /// Y
        /// </summary>
        public double Y { get; set; }
        
        /// <summary>
        /// Surcharge d'opérateur pour "transformer" cette structure en tableau.
        /// </summary>
        /// <param name="key">L'indice de tableau</param>
        /// <returns></returns>
        public double this[int key]
        {
            get
            {
                if (key == 0) return X;
                if (key == 1) return Y;
                throw new IndexOutOfRangeException();
            }
            set
            {
                if (key == 0) X = value;
                if (key == 1) Y = value;
                throw new IndexOutOfRangeException();
            }
        }

        /// <inheritdoc />
        public override string ToString()
        {
            return $"[{X},{Y}]";
        }

        [SuppressMessage("ReSharper", "CompareOfFloatsByEqualityOperator")]
        public override bool Equals(object obj)
        {
            if (!(obj is Point2D)) return false;
            var point = (Point2D)obj;
            return X == point.X && Y == point.Y;
        }

        /// <summary>
        /// Méthode de comparaison fortement typée.
        /// </summary>
        /// <param name="other">L'autre point à tester.</param>
        /// <returns>TRUE si rigoureusement égal.</returns>
        public bool Equals(Point2D other)
        {
            return X.Equals(other.X) && Y.Equals(other.Y);
        }

        /// <inheritdoc/>
        [SuppressMessage("ReSharper", "NonReadonlyMemberInGetHashCode")]
        public override int GetHashCode()
        {
            return (X.GetHashCode() * 397) ^ Y.GetHashCode();
        }
    }
}