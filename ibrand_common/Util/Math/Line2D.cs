using System;
using System.Diagnostics.CodeAnalysis;

namespace NomadLogic.Common.Util.Math
{
    /// <summary>
    /// Structure de données pour représenter un point dans un espace euclédien à deux dimensions.
    /// L'ordre des points revêt une certaines importance si on compte utiliser cette structure comme vecteur à cause de la direction.
    /// </summary>
    public struct Line2D
    {
        /// <summary>
        /// Constructeur principal.
        /// </summary>
        /// <param name="point1">Le premier point.</param>
        /// <param name="point2">Le deuxième point.</param>
        public Line2D(Point2D point1, Point2D point2)
        {
            Point1 = point1;
            Point2 = point2;
        }
        
        /// <summary>
        /// Le premier point
        /// </summary>
        public Point2D Point1 { get; set; }
        /// <summary>
        /// Le deuxième point
        /// </summary>
        public Point2D Point2 { get; set; }

        /// <summary>
        /// Surcharge d'opérateur pour "transformer" cette structure en tableau.
        /// </summary>
        /// <param name="key">L'indice de tableau</param>
        /// <returns></returns>
        public Point2D this[int key]
        {
            get
            {
                if (key == 0) return Point1;
                if (key == 1) return Point2;
                throw new IndexOutOfRangeException();
            }
            set
            {
                if (key == 0) Point1 = value;
                if (key == 1) Point2 = value;
                throw new IndexOutOfRangeException();
            }
        }

        /// <summary>
        /// Calcule la longueur de cette ligne en considérant que c'est un vecteur.
        /// </summary>
        /// <returns>La longueur de la ligne.</returns>
        public double GetLenght()
        {
            var vec = new[] { Point2.X - Point1.X, Point2.Y - Point1.Y};
            return System.Math.Sqrt(vec[0]*vec[0] + vec[1]*vec[1]);
        }

        /// <inheritdoc />
        public override string ToString()
        {
            return $"[{Point1},{Point2}]";
        }

        /// <inheritdoc />
        public override bool Equals(object obj)
        {
            if (!(obj is Line2D)) return false;
            var line = (Line2D) obj;
            return Point1.Equals(line.Point1) && Point2.Equals(line.Point2);
        }

        /// <summary>
        /// Méthode d'égalité avec typage.
        /// </summary>
        /// <param name="other">L'autre ligne à comparer.</param>
        /// <returns></returns>
        public bool Equals(Line2D other)
        {
            return Point1.Equals(other.Point1) && Point2.Equals(other.Point2);
        }

        /// <inheritdoc />
        [SuppressMessage("ReSharper", "NonReadonlyMemberInGetHashCode")]
        public override int GetHashCode()
        {
                return (Point1.GetHashCode()*397) ^ Point2.GetHashCode();
        }

        /// <summary>
        /// Calcule une intersection entre lignes (sur un plan 2D), selon la méthode suivante:
        /// https://en.wikipedia.org/wiki/Line%E2%80%93line_intersection
        /// S'il n'y a aucune intersection, ou une infinité d'intersections (lignes co-jointes), on retourne null, et la variable
        /// de retour sera nulle.
        /// </summary>
        /// <param name="ligne">Ligne à comparer</param>
        /// <param name="result">Variable de retour. NULL si le calcul était impossible à faire pour des raisons trigonométrique.</param>
        /// <return>TRUE si on a réussi à calculer un et un seul point d'intersection.</return>
        public bool CalculateIntersection(Line2D ligne, out Point2D? result)
        {
            Point2D? resultIn;
            var ok = MathUtil.CalculateIntersection(this, ligne, out resultIn);
            result = resultIn;
            return ok;
        }
    }
}